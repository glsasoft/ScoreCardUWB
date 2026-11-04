using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 摄像头帧率显示组件。两种统计模式：
///
/// RenderFps（默认）：
///   在 <see cref="UvcBridge"/> Update() 里统计，每次成功 updateTexImage 计一次。
///   反映 Unity UI 实际显示摄像头画面的频率，受 Unity 帧率上限约束。
///   不需要额外 JNI 调用，性能开销接近零。
///
/// CameraFps：
///   由 native V4L2 capture 线程统计，反映摄像头硬件实际输出帧率。
///   每次查询需要一次 JNI 调用，建议 queryInterval >= 0.5s。
/// </summary>
public class CameraFpsDisplay : MonoBehaviour
{
    public enum FpsSource
    {
        RenderFps,
        CameraFps,
        Both,
    }

    [Header("数据源")]
    [Tooltip("UvcBridge 组件，留空则自动查找")]
    public UvcBridge uvcBridge;

    [Tooltip("帧率统计来源")]
    public FpsSource fpsSource = FpsSource.RenderFps;

    [Header("显示设置")]
    [Tooltip("主显示 Text 组件")]
    public Text fpsText;

    [Tooltip(
        "RenderFps 或 CameraFps 模式格式，{0}=帧率。\n" +
        "Both 模式格式：{0}=UI渲染帧率，{1}=摄像头硬件帧率。")]
    public string displayFormat = "FPS: {0:F1}";

    [Tooltip("Both 模式格式")]
    public string displayFormatBoth = "渲染: {0:F1} FPS  摄像头: {1:F1} FPS";

    [Tooltip("CameraFps / Both 模式 JNI 查询间隔（秒）")]
    [Range(0.2f, 3f)]
    public float jniQueryInterval = 1f;

    private float mJniTimer;
    private float mLastCameraFps;

    private int   mRenderFrameCount;
    private float mRenderFpsTimer;
    private float mCurrentRenderFps;

    private bool mIsAndroid;
    private float mFindBridgeTimer;

    IEnumerator Start()
    {
        mIsAndroid = Application.platform == RuntimePlatform.Android;

        if (mIsAndroid)
        {
            yield return null;
            if (uvcBridge == null) uvcBridge = GetComponent<UvcBridge>();
            if (uvcBridge == null) uvcBridge = FindObjectOfType<UvcBridge>();
            if (uvcBridge == null) Debug.LogWarning("[CameraFpsDisplay] 未找到 UvcBridge，将在 Update 中重试");
        }

        if (fpsText == null) fpsText = GetComponent<Text>();
        UpdateDisplay();
    }

    void Update()
    {
        if (mIsAndroid)
        {
            if (uvcBridge == null)
            {
                mFindBridgeTimer += Time.unscaledDeltaTime;
                if (mFindBridgeTimer >= 1f)
                {
                    mFindBridgeTimer = 0f;
                    uvcBridge = FindObjectOfType<UvcBridge>();
                }
                return;
            }

            if (fpsSource != FpsSource.RenderFps)
            {
                mJniTimer += Time.unscaledDeltaTime;
                if (mJniTimer >= jniQueryInterval)
                {
                    mJniTimer = 0f;
                    mLastCameraFps = uvcBridge.GetCameraFps();
                }
            }
        }
        else
        {
            mRenderFrameCount++;
            mRenderFpsTimer += Time.unscaledDeltaTime;
            if (mRenderFpsTimer >= 1f)
            {
                mCurrentRenderFps = mRenderFrameCount / mRenderFpsTimer;
                mRenderFrameCount = 0;
                mRenderFpsTimer = 0f;
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (fpsText == null) return;

        float renderFps = mCurrentRenderFps;
        if (mIsAndroid)
            renderFps = uvcBridge != null ? uvcBridge.RenderFps : 0f;

        switch (fpsSource)
        {
            case FpsSource.RenderFps:
                fpsText.text = renderFps > 0f ? string.Format(displayFormat, renderFps) : "FPS: --";
                break;

            case FpsSource.CameraFps:
                fpsText.text = mLastCameraFps > 0f ? string.Format(displayFormat, mLastCameraFps) : "FPS: --";
                break;

            case FpsSource.Both:
                fpsText.text = (renderFps <= 0f && mLastCameraFps <= 0f)
                    ? "FPS: --"
                    : string.Format(displayFormatBoth, renderFps, mLastCameraFps);
                break;
        }
    }
}
