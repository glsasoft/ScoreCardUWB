using UnityEngine;

/// <summary>
/// Android 系统音量控制工具类（静态）。
///
/// 直接桥接到 Android <c>AudioManager</c>，调整系统级音频流音量，
/// 与 Unity 音频系统（<c>AudioListener.volume</c>）及 ALSA 混音器完全独立。
///
/// 常用场景：
///   通过 MQTT 远程设置设备媒体音量（对 Unity 内置 AudioSource、Android MediaPlayer 均生效）。
///
/// 使用示例：
/// <code>
///   // 设置媒体音量到 70%
///   AndroidSystemVolume.SetPercent(70f);
///
///   // 静音
///   AndroidSystemVolume.SetPercent(0f);
///
///   // 查询当前媒体音量百分比
///   float pct = AndroidSystemVolume.GetPercent();
/// </code>
/// </summary>
public static class AndroidSystemVolume
{
    // ════════════════════════════════════════════════════════════════════
    //  Android AudioManager 音频流类型常量
    //  （对应 android.media.AudioManager 中的静态整型常量）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>媒体音量（音乐/视频），比分牌播放的声音走此流。</summary>
    public const int StreamMusic        = 3;

    /// <summary>通话音量。</summary>
    public const int StreamVoiceCall    = 0;

    /// <summary>铃声音量。</summary>
    public const int StreamRing         = 2;

    /// <summary>系统提示音音量。</summary>
    public const int StreamSystem       = 1;

    /// <summary>通知音量。</summary>
    public const int StreamNotification = 5;

    // ════════════════════════════════════════════════════════════════════
    //  AudioManager.adjustStreamVolume / setStreamVolume flags
    // ════════════════════════════════════════════════════════════════════

    /// <summary>调整音量时在屏幕上弹出系统音量 UI 条。</summary>
    private const int FlagShowUi = 1;

    /// <summary>静默调整，不弹 UI。</summary>
    private const int FlagSilent = 0;

    // ════════════════════════════════════════════════════════════════════
    //  核心接口
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取指定流的当前音量档位（绝对值）。
    /// </summary>
    /// <param name="streamType">音频流类型，默认 <see cref="StreamMusic"/>（媒体音量）</param>
    public static int GetLevel(int streamType = StreamMusic)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var audioManager = GetAudioManager();
            return audioManager.Call<int>("getStreamVolume", streamType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SystemVolume] GetLevel failed: {ex.Message}");
            return 0;
        }
#else
        return 10; // Editor 模拟：返回固定档位
#endif
    }

    /// <summary>
    /// 获取指定流的最大音量档位。Android 媒体音量通常为 15 档。
    /// </summary>
    /// <param name="streamType">音频流类型，默认 <see cref="StreamMusic"/></param>
    public static int GetMaxLevel(int streamType = StreamMusic)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var audioManager = GetAudioManager();
            return audioManager.Call<int>("getStreamMaxVolume", streamType);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SystemVolume] GetMaxLevel failed: {ex.Message}");
            return 15;
        }
#else
        return 15;
#endif
    }

    /// <summary>
    /// 以百分比形式查询当前音量（0~100）。
    /// </summary>
    /// <param name="streamType">音频流类型，默认 <see cref="StreamMusic"/></param>
    public static float GetPercent(int streamType = StreamMusic)
    {
        int max = GetMaxLevel(streamType);
        if (max <= 0) return 0f;
        return GetLevel(streamType) * 100f / max;
    }

    /// <summary>
    /// 设置指定流的绝对音量档位。
    /// </summary>
    /// <param name="level">音量档位（0 ~ <see cref="GetMaxLevel"/>）</param>
    /// <param name="streamType">音频流类型，默认 <see cref="StreamMusic"/></param>
    /// <param name="showUi">是否弹出系统音量 UI 条，默认不弹</param>
    public static void SetLevel(int level, int streamType = StreamMusic, bool showUi = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var audioManager = GetAudioManager();
            int max = audioManager.Call<int>("getStreamMaxVolume", streamType);
            int clamped = Mathf.Clamp(level, 0, max);
            audioManager.Call("setStreamVolume", streamType, clamped, showUi ? FlagShowUi : FlagSilent);
            Debug.Log($"[SystemVolume] SetLevel: stream={streamType} level={clamped}/{max}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SystemVolume] SetLevel failed: {ex.Message}");
        }
#else
        Debug.Log($"[SystemVolume] SetLevel (Editor mock): stream={streamType} level={level} showUi={showUi}");
#endif
    }

    /// <summary>
    /// 以百分比形式设置音量（0~100）。
    /// 自动映射到实际档位，四舍五入到最近档。
    /// </summary>
    /// <param name="percent">音量百分比，0 = 静音，100 = 最大音量</param>
    /// <param name="streamType">音频流类型，默认 <see cref="StreamMusic"/></param>
    /// <param name="showUi">是否弹出系统音量 UI 条，默认不弹</param>
    public static void SetPercent(float percent, int streamType = StreamMusic, bool showUi = false)
    {
        float clamped = Mathf.Clamp(percent, 0f, 100f);
#if UNITY_ANDROID && !UNITY_EDITOR
        int max = GetMaxLevel(streamType);
        int level = Mathf.RoundToInt(clamped / 100f * max);
        SetLevel(level, streamType, showUi);
#else
        Debug.Log($"[SystemVolume] SetPercent (Editor mock): {clamped:F1}% stream={streamType}");
#endif
    }

    // ════════════════════════════════════════════════════════════════════
    //  便捷方法（媒体音量快捷操作）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>将媒体音量静音（档位设为 0）。</summary>
    public static void Mute() => SetLevel(0, StreamMusic, false);

    /// <summary>将媒体音量设置为最大档位。</summary>
    public static void MaxOut() => SetLevel(GetMaxLevel(StreamMusic), StreamMusic, false);

    // ════════════════════════════════════════════════════════════════════
    //  私有工具
    // ════════════════════════════════════════════════════════════════════

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>获取 AudioManager 实例（调用方负责 using 释放）。</summary>
    private static AndroidJavaObject GetAudioManager()
    {
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        // getSystemService("audio") 返回 AudioManager
        return activity.Call<AndroidJavaObject>("getSystemService", "audio");
    }
#endif
}
