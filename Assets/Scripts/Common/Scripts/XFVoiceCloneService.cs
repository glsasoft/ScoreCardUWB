using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 科大讯飞「一句话复刻」服务（训练 + 合成），与 <see cref="NoizTTSService"/> 完全独立。
///
/// 参考：
///   一句话复刻（标准版）API 文档 https://www.xfyun.cn/doc/spark/reproduction.html
///   官方示例 https://github.com/iflytek/iFly-Skills/tree/main/ifly-voiceclone-tts
///
/// 两条主流程：
///   1) 训练（HTTP，MD5 鉴权）：
///        获取 token → /task/add 创建任务 → /task/submitWithAudio 上传本地音频并提交
///        → 轮询 /task/result 直到 trainStatus==1，拿到音色 id（assetId / res_id）。
///   2) 合成（WebSocket，HMAC-SHA256 URL 鉴权）：
///        用 res_id 连接 /v1/private/voice_clone，发送文本，接收 base64 音频流。
///        本实现请求 encoding=raw（PCM 16bit/24k 单声道），直接构建 AudioClip 播放，
///        免去 mp3 解码依赖。
///
/// 说明：训练属于一次性的开发期操作，建议在 Unity Editor 中通过 UIDEV 面板触发。
/// 训练得到的 res_id 会写入 PlayerPrefs 与 Docs/VoiceClone_ResId.txt，供合成测试复用。
/// </summary>
public class XFVoiceCloneService : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────
    //  接口地址
    // ────────────────────────────────────────────────────────────────────

    private const string AUTH_TOKEN_URL = "http://avatar-hci.xfyousheng.com/aiauth/v1/token";
    private const string TRAIN_BASE     = "http://opentrain.xfyousheng.com/voice_train";
    private const string TTS_WS_URL     = "wss://cn-huabei-1.xf-yun.com/v1/private/voice_clone";

    // 槽位 0 = A(原音色)：沿用旧 key，保证已训练音色无缝保留
    private const string PREF_RESID  = "XF_VC_RESID";
    private const string PREF_TASKID = "XF_VC_TASKID";
    // 槽位 1 = B(新音色)
    private const string PREF_RESID_B   = "XF_VC_RESID_B";
    private const string PREF_TASKID_B  = "XF_VC_TASKID_B";
    private const string PREF_ACTIVE_SLOT = "XF_VC_ACTIVE_SLOT";

    // 已训练音色的固化兜底：打包安卓/全新安装 PlayerPrefs 为空时直接可用，无需重训。
    // 数值来自 Docs/VoiceClone_ResId.txt(A) 与 Docs/VoiceClone_ResId_B.txt(B)。
    private const string DEFAULT_RESID_A = "4504f63_ttsclone-604ef9ce-lbxhz";
    private const string DEFAULT_RESID_B = "fc94ddc_ttsclone-604ef9ce-jskhu";
    private static string DefaultResId(int slot) => slot == 0 ? DEFAULT_RESID_A : DEFAULT_RESID_B;

    // ────────────────────────────────────────────────────────────────────
    //  Inspector 鉴权参数（默认复用项目内讯飞应用三元组，可在 Inspector 覆盖）
    // ────────────────────────────────────────────────────────────────────

    [Header("讯飞鉴权（与超拟人 TTS 共用同一应用三元组）")]
    [SerializeField] private string _appId     = "604ef9ce";
    [SerializeField] private string _apiKey    = "2ec192ae6400141c853ebdd6d02f8bac";
    [SerializeField] private string _apiSecret = "174ca26b2586f73330c26bd57aa665bd";

    [Header("版本：多风格版=true(x6_clone, engineVersion=omni_v1)；标准版=false(x5_clone)")]
    [SerializeField] private bool _useMultiStyle = true;

    [Header("多风格版合成参数（仅多风格版生效）")]
    [Tooltip("合成语种：0中 1英 2日 3韩 4俄 5法 6阿 7西 8粤")]
    [SerializeField] private int    _languageId   = 0;
    [Tooltip("方言/风格/情感，仅中文(languageID=0)生效，空=默认。如 sichuan/news/happy/sunwukong")]
    [SerializeField] private string _style        = "";
    [Tooltip("原声影响因子 0~10，越大越像原声；-1 按 style 自动")]
    [SerializeField] private int    _impactFactor = -1;

    [Header("轮询训练结果配置")]
    [Tooltip("轮询间隔（秒）")]
    [SerializeField] private float _pollInterval = 10f;
    [Tooltip("最大轮询次数（超时 = pollInterval × maxPollCount）默认 10s×720 = 2小时")]
    [SerializeField] private int _maxPollCount = 720;

    // ────────────────────────────────────────────────────────────────────
    //  单例
    // ────────────────────────────────────────────────────────────────────

    private static XFVoiceCloneService _instance;

    public static XFVoiceCloneService Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[XFVoiceCloneService]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<XFVoiceCloneService>();
            return _instance;
        }
    }

    /// <summary>诊断代码用：查询单例是否已存在，避免采样时意外触发首次创建。</summary>
    public static bool HasInstance => _instance != null;

    /// <summary>诊断用：返回内存合成缓存的条目数与总字节数，供 MemoryDiagnosticsLogger 采样，不影响正常逻辑。</summary>
    public (int count, long bytes) GetCacheDebugStats()
    {
        lock (_cache)
        {
            long bytes = 0;
            foreach (var pcm in _cache.Values) bytes += pcm?.Length ?? 0;
            return (_cache.Count, bytes);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开属性 / 事件
    // ────────────────────────────────────────────────────────────────────

    public event Action<string> OnStatusChanged;

    private string Vcn => _useMultiStyle ? "x6_clone" : "x5_clone";

    /// <summary>当前是否多风格版（x6_clone, engineVersion=omni_v1）。</summary>
    public bool IsMultiStyle => _useMultiStyle;

    /// <summary>当前版本名称（用于 UI 展示）。</summary>
    public string VersionName => _useMultiStyle ? "多风格版" : "标准版";

    /// <summary>多风格版合成语种：0中 1英 2日 3韩 4俄 5法 6阿 7西 8粤。</summary>
    public int LanguageId { get => _languageId; set => _languageId = value; }

    /// <summary>多风格版方言/风格/情感（仅中文生效），空=默认。</summary>
    public string Style { get => _style; set => _style = value ?? ""; }

    /// <summary>多风格版原声影响因子 0~10，-1 按 style 自动。</summary>
    public int ImpactFactor { get => _impactFactor; set => _impactFactor = value; }

    /// <summary>音色槽位数量：0=A(原) 1=B(新)。</summary>
    public const int VoiceSlotCount = 2;

    private static string ResIdPrefKey(int slot)  => slot == 0 ? PREF_RESID  : PREF_RESID_B;
    private static string TaskIdPrefKey(int slot) => slot == 0 ? PREF_TASKID : PREF_TASKID_B;

    /// <summary>槽位名称（UI 展示）。</summary>
    public static string SlotName(int slot) => slot == 0 ? "A(原)" : "B(新)";

    /// <summary>当前用于合成/测试的音色槽位（0=A原 1=B新），持久化。切换后合成走对应音色。</summary>
    public int ActiveVoiceSlot
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PREF_ACTIVE_SLOT, 0), 0, VoiceSlotCount - 1);
        set
        {
            int v = Mathf.Clamp(value, 0, VoiceSlotCount - 1);
            PlayerPrefs.SetInt(PREF_ACTIVE_SLOT, v);
            PlayerPrefs.Save();
        }
    }

    /// <summary>读取指定槽位的音色 id（res_id / assetId）；PlayerPrefs 为空时回退固化默认值。</summary>
    public string GetResId(int slot)
    {
        var v = PlayerPrefs.GetString(ResIdPrefKey(slot), "");
        return string.IsNullOrEmpty(v) ? DefaultResId(slot) : v;
    }
    /// <summary>读取指定槽位最近一次训练任务 id。</summary>
    public string GetTaskId(int slot) => PlayerPrefs.GetString(TaskIdPrefKey(slot), "");

    /// <summary>当前活动槽位的音色 id（res_id / assetId），合成时使用，持久化在 PlayerPrefs。</summary>
    public string ResId
    {
        get => GetResId(ActiveVoiceSlot);
        set { PlayerPrefs.SetString(ResIdPrefKey(ActiveVoiceSlot), value ?? ""); PlayerPrefs.Save(); }
    }

    /// <summary>当前活动槽位最近一次创建的训练任务 id（持久化），用于训练超时后手动查询结果。</summary>
    public string LastTaskId
    {
        get => PlayerPrefs.GetString(TaskIdPrefKey(ActiveVoiceSlot), "");
        set { PlayerPrefs.SetString(TaskIdPrefKey(ActiveVoiceSlot), value ?? ""); PlayerPrefs.Save(); }
    }

    public bool IsTraining { get; private set; }
    public bool IsSpeaking => _speakState != SpeakState.Idle;

    /// <summary>
    /// 是否裁剪合成音频尾部静音（默认 false）。开启后播放读到有效语音末尾即判定结束，
    /// 避免多风格合成尾部不定长静音造成"话说完后延迟很久"。用于需要顺序拼接的短播报。
    /// </summary>
    public bool TrimTrailingSilence { get; set; }

    // ────────────────────────────────────────────────────────────────────
    //  内部状态
    // ────────────────────────────────────────────────────────────────────

    private AudioSource _audioSource;
    private string      _token;

    // ── 合成/播放状态（流式边收边播）─────────────────────────────────
    private enum SpeakState { Idle, Synthesizing, Playing }
    private SpeakState _speakState = SpeakState.Idle;

    private int _synthSampleRate = 24000;

    /// <summary>
    /// 导出 WAV 的目标采样率。
    /// 16kHz/mono/16bit 口播够用，体积约为 24kHz 的 2/3；配合尾部静音裁剪可进一步减小。
    /// </summary>
    public const int EmbeddedExportSampleRate = 16000;

    /// <summary>导出 WAV 末尾淡出时长（毫秒），覆盖语音自然衰减段，避免「哒」声。</summary>
    public const int EmbeddedExportFadeOutMs = 55;

    /// <summary>最后一个有效样本后再保留的尾音（毫秒），避免裁在衰减中间。</summary>
    public const int EmbeddedExportReleaseMs = 35;

    /// <summary>淡出后至少保留的尾部静音（毫秒）；过短易在播放结束产生爆破音。</summary>
    public const int EmbeddedExportMinTrailingSilenceMs = 500;

    /// <summary>尾部静音上限（毫秒）；API 常带数秒静音，导出时截断以控制体积。</summary>
    public const int EmbeddedExportMaxTrailingSilenceMs = 1500;

    // 线程安全 PCM 流缓冲：worker 线程写入，音频线程读取
    private readonly PcmStreamBuffer _pcm = new();

    // worker → 主线程 交接（在 _stateLock 下访问）
    private readonly object _stateLock = new();
    private string _workerError;
    private string _workerSid;
    private bool   _workerFirstFrame;   // 已收到首帧音频
    private bool   _workerDone;          // worker 已结束（收完/出错/取消）
    private int    _workerSampleRate = 24000;
    private int    _workerPcmBytes;
    private int    _workerGeneration;    // 与 _speakGeneration 对齐，用于忽略过期 worker 回写
    private double _firstFrameSec;
    private double _synthSec;

    // 回调
    private Action         _onComplete;
    private Action<string> _onError;
    private Action<float>  _onFirstAudio;   // 首帧就绪（首帧耗时秒）

    // 播放控制
    private AudioClip _streamClip;
    private int       _streamClipRate;       // 常驻流式 clip 的采样率（复用判定）
    private bool      _playbackStarted;
    private bool      _completeFired;
    private float     _drainedAt = -1f;
    private CancellationTokenSource _synthCts;
    private int       _speakGeneration;   // 每次 Speak/StopSpeak 自增，worker 回写须代际匹配
    private int       _speakRdn;          // 当前 Speak 的 rdn 参数（合成请求与缓存 key 共用）

    // 当前 Speak 会话日志上下文（主线程写入，便于 PLAY_START/END 关联）
    int    _logGen;
    string _logText;
    float  _logSpeakWallStart;
    float  _logPlayWallStart;
    string _logSid;

    [Header("流式播放")]
    [Tooltip("开播前预缓冲时长(ms)，跨过服务端推流启动间隙避免开头卡顿；短词/缓存命中会立即播放。网络差可调大(如1200)")]
    [SerializeField] private int _prebufferMs = 800;
    [Tooltip("PCM 读完后额外等待(s)，让 Unity AudioSource/DSP 缓冲播完末字再 Stop，避免「战士166」末位被切")]
    [SerializeField] private float _playbackTailPadSec = 0.08f;
    const float DrainedConfirmSec = 0.04f;

    // 最近一次合成指标（主线程更新，供 UI/日志读取）
    public float LastFirstFrameSec { get; private set; }
    public float LastSynthSec      { get; private set; }
    public int   LastPcmBytes      { get; private set; }
    public bool  LastFromCache     { get; private set; }

    // 最近一次合成/缓存命中的完整 PCM（16bit LE），供导出音频文件用
    private byte[] _lastFullPcm;

    /// <summary>最近一次合成/缓存命中的完整 PCM(16bit LE，单声道)，供外部按需复制播放到其它音频通道。</summary>
    public byte[] LastFullPcm => _lastFullPcm;

    /// <summary>合成采样率（Hz），配合 <see cref="LastFullPcm"/> 使用。</summary>
    public int SynthSampleRate => _synthSampleRate;

    // ── 合成结果缓存（相同文本命中直接播放，适合高频短词）──────────────
    [Header("合成缓存")]
    [SerializeField] private bool _enableCache   = true;
    [Tooltip("最多缓存多少条合成结果（LRU 淘汰）")]
    [SerializeField] private int  _cacheCapacity = 256;
    [Header("磁盘缓存")]
    [SerializeField] private bool _enableDiskCache = true;
    [Tooltip("磁盘缓存条数上限")]
    [SerializeField] private int  _diskCacheMaxEntries = VoiceCloneDiskCache.DefaultMaxEntries;
    private readonly Dictionary<string, byte[]> _cache    = new();
    private readonly LinkedList<string>         _cacheLru = new();
    VoiceCloneDiskCache _diskCache;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        if (_enableDiskCache)
        {
            _diskCache = new VoiceCloneDiskCache();
            _diskCache.Initialize(_diskCacheMaxEntries);
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) _diskCache?.FlushIndex();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        _diskCache?.FlushIndex();
        try { _synthCts?.Cancel(); } catch { }
        if (_streamClip != null) { Destroy(_streamClip); _streamClip = null; }
    }

    private void SetStatus(string s)
    {
        Debug.Log($"[VoiceClone] {s}");
        OnStatusChanged?.Invoke(s);
    }

    // ── 合成/播放生命周期结构化日志（grep: [VoiceClone][LIFE]）────────────────
    static string PreviewText(string text, int maxLen = 48) =>
        string.IsNullOrEmpty(text) ? "(空)"
        : text.Length <= maxLen ? text : text.Substring(0, maxLen) + "…";

    static double PcmBytesToDurationSec(int pcmBytes, int sampleRate) =>
        pcmBytes >= 2 ? pcmBytes / 2.0 / Math.Max(sampleRate, 1) : 0;

    void BeginSpeakLog(int gen, string text, string resId, bool fromCache)
    {
        _logGen = gen;
        _logText = text;
        _logSid = null;
        _logSpeakWallStart = Time.realtimeSinceStartup;
        _logPlayWallStart = 0f;
        string styleInfo = _useMultiStyle
            ? $"lang={_languageId} style={_style ?? "(默认)"} impact={_impactFactor}"
            : "standard";
        Debug.Log(
            "[VoiceClone][LIFE] SYNTH_REQUEST\n" +
            $"  gen         : {gen}\n" +
            $"  text        : {PreviewText(text)}\n" +
            $"  textLen     : {text?.Length ?? 0}\n" +
            $"  res_id      : {resId}\n" +
            $"  vcn         : {Vcn}\n" +
            $"  voice       : {styleInfo}\n" +
            $"  sampleRate  : {_synthSampleRate}\n" +
            $"  trimSilence : {TrimTrailingSilence}\n" +
            $"  fromCache   : {fromCache}");
    }

    void LogLife(string phase, int gen, string body)
    {
        Debug.Log($"[VoiceClone][LIFE] {phase} gen={gen}\n{body}");
    }

    void LogStreamEnd(int gen, string sid, byte[] pcm, int sampleRate, double synthSec, double firstFrameSec, string via)
    {
        int bytes = pcm?.Length ?? 0;
        long trim = TrimTrailingSilence && bytes >= 2
            ? CountTrailingSilenceSamples(pcm, sampleRate) : 0;
        double dur = PcmBytesToDurationSec(bytes, sampleRate);
        LogLife("SYNTH_STREAM_END", gen,
            $"  sid           : {sid}\n" +
            $"  via           : {via}\n" +
            $"  pcmBytes      : {bytes}\n" +
            $"  audioDur      : {dur:F3}s\n" +
            $"  samples       : {bytes / 2}\n" +
            $"  trimSamples   : {trim} ({trim * 1000.0 / sampleRate:F0}ms)\n" +
            $"  ttfb          : {firstFrameSec * 1000:F0}ms\n" +
            $"  synthWall     : {synthSec * 1000:F0}ms");
    }

    void LogPlayStart(int gen, string sid, int sampleRate, int pcmBytesSoFar, long bufferedSamples, bool fromCache, double firstFrameSec)
    {
        _logSid = sid;
        _logPlayWallStart = Time.realtimeSinceStartup;
        double estDur = PcmBytesToDurationSec(pcmBytesSoFar, sampleRate);
        LogLife("PLAY_START", gen,
            $"  sid              : {sid}\n" +
            $"  text             : {PreviewText(_logText)}\n" +
            $"  fromCache        : {fromCache}\n" +
            $"  ttfb             : {firstFrameSec * 1000:F0}ms\n" +
            $"  sinceSynthReq    : {( _logPlayWallStart - _logSpeakWallStart) * 1000:F0}ms\n" +
            $"  prebufferMs      : {_prebufferMs}\n" +
            $"  bufferedSamples  : {bufferedSamples} ({bufferedSamples * 1000.0 / sampleRate:F0}ms)\n" +
            $"  pcmReceivedBytes : {pcmBytesSoFar}\n" +
            $"  estAudioDur      : {estDur:F3}s (收流中可能增长)");
    }

    void LogPlayEnd(int gen, string sid, int sampleRate, int pcmBytes, double synthSec, double firstFrameSec, string reason)
    {
        float playWallMs = _logPlayWallStart > 0f
            ? (Time.realtimeSinceStartup - _logPlayWallStart) * 1000f : 0f;
        float totalWallMs = (Time.realtimeSinceStartup - _logSpeakWallStart) * 1000f;
        double audioDur = PcmBytesToDurationSec(pcmBytes, sampleRate);
        LogLife("PLAY_END", gen,
            $"  sid              : {sid}\n" +
            $"  text             : {PreviewText(_logText)}\n" +
            $"  reason           : {reason}\n" +
            $"  pcmBytes         : {pcmBytes}\n" +
            $"  audioDur         : {audioDur:F3}s\n" +
            $"  ttfb             : {firstFrameSec * 1000:F0}ms\n" +
            $"  synthWall        : {synthSec * 1000:F0}ms\n" +
            $"  playWall         : {playWallMs:F0}ms\n" +
            $"  totalWall        : {totalWallMs:F0}ms (请求→播完)\n" +
            $"  readSamples      : {_pcm.ReadSamples}\n" +
            $"  enqueuedSamples  : {_pcm.EnqueuedSamples}\n" +
            $"  trimSamples      : {_pcm.TrailingSilenceSamples}\n" +
            $"  inputDone        : {_pcm.InputDone}");
    }

    void LogPlayCancel(int gen, string reason)
    {
        float elapsedMs = _logSpeakWallStart > 0f
            ? (Time.realtimeSinceStartup - _logSpeakWallStart) * 1000f : 0f;
        LogLife("PLAY_CANCEL", gen,
            $"  text      : {PreviewText(_logText)}\n" +
            $"  sid       : {_logSid}\n" +
            $"  reason    : {reason}\n" +
            $"  elapsedMs : {elapsedMs:F0}");
    }

    // ════════════════════════════════════════════════════════════════════
    //  训练流程
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 一键训练：用一段本地音频复刻音色。
    /// </summary>
    /// <param name="audioPath">本地音频文件绝对路径（wav/mp3/m4a/pcm，单段≤40s、≤3MB）</param>
    /// <param name="textId">训练文本集 id，通用文本为 5001</param>
    /// <param name="segId">所录文本对应的段落 id（与音频内容必须一致）</param>
    /// <param name="sex">性别 1:男 2:女</param>
    /// <param name="taskName">任务/音库名称</param>
    /// <param name="targetSlot">训练写入的音色槽位（0=A原 1=B新）；训练期间活动槽位会切到此槽。</param>
    public void StartTraining(string audioPath, int textId, int segId, int sex, string taskName,
                              int targetSlot = 0,
                              Action<string> onSuccess = null, Action<string> onError = null)
    {
        if (IsTraining) { onError?.Invoke("已有训练任务进行中"); return; }
        StartCoroutine(TrainCoroutine(audioPath, textId, segId, sex, taskName, targetSlot, onSuccess, onError));
    }

    private IEnumerator TrainCoroutine(string audioPath, int textId, int segId, int sex, string taskName,
                                       int targetSlot, Action<string> onSuccess, Action<string> onError)
    {
        IsTraining = true;
        // 训练期间活动槽位指向被训练音色：使 LastTaskId / ResId 自动落到该槽，查询/提交/测试一致。
        ActiveVoiceSlot = targetSlot;

        Debug.Log(
            "[VoiceClone][REQ] ########## 一句话复刻 训练开始 ##########\n" +
            $"  appId      : {_appId}\n" +
            $"  版本       : {(_useMultiStyle ? "多风格版(x6_clone, engineVersion=omni_v1)" : "标准版(x5_clone)")}\n" +
            $"  目标槽位   : {targetSlot} ({SlotName(targetSlot)})\n" +
            $"  音频文件   : {audioPath}\n" +
            $"  textId     : {textId}\n" +
            $"  textSegId  : {segId}\n" +
            $"  sex        : {sex}\n" +
            $"  taskName   : {taskName}");

        bool useUrl = IsHttpUrl(audioPath);
        if (!useUrl && !File.Exists(audioPath))
        {
            FinishTrainError($"音频文件不存在: {audioPath}", onError);
            yield break;
        }

        // ── Step 1: 获取鉴权 token ──────────────────────────────────────
        SetStatus("正在获取鉴权 token…");
        string error = null;
        yield return GetTokenCoroutine(err => error = err);
        if (error != null) { FinishTrainError(error, onError); yield break; }

        // ── Step 2: 创建训练任务 /task/add ──────────────────────────────
        SetStatus("正在创建训练任务…");
        var addBody = new StringBuilder();
        addBody.Append('{');
        if (_useMultiStyle) addBody.Append("\"engineVersion\":\"omni_v1\",");
        addBody.Append("\"taskName\":").Append(JsonStr(taskName)).Append(',');
        addBody.Append("\"sex\":").Append(sex).Append(',');
        addBody.Append("\"resourceType\":12,");
        addBody.Append("\"resourceName\":").Append(JsonStr(taskName));
        addBody.Append('}');

        string taskId = null;
        yield return PostJsonTrain("/task/add", addBody.ToString(),
            raw =>
            {
                var resp = JsonUtility.FromJson<XFCommonResp>(raw);
                if (resp == null || resp.code != 0)
                    error = $"创建任务失败: code={resp?.code} desc={resp?.desc}（请确认已开通一句话复刻服务）";
                else if (string.IsNullOrEmpty(resp.data))
                    error = "创建任务失败: 未返回 taskId";
                else
                    taskId = resp.data;
            },
            err => error = err);
        if (error != null) { FinishTrainError(error, onError); yield break; }
        LastTaskId = taskId;   // 持久化，便于超时后手动查询
        SetStatus($"任务已创建 taskId={taskId}");

        // ── Step 3: 添加音频并提交训练 ─────────────────────────────────
        //   URL 方式  → /audio/v1/add（JSON，排除本地上传/格式问题）
        //   本地文件  → /task/submitWithAudio（multipart）
        if (useUrl)
        {
            SetStatus("正在通过 URL 添加音频并提交训练…");
            yield return AddAudioByUrlCoroutine(taskId, audioPath, textId, segId,
                err => error = err);
        }
        else
        {
            SetStatus("正在上传音频并提交训练…");
            yield return SubmitWithAudioCoroutine(taskId, audioPath, textId, segId,
                err => error = err);
        }
        if (error != null) { FinishTrainError(error, onError); yield break; }

        // ── Step 4: 轮询训练结果 /task/result ───────────────────────────
        string resId = null;
        string resultBody = "{\"taskId\":" + JsonStr(taskId) + "}";

        for (int i = 0; i < _maxPollCount; i++)
        {
            yield return new WaitForSeconds(_pollInterval);

            int    trainStatus = -99;
            string assetId     = null;
            string failedDesc  = null;
            string pollErr     = null;

            yield return PostJsonTrain("/task/result", resultBody,
                raw =>
                {
                    var resp = JsonUtility.FromJson<XFResultResp>(raw);
                    if (resp == null || resp.code != 0)
                    {
                        pollErr = $"查询结果失败: code={resp?.code} desc={resp?.desc}";
                    }
                    else if (resp.data != null)
                    {
                        trainStatus = resp.data.trainStatus;
                        assetId     = resp.data.assetId;
                        failedDesc  = resp.data.failedDesc;
                    }
                },
                err => pollErr = err);

            if (pollErr != null)
            {
                Debug.LogWarning($"[VoiceClone] 轮询出错(继续重试): {pollErr}");
                SetStatus($"轮询出错重试中… ({i + 1}/{_maxPollCount})");
                continue;
            }

            // trainStatus: -1 训练中, 1 成功, 0 失败, 2 草稿/排队
            if (trainStatus == 1 && !string.IsNullOrEmpty(assetId))
            {
                resId = assetId;
                break;
            }
            if (trainStatus == 0)
            {
                FinishTrainError($"训练失败: {failedDesc}", onError);
                yield break;
            }

            SetStatus($"训练中… ({i + 1}/{_maxPollCount}) status={trainStatus}");
        }

        if (string.IsNullOrEmpty(resId))
        {
            FinishTrainError($"训练超时（已等待 {_pollInterval * _maxPollCount:F0}s）", onError);
            yield break;
        }

        // ── 成功：保存 res_id ───────────────────────────────────────────
        ResId = resId;
        SaveResIdToDocs(targetSlot, resId, taskId);
        IsTraining = false;
        SetStatus($"训练成功！[{SlotName(targetSlot)}] res_id={resId}");
        onSuccess?.Invoke(resId);
    }

    private void FinishTrainError(string msg, Action<string> onError)
    {
        IsTraining = false;
        SetStatus($"错误: {msg}");
        onError?.Invoke(msg);
    }

    /// <summary>
    /// 手动查询某个训练任务的结果（训练在客户端超时但服务端仍在跑时使用）。
    /// 成功(trainStatus==1)会自动把 assetId 存为 ResId。
    /// </summary>
    /// <param name="taskId">为空则使用 <see cref="LastTaskId"/></param>
    public void QueryTaskResult(string taskId = null,
                                Action<int, string, string> onResult = null,
                                Action<string> onError = null)
    {
        string tid = string.IsNullOrEmpty(taskId) ? LastTaskId : taskId;
        if (string.IsNullOrEmpty(tid)) { onError?.Invoke("没有可查询的 taskId（请先发起一次训练）"); return; }
        StartCoroutine(QueryTaskResultCoroutine(tid, onResult, onError));
    }

    /// <summary>
    /// 提交一个已添加音频但仍是草稿(trainStatus==2)的任务，使其真正开始训练。
    /// 用于 URL 流程漏提交、或想直接复用已有草稿任务的场景。
    /// </summary>
    /// <param name="taskId">为空则使用 <see cref="LastTaskId"/></param>
    public void SubmitExistingTask(string taskId = null,
                                   Action onSuccess = null, Action<string> onError = null)
    {
        string tid = string.IsNullOrEmpty(taskId) ? LastTaskId : taskId;
        if (string.IsNullOrEmpty(tid)) { onError?.Invoke("没有可提交的 taskId（请先发起一次训练）"); return; }
        StartCoroutine(SubmitExistingTaskCoroutine(tid, onSuccess, onError));
    }

    private IEnumerator SubmitExistingTaskCoroutine(string taskId, Action onSuccess, Action<string> onError)
    {
        SetStatus($"正在提交草稿任务 taskId={taskId}…");

        if (string.IsNullOrEmpty(_token))
        {
            string tokErr = null;
            yield return GetTokenCoroutine(e => tokErr = e);
            if (tokErr != null) { SetStatus($"错误: {tokErr}"); onError?.Invoke(tokErr); yield break; }
        }

        string err = null;
        yield return SubmitTaskCoroutine(taskId, e => err = e);
        if (err != null) { SetStatus($"错误: {err}"); onError?.Invoke(err); yield break; }

        SetStatus("已提交训练，请用「查询训练结果」跟进状态。");
        onSuccess?.Invoke();
    }

    private IEnumerator QueryTaskResultCoroutine(string taskId,
                                                 Action<int, string, string> onResult,
                                                 Action<string> onError)
    {
        SetStatus($"正在查询任务结果 taskId={taskId}…");

        // token 可能为空（重启后），先确保有 token
        if (string.IsNullOrEmpty(_token))
        {
            string tokErr = null;
            yield return GetTokenCoroutine(e => tokErr = e);
            if (tokErr != null) { SetStatus($"错误: {tokErr}"); onError?.Invoke(tokErr); yield break; }
        }

        int    trainStatus = -99;
        string assetId     = null;
        string failedDesc  = null;
        string err         = null;

        string body = "{\"taskId\":" + JsonStr(taskId) + "}";
        yield return PostJsonTrain("/task/result", body,
            raw =>
            {
                var resp = JsonUtility.FromJson<XFResultResp>(raw);
                if (resp == null || resp.code != 0)
                    err = $"查询失败: code={resp?.code} desc={resp?.desc}";
                else if (resp.data != null)
                {
                    trainStatus = resp.data.trainStatus;
                    assetId     = resp.data.assetId;
                    failedDesc  = resp.data.failedDesc;
                }
            },
            e => err = e);

        if (err != null) { SetStatus($"错误: {err}"); onError?.Invoke(err); yield break; }

        // -1 训练中, 1 成功, 0 失败, 2 草稿/排队
        if (trainStatus == 1 && !string.IsNullOrEmpty(assetId))
        {
            ResId = assetId;
            SaveResIdToDocs(ActiveVoiceSlot, assetId, taskId);
            SetStatus($"训练已成功！[{SlotName(ActiveVoiceSlot)}] res_id={assetId}");
        }
        else if (trainStatus == 0)
            SetStatus($"训练失败: {failedDesc}");
        else if (trainStatus == 2)
            SetStatus("任务为草稿/未提交(status=2)，需点「提交草稿任务」才会开始训练。");
        else
            SetStatus($"训练中… status={trainStatus}（-1训练中）");

        onResult?.Invoke(trainStatus, assetId, failedDesc);
    }

    // ── 获取 token：Authorization = MD5( MD5(apiKey+ts) + body ) ─────────
    private IEnumerator GetTokenCoroutine(Action<string> onError)
    {
        string ts   = NowMs();
        string body = "{\"base\":{\"appid\":\"" + _appId +
                      "\",\"version\":\"v1\",\"timestamp\":\"" + ts +
                      "\"},\"model\":\"remote\"}";
        string keySign       = Md5(_apiKey + ts);
        string authorization = Md5(keySign + body);

        Debug.Log(
            "[VoiceClone][REQ] ===== Step1 获取鉴权Token =====\n" +
            $"  POST {AUTH_TOKEN_URL}\n" +
            "  Header Content-Type: application/json\n" +
            $"  Header Authorization: {authorization}\n" +
            $"  Body: {body}");

        using var req = new UnityWebRequest(AUTH_TOKEN_URL, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", authorization);
        req.timeout = 15;

        yield return req.SendWebRequest();

        Debug.Log($"[VoiceClone][RESP] Step1 获取Token <- HTTP {req.responseCode}\n  {req.downloadHandler.text}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"token HTTP 失败: {req.error}");
            yield break;
        }

        var resp = JsonUtility.FromJson<XFTokenResp>(req.downloadHandler.text);
        if (resp == null || resp.retcode != "000000" || string.IsNullOrEmpty(resp.accesstoken))
        {
            onError?.Invoke($"token API 错误: retcode={resp?.retcode}");
            yield break;
        }
        _token = resp.accesstoken;
    }

    // ── JSON 接口通用 POST：X-Sign = MD5(apiKey + X-Time + MD5(body)) ────
    private IEnumerator PostJsonTrain(string path, string bodyJson,
                                      Action<string> onRaw, Action<string> onError)
    {
        string xTime = NowMs();
        string xSign = Md5(_apiKey + xTime + Md5(bodyJson));
        string url   = TRAIN_BASE + path;

        Debug.Log(
            $"[VoiceClone][REQ] ===== POST {url} =====\n" +
            "  Header Content-Type: application/json\n" +
            $"  Header X-AppId: {_appId}\n" +
            $"  Header X-Time : {xTime}\n" +
            $"  Header X-Sign : {xSign}  (= MD5(apiKey + X-Time + MD5(body)))\n" +
            $"  Header X-Token: {_token}\n" +
            $"  Body: {bodyJson}");

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("X-Sign",  xSign);
        req.SetRequestHeader("X-Token", _token);
        req.SetRequestHeader("X-AppId", _appId);
        req.SetRequestHeader("X-Time",  xTime);
        req.timeout = 30;

        yield return req.SendWebRequest();

        Debug.Log($"[VoiceClone][RESP] POST {path} <- HTTP {req.responseCode}\n  {req.downloadHandler.text}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"{path} HTTP 失败: {req.error} | {req.downloadHandler.text}");
            yield break;
        }
        string raw = req.downloadHandler.text;
        onRaw?.Invoke(raw);
    }

    private static bool IsHttpUrl(string s) =>
        !string.IsNullOrEmpty(s) &&
        (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
         s.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

    // ── 通过 URL 添加音频并提交 /audio/v1/add（application/json）─────────
    //    body: {"taskId":..,"audioUrl":..,"textId":..,"textSegId":..}
    //    audioUrl 必须 http/https 开头、以 mp3/wav/m4a/pcm 结尾。
    private IEnumerator AddAudioByUrlCoroutine(string taskId, string audioUrl,
                                               int textId, int segId, Action<string> onError)
    {
        var body = new StringBuilder();
        body.Append('{');
        body.Append("\"taskId\":").Append(JsonStr(taskId)).Append(',');
        body.Append("\"audioUrl\":").Append(JsonStr(audioUrl)).Append(',');
        body.Append("\"textId\":").Append(textId).Append(',');
        body.Append("\"textSegId\":").Append(segId);
        body.Append('}');

        Debug.Log(
            "[VoiceClone][REQ] ===== Step3a 通过URL添加音频(/audio/v1/add) =====\n" +
            $"  audioUrl: {audioUrl}");

        string err = null;
        yield return PostJsonTrain("/audio/v1/add", body.ToString(),
            raw =>
            {
                var resp = JsonUtility.FromJson<XFCommonResp>(raw);
                if (resp == null || resp.code != 0)
                    err = $"添加音频(URL)失败: code={resp?.code} desc={resp?.desc}";
            },
            e => err = e);

        if (err != null) { onError?.Invoke(err); yield break; }

        // URL 方式必须再单独调用 /task/submit 才会真正开始训练
        // （本地 /task/submitWithAudio 是“添加+提交”二合一，无需此步）
        SetStatus("正在提交训练任务…");
        yield return SubmitTaskCoroutine(taskId, e => err = e);
        if (err != null) onError?.Invoke(err);
    }

    // ── 提交训练任务 /task/submit（application/json，body: {"taskId":..}）──
    private IEnumerator SubmitTaskCoroutine(string taskId, Action<string> onError)
    {
        string body = "{\"taskId\":" + JsonStr(taskId) + "}";
        Debug.Log($"[VoiceClone][REQ] ===== Step3b 提交训练(/task/submit) ===== taskId={taskId}");

        string err = null;
        yield return PostJsonTrain("/task/submit", body,
            raw =>
            {
                var resp = JsonUtility.FromJson<XFCommonResp>(raw);
                if (resp == null || resp.code != 0)
                    err = $"提交训练失败: code={resp?.code} desc={resp?.desc}";
            },
            e => err = e);

        if (err != null) onError?.Invoke(err);
    }

    // ── 上传本地音频并提交（multipart/form-data）────────────────────────
    //    X-Sign 的 body 取官方示例口径：Python str(dict) 形式
    //    "{'taskId': <id>, 'textId': <id>, 'textSegId': <id>}"
    private IEnumerator SubmitWithAudioCoroutine(string taskId, string audioPath,
                                                 int textId, int segId, Action<string> onError)
    {
        // 讯飞引擎只认 16-bit 整数 PCM；若源音频是 float/24bit/多声道，先转码，
        // 否则会出现“音频内容识别失败”。转码失败则回退用原文件。
        string srcDesc = DescribeWav(audioPath);
        Debug.Log($"[VoiceClone] 原始音频: {audioPath}\n  [{srcDesc}]");

        string uploadPath = audioPath;
        try
        {
            string converted = EnsureCompliantWav(audioPath);
            if (converted != null) uploadPath = converted;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[VoiceClone] 音频转码失败，改用原文件上传: {ex.Message}");
        }

        byte[] audioBytes;
        try { audioBytes = File.ReadAllBytes(uploadPath); }
        catch (Exception ex) { onError?.Invoke($"读取音频失败: {ex.Message}"); yield break; }

        string fileName = Path.GetFileName(uploadPath);
        string mime     = MimeOf(fileName);
        string upDesc   = DescribeWav(uploadPath);

        var form = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", audioBytes, fileName, mime),
            new MultipartFormDataSection("taskId",    taskId),
            new MultipartFormDataSection("textId",    textId.ToString()),
            new MultipartFormDataSection("textSegId", segId.ToString()),
        };

        string xTime = NowMs();
        // 与官方 python 示例一致：以字段字典的 str() 形式参与签名
        string bodyForSign = "{'taskId': " + taskId +
                             ", 'textId': " + textId +
                             ", 'textSegId': " + segId + "}";
        string xSign = Md5(_apiKey + xTime + Md5(bodyForSign));
        string url   = TRAIN_BASE + "/task/submitWithAudio";

        Debug.Log(
            $"[VoiceClone][REQ] ===== Step3 上传音频并提交 =====\n" +
            $"  POST {url}   (multipart/form-data)\n" +
            $"  Header X-AppId: {_appId}\n" +
            $"  Header X-Time : {xTime}\n" +
            $"  Header X-Sign : {xSign}  (= MD5(apiKey + X-Time + MD5(signBody)))\n" +
            $"  Header X-Token: {_token}\n" +
            $"  signBody(参与签名的串): {bodyForSign}\n" +
            $"  form.file  : 字段名=file  文件名={fileName}  MIME={mime}  大小={audioBytes.Length}字节\n" +
            $"  form.file  : 音频格式=[{upDesc}]\n" +
            $"  form.taskId   : {taskId}\n" +
            $"  form.textId   : {textId}\n" +
            $"  form.textSegId: {segId}");

        using var req = UnityWebRequest.Post(url, form);
        req.SetRequestHeader("X-Sign",  xSign);
        req.SetRequestHeader("X-Token", _token);
        req.SetRequestHeader("X-AppId", _appId);
        req.SetRequestHeader("X-Time",  xTime);
        req.timeout = 120;

        yield return req.SendWebRequest();

        Debug.Log($"[VoiceClone][RESP] Step3 submitWithAudio <- HTTP {req.responseCode}\n  {req.downloadHandler.text}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"submitWithAudio HTTP 失败: {req.error} | {req.downloadHandler.text}");
            yield break;
        }

        string raw = req.downloadHandler.text;
        var resp = JsonUtility.FromJson<XFCommonResp>(raw);
        if (resp == null || resp.code != 0)
            onError?.Invoke($"submitWithAudio 失败: code={resp?.code} desc={resp?.desc}");
    }

    private void SaveResIdToDocs(int slot, string resId, string taskId)
    {
#if UNITY_EDITOR
        try
        {
            // 槽位 0 沿用原文件名，槽位 1 写独立文件，两个音色记录互不覆盖
            string fileName = slot == 0 ? "VoiceClone_ResId.txt" : "VoiceClone_ResId_B.txt";
            string path = Path.Combine(Application.dataPath, "..", "Docs", fileName);
            string content =
                $"# 科大讯飞一句话复刻 训练结果\r\n" +
                $"# 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                $"slot    : {slot} ({SlotName(slot)})\r\n" +
                $"version : {(_useMultiStyle ? "多风格版 x6_clone" : "标准版 x5_clone")}\r\n" +
                $"taskId  : {taskId}\r\n" +
                $"res_id  : {resId}\r\n";
            File.WriteAllText(path, content, Encoding.UTF8);
            Debug.Log($"[VoiceClone] res_id 已保存: {path}");
        }
        catch (Exception ex) { Debug.LogWarning($"[VoiceClone] 保存 res_id 文件失败: {ex.Message}"); }
#endif
    }

    // ════════════════════════════════════════════════════════════════════
    //  合成流程（WebSocket）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 用已训练的音色合成并「边收边播」一段文本（流式，低首帧延迟）。
    /// 命中缓存时直接播放，不发起网络请求。
    /// </summary>
    /// <param name="onComplete">播放完成回调</param>
    /// <param name="onError">出错回调</param>
    /// <param name="onFirstAudio">首帧音频就绪回调（参数=首帧耗时秒；缓存命中为 0）</param>
    /// <param name="useCache">是否使用合成缓存</param>
    /// <param name="cacheEvictable">是否允许 LRU 淘汰；仅玩家自定义昵称应传 true，固定播报默认 false</param>
    /// <param name="rdn">数字发音方式：0 自动, 1 完全数值, 2 完全字符串(逐位/电话号), 3 字符串优先</param>
    public void Speak(string text, Action onComplete = null, Action<string> onError = null,
                      Action<float> onFirstAudio = null, bool useCache = true, bool cacheEvictable = false,
                      int rdn = 0)
    {
        if (string.IsNullOrEmpty(text)) { onError?.Invoke("文本为空"); return; }

        string resId = ResId;
        if (string.IsNullOrEmpty(resId)) { onError?.Invoke("尚未训练出音色（res_id 为空），请先训练"); return; }

        if (_speakState != SpeakState.Idle) StopSpeak();

        int gen = ++_speakGeneration;
        _speakRdn = rdn;

        _onComplete   = onComplete;
        _onError      = onError;
        _onFirstAudio = onFirstAudio;

        _pcm.Reset();
        lock (_stateLock)
        {
            _workerError       = null;
            _workerSid         = null;
            _workerFirstFrame  = false;
            _workerDone        = false;
            _workerGeneration  = gen;
            _workerSampleRate  = _synthSampleRate;
            _workerPcmBytes    = 0;
            _firstFrameSec     = 0;
            _synthSec          = 0;
        }
        _playbackStarted = false;
        _completeFired   = false;
        _drainedAt       = -1f;
        LastFromCache    = false;
        _speakState      = SpeakState.Synthesizing;

        // 缓存命中：灌入完整 PCM，免去网络与等待
        string key = CacheKey(resId, text, cacheEvictable, rdn);
        byte[] cached = null;
        bool cacheHit = useCache && _enableCache && TryGetCache(key, out cached);
        BeginSpeakLog(gen, text, resId, cacheHit);

        if (cacheHit)
        {
            ApplyCachedPcmForPlayback(gen, key, cached, "memory_cache_hit", sid: "memory");
            return;
        }

        _synthCts = new CancellationTokenSource();
        var ct = _synthCts.Token;

        if (useCache && _enableDiskCache && _diskCache != null && _diskCache.Contains(key))
        {
            SetStatus("正在读取本地磁盘缓存…");
            Task.Run(() => DiskOrSynthWorker(text, resId, key, ct, gen));
            return;
        }

        SetStatus("正在连接合成服务…");
        Task.Run(() => SynthWorker(text, resId, key, ct, gen));
    }

    async Task DiskOrSynthWorker(string text, string resId, string cacheKey, CancellationToken ct, int gen)
    {
        byte[] pcm = null;
        try
        {
            pcm = await _diskCache.TryLoadPcmAsync(cacheKey, ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[VoiceClone][DISK] 加载异常，回退网络合成: {ex.Message}");
        }

        if (ct.IsCancellationRequested) return;

        if (pcm != null && pcm.Length >= 2 && gen == _speakGeneration)
        {
            ApplyCachedPcmForPlayback(gen, cacheKey, pcm, "disk_cache_hit", sid: "disk");
            return;
        }

        await SynthWorker(text, resId, cacheKey, ct, gen);
    }

    void ApplyCachedPcmForPlayback(int gen, string cacheKey, byte[] pcm, string via, string sid)
    {
        if (_enableCache) StoreCache(cacheKey, pcm);

        LastFromCache     = true;
        LastFirstFrameSec = 0;
        LastSynthSec      = 0;
        LastPcmBytes      = pcm.Length;
        _lastFullPcm      = pcm;
        _pcm.EnqueueBytes(pcm);
        if (TrimTrailingSilence)
            _pcm.SetTrailingSilence(CountTrailingSilenceSamples(pcm, _synthSampleRate));
        _pcm.InputDone = true;

        PublishWorkerState(gen, () =>
        {
            _workerFirstFrame  = true;
            _workerDone        = true;
            _workerGeneration  = gen;
            _workerPcmBytes    = pcm.Length;
            _workerSampleRate  = _synthSampleRate;
            _workerSid         = sid;
            _firstFrameSec     = 0;
        });

        LogStreamEnd(gen, sid, pcm, _synthSampleRate, synthSec: 0, firstFrameSec: 0, via: via);
        SetStatus($"缓存命中({via})，直接播放（{pcm.Length} 字节）");
    }

    public void StopSpeak()
    {
        if (_speakState != SpeakState.Idle)
        {
            LogPlayCancel(_logGen > 0 ? _logGen : _speakGeneration, "StopSpeak");
        }
        _speakGeneration++;
        try { _synthCts?.Cancel(); } catch { }
        _pcm.InputDone = true;
        _completeFired = true;   // 手动停止不触发 onComplete
        FinishSpeak();
    }

    // worker 线程：仅当代际仍有效时回写主线程状态，避免取消/打断后过期 worker 污染新会话
    private void PublishWorkerState(int gen, Action publish)
    {
        lock (_stateLock)
        {
            if (gen != _speakGeneration) return;
            publish();
        }
    }

    private async Task SynthWorker(string text, string resId, string cacheKey, CancellationToken ct, int gen)
    {
        ClientWebSocket ws  = null;
        var             sw  = System.Diagnostics.Stopwatch.StartNew();
        using var       all = new MemoryStream();   // 累积完整 PCM 以写入缓存
        try
        {
            string url = BuildWsAuthUrl(TTS_WS_URL, _apiKey, _apiSecret);
            string preview = text.Length <= 80 ? text : text.Substring(0, 80) + "…";
            string styleInfo = _useMultiStyle
                ? $"  languageID: {_languageId}  style: {(string.IsNullOrEmpty(_style) ? "(默认)" : _style)}  impactFactor: {_impactFactor}\n"
                : "";
            Debug.Log(
                $"[VoiceClone][REQ] ===== 合成(WebSocket 流式, {(_useMultiStyle ? "多风格版" : "标准版")}) =====\n" +
                $"  WSS {TTS_WS_URL}\n" +
                $"  appId    : {_appId}\n" +
                $"  res_id   : {resId}\n" +
                $"  vcn      : {Vcn}\n" +
                styleInfo +
                $"  encoding : raw  sample_rate={_synthSampleRate}  channels=1  bit_depth=16\n" +
                $"  textLen  : {text.Length} 字\n" +
                $"  textPreview: {preview}");

            ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(url), ct);

            string requestJson = BuildSynthRequest(text, resId);
            var sendBytes = Encoding.UTF8.GetBytes(requestJson);
            await ws.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, ct);

            using var msgBuf = new MemoryStream();
            var       buffer = new byte[16384];
            bool      done   = false;
            string    sid    = null;
            string    err    = null;
            bool      first  = false;
            double    firstFrameSec = 0;
            bool      streamEndLogged = false;

            while (!done && !ct.IsCancellationRequested)
            {
                msgBuf.SetLength(0);
                WebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close) { done = true; break; }
                    msgBuf.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (msgBuf.Length == 0) continue;

                string json = Encoding.UTF8.GetString(msgBuf.GetBuffer(), 0, (int)msgBuf.Length);
                var resp = JsonUtility.FromJson<WsResp>(json);
                if (resp?.header != null)
                {
                    sid = resp.header.sid;
                    if (resp.header.code != 0)
                    {
                        err  = $"合成错误 code={resp.header.code} msg={resp.header.message}";
                        done = true;
                        break;
                    }

                    // 讯飞流式最后一帧常在 header.status=2（audio 字段可能为空），必须识别否则 ReceiveAsync 永久阻塞
                    if (resp.header.status == 2)
                    {
                        done = true;
                        MarkStreamInputDone(all, gen, sid, firstFrameSec, sw.Elapsed.TotalSeconds, "header.status=2", ref streamEndLogged);
                    }
                }

                if (resp?.payload?.audio != null && !string.IsNullOrEmpty(resp.payload.audio.audio))
                {
                    byte[] chunk = Convert.FromBase64String(resp.payload.audio.audio);
                    all.Write(chunk, 0, chunk.Length);
                    _pcm.EnqueueBytes(chunk);   // 立即进入播放缓冲，边收边播

                    if (!first)
                    {
                        first = true;
                        firstFrameSec = sw.Elapsed.TotalSeconds;
                        Debug.Log($"[VoiceClone][RESP] 合成首帧到达：{firstFrameSec * 1000:F0}ms ({firstFrameSec:F2}s) sid={sid}");
                        LogLife("SYNTH_FIRST_FRAME", gen,
                            $"  sid        : {sid}\n" +
                            $"  ttfb       : {firstFrameSec * 1000:F0}ms\n" +
                            $"  chunkBytes : {chunk.Length}\n" +
                            $"  chunkDur   : {PcmBytesToDurationSec(chunk.Length, _synthSampleRate):F3}s");
                        PublishWorkerState(gen, () =>
                        {
                            _workerFirstFrame = true;
                            _workerSid        = sid;
                            _firstFrameSec    = firstFrameSec;
                            _workerSampleRate = _synthSampleRate;
                        });
                    }

                    if (resp.payload.audio.status == 2)
                    {
                        done = true;
                        MarkStreamInputDone(all, gen, sid, firstFrameSec, sw.Elapsed.TotalSeconds, "payload.audio.status=2", ref streamEndLogged);
                    }
                }
            }

            try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None); }
            catch { }

            sw.Stop();

            byte[] fullBytes = all.ToArray();
            double audioDur  = fullBytes.Length >= 2 ? fullBytes.Length / 2.0 / _synthSampleRate : 0;

            if (err == null && TrimTrailingSilence && fullBytes.Length >= 2)
                _pcm.SetTrailingSilence(CountTrailingSilenceSamples(fullBytes, _synthSampleRate));

            _pcm.InputDone = true;

            if (err == null)
            {
                Debug.Log(
                    "[VoiceClone][RESP] 合成(WebSocket 流式) <- 完成\n" +
                    $"  sid       : {sid}\n" +
                    $"  pcmBytes  : {fullBytes.Length}\n" +
                    $"  audioDur  : {audioDur:F2}s\n" +
                    $"  首帧耗时  : {firstFrameSec * 1000:F0}ms\n" +
                    $"  合成耗时  : {sw.Elapsed.TotalMilliseconds:F0}ms ({sw.Elapsed.TotalSeconds:F2}s)");

                if (!streamEndLogged)
                    LogStreamEnd(gen, sid, fullBytes, _synthSampleRate, sw.Elapsed.TotalSeconds, firstFrameSec, "worker_done");

                if (fullBytes.Length >= 2) _lastFullPcm = fullBytes;   // 供导出音频
                if (fullBytes.Length >= 2 && _enableCache) StoreCache(cacheKey, fullBytes);
                if (fullBytes.Length >= 2 && _enableDiskCache && _diskCache != null)
                    _diskCache.StoreAsync(cacheKey, fullBytes, _synthSampleRate);
            }
            else
            {
                Debug.Log(
                    "[VoiceClone][RESP] 合成(WebSocket 流式) <- 失败\n" +
                    $"  sid       : {sid}\n" +
                    $"  error     : {err}\n" +
                    $"  耗时      : {sw.Elapsed.TotalMilliseconds:F0}ms");
                LogLife("SYNTH_FAIL", gen,
                    $"  sid    : {sid}\n" +
                    $"  error  : {err}\n" +
                    $"  synthMs: {sw.Elapsed.TotalMilliseconds:F0}");
            }

            PublishWorkerState(gen, () =>
            {
                _workerError    = err;
                _workerSid      = sid;
                _workerPcmBytes = fullBytes.Length;
                _synthSec       = sw.Elapsed.TotalSeconds;
                _workerDone     = true;
            });
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            LogPlayCancel(gen, $"worker_cancelled synthMs={sw.Elapsed.TotalMilliseconds:F0}");
            // 取消时不回写 _workerDone，避免污染新一轮 Speak
        }
        catch (Exception ex)
        {
            sw.Stop();
            _pcm.InputDone = true;
            Debug.Log($"[VoiceClone][RESP] 合成(WebSocket 流式) <- 异常: {ex.Message}  耗时={sw.Elapsed.TotalMilliseconds:F0}ms");
            PublishWorkerState(gen, () =>
            {
                _workerError = ex.Message;
                _workerDone  = true;
                _synthSec    = sw.Elapsed.TotalSeconds;
            });
        }
        finally
        {
            try { ws?.Dispose(); } catch { }
        }
    }

    /// <summary>仅合成 PCM 的结果（不播放、不占用 <see cref="Speak"/> 状态机）。</summary>
    public struct VoiceClonePcmResult
    {
        public bool   Success;
        public byte[] Pcm;
        public int    SampleRate;
        public string Sid;
        public string Error;
    }

    /// <summary>
    /// 用当前活动槽位的 res_id 合成 PCM，不播放。
    /// <paramref name="requestSampleRate"/> 为 0 时使用 <see cref="SynthSampleRate"/>（24k）；
    /// 批量导出 WAV 请传 <see cref="EmbeddedExportSampleRate"/>（16k）。
    /// </summary>
    public Task<VoiceClonePcmResult> SynthesizePcmAsync(string text, int requestSampleRate = 0,
                                                        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(text))
            return Task.FromResult(new VoiceClonePcmResult { Success = false, Error = "文本为空" });

        string resId = ResId;
        if (string.IsNullOrEmpty(resId))
            return Task.FromResult(new VoiceClonePcmResult { Success = false, Error = "尚未训练出音色（res_id 为空）" });

        int rate = requestSampleRate > 0 ? requestSampleRate : _synthSampleRate;
        return SynthPcmOnlyAsync(text, resId, rate, ct);
    }

    /// <summary>
    /// 把 16bit PCM 写入 WAV。
    /// <paramref name="sampleRate"/> 为 0 时使用 <see cref="SynthSampleRate"/>。
    /// </summary>
    public bool SavePcmToWav(string fullPath, byte[] pcm, out string error, int sampleRate = 0)
    {
        error = null;
        if (pcm == null || pcm.Length < 2)
        {
            error = "PCM 为空";
            return false;
        }
        try
        {
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            WritePcm16WavFromBytes(fullPath, pcm, sampleRate > 0 ? sampleRate : _synthSampleRate);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 将合成 PCM 转为播放友好格式：降采样 + 智能裁尾（在有效语音末尾淡出，而非硬切静音）。
    /// </summary>
    public byte[] PrepareEmbeddedExportPcm(byte[] pcm16, int srcSampleRate,
                                           int dstSampleRate = EmbeddedExportSampleRate)
    {
        if (pcm16 == null || pcm16.Length < 2) return pcm16;

        byte[] pcm = srcSampleRate == dstSampleRate
            ? pcm16
            : DownsamplePcm16(pcm16, srcSampleRate, dstSampleRate);

        return FinalizeEmbeddedExportPcm(pcm, dstSampleRate);
    }

    private async Task<VoiceClonePcmResult> SynthPcmOnlyAsync(string text, string resId, int requestSampleRate,
                                                              CancellationToken ct)
    {
        ClientWebSocket ws = null;
        using var all = new MemoryStream();
        try
        {
            string url = BuildWsAuthUrl(TTS_WS_URL, _apiKey, _apiSecret);
            string preview = text.Length <= 80 ? text : text.Substring(0, 80) + "…";
            Debug.Log(
                $"[VoiceClone][EXPORT-REQ] res_id={resId} vcn={Vcn} sample_rate={requestSampleRate}\n" +
                $"  textLen={text.Length} preview={preview}");

            ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(url), ct);

            byte[] sendBytes = Encoding.UTF8.GetBytes(BuildSynthRequest(text, resId, requestSampleRate));
            await ws.SendAsync(new ArraySegment<byte>(sendBytes), WebSocketMessageType.Text, true, ct);

            using var msgBuf = new MemoryStream();
            var buffer = new byte[16384];
            bool done = false;
            string sid = null;
            string err = null;
            int respSampleRate = requestSampleRate;

            while (!done && !ct.IsCancellationRequested)
            {
                msgBuf.SetLength(0);
                WebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close) { done = true; break; }
                    msgBuf.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (msgBuf.Length == 0) continue;

                string json = Encoding.UTF8.GetString(msgBuf.GetBuffer(), 0, (int)msgBuf.Length);
                var resp = JsonUtility.FromJson<WsResp>(json);
                if (resp?.header != null)
                {
                    sid = resp.header.sid;
                    if (resp.header.code != 0)
                    {
                        err = $"code={resp.header.code} msg={resp.header.message}";
                        done = true;
                        break;
                    }
                    if (resp.header.status == 2)
                        done = true;
                }

                if (resp?.payload?.audio != null && !string.IsNullOrEmpty(resp.payload.audio.audio))
                {
                    byte[] chunk = Convert.FromBase64String(resp.payload.audio.audio);
                    all.Write(chunk, 0, chunk.Length);
                    if (resp.payload.audio.sample_rate > 0)
                        respSampleRate = resp.payload.audio.sample_rate;
                    if (resp.payload.audio.status == 2)
                        done = true;
                }
            }

            try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None); }
            catch { }

            byte[] pcm = all.ToArray();
            if (err != null)
                return new VoiceClonePcmResult { Success = false, Error = err, Sid = sid };

            if (pcm.Length < 2)
                return new VoiceClonePcmResult { Success = false, Error = "pcm_empty", Sid = sid };

            return new VoiceClonePcmResult
            {
                Success    = true,
                Pcm        = pcm,
                SampleRate = respSampleRate,
                Sid        = sid,
            };
        }
        catch (OperationCanceledException)
        {
            return new VoiceClonePcmResult { Success = false, Error = "cancelled" };
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VoiceClone][EXPORT] SynthPcmOnlyAsync failed: {ex}");
            return new VoiceClonePcmResult { Success = false, Error = ex.Message };
        }
        finally
        {
            try { ws?.Dispose(); } catch { }
        }
    }

    // 主线程：根据 worker 交接状态推进「开始播放 / 播放完成」
    private void Update()
    {
        if (_speakState == SpeakState.Idle) return;

        bool firstFrame, workerDone;
        string err, sid;
        double ffSec, synthSec;
        int sr, pcmBytes, workerGen;
        lock (_stateLock)
        {
            firstFrame   = _workerFirstFrame;
            workerDone   = _workerDone;
            err          = _workerError;
            sid          = _workerSid;
            ffSec        = _firstFrameSec;
            synthSec     = _synthSec;
            sr           = _workerSampleRate;
            pcmBytes     = _workerPcmBytes;
            workerGen    = _workerGeneration;
        }

        if (workerGen != _speakGeneration) return;

        if (_speakState == SpeakState.Synthesizing)
        {
            if (err != null && !_playbackStarted)
            {
                SetStatus($"合成失败: {err}");
                FinishSpeak();
                _onError?.Invoke(err);
                return;
            }

            // 预缓冲：积累到阈值或 worker 已收完再开播，避免开头欠载卡顿
            int prebufferSamples = Mathf.Max(1, (int)(sr * _prebufferMs / 1000f));
            bool prebuffered = workerDone || _pcm.BufferedSamples >= prebufferSamples;

            if (firstFrame && prebuffered && !_playbackStarted)
            {
                StartStreamPlayback(sr);
                _playbackStarted  = true;
                _speakState       = SpeakState.Playing;
                LastFirstFrameSec = (float)ffSec;
                LogPlayStart(workerGen, sid, sr, pcmBytes, _pcm.BufferedSamples, LastFromCache, ffSec);
                SetStatus($"开始播放（首帧 {ffSec:F2}s{(LastFromCache ? "，缓存" : "")}，sid={sid}）");
                _onFirstAudio?.Invoke((float)ffSec);
            }
            else if (workerDone && !firstFrame && pcmBytes <= 0)
            {
                SetStatus("合成完成但无音频数据");
                FinishSpeak();
                _onError?.Invoke(err ?? "无音频数据");
                return;
            }
        }

        if (_speakState == SpeakState.Playing)
        {
            LastSynthSec = (float)synthSec;
            if (pcmBytes > 0) LastPcmBytes = pcmBytes;

            bool playbackDone = _pcm.Drained;
            string completeReason = "drained";
            // 兜底：worker 已结束、输入已标记完成且有效样本已读完
            if (!playbackDone && workerDone && _pcm.InputDone
                && _pcm.BufferedSamples <= 0 && _pcm.ReadSamples > 0)
            {
                long effectiveEnd = _pcm.EnqueuedSamples - _pcm.TrailingSilenceSamples;
                if (effectiveEnd < 0) effectiveEnd = 0;
                if (_pcm.ReadSamples >= effectiveEnd)
                {
                    playbackDone = true;
                    completeReason = "fallback_buffer_empty";
                }
            }

            if (playbackDone)
            {
                if (_drainedAt < 0f) _drainedAt = Time.realtimeSinceStartup;
                else if (Time.realtimeSinceStartup - _drainedAt > DrainedConfirmSec + _playbackTailPadSec)
                {
                    LogPlayEnd(workerGen, sid, sr, pcmBytes, synthSec, ffSec, completeReason);
                    SetStatus($"播放完成 read={_pcm.ReadSamples} enq={_pcm.EnqueuedSamples} trim={_pcm.TrailingSilenceSamples}");
                    FinishSpeak();
                    if (!_completeFired) { _completeFired = true; _onComplete?.Invoke(); }
                    return;
                }
            }
            else
            {
                _drainedAt = -1f;
            }
        }
    }

    void MarkStreamInputDone(MemoryStream all, int gen, string sid, double firstFrameSec, double synthSec, string via, ref bool logged)
    {
        byte[] partial = all.ToArray();
        if (TrimTrailingSilence && partial.Length >= 2)
            _pcm.SetTrailingSilence(CountTrailingSilenceSamples(partial, _synthSampleRate));
        _pcm.InputDone = true;
        if (!logged)
        {
            LogStreamEnd(gen, sid, partial, _synthSampleRate, synthSec, firstFrameSec, via);
            logged = true;
        }
    }

    // 复用常驻流式 AudioClip（避免每次 Create/Destroy 造成主线程 spike）
    // PCMReaderCallback 持续从 _pcm 拉取，loop 保证不停；结束只 Stop 不销毁。
    private void StartStreamPlayback(int sampleRate)
    {
        if (_streamClip == null || _streamClipRate != sampleRate)
        {
            if (_streamClip != null) Destroy(_streamClip);
            int loopLen = Mathf.Max(sampleRate, 1);
            _streamClip = AudioClip.Create("voice_clone_stream", loopLen, 1, sampleRate, true,
                                           OnAudioRead, OnAudioSetPosition);
            _streamClipRate   = sampleRate;
            _audioSource.clip = _streamClip;
            _audioSource.loop = true;
        }
        _audioSource.Play();
    }

    private void OnAudioRead(float[] data) => _pcm.Read(data);
    private void OnAudioSetPosition(int newPosition) { }

    private void FinishSpeak()
    {
        if (_audioSource != null && _audioSource.isPlaying) _audioSource.Stop();
        _speakState = SpeakState.Idle;
    }

    // 从 16bit LE PCM 末尾统计连续静音样本数（供 TrimTrailingSilence 使用）。
    // keepMs 保留尾音余量；maxTrimMs / maxTrimRatio 防止把低音量收尾误判成静音而裁掉。
    private static long CountTrailingSilenceSamples(byte[] pcm16, int sampleRate,
                                                    float threshold = 0.004f, int keepMs = 150,
                                                    int maxTrimMs = 120, int maxTrimRatioPercent = 12)
    {
        if (pcm16 == null || pcm16.Length < 2) return 0;

        int totalSamples = pcm16.Length / 2;
        int limit = (int)(threshold * 32768f);
        int silent = 0;
        for (int i = totalSamples - 1; i >= 0; i--)
        {
            short s = (short)(pcm16[2 * i] | (pcm16[2 * i + 1] << 8));
            if (Math.Abs((int)s) > limit) break;
            silent++;
        }

        long keep = (long)sampleRate * keepMs / 1000;
        long trim = silent - keep;
        if (trim <= 0) return 0;

        long maxTrim = (long)sampleRate * maxTrimMs / 1000;
        long maxByRatio = (long)totalSamples * maxTrimRatioPercent / 100;
        trim = Math.Min(trim, Math.Min(maxTrim, maxByRatio));
        return trim > 0 ? trim : 0;
    }

    // ── 合成缓存（LRU）──────────────────────────────────────────────────
    // cacheKey: {0|1}|resId|Vcn|lang|style|impact|rdn|text — 0=钉选(默认) 1=可淘汰(玩家自定义昵称)
    private string CacheKey(string resId, string text, bool evictable, int rdn) =>
        $"{(evictable ? '1' : '0')}|{resId}|{Vcn}|{_languageId}|{_style}|{_impactFactor}|{rdn}|{text}";

    private bool TryGetCache(string key, out byte[] pcm)
    {
        lock (_cache)
        {
            if (_cache.TryGetValue(key, out pcm))
            {
                _cacheLru.Remove(key);
                _cacheLru.AddLast(key);
                return true;
            }
        }
        pcm = null;
        return false;
    }

    private void StoreCache(string key, byte[] pcm)
    {
        lock (_cache)
        {
            if (_cache.ContainsKey(key)) { _cacheLru.Remove(key); }
            _cache[key] = pcm;
            _cacheLru.AddLast(key);
            while (_cacheLru.Count > _cacheCapacity)
            {
                string oldest = FindOldestEvictableCacheKey();
                if (oldest == null) break;
                _cacheLru.Remove(oldest);
                _cache.Remove(oldest);
            }
        }
    }

    string FindOldestEvictableCacheKey()
    {
        foreach (var key in _cacheLru)
        {
            if (VoiceCloneDiskCache.IsEvictablePlayerVoice(key))
                return key;
        }
        return null;
    }

    /// <summary>清空合成缓存（内存 + 磁盘）。</summary>
    public void ClearCache()
    {
        lock (_cache) { _cache.Clear(); _cacheLru.Clear(); }
        _diskCache?.ClearAll();
        SetStatus("已清空合成缓存（内存+磁盘）");
    }

    /// <summary>当前内存缓存条数。</summary>
    public int CacheCount { get { lock (_cache) { return _cache.Count; } } }

    /// <summary>当前磁盘缓存条数。</summary>
    public int DiskCacheCount => _diskCache?.Count ?? 0;

    /// <summary>磁盘缓存目录（Android: /sdcard/GLSA/VoiceCloneCache）。</summary>
    public string DiskCacheDirectory => _diskCache?.RootDirectory ?? "";

    // ── 构建合成请求 JSON（标准版 vcn=x5_clone；encoding=raw 即 PCM）────
    private string BuildSynthRequest(string text, string resId)
        => BuildSynthRequest(text, resId, _synthSampleRate);

    private string BuildSynthRequest(string text, string resId, int sampleRate)
    {
        string textB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append("\"header\":{")
          .Append("\"app_id\":").Append(JsonStr(_appId)).Append(',')
          .Append("\"res_id\":").Append(JsonStr(resId)).Append(',')
          .Append("\"status\":2},");
        sb.Append("\"parameter\":{\"tts\":{")
          .Append("\"vcn\":").Append(JsonStr(Vcn)).Append(',')
          .Append("\"volume\":50,\"speed\":50,\"pitch\":50,")
          .Append("\"bgs\":0,\"reg\":0,\"rdn\":").Append(_speakRdn).Append(",\"rhy\":0,\"pybuffer\":1,");
        if (_useMultiStyle)
        {
            // 多风格版必传 languageID；中文下可选 style/impactFactor
            sb.Append("\"languageID\":").Append(_languageId).Append(',');
            if (!string.IsNullOrEmpty(_style))
            {
                sb.Append("\"style\":").Append(JsonStr(_style)).Append(',');
                sb.Append("\"impactFactor\":").Append(_impactFactor).Append(',');
            }
        }
        sb.Append("\"audio\":{\"encoding\":\"raw\",\"sample_rate\":").Append(sampleRate)
          .Append(",\"channels\":1,\"bit_depth\":16,\"frame_size\":0}")
          .Append("}},");
        sb.Append("\"payload\":{\"text\":{")
          .Append("\"encoding\":\"utf8\",\"compress\":\"raw\",\"format\":\"plain\",")
          .Append("\"status\":2,\"seq\":0,\"text\":").Append(JsonStr(textB64))
          .Append("}}");
        sb.Append('}');
        return sb.ToString();
    }

    // ── WebSocket HMAC-SHA256 URL 鉴权 ──────────────────────────────────
    private static string BuildWsAuthUrl(string wsUrl, string apiKey, string apiSecret)
    {
        var uri  = new Uri(wsUrl);
        string host = uri.Host;
        string path = uri.AbsolutePath;
        string date = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture); // RFC1123 GMT

        string signatureOrigin = $"host: {host}\ndate: {date}\nGET {path} HTTP/1.1";
        string signature;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureOrigin));
            signature = Convert.ToBase64String(hash);
        }

        string authOrigin = $"api_key=\"{apiKey}\", algorithm=\"hmac-sha256\", " +
                            $"headers=\"host date request-line\", signature=\"{signature}\"";
        string authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(authOrigin));

        return wsUrl +
               "?authorization=" + Uri.EscapeDataString(authorization) +
               "&date="          + Uri.EscapeDataString(date) +
               "&host="          + Uri.EscapeDataString(host);
    }

    // ────────────────────────────────────────────────────────────────────
    //  工具
    // ────────────────────────────────────────────────────────────────────

    private static string NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    private static string Md5(string input)
    {
        using var md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(32);
        foreach (var b in hash) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    // 最小 JSON 字符串转义（够用于 appId/resId/taskId/base64/任务名）
    private static string JsonStr(string s)
    {
        if (s == null) return "\"\"";
        var sb = new StringBuilder(s.Length + 2);
        sb.Append('"');
        foreach (char c in s)
        {
            switch (c)
            {
                case '"':  sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n");  break;
                case '\r': sb.Append("\\r");  break;
                case '\t': sb.Append("\\t");  break;
                default:   sb.Append(c);      break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    // 读取 WAV 文件头，返回一行人类可读的格式描述（用于排查日志）
    private static string DescribeWav(string path)
    {
        try
        {
            byte[] b = File.ReadAllBytes(path);
            if (b.Length < 12 ||
                Encoding.ASCII.GetString(b, 0, 4) != "RIFF" ||
                Encoding.ASCII.GetString(b, 8, 4) != "WAVE")
                return "非WAV/无法解析";

            int audioFormat = 0, channels = 0, sampleRate = 0, bits = 0, dataSize = 0;
            int pos = 12;
            while (pos + 8 <= b.Length)
            {
                string id   = Encoding.ASCII.GetString(b, pos, 4);
                uint   size = BitConverter.ToUInt32(b, pos + 4);
                int    body = pos + 8;
                if (id == "fmt " && body + 16 <= b.Length)
                {
                    audioFormat = BitConverter.ToUInt16(b, body);
                    channels    = BitConverter.ToUInt16(b, body + 2);
                    sampleRate  = (int)BitConverter.ToUInt32(b, body + 4);
                    bits        = BitConverter.ToUInt16(b, body + 14);
                }
                else if (id == "data")
                {
                    dataSize = (int)Math.Min(size, (uint)(b.Length - body));
                }
                pos = body + (int)size + ((size % 2 == 1) ? 1 : 0);
            }
            string fmtName = audioFormat == 1 ? "PCM整数" : audioFormat == 3 ? "IEEE浮点" : $"未知({audioFormat})";
            int    frameStride = Math.Max(1, (bits / 8) * Math.Max(1, channels));
            double durSec = sampleRate > 0 ? (double)(dataSize / frameStride) / sampleRate : 0;
            return $"格式={fmtName} 声道={channels} 采样率={sampleRate}Hz 位深={bits}bit 数据={dataSize}字节 时长={durSec:F2}s";
        }
        catch (Exception ex) { return $"解析失败: {ex.Message}"; }
    }

    private static string MimeOf(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".wav" => "audio/wav",
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            ".pcm" => "audio/pcm",
            _      => "application/octet-stream",
        };
    }

    // ────────────────────────────────────────────────────────────────────
    //  WAV 合规化：转为 16-bit 整数 PCM 单声道（保留原采样率，≥24k 合规）
    //  仅处理 .wav；已是 16bit/单声道 PCM 则原样返回。返回临时文件路径。
    //  非 wav（mp3/m4a/pcm）直接返回 null（不转码，按原文件上传）。
    // ────────────────────────────────────────────────────────────────────
    private static string EnsureCompliantWav(string srcPath)
    {
        if (Path.GetExtension(srcPath).ToLowerInvariant() != ".wav") return null;

        byte[] b = File.ReadAllBytes(srcPath);
        if (b.Length < 12 ||
            Encoding.ASCII.GetString(b, 0, 4) != "RIFF" ||
            Encoding.ASCII.GetString(b, 8, 4) != "WAVE")
        {
            Debug.LogWarning("[VoiceClone] 非标准 WAV，跳过转码");
            return null;
        }

        int    audioFormat = 0, channels = 0, sampleRate = 0, bits = 0;
        int    dataOffset = -1, dataSize = 0;

        // 遍历 chunk（float WAV 通常带 fact 块，不能假设 data 从 44 开始）
        int pos = 12;
        while (pos + 8 <= b.Length)
        {
            string id   = Encoding.ASCII.GetString(b, pos, 4);
            uint   size = BitConverter.ToUInt32(b, pos + 4);
            int    body = pos + 8;

            if (id == "fmt " && body + 16 <= b.Length)
            {
                audioFormat = BitConverter.ToUInt16(b, body);
                channels    = BitConverter.ToUInt16(b, body + 2);
                sampleRate  = (int)BitConverter.ToUInt32(b, body + 4);
                bits        = BitConverter.ToUInt16(b, body + 14);
            }
            else if (id == "data")
            {
                dataOffset = body;
                dataSize   = (int)Math.Min(size, (uint)(b.Length - body));
            }
            pos = body + (int)size + ((size % 2 == 1) ? 1 : 0);
        }

        if (dataOffset < 0 || channels < 1 || bits < 8 || sampleRate < 1)
        {
            Debug.LogWarning("[VoiceClone] WAV 解析失败，跳过转码");
            return null;
        }

        const int TARGET_RATE = 24000; // 讯飞要求原始音频采样率为 24k

        // 已经是 16bit 整数 PCM 单声道 24k：无需转码
        if (audioFormat == 1 && bits == 16 && channels == 1 && sampleRate == TARGET_RATE)
            return null;

        Debug.Log($"[VoiceClone] 转码: fmt={audioFormat} ch={channels} rate={sampleRate} bits={bits} → PCM16/mono/{TARGET_RATE}");

        // 解码为单声道 float[]
        int    bytesPerSample = bits / 8;
        int    frameStride    = bytesPerSample * channels;
        int    frameCount     = dataSize / frameStride;
        var    mono           = new float[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            double sum = 0;
            int baseIdx = dataOffset + i * frameStride;
            for (int c = 0; c < channels; c++)
            {
                int s = baseIdx + c * bytesPerSample;
                sum += ReadSample(b, s, audioFormat, bits);
            }
            mono[i] = (float)(sum / channels);
        }

        // 重采样到 24k（线性插值；48k→24k 为 2:1，损失极小）
        if (sampleRate != TARGET_RATE)
        {
            mono = Resample(mono, sampleRate, TARGET_RATE);
            sampleRate = TARGET_RATE;
        }

        // 编码为 16-bit PCM WAV（24k，单声道）
        string outPath = Path.Combine(
            Application.temporaryCachePath,
            Path.GetFileNameWithoutExtension(srcPath) + "_pcm16_24k.wav");
        WritePcm16Wav(outPath, mono, sampleRate);
        return outPath;
    }

    // 把不同格式的一个采样点读成 [-1,1] 的 float
    private static double ReadSample(byte[] b, int offset, int audioFormat, int bits)
    {
        if (audioFormat == 3) // IEEE float
        {
            if (bits == 32) return BitConverter.ToSingle(b, offset);
            if (bits == 64) return BitConverter.ToDouble(b, offset);
        }
        // 整数 PCM
        switch (bits)
        {
            case 8:  return (b[offset] - 128) / 128.0;                       // 无符号
            case 16: return BitConverter.ToInt16(b, offset) / 32768.0;
            case 24:
            {
                int v = (b[offset]) | (b[offset + 1] << 8) | (b[offset + 2] << 16);
                if ((v & 0x800000) != 0) v |= unchecked((int)0xFF000000);    // 符号扩展
                return v / 8388608.0;
            }
            case 32: return BitConverter.ToInt32(b, offset) / 2147483648.0;
        }
        return 0;
    }

    // 线性插值重采样（单声道）
    private static float[] Resample(float[] src, int srcRate, int dstRate)
    {
        if (src.Length == 0 || srcRate == dstRate) return src;
        long outLen = (long)src.Length * dstRate / srcRate;
        var dst = new float[outLen];
        double step = (double)srcRate / dstRate;
        for (long i = 0; i < outLen; i++)
        {
            double srcPos = i * step;
            int    i0     = (int)srcPos;
            double frac   = srcPos - i0;
            float  s0     = src[i0];
            float  s1     = (i0 + 1 < src.Length) ? src[i0 + 1] : s0;
            dst[i] = (float)(s0 * (1.0 - frac) + s1 * frac);
        }
        return dst;
    }

    private static byte[] DownsamplePcm16(byte[] pcm16, int srcRate, int dstRate)
    {
        if (pcm16 == null || pcm16.Length < 2 || srcRate == dstRate) return pcm16;

        int sampleCount = pcm16.Length / 2;
        var floats = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
            floats[i] = BitConverter.ToInt16(pcm16, i * 2) / 32768f;

        float[] resampled = Resample(floats, srcRate, dstRate);
        var outBytes = new byte[resampled.Length * 2];
        for (int i = 0; i < resampled.Length; i++)
        {
            short s = (short)Mathf.Clamp(Mathf.RoundToInt(resampled[i] * 32767f), short.MinValue, short.MaxValue);
            outBytes[i * 2]     = (byte)(s & 0xFF);
            outBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }
        return outBytes;
    }

    private static byte[] TrimTrailingSilenceBytes(byte[] pcm16, int sampleRate,
        float threshold = 0.004f, int keepMs = 150, int maxTrimMs = 120, int maxTrimRatioPercent = 12)
    {
        if (pcm16 == null || pcm16.Length < 2) return pcm16;

        long trimSamples = CountTrailingSilenceSamples(
            pcm16, sampleRate, threshold, keepMs, maxTrimMs, maxTrimRatioPercent);
        if (trimSamples <= 0) return pcm16;

        int trimBytes = (int)(trimSamples * 2);
        int newLen = pcm16.Length - trimBytes;
        if (newLen < 2) return pcm16;

        var dst = new byte[newLen];
        Array.Copy(pcm16, 0, dst, 0, newLen);
        return dst;
    }

    // 导出：定位最后一个有效样本 → 保留短尾音 → cosine 淡出 → 保留 ≥500ms 尾部静音。
    private static byte[] FinalizeEmbeddedExportPcm(byte[] pcm16, int sampleRate,
        float activeThreshold = 0.004f)
    {
        if (pcm16 == null || pcm16.Length < 2) return pcm16;

        int totalSamples = pcm16.Length / 2;
        int limit = (int)(activeThreshold * 32768f);
        int lastActive = -1;
        for (int i = totalSamples - 1; i >= 0; i--)
        {
            short s = (short)(pcm16[i * 2] | (pcm16[i * 2 + 1] << 8));
            if (Math.Abs((int)s) > limit)
            {
                lastActive = i;
                break;
            }
        }

        int minSilenceSamples = Math.Max(1, sampleRate * EmbeddedExportMinTrailingSilenceMs / 1000);
        int maxSilenceSamples = Math.Max(minSilenceSamples, sampleRate * EmbeddedExportMaxTrailingSilenceMs / 1000);
        if (lastActive < 0)
        {
            return new byte[minSilenceSamples * 2];
        }

        int releaseSamples = Math.Max(1, sampleRate * EmbeddedExportReleaseMs / 1000);
        int contentEnd = Math.Min(totalSamples, lastActive + 1 + releaseSamples);

        int fadeSamples = Math.Max(1, sampleRate * EmbeddedExportFadeOutMs / 1000);
        fadeSamples = Math.Min(fadeSamples, contentEnd);

        int sourceTrailing = Math.Max(0, totalSamples - contentEnd);
        int silenceSamples = Math.Max(minSilenceSamples, Math.Min(sourceTrailing, maxSilenceSamples));

        var dst = new byte[(contentEnd + silenceSamples) * 2];
        Array.Copy(pcm16, 0, dst, 0, contentEnd * 2);

        for (int i = 0; i < fadeSamples; i++)
        {
            int idx = contentEnd - fadeSamples + i;
            short s = (short)(dst[idx * 2] | (dst[idx * 2 + 1] << 8));
            float t = (float)(i + 1) / fadeSamples;
            float gain = 0.5f * (1f + Mathf.Cos(Mathf.PI * t));
            short outS = (short)Mathf.Clamp(Mathf.RoundToInt(s * gain), short.MinValue, short.MaxValue);
            dst[idx * 2]     = (byte)(outS & 0xFF);
            dst[idx * 2 + 1] = (byte)((outS >> 8) & 0xFF);
        }

        int copySilence = Math.Min(sourceTrailing, silenceSamples);
        if (copySilence > 0)
            Array.Copy(pcm16, contentEnd * 2, dst, contentEnd * 2, copySilence * 2);

        return dst;
    }

    /// <summary>
    /// 把最近一次合成（或缓存命中）的完整音频导出为 WAV 文件（16bit/单声道/24k）。
    /// 合成结果本身是 raw PCM，WAV 无损且任意播放器可放；运行时无 mp3 编码器，故保存为 wav。
    /// </summary>
    /// <returns>成功返回 true；失败 error 带原因。</returns>
    public bool TrySaveLastAudioWav(string fullPath, out string error)
    {
        error = null;
        byte[] pcm = _lastFullPcm;
        if (pcm == null || pcm.Length < 2)
        {
            error = "没有可保存的音频（请先成功合成一次）";
            return false;
        }
        try
        {
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            WritePcm16WavFromBytes(fullPath, pcm, _synthSampleRate);
            Debug.Log($"[VoiceClone] 音频已导出: {fullPath}（{pcm.Length} 字节 PCM）");
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    // 直接把 16bit LE PCM 字节写成 WAV（不做 float 转换，原样落盘）
    private static void WritePcm16WavFromBytes(string path, byte[] pcm16, int sampleRate)
    {
        int dataBytes = pcm16.Length;
        int byteRate  = sampleRate * 2;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var w  = new BinaryWriter(fs);

        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataBytes);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);           // PCM
        w.Write((short)1);           // mono
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)2);           // block align
        w.Write((short)16);          // bits per sample
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataBytes);
        w.Write(pcm16);
    }

    private static void WritePcm16Wav(string path, float[] mono, int sampleRate)
    {
        int    sampleCount = mono.Length;
        int    dataBytes   = sampleCount * 2;
        int    byteRate    = sampleRate * 2;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var w  = new BinaryWriter(fs);

        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataBytes);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);                 // fmt chunk size
        w.Write((short)1);           // PCM
        w.Write((short)1);           // mono
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)2);           // block align
        w.Write((short)16);          // bits per sample
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataBytes);

        for (int i = 0; i < sampleCount; i++)
        {
            float f = mono[i];
            if (f > 1f) f = 1f; else if (f < -1f) f = -1f;
            w.Write((short)Mathf.RoundToInt(f * 32767f));
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  线程安全 PCM 流缓冲：worker 线程入队 16bit PCM，音频线程出队 float
    // ────────────────────────────────────────────────────────────────────
    private sealed class PcmStreamBuffer
    {
        private readonly object         _lock   = new();
        private readonly Queue<float[]> _chunks = new();
        private float[] _head;
        private int     _headPos;
        private long    _enq;
        private long    _read;
        private long    _trailingSilence;   // 尾部静音样本数：Drained 判定时从 _enq 扣除

        public volatile bool InputDone;

        public void Reset()
        {
            lock (_lock)
            {
                _chunks.Clear();
                _head = null; _headPos = 0;
                _enq = 0; _read = 0;
                _trailingSilence = 0;
                InputDone = false;
            }
        }

        /// <summary>设置尾部静音样本数；播放读到有效末尾即视为 Drained，随后由上层 Stop 截断静音。</summary>
        public void SetTrailingSilence(long samples)
        {
            lock (_lock) { _trailingSilence = Math.Max(0, samples); }
        }

        public void EnqueueBytes(byte[] pcm16)
        {
            int n = pcm16.Length / 2;
            if (n == 0) return;
            var f = new float[n];
            for (int i = 0; i < n; i++)
            {
                short s = (short)(pcm16[2 * i] | (pcm16[2 * i + 1] << 8));
                f[i] = s / 32768f;
            }
            lock (_lock) { _chunks.Enqueue(f); _enq += n; }
        }

        // 音频线程调用：尽量填满 data，欠载时补静音（避免播放中断）
        public void Read(float[] data)
        {
            int i = 0;
            lock (_lock)
            {
                while (i < data.Length)
                {
                    if (_head == null || _headPos >= _head.Length)
                    {
                        if (_chunks.Count == 0) break;
                        _head = _chunks.Dequeue();
                        _headPos = 0;
                    }
                    int c = Math.Min(data.Length - i, _head.Length - _headPos);
                    Array.Copy(_head, _headPos, data, i, c);
                    _headPos += c; i += c; _read += c;
                }
            }
            for (; i < data.Length; i++) data[i] = 0f;
        }

        // 已入队但尚未读取的真实样本数（用于开播前预缓冲判定）
        public long BufferedSamples
        {
            get { lock (_lock) { return _enq - _read; } }
        }

        public long ReadSamples => _read;
        public long EnqueuedSamples => _enq;
        public long TrailingSilenceSamples => _trailingSilence;

        // 输入结束且已读完有效样本（扣除尾部静音）
        public bool Drained
        {
            get { lock (_lock) { return InputDone && _read >= (_enq - _trailingSilence); } }
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  JSON 反序列化模型
    // ────────────────────────────────────────────────────────────────────

    [Serializable] private class XFTokenResp
    {
        public string retcode;
        public string accesstoken;
        public string expiresin;
    }

    [Serializable] private class XFCommonResp
    {
        public bool   flag;
        public int    code;
        public string desc;
        public string data;   // /task/add 返回 taskId
    }

    [Serializable] private class XFResultResp
    {
        public bool          flag;
        public int           code;
        public string        desc;
        public XFResultData  data;
    }

    [Serializable] private class XFResultData
    {
        public string taskName;
        public string resourceName;
        public string trainVid;
        public string assetId;     // 训练得到的音色 id（res_id）
        public string trainId;
        public string appId;
        public int    trainStatus; // -1 训练中, 1 成功, 0 失败, 2 草稿
        public string failedDesc;
    }

    [Serializable] private class WsResp
    {
        public WsHeader  header;
        public WsPayload payload;
    }

    [Serializable] private class WsHeader
    {
        public int    code;
        public string message;
        public string sid;
        public int    status;
    }

    [Serializable] private class WsPayload
    {
        public WsAudio audio;
    }

    [Serializable] private class WsAudio
    {
        public string encoding;
        public int    sample_rate;
        public int    status;
        public int    seq;
        public string audio;   // base64
    }
}
