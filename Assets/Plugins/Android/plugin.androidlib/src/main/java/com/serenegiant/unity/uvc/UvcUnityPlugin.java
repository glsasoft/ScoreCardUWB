package com.serenegiant.unity.uvc;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Unity 端通过 AndroidJavaClass("com.serenegiant.unity.uvc.UvcUnityPlugin") 调用本类。
 *
 * <h3>主线程不阻塞约定（M0）</h3>
 * 所有暴露给 Unity 的方法在主线程调用时均 &lt; 5ms（典型 &lt; 1ms）：
 * <ul>
 *   <li>openCamera/closeCamera：仅发起异步任务，立即返回</li>
 *   <li>updateTexImage：fire-and-forget post 到 GL handler，不等结果</li>
 *   <li>所有状态查询：单次 JNI 取原子标志</li>
 *   <li>allocPreviewTexture：池命中时 &lt; 1ms；未命中时退化为同步等 GL 线程</li>
 * </ul>
 * 唯一例外是 {@link #initUnityGlContext()} 与 {@link #prewarmTexturePool}：
 * 都建议在场景 loading 阶段调用一次，之后稳态零阻塞。
 *
 * <h3>典型时序</h3>
 * <pre>
 *   场景启动一次性预热：
 *     C# GL.IssuePluginEvent(GetCaptureEglContextFuncPtr(), 0)
 *       → initUnityGlContext()
 *       → prewarmTexturePool(W, H, 2)（按枚举到的每台 UVC 的最佳分辨率）
 *
 *   切换摄像头：
 *     enumerateCamerasAsync() → 通过 UnitySendMessage("UvcGlobalInit", "OnUvcEnumerateResult", json) 通知结果 → 选 devPath
 *     （enumerateCameras() 为同步版本，仅供调试面板等非热路径使用，正式流程一律走异步版本）
 *     allocPreviewTexture(W, H) → textureId（池命中, &lt;1ms）
 *     openCamera(devPath, textureId, W, H) → 立即返回
 *     C# 协程轮询 isFirstFrameReady(textureId) == true → swap RawImage.texture
 *     closeCamera(oldTextureId) → 立即返回
 *     deletePreviewTexture(oldTextureId) → 回池，&lt;1ms
 * </pre>
 */
public final class UvcUnityPlugin {
    private static final String TAG = "UvcUnityPlugin";

    static {
        System.loadLibrary("uvc_unity_plugin");
    }

    // ---------------------------------------------------------------------
    // native 声明
    // ---------------------------------------------------------------------

    static native boolean nativeCreateSharedEglEnv();
    static native boolean nativeMakeCurrent();
    static native String  nativeQueryCapabilities(String devPath);

    /** 立即返回；native 端 worker 线程异步做 V4L2 init + STREAMON */
    static native boolean nativeOpenDevice(String devPath, int textureId, int width, int height);
    /** 立即返回；native 端发停止信号后转交 reclaimer 线程异步释放 */
    static native void    nativeCloseDevice(int textureId);

    static native boolean nativeHasNewFrame(int textureId);
    /** native 解码完成首帧后即返回 true，C# 端用此判定预热完成 */
    static native boolean nativeHasFirstFrame(int textureId);
    /** 0=NotStarted 1=Initializing 2=Running 3=Failed 4=Stopped 99=NotFound */
    static native int     nativeGetState(int textureId);

    /** 在共享 GL 线程上把最新帧上传到 textureId（GL_TEXTURE_2D） */
    static native void    nativeUploadFrame(int textureId);

    static native float   nativeGetFps(int textureId);

    // ---------------------------------------------------------------------
    // 实例表
    // ---------------------------------------------------------------------

    private static final ConcurrentHashMap<Integer, UvcPreviewManager> sManagers
        = new ConcurrentHashMap<>();

    // ---------------------------------------------------------------------
    // EGL / 纹理
    // ---------------------------------------------------------------------

    /** 建议在场景 loading 阶段调用一次。同步等共享 EGL 创建完成（首次 ~100ms） */
    public static boolean initUnityGlContext() {
        boolean ok = GlTextureManager.getInstance().initWithUnityContext();
        Log.i(TAG, "initUnityGlContext: " + ok);
        return ok;
    }

    /**
     * 预热纹理池：在 GL 线程预分配 count 个 (width,height) 的 GL_TEXTURE_2D 入池。
     * 之后 {@link #allocPreviewTexture} 同尺寸命中时为 O(1) 内存操作。
     * 建议在 loading 阶段、对每台 UVC 的最佳分辨率各调用一次（count 通常 2 = 当前 + 预热）。
     */
    public static void prewarmTexturePool(int width, int height, int count) {
        GlTextureManager.getInstance().prewarmPool(width, height, count);
    }

    /**
     * 分配预览纹理。优先池命中（无 GL 调用），未命中则同步等 GL 线程创建。
     */
    public static int allocPreviewTexture(int width, int height) {
        int id = GlTextureManager.getInstance().allocTexture(width, height);
        if (id <= 0) Log.e(TAG, "allocPreviewTexture: 失败 " + width + "x" + height);
        return id;
    }

    /** 回池（不真删 GL 资源），&lt; 1ms */
    public static void deletePreviewTexture(int textureId) {
        GlTextureManager.getInstance().deleteTexture(textureId);
    }

    // ---------------------------------------------------------------------
    // 枚举
    // ---------------------------------------------------------------------

    /** 枚举所有 USB UVC 摄像头，返回 JSON 数组（同步，主线程会阻塞，仅供调试面板等非热路径使用） */
    public static String enumerateCameras() {
        List<UvcDevice> list = UvcHelper.enumerateDevices();
        String json = UvcHelper.toJsonArray(list);
        Log.i(TAG, "enumerateCameras: " + list.size() + " 个 → " + json);
        return json;
    }

    // 枚举扫描（/sys 遍历 + 逐设备 native ioctl 查询）放到独立单线程池，避免阻塞 Unity 主线程。
    private static final ExecutorService sEnumExecutor = Executors.newSingleThreadExecutor();

    // 与 C# 端 UvcGlobalInit 的约定：该 GameObject 由 C# 在启动时自动创建并常驻（DontDestroyOnLoad），
    // 名字固定为 "UvcGlobalInit"，其上挂的同名组件实现 OnUvcEnumerateResult(string) 接收回调。
    private static final String ENUM_CALLBACK_GO     = "UvcGlobalInit";
    private static final String ENUM_CALLBACK_METHOD = "OnUvcEnumerateResult";

    /**
     * 异步枚举 USB UVC 摄像头。主线程立即返回；扫描在后台线程执行，
     * 完成后通过 {@code UnityPlayer.UnitySendMessage} 把 JSON 结果发回 Unity 主线程。
     */
    public static void enumerateCamerasAsync() {
        sEnumExecutor.execute(() -> {
            List<UvcDevice> list = UvcHelper.enumerateDevices();
            String json = UvcHelper.toJsonArray(list);
            Log.i(TAG, "enumerateCamerasAsync: " + list.size() + " 个 → " + json);
            UnityPlayer.UnitySendMessage(ENUM_CALLBACK_GO, ENUM_CALLBACK_METHOD, json);
        });
    }

    // ---------------------------------------------------------------------
    // 打开 / 关闭（异步）
    // ---------------------------------------------------------------------

    /**
     * 异步打开摄像头。主线程立即返回；真正初始化（VIDIOC_S_FMT / mmap / STREAMON）
     * 在 native worker 线程内进行。C# 端应通过 {@link #isFirstFrameReady} 轮询就绪。
     *
     * @return devPath 成功；null 表示参数非法或线程派发失败
     */
    public static String openCamera(String devPath, int textureId, int width, int height) {
        if (devPath == null || devPath.isEmpty()) { Log.e(TAG, "openCamera: devPath 空"); return null; }
        if (textureId <= 0) { Log.e(TAG, "openCamera: textureId 非法 " + textureId); return null; }
        // 注意：不调用 closeCamera() 同步关闭旧实例；交给 nativeOpenDevice
        // 内部把 oldOne 转交 reclaimer 异步回收，主线程不阻塞。

        boolean ok = nativeOpenDevice(devPath, textureId, width, height);
        if (!ok) return null;

        UvcPreviewManager mgr = new UvcPreviewManager(devPath, textureId, width, height);
        sManagers.put(textureId, mgr);
        Log.i(TAG, "openCamera: " + devPath + " textureId=" + textureId + " 派发完成（异步初始化中）");
        return devPath;
    }

    /** 异步关闭。立即返回，资源由 reclaimer 后台线程释放 */
    public static void closeCamera(int textureId) {
        UvcPreviewManager mgr = sManagers.remove(textureId);
        if (mgr != null) {
            mgr.markClosed();
        }
        nativeCloseDevice(textureId);
        Log.d(TAG, "closeCamera: textureId=" + textureId + " 已发起异步关闭");
    }

    /** 关闭全部。立即返回 */
    public static void closeAllCameras() {
        for (Integer id : new java.util.ArrayList<>(sManagers.keySet())) {
            UvcPreviewManager m = sManagers.remove(id);
            if (m != null) m.markClosed();
            nativeCloseDevice(id);
        }
        Log.d(TAG, "closeAllCameras: 全部已发起异步关闭");
    }

    // ---------------------------------------------------------------------
    // 状态查询（每项均为单次原子读，主线程开销 < 0.5ms）
    // ---------------------------------------------------------------------

    public static boolean isNewFrameAvailable(int textureId) {
        return nativeHasNewFrame(textureId);
    }

    /** 首帧是否已解码完成。预热路用此判定可以做无缝 swap */
    public static boolean isFirstFrameReady(int textureId) {
        return nativeHasFirstFrame(textureId);
    }

    /** 摄像头是否进入 Running 状态（STREAMON 已完成） */
    public static boolean isCameraOpen(int textureId) {
        return nativeGetState(textureId) == 2;
    }

    /** 0=NotStarted 1=Initializing 2=Running 3=Failed 4=Stopped 99=NotFound */
    public static int getCameraState(int textureId) {
        return nativeGetState(textureId);
    }

    public static float getCameraFps(int textureId) {
        return nativeGetFps(textureId);
    }

    /** Fire-and-forget 上传最新帧到 GL_TEXTURE_2D */
    public static void updateTexImage(int textureId) {
        GlTextureManager.getInstance().uploadLatestFrame(textureId);
    }

    private UvcUnityPlugin() {}
}
