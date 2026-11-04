using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 独立的 ALSA 声卡过滤播放工具（Android 端）。
///
/// 通过声卡名称关键词（如 "hdmi"）自动过滤目标声卡并进行混音播放，
/// 与 Unity 原生音频系统（AudioSource / AudioMixer）完全独立。
///
/// 播放控制：
///   - <see cref="Play"/>               - 播放 PCM 字节数据到指定声卡
///   - <see cref="PlayFromClip"/>       - 从 AudioClip 获取 PCM 并播放
///   - <see cref="PlayFromAsset"/>      - 异步解码 assets 中的 MP3 并播放
///   - <see cref="Pause"/>              - 暂停指定 key 的播放（保留状态）
///   - <see cref="Resume"/>             - 恢复已暂停的播放
///   - <see cref="Stop"/>               - 停止并清除指定 key
///   - <see cref="StopAll"/>            - 停止所有由本工具管理的声音
///
/// 状态查询：
///   - <see cref="IsPlaying"/>          - key 是否正在播放（注册且未暂停）
///   - <see cref="IsPaused"/>           - key 是否已暂停
///   - <see cref="IsNamedVoicePlaying"/>- 混音池中是否存在该命名 voice（防重复触发）
///
/// 音量控制：
///   - <see cref="SetMasterVolume"/>    - 设置混音器总输出音量（0~1.5）
///   - <see cref="GetMasterVolume"/>    - 查询当前总音量
///   - <see cref="SetVoiceVolume"/>     - 单独调整某路 voice 的音量
///
/// 硬件控制：
///   - <see cref="ApplyCodecUnmute"/>   - 解除板载 codec 静音（ES8388 等）
///
/// 支持多路并发：不同 key 可同时播放，互相叠加混音。
/// 同一 key 重复调用 Play 会替换旧的播放实例。
/// </summary>
public static class AlsaDevicePlayer
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private const string JavaClass = "com.glsa.mqttscorecard5s.unity.AlsaDevicePlayer";

    private static AndroidJavaObject GetActivity()
    {
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return player.GetStatic<AndroidJavaObject>("currentActivity");
    }
#endif

    // ────────────────────────────────────────────────────────────────────
    //  主动初始化（可选）
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 预先查找并启动指定过滤名声卡的 Mixer（可选调用）。
    /// 首次 Play 时也会自动初始化，此方法用于提前预热以减少首播延迟。
    /// </summary>
    /// <param name="filterName">声卡名称关键词，例如 "hdmi"</param>
    /// <returns>找到的 card 索引，-1 表示未找到</returns>
    public static int Init(string filterName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<int>("findAndStartMixer", filterName);
#else
        Debug.Log($"[AlsaDevicePlayer] Init: filter={filterName} (Editor mock)");
        return 0;
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  播放接口
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 播放 PCM 字节数组到指定声卡。
    /// 大数组会经 JNI 拷贝，易卡主线程；BGM 等大文件请用 <see cref="PlayFromPcm16File"/>。
    /// </summary>
    public static void Play(string filterName, string key, byte[] pcm,
                            int sampleRate, int channels, bool loop = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("play", filterName, key, pcm, sampleRate, channels, loop);
#else
        Debug.Log($"[AlsaDevicePlayer] Play: filter={filterName} key={key} " +
                  $"rate={sampleRate} ch={channels} loop={loop} bytes={pcm?.Length} (Editor mock)");
#endif
    }

    /// <summary>
    /// 后台读取 .pcm16 缓存文件并播放（不经 JNI 搬运大 byte[]）。
    /// </summary>
    public static void PlayFromPcm16File(string filterName, string key, string path,
                                         bool loop = false, float volume = 1f)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("playFromPcm16File", filterName, key, path, loop, volume);
#else
        Debug.Log($"[AlsaDevicePlayer] PlayFromPcm16File: filter={filterName} key={key} " +
                  $"path={path} loop={loop} vol={volume} (Editor mock)");
#endif
    }

    /// <summary>
    /// 从 AudioClip 异步转 PCM 后播放到指定声卡（不阻塞调用方；大文件转换在线程池完成）。
    /// </summary>
    /// <param name="pcmCachePath">可选本地文件路径，用于 PCM 磁盘/内存缓存（见 <see cref="AudioClipPcmConverter.MakeFileCacheKey"/>）</param>
    /// <param name="usePcmCache">是否读写 PCM 缓存</param>
    public static void PlayFromClip(string filterName, string key, AudioClip clip, bool loop = false,
                                    string pcmCachePath = null, bool usePcmCache = true)
    {
        PlayFromClipAsync(filterName, key, clip, loop, pcmCachePath, usePcmCache).Forget();
    }

    /// <summary>
    /// 异步转换并播放；可 await 以在真正开播后再设音量等。
    /// </summary>
    public static async UniTask PlayFromClipAsync(string filterName, string key, AudioClip clip, bool loop = false,
                                                  string pcmCachePath = null, bool usePcmCache = true,
                                                  float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogError("[AlsaDevicePlayer] PlayFromClip: AudioClip is null");
            return;
        }

        string cacheKey = usePcmCache && !string.IsNullOrEmpty(pcmCachePath)
            ? AudioClipPcmConverter.MakeFileCacheKey(pcmCachePath)
            : null;

        try
        {
            if (!string.IsNullOrEmpty(cacheKey) &&
                AudioClipPcmConverter.TryGetDiskCachePath(cacheKey, out string existingPath))
            {
                await UniTask.SwitchToMainThread();
                PlayFromPcm16File(filterName, key, existingPath, loop, volume);
                return;
            }

            var pcm = await AudioClipPcmConverter.ConvertAsync(clip, cacheKey, usePcmCache);
            await UniTask.SwitchToMainThread();

            if (!string.IsNullOrEmpty(cacheKey) &&
                AudioClipPcmConverter.TryGetDiskCachePath(cacheKey, out string diskPath))
            {
                PlayFromPcm16File(filterName, key, diskPath, loop, volume);
                return;
            }

            // 无磁盘路径的大 PCM 仍走 JNI；尽量避免（MusicMgr 应传文件路径）
            Play(filterName, key, pcm.Data, pcm.SampleRate, pcm.Channels, loop);
            if (volume >= 0f && !Mathf.Approximately(volume, 1f))
                SetVoiceVolume(key, volume);
        }
        catch (Exception e)
        {
            Debug.LogError($"[AlsaDevicePlayer] PlayFromClip 失败 key={key}: {e.Message}");
        }
    }

    /// <summary>
    /// 异步解码 APK assets 中的 MP3/WAV 文件并播放到指定声卡。
    /// 解码在后台线程进行，不阻塞主线程。
    /// </summary>
    /// <param name="filterName">声卡过滤关键词，例如 "hdmi"</param>
    /// <param name="key">播放标识符</param>
    /// <param name="assetPath">assets 相对路径，例如 "music/bg.mp3"</param>
    /// <param name="loop">是否循环播放</param>
    public static void PlayFromAsset(string filterName, string key, string assetPath, bool loop = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        using var ctx = GetActivity();
        jc.CallStatic("playFromAsset", ctx, filterName, key, assetPath, loop);
#else
        Debug.Log($"[AlsaDevicePlayer] PlayFromAsset: filter={filterName} key={key} " +
                  $"asset={assetPath} loop={loop} (Editor mock)");
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  暂停 / 恢复 / 停止
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 暂停指定 key 的播放（从 Mixer 移除 voice，保留 PCM 状态）。
    /// 可通过 <see cref="Resume"/> 恢复。
    /// </summary>
    public static void Pause(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("pause", key);
#else
        Debug.Log($"[AlsaDevicePlayer] Pause: key={key} (Editor mock)");
#endif
    }

    /// <summary>
    /// 恢复已暂停的播放（从头开始重新播放 PCM）。
    /// </summary>
    /// <param name="filterName">声卡过滤关键词（用于确保 Mixer 运行中）</param>
    /// <param name="key">播放标识符</param>
    public static void Resume(string filterName, string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("resume", filterName, key);
#else
        Debug.Log($"[AlsaDevicePlayer] Resume: filter={filterName} key={key} (Editor mock)");
#endif
    }

    /// <summary>
    /// 停止并清除指定 key 的播放状态。
    /// </summary>
    public static void Stop(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("stop", key);
#else
        Debug.Log($"[AlsaDevicePlayer] Stop: key={key} (Editor mock)");
#endif
    }

    /// <summary>
    /// 停止所有由 AlsaDevicePlayer 管理的声音（不影响 UnityAudioBridge 的声音）。
    /// </summary>
    public static void StopAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("stopAll");
#else
        Debug.Log("[AlsaDevicePlayer] StopAll (Editor mock)");
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  状态查询
    // ────────────────────────────────────────────────────────────────────

    /// <summary>是否正在播放指定 key（已注册且未暂停）。</summary>
    public static bool IsPlaying(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<bool>("isPlaying", key);
#else
        return false;
#endif
    }

    /// <summary>是否已暂停指定 key。</summary>
    public static bool IsPaused(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<bool>("isPaused", key);
#else
        return false;
#endif
    }

    /// <summary>
    /// 查询指定名字的 voice 是否在混音池中（未完成且未被 stop）。
    /// 底层：TinyAlsaMixer.hasNamedVoice(name)。
    /// 可用于防止重复触发同一段音频，例如 BGM 点击防重。
    /// </summary>
    public static bool IsNamedVoicePlaying(string name)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<bool>("isNamedVoicePlaying", name);
#else
        return false;
#endif
    }

    /// <summary>返回所有可用 ALSA 声卡的文本描述（调试用）。</summary>
    public static string ListCards()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<string>("listCards");
#else
        return "[Editor mock] No ALSA cards available";
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  音量控制
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 设置混音器总输出音量（对所有 voice 生效）。
    /// 底层：TinyAlsaMixer.setMasterVolume(float)。
    /// </summary>
    /// <param name="volume">
    /// 0.0 = 静音；1.0 = 原始电平（默认）；最大 1.5（软 boost，超过 1.0 可能产生轻微失真）
    /// </param>
    public static void SetMasterVolume(float volume)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("setMasterVolume", volume);
#else
        Debug.Log($"[AlsaDevicePlayer] SetMasterVolume: {volume} (Editor mock)");
#endif
    }

    /// <summary>
    /// 查询当前混音器总输出音量。
    /// 底层：TinyAlsaMixer.getMasterVolume()。
    /// </summary>
    public static float GetMasterVolume()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<float>("getMasterVolume");
#else
        return 1f;
#endif
    }

    /// <summary>
    /// 单独调整某路命名 voice 的音量（不打断播放）。
    /// 底层：TinyAlsaMixer.setVoiceVolumeByName(name, volume)。
    /// 典型用途：单独降低 BGM 音量而不影响 SFX。
    /// </summary>
    /// <param name="key">播放时传入的 key</param>
    /// <param name="volume">0.0~1.0</param>
    /// <returns>true=成功；false=该 key 对应的 voice 不存在</returns>
    public static bool SetVoiceVolume(string key, float volume)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        return jc.CallStatic<bool>("setVoiceVolume", key, volume);
#else
        Debug.Log($"[AlsaDevicePlayer] SetVoiceVolume: key={key} vol={volume} (Editor mock)");
        return true;
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  硬件 Codec 控制
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 手动为指定声卡的 codec 执行解静音配置。
    /// 底层：TinyAlsaPlayer.nativeApplyCodecUnmute(card)。
    ///
    /// 绕开 AudioFlinger 后，板载 codec（如 ES8388）默认全静音；
    /// 调用本方法等价于在 shell 跑 tinymix 打开 DAC/HP/SPK 通路。
    /// HDMI 声卡无此问题，可不调。
    /// </summary>
    /// <param name="card">ALSA card 索引（板载声卡通常为 1）</param>
    public static void ApplyCodecUnmute(int card)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using var jc = new AndroidJavaClass(JavaClass);
        jc.CallStatic("applyCodecUnmute", card);
#else
        Debug.Log($"[AlsaDevicePlayer] ApplyCodecUnmute: card={card} (Editor mock)");
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  PCM 工具
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 同步将 AudioClip 转为 S16_LE（主线程 GetData + 同线程 float→S16）。
    /// 播放请用 <see cref="PlayFromClipAsync"/>；本方法仅留给需要立即拿到 PCM 的少数调用方。
    /// </summary>
    public static byte[] AudioClipToPcm(AudioClip clip)
    {
        if (clip == null) throw new ArgumentNullException(nameof(clip));
        float[] samples = NativeAudio.GetClipSamples(clip);
        return NativeAudio.FloatToPcm16(samples, 1f);
    }

    /// <summary>
    /// 合成一段正弦波测试音，返回 S16_LE PCM 字节数组。
    /// 不需要任何音频资源文件，适合快速测试声卡通路。
    /// </summary>
    /// <param name="frequency">频率（Hz），例如 440（A调）、880</param>
    /// <param name="durationSeconds">时长（秒）</param>
    /// <param name="sampleRate">采样率，默认 48000</param>
    /// <param name="channels">声道数，默认 2</param>
    /// <param name="amplitude">幅度 0.0~1.0，默认 0.5</param>
    public static byte[] SynthSine(float frequency = 440f, float durationSeconds = 1f,
                                   int sampleRate = 48000, int channels = 2, float amplitude = 0.5f)
    {
        int totalFrames = (int)(sampleRate * durationSeconds);
        byte[] pcm = new byte[totalFrames * channels * 2];
        for (int i = 0; i < totalFrames; i++)
        {
            short s16 = (short)(Math.Sin(2 * Math.PI * frequency * i / sampleRate) * amplitude * 32767);
            int offset = i * channels * 2;
            for (int c = 0; c < channels; c++)
            {
                pcm[offset + c * 2]     = (byte)(s16 & 0xFF);
                pcm[offset + c * 2 + 1] = (byte)((s16 >> 8) & 0xFF);
            }
        }
        return pcm;
    }
}
