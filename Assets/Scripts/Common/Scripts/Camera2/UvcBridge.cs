using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UVC USB 摄像头桥接（Android system app 直读 /dev/video* + V4L2 + MJPEG）。
///
/// <h3>主线程 60fps 不掉帧约定（M0/M1）</h3>
/// <ul>
///   <li>启动：所有 JNI 调用 &lt; 1ms，V4L2 协商在 native worker 线程完成</li>
///   <li>稳态：每帧仅一次 fire-and-forget 上传，主线程开销 &lt; 1ms</li>
///   <li>关闭：仅发停止信号，资源由 reclaimer 后台线程释放</li>
///   <li>切换：用 <see cref="HasFirstFrame"/> 判定真就绪，避免黑帧；
///       配合 <see cref="UvcGlobalInit"/> 预热的纹理池，alloc 零 GL 阻塞</li>
/// </ul>
///
/// <h3>初始化时序（无主线程阻塞）</h3>
/// <ol>
///   <li>EGL 预热：由 <see cref="UvcGlobalInit"/> 在场景启动时已完成；本组件运行时仅校验</li>
///   <li>解析 preferredDevPath；如未指定，取第一个 UVC，并使用其宣告的最佳分辨率</li>
///   <li>allocPreviewTexture(W,H) → 池命中 &lt; 1ms，得到 textureId</li>
///   <li>CreateExternalTexture + 绑定 RawImage</li>
///   <li>openCamera() → JNI 立即返回；native worker 跑 STREAMON</li>
///   <li>协程每帧 yield return null 轮询 isFirstFrameReady → 置 <see cref="HasFirstFrame"/></li>
/// </ol>
/// </summary>
public class UvcBridge : MonoBehaviour
{
    private const string PLUGIN_CLASS = "com.serenegiant.unity.uvc.UvcUnityPlugin";
    private const string NATIVE_LIB   = "uvc_unity_plugin";

    /// <summary>
    /// 安卓上强制要求的开播分辨率——不管设备枚举上报的 best 分辨率是多少，统一按此请求，
    /// 保证画面规格稳定可预期（不同摄像头型号上报的"最佳"分辨率可能不一致）。
    /// 若设备并不精确支持该值，V4L2 <c>VIDIOC_S_FMT</c> 会在 native worker 线程自动协商到
    /// 最接近支持的模式（见 v4l2_capture.cpp initV4l2），全程不阻塞 Unity 主线程，可放心固定写死。
    /// </summary>
    public const int RequiredWidth  = 1920;
    public const int RequiredHeight = 1080;

    [DllImport(NATIVE_LIB)]
    private static extern IntPtr GetCaptureEglContextFuncPtr();

    [Header("UVC 选择")]
    [Tooltip("指定 /dev/videoN 完整路径，留空则取枚举到的第一个 USB 摄像头")]
    public string preferredDevPath = "";

    [Header("UI 引用")]
    public RawImage targetRawImage;
    public Renderer targetRenderer;

    [Header("调试")]
    public bool verboseLog = false;

    /// <summary>已开启摄像头的实际分辨率</summary>
    public int Width  { get; private set; }
    public int Height { get; private set; }
    public string OpenedDevPath { get; private set; }

    /// <summary>capture 是否已经派发（仅表示 openCamera 调用成功，不代表有画面）</summary>
    public bool IsRunning => mIsRunning;

    /// <summary>
    /// 首帧是否已经解码完成（native 端 V4l2Capture::hasFirstFrame）。
    /// 切换/swap 应当用此判定，能保证目标纹理已有可见画面，避免黑帧。
    /// </summary>
    public bool HasFirstFrame { get; private set; }

    public Texture PreviewTexture => mExternalTexture;
    public int TextureId          => mTextureId;
    public float RenderFps        => mCurrentRenderFps;

    private int mTextureId = -1;
    private Texture2D mExternalTexture;

    private AndroidJavaClass mPluginClass;
    private bool mIsRunning;
    private bool mInitializing;
    private bool mTexBoundToUi;   // 是否已把纹理 set 到 RawImage（首帧到达时 swap）

    // 统计
    private int   mRenderFrameCount;
    private float mRenderFpsTimer;
    private float mCurrentRenderFps;
    private float mFpsLogTimer;

    // -------------------------------------------------------------------------
    // 生命周期
    // -------------------------------------------------------------------------

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        LogGraphicsAPI();
        TryStartInitialize();
#else
        Debug.Log("[UvcBridge] 仅在 Android 真机生效，当前平台跳过");
#endif
    }

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!mIsRunning || mPluginClass == null || mTextureId < 0) return;

        // HasFirstFrame 首次置位：通过 native 信号；置位后立即把纹理 swap 到 UI（若延后绑定）
        if (!HasFirstFrame)
        {
            try
            {
                if (mPluginClass.CallStatic<bool>("isFirstFrameReady", mTextureId))
                {
                    HasFirstFrame = true;
                    BindTextureToUiIfNeeded();
                    if (verboseLog) Debug.Log($"[UvcBridge] HasFirstFrame texId={mTextureId} dev={OpenedDevPath}");
                }
            }
            catch (Exception ex) { Debug.LogWarning($"[UvcBridge] isFirstFrameReady: {ex.Message}"); }
        }

        // 每帧推动 GL 上传（fire-and-forget；Java 端不阻塞主线程）
        try
        {
            if (mPluginClass.CallStatic<bool>("isNewFrameAvailable", mTextureId))
            {
                mPluginClass.CallStatic("updateTexImage", mTextureId);

                mRenderFrameCount++;
                mRenderFpsTimer += Time.unscaledDeltaTime;
                if (mRenderFpsTimer >= 1f)
                {
                    mCurrentRenderFps = mRenderFrameCount / mRenderFpsTimer;
                    mRenderFrameCount = 0;
                    mRenderFpsTimer   = 0f;
                }
            }

            mFpsLogTimer += Time.unscaledDeltaTime;
            if (verboseLog && mFpsLogTimer >= 3f)
            {
                Debug.Log($"[UvcBridge] texId={mTextureId} renderFps={mCurrentRenderFps:F1}" +
                          $" cameraFps={GetCameraFps():F1} dev={OpenedDevPath}");
                mFpsLogTimer = 0f;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UvcBridge] Update 异常 texId={mTextureId}: {ex.Message}");
        }
#endif
    }

    void OnDestroy()        { CloseCamera(); }

    void OnApplicationPause(bool paused)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (paused) CloseCamera();
        else        TryStartInitialize();
#endif
    }

    void OnApplicationQuit()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { mPluginClass?.CallStatic("closeAllCameras"); }
        catch (Exception ex) { Debug.LogWarning("[UvcBridge] closeAllCameras: " + ex.Message); }
#endif
    }

    // -------------------------------------------------------------------------
    // 初始化（完全无主线程阻塞）
    // -------------------------------------------------------------------------

    private void TryStartInitialize()
    {
        if (mInitializing || mIsRunning) return;
        mInitializing = true;
        StartCoroutine(InitializeCoroutine());
    }

    private void LogGraphicsAPI()
    {
        var api = SystemInfo.graphicsDeviceType;
        Debug.Log($"[UvcBridge] Graphics API: {api}  GL: {SystemInfo.graphicsDeviceVersion}");
        if (api == UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
            Debug.LogError("[UvcBridge] 当前 Vulkan 后端！请只勾选 OpenGLES3");
    }

    private IEnumerator InitializeCoroutine()
    {
        if (mPluginClass == null)
            mPluginClass = new AndroidJavaClass(PLUGIN_CLASS);

        // 1) EGL 准备：若 UvcGlobalInit 已预热（推荐），这里只是确认；
        //    否则 fallback 自行初始化（会有一次性 ~100ms 主线程等待，仅首次切换发生）
        if (!UvcGlobalInit.IsEglReady)
        {
            yield return UvcGlobalInit.EnsureReady();
        }
        if (!UvcGlobalInit.IsEglReady)
        {
            Debug.LogError("[UvcBridge] EGL 未就绪");
            mInitializing = false;
            yield break;
        }

        // 2) 选定目标设备 + 最佳分辨率（枚举走异步通知，不阻塞主线程）
        List<UvcDeviceInfo> devices = null;
        yield return EnumerateDevicesAsync(list => devices = list);
        UvcDeviceInfo target = ResolveTargetDeviceFrom(devices);
        if (target == null)
        {
            Debug.LogError("[UvcBridge] 未发现 USB 摄像头");
            mInitializing = false;
            yield break;
        }
        Width  = RequiredWidth;
        Height = RequiredHeight;
        Debug.Log($"[UvcBridge] 选定 {target.devPath}（设备上报 best={target.width}x{target.height}@{target.fps}fps，" +
                  $"按项目要求固定用 {Width}x{Height} 打开）");

        // 3) 分配纹理（池命中 < 1ms）
        int texId = -1;
        try { texId = mPluginClass.CallStatic<int>("allocPreviewTexture", Width, Height); }
        catch (Exception ex) { Debug.LogError($"[UvcBridge] allocPreviewTexture: {ex.Message}"); }
        if (texId <= 0)
        {
            Debug.LogError($"[UvcBridge] 分配纹理失败 id={texId}");
            mInitializing = false;
            yield break;
        }
        mTextureId = texId;

        mExternalTexture = Texture2D.CreateExternalTexture(
            Width, Height, TextureFormat.RGBA32, false, true, new IntPtr(texId));
        mExternalTexture.filterMode = FilterMode.Bilinear;
        mExternalTexture.wrapMode   = TextureWrapMode.Clamp;
        // 注意：先不 set 到 UI；等首帧到达再 swap，避免目标 RawImage 显示一帧黑色纹理
        mTexBoundToUi = false;

        // 4) 异步开启 V4L2（立即返回，初始化在 native worker）
        string result = null;
        try
        {
            result = mPluginClass.CallStatic<string>(
                "openCamera", target.devPath, texId, Width, Height);
        }
        catch (Exception ex) { Debug.LogError($"[UvcBridge] openCamera: {ex.Message}"); }
        if (string.IsNullOrEmpty(result))
        {
            Debug.LogError("[UvcBridge] openCamera 失败");
            CloseCamera();
            mInitializing = false;
            yield break;
        }
        OpenedDevPath = result;
        mIsRunning = true;
        mInitializing = false;

        Debug.Log($"[UvcBridge] 异步启动 dev={result} texId={texId} {Width}x{Height}");
        // 不在协程里 await 首帧；让 Update() 每帧检测 isFirstFrameReady 并 swap UI
    }

    /// <summary>首帧到达后把纹理绑到目标 UI。可被外部"延迟绑定"覆盖（DetachDisplayTargetForHandoff）</summary>
    private void BindTextureToUiIfNeeded()
    {
        if (mTexBoundToUi || mExternalTexture == null) return;
        if (targetRawImage != null)
        {
            targetRawImage.texture = mExternalTexture;
            targetRawImage.color   = Color.white;
            ApplyVerticalFlipToRawImage(targetRawImage);
            mTexBoundToUi = true;
        }
        else if (targetRenderer != null && targetRenderer.material != null)
        {
            targetRenderer.material.mainTexture = mExternalTexture;
            targetRenderer.material.color = Color.white;
            ApplyVerticalFlipToRenderer(targetRenderer);
            mTexBoundToUi = true;
        }
    }

    // -------------------------------------------------------------------------
    // UV 翻转工具
    // -------------------------------------------------------------------------
    // V4L2/MJPEG 解码得到的 RGBA 是 top-down（第一行 = 图像顶部），
    // 而 OpenGL 纹理是 bottom-up（纹理坐标 (0,0) 在左下），直接上传后
    // Unity 取到的纹理上下颠倒。通过 UV 翻转在采样阶段修正，
    // 零 GPU 拷贝、零 CPU 翻转，性能最优。
    // -------------------------------------------------------------------------

    /// <summary>RawImage UV 翻转矩形：起点 (0,1)，宽 1，高 -1，即"从顶往下"反向采样</summary>
    private static readonly Rect kFlipYUvRect = new Rect(0f, 1f, 1f, -1f);

    /// <summary>RawImage UV 恢复矩形：默认全图正向采样</summary>
    private static readonly Rect kIdentityUvRect = new Rect(0f, 0f, 1f, 1f);

    /// <summary>
    /// 给 RawImage 套上垂直翻转 UV（用于显示 UVC/V4L2 纹理）。
    /// 双缓冲切换时若直接覆盖 RawImage.texture 而不走 BindTextureToUiIfNeeded，
    /// 调用方需手动调用本方法保持方向正确。
    /// </summary>
    public static void ApplyVerticalFlipToRawImage(RawImage img)
    {
        if (img != null) img.uvRect = kFlipYUvRect;
    }

    /// <summary>把 RawImage UV 恢复为默认正向，避免 UVC 关闭后再显示普通纹理出现倒置</summary>
    public static void ResetOrientationOnRawImage(RawImage img)
    {
        if (img != null) img.uvRect = kIdentityUvRect;
    }

    /// <summary>给 Renderer.material 套上垂直翻转 UV（mainTextureScale.y=-1, mainTextureOffset.y=1）</summary>
    public static void ApplyVerticalFlipToRenderer(Renderer r)
    {
        if (r == null || r.material == null) return;
        r.material.mainTextureScale  = new Vector2(1f, -1f);
        r.material.mainTextureOffset = new Vector2(0f, 1f);
    }

    /// <summary>把 Renderer.material UV 恢复为默认</summary>
    public static void ResetOrientationOnRenderer(Renderer r)
    {
        if (r == null || r.material == null) return;
        r.material.mainTextureScale  = Vector2.one;
        r.material.mainTextureOffset = Vector2.zero;
    }

    /// <summary>选定目标设备：优先 preferredDevPath，否则取第一个</summary>
    private UvcDeviceInfo ResolveTargetDeviceFrom(List<UvcDeviceInfo> list)
    {
        if (list == null || list.Count == 0) return null;

        if (!string.IsNullOrEmpty(preferredDevPath))
        {
            foreach (var d in list)
                if (d.devPath == preferredDevPath) return d;
            Debug.LogWarning($"[UvcBridge] 未找到 {preferredDevPath}，使用第一个 {list[0].devPath}");
        }
        return list[0];
    }

    // -------------------------------------------------------------------------
    // 关闭（非阻塞）
    // -------------------------------------------------------------------------

    public void CloseCamera()
    {
        if (!mIsRunning && mTextureId < 0) return;
        mIsRunning = false;
        HasFirstFrame = false;
        mCurrentRenderFps = 0f;
        mRenderFrameCount = 0;
        mRenderFpsTimer   = 0f;
        mFpsLogTimer      = 0f;

        int tex = mTextureId;
        mTextureId = -1;
        OpenedDevPath = null;

        if (tex >= 0 && mPluginClass != null)
        {
            // 主线程仅两次 JNI 调用，每次 < 1ms
            try { mPluginClass.CallStatic("closeCamera", tex); }
            catch (Exception ex) { Debug.LogWarning($"[UvcBridge] closeCamera: {ex.Message}"); }
            try { mPluginClass.CallStatic("deletePreviewTexture", tex); }
            catch (Exception ex) { Debug.LogWarning($"[UvcBridge] deletePreviewTexture: {ex.Message}"); }
        }

        if (mTexBoundToUi)
        {
            if (targetRawImage != null)
            {
                targetRawImage.texture = null;
                ResetOrientationOnRawImage(targetRawImage);
            }
            if (targetRenderer != null && targetRenderer.material != null)
            {
                targetRenderer.material.mainTexture = null;
                ResetOrientationOnRenderer(targetRenderer);
            }
            mTexBoundToUi = false;
        }

        if (mExternalTexture != null)
        {
            Destroy(mExternalTexture);
            mExternalTexture = null;
        }
    }

    // -------------------------------------------------------------------------
    // 公共查询 / 工具
    // -------------------------------------------------------------------------

    /// <summary>native 端统计的硬件帧率（每秒刷新）</summary>
    public float GetCameraFps()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!mIsRunning || mPluginClass == null || mTextureId < 0) return 0f;
        try { return mPluginClass.CallStatic<float>("getCameraFps", mTextureId); }
        catch { return 0f; }
#else
        return 0f;
#endif
    }

    // -------------------------------------------------------------------------
    // 设备枚举（静态 + 缓存，避免每次切换都 ioctl）
    // -------------------------------------------------------------------------

    [Serializable]
    public class UvcDeviceInfo
    {
        public string id;
        public string devPath;
        public string vendor;
        public string product;
        public string name;
        public int    width;
        public int    height;
        public int    fps;

        public override string ToString() =>
            $"{devPath} [{vendor}:{product} \"{name}\"] {width}x{height}@{fps}";
    }

    // 缓存 5 秒；切换/枚举高频时不重复 ioctl
    private static readonly object sCacheLock = new object();
    private static List<UvcDeviceInfo> sCache;
    private static float sCacheTime = -1f;
    private const float kCacheTtl = 5f;

    /// <summary>枚举当前设备上所有 USB UVC 摄像头（带 5 秒缓存，主线程 &lt; 1ms 命中）</summary>
    public static List<UvcDeviceInfo> EnumerateDevices()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return EnumerateDevicesCached();
#else
        return new List<UvcDeviceInfo>();
#endif
    }

    private static List<UvcDeviceInfo> EnumerateDevicesCached()
    {
        lock (sCacheLock)
        {
            if (sCache != null && Time.realtimeSinceStartup - sCacheTime < kCacheTtl)
                return sCache;
        }

        var list = new List<UvcDeviceInfo>();
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var plugin = new AndroidJavaClass(PLUGIN_CLASS);
            string json = plugin.CallStatic<string>("enumerateCameras");
            ParseDeviceArray(json, list);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[UvcBridge] EnumerateDevices: {ex.Message}");
        }
#endif
        lock (sCacheLock)
        {
            sCache = list;
            sCacheTime = Time.realtimeSinceStartup;
        }
        return list;
    }

    private static bool sEnumPending;

    /// <summary>
    /// 异步枚举（带 5 秒缓存）：缓存命中直接回调；未命中时下发到 native 后台线程扫描，
    /// 完成后通过 <see cref="OnEnumerateResultFromNative"/> 回调写入缓存，全程不阻塞主线程。
    /// </summary>
    public static IEnumerator EnumerateDevicesAsync(Action<List<UvcDeviceInfo>> onDone)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        lock (sCacheLock)
        {
            if (sCache != null && Time.realtimeSinceStartup - sCacheTime < kCacheTtl)
            {
                onDone?.Invoke(sCache);
                yield break;
            }
        }

        // 已有别处在扫，跟着等同一次结果即可，不重复下发
        if (!sEnumPending)
        {
            sEnumPending = true;
            try
            {
                using var plugin = new AndroidJavaClass(PLUGIN_CLASS);
                plugin.CallStatic("enumerateCamerasAsync");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UvcBridge] enumerateCamerasAsync: {ex.Message}");
                sEnumPending = false;
                onDone?.Invoke(new List<UvcDeviceInfo>());
                yield break;
            }
        }

        while (sEnumPending) yield return null;
        onDone?.Invoke(sCache ?? new List<UvcDeviceInfo>());
#else
        onDone?.Invoke(new List<UvcDeviceInfo>());
        yield break;
#endif
    }

    /// <summary>由 <c>UvcGlobalInit.OnUvcEnumerateResult</c> 收到 Java 端 UnitySendMessage 回调后调用（主线程）。</summary>
    internal static void OnEnumerateResultFromNative(string json)
    {
        var list = new List<UvcDeviceInfo>();
        ParseDeviceArray(json, list);
        lock (sCacheLock)
        {
            sCache = list;
            sCacheTime = Time.realtimeSinceStartup;
        }
        sEnumPending = false;
    }

    /// <summary>强制刷新枚举缓存（如热插拔后）</summary>
    public static void InvalidateEnumerationCache()
    {
        lock (sCacheLock) { sCache = null; sCacheTime = -1f; }
    }

    /// <summary>仅返回 devPath 列表</summary>
    public static List<string> EnumerateDevPaths()
    {
        var list = EnumerateDevices();
        var paths = new List<string>(list.Count);
        foreach (var d in list) paths.Add(d.devPath);
        return paths;
    }

    // -------------------------------------------------------------------------
    // JSON 解析（极简）
    // -------------------------------------------------------------------------

    private static void ParseDeviceArray(string json, List<UvcDeviceInfo> outList)
    {
        if (string.IsNullOrEmpty(json)) return;
        int i = 0;
        while (i < json.Length)
        {
            int objStart = json.IndexOf('{', i);
            if (objStart < 0) return;
            int objEnd = json.IndexOf('}', objStart);
            if (objEnd < 0) return;
            string obj = json.Substring(objStart, objEnd - objStart + 1);
            outList.Add(new UvcDeviceInfo
            {
                id      = JsonStr(obj, "id"),
                devPath = JsonStr(obj, "devPath"),
                vendor  = JsonStr(obj, "vendor"),
                product = JsonStr(obj, "product"),
                name    = JsonStr(obj, "name"),
                width   = JsonInt(obj, "width"),
                height  = JsonInt(obj, "height"),
                fps     = JsonInt(obj, "fps"),
            });
            i = objEnd + 1;
        }
    }

    private static string JsonStr(string obj, string key)
    {
        string needle = "\"" + key + "\":\"";
        int p = obj.IndexOf(needle, StringComparison.Ordinal);
        if (p < 0) return "";
        int s = p + needle.Length;
        int e = obj.IndexOf('"', s);
        return e < 0 ? "" : obj.Substring(s, e - s);
    }

    private static int JsonInt(string obj, string key)
    {
        string needle = "\"" + key + "\":";
        int p = obj.IndexOf(needle, StringComparison.Ordinal);
        if (p < 0) return 0;
        int s = p + needle.Length;
        int e = s;
        while (e < obj.Length)
        {
            char c = obj[e];
            if ((c >= '0' && c <= '9') || c == '-') e++;
            else break;
        }
        if (e == s) return 0;
        return int.TryParse(obj.Substring(s, e - s), out var v) ? v : 0;
    }

    // -------------------------------------------------------------------------
    // 内部：给 UvcGlobalInit 复用的 EGL 捕获入口
    // -------------------------------------------------------------------------

    internal static IntPtr InternalCaptureFuncPtr()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { return GetCaptureEglContextFuncPtr(); }
        catch { return IntPtr.Zero; }
#else
        return IntPtr.Zero;
#endif
    }
}
