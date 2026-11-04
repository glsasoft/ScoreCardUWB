using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 音乐文件下载管理 / 播放模块。
///
/// 职责：
///   1. 断点续传下载（HTTP Range）音乐文件到本地持久存储目录。
///   2. 通过 <see cref="ExternalDeviceAudioPlayer"/> 将音乐路由到指定 ALSA 声卡（战场音效声卡），
///      与 Unity 内置 AudioSource 完全独立。
///   3. 音乐文件管理（查询 / 删除）。
///   4. 预留 MQTT 对接入口，格式确定后在对应 Handle* 方法内填充 JSON 解析逻辑。
///
/// 存储路径：<c>Application.persistentDataPath/glsa_music/</c>
///
/// 使用示例：
/// <code>
///   // 指定战场声卡
///   MusicDownloadManager.Instance.DeviceSelector = "hdmi0";
///
///   // 下载并播放
///   MusicDownloadManager.Instance.Enqueue(
///       url:      "http://example.com/music/battle.mp3",
///       fileName: "battle.mp3",
///       onProgress: p => Debug.Log($"下载进度 {p*100:F0}%"),
///       onComplete: _ => MusicDownloadManager.Instance.Play("battle.mp3", loop: true));
///
///   // 仅播放已存在文件
///   MusicDownloadManager.Instance.Play("battle.mp3", loop: true);
///
///   // 停止播放
///   MusicDownloadManager.Instance.StopBgm();
///
///   // 删除文件
///   MusicDownloadManager.Instance.Delete("battle.mp3");
/// </code>
/// </summary>
public class MusicDownloadManager : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════════
    //  常量
    // ════════════════════════════════════════════════════════════════════

    /// <summary>本地音乐存储子目录名。</summary>
    private const string MusicSubDir = "glsa_music";

    /// <summary>未完成下载的临时后缀；仅成功校验后才改名为正式文件。</summary>
    private const string PartSuffix = ".part";

    /// <summary>过小则视为无效/截断的音乐文件（字节）。</summary>
    private const long MinValidMusicBytes = 2048;

    /// <summary>AlsaDevicePlayer 中背景音乐的固定 key。</summary>
    public const string BgmKey = "bgm";

    // ════════════════════════════════════════════════════════════════════
    //  单例
    // ════════════════════════════════════════════════════════════════════

    private static MusicDownloadManager _instance;

    public static MusicDownloadManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[MusicDownloadManager]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MusicDownloadManager>();
            return _instance;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  Inspector 参数
    // ════════════════════════════════════════════════════════════════════

    [Header("外置声卡")]
    [Tooltip("ALSA 声卡过滤关键词（如 \"hdmi0\"），留空则使用默认 ALSA 设备")]
    [SerializeField] private string _deviceSelector = "";

    /// <summary>
    /// 战场 BGM 音量缓存（0~1）。底层每次 Play/Resume 都会以默认 1.0 重建 voice，
    /// 故须记住期望音量并在开播后重新应用。
    /// </summary>
    float m_BgmVolume = 1f;

    [Tooltip("同时进行的最大下载任务数（建议 1，避免带宽竞争）")]
    [SerializeField, Range(1, 4)] private int _maxConcurrent = 1;

    // ════════════════════════════════════════════════════════════════════
    //  下载任务模型
    // ════════════════════════════════════════════════════════════════════

    public enum DownloadState
    {
        /// <summary>等待队列调度。</summary>
        Queued,
        /// <summary>正在下载。</summary>
        Downloading,
        /// <summary>下载完成。</summary>
        Done,
        /// <summary>下载出错。</summary>
        Error,
        /// <summary>已被取消。</summary>
        Cancelled,
    }

    public class DownloadTask
    {
        /// <summary>目标文件名（含扩展名），如 "battle.mp3"。</summary>
        public string FileName;

        /// <summary>下载 URL。</summary>
        public string Url;

        /// <summary>当前任务状态。</summary>
        public DownloadState State = DownloadState.Queued;

        /// <summary>下载进度（0~1）。</summary>
        public float Progress;

        /// <summary>文件总大小（字节），-1 表示服务端未返回。</summary>
        public long TotalBytes = -1;

        /// <summary>已下载字节数。</summary>
        public long DownloadedBytes;

        /// <summary>错误信息（State == Error 时有效）。</summary>
        public string ErrorMessage;

        // ──── 回调（主线程触发）────

        /// <summary>进度回调（0~1）。</summary>
        public Action<float> OnProgress;

        /// <summary>完成回调，参数为本地文件绝对路径。</summary>
        public Action<string> OnComplete;

        /// <summary>错误回调，参数为错误描述。</summary>
        public Action<string> OnError;
    }

    // ════════════════════════════════════════════════════════════════════
    //  状态
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 当前 ALSA 声卡过滤关键词（可在运行时动态修改）。
    /// 未显式设置时，自动回退到当前开发板的耳机声卡（BoardProfile）。
    /// </summary>
    public string DeviceSelector
    {
        get => string.IsNullOrEmpty(_deviceSelector)
            ? BoardProfile.AudioCardSelector
            : _deviceSelector;
        set => _deviceSelector = value;
    }

    /// <summary>当前是否有 BGM 正在播放。</summary>
    public bool IsBgmPlaying => ExternalDeviceAudioPlayer.IsPlaying(BgmKey);

    /// <summary>音乐本地存储目录完整路径。</summary>
    public string MusicDirectory => Path.Combine(Application.persistentDataPath, MusicSubDir);

    private readonly Queue<DownloadTask> _queue = new();
    private readonly Dictionary<string, DownloadTask> _activeTasks = new();
    private readonly List<Coroutine> _runningCoroutines = new();
    private int _runningCount;

    // ════════════════════════════════════════════════════════════════════
    //  Unity 生命周期
    // ════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 确保存储目录存在
        Directory.CreateDirectory(MusicDirectory);
        Debug.Log($"[MusicMgr] 存储目录: {MusicDirectory}");
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        foreach (var c in _runningCoroutines) { if (c != null) StopCoroutine(c); }
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开 API —— 下载
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 将音乐文件加入下载队列（支持断点续传）。
    /// 仅当本地正式文件通过完整性校验时才跳过下载；下载中的 .part 不算可用缓存。
    /// 同一 fileName 重复入队会合并回调（解决 MQTT 快速连发 play 丢回调）。
    /// </summary>
    public DownloadTask Enqueue(string url, string fileName,
        Action<float> onProgress = null,
        Action<string> onComplete = null,
        Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("[MusicMgr] Enqueue: URL 为空");
            return null;
        }
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("[MusicMgr] Enqueue: fileName 为空");
            return null;
        }

        // 已在队列 / 下载中 → 合并回调，不另起任务
        if (_activeTasks.TryGetValue(fileName, out var existing))
        {
            MergeTaskCallbacks(existing, onProgress, onComplete, onError);
            Debug.Log($"[MusicMgr] 任务已存在，合并回调: {fileName} state={existing.State}");
            return existing;
        }

        string localPath = GetLocalPath(fileName);
        if (IsUsableLocalMusicFile(localPath))
        {
            Debug.Log($"[MusicMgr] 本地文件校验通过，跳过下载: {fileName} ({new FileInfo(localPath).Length} bytes)");
            var doneTask = new DownloadTask
            {
                FileName = fileName, Url = url, State = DownloadState.Done,
                Progress = 1f, OnComplete = onComplete, OnProgress = onProgress, OnError = onError
            };
            onComplete?.Invoke(localPath);
            return doneTask;
        }

        // 正式文件损坏或过小 → 清掉，走重新下载
        if (File.Exists(localPath))
        {
            Debug.LogWarning($"[MusicMgr] 本地文件无效，将重新下载: {fileName}");
            TryDeleteLocalMusicAndPcm(fileName);
        }

        var task = new DownloadTask
        {
            FileName = fileName,
            Url = url,
            State = DownloadState.Queued,
            OnProgress = onProgress,
            OnComplete = onComplete,
            OnError = onError,
        };

        _activeTasks[fileName] = task;
        _queue.Enqueue(task);
        Debug.Log($"[MusicMgr] 已入队: {fileName}  队列深度={_queue.Count}");

        TryStartNext();
        return task;
    }

    static void MergeTaskCallbacks(DownloadTask task,
        Action<float> onProgress, Action<string> onComplete, Action<string> onError)
    {
        if (onProgress != null)
        {
            var prev = task.OnProgress;
            task.OnProgress = p => { prev?.Invoke(p); onProgress(p); };
        }
        if (onComplete != null)
        {
            var prev = task.OnComplete;
            task.OnComplete = path => { prev?.Invoke(path); onComplete(path); };
        }
        if (onError != null)
        {
            var prev = task.OnError;
            task.OnError = err => { prev?.Invoke(err); onError(err); };
        }
    }

    /// <summary>
    /// 取消指定文件名的下载任务（已下载的部分文件保留，下次可续传）。
    /// </summary>
    public void CancelDownload(string fileName)
    {
        if (_activeTasks.TryGetValue(fileName, out var task))
        {
            task.State = DownloadState.Cancelled;
            Debug.Log($"[MusicMgr] 已标记取消: {fileName}");
        }
    }

    /// <summary>查询某文件名是否有任务正在下载中。</summary>
    public bool IsDownloading(string fileName) =>
        _activeTasks.TryGetValue(fileName, out var t) && t.State == DownloadState.Downloading;

    /// <summary>获取某文件名的当前下载任务（不存在则 null）。</summary>
    public DownloadTask GetTask(string fileName) =>
        _activeTasks.TryGetValue(fileName, out var t) ? t : null;

    // ════════════════════════════════════════════════════════════════════
    //  公开 API —— 文件管理
    // ════════════════════════════════════════════════════════════════════

    /// <summary>检查本地是否存在<strong>可用</strong>的已下载音乐文件（非 .part、非损坏）。</summary>
    public bool FileExists(string fileName) => IsUsableLocalMusicFile(GetLocalPath(fileName));

    /// <summary>获取指定文件名的本地绝对路径（不保证文件存在）。</summary>
    public string GetLocalPath(string fileName) => Path.Combine(MusicDirectory, fileName);

    string GetPartPath(string fileName) => GetLocalPath(fileName) + PartSuffix;

    /// <summary>
    /// 删除指定音乐文件及其 PCM 缓存。
    /// </summary>
    public bool Delete(string fileName)
    {
        TryDeleteLocalMusicAndPcm(fileName);
        _activeTasks.Remove(fileName);
        return !File.Exists(GetLocalPath(fileName));
    }

    /// <summary>删除所有已下载的音乐文件与临时 .part。</summary>
    public void DeleteAll()
    {
        try
        {
            foreach (var f in Directory.GetFiles(MusicDirectory))
            {
                TryDeleteFile(f);
                if (!f.EndsWith(PartSuffix, StringComparison.OrdinalIgnoreCase))
                    AudioClipPcmConverter.InvalidateDiskCacheForFile(f);
            }
            _activeTasks.Clear();
            Debug.Log("[MusicMgr] 已清空所有音乐文件");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MusicMgr] DeleteAll 失败: {ex.Message}");
        }
    }

    /// <summary>返回本地所有已下载音乐文件的文件名列表（不含目录路径、不含 .part）。</summary>
    public string[] GetDownloadedFileNames()
    {
        if (!Directory.Exists(MusicDirectory)) return Array.Empty<string>();
        var paths = Directory.GetFiles(MusicDirectory);
        var names = new List<string>(paths.Length);
        for (int i = 0; i < paths.Length; i++)
        {
            string name = Path.GetFileName(paths[i]);
            if (name.EndsWith(PartSuffix, StringComparison.OrdinalIgnoreCase)) continue;
            if (!IsUsableLocalMusicFile(paths[i])) continue;
            names.Add(name);
        }
        return names.ToArray();
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开 API —— 播放控制
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 播放本地已下载的音乐文件到 ALSA 指定声卡。
    /// 若文件不可用，打印错误并返回。
    /// </summary>
    public void Play(string fileName, bool loop = true, Action onReady = null)
    {
        string path = GetLocalPath(fileName);
        if (!IsUsableLocalMusicFile(path))
        {
            Debug.LogError($"[MusicMgr] Play: 本地文件不可用 {path}，请先下载或重新下载");
            return;
        }
        StartCoroutine(LoadAndPlayCoroutine(path, BgmKey, loop, onReady, allowCorruptRetry: true));
    }

    /// <summary>
    /// 播放本地已下载的音乐文件并使用自定义 key（可与 BGM 并行）。
    /// </summary>
    public void PlayWithKey(string fileName, string key, bool loop = false)
    {
        string path = GetLocalPath(fileName);
        if (!IsUsableLocalMusicFile(path))
        {
            Debug.LogError($"[MusicMgr] PlayWithKey: 本地文件不可用 {path}");
            return;
        }
        StartCoroutine(LoadAndPlayCoroutine(path, key, loop, null, allowCorruptRetry: true));
    }

    /// <summary>
    /// 下载（或校验本地）后播放。若本地损坏会被清掉并重新下载一次。
    /// 适合 MQTT 快速连发 play。
    /// </summary>
    public void DownloadAndPlay(string url, string fileName, bool loop = true,
        Action onReady = null, Action<string> onError = null)
    {
        StartCoroutine(DownloadAndPlayCoroutine(url, fileName, loop, onReady, onError, allowRetry: true));
    }

    IEnumerator DownloadAndPlayCoroutine(string url, string fileName, bool loop,
        Action onReady, Action<string> onError, bool allowRetry)
    {
        bool downloadOk = false;
        string downloadErr = null;
        bool finished = false;

        Enqueue(url, fileName,
            onComplete: _ => { downloadOk = true; finished = true; },
            onError: err => { downloadErr = err; finished = true; });

        while (!finished) yield return null;

        if (!downloadOk)
        {
            onError?.Invoke(downloadErr ?? "download failed");
            yield break;
        }

        string path = GetLocalPath(fileName);
        if (!IsUsableLocalMusicFile(path))
        {
            if (allowRetry)
            {
                Debug.LogWarning($"[MusicMgr] 下载后仍不可用，强制重下: {fileName}");
                TryDeleteLocalMusicAndPcm(fileName);
                yield return DownloadAndPlayCoroutine(url, fileName, loop, onReady, onError, allowRetry: false);
            }
            else
            {
                onError?.Invoke("local file invalid after download");
            }
            yield break;
        }

        // 播放；若解码失败会清本地文件，再允许一次重下
        yield return LoadAndPlayCoroutine(path, BgmKey, loop, onReady, allowCorruptRetry: false);

        if (allowRetry && !IsUsableLocalMusicFile(GetLocalPath(fileName)))
        {
            Debug.LogWarning($"[MusicMgr] 播放失败并已清理损坏文件，自动重下: {fileName}");
            yield return DownloadAndPlayCoroutine(url, fileName, loop, onReady, onError, allowRetry: false);
        }
    }

    /// <summary>停止 BGM 播放（key = <see cref="BgmKey"/>）。</summary>
    public void StopBgm() => ExternalDeviceAudioPlayer.Stop(BgmKey);

    /// <summary>停止指定 key 的播放。</summary>
    public void Stop(string key) => ExternalDeviceAudioPlayer.Stop(key);

    /// <summary>停止所有由本管理器发起的播放。</summary>
    public void StopAll() => ExternalDeviceAudioPlayer.StopAll();

    /// <summary>暂停 BGM。</summary>
    public void PauseBgm() => ExternalDeviceAudioPlayer.Pause(BgmKey);

    /// <summary>恢复 BGM（Resume 底层会以默认音量重建 voice，随后套用缓存音量）。</summary>
    public void ResumeBgm()
    {
        ExternalDeviceAudioPlayer.Resume(DeviceSelector, BgmKey);
        ExternalDeviceAudioPlayer.SetVoiceVolume(BgmKey, m_BgmVolume);
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开 API —— 音量
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 设置 ALSA 混音器总输出音量（影响战场声卡的所有声音）。
    /// </summary>
    /// <param name="volume">0 = 静音；1.0 = 正常；最大 1.5（可能轻微失真）</param>
    public void SetAlsaMasterVolume(float volume)
    {
        ExternalDeviceAudioPlayer.SetMasterVolume(Mathf.Clamp(volume, 0f, 1.5f));
        Debug.Log($"[MusicMgr] ALSA 总音量: {volume:F2}");
    }

    /// <summary>单独设置 BGM 音量（0~1），立即作用于当前播放，并供后续 Play/Resume 自动套用。</summary>
    public void SetBgmVolume(float volume)
    {
        m_BgmVolume = Mathf.Clamp01(volume);
        ExternalDeviceAudioPlayer.SetVoiceVolume(BgmKey, m_BgmVolume);
    }

    /// <summary>当前缓存的 BGM 音量（0~1）。</summary>
    public float BgmVolume => m_BgmVolume;

    // ════════════════════════════════════════════════════════════════════
    //  预留：MQTT 对接入口
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// [预留 · MQTT 对接] 处理服务器下发的 play 命令（播放 / 停止背景音乐）。
    /// 当 MQTT 消息格式确定后，在此处解析 JSON 并调用 <see cref="Play"/> / <see cref="StopBgm"/>。
    ///
    /// 预期 JSON 示例（待确认）：
    /// <code>{"type":"play","data":{"fileName":"battle.mp3","loop":true}}</code>
    /// 或停止：
    /// <code>{"type":"play","data":{"stop":true}}</code>
    /// </summary>
    public void HandleMqttPlay(string json)
    {
        // TODO: 解析 json，提取 fileName / loop / stop 字段
        // 示例占位：
        // var data = JsonUtility.FromJson<MusicPlayCmd>(json);
        // if (data.stop) StopBgm();
        // else Play(data.fileName, data.loop);
        Debug.Log($"[MusicMgr][TODO] HandleMqttPlay: {json}");
    }

    /// <summary>
    /// [预留 · MQTT 对接] 处理服务器下发的 download 命令（触发音乐下载）。
    ///
    /// 预期 JSON 示例（待确认）：
    /// <code>{"type":"download","data":{"url":"http://...","fileName":"battle.mp3"}}</code>
    /// </summary>
    public void HandleMqttDownload(string json)
    {
        // TODO: 解析 json，提取 url / fileName 字段
        // 示例占位：
        // var data = JsonUtility.FromJson<MusicDownloadCmd>(json);
        // Enqueue(data.url, data.fileName);
        Debug.Log($"[MusicMgr][TODO] HandleMqttDownload: {json}");
    }

    /// <summary>
    /// [预留 · MQTT 对接] 处理服务器下发的 del 命令（删除本地音乐文件）。
    ///
    /// 预期 JSON 示例（待确认）：
    /// <code>{"type":"del","data":{"fileName":"battle.mp3"}}</code>
    /// 或全部删除：
    /// <code>{"type":"del","data":{"all":true}}</code>
    /// </summary>
    public void HandleMqttDelete(string json)
    {
        // TODO: 解析 json，提取 fileName / all 字段
        // 示例占位：
        // var data = JsonUtility.FromJson<MusicDeleteCmd>(json);
        // if (data.all) DeleteAll();
        // else Delete(data.fileName);
        Debug.Log($"[MusicMgr][TODO] HandleMqttDelete: {json}");
    }

    /// <summary>
    /// [预留 · MQTT 对接] 处理服务器下发的 vol 命令（调节战场声卡总音量）。
    ///
    /// 预期 JSON 示例（待确认）：
    /// <code>{"type":"vol","data":{"volume":0.8}}</code>
    /// </summary>
    public void HandleMqttVol(string json)
    {
        // TODO: 解析 json，提取 volume 字段（0~1 或 0~100 待确认）
        // 示例占位：
        // var data = JsonUtility.FromJson<MusicVolCmd>(json);
        // SetAlsaMasterVolume(data.volume);
        Debug.Log($"[MusicMgr][TODO] HandleMqttVol: {json}");
    }

    // ════════════════════════════════════════════════════════════════════
    //  内部：下载队列调度
    // ════════════════════════════════════════════════════════════════════

    private void TryStartNext()
    {
        while (_runningCount < _maxConcurrent && _queue.Count > 0)
        {
            var task = _queue.Dequeue();
            if (task.State == DownloadState.Cancelled)
            {
                _activeTasks.Remove(task.FileName);
                continue;
            }
            _runningCount++;
            var co = StartCoroutine(RunDownload(task));
            _runningCoroutines.Add(co);
        }
    }

    private IEnumerator RunDownload(DownloadTask task)
    {
        yield return StartCoroutine(DownloadWithResume(task));
        _runningCount--;
        _activeTasks.Remove(task.FileName);
        TryStartNext();
    }

    // ════════════════════════════════════════════════════════════════════
    //  内部：断点续传下载
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 断点续传下载核心逻辑。写入 *.part，仅成功且通过大小校验后才原子替换为正式文件。
    /// </summary>
    private IEnumerator DownloadWithResume(DownloadTask task)
    {
        if (task.State == DownloadState.Cancelled) yield break;

        task.State = DownloadState.Downloading;
        string destPath = GetLocalPath(task.FileName);
        string partPath = GetPartPath(task.FileName);
        long existingSize = File.Exists(partPath) ? new FileInfo(partPath).Length : 0;

        // ── Step 1: HEAD 请求（获取文件大小与 Range 支持情况）────────────
        long serverTotalSize = -1;
        bool serverSupportsRange = false;

        using (var headReq = UnityWebRequest.Head(task.Url))
        {
            headReq.timeout = 10;
            yield return headReq.SendWebRequest();

            if (headReq.result == UnityWebRequest.Result.Success)
            {
                if (long.TryParse(headReq.GetResponseHeader("Content-Length"), out long sz))
                    serverTotalSize = sz;

                string acceptRanges = headReq.GetResponseHeader("Accept-Ranges");
                serverSupportsRange = !string.IsNullOrEmpty(acceptRanges)
                                      && !acceptRanges.Equals("none", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Debug.LogWarning($"[MusicMgr] HEAD 失败（{headReq.error}），降级为全量下载");
            }
        }

        task.TotalBytes = serverTotalSize;

        // ── Step 2: 正式文件已完整 → 清理 .part 后完成 ─────────────────
        if (IsUsableLocalMusicFile(destPath, serverTotalSize))
        {
            TryDeleteFile(partPath);
            Debug.Log($"[MusicMgr] 正式文件已完整，跳过下载: {task.FileName}");
            FinishTaskSuccess(task, destPath);
            yield break;
        }

        // .part 已达到服务端大小但尚未提交 → 校验后提交
        if (existingSize > 0 && serverTotalSize > 0 && existingSize >= serverTotalSize)
        {
            if (TryCommitPartFile(partPath, destPath, serverTotalSize))
            {
                FinishTaskSuccess(task, destPath);
                yield break;
            }
            existingSize = 0;
        }

        // ── Step 3: 决定是否续传（只对 .part）────────────────────────────
        bool resume = existingSize > 0 && serverSupportsRange && serverTotalSize > 0
                      && existingSize < serverTotalSize;
        long rangeStart = resume ? existingSize : 0;

        if (!resume)
        {
            TryDeleteFile(partPath);
            rangeStart = 0;
            Debug.Log($"[MusicMgr] 全量下载: {task.FileName}");
        }
        else
        {
            Debug.Log($"[MusicMgr] 断点续传: {task.FileName} 从 {rangeStart} bytes 开始");
        }

        // ── Step 4: 下载到 .part ─────────────────────────────────────────
        using (var req = new UnityWebRequest(task.Url, UnityWebRequest.kHttpVerbGET))
        {
            req.downloadHandler = new DownloadHandlerFile(partPath, resume);

            if (resume)
                req.SetRequestHeader("Range", $"bytes={rangeStart}-");

            req.timeout = 0;

            var op = req.SendWebRequest();

            while (!op.isDone)
            {
                if (task.State == DownloadState.Cancelled)
                {
                    req.Abort();
                    Debug.Log($"[MusicMgr] 下载已取消: {task.FileName}");
                    yield break;
                }

                task.DownloadedBytes = rangeStart + (long)req.downloadedBytes;
                task.Progress = serverTotalSize > 0
                    ? Mathf.Clamp01((float)task.DownloadedBytes / serverTotalSize)
                    : req.downloadProgress;
                task.OnProgress?.Invoke(task.Progress);

                yield return null;
            }

            bool httpOk = req.result == UnityWebRequest.Result.Success
                          || req.responseCode == 206;

            if (req.responseCode == 416 && File.Exists(partPath))
            {
                // 本地 .part 已满足 Range，尝试提交
                httpOk = true;
            }

            if (!httpOk)
            {
                task.State = DownloadState.Error;
                task.ErrorMessage = $"HTTP {req.responseCode}: {req.error}";
                Debug.LogError($"[MusicMgr] 下载失败: {task.FileName}  {task.ErrorMessage}");
                // 保留 .part 以便下次续传；但若明显过小则删掉避免误用
                if (File.Exists(partPath) && new FileInfo(partPath).Length < MinValidMusicBytes)
                    TryDeleteFile(partPath);
                task.OnError?.Invoke(task.ErrorMessage);
                yield break;
            }

            long finalSize = File.Exists(partPath) ? new FileInfo(partPath).Length : 0;
            if (serverTotalSize > 0 && finalSize < serverTotalSize)
            {
                task.State = DownloadState.Error;
                task.ErrorMessage = $"下载不完整: {finalSize}/{serverTotalSize}";
                Debug.LogError($"[MusicMgr] {task.ErrorMessage}  file={task.FileName}");
                task.OnError?.Invoke(task.ErrorMessage);
                yield break;
            }

            if (!TryCommitPartFile(partPath, destPath, serverTotalSize))
            {
                task.State = DownloadState.Error;
                task.ErrorMessage = "下载完成但提交正式文件失败（校验未通过）";
                Debug.LogError($"[MusicMgr] {task.ErrorMessage}  file={task.FileName}");
                task.OnError?.Invoke(task.ErrorMessage);
                yield break;
            }

            task.DownloadedBytes = new FileInfo(destPath).Length;
            FinishTaskSuccess(task, destPath);
        }
    }

    static void FinishTaskSuccess(DownloadTask task, string destPath)
    {
        task.State = DownloadState.Done;
        task.Progress = 1f;
        if (task.DownloadedBytes <= 0 && File.Exists(destPath))
            task.DownloadedBytes = new FileInfo(destPath).Length;
        task.OnProgress?.Invoke(1f);
        Debug.Log($"[MusicMgr] 下载完成: {task.FileName}  ({task.DownloadedBytes} bytes)  → {destPath}");
        task.OnComplete?.Invoke(destPath);
    }

    /// <summary>
    /// 将 .part 原子替换为正式文件；必须通过大小校验。失败则删除坏文件。
    /// </summary>
    bool TryCommitPartFile(string partPath, string destPath, long expectedSize)
    {
        try
        {
            if (!File.Exists(partPath)) return false;
            long len = new FileInfo(partPath).Length;
            if (len < MinValidMusicBytes) return false;
            if (expectedSize > 0 && len < expectedSize) return false;

            // 先校验音频头，避免把空壳当成功
            if (!LooksLikeAudioFile(partPath))
            {
                Debug.LogWarning($"[MusicMgr] .part 不像有效音频，丢弃: {partPath}");
                TryDeleteFile(partPath);
                return false;
            }

            TryDeleteFile(destPath);
            File.Move(partPath, destPath);

            if (!IsUsableLocalMusicFile(destPath, expectedSize))
            {
                TryDeleteFile(destPath);
                return false;
            }

            // 旧正式文件对应的 PCM 可能已过期（长度/mtime 变了键会变，但仍清旧键路径）
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MusicMgr] 提交正式文件失败: {e.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  内部：加载 AudioClip 并播放到 ALSA
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 从本地文件路径加载 AudioClip，然后通过 ExternalDeviceAudioPlayer 异步转 PCM 并播放到 ALSA。
    /// 损坏的 PCM / 无法解码的音乐会删除缓存；allowCorruptRetry 时由调用方配合重新下载。
    /// </summary>
    private IEnumerator LoadAndPlayCoroutine(string filePath, string key, bool loop, Action onReady,
        bool allowCorruptRetry)
    {
        string cacheKey = AudioClipPcmConverter.MakeFileCacheKey(filePath);
        float volume = key == BgmKey ? m_BgmVolume : 1f;
        string fileName = Path.GetFileName(filePath);

        // 磁盘缓存命中：只把路径交给 Java 后台读（已校验 pcmLen）
        if (AudioClipPcmConverter.TryGetDiskCachePath(cacheKey, out string pcm16Path))
        {
            Debug.Log($"[MusicMgr] PCM 磁盘缓存命中（后台播放）: {fileName}  path={pcm16Path}");
            onReady?.Invoke();
            ExternalDeviceAudioPlayer.PlayFromPcm16File(DeviceSelector, key, pcm16Path, loop, volume);
            yield break;
        }

        string fileUri = "file://" + filePath;
        AudioType audioType = DetectAudioType(filePath);

        Debug.Log($"[MusicMgr] 加载音频: {fileName}  AudioType={audioType}");

        using (var req = UnityWebRequestMultimedia.GetAudioClip(fileUri, audioType))
        {
            ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = false;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[MusicMgr] 音频加载失败: {req.error}  path={filePath}");
                HandleCorruptLocalMusic(fileName, filePath, allowCorruptRetry);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip == null || clip.samples <= 0 || clip.frequency <= 0)
            {
                Debug.LogError($"[MusicMgr] AudioClip 无效: {filePath} samples={clip?.samples} freq={clip?.frequency}");
                HandleCorruptLocalMusic(fileName, filePath, allowCorruptRetry);
                yield break;
            }

            Debug.Log($"[MusicMgr] 开始异步转 PCM → 声卡:{DeviceSelector}  key:{key}  loop:{loop}  " +
                      $"时长:{clip.length:F1}s  采样率:{clip.frequency}Hz  声道:{clip.channels}");

            onReady?.Invoke();

            var playTask = ExternalDeviceAudioPlayer
                .PlayFromClipAsync(DeviceSelector, key, clip, loop, filePath, usePcmCache: true, volume);
            yield return playTask.ToCoroutine();
        }
    }

    void HandleCorruptLocalMusic(string fileName, string filePath, bool allowCorruptRetry)
    {
        Debug.LogWarning($"[MusicMgr] 判定本地音乐损坏，清理文件与 PCM: {fileName}");
        TryDeleteLocalMusicAndPcm(fileName);
        if (!allowCorruptRetry)
            return;
        // 不在这里自动重下（无 URL）；由 DownloadAndPlay / 上层再次 Enqueue
    }

    // ════════════════════════════════════════════════════════════════════
    //  内部：本地文件校验 / 清理
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 正式音乐文件是否可用。expectedSize&gt;0 时还要求长度一致（或不少于）。
    /// </summary>
    public static bool IsUsableLocalMusicFile(string path, long expectedSize = -1)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            if (path.EndsWith(PartSuffix, StringComparison.OrdinalIgnoreCase)) return false;

            var fi = new FileInfo(path);
            if (fi.Length < MinValidMusicBytes) return false;
            if (expectedSize > 0 && fi.Length < expectedSize) return false;
            return LooksLikeAudioFile(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>粗检文件头：MP3(ID3/帧同步)、OGG、WAV、AAC/ADTS；识别误标扩展名的 MP4。</summary>
    static bool LooksLikeAudioFile(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fs.Length < 8) return false;
            byte[] hdr = new byte[12];
            int n = fs.Read(hdr, 0, hdr.Length);
            if (n < 3) return false;

            // ISO BMFF / MP4: ....ftyp — 常被误命名为 .mp3
            if (n >= 8 && hdr[4] == (byte)'f' && hdr[5] == (byte)'t' && hdr[6] == (byte)'y' && hdr[7] == (byte)'p')
            {
                Debug.LogWarning($"[MusicMgr] 文件实际是 MP4/视频容器(ftyp)，不是音频: {Path.GetFileName(path)}");
                return false;
            }

            // ID3
            if (hdr[0] == (byte)'I' && hdr[1] == (byte)'D' && hdr[2] == (byte)'3') return true;
            // MP3 frame sync 0xFFEx
            if (hdr[0] == 0xFF && (hdr[1] & 0xE0) == 0xE0) return true;
            // OggS
            if (n >= 4 && hdr[0] == (byte)'O' && hdr[1] == (byte)'g' && hdr[2] == (byte)'g' && hdr[3] == (byte)'S')
                return true;
            // RIFF....WAVE
            if (n >= 12 && hdr[0] == (byte)'R' && hdr[1] == (byte)'I' && hdr[2] == (byte)'F' && hdr[3] == (byte)'F'
                && hdr[8] == (byte)'W' && hdr[9] == (byte)'A' && hdr[10] == (byte)'V' && hdr[11] == (byte)'E')
                return true;
            // fLaC
            if (n >= 4 && hdr[0] == (byte)'f' && hdr[1] == (byte)'L' && hdr[2] == (byte)'a' && hdr[3] == (byte)'C')
                return true;

            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext is ".mp3" or ".ogg" or ".wav" or ".aac" or ".flac" or ".aiff" or ".aif")
                return false;
            return fs.Length >= MinValidMusicBytes;
        }
        catch
        {
            return false;
        }
    }

    void TryDeleteLocalMusicAndPcm(string fileName)
    {
        string path = GetLocalPath(fileName);
        string part = GetPartPath(fileName);
        if (File.Exists(path))
            AudioClipPcmConverter.InvalidateDiskCacheForFile(path);
        TryDeleteFile(path);
        TryDeleteFile(part);
        Debug.Log($"[MusicMgr] 已清理本地音乐与临时文件: {fileName}");
    }

    static void TryDeleteFile(string path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MusicMgr] 删除失败 {path}: {e.Message}");
        }
    }

    /// <summary>根据文件扩展名推断 AudioType。</summary>
    private static AudioType DetectAudioType(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".mp3"          => AudioType.MPEG,
            ".ogg"          => AudioType.OGGVORBIS,
            ".wav"          => AudioType.WAV,
            ".aiff" or ".aif" => AudioType.AIFF,
            ".aac"          => AudioType.ACC,
            _               => AudioType.UNKNOWN,
        };
    }
}
