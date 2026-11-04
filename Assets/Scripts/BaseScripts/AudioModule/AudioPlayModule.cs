using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using LetsPiu.Base.BaseScripts.Define;
using UnityEngine;
using UnityEngine.Rendering;
using YooAsset;

namespace LetsPiu.Base
{
    public partial class AudioPlayModule : SingletonBase<AudioPlayModule>
    {
        ResLoader m_ResLoader;

        Dictionary<string, AudioClip> m_AudioClipsMap;
        Dictionary<string, AssetHandle> m_AudioClipsHandleMap;
        /// <summary>
        /// 循环播放的音源列表
        /// </summary>
        Dictionary<int, AudioSource> m_LoopAudioSourceMap;
        /// <summary>
        /// PlayGlobalLoop 播放中的音源，按资源名索引
        /// </summary>
        Dictionary<string, AudioSource> m_GlobalLoopSourceMap;
        /// <summary>
        /// PlayGlobalLoop 每个资源名的最新请求版本号。GetAudioSource 异步加载耗时不定，
        /// 若加载完成前该资源名已被新的 Play/Stop 请求取代（如已切出 S6/S7 区域进入 S8），
        /// 旧请求据此判断自己已过期，放弃播放，避免"资源还没加载完就已经离开该场景，
        /// 加载完成后才迟到播放"的背景音乐残留问题。
        /// </summary>
        Dictionary<string, int> m_GlobalLoopVersionMap;
        /// <summary>
        /// PlayGlobalOneShot 播放中的音源，按资源名索引（用于面板打断时立即停止）
        /// </summary>
        Dictionary<string, HashSet<AudioSource>> m_ActiveGlobalOneShotMap;

        ObjectCache<AudioSource> m_AudioClipCache;
        /// <summary>
        /// 维护所有正在播放的音频
        /// </summary>
        List<AudioSource> m_AudioSourceList = new List<AudioSource>();

        static float m_AudioVolume = 1f;

        // 是否在播放玩家脚步声
        bool m_IsPlayMainPlayerStep;
        CancellationTokenSource m_CancellationTokenSource;

        Dictionary<TDAudioResource.AudioKind, float> audioVolume = new Dictionary<TDAudioResource.AudioKind, float>();
        Dictionary<TDAudioResource.AudioKind, bool> audioMute = new Dictionary<TDAudioResource.AudioKind, bool>();

        public string currentClipName;

        public AudioPlayModule()
        {
            m_ResLoader = new ResLoader();
            m_AudioClipsMap = new Dictionary<string, AudioClip>();
            m_AudioClipsHandleMap = new Dictionary<string, AssetHandle>();
            m_LoopAudioSourceMap = new Dictionary<int, AudioSource>();
            m_GlobalLoopSourceMap = new Dictionary<string, AudioSource>();
            m_GlobalLoopVersionMap = new Dictionary<string, int>();
            m_ActiveGlobalOneShotMap = new Dictionary<string, HashSet<AudioSource>>();

            var firstAudioSource = new GameObject("AudioSource");
            firstAudioSource.transform.SetParent(ConstantContainer.audioCacheRoot);
            AudioSource audioSource = (AudioSource)firstAudioSource.AddComponent(typeof(AudioSource));
            audioSource.spatialBlend = 1f;

            m_AudioClipCache = new ObjectCache<AudioSource>();
            m_AudioClipCache.Start(ConstantContainer.audioCacheRoot, audioSource);
            firstAudioSource.SafeSetActive(false);
        }

        public void PlayUIAudio(AudioDefine audioId)
        {
            PlayGameAudio(Camera.main.transform.position, audioId);
        }

        /// <summary>
        /// 按资源名全局播放一次音频（2D），播完自动回收。
        /// 音源挂在全局 audioCacheRoot 上、由本单例驱动，不随面板/场景切换停止，
        /// 适用于跨面板的一次性 BGM（如 S3 倒计时电台声）。
        /// </summary>
        /// <param name="resName">音频资源名（YooAsset 寻址名）</param>
        /// <param name="volume">音量；传负数则用默认音量</param>
        public void PlayGlobalOneShot(string resName, float volume = -1f)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return;
            }

            DoPlayGlobalOneShot(resName, volume < 0f ? m_AudioVolume : volume).Forget();
        }

        /// <summary>
        /// 播放已缓存的全局一次性音频（2D），不等待加载，用于与动画帧对齐的场景。
        /// 若尚未缓存则回退到 <see cref="PlayGlobalOneShot"/>。
        /// </summary>
        public void PlayGlobalOneShotImmediate(string resName, float volume = -1f)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return;
            }

            if (!m_AudioClipsMap.TryGetValue(resName, out var clip) || clip == null)
            {
                PlayGlobalOneShot(resName, volume);
                return;
            }

            PlayGlobalOneShotFromClip(clip, resName, volume < 0f ? m_AudioVolume : volume);
        }

        void PlayGlobalOneShotFromClip(AudioClip clip, string resName, float volume)
        {
            var source = m_AudioClipCache.Get();
            source.name = $"global_{resName}";
            source.clip = clip;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.mute = false;
            source.Play();
            RegisterGlobalOneShot(resName, source);
            ReleaseGlobalOneShotSourceAfterDelay(source, resName, clip.length).Forget();
        }

        void RegisterGlobalOneShot(string resName, AudioSource source)
        {
            if (!m_ActiveGlobalOneShotMap.TryGetValue(resName, out var set))
            {
                set = new HashSet<AudioSource>();
                m_ActiveGlobalOneShotMap[resName] = set;
            }

            set.Add(source);
        }

        void UnregisterGlobalOneShot(string resName, AudioSource source)
        {
            if (m_ActiveGlobalOneShotMap.TryGetValue(resName, out var set))
            {
                set.Remove(source);
                if (set.Count == 0)
                {
                    m_ActiveGlobalOneShotMap.Remove(resName);
                }
            }
        }

        async UniTask ReleaseGlobalOneShotSourceAfterDelay(AudioSource source, string resName, float delay)
        {
            await UniTask.WaitForSeconds(delay);
            if (source == null)
            {
                return;
            }

            if (!m_ActiveGlobalOneShotMap.TryGetValue(resName, out var set) || !set.Remove(source))
            {
                return;
            }

            if (set.Count == 0)
            {
                m_ActiveGlobalOneShotMap.Remove(resName);
            }

            m_AudioClipCache.Release(source);
        }

        /// <summary>立即停止并回收指定资源名的全局一次性音效；未在播放则无操作。</summary>
        public void StopGlobalOneShots(params string[] resNames)
        {
            if (resNames == null || resNames.Length == 0)
            {
                return;
            }

            foreach (var resName in resNames)
            {
                StopGlobalOneShotsByKey(resName);
            }
        }

        /// <summary>立即停止并回收资源名前缀匹配的全局一次性音效（如 S6-P- 熊猫语音）。</summary>
        public void StopGlobalOneShotsByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return;
            }

            var keys = new List<string>();
            foreach (var key in m_ActiveGlobalOneShotMap.Keys)
            {
                if (key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    keys.Add(key);
                }
            }

            foreach (var key in keys)
            {
                StopGlobalOneShotsByKey(key);
            }
        }

        void StopGlobalOneShotsByKey(string resName)
        {
            if (string.IsNullOrEmpty(resName)
                || !m_ActiveGlobalOneShotMap.TryGetValue(resName, out var set))
            {
                return;
            }

            var sources = new List<AudioSource>(set);
            set.Clear();
            m_ActiveGlobalOneShotMap.Remove(resName);

            foreach (var source in sources)
            {
                if (source == null)
                {
                    continue;
                }

                source.Stop();
                m_AudioClipCache.Release(source);
            }
        }

        /// <summary>
        /// PlayGlobalOneShot 的可等待版本：等音频真正播完再返回，用于「先播一次、再接循环」的时序场景。
        /// </summary>
        public UniTask PlayGlobalOneShotAsync(string resName, float volume = -1f)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return UniTask.CompletedTask;
            }

            return DoPlayGlobalOneShot(resName, volume < 0f ? m_AudioVolume : volume);
        }

        async UniTask DoPlayGlobalOneShot(string resName, float volume)
        {
            var clip = await GetAudioSource(resName);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioPlayModule] 全局音频加载失败: {resName}");
                return;
            }

            PlayGlobalOneShotFromClip(clip, resName, volume);
            await UniTask.WaitForSeconds(clip.length);
        }

        /// <summary>
        /// 按资源名全局循环播放（2D），需配合 <see cref="StopGlobalLoop"/> 手动停止。
        /// 用于不依赖 AudioDefine/TDAudioResource 表的跨面板循环音效（如 S1 屏幕底噪）。
        /// </summary>
        /// <param name="resName">音频资源名（YooAsset 寻址名）</param>
        /// <param name="volume">音量；传负数则用默认音量</param>
        public void PlayGlobalLoop(string resName, float volume = -1f)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return;
            }

            int version = BumpGlobalLoopVersion(resName);
            DoPlayGlobalLoop(resName, volume < 0f ? m_AudioVolume : volume, version).Forget();
        }

        async UniTask DoPlayGlobalLoop(string resName, float volume, int version)
        {
            ClearGlobalLoopSource(resName);

            var clip = await GetAudioSource(resName);

            // GetAudioSource 异步耗时不定；若加载完成时该资源名已被更新的 Play/Stop 请求取代
            // （例如已离开 S6/S7 区域进入 S8 并调用了 StopGlobalLoopWithFade），此次播放已过期，放弃。
            if (!IsGlobalLoopVersionCurrent(resName, version))
            {
                return;
            }

            if (clip == null)
            {
                Debug.LogWarning($"[AudioPlayModule] 全局循环音频加载失败: {resName}");
                return;
            }

            var source = m_AudioClipCache.Get();
            source.name = $"global_loop_{resName}";
            source.clip = clip;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.mute = false;
            source.Play();

            m_GlobalLoopSourceMap[resName] = source;
        }

        /// <summary>停止 <see cref="PlayGlobalLoop"/> 播放的循环音效；未在播放则无操作。</summary>
        public void StopGlobalLoop(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return;
            }

            BumpGlobalLoopVersion(resName);
            ClearGlobalLoopSource(resName);
        }

        /// <summary>渐隐后停止 <see cref="PlayGlobalLoop"/> 播放的循环音效；未在播放则无操作。</summary>
        public void StopGlobalLoopWithFade(string resName, float fadeSeconds)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return;
            }

            // 即使音源尚未加载完成（m_GlobalLoopSourceMap 里还查不到），也要让其失效，
            // 否则加载完成后会在已经离开的场景里迟到播放。
            BumpGlobalLoopVersion(resName);

            if (!m_GlobalLoopSourceMap.TryGetValue(resName, out var source) || source == null)
            {
                m_GlobalLoopSourceMap.Remove(resName);
                return;
            }

            m_GlobalLoopSourceMap.Remove(resName);

            if (fadeSeconds <= 0f)
            {
                source.Stop();
                m_AudioClipCache.Release(source);
                return;
            }

            source.DOFade(0f, fadeSeconds).OnComplete(() =>
            {
                source.Stop();
                source.volume = m_AudioVolume;
                m_AudioClipCache.Release(source);
            });
        }

        /// <summary>清理指定资源名当前的循环音源（若有），不改动版本号；供播放/停止内部复用。</summary>
        void ClearGlobalLoopSource(string resName)
        {
            if (m_GlobalLoopSourceMap.TryGetValue(resName, out var source) && source != null)
            {
                DOTween.Kill(source);
                source.Stop();
                m_AudioClipCache.Release(source);
            }

            m_GlobalLoopSourceMap.Remove(resName);
        }

        /// <summary>生成资源名的下一个请求版本号，使该资源名此前发出、尚未完成的 Play 请求失效。</summary>
        int BumpGlobalLoopVersion(string resName)
        {
            int next = (m_GlobalLoopVersionMap.TryGetValue(resName, out var v) ? v : 0) + 1;
            m_GlobalLoopVersionMap[resName] = next;
            return next;
        }

        bool IsGlobalLoopVersionCurrent(string resName, int version)
        {
            return m_GlobalLoopVersionMap.TryGetValue(resName, out var v) && v == version;
        }

        /// <summary>指定资源名的 <see cref="PlayGlobalLoop"/> 是否正在循环播放。</summary>
        public bool IsGlobalLoopPlaying(string resName)
        {
            return !string.IsNullOrEmpty(resName)
                && m_GlobalLoopSourceMap.TryGetValue(resName, out var source)
                && source != null;
        }

        /// <summary>
        /// 暂停 <see cref="PlayGlobalLoop"/> 播放中的循环音效（冻结在当前播放位置）；未在播放则无操作。
        /// 用于临时打断 BGM（如 S2-3 讲解视频播放期间），需配合 <see cref="ResumeGlobalLoop"/> 恢复。
        /// </summary>
        public void PauseGlobalLoop(string resName)
        {
            if (string.IsNullOrEmpty(resName)) return;
            if (m_GlobalLoopSourceMap.TryGetValue(resName, out var source) && source != null)
            {
                source.Pause();
            }
        }

        /// <summary>恢复被 <see cref="PauseGlobalLoop"/> 暂停的循环音效；未在暂停则无操作。</summary>
        public void ResumeGlobalLoop(string resName)
        {
            if (string.IsNullOrEmpty(resName)) return;
            if (m_GlobalLoopSourceMap.TryGetValue(resName, out var source) && source != null)
            {
                source.UnPause();
            }
        }

        public void PlayGameAudio(Vector3 position, AudioDefine audioId)
        {
            DoPlayAudio(audioId, position).Forget();
        }

        public void StopGameAudio(AudioDefine audioIdx)
        {
            if (m_LoopAudioSourceMap.TryGetValue((int)audioIdx, out var audioSource) && audioSource != null)
            {
                audioSource.Stop();
                m_LoopAudioSourceMap.Remove((int)audioIdx);
                m_AudioClipCache.Release(audioSource);
            }
        }

        void OnMainPlayerMove(bool isMove)
        {
            if (isMove)
            {
                // TODO:MYC 播放移动音效

            }
            else
            {
                // TODO:MYC 停止移动音效

            }
        }

        async UniTask DoPlayAudio(AudioDefine audioId, Vector3 position)
        {
            var tdData = TDAudioResourceTable.GetData(audioId);
            if (tdData is null)
            {
                return;
            }

            var clip = await GetAudioSource(tdData.ResName);
            if (clip == null)
            {
                return;
            }
            currentClipName = tdData.ResName;
            AudioSource source = null;
            if (m_LoopAudioSourceMap.TryGetValue((int)audioId, out var tempSource) && tempSource != null)
            {
                source = tempSource;
            }
            else
            {
                source = m_AudioClipCache.Get();
                m_AudioSourceList.Add(source);
            }
            source.name = $"{(int)audioId}";
            source.clip = clip;
            source.loop = tdData.IsLoop == 1;
            source.spatialBlend = tdData.Type;
            source.volume = audioVolume.TryGetValue(tdData.audioKind, out float volume) ? volume : m_AudioVolume;
            source.mute = audioMute.TryGetValue(tdData.audioKind, out bool mute) ? mute : false;

            //若为3D音效，怎么设置范围大小
            if (tdData.Type == 1)
            {
                source.transform.position = position;
                source.minDistance = 1f;
                source.maxDistance = 50f;
                //source.spread = 180f;贾哥不需要的
            }

            source.Play();

            if (tdData.IsLoop != 1)
            {
                await UniTask.WaitForSeconds(clip.length);
                m_AudioClipCache.Release(source);
                m_AudioSourceList.Remove(source);
            }
            else
            {
                m_LoopAudioSourceMap[(int)audioId] = source;
            }
        }

        /// <summary>
        /// 检查 YooAsset 是否已注册该音频寻址名（未注册则 ResLoader 会 LogError）。
        /// </summary>
        public static bool IsClipLocationValid(string resName)
        {
            if (string.IsNullOrEmpty(resName))
            {
                return false;
            }

            var package = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            return package != null && package.CheckLocationValid(resName);
        }

        /// <summary>
        /// 按资源名加载 AudioClip；寻址不存在时返回 null，不触发 ResLoader 报错。
        /// </summary>
        public async UniTask<AudioClip> TryLoadClipAsync(string resName)
        {
            if (!IsClipLocationValid(resName))
            {
                return null;
            }

            return await GetAudioSource(resName);
        }

        /// <summary>
        /// 按资源名加载并缓存 AudioClip（YooAsset 寻址名），供外部自行控制播放/循环/回调。
        /// 寻址不存在时 ResLoader 会 LogError 并返回 null。
        /// </summary>
        public UniTask<AudioClip> LoadClipAsync(string resName)
        {
            return GetAudioSource(resName);
        }

        async UniTask<AudioClip> GetAudioSource(string resPath)
        {
            if (!m_AudioClipsMap.TryGetValue(resPath, out var clip) || clip == null)
            {
                m_ResLoader.AddRes(resPath, ResType.AudioClip);
                await m_ResLoader.StartLoad();
                var handle = m_ResLoader.GetAsset(resPath);
                m_AudioClipsMap[resPath] = handle.AssetObject as AudioClip;
                m_AudioClipsHandleMap[resPath] = handle;
            }

            return m_AudioClipsMap[resPath];
        }

        public void SetAudioVolumeByKind(TDAudioResource.AudioKind kind, float volume)
        {
            audioVolume[kind] = volume;

            TDAudioResource tdData;
            foreach (AudioSource source in m_AudioSourceList)
            {
                tdData = TDAudioResourceTable.GetData(Enum.Parse<AudioDefine>(source.name));
                if (tdData != null && tdData.audioKind == kind)
                    source.volume = volume;
            }
        }

        public void SetAudioMuteByKind(TDAudioResource.AudioKind kind, bool mute)
        {
            audioMute[kind] = mute;

            TDAudioResource tdData;
            foreach (AudioSource source in m_AudioSourceList)
            {
                tdData = TDAudioResourceTable.GetData(Enum.Parse<AudioDefine>(source.name));
                if (tdData != null && tdData.audioKind == kind)
                    source.mute = mute;
            }
        }

        public void ReleaseAll()
        {
            try
            {
                m_ResLoader.ReleaseAll();
                m_AudioClipCache.DestroyAll();

                HashSet<string> allAudioNames = new HashSet<string>(m_AudioClipsMap.Keys);
                HashSet<string> playingAudioNames = new HashSet<string>();
                for (int i = m_AudioSourceList.Count - 1; i >= 0; i--)
                {
                    if (m_AudioSourceList[i])
                        playingAudioNames.Add(m_AudioSourceList[i].clip.name);
                    else
                        m_AudioSourceList.RemoveAt(i);
                }

                allAudioNames.ExceptWith(playingAudioNames);
                foreach (string name in allAudioNames)
                {
                    m_AudioClipsMap.Remove(name);
                    m_AudioClipsHandleMap[name].Dispose();
                    m_AudioClipsHandleMap.Remove(name);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
