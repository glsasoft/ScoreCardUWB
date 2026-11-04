using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// 摄像头控制器。
/// Android 平台：使用 <see cref="UvcBridge"/>（V4L2 直读 USB UVC 摄像头）。
/// 其他平台（编辑器/PC）：使用 <see cref="WebCamTexture"/>。
/// <para>调色功能：通过 <see cref="SetColorAdjustment"/> 或 Inspector 面板实时调整亮度/对比度/饱和度/色相。</para>
/// </summary>
public class WebCamController : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("用于显示摄像头画面的 RawImage 组件")]
    public RawImage rawImage;

    [Header("Renderer 引用")]
    [Tooltip("用于显示摄像头画面的 Renderer 组件")]
    public Renderer quadRenderer;

    [Header("UVC 设置（Android）")]
    [Tooltip("指定 /dev/videoN 完整路径，留空则枚举到第一个 USB 摄像头自动打开")]
    public string preferredDevPath = "";

    [Header("WebCamTexture 设置（非 Android）")]
    [Tooltip("设备名称过滤，留空则使用第一个设备")]
    public string deviceNameFilter = "";

    [Tooltip("暂不生效：非 Android 平台仅用于测试，统一使用摄像头默认分辨率打开，避免驱动协商导致的卡顿；" +
             "Android 固定使用 UvcBridge.RequiredWidth/RequiredHeight（1920x1080）")]
    public int requestedWidth = 1280;
    [Tooltip("暂不生效，见 requestedWidth")]
    public int requestedHeight = 720;
    [Tooltip("暂不生效，见 requestedWidth")]
    public int requestedFPS = 30;

    [Header("调色参数")]
    [Tooltip("调色着色器（留空则自动查找 UI/CameraColorAdjust）")]
    public Shader colorAdjustShader;

    [Tooltip("亮度偏移：0 = 原始，正值更亮，负值更暗")]
    [Range(-1f, 1f)]
    public float brightness = 0f;

    [Tooltip("对比度：1 = 原始，>1 层次更分明，<1 更柔和")]
    [Range(0f, 3f)]
    public float contrast = 1f;

    [Tooltip("饱和度：1 = 原始，>1 色彩更艳丽，0 = 灰度")]
    [Range(0f, 3f)]
    public float saturation = 1.3f;

    [Tooltip("色相偏移：0 = 不偏移，0.5 = 互补色")]
    [Range(0f, 1f)]
    public float hueShift = 0f;

    // Android UVC
    private UvcBridge mUvcBridge;

    // 非 Android
    private WebCamTexture mWebCamTexture;

    private bool mIsAndroid;

    // 调色材质
    private Material mColorAdjustMat;

    private static readonly int sPropBrightness = Shader.PropertyToID("_Brightness");
    private static readonly int sPropContrast    = Shader.PropertyToID("_Contrast");
    private static readonly int sPropSaturation  = Shader.PropertyToID("_Saturation");
    private static readonly int sPropHueShift    = Shader.PropertyToID("_HueShift");

    // -------------------------------------------------------------------------
    // 生命周期
    // -------------------------------------------------------------------------

    void Start()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
        if (quadRenderer == null)
            quadRenderer = GetComponent<Renderer>();

        if (rawImage == null && quadRenderer == null)
        {
            Debug.LogError("[WebCamController] 未找到 RawImage 或 Renderer 组件");
            return;
        }

        if (quadRenderer)
        {
            float h = Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) * Camera.main.farClipPlane;
            float w = h * Camera.main.aspect;
            this.transform.localScale = new Vector3(w, h, 1f);
            this.transform.position = Camera.main.transform.position
                + Camera.main.transform.forward * Camera.main.farClipPlane * 0.5f;
        }

        EnsureColorAdjustMat();

        mIsAndroid = Application.platform == RuntimePlatform.Android;

        if (mIsAndroid) InitAndroid();
        else            StartCoroutine(InitWebCam());
    }

    void OnDestroy()
    {
        if (mIsAndroid) StopAndroid();
        else            StopWebCam();

        if (mColorAdjustMat != null)
        {
            Destroy(mColorAdjustMat);
            mColorAdjustMat = null;
        }
    }

    void OnDisable()
    {
        if (!mIsAndroid) StopWebCam();
    }

    void OnValidate()
    {
        // Inspector 滑杆实时预览
        ApplyColorAdjustToMat();
    }

    // -------------------------------------------------------------------------
    // Android - UVC
    // -------------------------------------------------------------------------

    private void InitAndroid()
    {
        mUvcBridge = gameObject.AddComponent<UvcBridge>();
        mUvcBridge.targetRawImage = rawImage;
        mUvcBridge.targetRenderer = quadRenderer;
        if (!string.IsNullOrEmpty(preferredDevPath))
            mUvcBridge.preferredDevPath = preferredDevPath;
    }

    private void StopAndroid()
    {
        if (mUvcBridge != null)
        {
            mUvcBridge.CloseCamera();
            mUvcBridge = null;
        }
    }

    // -------------------------------------------------------------------------
    // 非 Android - WebCamTexture
    // -------------------------------------------------------------------------

    private IEnumerator InitWebCam()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("摄像头权限申请失败");
            yield break;
        }

        // 用缓存枚举：调用方（如 ScoreCardWebCamPage）通常刚枚举过一次，此处复用避免重复查询驱动。
        var sw = Stopwatch.StartNew();
        WebCamDevice[] devices = WebCamDeviceCache.GetDevices();
        Debug.Log($"[WebCamController][耗时] GetDevices(缓存) 耗时={sw.ElapsedMilliseconds}ms");
        if (devices.Length == 0)
        {
            Debug.LogError("[WebCamController] 未检测到任何摄像头");
            yield break;
        }

        string target = deviceNameFilter;
        if (string.IsNullOrEmpty(target))
        {
            target = devices[0].name;
        }
        else
        {
            bool found = false;
            foreach (var d in devices)
            {
                if (d.name == target) { found = true; break; }
            }
            if (!found)
            {
                Debug.LogWarning($"[WebCamController] 未找到设备 '{target}'，使用默认");
                target = devices[0].name;
            }
        }

        // 实测发现：即便请求的分辨率确实在设备自报的 availableResolutions 里，Play() 仍可能
        // 因驱动协商耗时数秒（如某些机型 2560x1440 属于"支持但慢"档）。非 Android 平台仅用于
        // 测试，对分辨率无硬性要求，因此不再传入任何宽高/帧率，直接让驱动用其默认格式打开，
        // 从根源跳过协商搜索。
        yield return null;

        sw.Restart();
        mWebCamTexture = new WebCamTexture(target);
        Debug.Log($"[WebCamController][耗时] new WebCamTexture(target={target}，默认分辨率) 耗时={sw.ElapsedMilliseconds}ms");
        yield return null;

        if (rawImage)
        {
            rawImage.texture = mWebCamTexture;
            rawImage.color = Color.white;
        }
        if (quadRenderer && quadRenderer.material)
        {
            quadRenderer.material.mainTexture = mWebCamTexture;
            quadRenderer.material.color = Color.white;
        }

        yield return null;

        sw.Restart();
        mWebCamTexture.Play();
        Debug.Log($"[WebCamController][耗时] Play() 耗时={sw.ElapsedMilliseconds}ms（此为主线程阻塞时长，若较大即为卡顿元凶）");
    }

    public void StopWebCam()
    {
        if (mWebCamTexture != null && mWebCamTexture.isPlaying)
            mWebCamTexture.Stop();
    }

    // -------------------------------------------------------------------------
    // 公开 API
    // -------------------------------------------------------------------------

    /// <summary>
    /// 解绑显示目标，避免销毁本控制器时把已交接给其它 RawImage 的纹理清空。
    /// 双缓冲切换标准流程：
    ///   1) 可见 RawImage.texture = nextTex
    ///   2) oldCtrl.DetachDisplayTargetForHandoff()
    ///   3) Destroy(oldDriver)
    /// </summary>
    public void DetachDisplayTargetForHandoff()
    {
        if (mUvcBridge != null)
        {
            mUvcBridge.targetRawImage = null;
            mUvcBridge.targetRenderer = null;
        }
        rawImage = null;
        quadRenderer = null;
    }

    /// <summary>当前预览纹理；Android 上首帧到达后才非 null（避免黑帧）</summary>
    public Texture GetCurrentTexture()
    {
        if (mIsAndroid)
            return (mUvcBridge && mUvcBridge.HasFirstFrame) ? mUvcBridge.PreviewTexture : null;
        return mWebCamTexture;
    }

    /// <summary>摄像头预览是否已经"可呈现"（首帧已就绪）。M1 关键：用此判定切换</summary>
    public bool IsPlaying()
    {
        if (mIsAndroid)
            return mUvcBridge != null && mUvcBridge.IsRunning && mUvcBridge.HasFirstFrame;
        return mWebCamTexture != null && mWebCamTexture.isPlaying;
    }

    // -------------------------------------------------------------------------
    // 调色 API
    // -------------------------------------------------------------------------

    /// <summary>
    /// 动态调整摄像头画面色彩，参数立即生效。
    /// 同样可在 Inspector 面板拖动滑杆实时预览。
    /// </summary>
    /// <param name="brightness">亮度偏移 [-1, 1]，0 = 原始</param>
    /// <param name="contrast">对比度 [0, 3]，1 = 原始</param>
    /// <param name="saturation">饱和度 [0, 3]，1 = 原始，推荐 1.2~1.6 使画面更艳丽</param>
    /// <param name="hueShift">色相偏移 [0, 1]，0 = 不偏移</param>
    public void SetColorAdjustment(float brightness, float contrast, float saturation, float hueShift = 0f)
    {
        this.brightness  = brightness;
        this.contrast    = contrast;
        this.saturation  = saturation;
        this.hueShift    = hueShift;
        ApplyColorAdjustToMat();
    }

    /// <summary>重置为无色彩处理的原始画面</summary>
    public void ResetColorAdjustment()
    {
        SetColorAdjustment(0f, 1f, 1f, 0f);
    }

    // -------------------------------------------------------------------------
    // 调色材质内部实现
    // -------------------------------------------------------------------------

    private void EnsureColorAdjustMat()
    {
        if (mColorAdjustMat != null) return;

        var sh = colorAdjustShader != null
            ? colorAdjustShader
            : Shader.Find("UI/CameraColorAdjust");

        if (sh == null)
        {
            Debug.LogWarning("[WebCamController] 未找到 UI/CameraColorAdjust 着色器，调色功能不可用。" +
                             "请确认 Assets/Art/Shaders/UI/UI_CameraColorAdjust.shader 已导入，" +
                             "或在 Inspector 手动赋值 colorAdjustShader 字段。");
            return;
        }

        mColorAdjustMat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
        ApplyColorAdjustToMat();

        if (rawImage != null)
            rawImage.material = mColorAdjustMat;

        if (quadRenderer != null)
            quadRenderer.material = mColorAdjustMat;
    }

    private void ApplyColorAdjustToMat()
    {
        if (mColorAdjustMat == null) return;
        mColorAdjustMat.SetFloat(sPropBrightness, brightness);
        mColorAdjustMat.SetFloat(sPropContrast,   contrast);
        mColorAdjustMat.SetFloat(sPropSaturation, saturation);
        mColorAdjustMat.SetFloat(sPropHueShift,   hueShift);
    }
}
