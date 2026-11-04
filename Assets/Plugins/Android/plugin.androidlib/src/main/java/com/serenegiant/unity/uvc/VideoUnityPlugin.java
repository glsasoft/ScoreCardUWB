package com.serenegiant.unity.uvc;

import android.util.Log;

import java.util.concurrent.ConcurrentHashMap;

/**
 * Unity 端通过 AndroidJavaClass("com.serenegiant.unity.uvc.VideoUnityPlugin") 调用。
 *
 * 复用 UVC 插件的共享 EGL 环境与 {@link GlTextureManager} 纹理池，用 MediaCodec 硬解码视频，
 * 在共享 GL 线程上把每帧从 OES 采样写入池化的 GL_TEXTURE_2D，交给 Unity 直接显示。
 *
 * <h3>典型时序（对应 C# AndroidVideoPlayer）</h3>
 * <pre>
 *   1. UvcGlobalInit.EnsureReady() 保证共享 EGL 已就绪（复用 UVC 引导）
 *   2. int texId = openVideo(absPath, loop, withAudio, masterTexId)  // 探测尺寸 + 分配 2D 纹理 + 建 GL + 启动解码
 *   3. int w = getVideoWidth(texId); h = getVideoHeight(texId)
 *   4. Texture2D.CreateExternalTexture(w, h, ..., texId) 绑到 RawImage
 *   5. 每帧：if isNewFrameAvailable(texId) updateTexImage(texId)
 *   6. 首帧就绪：isFirstFrameReady(texId) → swap 到 UI，避免黑帧
 *   7. 播放结束：isFinished(texId) → 触发结束回调
 *   8. closeVideo(texId)                          // 停解码 + 释放 GL + 回收纹理
 * </pre>
 *
 * 所有方法主线程调用均非阻塞（openVideo 内部同步等一次 GL 建资源，属 loading 级别）。
 */
public final class VideoUnityPlugin {
    private static final String TAG = "VideoUnityPlugin";

    private static final ConcurrentHashMap<Integer, VideoPlaybackSession> sSessions
        = new ConcurrentHashMap<>();

    private VideoUnityPlugin() {}

    /**
     * 打开视频：探测尺寸 → 分配 GL_TEXTURE_2D → 建立 GL/解码。
     *
     * @param path 视频文件绝对路径（Unity 侧需先把 StreamingAssets 拷到可读路径）
     * @param loop 是否循环播放
     * @param withAudio 是否解码播放该会话自带的音轨；多个会话播放同一份视频时应只让一个传 true，
     *                  避免各自独立解码音频、时钟不完全一致导致的叠音/回声。
     * @param masterTexId 同步时钟来源会话的 texId（另一路已 open 的会话）；本会话节流将对齐它的
     *                  音频播放时钟，而不是自己独立走时钟，用于让两路播同一视频的画面彼此同步。
     *                  传 <=0 表示不指定，本会话走自己的时钟（有音轨则用自己音轨，否则自由时钟）。
     * @return 目标 GL_TEXTURE_2D 的 id（>0 成功，Unity 用它 CreateExternalTexture）；失败返回 -1
     */
    public static int openVideo(String path, boolean loop, boolean withAudio, int masterTexId) {
        if (path == null || path.isEmpty()) {
            Log.e(TAG, "openVideo: path 为空");
            return -1;
        }
        if (!GlTextureManager.getInstance().isReady()) {
            Log.e(TAG, "openVideo: EGL 未就绪，请先 UvcGlobalInit.EnsureReady()");
            return -1;
        }

        VideoPlaybackSession session = new VideoPlaybackSession(path, loop, withAudio);
        if (!session.probe()) {
            Log.e(TAG, "openVideo: 探测失败 " + path);
            return -1;
        }

        int texId = GlTextureManager.getInstance().allocTexture(session.getWidth(), session.getHeight());
        if (texId <= 0) {
            Log.e(TAG, "openVideo: 分配纹理失败 " + session.getWidth() + "x" + session.getHeight());
            return -1;
        }

        if (!session.setupGl(texId)) {
            Log.e(TAG, "openVideo: setupGl 失败");
            GlTextureManager.getInstance().deleteTexture(texId);
            return -1;
        }

        if (masterTexId > 0) {
            VideoPlaybackSession master = sSessions.get(masterTexId);
            if (master != null) {
                session.setClockMaster(master);
            } else {
                Log.w(TAG, "openVideo: 指定的 masterTexId=" + masterTexId + " 未找到会话，忽略同步时钟");
            }
        }

        if (!session.startDecode()) {
            Log.e(TAG, "openVideo: startDecode 失败");
            session.release();
            GlTextureManager.getInstance().deleteTexture(texId);
            return -1;
        }

        sSessions.put(texId, session);
        Log.i(TAG, "openVideo: " + path + " → texId=" + texId
            + " " + session.getWidth() + "x" + session.getHeight() + " loop=" + loop
            + " masterTexId=" + masterTexId);
        return texId;
    }

    public static int getVideoWidth(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        return s == null ? 0 : s.getWidth();
    }

    public static int getVideoHeight(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        return s == null ? 0 : s.getHeight();
    }

    /** 是否有新帧待上传 */
    public static boolean isNewFrameAvailable(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        return s != null && s.isNewFrameAvailable();
    }

    /** 首帧是否已渲染到目标 2D 纹理（用于无缝 swap，避免黑帧） */
    public static boolean isFirstFrameReady(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        return s != null && s.isFirstFrameReady();
    }

    /** 非循环模式下视频是否播放完毕 */
    public static boolean isFinished(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        return s != null && s.isFinished();
    }

    /** 由 Unity 每帧驱动：若有新帧则采样到目标 2D 纹理（fire-and-forget） */
    public static void updateTexImage(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        if (s != null) s.requestDraw();
    }

    /** 暂停/恢复播放（暂停时冻结在当前帧） */
    public static void setPaused(int textureId, boolean paused) {
        VideoPlaybackSession s = sSessions.get(textureId);
        if (s != null) s.setPaused(paused);
    }

    /** 按归一化进度 [0,1] seek（同步帧精度），用于正反视频方向切换的无缝对齐 */
    public static void seekNormalized(int textureId, float t) {
        VideoPlaybackSession s = sSessions.get(textureId);
        if (s != null) s.seekNormalized(t);
    }

    /** 当前归一化播放进度 [0,1] */
    public static float getNormalizedTime(int textureId) {
        VideoPlaybackSession s = sSessions.get(textureId);
        return s == null ? 0f : s.getNormalizedTime();
    }

    /** 关闭视频：停解码 + 释放 GL + 回收纹理到池 */
    public static void closeVideo(int textureId) {
        VideoPlaybackSession s = sSessions.remove(textureId);
        if (s != null) {
            s.release();
            GlTextureManager.getInstance().deleteTexture(textureId);
            Log.i(TAG, "closeVideo: texId=" + textureId);
        }
    }

    /** 关闭全部（如退出/暂停） */
    public static void closeAll() {
        for (Integer id : new java.util.ArrayList<>(sSessions.keySet())) {
            closeVideo(id);
        }
    }
}
