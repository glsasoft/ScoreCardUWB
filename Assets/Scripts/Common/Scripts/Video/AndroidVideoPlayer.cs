using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 通用 Android 硬解码视频播放器（可在任意面板复用）。
///
/// 走自研 Android 插件（com.serenegiant.unity.uvc.VideoUnityPlugin）：
/// MediaCodec 硬解码 → OES SurfaceTexture → 共享 GL 线程采样到 GL_TEXTURE_2D → Unity 直接显示。
/// 复用 UVC 插件的共享 EGL 环境与纹理池，规避 Unity 自带 VideoPlayer 在部分 Mali 驱动上
/// 因 GL_TEXTURE_EXTERNAL_OES 直采导致的 gles_error 0x0003 整屏黑问题。
///
/// <h3>用法</h3>
/// <pre>
///   // 挂在任意 GameObject，指定 targetRawImage
///   var player = go.GetComponent&lt;AndroidVideoPlayer&gt;();
///   player.OnCompleted += () =&gt; { /* 播放结束 */ };
///   player.Play("Video/Movie.mp4");          // StreamingAssets 相对路径
///   // 或
///   yield return player.PlayAndWait("Video/Movie.mp4");
/// </pre>
///
/// 桶上下载的视频直接传绝对路径。仍打包在 StreamingAssets 里的视频（倒计时、回放环）
/// 在首次播放时拷到可读路径。
/// </summary>
public class AndroidVideoPlayer : MonoBehaviour
{
    private const string PLUGIN_CLASS = "com.serenegiant.unity.uvc.VideoUnityPlugin";

    [Header("显示目标")]
    [Tooltip("视频画面输出到该 RawImage")]
    public RawImage targetRawImage;

    [Header("播放设置")]
    [Tooltip("StreamingAssets 下的相对路径，如 \"Video/Movie.mp4\"；留空则用 Play(path) 传入")]
    public string streamingAssetsPath = "";
    public bool loop = false;
    [Tooltip("组件启用时自动播放 streamingAssetsPath")]
    public bool playOnEnable = false;
    [Tooltip("画面上下颠倒时勾选（不同解码路径方向可能不同）")]
    public bool flipVertically = false;
    [Tooltip("是否解码播放该视频自带的音轨。多个播放器同源视频时应只让其中一个播音频，" +
        "否则两路各自独立解码音频、时钟不完全一致会出现叠音/回声。")]
    public bool enableAudio = true;
    [Tooltip("同步时钟来源（可选）：指定后本播放器的播放进度会对齐该播放器的音频时钟，而不是自己独立走时钟。" +
        "用于多路播放器播同一份视频时保持画面彼此同步（配合另一路 enableAudio=true 使用）。")]
    public AndroidVideoPlayer clockMaster;

    /// <summary>首帧渲染完成（已 swap 到 UI）</summary>
    public event Action OnFirstFrame;
    /// <summary>非循环视频播放完毕时触发（仅一次）。启动失败时也会触发，避免上层等待方永久挂起。</summary>
    public event Action OnCompleted;

    public bool IsPlaying     { get; private set; }
    public bool HasFirstFrame { get; private set; }
    public bool IsFinished    { get; private set; }
    public int  Width         { get; private set; }
    public int  Height        { get; private set; }
    public int  TextureId     => mTextureId;
    public Texture VideoTexture => mExternalTexture;

    private int mTextureId = -1;
    private Texture2D mExternalTexture;
    private AndroidJavaClass mPlugin;
    private bool mTexBound;
    private bool mCompletedFired;
    private Coroutine mPlayCo;

    private static readonly Rect kFlipYUvRect   = new Rect(0f, 1f, 1f, -1f);
    private static readonly Rect kIdentityUvRect = new Rect(0f, 0f, 1f, 1f);

    void OnEnable()
    {
        if (playOnEnable && !string.IsNullOrEmpty(streamingAssetsPath))
            Play(streamingAssetsPath);
    }

    void OnDisable() { Stop(); }
    void OnDestroy() { Stop(); }

    void OnApplicationPause(bool paused)
    {
        if (paused) Stop();
    }

    // -------------------------------------------------------------------------
    // 对外 API
    // -------------------------------------------------------------------------

    /// <summary>播放（StreamingAssets 相对路径或绝对路径）。会先停掉当前播放。</summary>
    public void Play(string path)
    {
        Stop();
#if UNITY_ANDROID && !UNITY_EDITOR
        mPlayCo = StartCoroutine(PlayCoroutine(path));
#else
        Debug.Log($"[AndroidVideoPlayer] 仅 Android 真机生效，当前平台跳过：{path}");
#endif
    }

    /// <summary>播放默认 streamingAssetsPath</summary>
    public void Play()
    {
        if (string.IsNullOrEmpty(streamingAssetsPath))
        {
            Debug.LogError("[AndroidVideoPlayer] streamingAssetsPath 未设置");
            return;
        }
        Play(streamingAssetsPath);
    }

    /// <summary>播放并等待结束（非循环）。循环视频会一直不返回。</summary>
    public IEnumerator PlayAndWait(string path)
    {
        Play(path);
        while (!IsFinished) yield return null;
    }

    /// <summary>停止播放并释放资源</summary>
    public void Stop()
    {
        if (mPlayCo != null) { StopCoroutine(mPlayCo); mPlayCo = null; }
#if UNITY_ANDROID && !UNITY_EDITOR
        if (mTextureId > 0 && mPlugin != null)
        {
            try { mPlugin.CallStatic("closeVideo", mTextureId); }
            catch (Exception ex) { Debug.LogWarning($"[AndroidVideoPlayer] closeVideo: {ex.Message}"); }
        }
#endif
        if (mTexBound && targetRawImage != null)
        {
            targetRawImage.texture = null;
            targetRawImage.uvRect  = kIdentityUvRect;
            // 首帧绑定时 color 会被置为白色（见 BindFirstFrame）；纹理清空后若不还原，
            // RawImage 在 texture==null 时会用内置白贴图渲染，整屏变白，而不是黑。
            targetRawImage.color   = Color.black;
        }
        if (mExternalTexture != null) { Destroy(mExternalTexture); mExternalTexture = null; }

        mTextureId      = -1;
        mTexBound       = false;
        IsPlaying       = false;
        HasFirstFrame   = false;
        IsFinished      = false;
        mCompletedFired = false;
    }

    /// <summary>暂停/恢复（暂停时冻结在当前帧）。仅 Android 生效。</summary>
    public void SetPaused(bool paused)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (mPlugin == null || mTextureId <= 0) return;
        try { mPlugin.CallStatic("setPaused", mTextureId, paused); }
        catch (Exception ex) { Debug.LogWarning($"[AndroidVideoPlayer] setPaused: {ex.Message}"); }
#endif
    }

    /// <summary>按归一化进度 [0,1] 跳转（同步帧精度）。用于正/反视频切换的无缝对齐。仅 Android 生效。</summary>
    public void SeekNormalized(float t)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (mPlugin == null || mTextureId <= 0) return;
        try { mPlugin.CallStatic("seekNormalized", mTextureId, Mathf.Clamp01(t)); }
        catch (Exception ex) { Debug.LogWarning($"[AndroidVideoPlayer] seekNormalized: {ex.Message}"); }
#endif
    }

    /// <summary>当前归一化播放进度 [0,1]。仅 Android 有效，其它平台返回 0。</summary>
    public float NormalizedTime
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (mPlugin == null || mTextureId <= 0) return 0f;
            try { return mPlugin.CallStatic<float>("getNormalizedTime", mTextureId); }
            catch { return 0f; }
#else
            return 0f;
#endif
        }
    }

    /// <summary>标记完成并触发 OnCompleted（幂等）。正常播放完毕或启动失败时调用。</summary>
    private void FireCompletedOnce()
    {
        if (mCompletedFired) return;
        mCompletedFired = true;
        IsFinished = true;
        OnCompleted?.Invoke();
    }

    // -------------------------------------------------------------------------
    // 每帧驱动
    // -------------------------------------------------------------------------

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsPlaying || mPlugin == null || mTextureId <= 0) return;

        if (!HasFirstFrame)
        {
            try
            {
                if (mPlugin.CallStatic<bool>("isFirstFrameReady", mTextureId))
                {
                    HasFirstFrame = true;
                    BindTextureToUi();
                    OnFirstFrame?.Invoke();
                }
            }
            catch (Exception ex) { Debug.LogWarning($"[AndroidVideoPlayer] isFirstFrameReady: {ex.Message}"); }
        }

        try
        {
            if (mPlugin.CallStatic<bool>("isNewFrameAvailable", mTextureId))
                mPlugin.CallStatic("updateTexImage", mTextureId);

            if (!loop && mPlugin.CallStatic<bool>("isFinished", mTextureId))
                FireCompletedOnce();
        }
        catch (Exception ex) { Debug.LogError($"[AndroidVideoPlayer] Update: {ex.Message}"); }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator PlayCoroutine(string path)
    {
        // 1) 共享 EGL 就绪（复用 UVC 引导，静态自举，不依赖场景组件）
        if (!UvcGlobalInit.IsEglReady)
            yield return UvcGlobalInit.EnsureReady();
        if (!UvcGlobalInit.IsEglReady)
        {
            Debug.LogError("[AndroidVideoPlayer] EGL 未就绪");
            FireCompletedOnce();
            yield break;
        }

        // 2) 解析为设备可读的绝对文件路径
        string absPath = null;
        yield return ResolveToLocalPath(path, p => absPath = p);
        if (string.IsNullOrEmpty(absPath))
        {
            Debug.LogError($"[AndroidVideoPlayer] 无法定位视频文件：{path}");
            FireCompletedOnce();
            yield break;
        }

        if (mPlugin == null) mPlugin = new AndroidJavaClass(PLUGIN_CLASS);

        // 2.5) 指定了同步时钟来源：等待它的纹理就绪拿到 texId 作为 master（超时则放弃同步，自己走时钟）
        int masterTexId = -1;
        if (clockMaster != null)
        {
            float waitStart = Time.realtimeSinceStartup;
            while (clockMaster.TextureId <= 0 && Time.realtimeSinceStartup - waitStart < 3f)
                yield return null;
            masterTexId = clockMaster.TextureId;
        }

        // 3) 打开视频（探测尺寸 + 分配纹理 + 建 GL + 启动解码）
        int texId = -1;
        try { texId = mPlugin.CallStatic<int>("openVideo", absPath, loop, enableAudio, masterTexId); }
        catch (Exception ex) { Debug.LogError($"[AndroidVideoPlayer] openVideo: {ex.Message}"); }
        if (texId <= 0)
        {
            Debug.LogError($"[AndroidVideoPlayer] openVideo 失败：{absPath}");
            FireCompletedOnce();
            yield break;
        }
        mTextureId = texId;

        try
        {
            Width  = mPlugin.CallStatic<int>("getVideoWidth",  texId);
            Height = mPlugin.CallStatic<int>("getVideoHeight", texId);
        }
        catch (Exception ex) { Debug.LogError($"[AndroidVideoPlayer] getVideoSize: {ex.Message}"); }
        if (Width <= 0 || Height <= 0)
        {
            Debug.LogError($"[AndroidVideoPlayer] 视频尺寸非法 {Width}x{Height}");
            Stop();
            FireCompletedOnce();
            yield break;
        }

        // 4) 创建外部纹理（先不 set 到 UI，等首帧到达再 swap，避免一帧黑）
        mExternalTexture = Texture2D.CreateExternalTexture(
            Width, Height, TextureFormat.RGBA32, false, true, new IntPtr(texId));
        mExternalTexture.filterMode = FilterMode.Bilinear;
        mExternalTexture.wrapMode   = TextureWrapMode.Clamp;
        mTexBound = false;

        IsPlaying = true;
        Debug.Log($"[AndroidVideoPlayer] 开始播放 texId={texId} {Width}x{Height} loop={loop} src={absPath}");
    }

    private void BindTextureToUi()
    {
        if (mTexBound || mExternalTexture == null || targetRawImage == null) return;
        targetRawImage.texture = mExternalTexture;
        targetRawImage.color   = Color.white;
        targetRawImage.uvRect  = flipVertically ? kFlipYUvRect : kIdentityUvRect;
        mTexBound = true;
    }

    /// <summary>把 StreamingAssets 相对路径拷贝到 persistentDataPath 后回调绝对路径；绝对路径直接回调。</summary>
    private IEnumerator ResolveToLocalPath(string path, Action<string> onDone)
    {
        // 已是可直接读取的绝对文件路径
        if (path.StartsWith("/") || path.Contains(":\\") || path.StartsWith("file://"))
        {
            string p = path.StartsWith("file://") ? path.Substring("file://".Length) : path;
            onDone(File.Exists(p) ? p : null);
            yield break;
        }

        string dst = Path.Combine(Application.persistentDataPath, "avp_cache", path);
        if (!File.Exists(dst))
        {
            string src = Path.Combine(Application.streamingAssetsPath, path);
            string tmp = dst + ".tmp";
            string dir = Path.GetDirectoryName(dst);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(tmp)) File.Delete(tmp);

            using (var req = new UnityWebRequest(src, UnityWebRequest.kHttpVerbGET))
            {
                req.downloadHandler = new DownloadHandlerFile(tmp);
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AndroidVideoPlayer] 读取包内视频失败：{src} ({req.error})");
                    onDone(null);
                    yield break;
                }
            }

            if (File.Exists(dst)) File.Delete(dst);
            File.Move(tmp, dst);
        }

        onDone(dst);
    }
#endif
}
