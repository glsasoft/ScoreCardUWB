using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LetsPiu.Main.Common.Scripts
{
    public class WebAudioPlayer : MonoBehaviour
    {
        public AudioSource audioSource;
        private AudioClip downloadedClip; // 下载的音频剪辑
        private bool isDownloading = false;
        private string currentUrl = null;
        
        private Coroutine trackPlayProgress = null;
        
        public event Action PlayStartEvent;
        public event Action<string> PlayEndEvent;
        public event Action PlayPauseEvent;
        public event Action PlayResumeEvent;

        private static float audioVolume = 1.0f;
        public static void SetAudioVolume(float volume)
        {
            audioVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 将 <see cref="SetAudioVolume"/> 写入的静态音量同步到当前 <see cref="AudioSource"/>（播放中调节 MQTT/设置时立即生效）。
        /// </summary>
        public void ApplyStaticVolumeToSource()
        {
            InitAudioSource();
            audioSource.volume = audioVolume;
        }

        private void Awake()
        {
            InitAudioSource();
        }

        private void InitAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.volume = 3f;
            }
        }
        
        /// <summary>
        /// 是否正在播放中
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            InitAudioSource();
            return isDownloading || audioSource.isPlaying;
        }
        
        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="url"></param>
        /// <param name="loop">是否循环；循环模式下不会在结束时自动触发单次播放的结束回调协程。</param>
        public async UniTask<bool> PlayAudioFromURLAsync(string url, bool loop = false)
        {
            if (isDownloading) return false;

            if (audioSource.isPlaying)
            {
                StopAudio();
                if (downloadedClip != null)
                {
                    Destroy(downloadedClip);
                }
            }
            
            return await DownloadAndPlayAudio(url, loop);
        }
        
        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public void PlayAudioFromURL(string url, Action<string, bool> callback)
        {
            StartCoroutine(PlayAudioFromURLAsync(url).ToCoroutine(success =>
            {
                callback(url, success);
            }));
        }
        
        /// <summary>
        /// 下载音频并播放
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async UniTask<bool> DownloadAndPlayAudio(string url, bool loop = false)
        {
            isDownloading = true;
            var success = false;
            
            var audioType = AudioType.UNKNOWN;
            if (url.EndsWith(".mp3")) audioType = AudioType.MPEG;
            if (url.EndsWith(".wav")) audioType = AudioType.WAV;
            
            using (var www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    downloadedClip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = downloadedClip;
                    audioSource.loop = loop;
                    audioSource.volume = audioVolume;
                    audioSource.Play();
                    success = true;
                    currentUrl = url;
                    PlayStartEvent?.Invoke();
                    Debug.Log("WebAudioPlayer Audio playing: " + url + " loop=" + loop);
                    if (!loop)
                    {
                        if (trackPlayProgress != null)
                        {
                            StopCoroutine(trackPlayProgress);
                            trackPlayProgress = null;
                        }

                        trackPlayProgress = StartCoroutine(TrackPlayProgress());
                    }
                    else if (trackPlayProgress != null)
                    {
                        StopCoroutine(trackPlayProgress);
                        trackPlayProgress = null;
                    }
                }
                else
                {
                    Debug.LogError("WebAudioPlayer Failed to download audio: " + www.error);
                }
            }
            isDownloading = false;
            return success;
        }
        
        // 跟踪播放进度
        private IEnumerator TrackPlayProgress()
        {
            while (audioSource.isPlaying || 
                   (audioSource.time > 0 && !audioSource.isPlaying))
            {
                yield return null;
            }

            if (trackPlayProgress == null) yield break;
            PlayEndEvent?.Invoke(currentUrl);
            StopCoroutine(trackPlayProgress);
        }

        /// <summary>
        /// 暂停/恢复播放
        /// </summary>
        public void TogglePause()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                PlayPauseEvent?.Invoke();
            }
            else
            {
                audioSource.UnPause();
                PlayResumeEvent?.Invoke();
            }
        }

        /// <summary>
        /// 若当前正在播放则暂停（用于 MQTT 等远程指令，避免 Toggle 语义）。
        /// </summary>
        public void PausePlaybackOnly()
        {
            InitAudioSource();
            if (!audioSource.isPlaying)
                return;
            audioSource.Pause();
            PlayPauseEvent?.Invoke();
        }

        /// <summary>
        /// 恢复暂停的剪辑。
        /// </summary>
        public void ResumePlaybackOnly()
        {
            InitAudioSource();
            if (audioSource.clip == null)
                return;
            audioSource.UnPause();
            PlayResumeEvent?.Invoke();
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopAudio()
        {
            InitAudioSource();
            audioSource.Stop();
            if (trackPlayProgress != null)
            {
                StopCoroutine(trackPlayProgress);
                trackPlayProgress = null;
            }

            PlayEndEvent?.Invoke(currentUrl);
            currentUrl = null;
        }

        /// <summary>
        /// 设置音量 (0.0 ~ 1.0)
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            audioSource.volume = volume;
        }

        public void SetMute(bool mute)
        {
            audioSource.mute = mute;
        }
    }
}
