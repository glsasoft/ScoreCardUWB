using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UVC 全局一次性预热（M1.3 + M2.2 合并实现）。
///
/// 在场景启动（或进入"摄像头页"之前）调用 <see cref="EnsureReady"/>，完成：
///   1. <b>EGL 共享 context</b>：GL.IssuePluginEvent 捕获 Unity EGLContext + Java
///      建立共享 EGL 环境（仅首次发生，~100ms）
///   2. <b>UVC 设备枚举</b>：缓存设备列表到 <see cref="UvcBridge"/> 5 秒缓存
///   3. <b>纹理池预热</b>：按枚举到的每台 UVC 的最佳分辨率，预分配 N 个 GL_TEXTURE_2D
///      （默认 2 个：当前路 + 预热路），后续 <see cref="UvcBridge"/> alloc 时池命中 &lt;1ms
///
/// 这些工作完成后，运行期所有摄像头切换都不会再触发 EGL/纹理 GL 阻塞。
///
/// <h3>自动启动</h3>
/// 本类通过 <see cref="AutoBootstrap"/>（<see cref="RuntimeInitializeOnLoadMethodAttribute"/>）
/// 在 App 启动、场景加载完成后自动创建一个常驻（<c>DontDestroyOnLoad</c>）的 GameObject 并开始预热，
/// <b>不需要</b>手动把本组件拖进任何场景——这样能保证 EGL 首次 ~100ms 的同步等待永远发生在
/// 第一次真正打开摄像头页之前（不会卡在比赛流程里），且整个 App 生命周期内只发生一次。
/// 该 GameObject 名字固定为 <c>"UvcGlobalInit"</c>，与 Java 端 <c>enumerateCamerasAsync</c>
/// 的 <c>UnitySendMessage</c> 回调目标一致，详见 <see cref="OnUvcEnumerateResult"/>。
///
/// <h3>使用</h3>
/// <pre>
///   // 任何代码路径，越早越好：
///   yield return UvcGlobalInit.EnsureReady();
/// </pre>
/// </summary>
[DefaultExecutionOrder(-1000)]
public class UvcGlobalInit : MonoBehaviour
{
    private const string PLUGIN_CLASS = "com.serenegiant.unity.uvc.UvcUnityPlugin";

    [Tooltip("每个分辨率预分配的纹理数（推荐 2：当前路 + 预热路）")]
    public int prewarmCountPerSize = 2;

    [Tooltip("场景启动时自动开始预热")]
    public bool autoPrewarmOnStart = true;

    private static volatile bool sEglReady;
    private static volatile bool sStarted;
    private static Coroutine sCoroutine;

    public static bool IsEglReady => sEglReady;

    /// <summary>App 启动、首个场景加载完成后自动创建常驻实例并开始预热，全程只发生一次。</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (FindObjectOfType<UvcGlobalInit>() != null) return;

        var go = new GameObject(nameof(UvcGlobalInit));
        DontDestroyOnLoad(go);
        go.AddComponent<UvcGlobalInit>();
#endif
    }

    void Start()
    {
        if (autoPrewarmOnStart)
            StartCoroutine(EnsureReady());
    }

    /// <summary>Java 端 <c>enumerateCamerasAsync</c> 扫描完成后通过 UnitySendMessage 回调到这里（主线程）。</summary>
    public void OnUvcEnumerateResult(string json)
    {
        UvcBridge.OnEnumerateResultFromNative(json);
    }

    /// <summary>
    /// 启动预热（幂等）。若已就绪立即 yield break；否则跑完整流程。
    /// 调用方应当 yield return 本协程以保证返回时纹理池已可用。
    /// </summary>
    public static IEnumerator EnsureReady()
    {
        if (sEglReady) yield break;
        if (sStarted)
        {
            // 已有别处在跑：本地循环等
            while (!sEglReady) yield return null;
            yield break;
        }
        sStarted = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        // 1) 渲染线程捕获 EGLContext
        yield return new WaitForEndOfFrame();
        IntPtr captureFunc = UvcBridge.InternalCaptureFuncPtr();
        if (captureFunc == IntPtr.Zero)
        {
            Debug.LogError("[UvcGlobalInit] captureFunc 为空（NATIVE_LIB 未加载？）");
            sStarted = false;
            yield break;
        }
        GL.IssuePluginEvent(captureFunc, 0);
        yield return new WaitForEndOfFrame();

        // 2) 创建共享 EGL（同步等 GL handler，首次 ~100ms，仅发生一次）
        AndroidJavaClass plugin;
        try { plugin = new AndroidJavaClass(PLUGIN_CLASS); }
        catch (Exception ex) { Debug.LogError($"[UvcGlobalInit] AndroidJavaClass: {ex.Message}"); sStarted = false; yield break; }

        bool egl = false;
        try { egl = plugin.CallStatic<bool>("initUnityGlContext"); }
        catch (Exception ex) { Debug.LogError($"[UvcGlobalInit] initUnityGlContext: {ex.Message}"); }
        if (!egl)
        {
            Debug.LogError("[UvcGlobalInit] EGL 初始化失败");
            sStarted = false;
            yield break;
        }
        Debug.Log("[UvcGlobalInit] EGL 共享环境就绪");

        // 3) 枚举（异步通知，不阻塞主线程）+ 预热纹理池
        // 实际开播分辨率统一固定为 UvcBridge.RequiredWidth/RequiredHeight（不再按各设备上报的
        // best 分辨率各自预热），这里按同一固定尺寸预热，保证 alloc 命中池、且不同型号摄像头
        // 表现一致。
        List<UvcBridge.UvcDeviceInfo> devs = null;
        yield return UvcBridge.EnumerateDevicesAsync(list => devs = list);
        devs ??= new List<UvcBridge.UvcDeviceInfo>();
        int count = FindObjectOfType<UvcGlobalInit>()?.prewarmCountPerSize ?? 2;
        if (devs.Count > 0)
        {
            try
            {
                plugin.CallStatic("prewarmTexturePool", UvcBridge.RequiredWidth, UvcBridge.RequiredHeight, count);
                Debug.Log($"[UvcGlobalInit] 预热纹理池 {UvcBridge.RequiredWidth}x{UvcBridge.RequiredHeight} ×{count}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UvcGlobalInit] prewarmTexturePool: {ex.Message}");
            }
        }

        sEglReady = true;
        Debug.Log($"[UvcGlobalInit] 全部预热完成（{devs.Count} 台 UVC，固定分辨率 " +
                  $"{UvcBridge.RequiredWidth}x{UvcBridge.RequiredHeight} 已注册纹理池）");
#else
        sEglReady = true;
        yield break;
#endif
    }
}
