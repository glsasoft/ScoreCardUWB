using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 科大讯飞 SparkChain <b>超拟人</b>语音合成（PersonateTTS）Unity 封装层。
///
/// 与普通在线 TTS（<see cref="XFTTSPlayer"/>，基于 Msc.jar）并存，区别：
///   • 引擎不同：本类基于 SparkChain SDK 的 <c>PersonateTTS</c>，发音更自然、可加入大模型口语化处理。
///   • 发音人：x4_lingxiaoxuan_oral / x4_lingfeiyi_oral / x5_lingyuyan_flow 等"x4_*_oral"系列。
///   • 鉴权：除 AppID 外，还需要 <see cref="ApiKey"/> 与 <see cref="ApiSecret"/>。
///
/// 依赖 Android 插件：
///   Assets/Plugins/Android/SparkChain.aar
///   Assets/Plugins/Android/Codec.aar
///   Assets/Plugins/Android/XFPersonateTTSBridge.java
///
/// 输出模式 <see cref="TtsOutputMode"/>：
///   • <see cref="TtsOutputMode.UnityAudio"/>   - Android AudioTrack 流式播放（默认声卡）
///   • <see cref="TtsOutputMode.ExternalAlsa"/> - 累积 PCM，结束后由 AlsaDevicePlayer 路由到指定声卡
///
/// 使用：
/// <example>
///   var p = XFPersonateTTSPlayer.Instance;
///   p.ApiKey    = "你的_apikey";
///   p.ApiSecret = "你的_apisecret";
///   p.SetVoiceName("x4_lingxiaoxuan_oral");
///   p.Speak("欢迎光临，超拟人语音合成测试");
/// </example>
/// </summary>
public class XFPersonateTTSPlayer : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────
    //  Inspector 参数
    // ────────────────────────────────────────────────────────────────────

    [Header("讯飞 SparkChain 鉴权（必须填写完整）")]
    [Tooltip("讯飞控制台的 AppID，与普通 TTS 共用 \"604ef9ce\"")]
    [SerializeField] private string _appId = "604ef9ce";

    [Tooltip("SparkChain 控制台分配的 ApiKey（必填）；与讯飞控制台该应用三元组一致，可在 Inspector 覆盖")]
    [SerializeField] private string _apiKey = "2ec192ae6400141c853ebdd6d02f8bac";

    [Tooltip("SparkChain 控制台分配的 ApiSecret（必填）；与讯飞控制台该应用三元组一致，可在 Inspector 覆盖")]
    [SerializeField] private string _apiSecret = "174ca26b2586f73330c26bd57aa665bd";

    [Header("超拟人 TTS 参数")]
    [Tooltip("发音人 vcn（仅可使用本账号已开通的，详见 Docs/语音合成SDK/语音合成音色列表_已开通.xlsx）。\n" +
             "默认 x6_lingxiaoxuan_pro(聆小璇,女)；其他常用：x6_lingfeiyi_pro(聆飞逸,男) / x6_lingyuyan_pro(聆玉言,女) / x5_lingyuzhao_flow(聆玉昭,女)")]
    [SerializeField] private string _voiceName = "x6_lingxiaoxuan_pro";

    [Tooltip("语速 0~100，默认 50")]
    [SerializeField, Range(0, 100)] private int _speed = 50;

    [Tooltip("语调 0~100，默认 50")]
    [SerializeField, Range(0, 100)] private int _pitch = 50;

    [Tooltip("音量 0~100，默认 50")]
    [SerializeField, Range(0, 100)] private int _volume = 50;

    [Tooltip("口语化等级：high / mid / low")]
    [SerializeField] private string _oralLevel = "mid";

    [Tooltip("是否启用大模型口语化（关闭则使用纯朗读模式）")]
    [SerializeField] private bool _sparkAssist = true;

    [Header("输出模式")]
    [Tooltip("UnityAudio=走默认声卡（电视）；ExternalAlsa=走指定 ALSA 声卡（战场音效）")]
    [SerializeField] private TtsOutputMode _outputMode = TtsOutputMode.UnityAudio;

    [Tooltip("ExternalAlsa 模式：ALSA 声卡过滤关键词，例如 \"hdmi0\"")]
    [SerializeField] private string _externalDeviceSelector = "";

    [Tooltip("ExternalAlsa 模式：播放 key（相同 key 会替换旧实例，建议固定为 \"tts_personate\"）")]
    [SerializeField] private string _externalAudioKey = "tts_personate";

    [Tooltip("ExternalAlsa 模式：PCM 采样率（SparkChain 默认 16000）")]
    [SerializeField] private int _externalSampleRate = 16000;

    [Tooltip("ExternalAlsa 模式：PCM 声道数（SparkChain 默认单声道 1）")]
    [SerializeField, Range(1, 2)] private int _externalChannels = 1;

    // ────────────────────────────────────────────────────────────────────
    //  单例
    // ────────────────────────────────────────────────────────────────────

    private static XFPersonateTTSPlayer _instance;

    /// <summary>自动单例。首次访问时若场景中不存在，会自动创建一个 GameObject。</summary>
    public static XFPersonateTTSPlayer Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[XFPersonateTTSPlayer]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<XFPersonateTTSPlayer>();
            return _instance;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开属性
    // ────────────────────────────────────────────────────────────────────

    public TtsOutputMode OutputMode
    {
        get => _outputMode;
        set => _outputMode = value;
    }

    public string ExternalDeviceSelector
    {
        get => _externalDeviceSelector;
        set => _externalDeviceSelector = value;
    }

    /// <summary>
    /// SparkChain 鉴权 ApiKey。建议在 Inspector 配置；运行时也可通过赋值后调用 <see cref="Init"/> 重新初始化。
    /// </summary>
    public string ApiKey
    {
        get => _apiKey;
        set => _apiKey = value;
    }

    /// <summary>SparkChain 鉴权 ApiSecret。</summary>
    public string ApiSecret
    {
        get => _apiSecret;
        set => _apiSecret = value;
    }

    /// <summary>讯飞控制台 AppID。</summary>
    public string AppId
    {
        get => _appId;
        set => _appId = value;
    }

    public bool IsSpeaking { get; private set; }

    // ────────────────────────────────────────────────────────────────────
    //  公开事件
    // ────────────────────────────────────────────────────────────────────

    public event Action OnSpeakBegin;
    public event Action<int> OnProgress;
    public event Action OnCompleted;
    public event Action<string> OnError;

    // ────────────────────────────────────────────────────────────────────
    //  状态
    // ────────────────────────────────────────────────────────────────────

    private bool _initialized;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string JavaBridgeClass = "com.glsa.mqttscorecard5s.unity.XFPersonateTTSBridge";
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
        Init();
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
            Debug.LogWarning($"[XFPersonateTTSPlayer] OnDestroy: {ex.Message}");
        }
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开方法
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 初始化 SparkChain SDK。重复调用安全（首次成功后跳过 SDK 真正初始化）。
    /// 若运行时修改了 <see cref="ApiKey"/> / <see cref="ApiSecret"/>，可再次调用以让 Java 层应用新值。
    /// </summary>
    public void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var jc       = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("init",
                activity, gameObject.name,
                _appId, _apiKey ?? "", _apiSecret ?? "",
                _voiceName, _speed, _pitch, _volume);
            int code = jc.CallStatic<int>("getInitCode");
            _initialized = jc.CallStatic<bool>("isInited");
            if (_initialized)
                Debug.Log($"[XFPersonateTTSPlayer] Init ok: vcn={_voiceName} speed={_speed} pitch={_pitch} vol={_volume}");
            else
                Debug.LogError($"[XFPersonateTTSPlayer] SparkChain 初始化失败，code={code}（请检查 ApiKey/ApiSecret/网络/授权）");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFPersonateTTSPlayer] Init failed: {ex}");
        }
#else
        _initialized = true;
        Debug.Log($"[XFPersonateTTSPlayer] Init (Editor mock): vcn={_voiceName}");
#endif
    }

    /// <summary>合成并播报一段文本。会自动停止上一段播报。</summary>
    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[XFPersonateTTSPlayer] Speak: text 为空");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!_initialized)
        {
            Debug.LogWarning("[XFPersonateTTSPlayer] Speak: 尚未初始化或鉴权失败，尝试重新 Init");
            Init();
            if (!_initialized) return;
        }
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            if (_outputMode == TtsOutputMode.ExternalAlsa)
            {
                Debug.Log($"[XFPersonateTTSPlayer] Speak(ExternalAlsa): {text} → {_externalDeviceSelector}");
                jc.CallStatic("speakToExternal", text);
            }
            else
            {
                Debug.Log($"[XFPersonateTTSPlayer] Speak(UnityAudio): {text}");
                jc.CallStatic("speak", text);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFPersonateTTSPlayer] Speak failed: {ex}");
        }
#else
        Debug.Log($"[XFPersonateTTSPlayer] Speak (Editor mock, mode={_outputMode}): {text}");
        OnSpeakBegin?.Invoke();
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
        Debug.Log("[XFPersonateTTSPlayer] Editor mock: ExternalAlsa PCM route simulated");
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
            jc.CallStatic("stop");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[XFPersonateTTSPlayer] Stop failed: {ex}");
        }
#else
        Debug.Log("[XFPersonateTTSPlayer] Stop (Editor mock)");
#endif
    }

    /// <summary>动态修改发音人（下次 speak 生效）。</summary>
    public void SetVoiceName(string voiceName)
    {
        _voiceName = voiceName;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setVoiceName", voiceName);
        }
        catch (Exception ex) { Debug.LogError($"[XFPersonateTTSPlayer] SetVoiceName failed: {ex}"); }
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
        catch (Exception ex) { Debug.LogError($"[XFPersonateTTSPlayer] SetSpeed failed: {ex}"); }
#endif
    }

    /// <summary>动态修改语调（0~100）。</summary>
    public void SetPitch(int pitch)
    {
        _pitch = Mathf.Clamp(pitch, 0, 100);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setPitch", _pitch);
        }
        catch (Exception ex) { Debug.LogError($"[XFPersonateTTSPlayer] SetPitch failed: {ex}"); }
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
        catch (Exception ex) { Debug.LogError($"[XFPersonateTTSPlayer] SetVolume failed: {ex}"); }
#endif
    }

    /// <summary>动态设置口语化等级：high / mid / low。</summary>
    public void SetOralLevel(string level)
    {
        if (string.IsNullOrEmpty(level)) return;
        _oralLevel = level;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setOralLevel", level);
        }
        catch (Exception ex) { Debug.LogError($"[XFPersonateTTSPlayer] SetOralLevel failed: {ex}"); }
#endif
    }

    /// <summary>是否启用大模型口语化处理。</summary>
    public void SetSparkAssist(bool enable)
    {
        _sparkAssist = enable;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var jc = new AndroidJavaClass(JavaBridgeClass);
            jc.CallStatic("setSparkAssist", enable);
        }
        catch (Exception ex) { Debug.LogError($"[XFPersonateTTSPlayer] SetSparkAssist failed: {ex}"); }
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  UnitySendMessage 回调（由 XFPersonateTTSBridge.java 调用）
    //  方法名必须与 Java 端 sendToUnityPersonate 完全一致
    // ────────────────────────────────────────────────────────────────────

    // ReSharper disable UnusedMember.Local — 由 UnitySendMessage 反射调用

    private void OnPersonateTTSSpeakBegin(string msg)
    {
        IsSpeaking = true;
        Debug.Log("[XFPersonateTTSPlayer] 开始播报");
        OnSpeakBegin?.Invoke();
    }

    private void OnPersonateTTSProgress(string msg)
    {
        if (int.TryParse(msg, out int percent))
            OnProgress?.Invoke(percent);
    }

    private void OnPersonateTTSCompleted(string msg)
    {
        IsSpeaking = false;
        Debug.Log("[XFPersonateTTSPlayer] 播报完成");
        OnCompleted?.Invoke();
    }

    private void OnPersonateTTSError(string msg)
    {
        IsSpeaking = false;
        Debug.LogError($"[XFPersonateTTSPlayer] 播报错误: {msg}");
        OnError?.Invoke(msg);
    }

    /// <summary>
    /// ExternalAlsa 模式专用回调：Java 合成完毕后携带 Base64 PCM 数据调用此方法。
    /// 解码后交给 <see cref="ExternalDeviceAudioPlayer"/> 路由到指定声卡。
    /// </summary>
    private void OnPersonateTTSPcmReady(string base64Pcm)
    {
        IsSpeaking = false;

        if (string.IsNullOrEmpty(base64Pcm))
        {
            Debug.LogWarning("[XFPersonateTTSPlayer] OnPersonateTTSPcmReady: 收到空 PCM 数据");
            OnError?.Invoke("pcm_empty");
            return;
        }

        try
        {
            byte[] pcm = Convert.FromBase64String(base64Pcm);
            Debug.Log($"[XFPersonateTTSPlayer] PCM ready: {pcm.Length} bytes → " +
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
            Debug.LogError($"[XFPersonateTTSPlayer] OnPersonateTTSPcmReady 解码失败: {ex.Message}");
            OnError?.Invoke($"pcm_decode_failed:{ex.Message}");
        }
    }
}
