using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// APK 静默下载 + 升级工具类（MonoBehaviour 单例）。
///
/// 完整升级流程：
/// <code>
///   下载 APK (UnityWebRequest + 断点续传)
///       ↓ 下载完成
///   AppUpdaterBridge.installApk() → pm install -r (系统签名 Shell 命令)
///       ↓ 系统替换 APK，杀掉旧进程
///   PackageReceiver.onReceive(MY_PACKAGE_REPLACED)
///       ↓ 新版进程启动
///   MainActivity 自动启动（无需重启设备）
/// </code>
///
/// 依赖：
///   - APK 以 android:sharedUserId="android.uid.system" 系统签名安装
///   - AndroidManifest.xml 已注册 PackageReceiver（MY_PACKAGE_REPLACED）
///   - Assets/Plugins/Android/AppUpdaterBridge.java
///
/// 使用示例：
/// <code>
///   AppUpdater.Instance.StartUpdate(
///       url:        "http://example.com/release/app_v1.2.apk",
///       onProgress: (p, state) => Debug.Log($"{state}: {p*100:F0}%"),
///       onComplete: ()         => Debug.Log("安装已触发，等待系统重启应用..."),
///       onError:    err        => Debug.LogError("升级失败: " + err));
/// </code>
/// </summary>
public class AppUpdater : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════════
    //  常量
    // ════════════════════════════════════════════════════════════════════

#if UNITY_ANDROID && !UNITY_EDITOR
    private const string BridgeClass = "com.glsa.mqttscorecard5s.unity.AppUpdaterBridge";
#endif

    /// <summary>APK 下载文件名（保存在应用私有临时目录）。</summary>
    private const string ApkFileName = "app_update.apk";

    // ════════════════════════════════════════════════════════════════════
    //  升级状态枚举
    // ════════════════════════════════════════════════════════════════════

    public enum UpdateState
    {
        /// <summary>空闲，未开始</summary>
        Idle,
        /// <summary>正在下载 APK</summary>
        Downloading,
        /// <summary>下载完成，正在执行安装</summary>
        Installing,
        /// <summary>安装成功（系统即将杀掉当前进程）</summary>
        Success,
        /// <summary>升级失败</summary>
        Failed,
        /// <summary>已取消</summary>
        Cancelled,
    }

    // ════════════════════════════════════════════════════════════════════
    //  单例
    // ════════════════════════════════════════════════════════════════════

    private static AppUpdater _instance;

    public static AppUpdater Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[AppUpdater]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AppUpdater>();
            return _instance;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  状态属性
    // ════════════════════════════════════════════════════════════════════

    /// <summary>当前升级状态。</summary>
    public UpdateState State { get; private set; } = UpdateState.Idle;

    /// <summary>当前下载进度（0~1）。</summary>
    public float DownloadProgress { get; private set; }

    /// <summary>是否正在进行升级（下载中或安装中）。</summary>
    public bool IsUpdating => State == UpdateState.Downloading || State == UpdateState.Installing;

    // ════════════════════════════════════════════════════════════════════
    //  Unity 生命周期
    // ════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        _updateCoroutine = null;
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开 API
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 启动完整的下载 + 静默安装升级流程。
    ///
    /// 若已有升级任务正在进行，直接返回 false（不重复触发）。
    /// </summary>
    /// <param name="url">APK 下载地址</param>
    /// <param name="onProgress">
    ///   进度回调 (float progress 0~1, UpdateState state)。
    ///   Downloading 阶段持续触发；Installing 阶段触发一次（progress=1）。
    /// </param>
    /// <param name="onComplete">
    ///   安装命令发出后触发（不代表安装已完成，系统随后会杀掉当前进程并通过
    ///   PackageReceiver 重启新版本）。
    /// </param>
    /// <param name="onError">错误回调（下载失败或安装命令返回错误）</param>
    /// <param name="forceRedownload">true = 忽略本地已有缓存，强制重新下载</param>
    /// <param name="apkFileName">
    ///   本地缓存文件名。不同版本应使用不同文件名，避免断点续传把新旧版本拼接成损坏的 APK；
    ///   下载前会删除缓存目录中其他 .apk 文件。
    /// </param>
    /// <returns>true = 成功启动；false = 已有任务进行中</returns>
    public bool StartUpdate(string url,
        Action<float, UpdateState> onProgress = null,
        Action onComplete = null,
        Action<string> onError = null,
        bool forceRedownload = false,
        string apkFileName = ApkFileName)
    {
        if (IsUpdating)
        {
            Debug.LogWarning("[AppUpdater] 已有升级任务进行中，忽略重复调用");
            return false;
        }
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("[AppUpdater] StartUpdate: URL 为空");
            onError?.Invoke("URL 为空");
            return false;
        }

        _cancelRequested = false;
        _updateCoroutine = StartCoroutine(UpdateFlow(url, GetApkSavePath(apkFileName), forceRedownload, onProgress, onComplete, onError));
        return true;
    }

    /// <summary>
    /// 仅执行安装（跳过下载），适用于 APK 文件已在本地的场景。
    /// </summary>
    /// <param name="apkPath">本地 APK 绝对路径</param>
    /// <param name="onComplete">安装命令发出后回调</param>
    /// <param name="onError">安装失败回调</param>
    public void InstallApk(string apkPath,
        Action onComplete = null,
        Action<string> onError = null)
    {
        if (!File.Exists(apkPath))
        {
            string err = $"APK 文件不存在: {apkPath}";
            Debug.LogError("[AppUpdater] " + err);
            onError?.Invoke(err);
            return;
        }

        _pendingInstallComplete = onComplete;
        _pendingInstallError    = onError;

        State = UpdateState.Installing;
        Debug.Log($"[AppUpdater] 开始安装: {apkPath}");

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var ctx = GetActivity();
            using var jc  = new AndroidJavaClass(BridgeClass);
            jc.CallStatic("installApk", ctx, apkPath, gameObject.name);
        }
        catch (Exception ex)
        {
            State = UpdateState.Failed;
            string err = $"installApk 调用异常: {ex.Message}";
            Debug.LogError("[AppUpdater] " + err);
            onError?.Invoke(err);
        }
#else
        Debug.Log($"[AppUpdater] InstallApk (Editor mock): {apkPath}");
        State = UpdateState.Success;
        onComplete?.Invoke();
#endif
    }

    /// <summary>取消正在进行的下载任务（安装阶段无法取消）。</summary>
    public void Cancel()
    {
        if (State == UpdateState.Downloading)
        {
            _cancelRequested = true;
            Debug.Log("[AppUpdater] 已请求取消下载");
        }
    }

    /// <summary>删除本地缓存的 APK 文件（清理磁盘空间用）。</summary>
    public void DeleteCachedApk()
    {
        string path = GetApkSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[AppUpdater] 已删除缓存 APK: {path}");
        }
    }

    /// <summary>返回当前设备已安装 APK 的版本号（versionCode）。</summary>
    public int GetInstalledVersionCode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var ctx = GetActivity();
            using var jc  = new AndroidJavaClass(BridgeClass);
            return jc.CallStatic<int>("getInstalledVersionCode", ctx);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AppUpdater] GetInstalledVersionCode 失败: {ex.Message}");
            return 0;
        }
#else
        if (int.TryParse(Application.version.Replace(".", ""), out int code)) return code;
        return 0;
#endif
    }

    /// <summary>返回当前设备已安装 APK 的版本名称（versionName）。</summary>
    public string GetInstalledVersionName()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var ctx = GetActivity();
            using var jc  = new AndroidJavaClass(BridgeClass);
            return jc.CallStatic<string>("getInstalledVersionName", ctx) ?? Application.version;
        }
        catch
        {
            return Application.version;
        }
#else
        return Application.version;
#endif
    }

    // ════════════════════════════════════════════════════════════════════
    //  内部：升级主流程 Coroutine
    // ════════════════════════════════════════════════════════════════════

    private Coroutine _updateCoroutine;
    private bool      _cancelRequested;
    private Action    _pendingInstallComplete;
    private Action<string> _pendingInstallError;

    private IEnumerator UpdateFlow(string url, string apkPath, bool forceRedownload,
        Action<float, UpdateState> onProgress,
        Action onComplete,
        Action<string> onError)
    {
        State          = UpdateState.Downloading;
        DownloadProgress = 0f;

        foreach (string stale in Directory.GetFiles(Path.GetDirectoryName(apkPath), "*.apk"))
        {
            if (stale == apkPath) continue;
            try { File.Delete(stale); Debug.Log($"[AppUpdater] 已删除旧版本缓存: {stale}"); }
            catch (Exception ex) { Debug.LogWarning($"[AppUpdater] 删除旧缓存失败: {stale} {ex.Message}"); }
        }

        // ── Step 1: 下载 APK（支持断点续传）─────────────────────────────
        yield return StartCoroutine(DownloadApk(url, apkPath, forceRedownload,
            progress =>
            {
                DownloadProgress = progress;
                onProgress?.Invoke(progress, UpdateState.Downloading);
            },
            downloadError =>
            {
                State = UpdateState.Failed;
                Debug.LogError("[AppUpdater] 下载失败: " + downloadError);
                onError?.Invoke(downloadError);
            }));

        // 下载失败或被取消
        if (State == UpdateState.Failed || State == UpdateState.Cancelled)
            yield break;

        // ── Step 2: 安装 ──────────────────────────────────────────────────
        onProgress?.Invoke(1f, UpdateState.Installing);

        _pendingInstallComplete = onComplete;
        _pendingInstallError    = onError;

        InstallApk(apkPath, onComplete, onError);
    }

    // ════════════════════════════════════════════════════════════════════
    //  内部：断点续传下载
    // ════════════════════════════════════════════════════════════════════

    private IEnumerator DownloadApk(string url, string destPath, bool forceRedownload,
        Action<float> onProgress, Action<string> onError)
    {
        if (forceRedownload && File.Exists(destPath))
        {
            File.Delete(destPath);
            Debug.Log("[AppUpdater] 强制重新下载，已删除旧缓存");
        }

        long existingSize = File.Exists(destPath) ? new FileInfo(destPath).Length : 0;
        long serverSize   = -1;
        bool supportsRange = false;

        // ── HEAD 请求 ─────────────────────────────────────────────────
        using (var headReq = UnityWebRequest.Head(url))
        {
            headReq.timeout = 10;
            yield return headReq.SendWebRequest();

            if (headReq.result == UnityWebRequest.Result.Success)
            {
                if (long.TryParse(headReq.GetResponseHeader("Content-Length"), out long sz))
                    serverSize = sz;

                string acceptRanges = headReq.GetResponseHeader("Accept-Ranges");
                supportsRange = !string.IsNullOrEmpty(acceptRanges)
                                && !acceptRanges.Equals("none", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Debug.LogWarning($"[AppUpdater] HEAD 失败（{headReq.error}），降级为全量下载");
            }
        }

        // ── 本地文件已完整？ ──────────────────────────────────────────
        if (existingSize > 0 && serverSize > 0 && existingSize >= serverSize)
        {
            Debug.Log($"[AppUpdater] APK 已完整缓存（{existingSize} bytes），跳过下载");
            onProgress?.Invoke(1f);
            yield break;
        }

        // ── GET 下载（可能续传）───────────────────────────────────────
        bool resume    = existingSize > 0 && supportsRange;
        long rangeStart = resume ? existingSize : 0;

        Debug.Log($"[AppUpdater] {(resume ? $"续传 from {rangeStart} bytes" : "全量下载")} → {destPath}");

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {
            req.downloadHandler = new DownloadHandlerFile(destPath, resume);
            if (resume) req.SetRequestHeader("Range", $"bytes={rangeStart}-");
            req.timeout = 0;

            var op = req.SendWebRequest();

            while (!op.isDone)
            {
                if (_cancelRequested)
                {
                    req.Abort();
                    State = UpdateState.Cancelled;
                    Debug.Log("[AppUpdater] 下载已取消");
                    yield break;
                }

                float progress = serverSize > 0
                    ? Mathf.Clamp01((float)(rangeStart + (long)req.downloadedBytes) / serverSize)
                    : req.downloadProgress;
                onProgress?.Invoke(progress);
                yield return null;
            }

            bool success = req.result == UnityWebRequest.Result.Success
                           || req.responseCode == 206;

            if (req.responseCode == 416)
            {
                // 416 = 本地文件已完整
                Debug.Log("[AppUpdater] 416 Range Not Satisfiable，视为下载完成");
                onProgress?.Invoke(1f);
                yield break;
            }

            if (!success)
            {
                onError?.Invoke($"HTTP {req.responseCode}: {req.error}");
                yield break;
            }

            onProgress?.Invoke(1f);
            Debug.Log($"[AppUpdater] APK 下载完成 → {destPath}");
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  UnitySendMessage 回调（由 AppUpdaterBridge.java 调用）
    // ════════════════════════════════════════════════════════════════════

    // ReSharper disable UnusedMember.Local

    private void OnApkInstallSuccess(string apkPath)
    {
        State = UpdateState.Success;
        Debug.Log($"[AppUpdater] 安装命令执行成功: {apkPath}");
        Debug.Log("[AppUpdater] 等待系统发送 MY_PACKAGE_REPLACED → PackageReceiver 将自动重启 MainActivity");

        // 删除缓存（如果进程还没被杀掉）
        try { if (File.Exists(apkPath)) File.Delete(apkPath); }
        catch { /* 被系统杀掉时可能来不及，忽略 */ }

        _pendingInstallComplete?.Invoke();
        _pendingInstallComplete = null;
        _pendingInstallError    = null;
    }

    private void OnApkInstallError(string errorMsg)
    {
        State = UpdateState.Failed;
        Debug.LogError($"[AppUpdater] 安装失败: {errorMsg}");
        _pendingInstallError?.Invoke(errorMsg);
        _pendingInstallComplete = null;
        _pendingInstallError    = null;
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有工具
    // ════════════════════════════════════════════════════════════════════

    /// <summary>APK 本地保存路径。使用应用私有临时目录，无需存储权限。</summary>
    private static string GetApkSavePath(string fileName = ApkFileName)
        => Path.Combine(Application.temporaryCachePath, fileName);

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject GetActivity()
    {
        using var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return up.GetStatic<AndroidJavaObject>("currentActivity");
    }
#endif
}
