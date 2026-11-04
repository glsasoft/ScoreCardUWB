using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// TTS 音频输出目标。
/// </summary>
public enum TtsOutputMode
{
    /// <summary>
    /// Unity 默认音频输出（当前系统声卡 → 电视/显示器）。
    /// 使用 Android AudioTrack 播放，即 <c>XFTTSBridge.speak()</c>。
    /// 适合比分牌播报、提示音等需要走同一路音频的场景。
    /// </summary>
    UnityAudio,

    /// <summary>
    /// 外置 ALSA 指定声卡输出（战场音效声卡）。
    /// 先合成 PCM 文件（<c>XFTTSBridge.speakToExternal()</c>），
    /// 完成后由 Unity 侧通过 <c>AlsaDevicePlayer</c> 路由到指定声卡。
    /// 适合将 TTS 语音混入战场背景音乐声卡的场景。
    /// </summary>
    ExternalAlsa,
}

/// <summary>
/// 科大讯飞语音合成（TTS）Unity 封装层。
///
/// 依赖 Android 插件：
///   Assets/Plugins/Android/Msc.jar
///   Assets/Plugins/Android/arm64-v8a/libmsc.so
///   Assets/Plugins/Android/armeabi-v7a/libmsc.so
///   Assets/Plugins/Android/XFTTSBridge.java
///
/// 支持两种输出模式（<see cref="TtsOutputMode"/>）：
///   • <see cref="TtsOutputMode.UnityAudio"/>   - Unity 默认声卡（电视），走 Android AudioTrack
///   • <see cref="TtsOutputMode.ExternalAlsa"/> - 指定 ALSA 声卡（战场音效），先合成 PCM 再播
///
/// 使用方式：
///   1. 在场景中创建一个 GameObject，挂载本组件（或使用 <see cref="Instance"/>）。
///   2. 调用 <see cref="Speak"/> 传入文字即可。
///   3. 订阅 <see cref="OnCompleted"/> / <see cref="OnError"/> 等事件获取回调。
///
/// <example>
///   // 模式一：走 Unity 默认声卡播报
///   XFTTSPlayer.Instance.Speak("欢迎光临");
///
///   // 模式二：播到战场音效声卡
///   XFTTSPlayer.Instance.OutputMode = TtsOutputMode.ExternalAlsa;
///   XFTTSPlayer.Instance.ExternalDeviceSelector = "hdmi0";
///   XFTTSPlayer.Instance.Speak("战斗开始");
/// </example>
/// </summary>
public class XFTTSPlayer : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────
    //  Inspector 参数
    // ────────────────────────────────────────────────────────────────────

    [Header("讯飞 TTS 参数")]
    [Tooltip("appid 已在 PiuPiuApplication.java 中硬编码，此处仅供查阅")]
    [SerializeField] private string _appId = "604ef9ce";

    [Tooltip("发音人：xiaoyan(小燕,女)/aisjinger(小晶,女)/aisbabyxu(许久,男)/aisjiuxu(许逸,男)...")]
    [SerializeField] private string _voiceName = "xiaoyan";

    [Tooltip("语速 0~100")]
    [SerializeField, Range(0, 100)] private int _speed = 60;

    [Tooltip("音量 0~100")]
    [SerializeField, Range(0, 100)] private int _volume = 100;

    [Header("输出模式")]
    [Tooltip("UnityAudio=走 Unity 默认声卡（电视）；ExternalAlsa=走指定 ALSA 声卡（战场音效）")]
    [SerializeField] private TtsOutputMode _outputMode = TtsOutputMode.UnityAudio;

    [Tooltip("ExternalAlsa 模式：ALSA 声卡过滤关键词，例如 \"hdmi0\"（与 AlsaDevicePlayer.Init 传入的参数一致）")]
    [SerializeField] private string _externalDeviceSelector = "";

    [Tooltip("ExternalAlsa 模式：播放时的 key（相同 key 会替换旧实例，建议固定为 \"tts_voice\"）")]
    [SerializeField] private string _externalAudioKey = "tts_voice";

    [Tooltip("ExternalAlsa 模式：PCM 采样率（iFlytek 云端 TTS 默认 16000）")]
    [SerializeField] private int _externalSampleRate = 16000;

    [Tooltip("ExternalAlsa 模式：PCM 声道数（iFlytek 云端 TTS 默认单声道 1）")]
    [SerializeField, Range(1, 2)] private int _externalChannels = 1;

    // ────────────────────────────────────────────────────────────────────
    //  单例
    // ────────────────────────────────────────────────────────────────────

    private static XFTTSPlayer _instance;

    /// <summary>
    /// 自动创建单例。首次访问时若场景中不存在，会自动生成一个 GameObject。
    /// </summary>
    public static XFTTSPlayer Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[XFTTSPlayer]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<XFTTSPlayer>();
            return _instance;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  运行时可动态切换的输出模式属性
    // ────────────────────────────────────────────────────────────────────

    /// <summary>当前 TTS 输出模式，可在运行时动态切换。</summary>
    public TtsOutputMode OutputMode
    {
        get => _outputMode;
        set => _outputMode = value;
    }

    /// <summary>
    /// ExternalAlsa 模式下的 ALSA 声卡过滤关键词（如 "hdmi0"）。
    /// 对应 <see cref="AlsaDevicePlayer.Play"/> 的 filterName 参数。
    /// </summary>
    public string ExternalDeviceSelector
    {
        get => _externalDeviceSelector;
        set => _externalDeviceSelector = value;
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开事件
    // ────────────────────────────────────────────────────────────────────

    /// <summary>开始播报时触发（Java 回调 onSpeakBegin）。</summary>
    public event Action OnSpeakBegin;

    /// <summary>播报进度更新（0~100），由 Java 回调 onSpeakProgress 触发。</summary>
    public event Action<int> OnProgress;

    /// <summary>播报正常完成时触发。</summary>
    public event Action OnCompleted;

    /// <summary>发生错误时触发，参数为错误描述字符串。</summary>
    public event Action<string> OnError;

    // ────────────────────────────────────────────────────────────────────
    //  状态
    // ────────────────────────────────────────────────────────────────────

    /// <summary>当前是否正在播报。</summary>
    public bool IsSpeaking { get; private set; }

    private bool _initialized;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string JavaBridgeClass = "com.glsa.mqttscorecard5s.unity.XFTTSBridge";
#endif

    // ────────────────────────────────────────────────────────────────────
    //  生命周期
    // ────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 在 Awake 同步初始化，确保 Speak() 在同帧内调用时 Init 已派发到 UI 线程队列
        // UI 线程会先处理 createSynthesizer，再处理 startSpeaking（队列有序），不存在竞态
        Init();
    }

    private void Start()
    {
        // Init 已在 Awake 中完成（无论是场景预置还是 Instance 自动创建），此处无需重复
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("destroy");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[XFTTSPlayer] OnDestroy: {ex.Message}");
        }
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开方法
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 初始化 TTS 引擎（Start 中自动调用，一般不需要手动调用）。
    /// 若需要更换发音人/语速/音量后重新初始化，可再次调用。
    /// </summary>
    public void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("init", activity, gameObject.name, _voiceName, _speed, _volume);
            _initialized = true;
            Debug.Log($"[XFTTSPlayer] Init ok: voice={_voiceName} speed={_speed} vol={_volume}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] Init failed: {ex}");
        }
#else
        _initialized = true;
        Debug.Log($"[XFTTSPlayer] Init (Editor mock): voice={_voiceName} speed={_speed} vol={_volume}");
#endif
    }

    /// <summary>
    /// 合成并播报文本。若正在播报，会先停止上一段。
    /// 根据 <see cref="OutputMode"/> 自动选择输出路径：
    ///   • <see cref="TtsOutputMode.UnityAudio"/>   → Android AudioTrack（Unity 默认声卡）
    ///   • <see cref="TtsOutputMode.ExternalAlsa"/> → 先合成 PCM，完成后送入 AlsaDevicePlayer
    /// </summary>
    /// <param name="text">要播报的文字</param>
    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[XFTTSPlayer] Speak: text is empty");
            return;
        }
        if (!_initialized)
        {
            Debug.LogWarning("[XFTTSPlayer] Speak: 尚未初始化，请稍后重试");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            if (_outputMode == TtsOutputMode.ExternalAlsa)
            {
                Debug.Log($"[XFTTSPlayer] Speak(ExternalAlsa): {text} → {_externalDeviceSelector}");
                jc.CallStatic("speakToExternal", text);
            }
            else
            {
                Debug.Log($"[XFTTSPlayer] Speak(UnityAudio): {text}");
                jc.CallStatic("speak", text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] Speak failed: {ex}");
        }
#else
        Debug.Log($"[XFTTSPlayer] Speak (Editor mock, mode={_outputMode}): {text}");
        OnSpeakBegin?.Invoke();
        // Editor 模式下模拟 ExternalAlsa：生成静音 PCM 并走相同路径
        if (_outputMode == TtsOutputMode.ExternalAlsa)
            StartCoroutine(MockExternalAlsa());
        else
            OnCompleted?.Invoke();
#endif
    }

#if UNITY_EDITOR
    private IEnumerator MockExternalAlsa()
    {
        yield return new WaitForSeconds(0.1f);
        // 模拟 300ms 延迟后 PCM 就绪
        Debug.Log("[XFTTSPlayer] Editor mock: ExternalAlsa PCM route simulated");
        OnCompleted?.Invoke();
    }
#endif

    /// <summary>停止当前播报。</summary>
    public void Stop()
    {
        IsSpeaking = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("stopSpeaking");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] Stop failed: {ex}");
        }
#else
        Debug.Log("[XFTTSPlayer] Stop (Editor mock)");
#endif
    }

    /// <summary>动态修改发音人（无需重新 Init）。</summary>
    public void SetVoiceName(string voiceName)
    {
        _voiceName = voiceName;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setVoiceName", voiceName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] SetVoiceName failed: {ex}");
        }
#endif
    }

    /// <summary>动态修改语速（0~100）。</summary>
    public void SetSpeed(int speed)
    {
        _speed = Mathf.Clamp(speed, 0, 100);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setSpeed", _speed);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] SetSpeed failed: {ex}");
        }
#endif
    }

    /// <summary>动态修改音量（0~100）。</summary>
    public void SetVolume(int volume)
    {
        _volume = Mathf.Clamp(volume, 0, 100);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setVolume", _volume);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] SetVolume failed: {ex}");
        }
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  UnitySendMessage 回调（由 XFTTSBridge.java 调用）
    //  方法名必须与 Java 侧 sendToUnity 的 method 参数完全一致
    // ────────────────────────────────────────────────────────────────────

    // ReSharper disable UnusedMember.Local — 由 UnitySendMessage 反射调用

    private void OnTTSSpeakBegin(string msg)
    {
        IsSpeaking = true;
        Debug.Log("[XFTTSPlayer] 开始播报");
        OnSpeakBegin?.Invoke();
    }

    private void OnTTSProgress(string msg)
    {
        if (int.TryParse(msg, out int percent))
            OnProgress?.Invoke(percent);
    }

    private void OnTTSCompleted(string msg)
    {
        IsSpeaking = false;
        Debug.Log("[XFTTSPlayer] 播报完成");
        OnCompleted?.Invoke();
    }

    private void OnTTSError(string msg)
    {
        IsSpeaking = false;
        Debug.LogError($"[XFTTSPlayer] 播报错误: {msg}");
        OnError?.Invoke(msg);
    }

    /// <summary>
    /// ExternalAlsa 模式专用回调：Java 合成完毕后携带 Base64 PCM 数据调用此方法。
    /// 解码后交给 <see cref="AlsaDevicePlayer"/> 播放到指定声卡。
    /// </summary>
    private void OnTTSPcmReady(string base64Pcm)
    {
        IsSpeaking = false;

        if (string.IsNullOrEmpty(base64Pcm))
        {
            Debug.LogWarning("[XFTTSPlayer] OnTTSPcmReady: 收到空 PCM 数据");
            OnError?.Invoke("pcm_empty");
            return;
        }

        try
        {
            byte[] pcm = Convert.FromBase64String(base64Pcm);
            Debug.Log($"[XFTTSPlayer] PCM ready: {pcm.Length} bytes → " +
                      $"{_externalDeviceSelector} key={_externalAudioKey} " +
                      $"{_externalSampleRate}Hz ch={_externalChannels}");

            ExternalDeviceAudioPlayer.Play(
                _externalDeviceSelector,
                _externalAudioKey,
                pcm,
                _externalSampleRate,
                _externalChannels);

            OnCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFTTSPlayer] OnTTSPcmReady 解码失败: {ex.Message}");
            OnError?.Invoke($"pcm_decode_failed:{ex.Message}");
        }
    }
}
