using UnityEngine;

public enum AudioOutput
{
    HDMI    = 0,
    Builtin = 1,
}

/// <summary>
/// Unity ↔ Android ALSA 音频桥接的 C# 侧封装。
///
/// 提供两套播放路径：
///   1. 文件路径（PlayBGM / PlaySFX / PlaySustained）
///      要求音频文件存放在 Assets/StreamingAssets/ 下，传入相对路径，如 "audio/bgm.mp3"。
///
///   2. AudioClip 直接播放（PlayClipSFX / PlayClipBGM / PlayClipSustained）
///      内部将 AudioClip.GetData() 转换为 S16_LE PCM 字节数组后传给 Java 层，
///      与 YooAsset / Resources 等资源系统无缝兼容，无需文件落盘。
/// </summary>
public static class NativeAudio
{
#if UNITY_ANDROID && !UNITY_EDITOR

    private static AndroidJavaClass _bridge;
    private static AndroidJavaClass Bridge =>
        _bridge ??= new AndroidJavaClass(
            "com.glsa.mqttscorecard5s.unity.UnityAudioBridge");

    private static AndroidJavaObject Activity {
        get {
            using var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return up.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    // ── 调试：打印 ALSA 设备列表 ──────────────────────────────────────────

    /// <summary>
    /// 读取设备上所有可用的 ALSA 声卡，并以 Debug.Log 打印。
    /// 在 Init() 之前调用，用于确认正确的 card 编号。
    /// </summary>
    public static void LogAlsaCards()
    {
        string cards  = Bridge.CallStatic<string>("listAlsaCards");
        string raw    = Bridge.CallStatic<string>("readCardsProc");
        Debug.Log($"[NativeAudio] ===== ALSA 声卡列表 =====\n{cards}");
        Debug.Log($"[NativeAudio] /proc/asound/cards 原文:\n{raw}");
    }

    /// <summary>
    /// 诊断用：在 Java 端合成 1 秒 880Hz sine wave 直接送给 mixer。
    /// 完全绕开 Unity 资源 / AudioClip，纯粹测试 ALSA → HDMI 通路。
    /// 听到声音 = 通路 OK；听不到 = 进程内 ALSA 通路被静音。
    /// </summary>
    public static void PlayTestTone()
        => Bridge.CallStatic("playTestTone");

    // ── 初始化 / 声卡切换 ──────────────────────────────────────────────────

    /// <summary>
    /// 启动时调一次，选定输出声卡。
    /// </summary>
    /// <param name="output">输出目标：HDMI 或板载声卡</param>
    /// <param name="hdmiCard">HDMI card 号，默认1（adb shell cat /proc/asound/cards 确认）</param>
    /// <param name="builtinCard">板载声卡 card 号，默认0</param>
    public static void Init(
        AudioOutput output      = AudioOutput.HDMI,
        int         hdmiCard    = 2,
        int         builtinCard = 1)
    {
        Bridge.CallStatic("init", Activity, (int)output, hdmiCard, builtinCard);
    }

    /// <summary>运行时切换输出声卡（会停止当前所有播放）。</summary>
    public static void SetOutput(AudioOutput output)
        => Bridge.CallStatic("setOutput", (int)output);

    // ── 文件路径播放（要求文件在 StreamingAssets）─────────────────────────

    public static void PlayBGM(string path)
        => Bridge.CallStatic("playBGM", path);

    public static void StopBGM()
        => Bridge.CallStatic("stopBGM");

    public static void PlaySustained(string name, string path, bool loop = false)
        => Bridge.CallStatic("playSustained", name, path, loop);

    public static void StopSustained(string name)
        => Bridge.CallStatic("stopSustained", name);

    public static void PlaySFX(string path)
        => Bridge.CallStatic("playSFX", path);

    // ── AudioClip 直接播放（YooAsset / Resources 均可）──────────────────────
    //
    // 以下 PlayClip* 方法接受已转换好的 PCM byte[]，
    // 由调用方（AudioPlayModule）在线程池完成转换后再调用，避免阻塞主线程。
    // 外部不应直接使用这组方法；请通过 AudioPlayModule 发起播放。

    internal static void PlayClipSFXDirect(byte[] pcm, int frequency, int channels)
        => Bridge.CallStatic("playSFXFromPcm", pcm, frequency, channels);

    internal static void PlayClipBGMDirect(byte[] pcm, int frequency, int channels)
        => Bridge.CallStatic("playBGMFromPcm", pcm, frequency, channels);

    public static void StopClipBGM()
        => Bridge.CallStatic("stopBGMPcm");

    internal static void PlayClipSustainedDirect(string name, byte[] pcm,
                                                  int frequency, int channels, bool loop)
        => Bridge.CallStatic("playSustainedFromPcm", name, pcm, frequency, channels, loop);

    public static void StopClipSustained(string name)
        => Bridge.CallStatic("stopSustainedPcm", name);

    // ── 全局停止 ──────────────────────────────────────────────────────────

    /// <summary>停止所有正在播放的声音（文件路径 + PCM 两条路径）。</summary>
    public static void StopAll()
        => Bridge.CallStatic("stopAll");

    // ── 工具：AudioClip 采样提取（主线程调用）+ PCM 转换（线程池调用）────────
    //
    // 调用方负责线程切换：
    //   1. 主线程：GetClipSamples()        —— AudioClip.GetData 必须主线程
    //   2. 线程池：FloatToPcm16()          —— 纯 CPU 运算，线程安全
    //   3. 主线程：PlayClip*Direct()       —— JNI 调用，回主线程以保证兼容性

    /// <summary>
    /// 从 AudioClip 读取 PCM float 样本，必须在 Unity 主线程调用。
    /// 调用方应保证 clip.loadState == Loaded（loadType 设为 DecompressOnLoad 并勾选 Preload Audio Data）。
    /// </summary>
    public static float[] GetClipSamples(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        return samples;
    }

    /// <summary>
    /// 将 float[] PCM 转换为 S16_LE byte[]，纯 CPU 运算，可在线程池调用。
    /// volume 范围 0.0～1.0。
    /// </summary>
    public static byte[] FloatToPcm16(float[] samples, float volume)
    {
        float  gain = System.Math.Max(0f, System.Math.Min(1f, volume));
        byte[] pcm  = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            float  f = samples[i] * gain;
            if (f >  1f) f =  1f;
            if (f < -1f) f = -1f;
            short s        = (short)(f * 32767f);
            pcm[i * 2]     = (byte)(s         & 0xFF);
            pcm[i * 2 + 1] = (byte)((s >> 8)  & 0xFF);
        }
        return pcm;
    }

#else
    // ── Editor / 非 Android 平台 存根（仅输出日志）────────────────────────

    public static void LogAlsaCards()
        => Debug.Log("[NativeAudio] LogAlsaCards: Editor 模式，无 ALSA 设备");

    public static void PlayTestTone()
        => Debug.Log("[NativeAudio] PlayTestTone: Editor 模式，无 ALSA 设备");

    public static void Init(AudioOutput o = AudioOutput.HDMI, int hc = 1, int bc = 0)
        => Debug.Log($"[NativeAudio] Init output={o} hdmiCard={hc} builtinCard={bc}");

    public static void SetOutput(AudioOutput o)
        => Debug.Log($"[NativeAudio] SetOutput: {o}");

    public static void PlayBGM(string p)
        => Debug.Log($"[NativeAudio] PlayBGM: {p}");

    public static void StopBGM()
        => Debug.Log("[NativeAudio] StopBGM");

    public static void PlaySustained(string n, string p, bool loop = false)
        => Debug.Log($"[NativeAudio] PlaySustained[{n}]: {p} loop={loop}");

    public static void StopSustained(string n)
        => Debug.Log($"[NativeAudio] StopSustained: {n}");

    public static void PlaySFX(string p)
        => Debug.Log($"[NativeAudio] PlaySFX: {p}");

    public static void StopClipBGM()
        => Debug.Log("[NativeAudio] StopClipBGM");

    public static void StopClipSustained(string name)
        => Debug.Log($"[NativeAudio] StopClipSustained: {name}");

    public static void StopAll()
        => Debug.Log("[NativeAudio] StopAll");

    // Editor 下的 Direct 存根（AudioPlayModule 内部调用）
    internal static void PlayClipSFXDirect(byte[] pcm, int frequency, int channels)
        => Debug.Log($"[NativeAudio] PlayClipSFXDirect {frequency}Hz ch={channels} bytes={pcm?.Length}");

    internal static void PlayClipBGMDirect(byte[] pcm, int frequency, int channels)
        => Debug.Log($"[NativeAudio] PlayClipBGMDirect {frequency}Hz ch={channels}");

    internal static void PlayClipSustainedDirect(string name, byte[] pcm,
                                                  int frequency, int channels, bool loop)
        => Debug.Log($"[NativeAudio] PlayClipSustainedDirect[{name}] {frequency}Hz loop={loop}");

    // Editor 下工具方法保持可用（逻辑相同，纯 C# 运算）
    public static float[] GetClipSamples(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        return samples;
    }

    public static byte[] FloatToPcm16(float[] samples, float volume)
    {
        float  gain = System.Math.Max(0f, System.Math.Min(1f, volume));
        byte[] pcm  = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            float  f = samples[i] * gain;
            if (f >  1f) f =  1f;
            if (f < -1f) f = -1f;
            short s        = (short)(f * 32767f);
            pcm[i * 2]     = (byte)(s         & 0xFF);
            pcm[i * 2 + 1] = (byte)((s >> 8)  & 0xFF);
        }
        return pcm;
    }
#endif
}
