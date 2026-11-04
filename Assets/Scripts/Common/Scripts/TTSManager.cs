#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.IO;
using System.Threading;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TTS 引擎类型（仅 Android 生效；Windows 始终走 SAPI）。
/// </summary>
public enum TtsEngine
{
    /// <summary>
    /// 普通在线 TTS：科大讯飞 Msc.jar，发音人 xiaoyan/aisjinger 等。
    /// 走 <see cref="XFTTSPlayer"/>。
    /// </summary>
    Standard,

    /// <summary>
    /// 超拟人 TTS：科大讯飞 SparkChain SDK PersonateTTS，
    /// 发音人 x4_lingxiaoxuan_oral / x4_lingfeiyi_oral / x5_lingyuyan_flow 等，
    /// 发音更自然，可选大模型口语化处理。走 <see cref="XFPersonateTTSPlayer"/>。
    /// </summary>
    Personate,
}

/// <summary>
/// 跨平台 TTS 语音合成管理器（高层封装）。
///
/// 平台路由：
///   Android（非 Editor）        → 科大讯飞，可在 <see cref="TtsEngine.Standard"/>（普通）
///                                 与 <see cref="TtsEngine.Personate"/>（超拟人 SparkChain）之间动态切换
///   Windows PC / Windows Editor → 持久化 PowerShell 进程调用 System.Speech（无需额外运行时）
///   其他 Editor（macOS / Linux） → 日志模拟（不发声，但同步触发事件）
///
/// 输出模式（<see cref="TtsOutputMode"/>）：
///   <see cref="TtsOutputMode.UnityAudio"/>   → Android AudioTrack / Windows 系统默认音频设备
///   <see cref="TtsOutputMode.ExternalAlsa"/> → Android ALSA 指定声卡
///                                              Windows：先合成 WAV 文件 → 解析 PCM → ExternalDeviceAudioPlayer 路由
///
/// Windows 初始化说明：
///   首次使用时会启动一个后台 PowerShell 进程（约 2~3 秒预热），
///   预热完成后每次播报延迟 &lt; 100ms。
///   调用 Stop() 会终止该进程，下次 Speak() 将重新启动。
///
/// <example>
///   TTSManager.Instance.Speak("欢迎光临");
///
///   // 切换为超拟人引擎
///   TTSManager.Instance.Engine = TtsEngine.Personate;
///   TTSManager.Instance.SetVoiceName("x4_lingxiaoxuan_oral");
///   TTSManager.Instance.Speak("超拟人语音合成测试");
///
///   TTSManager.Instance.OutputMode = TtsOutputMode.ExternalAlsa;
///   TTSManager.Instance.ExternalDeviceSelector = "hdmi0";
///   TTSManager.Instance.Speak("战斗开始");
///
///   var voices = TTSManager.GetVoiceNames(); // Windows: 已安装 SAPI 语音名列表
/// </example>
/// </summary>
public class TTSManager : MonoBehaviour
{
    // ────────────────────────────────────────────────────────────────────
    //  Inspector 参数
    // ────────────────────────────────────────────────────────────────────

    [Header("TTS 通用参数")]
    [Tooltip("Android 讯飞发音人（xiaoyan/aisjinger/aisbabyxu...）；Windows 端请填写下方 _windowsVoiceName")]
    [SerializeField] private string _voiceName = "xiaoyan";

    [Tooltip("语速 0~100（Android 原始值；Windows 映射到 SAPI Rate -10~10）")]
    [SerializeField, Range(0, 100)] private int _speed = 60;

    [Tooltip("音量 0~100")]
    [SerializeField, Range(0, 100)] private int _volume = 100;

    [Header("引擎选择（仅 Android 生效）")]
    [Tooltip("Standard=普通在线 TTS（Msc.jar，xiaoyan 等）；Personate=超拟人 TTS（SparkChain，x4_*_oral 等，质量更高）")]
    [SerializeField] private TtsEngine _engine = TtsEngine.Standard;

    [Header("输出模式")]
    [Tooltip("UnityAudio=默认声卡；ExternalAlsa=指定外置声卡")]
    [SerializeField] private TtsOutputMode _outputMode = TtsOutputMode.UnityAudio;

    [Tooltip("指定声卡关键词（Android=ALSA 名称关键词如 \"hdmi0\"；Windows=ExternalDeviceAudioPlayer 设备选择子）")]
    [SerializeField] private string _externalDeviceSelector = "";

    [Tooltip("ExternalAlsa 模式播放 key（相同 key 会替换旧实例，建议固定为 \"tts_voice\"）")]
    [SerializeField] private string _externalAudioKey = "tts_voice";

    [Header("Windows SAPI（仅 PC / Windows Editor 生效）")]
    [Tooltip("语音名称关键词，留空则使用系统默认语音\n已知可用：Microsoft Huihui Desktop（惠惠，普通话女）\n         Microsoft Yaoyao Desktop（瑶瑶，普通话女）\n         Microsoft Kangkang Desktop（康康，普通话男）\n若无中文语音请在 系统设置→时间和语言→语音 中安装")]
    [SerializeField] private string _windowsVoiceName = "";

    // ────────────────────────────────────────────────────────────────────
    //  单例
    // ────────────────────────────────────────────────────────────────────

    private static TTSManager _instance;

    /// <summary>自动创建单例，首次访问若场景中不存在则自动生成 GameObject。</summary>
    public static TTSManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[TTSManager]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<TTSManager>();
            return _instance;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开属性
    // ────────────────────────────────────────────────────────────────────

    /// <summary>当前输出模式，可在运行时动态切换。</summary>
    public TtsOutputMode OutputMode
    {
        get => _outputMode;
        set => _outputMode = value;
    }

    /// <summary>
    /// 当前 TTS 引擎（仅 Android 生效；Windows 始终走 SAPI）。
    /// 切换后下次 <see cref="Speak"/> 会路由到对应实现。
    /// </summary>
    public TtsEngine Engine
    {
        get => _engine;
        set => _engine = value;
    }

    /// <summary>
    /// 外置设备选择子。
    /// Android：ALSA 名称关键词（如 "hdmi0"）。
    /// Windows：ExternalDeviceAudioPlayer 设备选择子（ExternalAlsa 模式下将 PCM 路由到此设备）。
    /// </summary>
    public string ExternalDeviceSelector
    {
        // 未显式设置时，自动回退到当前开发板的耳机声卡（BoardProfile）。
        get => string.IsNullOrEmpty(_externalDeviceSelector)
            ? BoardProfile.AudioCardSelector
            : _externalDeviceSelector;
        set => _externalDeviceSelector = value;
    }

    /// <summary>当前是否正在播报。</summary>
    public bool IsSpeaking { get; private set; }

    // ────────────────────────────────────────────────────────────────────
    //  公开事件
    // ────────────────────────────────────────────────────────────────────

    /// <summary>开始播报时触发。</summary>
    public event Action OnSpeakBegin;

    /// <summary>播报进度（0~100），Android 专用（Windows / mock 不触发）。</summary>
    public event Action<int> OnProgress;

    /// <summary>播报正常完成时触发。</summary>
    public event Action OnCompleted;

    /// <summary>发生错误时触发，参数为错误描述字符串。</summary>
    public event Action<string> OnError;

    // ────────────────────────────────────────────────────────────────────
    //  内部状态
    // ────────────────────────────────────────────────────────────────────

    private bool _initialized;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    // 持久化 PowerShell 进程（UnityAudio 模式复用）
    private Process      _psProcess;
    private StreamWriter _psInput;
    private StreamReader _psOutput;

    // 从后台线程派发回主线程的回调（volatile 保证可见性）
    private volatile Action _pendingMainThreadAction;

    // PowerShell 脚本路径（进程退出后删除）
    private string _psScriptPath;

    // 持久化脚本：从 stdin 接收命令，循环处理
    //
    // 注意：脚本里禁止出现以 # 开头（含前导空白）的独立注释行。
    // 因为整段 const 处于 #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN 块内，
    // 当在 Android 等平台构建时该 #if 为 false，C# 进入「非活动代码」扫描模式，
    // 此时 @"..." 不再被识别为字符串，列 1 起的 `#` 会被当成预处理指令（CS1024）。
    // C# 规范允许 # 前有任意空白，所以"加空格"不能绕过；正确做法是把 # 注释
    // 挪到语句行尾，使行首是 `[`、`$` 等非 # 字符。
    private const string PS_DAEMON_SCRIPT = @"
[Console]::InputEncoding  = [System.Text.Encoding]::UTF8   # 设置 stdin 为 UTF-8，确保中文字符正确传输
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8   # 设置 stdout 为 UTF-8
Add-Type -AssemblyName System.Speech
$s = New-Object System.Speech.Synthesis.SpeechSynthesizer
$zhVoice = $s.GetInstalledVoices() | Where-Object { $_.VoiceInfo.Culture.Name -like 'zh-*' } | Select-Object -First 1   # 自动选第一个中文语音
if ($zhVoice) { $s.SelectVoice($zhVoice.VoiceInfo.Name) }
[Console]::Out.WriteLine('RDY')
[Console]::Out.Flush()
while ($true) {
    $line = [Console]::In.ReadLine()
    if ($null -eq $line -or $line -eq 'BYE') { break }
    $sp   = $line.IndexOf(' ')
    $cmd  = if ($sp -ge 0) { $line.Substring(0, $sp) } else { $line }
    $arg  = if ($sp -ge 0) { $line.Substring($sp + 1) } else { '' }
    switch ($cmd) {
        'SPEAK' {
            $s.Speak($arg)
            [Console]::Out.WriteLine('DONE')
            [Console]::Out.Flush()
        }
        'RATE'  { try { $s.Rate   = [int]$arg  } catch {} }
        'VOL'   { try { $s.Volume = [int]$arg  } catch {} }
        'VOICE' { try { $s.SelectVoice($arg)   } catch {} }
    }
}
";
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
    }

    private void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var action = _pendingMainThreadAction;
        if (action != null)
        {
            _pendingMainThreadAction = null;
            action();
        }
#endif
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        UnsubscribeAndroid();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        KillPsProcess();
#endif
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开方法
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 初始化 TTS 引擎（第一次调用 <see cref="Speak"/> 时自动触发）。
    /// Windows：启动后台 PowerShell 进程，约 2~3 秒预热后 <see cref="IsSpeaking"/> 才可用。
    /// </summary>
    public void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        InitAndroid();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        InitPsDaemon();
#else
        _initialized = true;
        Debug.Log("[TTSManager] Init (mock): 非 Android/Windows 平台，使用日志模拟");
#endif
    }

    /// <summary>
    /// 合成并播报文字。
    /// Windows：若进程尚未预热，会自动触发初始化并在就绪后播报。
    /// </summary>
    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[TTSManager] Speak: text 为空");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!_initialized) { Debug.LogWarning("[TTSManager] Speak: 尚未初始化"); return; }
        SpeakAndroid(text);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // 自动懒初始化
        if (!_initialized)
        {
            Init();
            // 等待 PS 进程就绪后再播
            string pendingText = text;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                int waited = 0;
                while (!_initialized && waited < 8000)
                {
                    Thread.Sleep(100);
                    waited += 100;
                }
                _pendingMainThreadAction = () =>
                {
                    if (_initialized) SpeakWindows(pendingText);
                    else Debug.LogError("[TTSManager] Speak: 等待 PowerShell 就绪超时");
                };
            });
            return;
        }
        SpeakWindows(text);
#else
        Debug.Log($"[TTSManager] Speak (mock, mode={_outputMode}): {text}");
        IsSpeaking = true;
        OnSpeakBegin?.Invoke();
        IsSpeaking = false;
        OnCompleted?.Invoke();
#endif
    }

    /// <summary>停止当前播报（Windows：终止 PS 进程，下次 Speak 将重新启动）。</summary>
    public void Stop()
    {
        IsSpeaking = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        // 同时停止两个引擎，避免引擎切换时上一次播报继续
        XFTTSPlayer.Instance.Stop();
        XFPersonateTTSPlayer.Instance.Stop();
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        KillPsProcess();
        Debug.Log("[TTSManager] Stop: PS 进程已终止，下次播报时自动重启");
#else
        Debug.Log("[TTSManager] Stop (mock)");
#endif
    }

    /// <summary>动态修改发音人（Android：讯飞发音人名；Windows：SAPI 语音名关键词）。</summary>
    public void SetVoiceName(string voiceName)
    {
        _voiceName = voiceName;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_engine == TtsEngine.Personate)
            XFPersonateTTSPlayer.Instance.SetVoiceName(voiceName);
        else
            XFTTSPlayer.Instance.SetVoiceName(voiceName);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        _windowsVoiceName = voiceName;
        SendPsCommand($"VOICE {voiceName}");
#endif
    }

    /// <summary>动态修改语速（0~100），无需重新 Init。</summary>
    public void SetSpeed(int speed)
    {
        _speed = Mathf.Clamp(speed, 0, 100);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_engine == TtsEngine.Personate)
            XFPersonateTTSPlayer.Instance.SetSpeed(_speed);
        else
            XFTTSPlayer.Instance.SetSpeed(_speed);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        SendPsCommand($"RATE {SpeedToSapiRate(_speed)}");
#endif
    }

    /// <summary>动态修改音量（0~100），无需重新 Init。</summary>
    public void SetVolume(int volume)
    {
        _volume = Mathf.Clamp(volume, 0, 100);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_engine == TtsEngine.Personate)
            XFPersonateTTSPlayer.Instance.SetVolume(_volume);
        else
            XFTTSPlayer.Instance.SetVolume(_volume);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        SendPsCommand($"VOL {_volume}");
#endif
    }

    /// <summary>
    /// 获取可用语音列表（Windows：快速调用 PowerShell 枚举 SAPI 已安装语音；其他平台返回空列表）。
    /// 此方法会阻塞最多 5 秒，请在非主线程或可接受延迟的场景下调用。
    /// </summary>
    public static List<string> GetVoiceNames()
    {
        var result = new List<string>();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName               = "powershell",
                Arguments              = "-NoProfile -NonInteractive -Command \"Add-Type -AssemblyName System.Speech; (New-Object System.Speech.Synthesis.SpeechSynthesizer).GetInstalledVoices() | ForEach-Object { $_.VoiceInfo.Name }\"",
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                CreateNoWindow         = true
            };
            using var p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(5000);
            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                result.Add(line.Trim());
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TTSManager] GetVoiceNames 失败: {ex.Message}");
        }
#endif
        return result;
    }

    /// <summary>获取可用音频输出设备列表（由 ExternalDeviceAudioPlayer 提供，此处返回空列表）。</summary>
    public static List<string> GetAudioOutputs() => new List<string>();

    // ────────────────────────────────────────────────────────────────────
    //  Android 实现（代理到 XFTTSPlayer / XFPersonateTTSPlayer）
    // ────────────────────────────────────────────────────────────────────

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool _androidSubscribed;

    private void InitAndroid()
    {
        // 同时初始化两个引擎，运行时按 Engine 字段路由（双订阅，事件互不冲突）
        var xfStd      = XFTTSPlayer.Instance;
        var xfPersonate = XFPersonateTTSPlayer.Instance;

        xfStd.OutputMode              = _outputMode;
        xfStd.ExternalDeviceSelector  = ExternalDeviceSelector;
        xfPersonate.OutputMode             = _outputMode;
        xfPersonate.ExternalDeviceSelector = ExternalDeviceSelector;

        if (!_androidSubscribed)
        {
            xfStd.OnSpeakBegin += HandleAndroidSpeakBegin;
            xfStd.OnProgress   += HandleAndroidProgress;
            xfStd.OnCompleted  += HandleAndroidCompleted;
            xfStd.OnError      += HandleAndroidError;

            xfPersonate.OnSpeakBegin += HandleAndroidSpeakBegin;
            xfPersonate.OnProgress   += HandleAndroidProgress;
            xfPersonate.OnCompleted  += HandleAndroidCompleted;
            xfPersonate.OnError      += HandleAndroidError;

            _androidSubscribed = true;
        }
        _initialized = true;
        Debug.Log($"[TTSManager] Init (Android): engine={_engine}");
    }

    private void SpeakAndroid(string text)
    {
        if (_engine == TtsEngine.Personate)
        {
            var xf = XFPersonateTTSPlayer.Instance;
            xf.OutputMode             = _outputMode;
            xf.ExternalDeviceSelector = ExternalDeviceSelector;
            xf.Speak(text);
        }
        else
        {
            var xf = XFTTSPlayer.Instance;
            xf.OutputMode             = _outputMode;
            xf.ExternalDeviceSelector = ExternalDeviceSelector;
            xf.Speak(text);
        }
    }

    private void UnsubscribeAndroid()
    {
        if (!_androidSubscribed) return;

        if (_instance != null)
        {
            var xfStd = XFTTSPlayer.Instance;
            if (xfStd != null)
            {
                xfStd.OnSpeakBegin -= HandleAndroidSpeakBegin;
                xfStd.OnProgress   -= HandleAndroidProgress;
                xfStd.OnCompleted  -= HandleAndroidCompleted;
                xfStd.OnError      -= HandleAndroidError;
            }
            var xfPersonate = XFPersonateTTSPlayer.Instance;
            if (xfPersonate != null)
            {
                xfPersonate.OnSpeakBegin -= HandleAndroidSpeakBegin;
                xfPersonate.OnProgress   -= HandleAndroidProgress;
                xfPersonate.OnCompleted  -= HandleAndroidCompleted;
                xfPersonate.OnError      -= HandleAndroidError;
            }
        }
    }

    private void HandleAndroidSpeakBegin()          { IsSpeaking = true;  OnSpeakBegin?.Invoke(); }
    private void HandleAndroidProgress(int percent) => OnProgress?.Invoke(percent);
    private void HandleAndroidCompleted()           { IsSpeaking = false; OnCompleted?.Invoke(); }
    private void HandleAndroidError(string msg)     { IsSpeaking = false; OnError?.Invoke(msg); }
#endif

    // ────────────────────────────────────────────────────────────────────
    //  Windows 实现：持久化 PowerShell 进程
    // ────────────────────────────────────────────────────────────────────

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    private void InitPsDaemon()
    {
        if (_psProcess != null && !_psProcess.HasExited)
        {
            Debug.Log("[TTSManager] PS 进程已在运行，跳过重新初始化");
            return;
        }

        try
        {
            // 写脚本到临时文件（避免命令行长度限制）
            _psScriptPath = Path.Combine(Path.GetTempPath(), $"ttsmgr_{GetInstanceID()}.ps1");
            File.WriteAllText(_psScriptPath, PS_DAEMON_SCRIPT, System.Text.Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName               = "powershell",
                Arguments              = $"-NoProfile -NonInteractive -File \"{_psScriptPath}\"",
                UseShellExecute        = false,
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };
            _psProcess = Process.Start(psi);
            // 使用 UTF-8（无 BOM）写入 stdin，确保中文字符正确送达 PowerShell
            _psInput   = new StreamWriter(
                _psProcess.StandardInput.BaseStream,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };
            _psOutput  = _psProcess.StandardOutput;

            // 预先写入初始设置（脚本进入 while 循环后会读取这些命令）
            SendPsCommand($"RATE {SpeedToSapiRate(_speed)}");
            SendPsCommand($"VOL {_volume}");
            if (!string.IsNullOrEmpty(_windowsVoiceName))
                SendPsCommand($"VOICE {_windowsVoiceName}");

            // 后台等待 "RDY"
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    StreamReader reader = _psOutput;
                    while (reader != null)
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;
                        if (line.Trim() == "RDY")
                        {
                            _pendingMainThreadAction = () =>
                            {
                                _initialized = true;
                                Debug.Log("[TTSManager] Windows SAPI (PowerShell) 就绪");
                            };
                            return;
                        }
                    }
                    _pendingMainThreadAction = () =>
                        Debug.LogError("[TTSManager] 等待 PowerShell 就绪失败：进程提前退出");
                }
                catch (Exception ex)
                {
                    _pendingMainThreadAction = () =>
                        Debug.LogError($"[TTSManager] 等待 RDY 时异常: {ex.Message}");
                }
            });

            Debug.Log("[TTSManager] PowerShell 进程已启动，正在预热...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TTSManager] InitPsDaemon 失败: {ex}");
        }
    }

    private void SpeakWindows(string text)
    {
        if (_outputMode == TtsOutputMode.ExternalAlsa)
            SpeakToExternalDevice(text);
        else
            SpeakDefaultDevice(text);
    }

    private void SpeakDefaultDevice(string text)
    {
        // 保证 PS 进程存活（Stop 后进程会被 Kill）
        if (_psProcess == null || _psProcess.HasExited)
        {
            _initialized = false;
            Speak(text); // 触发懒初始化 + 排队
            return;
        }

        IsSpeaking = true;
        OnSpeakBegin?.Invoke();

        // 发送 SPEAK 命令（PS 脚本中 Speak() 是阻塞调用，完成后输出 "DONE"）
        SendPsCommand($"SPEAK {text}");

        // 后台线程等待 "DONE"
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                StreamReader reader = _psOutput;
                while (reader != null)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    if (line.Trim() == "DONE")
                    {
                        _pendingMainThreadAction = () =>
                        {
                            IsSpeaking = false;
                            OnCompleted?.Invoke();
                        };
                        return;
                    }
                }
            }
            catch
            {
                // 进程被 Kill（Stop）时 ReadLine 抛出异常，静默忽略
            }
            _pendingMainThreadAction = () => { IsSpeaking = false; };
        });
    }

    /// <summary>
    /// ExternalAlsa 模式（Windows）：
    /// 启动临时 PS 进程合成 WAV → 解析 PCM → ExternalDeviceAudioPlayer 路由。
    /// </summary>
    private void SpeakToExternalDevice(string text)
    {
        int    rate      = SpeedToSapiRate(_speed);
        int    vol       = _volume;
        string voice     = _windowsVoiceName;
        string deviceSel = ExternalDeviceSelector;
        string audioKey  = _externalAudioKey;

        string wavPath    = Path.Combine(Path.GetTempPath(), $"tts_{Guid.NewGuid():N}.wav");
        string scriptPath = Path.Combine(Path.GetTempPath(), $"tts_{Guid.NewGuid():N}.ps1");
        string voiceCmd   = string.IsNullOrEmpty(voice)
            ? ""
            : $"try {{ $s.SelectVoice('{voice.Replace("'", "''")}') }} catch {{}}";

        string script = $@"
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
Add-Type -AssemblyName System.Speech
$s = New-Object System.Speech.Synthesis.SpeechSynthesizer
$s.Rate   = {rate}
$s.Volume = {vol}
{voiceCmd}
$s.SetOutputToWaveFile('{wavPath.Replace("\\", "\\\\")}')
$s.Speak([Console]::In.ReadToEnd())
";
        File.WriteAllText(scriptPath, script, System.Text.Encoding.UTF8);

        IsSpeaking = true;
        OnSpeakBegin?.Invoke();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName              = "powershell",
                    Arguments             = $"-NoProfile -NonInteractive -File \"{scriptPath}\"",
                    UseShellExecute       = false,
                    RedirectStandardInput = true,
                    CreateNoWindow        = true
                };
                using var p = Process.Start(psi);
                using var utf8In = new StreamWriter(
                    p.StandardInput.BaseStream,
                    new System.Text.UTF8Encoding(false));
                utf8In.Write(text);
                utf8In.Close();
                p.WaitForExit(15000);

                byte[] wav = File.ReadAllBytes(wavPath);
                var (sr, ch, offset) = ParseWavHeader(wav);
                int pcmLen = wav.Length - offset;
                if (pcmLen <= 0) throw new InvalidDataException("WAV PCM 数据为空");

                byte[] pcm = new byte[pcmLen];
                Array.Copy(wav, offset, pcm, 0, pcmLen);
                Debug.Log($"[TTSManager] ExternalAlsa WAV ready: {pcm.Length} bytes {sr}Hz ch={ch} → {deviceSel}");

                _pendingMainThreadAction = () =>
                {
                    try
                    {
                        ExternalDeviceAudioPlayer.Play(deviceSel, audioKey, pcm, sr, ch);
                        IsSpeaking = false;
                        OnCompleted?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        IsSpeaking = false;
                        OnError?.Invoke(ex.Message);
                    }
                    CleanupTempFiles(wavPath, scriptPath);
                };
            }
            catch (Exception ex)
            {
                _pendingMainThreadAction = () =>
                {
                    IsSpeaking = false;
                    Debug.LogError($"[TTSManager] ExternalAlsa 失败: {ex.Message}");
                    OnError?.Invoke(ex.Message);
                };
                CleanupTempFiles(wavPath, scriptPath);
            }
        });
    }

    private void KillPsProcess()
    {
        _initialized = false;
        if (_psProcess == null) return;
        try
        {
            if (!_psProcess.HasExited)
            {
                SendPsCommand("BYE");
                if (!_psProcess.WaitForExit(500))
                    _psProcess.Kill();
            }
            _psProcess.Dispose();
        }
        catch { /* ignore */ }
        finally
        {
            _psProcess = null;
            _psInput   = null;
            _psOutput  = null;
            if (_psScriptPath != null)
            {
                try { File.Delete(_psScriptPath); } catch { }
                _psScriptPath = null;
            }
        }
    }

    private void SendPsCommand(string cmd)
    {
        try { _psInput?.WriteLine(cmd); _psInput?.Flush(); }
        catch { /* PS 进程可能已退出 */ }
    }

    /// <summary>将 0~100 语速映射到 SAPI Rate -10~10。</summary>
    private static int SpeedToSapiRate(int speed0to100)
        => Mathf.RoundToInt(speed0to100 / 100f * 20f) - 10;

    /// <summary>
    /// 解析 WAV 文件头，返回采样率、声道数及 PCM 数据起始偏移。
    /// 兼容含额外 chunk 的 WAV 文件。
    /// </summary>
    private static (int sampleRate, int channels, int dataOffset) ParseWavHeader(byte[] wav)
    {
        if (wav.Length < 44)
            throw new InvalidDataException($"WAV 数据过短: {wav.Length} bytes");

        int channels   = BitConverter.ToInt16(wav, 22);
        int sampleRate = BitConverter.ToInt32(wav, 24);

        // 扫描 "data" 子块（跳过 fmt、fact、LIST 等额外 chunk）
        int pos = 12;
        while (pos + 8 <= wav.Length)
        {
            string tag       = System.Text.Encoding.ASCII.GetString(wav, pos, 4);
            int    chunkSize = BitConverter.ToInt32(wav, pos + 4);
            if (tag == "data") return (sampleRate, channels, pos + 8);
            pos += 8 + chunkSize;
        }
        return (sampleRate, channels, 44); // 保底 offset
    }

    private static void CleanupTempFiles(params string[] paths)
    {
        foreach (var p in paths)
        {
            try { if (File.Exists(p)) File.Delete(p); } catch { }
        }
    }

#endif
}
