using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 外置输出设备 PCM 播放的统一入口：Android 走 ALSA（<see cref="AlsaDevicePlayer"/>），
/// Windows / Windows Editor 走 WASAPI（与 Unity 内置音频无关）。
/// </summary>
public readonly struct ExternalAudioDeviceOption
{
    public readonly string Selector;
    public readonly string Caption;

    public ExternalAudioDeviceOption(string selector, string caption)
    {
        Selector = selector ?? "";
        Caption = caption ?? "";
    }
}

public static class ExternalDeviceAudioPlayer
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private static string _lastWindowsSelector = "\u0001";
#endif

    /// <summary>填充当前平台可用的输出设备列表（用于下拉框）。</summary>
    public static void FillDeviceOptions(List<ExternalAudioDeviceOption> sink)
    {
        if (sink == null) throw new ArgumentNullException(nameof(sink));
        sink.Clear();

#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (string line in AlsaDevicePlayer.ListCards().Split('\n'))
        {
            var m = Regex.Match(line.Trim(), @"^card\s+\d+\s+\[(\w+)\]\s+(.+)$");
            if (!m.Success) continue;
            string id = m.Groups[1].Value;
            string desc = m.Groups[2].Value.Trim();
            sink.Add(new ExternalAudioDeviceOption(id, $"[{id}] {desc}"));
        }
        if (sink.Count == 0)
            sink.Add(new ExternalAudioDeviceOption("hdmi0", "(未找到声卡) 仍可用 hdmi0 过滤"));
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        sink.Add(new ExternalAudioDeviceOption("", "(系统默认输出设备)"));
        var tmp = new List<WindowsWasapiExternalAudio.DeviceEntry>();
        WindowsWasapiExternalAudio.EnumerateRenderDevices(tmp);
        foreach (var e in tmp)
            sink.Add(new ExternalAudioDeviceOption(e.DeviceId, e.FriendlyName));
#else
        sink.Add(new ExternalAudioDeviceOption("", "(当前平台无外置设备播放插件)"));
#endif
    }

    /// <summary>多行文本，便于日志 / 调试。</summary>
    public static string ListDevicesDebugText()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.ListCards();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var sb = new System.Text.StringBuilder();
        var tmp = new List<WindowsWasapiExternalAudio.DeviceEntry>();
        WindowsWasapiExternalAudio.EnumerateRenderDevices(tmp);
        foreach (var e in tmp)
            sb.AppendLine(e.FriendlyName + "\t" + e.DeviceId);
        if (sb.Length == 0) return "(无渲染设备)";
        return sb.ToString();
#else
        return "[当前平台] 无外置设备枚举";
#endif
    }

    /// <summary>
    /// 预先初始化输出设备（可选）。Android：声卡关键词；Windows：设备 ID 或友好名称子串。
    /// </summary>
    public static int Init(string deviceSelector)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.Init(deviceSelector);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        int r = WindowsWasapiExternalAudio.Init(deviceSelector ?? "");
        if (r == 0) _lastWindowsSelector = deviceSelector ?? "";
        return r;
#else
        Debug.Log($"[ExternalDeviceAudio] Init (stub): {deviceSelector}");
        return 0;
#endif
    }

    public static void Play(string deviceSelector, string key, byte[] pcm,
        int sampleRate, int channels, bool loop = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.Play(deviceSelector, key, pcm, sampleRate, channels, loop);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string sel = deviceSelector ?? "";
        if (_lastWindowsSelector != sel)
        {
            Init(sel);
            _lastWindowsSelector = sel;
        }
        WindowsWasapiExternalAudio.Play(key, pcm, sampleRate, channels, loop);
#else
        Debug.Log($"[ExternalDeviceAudio] Play stub key={key}");
#endif
    }

    /// <summary>后台读 .pcm16 缓存并播放（Android 不经 JNI 大数组拷贝）。</summary>
    public static void PlayFromPcm16File(string deviceSelector, string key, string path,
                                         bool loop = false, float volume = 1f)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.PlayFromPcm16File(deviceSelector, key, path, loop, volume);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        PlayFromPcm16FileOnWindowsAsync(deviceSelector, key, path, loop, volume).Forget();
#else
        Debug.Log($"[ExternalDeviceAudio] PlayFromPcm16File stub key={key} path={path}");
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    static async UniTaskVoid PlayFromPcm16FileOnWindowsAsync(string deviceSelector, string key,
        string path, bool loop, float volume)
    {
        try
        {
            var result = await UniTask.RunOnThreadPool(() => ReadPcm16File(path));
            string sel = deviceSelector ?? "";
            if (_lastWindowsSelector != sel)
            {
                Init(sel);
                _lastWindowsSelector = sel;
            }
            await UniTask.SwitchToMainThread();
            WindowsWasapiExternalAudio.Play(key, result.Data, result.SampleRate, result.Channels, loop);
            if (volume >= 0f && !Mathf.Approximately(volume, 1f))
                WindowsWasapiExternalAudio.SetVoiceVolume(key, volume);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExternalDeviceAudio] PlayFromPcm16File 失败 key={key}: {e.Message}");
        }
    }

    static AudioClipPcmConverter.PcmResult ReadPcm16File(string path)
    {
        using var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        using var br = new System.IO.BinaryReader(fs);
        byte[] magic = br.ReadBytes(4);
        if (magic.Length != 4 || magic[0] != (byte)'P' || magic[1] != (byte)'C' ||
            magic[2] != (byte)'1' || magic[3] != (byte)'6')
            throw new System.IO.InvalidDataException("bad pcm16 magic");
        int version = br.ReadInt32();
        if (version != 1) throw new System.IO.InvalidDataException("bad pcm16 version");
        int sampleRate = br.ReadInt32();
        int channels = br.ReadInt32();
        int pcmLen = br.ReadInt32();
        byte[] pcm = br.ReadBytes(pcmLen);
        if (pcm.Length != pcmLen) throw new System.IO.InvalidDataException("truncated pcm16");
        return new AudioClipPcmConverter.PcmResult(pcm, sampleRate, channels);
    }
#endif

    public static void PlayFromClip(string deviceSelector, string key, AudioClip clip, bool loop = false,
                                    string pcmCachePath = null, bool usePcmCache = true, float volume = 1f)
    {
        PlayFromClipAsync(deviceSelector, key, clip, loop, pcmCachePath, usePcmCache, volume).Forget();
    }

    /// <summary>异步转 PCM 后播放；可 await。pcmCachePath 建议传本地文件绝对路径以启用磁盘缓存。</summary>
    public static async UniTask PlayFromClipAsync(string deviceSelector, string key, AudioClip clip, bool loop = false,
                                                  string pcmCachePath = null, bool usePcmCache = true, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogError("[ExternalDeviceAudio] PlayFromClip: clip null");
            return;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        await AlsaDevicePlayer.PlayFromClipAsync(deviceSelector, key, clip, loop, pcmCachePath, usePcmCache, volume);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string sel = deviceSelector ?? "";
        if (_lastWindowsSelector != sel)
        {
            Init(sel);
            _lastWindowsSelector = sel;
        }
        await WindowsWasapiExternalAudio.PlayFromClipAsync(key, clip, loop, pcmCachePath, usePcmCache);
        if (volume >= 0f && !Mathf.Approximately(volume, 1f))
            WindowsWasapiExternalAudio.SetVoiceVolume(key, volume);
#else
        Debug.Log($"[ExternalDeviceAudio] PlayFromClip stub {clip.name}");
        await UniTask.CompletedTask;
#endif
    }

    public static void PlayFromAsset(string deviceSelector, string key, string assetPath, bool loop = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.PlayFromAsset(deviceSelector, key, assetPath, loop);
#else
        Debug.LogWarning("[ExternalDeviceAudio] PlayFromAsset 当前仅在 Android 实机支持 APK assets。");
#endif
    }

    public static void Pause(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.Pause(key);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        WindowsWasapiExternalAudio.Pause(key);
#else
        Debug.Log($"[ExternalDeviceAudio] Pause stub key={key}");
#endif
    }

    public static void Resume(string deviceSelector, string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.Resume(deviceSelector, key);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string sel = deviceSelector ?? "";
        if (_lastWindowsSelector != sel)
        {
            Init(sel);
            _lastWindowsSelector = sel;
        }
        WindowsWasapiExternalAudio.Resume(key);
#else
        Debug.Log($"[ExternalDeviceAudio] Resume stub key={key}");
#endif
    }

    public static void Stop(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.Stop(key);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        WindowsWasapiExternalAudio.Stop(key);
#else
        Debug.Log($"[ExternalDeviceAudio] Stop stub key={key}");
#endif
    }

    public static void StopAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.StopAll();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        WindowsWasapiExternalAudio.StopAll();
#else
        Debug.Log("[ExternalDeviceAudio] StopAll stub");
#endif
    }

    /// <summary>
    /// 停播并释放底层播放引擎。与 <see cref="StopAll"/> 的区别是它连后台混音线程一起收掉，
    /// 供 Editor 退出 Play 模式时清场用——这条链路不经过 Unity 音频系统，Unity 不会替它收尾。
    /// 之后再调 <see cref="Play"/> 会自动重新初始化，不需要手动 <see cref="Init"/>。
    /// </summary>
    public static void Shutdown()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.StopAll();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        WindowsWasapiExternalAudio.Shutdown();
        _lastWindowsSelector = "\u0001";
#else
        Debug.Log("[ExternalDeviceAudio] Shutdown stub");
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor 退出 Play 模式时清掉外置音频。WASAPI 引擎跑在自己的后台线程上、状态全是静态字段，
    /// 不主动关的话战场 BGM 之类的循环音会一直播到下次进 Play（Init 时才顺带停掉旧引擎）。
    /// </summary>
    [UnityEditor.InitializeOnLoadMethod]
    static void HookEditorPlayModeExit()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnEditorPlayModeChanged;
    }

    static void OnEditorPlayModeChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;

        Shutdown();
    }
#endif

    public static bool IsPlaying(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.IsPlaying(key);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return WindowsWasapiExternalAudio.IsPlaying(key);
#else
        return false;
#endif
    }

    public static bool IsPaused(string key)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.IsPaused(key);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return WindowsWasapiExternalAudio.IsPaused(key);
#else
        return false;
#endif
    }

    public static bool IsNamedVoicePlaying(string name)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.IsNamedVoicePlaying(name);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return WindowsWasapiExternalAudio.IsNamedVoicePlaying(name);
#else
        return false;
#endif
    }

    public static void SetMasterVolume(float volume)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.SetMasterVolume(volume);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        WindowsWasapiExternalAudio.SetMasterVolume(volume);
#else
        Debug.Log($"[ExternalDeviceAudio] SetMasterVolume stub {volume}");
#endif
    }

    public static float GetMasterVolume()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.GetMasterVolume();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return WindowsWasapiExternalAudio.GetMasterVolume();
#else
        return 1f;
#endif
    }

    public static bool SetVoiceVolume(string key, float volume)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AlsaDevicePlayer.SetVoiceVolume(key, volume);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return WindowsWasapiExternalAudio.SetVoiceVolume(key, volume);
#else
        return false;
#endif
    }

    public static void ApplyCodecUnmute(int card)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AlsaDevicePlayer.ApplyCodecUnmute(card);
#else
        Debug.Log($"[ExternalDeviceAudio] ApplyCodecUnmute 仅 Android 板载 codec 需要，已忽略 card={card}");
#endif
    }

    public static byte[] AudioClipToPcm(AudioClip clip) => AlsaDevicePlayer.AudioClipToPcm(clip);

    public static byte[] SynthSine(float frequency = 440f, float durationSeconds = 1f,
        int sampleRate = 48000, int channels = 2, float amplitude = 0.5f) =>
        AlsaDevicePlayer.SynthSine(frequency, durationSeconds, sampleRate, channels, amplitude);
}
