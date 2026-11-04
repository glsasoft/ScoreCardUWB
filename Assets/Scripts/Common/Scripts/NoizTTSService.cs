using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Noiz 云端 TTS 语音合成服务（异步 HTTP 方式）。
///
/// 流程：选择预设角色/克隆模型 → 提交文本 → 获取 taskId → 轮询任务状态 → 下载音频 → 播放。
///
/// API 文档：
///   异步语音合成：POST https://openapi.lipvoice.cn/api/third/tts/create
///   语音合成列表：GET  https://openapi.lipvoice.cn/api/third/tts/list
///   克隆模型列表：GET  https://openapi.lipvoice.cn/api/third/reference/list
///
/// 与现有 <see cref="TTSManager"/> 完全独立，二者互不干扰。
/// </summary>
public class NoizTTSService : MonoBehaviour
{
    private const string BASE_URL        = "https://openapi.lipvoice.cn";
    private const string CREATE_URL      = BASE_URL + "/api/third/tts/create";
    private const string TTS_RESULT_URL  = BASE_URL + "/api/third/tts/result";   // 按 taskId 精确查询
    private const string TTS_LIST_URL    = BASE_URL + "/api/third/tts/list";     // 保留备用
    private const string MODEL_URL       = BASE_URL + "/api/third/reference/list";

    // ────────────────────────────────────────────────────────────────────
    //  预设角色列表（来源：官方文档 预设角色对应的id）
    //  这些是平台内置的，无需克隆即可直接使用。
    // ────────────────────────────────────────────────────────────────────

    private static readonly VoiceInfo[] PresetVoices =
    {
        new("A1fKB52bjj6tf1zfcGkipF7JkX",       "男声（足球解说）"),
        new("A1fKB52bjtrmNdQuhUXhrfTh3M",       "男声（哲理解说）"),
        new("A1fKB52c6oPQePtRh6eBcHk2x7",       "男声（清脆朗读）"),
        new("A1fKB52c73vAxRH5RBoZEeR4Go",       "女声（妖娆）"),
        new("A1fKB52c7DWBc4UQ2Vx3q1CTpj",       "女声（知性冷静）"),
        new("A1fKB52cU3EoZFaUcUaWFGQ86B",       "男声（新闻播报）"),
        new("A1fKhwMWKiDo1qHTQ26ZCMNWYs",       "女声（新闻播报）"),
        new("A1fKhh7YJPnoV3Cy2WbdL7ehRX",       "男声（电影解说）"),
        new("A1fKhwMVxock6WQqGPqZrDMM9H",       "女声（冷静解说）"),
        new("A1fKhwMWKsuHK866jUZRJ8xseX",       "女声（知识文艺）"),
        new("A1fKAKyHz2twQReSWg3d4yUt2s",       "男声（大气宣传片）"),
        new("A1fKAKyHzCYCWMKovTXdvKAmFq",       "男声（舌尖上的中国）"),
        new("A1fKhZqebTFw6feJDZR7fnsrws",       "男声（麦克阿瑟）"),
        new("A1fKhwMWKTjCfdpQWkS5qAhh2B",       "男声（军事纪录片）"),
        new("A1fKhwP6xy2F4bnQZ18gc1Lbry",       "女声（中年知识）"),
        new("A1fKhwP6y8dNRkNBRCggWgxQoh",       "女声（青春大学生）"),
        new("A1fKAKyJLwTkeMczU1tzJKo22h",       "女声（温柔甜美）"),
        new("A1fKAwnJf3VdaCushYSAzHW315",       "女声（带货主播）"),
        new("A1fKhwMWgnPhtEZ6Q1rUon7bWP",       "童声（曼波）"),
        new("A1fKiJp3nzSWhiTUkk4FRLKUsR",       "童声（女孩小学）"),
        new("A1fKiJp3o5JsD8njk4x3bTvB3q",       "童声（女童幼儿）"),
        new("A1fKiJp49p3UHDZsK4E6buaSKw",       "童声（男生小学生）"),
        new("A1fKAwqaiztFxLRQRquWZN2a9B",       "王者荣耀播报"),
        new("A1fJdTPpCVqHNtLjrXfWyJvNxs",       "甄嬛"),
        new("A1fJdTPpZesbyTDiVV4XJudYZR",       "李云龙"),
        new("A1fKAKv5bUHFuG7XMCpJRaMKWm",       "曹操（新三国）"),
        new("A1fKAKv6LNEYH8mCmxQyNk2JiD",       "诸葛亮（新三国）"),
        new("A1fKAwkguenzqsycdL53UZ5dcu",       "星爷"),
        new("A1fKAwkheU3jZqRD5iUkYShE59",       "高启强（狂飙）"),
        new("A1fKAweEHkMZhXGvAfgTYjE91D",       "佩奇"),
        new("A1fKAKyJMc57hSkaxJw4YziJHP",       "奶龙"),
        new("A1fKAwkheDWxFYmycfch6rj9kR",       "蜡笔小新"),
    };

    // ────────────────────────────────────────────────────────────────────
    //  Inspector 参数
    // ────────────────────────────────────────────────────────────────────

    [Header("鉴权")]
    [Tooltip("API 签名，放在每个请求的 Header sign 字段")]
    [SerializeField] private string _sign = "Oc1ouMzikVsWq4gEPnbKOwuLKuHyd59S";

    [Header("TTS 配置")]
    [Tooltip("模型/角色 ID（从预设角色或克隆模型列表选取）")]
    [SerializeField] private string _audioId = "A1fKB52bjj6tf1zfcGkipF7JkX";

    [Tooltip("模型版本：1=基础, 2=专业, 3=多语言")]
    [SerializeField] private string _style = "1";

    [Tooltip("语速 0.5~1.5")]
    [SerializeField, Range(0.5f, 1.5f)] private float _speed = 1.0f;

    [Tooltip("模型类别：0=参考原音频, 1=语气参考(专业), 2=使用参考音频(专业)")]
    [SerializeField] private int _genre = 0;

    [Header("轮询配置")]
    [Tooltip("轮询间隔（秒）")]
    [SerializeField] private float _pollInterval = 2f;

    [Tooltip("最大轮询次数（超时 = pollInterval × maxPollCount）")]
    [SerializeField] private int _maxPollCount = 60;

    // ────────────────────────────────────────────────────────────────────
    //  单例
    // ────────────────────────────────────────────────────────────────────

    private static NoizTTSService _instance;

    public static NoizTTSService Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[NoizTTSService]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<NoizTTSService>();
            return _instance;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开属性 / 事件
    // ────────────────────────────────────────────────────────────────────

    public bool IsSpeaking { get; private set; }

    public event Action OnTaskSubmitted;
    public event Action OnPlayBegin;
    public event Action OnCompleted;
    public event Action<string> OnError;
    public event Action<string> OnStatusChanged;

    public string Sign         { get => _sign;         set => _sign = value; }
    public string AudioId      { get => _audioId;      set => _audioId = value; }
    public string Style        { get => _style;        set => _style = value; }
    public float  Speed        { get => _speed;        set => _speed = Mathf.Clamp(value, 0.5f, 1.5f); }
    public int    Genre        { get => _genre;        set => _genre = value; }
    public float  PollInterval { get => _pollInterval; set => _pollInterval = Mathf.Max(0.1f, value); }
    public int    MaxPollCount { get => _maxPollCount; set => _maxPollCount = Mathf.Max(1, value); }

    /// <summary>当前可用的音色列表（预设角色 + 克隆模型）。</summary>
    public IReadOnlyList<VoiceInfo> AvailableVoices => _voices;

    /// <summary>当前选中的音色索引。</summary>
    public int SelectedVoiceIndex
    {
        get => _selectedVoiceIndex;
        set
        {
            if (_voices.Count == 0) return;
            _selectedVoiceIndex = Mathf.Clamp(value, 0, _voices.Count - 1);
            _audioId = _voices[_selectedVoiceIndex].audioId;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  内部状态
    // ────────────────────────────────────────────────────────────────────

    private AudioSource _audioSource;
    private Coroutine   _speakCoroutine;
    private readonly List<VoiceInfo> _voices = new();
    private int _selectedVoiceIndex;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        LoadPresetVoices();
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    /// <summary>初始化时加载内置预设角色到 _voices。</summary>
    private void LoadPresetVoices()
    {
        _voices.Clear();
        _voices.AddRange(PresetVoices);
        _selectedVoiceIndex = 0;
        if (string.IsNullOrEmpty(_audioId))
            _audioId = _voices[0].audioId;
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开方法
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 从服务端获取你账号下的克隆模型列表，追加到预设角色之后。
    /// API: GET /api/third/reference/list
    /// </summary>
    public void FetchClonedModels(Action<IReadOnlyList<VoiceInfo>> onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(FetchClonedModelsCoroutine(onSuccess, onError));
    }

    /// <summary>
    /// 提交文本进行语音合成，完成后自动下载并播放。
    /// 同一时间只允许一个任务；再次调用会停止上一次。
    /// </summary>
    public void Speak(string text, Action onComplete = null, Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[NoizTTS] Speak: text 为空");
            return;
        }

        Stop();
        _speakCoroutine = StartCoroutine(SpeakCoroutine(text, onComplete, onError));
    }

    /// <summary>停止当前合成/播放。</summary>
    public void Stop()
    {
        if (_speakCoroutine != null)
        {
            StopCoroutine(_speakCoroutine);
            _speakCoroutine = null;
        }
        if (_audioSource != null && _audioSource.isPlaying)
            _audioSource.Stop();

        IsSpeaking = false;
    }

    // ────────────────────────────────────────────────────────────────────
    //  核心协程
    // ────────────────────────────────────────────────────────────────────

    private IEnumerator SpeakCoroutine(string text, Action onComplete, Action<string> onError)
    {
        IsSpeaking = true;

        float t0 = Time.realtimeSinceStartup;   // 整体开始时刻

        SetStatus("正在提交合成任务…");

        string taskId   = null;
        string voiceUrl = null;
        string error    = null;

        yield return CreateTask(text,
            (id, url) => { taskId = id; voiceUrl = url; },
            err => error = err);

        if (error != null)
        {
            HandleError(error, onError);
            yield break;
        }

        float tSubmit = Time.realtimeSinceStartup;
        Debug.Log($"[NoizTTS] 任务已创建: taskId={taskId}  ⏱ 提交耗时={tSubmit - t0:F2}s");
        OnTaskSubmitted?.Invoke();

        if (string.IsNullOrEmpty(voiceUrl))
        {
            for (int i = 0; i < _maxPollCount; i++)
            {
                yield return new WaitForSeconds(_pollInterval);

                float tPollStart = Time.realtimeSinceStartup;
                int    status   = 0;
                string foundUrl = null;
                string pollErr  = null;
                string pollRaw  = null;

                yield return PollTaskStatus(taskId,
                    (s, u) => { status = s; foundUrl = u; },
                    err => pollErr = err,
                    raw => pollRaw = raw);

                float tPollEnd = Time.realtimeSinceStartup;
                Debug.Log($"[NoizTTS] 轮询 #{i + 1}  ⏱ {tPollEnd - tPollStart:F2}s  status={status}  累计={tPollEnd - t0:F2}s\n{pollRaw}");

                if (pollErr != null)
                {
                    Debug.LogWarning($"[NoizTTS] 轮询出错: {pollErr}, 继续重试…");
                    SetStatus($"轮询出错, 重试中… ({i + 1}/{_maxPollCount})");
                    continue;
                }

                if (status == 3)
                {
                    HandleError("语音合成失败(服务端返回 status=3)", onError);
                    yield break;
                }

                if (status == 2 && !string.IsNullOrEmpty(foundUrl))
                {
                    voiceUrl = foundUrl;
                    break;
                }

                SetStatus($"合成中… ({i + 1}/{_maxPollCount})");
            }

            if (string.IsNullOrEmpty(voiceUrl))
            {
                HandleError($"语音合成超时 (已等待 {_pollInterval * _maxPollCount:F0}s)", onError);
                yield break;
            }
        }

        float tSynth = Time.realtimeSinceStartup;
        Debug.Log($"[NoizTTS] 合成完成: {voiceUrl}  ⏱ 合成耗时={tSynth - tSubmit:F2}s  累计={tSynth - t0:F2}s");

        SetStatus("正在下载音频…");
        yield return DownloadAndPlay(voiceUrl, onComplete, onError, t0, tSynth);
    }

    // ────────────────────────────────────────────────────────────────────
    //  HTTP：创建合成任务
    // ────────────────────────────────────────────────────────────────────

    private IEnumerator CreateTask(string content, Action<string, string> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(new CreateRequestBody
        {
            content = content,
            audioId = _audioId,
            style   = _style,
            speed   = _speed,
            genre   = _genre
        });

        Debug.Log($"[NoizTTS] create 请求: audioId={_audioId}, style={_style}, genre={_genre}");

        using var req = new UnityWebRequest(CREATE_URL, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("sign", _sign);
        req.timeout = 15;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke($"创建任务 HTTP 失败: {req.error}");
            yield break;
        }

        string responseText = req.downloadHandler.text;
        Debug.Log($"[NoizTTS] create 响应: {responseText}");

        var resp = JsonUtility.FromJson<CreateResponse>(responseText);
        if (resp == null || resp.code != 0)
        {
            onError?.Invoke($"创建任务 API 错误: {resp?.msg ?? "响应解析失败"}");
            yield break;
        }

        onSuccess?.Invoke(resp.data.taskId, resp.data.voiceUrl);
    }

    // ────────────────────────────────────────────────────────────────────
    //  HTTP：轮询任务状态
    // ────────────────────────────────────────────────────────────────────

    private IEnumerator PollTaskStatus(string taskId, Action<int, string> onResult,
                                       Action<string> onError, Action<string> onRaw = null)
    {
        // 使用精确查询接口，按 taskId 直接获取该任务状态，不依赖分页列表
        string url = $"{TTS_RESULT_URL}?taskId={taskId}";

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("sign", _sign);
        req.timeout = 10;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onRaw?.Invoke($"(HTTP 失败: {req.error})");
            onError?.Invoke($"轮询 HTTP 失败: {req.error}");
            yield break;
        }

        string responseText = req.downloadHandler.text;
        onRaw?.Invoke(responseText);

        var resp = JsonUtility.FromJson<TtsResultResponse>(responseText);

        if (resp == null || resp.code != 0)
        {
            onError?.Invoke($"轮询 API 错误: {resp?.msg ?? "响应解析失败"}");
            yield break;
        }

        onResult?.Invoke(resp.data.status, resp.data.voiceUrl);
    }

    // ────────────────────────────────────────────────────────────────────
    //  HTTP：获取克隆模型列表
    // ────────────────────────────────────────────────────────────────────

    private IEnumerator FetchClonedModelsCoroutine(Action<IReadOnlyList<VoiceInfo>> onSuccess, Action<string> onError)
    {
        SetStatus("正在获取克隆模型列表…");

        string url = $"{MODEL_URL}?page=1&pageSize=20";

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("sign", _sign);
        req.timeout = 15;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            string err = $"获取克隆模型 HTTP 失败: {req.error}";
            Debug.LogError($"[NoizTTS] {err}");
            SetStatus(err);
            onError?.Invoke(err);
            yield break;
        }

        string responseText = req.downloadHandler.text;
        Debug.Log($"[NoizTTS] reference/list 响应: {responseText}");

        var resp = JsonUtility.FromJson<ModelListResponse>(responseText);
        if (resp == null || resp.code != 0)
        {
            string err = $"获取克隆模型 API 错误: {resp?.msg ?? "响应解析失败"}";
            Debug.LogError($"[NoizTTS] {err}");
            SetStatus(err);
            onError?.Invoke(err);
            yield break;
        }

        // 重建列表：先加预设角色，再追加克隆模型
        LoadPresetVoices();

        int clonedCount = 0;
        if (resp.data?.list != null)
        {
            foreach (var m in resp.data.list)
            {
                _voices.Add(new VoiceInfo(m.audioId, $"[克隆]{m.name}", m.describe));
                clonedCount++;
            }
        }

        Debug.Log($"[NoizTTS] 预设 {PresetVoices.Length} + 克隆 {clonedCount} = 共 {_voices.Count} 个音色");
        SetStatus($"预设{PresetVoices.Length}+克隆{clonedCount}，共{_voices.Count}个音色");
        onSuccess?.Invoke(_voices);
    }

    // ────────────────────────────────────────────────────────────────────
    //  下载并播放音频
    // ────────────────────────────────────────────────────────────────────

    // t0: 点击时刻（由 SpeakCoroutine 传入）；tSynth: 合成完成时刻
    private IEnumerator DownloadAndPlay(string voiceUrl, Action onComplete, Action<string> onError,
                                        float t0 = 0f, float tSynth = 0f)
    {
        // sign 同时放 header（API 文档推荐方式）和 URL 参数（保底兼容）
        string downloadUrl = voiceUrl.Contains("?")
            ? $"{voiceUrl}&sign={_sign}"
            : $"{voiceUrl}?sign={_sign}";

        // 根据 URL 扩展名自动判断音频格式（服务端目前返回 .wav）
        AudioType audioType = downloadUrl.Contains(".mp3") ? AudioType.MPEG
                            : downloadUrl.Contains(".ogg") ? AudioType.OGGVORBIS
                            : AudioType.WAV;

        using var req = UnityWebRequestMultimedia.GetAudioClip(downloadUrl, audioType);
        req.SetRequestHeader("sign", _sign);
        req.timeout = 30;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            HandleError($"下载音频失败: {req.error}", onError);
            yield break;
        }

        // 服务端有时返回 HTTP 200 但内容是 JSON 错误体（例如 sign 无效）。
        // DownloadHandlerAudioClip 不支持 .text 属性，改用 Content-Type 响应头判断。
        string contentType = req.GetResponseHeader("Content-Type") ?? "";
        if (contentType.Contains("application/json") || contentType.Contains("text/"))
        {
            // 用原始字节读取错误信息（字节读取安全，不依赖 .text）
            byte[] raw = req.downloadHandler?.data;
            string body = (raw != null && raw.Length > 0)
                ? System.Text.Encoding.UTF8.GetString(raw)
                : "(无响应体)";
            HandleError($"音频服务器拒绝请求: {body}", onError);
            yield break;
        }

        float tDownload = Time.realtimeSinceStartup;
        Debug.Log($"[NoizTTS] ⏱ 下载耗时={tDownload - tSynth:F2}s  " +
                  $"【总耗时（点击→可播放）= {tDownload - t0:F2}s】");

        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
        if (clip == null || clip.length < 0.01f)
        {
            HandleError("音频解析失败或内容为空", onError);
            yield break;
        }

        SetStatus("播放中…");
        OnPlayBegin?.Invoke();

        _audioSource.clip = clip;
        _audioSource.Play();
        Debug.Log($"[NoizTTS] 开始播放, 时长={clip.length:F1}s");

        while (_audioSource.isPlaying)
            yield return null;

        IsSpeaking      = false;
        _speakCoroutine = null;
        SetStatus("播放完成");
        OnCompleted?.Invoke();
        onComplete?.Invoke();
        Debug.Log("[NoizTTS] 播放完成");
    }

    // ────────────────────────────────────────────────────────────────────
    //  辅助
    // ────────────────────────────────────────────────────────────────────

    private void HandleError(string msg, Action<string> callback)
    {
        IsSpeaking      = false;
        _speakCoroutine = null;
        Debug.LogError($"[NoizTTS] {msg}");
        SetStatus($"错误: {msg}");
        OnError?.Invoke(msg);
        callback?.Invoke(msg);
    }

    private void SetStatus(string status)
    {
        OnStatusChanged?.Invoke(status);
    }

    // ────────────────────────────────────────────────────────────────────
    //  公开数据结构
    // ────────────────────────────────────────────────────────────────────

    public readonly struct VoiceInfo
    {
        public readonly string audioId;
        public readonly string name;
        public readonly string desc;

        public VoiceInfo(string audioId, string name, string desc = "")
        {
            this.audioId = audioId;
            this.name    = name;
            this.desc    = desc ?? "";
        }

        public override string ToString() => name;
    }

    // ────────────────────────────────────────────────────────────────────
    //  JSON 序列化模型
    // ────────────────────────────────────────────────────────────────────

    [Serializable] private class CreateRequestBody
    {
        public string content;
        public string audioId;
        public string style;
        public float  speed;
        public int    genre;
    }

    [Serializable] private class CreateResponse
    {
        public int            code;
        public string         msg;
        public CreateRespData data;
    }

    [Serializable] private class CreateRespData
    {
        public string taskId;
        public int    status;
        public string voiceUrl;
    }

    // /api/third/tts/result — 按 taskId 精确查询单个任务
    [Serializable] private class TtsResultResponse
    {
        public int               code;
        public string            msg;
        public TtsResultRespData data;
    }

    [Serializable] private class TtsResultRespData
    {
        public string taskId;
        public int    status;    // 1=进行中, 2=成功, 3=失败
        public string voiceUrl;
    }

    // /api/third/tts/list — 分页列表（保留备用，目前轮询不再使用）
    [Serializable] private class TtsListResponse
    {
        public int             code;
        public string          msg;
        public TtsListRespData data;
    }

    [Serializable] private class TtsListRespData
    {
        public TtsListRespItem[] list;
        public int               total;
        public int               page;
        public int               pageSize;
    }

    [Serializable] private class TtsListRespItem
    {
        public string taskId;
        public int    status;
        public string voiceUrl;
    }

    [Serializable] private class ModelListResponse
    {
        public int               code;
        public string            msg;
        public ModelListRespData data;
    }

    [Serializable] private class ModelListRespData
    {
        public ModelListRespItem[] list;
        public int                 total;
        public int                 page;
        public int                 pageSize;
    }

    [Serializable] private class ModelListRespItem
    {
        public string audioId;
        public string name;
        public string describe;
        public string audioPath;
    }
}
