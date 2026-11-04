package com.serenegiant.unity.uvc;

import android.opengl.GLES20;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;

import java.util.ArrayDeque;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * 共享 EGL context 后台线程上的 GL_TEXTURE_2D 纹理管理器。
 *
 * 暴露给 Unity 的 ID 就是 GL_TEXTURE_2D 名，
 * 由 Texture2D.CreateExternalTexture(id) 直接接收显示。
 *
 * <h3>核心优化（M0/M1）</h3>
 * <ul>
 *   <li><b>fire-and-forget 上传</b>：{@link #uploadLatestFrame(int)} 不再 latch.await，
 *       主线程仅一次 JNI/Handler.post 调用，远低于 1ms。</li>
 *   <li><b>纹理池化 + 预热</b>：{@link #prewarmPool(int, int, int)} 在场景启动时
 *       预先分配 N 个 (W,H) 纹理；之后的 {@link #allocTexture(int, int)} 优先从池里取，
 *       命中时是 O(1) 内存操作，不触发任何 GL 调用，无主线程阻塞。
 *       {@link #deleteTexture(int)} 不释放 GL 资源，直接回池。</li>
 * </ul>
 *
 * 池化的代价：每个池化纹理在进程生命周期内常驻显存（1080p RGBA = 8.3MB）。
 * 双路 UVC + 一个 warm buffer + 一份冗余 = 约 33MB，对 RK3588 完全可接受。
 */
public final class GlTextureManager {
    private static final String TAG = "UvcGlTextureManager";

    private static volatile GlTextureManager sInstance;

    private HandlerThread mThread;
    private Handler       mHandler;
    private volatile boolean mReady = false;

    private static final class TexInfo {
        final int width;
        final int height;
        TexInfo(int w, int h) { width = w; height = h; }
    }
    private final ConcurrentHashMap<Integer, TexInfo> mTextures = new ConcurrentHashMap<>();

    // 池：按 (width,height) 分桶
    private static final class SizeKey {
        final int w; final int h;
        SizeKey(int w, int h) { this.w = w; this.h = h; }
        @Override public int hashCode() { return w * 31 + h; }
        @Override public boolean equals(Object o) {
            return (o instanceof SizeKey) && ((SizeKey)o).w == w && ((SizeKey)o).h == h;
        }
    }
    private final Map<SizeKey, ArrayDeque<Integer>> mPool = new HashMap<>();
    private final Object mPoolLock = new Object();

    public static GlTextureManager getInstance() {
        if (sInstance == null) {
            synchronized (GlTextureManager.class) {
                if (sInstance == null) {
                    sInstance = new GlTextureManager();
                    sInstance.startThread();
                }
            }
        }
        return sInstance;
    }

    private GlTextureManager() {}

    private void startThread() {
        mThread = new HandlerThread("UvcGlTextureManager");
        mThread.start();
        mHandler = new Handler(mThread.getLooper());
    }

    /**
     * 在共享 EGL context 后台线程建立 EGL 环境。
     * 必须在 Unity 渲染线程已通过 GL.IssuePluginEvent 捕获 EGLContext 之后调用。
     */
    public boolean initWithUnityContext() {
        if (mReady) return true;

        CountDownLatch latch = new CountDownLatch(1);
        mHandler.post(() -> {
            boolean ok = UvcUnityPlugin.nativeCreateSharedEglEnv();
            if (ok) {
                Log.i(TAG, "initWithUnityContext: 共享 EGL 环境就绪");
            } else {
                Log.e(TAG, "initWithUnityContext: nativeCreateSharedEglEnv 失败");
            }
            mReady = ok;
            latch.countDown();
        });
        try { latch.await(); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
        return mReady;
    }

    public boolean isReady() { return mReady; }

    /**
     * 供视频播放等外部复用：在共享 EGL 的 GL 线程上执行一段 GL 操作。
     * 执行前已保证 nativeMakeCurrent()（共享 context 已 current）。
     * fire-and-forget，不阻塞调用线程。
     */
    public void runOnGlThread(Runnable r) {
        if (!mReady) {
            Log.w(TAG, "runOnGlThread: EGL 未就绪，丢弃任务");
            return;
        }
        mHandler.post(() -> {
            try {
                UvcUnityPlugin.nativeMakeCurrent();
                r.run();
            } catch (Throwable t) {
                Log.e(TAG, "runOnGlThread 异常", t);
            }
        });
    }

    /**
     * 返回共享 GL 线程的 Handler，供 SurfaceTexture.setOnFrameAvailableListener 等
     * 需要在 GL 线程收回调的场景使用。仅在 {@link #isReady()} 为 true 后有效。
     */
    public Handler glHandler() { return mHandler; }

    /**
     * 预热：在 GL handler 线程预分配 count 个 (width,height) 的 GL_TEXTURE_2D 放入池中。
     * 建议在场景启动 / 进入 page 之前调用一次，让后续 allocTexture 完全零 GL 阻塞。
     *
     * 主线程会同步等待，但只会在 loading 阶段发生一次（不在游戏稳态期间）。
     */
    public void prewarmPool(int width, int height, int count) {
        if (!mReady) {
            Log.w(TAG, "prewarmPool: EGL 未就绪，跳过");
            return;
        }
        if (width <= 0 || height <= 0 || count <= 0) return;
        CountDownLatch latch = new CountDownLatch(1);
        mHandler.post(() -> {
            try {
                UvcUnityPlugin.nativeMakeCurrent();
                GLES20.glGetError();
                SizeKey k = new SizeKey(width, height);
                ArrayDeque<Integer> bucket;
                synchronized (mPoolLock) {
                    bucket = mPool.get(k);
                    if (bucket == null) { bucket = new ArrayDeque<>(); mPool.put(k, bucket); }
                }
                int created = 0;
                for (int i = 0; i < count; i++) {
                    int id = createGlTextureLocked(width, height);
                    if (id <= 0) break;
                    synchronized (mPoolLock) { bucket.addLast(id); }
                    created++;
                }
                Log.i(TAG, "prewarmPool: " + width + "x" + height + " 已预分配 " + created + "/" + count);
            } catch (Throwable t) {
                Log.e(TAG, "prewarmPool 异常", t);
            } finally {
                latch.countDown();
            }
        });
        try { latch.await(); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
    }

    /**
     * 优先从池里取一个纹理；池为空则同步在 GL 线程创建。
     * 命中池时 < 1ms；池未命中时与原实现一致（5-15ms）。
     */
    public int allocTexture(int width, int height) {
        if (!mReady) {
            Log.e(TAG, "allocTexture: EGL 未初始化");
            return -1;
        }
        // 1) 快路径：池命中
        synchronized (mPoolLock) {
            ArrayDeque<Integer> bucket = mPool.get(new SizeKey(width, height));
            if (bucket != null && !bucket.isEmpty()) {
                int id = bucket.pollFirst();
                mTextures.put(id, new TexInfo(width, height));
                return id;
            }
        }
        // 2) 慢路径：GL 线程同步创建
        AtomicInteger result = new AtomicInteger(-1);
        CountDownLatch latch = new CountDownLatch(1);
        mHandler.post(() -> {
            try {
                UvcUnityPlugin.nativeMakeCurrent();
                GLES20.glGetError();
                int id = createGlTextureLocked(width, height);
                if (id > 0) {
                    mTextures.put(id, new TexInfo(width, height));
                    result.set(id);
                }
            } finally {
                latch.countDown();
            }
        });
        try { latch.await(); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
        if (result.get() > 0) {
            Log.i(TAG, "allocTexture: 池未命中，新建 id=" + result.get()
                + " size=" + width + "x" + height + "（建议 prewarmPool）");
        }
        return result.get();
    }

    /** GL 线程内创建一个 RGBA8 GL_TEXTURE_2D，分配存储；失败返回 -1 */
    private int createGlTextureLocked(int width, int height) {
        int[] ids = new int[1];
        GLES20.glGenTextures(1, ids, 0);
        if (ids[0] <= 0) {
            Log.e(TAG, "createGlTextureLocked: glGenTextures 失败");
            return -1;
        }
        int texId = ids[0];
        GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, texId);
        GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
            GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
        GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
            GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
        GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
            GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
        GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
            GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
        GLES20.glTexImage2D(GLES20.GL_TEXTURE_2D, 0, GLES20.GL_RGBA,
            width, height, 0, GLES20.GL_RGBA, GLES20.GL_UNSIGNED_BYTE, null);
        GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, 0);
        GLES20.glFlush();
        return texId;
    }

    /**
     * Fire-and-forget 上传：主线程发起 handler.post 后立即返回，
     * 不等 GL 线程完成。GL 上传完后纹理在 Unity 渲染线程下一次采样时已可用。
     *
     * 实测主线程开销：< 0.3ms（仅 handler.post 调度）。
     */
    public void uploadLatestFrame(int textureId) {
        if (!mReady || textureId <= 0) return;
        if (!mTextures.containsKey(textureId)) return;

        mHandler.post(() -> {
            try {
                UvcUnityPlugin.nativeMakeCurrent();
                UvcUnityPlugin.nativeUploadFrame(textureId);
            } catch (Throwable t) {
                Log.w(TAG, "uploadLatestFrame: " + t.getMessage());
            }
        });
    }

    /**
     * 回收纹理：不真删除 GL 资源，放回池等下次复用。
     * 主线程开销 < 1ms（仅 ConcurrentHashMap + Deque 操作）。
     */
    public void deleteTexture(int textureId) {
        if (textureId <= 0) return;
        TexInfo info = mTextures.remove(textureId);
        if (info == null) return;

        synchronized (mPoolLock) {
            SizeKey k = new SizeKey(info.width, info.height);
            ArrayDeque<Integer> bucket = mPool.get(k);
            if (bucket == null) { bucket = new ArrayDeque<>(); mPool.put(k, bucket); }
            bucket.addLast(textureId);
        }
    }
}
