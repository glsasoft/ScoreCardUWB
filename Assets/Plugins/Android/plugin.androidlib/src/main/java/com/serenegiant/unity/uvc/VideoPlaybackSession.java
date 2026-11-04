package com.serenegiant.unity.uvc;

import android.graphics.SurfaceTexture;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.media.MediaCodec;
import android.media.MediaExtractor;
import android.media.MediaFormat;
import android.opengl.GLES11Ext;
import android.opengl.GLES20;
import android.util.Log;
import android.view.Surface;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.atomic.AtomicBoolean;

/**
 * 单个视频的硬解码 + OES→GL_TEXTURE_2D 会话。
 *
 * 解码链路：MediaExtractor → MediaCodec(硬件) → Surface(由 OES SurfaceTexture 提供)
 * 渲染链路：GL 线程上 updateTexImage(OES) → FBO 采样 samplerExternalOES → 写入目标 GL_TEXTURE_2D
 *
 * 目标 GL_TEXTURE_2D 由 {@link GlTextureManager} 的纹理池分配，Unity 端通过
 * Texture2D.CreateExternalTexture(textureId) 直接显示，规避 Unity VideoPlayer 的 OES 直采路径
 * （该路径在部分 Mali 驱动上会触发 gles_error 0x0003 导致整屏黑）。
 *
 * 所有 GL 操作都在 {@link GlTextureManager} 的共享 GL 线程上执行；
 * 解码在独立 worker 线程；对 Unity 暴露的方法均非阻塞。
 */
final class VideoPlaybackSession implements SurfaceTexture.OnFrameAvailableListener {
    private static final String TAG = "VideoPlaybackSession";

    private static final int TIMEOUT_US = 10_000;

    private final String  mPath;
    private final boolean mLoop;
    private final boolean mWithAudio;

    // 同步时钟来源（可选）：设置后本会话节流改用该会话的音频硬件时钟，而不是自己的（多用于自己
    // 没有音轨、但要与另一路播放同一视频的会话保持画面同步的场景，见 decodeLoop）。
    private volatile VideoPlaybackSession mClockMaster;

    private int mTargetTexId = -1;   // 目标 GL_TEXTURE_2D（Unity 显示用）
    private int mWidth;
    private int mHeight;
    private long mDurationUs;         // 视频总时长（微秒），用于归一化进度/seek

    // 播放控制（解码线程读取）
    private volatile boolean mPaused;
    private volatile float   mSeekRequest = -1f;   // [0,1] 归一化 seek 目标，-1 表示无
    private volatile long    mLastPresentationUs;  // 最近渲染帧的 PTS

    // 解码（视频）
    private MediaExtractor mExtractor;
    private MediaCodec     mCodec;
    private int            mTrackIndex = -1;
    private MediaFormat    mFormat;
    private Thread         mDecodeThread;
    private volatile boolean mDecodeRunning;

    // 解码 + 播放（音频，可选：找不到音轨则静音播放，不影响视频）。
    // 用独立的 MediaExtractor/线程，不与视频解码共享状态，避免相互影响；
    // 节流依赖 AudioTrack.write() 的阻塞写入天然按播放速率消费，无需手动对齐时钟。
    private int            mAudioTrackIndex = -1;
    private MediaFormat    mAudioFormat;
    private MediaExtractor mAudioExtractor;
    private MediaCodec     mAudioCodec;
    private AudioTrack     mAudioTrack;
    private Thread         mAudioThread;
    private volatile boolean mAudioRunning;
    private final Object mHeadLock = new Object();
    private long mLastHead32;
    private long mHeadWraps;

    // GL（仅 GL 线程访问）
    private int           mOesTexId  = -1;
    private SurfaceTexture mSurfaceTexture;
    private Surface        mSurface;
    private int   mProgram      = 0;
    private int   mAPositionLoc = -1;
    private int   mATexCoordLoc = -1;
    private int   mUTexMatrixLoc = -1;
    private int   mUTextureLoc   = -1;
    private final int[] mFbo = new int[1];
    private final float[] mStMatrix = new float[16];
    private FloatBuffer mVertexBuffer;
    private FloatBuffer mTexBuffer;

    // 状态标志
    private final AtomicBoolean mGlReady          = new AtomicBoolean(false);
    private final AtomicBoolean mFrameAvailable   = new AtomicBoolean(false);
    private final AtomicBoolean mFirstFrameReady  = new AtomicBoolean(false);
    private final AtomicBoolean mFinished         = new AtomicBoolean(false);
    private final AtomicBoolean mReleased         = new AtomicBoolean(false);

    private static final float[] QUAD_POS = {
        -1f, -1f,   1f, -1f,   -1f, 1f,   1f, 1f,
    };
    private static final float[] QUAD_UV = {
         0f,  0f,   1f,  0f,    0f, 1f,   1f, 1f,
    };

    private static final String VERTEX_SHADER =
        "attribute vec2 aPosition;\n" +
        "attribute vec2 aTexCoord;\n" +
        "uniform mat4 uTexMatrix;\n" +
        "varying vec2 vTexCoord;\n" +
        "void main() {\n" +
        "  gl_Position = vec4(aPosition, 0.0, 1.0);\n" +
        "  vTexCoord = (uTexMatrix * vec4(aTexCoord, 0.0, 1.0)).xy;\n" +
        "}\n";

    private static final String FRAGMENT_SHADER =
        "#extension GL_OES_EGL_image_external : require\n" +
        "precision mediump float;\n" +
        "varying vec2 vTexCoord;\n" +
        "uniform samplerExternalOES sTexture;\n" +
        "void main() {\n" +
        "  vec4 c = texture2D(sTexture, vTexCoord);\n" +
        // MediaCodec 解码输出的是可直接显示的 gamma 编码颜色；Unity 工程用的是 Linear 色彩空间，
        // 会在最终写帧缓冲时再做一次 linear→gamma 编码。这里写入池化 2D 纹理前先做 gamma→linear
        // 解码抵消，否则两次编码叠加会导致画面严重过曝（编辑器预览走 Unity 自带 VideoPlayer，
        // 不经过这条自绘纹理链路，所以不受影响）。
        "  gl_FragColor = vec4(pow(c.rgb, vec3(2.2)), c.a);\n" +
        "}\n";

    VideoPlaybackSession(String path, boolean loop, boolean withAudio) {
        mPath = path;
        mLoop = loop;
        mWithAudio = withAudio;
    }

    /** 设置同步时钟来源：本会话之后的节流将读取 master 的音频播放时钟而不是自己的，
     *  用于让画面相同的多路会话彼此对齐（而不只是自己的音画对齐）。传 null 取消。 */
    void setClockMaster(VideoPlaybackSession master) {
        mClockMaster = master;
    }

    // ---------------------------------------------------------------------
    // 1) 探测尺寸（不解码，快）
    // ---------------------------------------------------------------------
    boolean probe() {
        try {
            mExtractor = new MediaExtractor();
            mExtractor.setDataSource(mPath);
            int count = mExtractor.getTrackCount();
            for (int i = 0; i < count; i++) {
                MediaFormat f = mExtractor.getTrackFormat(i);
                String mime = f.getString(MediaFormat.KEY_MIME);
                if (mime == null) continue;

                if (mTrackIndex < 0 && mime.startsWith("video/")) {
                    mTrackIndex = i;
                    mFormat = f;
                    mWidth  = f.getInteger(MediaFormat.KEY_WIDTH);
                    mHeight = f.getInteger(MediaFormat.KEY_HEIGHT);
                    if (f.containsKey(MediaFormat.KEY_DURATION)) {
                        mDurationUs = f.getLong(MediaFormat.KEY_DURATION);
                    }
                } else if (mAudioTrackIndex < 0 && mime.startsWith("audio/")) {
                    // 音轨仅记录 format，实际解码用独立 MediaExtractor（见 startAudioIfPresent），
                    // 这里的 mExtractor 只 selectTrack 视频轨。
                    mAudioTrackIndex = i;
                    mAudioFormat = f;
                }
            }

            if (mTrackIndex < 0) {
                Log.e(TAG, "probe: 未找到视频轨道 " + mPath);
                return false;
            }

            mExtractor.selectTrack(mTrackIndex);
            Log.i(TAG, "probe: " + mPath + " → " + mWidth + "x" + mHeight
                + " durUs=" + mDurationUs + " hasAudio=" + (mAudioTrackIndex >= 0));
            return mWidth > 0 && mHeight > 0;
        } catch (Throwable t) {
            Log.e(TAG, "probe 失败 " + mPath, t);
        }
        return false;
    }

    int getWidth()  { return mWidth; }
    int getHeight() { return mHeight; }

    // ---------------------------------------------------------------------
    // 2) 在 GL 线程建立 OES / SurfaceTexture / Surface / 着色器 / FBO（阻塞等待完成）
    // ---------------------------------------------------------------------
    boolean setupGl(int targetTexId) {
        mTargetTexId = targetTexId;
        GlTextureManager mgr = GlTextureManager.getInstance();
        if (!mgr.isReady()) {
            Log.e(TAG, "setupGl: EGL 未就绪");
            return false;
        }
        final CountDownLatch latch = new CountDownLatch(1);
        final AtomicBoolean ok = new AtomicBoolean(false);
        mgr.runOnGlThread(() -> {
            try {
                initGlLocked();
                ok.set(true);
            } catch (Throwable t) {
                Log.e(TAG, "setupGl initGl 异常", t);
            } finally {
                latch.countDown();
            }
        });
        try { latch.await(); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
        mGlReady.set(ok.get());
        return ok.get();
    }

    private void initGlLocked() {
        int[] tex = new int[1];
        GLES20.glGenTextures(1, tex, 0);
        mOesTexId = tex[0];
        GLES20.glBindTexture(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, mOesTexId);
        GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES,
            GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR);
        GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES,
            GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
        GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES,
            GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
        GLES20.glTexParameteri(GLES11Ext.GL_TEXTURE_EXTERNAL_OES,
            GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
        GLES20.glBindTexture(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, 0);

        mSurfaceTexture = new SurfaceTexture(mOesTexId);
        mSurfaceTexture.setDefaultBufferSize(mWidth, mHeight);
        mSurfaceTexture.setOnFrameAvailableListener(this, GlTextureManager.getInstance().glHandler());
        mSurface = new Surface(mSurfaceTexture);

        mProgram = buildProgram(VERTEX_SHADER, FRAGMENT_SHADER);
        mAPositionLoc  = GLES20.glGetAttribLocation(mProgram, "aPosition");
        mATexCoordLoc  = GLES20.glGetAttribLocation(mProgram, "aTexCoord");
        mUTexMatrixLoc = GLES20.glGetUniformLocation(mProgram, "uTexMatrix");
        mUTextureLoc   = GLES20.glGetUniformLocation(mProgram, "sTexture");

        mVertexBuffer = toFloatBuffer(QUAD_POS);
        mTexBuffer    = toFloatBuffer(QUAD_UV);

        GLES20.glGenFramebuffers(1, mFbo, 0);
        Log.i(TAG, "initGlLocked: OES=" + mOesTexId + " program=" + mProgram
            + " fbo=" + mFbo[0] + " target2D=" + mTargetTexId);
    }

    // ---------------------------------------------------------------------
    // 3) 配置并启动 MediaCodec 解码线程
    // ---------------------------------------------------------------------
    boolean startDecode() {
        if (mSurface == null || mFormat == null) {
            Log.e(TAG, "startDecode: GL/format 未就绪");
            return false;
        }
        try {
            String mime = mFormat.getString(MediaFormat.KEY_MIME);
            mCodec = MediaCodec.createDecoderByType(mime);
            mCodec.configure(mFormat, mSurface, null, 0);
            mCodec.start();
        } catch (Throwable t) {
            Log.e(TAG, "startDecode: 配置解码器失败", t);
            return false;
        }
        mDecodeRunning = true;
        mDecodeThread = new Thread(this::decodeLoop, "VideoDecode");
        mDecodeThread.start();

        startAudioIfPresent();
        return true;
    }

    // ---------------------------------------------------------------------
    // 3b) 音轨解码 + 播放（可选）。独立于视频解码链路，出错时静音降级，不影响视频播放。
    // ---------------------------------------------------------------------
    private void startAudioIfPresent() {
        if (!mWithAudio || mAudioTrackIndex < 0 || mAudioFormat == null) return;

        try {
            mAudioExtractor = new MediaExtractor();
            mAudioExtractor.setDataSource(mPath);
            mAudioExtractor.selectTrack(mAudioTrackIndex);

            String mime = mAudioFormat.getString(MediaFormat.KEY_MIME);
            int sampleRate = mAudioFormat.getInteger(MediaFormat.KEY_SAMPLE_RATE);
            int channelCount = mAudioFormat.getInteger(MediaFormat.KEY_CHANNEL_COUNT);
            int channelConfig = channelCount >= 2 ? AudioFormat.CHANNEL_OUT_STEREO : AudioFormat.CHANNEL_OUT_MONO;

            mAudioCodec = MediaCodec.createDecoderByType(mime);
            mAudioCodec.configure(mAudioFormat, null, null, 0);
            mAudioCodec.start();

            int minBufSize = AudioTrack.getMinBufferSize(
                sampleRate, channelConfig, AudioFormat.ENCODING_PCM_16BIT);
            mAudioTrack = new AudioTrack(
                AudioManager.STREAM_MUSIC, sampleRate, channelConfig,
                AudioFormat.ENCODING_PCM_16BIT, Math.max(minBufSize, 4096) * 2,
                AudioTrack.MODE_STREAM);
            mAudioTrack.play();

            mAudioRunning = true;
            mAudioThread = new Thread(this::audioDecodeLoop, "VideoAudioDecode");
            mAudioThread.start();
            Log.i(TAG, "startAudioIfPresent: " + mPath + " sampleRate=" + sampleRate + " ch=" + channelCount);
        } catch (Throwable t) {
            Log.w(TAG, "音轨初始化失败，静音播放: " + t.getMessage());
            stopAudioInternal();
        }
    }

    // 音频节流依赖 AudioTrack.write() 的阻塞写入天然按播放速率消费，不需要像视频那样手动按 PTS 睡眠对齐。
    private void audioDecodeLoop() {
        MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
        boolean inputDone = false;

        try {
            while (mAudioRunning) {
                if (mPaused) {
                    try { Thread.sleep(10); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
                    continue;
                }

                if (!inputDone) {
                    int inIndex = mAudioCodec.dequeueInputBuffer(TIMEOUT_US);
                    if (inIndex >= 0) {
                        ByteBuffer inBuf = mAudioCodec.getInputBuffer(inIndex);
                        int sampleSize = (inBuf == null) ? -1 : mAudioExtractor.readSampleData(inBuf, 0);
                        // 读到末尾：循环模式先 seek 回起点重读，否则送 EOS（与视频解码loop的处理方式一致）。
                        if (sampleSize < 0 && mLoop) {
                            mAudioExtractor.seekTo(0, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
                            sampleSize = mAudioExtractor.readSampleData(inBuf, 0);
                        }
                        if (sampleSize < 0) {
                            mAudioCodec.queueInputBuffer(inIndex, 0, 0, 0,
                                MediaCodec.BUFFER_FLAG_END_OF_STREAM);
                            inputDone = true;
                        } else {
                            long pts = mAudioExtractor.getSampleTime();
                            mAudioCodec.queueInputBuffer(inIndex, 0, sampleSize, pts, 0);
                            mAudioExtractor.advance();
                        }
                    }
                }

                int outIndex = mAudioCodec.dequeueOutputBuffer(info, TIMEOUT_US);
                if (outIndex >= 0) {
                    if (info.size > 0 && mAudioTrack != null) {
                        ByteBuffer outBuf = mAudioCodec.getOutputBuffer(outIndex);
                        if (outBuf != null) {
                            byte[] pcm = new byte[info.size];
                            outBuf.get(pcm);
                            mAudioTrack.write(pcm, 0, pcm.length);
                        }
                    }
                    mAudioCodec.releaseOutputBuffer(outIndex, false);

                    if ((info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) {
                        if (mLoop) {
                            // 循环模式理论上不会到这里（输入端已 seek），保险处理
                            mAudioCodec.flush();
                            mAudioExtractor.seekTo(0, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
                            inputDone = false;
                        } else {
                            break;
                        }
                    }
                }
            }
        } catch (Throwable t) {
            if (mAudioRunning) Log.e(TAG, "audioDecodeLoop 异常", t);
        }
    }

    private void stopAudioInternal() {
        mAudioRunning = false;
        if (mAudioThread != null) {
            try { mAudioThread.join(500); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
            mAudioThread = null;
        }
        try { if (mAudioTrack != null) { mAudioTrack.stop(); mAudioTrack.release(); } }
        catch (Throwable t) { Log.w(TAG, "release audioTrack: " + t.getMessage()); }
        mAudioTrack = null;
        try { if (mAudioCodec != null) { mAudioCodec.stop(); mAudioCodec.release(); } }
        catch (Throwable t) { Log.w(TAG, "release audioCodec: " + t.getMessage()); }
        mAudioCodec = null;
        try { if (mAudioExtractor != null) mAudioExtractor.release(); }
        catch (Throwable t) { Log.w(TAG, "release audioExtractor: " + t.getMessage()); }
        mAudioExtractor = null;
    }

    /** 音频硬件时钟当前累计播放时长（微秒）：从 AudioTrack.play() 起单调递增，不受循环内部 seek 影响，
     *  是比自由时钟更准的播放基准（不受解码/调度线程抖动影响）。没有音轨或音轨未启用时返回 -1。 */
    private long audioPositionUs() {
        if (mAudioTrack == null) return -1;
        try {
            int sampleRate = mAudioTrack.getSampleRate();
            if (sampleRate <= 0) return -1;
            // getPlaybackHeadPosition 是 32 位无符号帧计数，48kHz 下约 24.8 小时回绕一次。
            // 回绕后若不处理，音频时钟会突然归零，视频节流会误判为"超前很多"而每帧睡满 200ms，画面掉到约 5fps。
            // 此方法每帧都会被调用，远小于回绕周期，因此检测到数值变小即可判定为回绕一次。
            long headFrames;
            synchronized (mHeadLock) {
                long head32 = mAudioTrack.getPlaybackHeadPosition() & 0xFFFFFFFFL;
                if (head32 < mLastHead32) mHeadWraps++;
                mLastHead32 = head32;
                headFrames = (mHeadWraps << 32) | head32;
            }
            return headFrames * 1_000_000L / sampleRate;
        } catch (Throwable t) {
            return -1;
        }
    }

    private void decodeLoop() {
        MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
        boolean inputDone = false;
        long startNs = -1;
        long lastOutputPts = -1;  // 用于在输出端识别"已经循环回到片头"，见下方节流基准重建
        long loopEpoch = 0;       // 循环轮次：配合 PTS 换算成跨轮次单调递增的"累计内容时间"，与音频累计时钟对齐

        try {
            while (mDecodeRunning) {
                // 暂停：冻结在最后一帧，解码线程空转；恢复后重新对齐时钟
                if (mPaused) {
                    try { Thread.sleep(10); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
                    startNs = -1;
                    continue;
                }

                // seek 请求：跳到归一化位置最近的同步帧，flush 解码器后从新位置继续
                float sk = mSeekRequest;
                if (sk >= 0f && mDurationUs > 0) {
                    mSeekRequest = -1f;
                    long us = (long) (sk * mDurationUs);
                    mExtractor.seekTo(us, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
                    mCodec.flush();
                    inputDone = false;
                    startNs = -1;
                }

                // 送入
                if (!inputDone) {
                    int inIndex = mCodec.dequeueInputBuffer(TIMEOUT_US);
                    if (inIndex >= 0) {
                        ByteBuffer inBuf = mCodec.getInputBuffer(inIndex);
                        int sampleSize = (inBuf == null) ? -1 : mExtractor.readSampleData(inBuf, 0);
                        // 读到末尾：循环模式先 seek 回起点重读，否则送 EOS。
                        // 注意：节流基准 startNs 不在这里重置——MediaCodec 是异步管线，此刻"取出"端
                        // 可能还有若干片尾旧帧未取出，若在这里提前重置会用旧帧（PTS 接近片尾）错误
                        // 建立基准，导致紧接着的新一轮片头帧被算成"已经过期"而不限速疯狂快进。
                        // 改为在下方"取出"端按 PTS 是否变小识别真正的循环点再重建基准。
                        if (sampleSize < 0 && mLoop) {
                            mExtractor.seekTo(0, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
                            sampleSize = mExtractor.readSampleData(inBuf, 0);
                        }
                        if (sampleSize < 0) {
                            mCodec.queueInputBuffer(inIndex, 0, 0, 0,
                                MediaCodec.BUFFER_FLAG_END_OF_STREAM);
                            inputDone = true;
                        } else {
                            long pts = mExtractor.getSampleTime();
                            mCodec.queueInputBuffer(inIndex, 0, sampleSize, pts, 0);
                            mExtractor.advance();
                        }
                    }
                }

                // 取出
                int outIndex = mCodec.dequeueOutputBuffer(info, TIMEOUT_US);
                if (outIndex >= 0) {
                    // 按 PTS 节流，保持正常播放速度；有音轨时以音频硬件时钟为准精确对齐音画。
                    if (info.size > 0) {
                        // 真正的循环点：本帧 PTS 比上一帧小，说明已经从片尾绕回片头，
                        // 在这里（而不是输入端 seek 时）重建节流基准，避免管线延迟导致的错误基准。
                        boolean wrapped = lastOutputPts >= 0 && info.presentationTimeUs < lastOutputPts;
                        if (wrapped) {
                            startNs = -1;
                            if (mDurationUs > 0) loopEpoch++;
                        }
                        lastOutputPts = info.presentationTimeUs;

                        // 优先用同步时钟来源（另一路会话）的音频时钟对齐，让两路画面彼此同步；
                        // 没有指定 master 时才用自己的音频时钟（没有音轨则没有时钟）。
                        VideoPlaybackSession masterSession = mClockMaster;
                        long audioUs = masterSession != null
                            ? masterSession.audioPositionUs()
                            : (mWithAudio ? audioPositionUs() : -1);
                        // 无音轨/取不到时钟，或"循环+时长未知"（无法换算跨轮次累计时间）时才退回自由时钟节流。
                        boolean useAudioClock = audioUs >= 0 && (!mLoop || mDurationUs > 0);

                        if (useAudioClock) {
                            long cumulativeVideoUs = loopEpoch * mDurationUs + info.presentationTimeUs;
                            long deltaUs = cumulativeVideoUs - audioUs;
                            if (deltaUs > 0) {
                                // 限幅，避免异常值（如刚起播音频时钟还未走动）导致长时间卡死等待
                                long delayNs = Math.min(deltaUs, 200_000L) * 1000;
                                try { Thread.sleep(delayNs / 1_000_000, (int) (delayNs % 1_000_000)); }
                                catch (InterruptedException e) { Thread.currentThread().interrupt(); }
                            }
                            // deltaUs <= 0：视频已落后音频，不睡眠，立刻渲染追赶，每帧都会重新校正，不会累积误差
                        } else {
                            if (startNs < 0) startNs = System.nanoTime() - info.presentationTimeUs * 1000;
                            long expectedNs = startNs + info.presentationTimeUs * 1000;
                            long delayNs = expectedNs - System.nanoTime();
                            if (delayNs > 0) {
                                try { Thread.sleep(delayNs / 1_000_000, (int) (delayNs % 1_000_000)); }
                                catch (InterruptedException e) { Thread.currentThread().interrupt(); }
                            }
                        }
                    }
                    boolean render = info.size > 0;
                    if (render) mLastPresentationUs = info.presentationTimeUs;
                    mCodec.releaseOutputBuffer(outIndex, render);

                    if ((info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) {
                        if (mLoop) {
                            // 循环模式理论上不会到这里（输入端已 seek），保险处理
                            mCodec.flush();
                            mExtractor.seekTo(0, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
                            inputDone = false;
                            startNs = -1;
                        } else {
                            mFinished.set(true);
                            break;
                        }
                    }
                }
            }
        } catch (Throwable t) {
            if (mDecodeRunning) Log.e(TAG, "decodeLoop 异常", t);
        }
    }

    // ---------------------------------------------------------------------
    // 4) 帧回调 + GL 采样到目标 2D 纹理
    // ---------------------------------------------------------------------
    @Override
    public void onFrameAvailable(SurfaceTexture st) {
        mFrameAvailable.set(true);
    }

    boolean isNewFrameAvailable() { return mFrameAvailable.get(); }
    boolean isFirstFrameReady()   { return mFirstFrameReady.get(); }
    boolean isFinished()          { return mFinished.get(); }

    /** 暂停/恢复：暂停时冻结在当前帧（解码线程空转），恢复后继续播放；同步暂停/恢复音轨。 */
    void setPaused(boolean paused) {
        mPaused = paused;
        if (mAudioTrack != null) {
            try {
                if (paused) mAudioTrack.pause(); else mAudioTrack.play();
            } catch (Throwable t) {
                Log.w(TAG, "setPaused audioTrack: " + t.getMessage());
            }
        }
    }

    /** 按归一化进度 [0,1] seek（同步帧精度）；解码线程在下一轮处理 */
    void seekNormalized(float t) {
        if (t < 0f) t = 0f;
        if (t > 1f) t = 1f;
        mSeekRequest = t;
    }

    /** 当前归一化播放进度 [0,1] */
    float getNormalizedTime() {
        if (mDurationUs <= 0) return 0f;
        float t = (float) ((double) mLastPresentationUs / (double) mDurationUs);
        if (t < 0f) return 0f;
        if (t > 1f) return 1f;
        return t;
    }

    /** 由 Unity 每帧驱动：若有新帧则 updateTexImage 并 blit 到目标 2D 纹理。GL 线程执行。 */
    void requestDraw() {
        if (mReleased.get() || !mGlReady.get()) return;
        if (!mFrameAvailable.compareAndSet(true, false)) return;
        GlTextureManager.getInstance().runOnGlThread(this::drawFrameLocked);
    }

    private void drawFrameLocked() {
        if (mReleased.get() || mSurfaceTexture == null) return;
        try {
            mSurfaceTexture.updateTexImage();
            mSurfaceTexture.getTransformMatrix(mStMatrix);
        } catch (Throwable t) {
            Log.w(TAG, "updateTexImage: " + t.getMessage());
            return;
        }

        GLES20.glBindFramebuffer(GLES20.GL_FRAMEBUFFER, mFbo[0]);
        GLES20.glFramebufferTexture2D(GLES20.GL_FRAMEBUFFER, GLES20.GL_COLOR_ATTACHMENT0,
            GLES20.GL_TEXTURE_2D, mTargetTexId, 0);

        int status = GLES20.glCheckFramebufferStatus(GLES20.GL_FRAMEBUFFER);
        if (status != GLES20.GL_FRAMEBUFFER_COMPLETE) {
            Log.w(TAG, "FBO 不完整: 0x" + Integer.toHexString(status));
            GLES20.glBindFramebuffer(GLES20.GL_FRAMEBUFFER, 0);
            return;
        }

        GLES20.glViewport(0, 0, mWidth, mHeight);
        GLES20.glClearColor(0f, 0f, 0f, 1f);
        GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT);

        GLES20.glUseProgram(mProgram);

        GLES20.glActiveTexture(GLES20.GL_TEXTURE0);
        GLES20.glBindTexture(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, mOesTexId);
        GLES20.glUniform1i(mUTextureLoc, 0);
        GLES20.glUniformMatrix4fv(mUTexMatrixLoc, 1, false, mStMatrix, 0);

        GLES20.glEnableVertexAttribArray(mAPositionLoc);
        GLES20.glVertexAttribPointer(mAPositionLoc, 2, GLES20.GL_FLOAT, false, 0, mVertexBuffer);
        GLES20.glEnableVertexAttribArray(mATexCoordLoc);
        GLES20.glVertexAttribPointer(mATexCoordLoc, 2, GLES20.GL_FLOAT, false, 0, mTexBuffer);

        GLES20.glDrawArrays(GLES20.GL_TRIANGLE_STRIP, 0, 4);

        GLES20.glDisableVertexAttribArray(mAPositionLoc);
        GLES20.glDisableVertexAttribArray(mATexCoordLoc);
        GLES20.glBindTexture(GLES11Ext.GL_TEXTURE_EXTERNAL_OES, 0);
        GLES20.glBindFramebuffer(GLES20.GL_FRAMEBUFFER, 0);
        GLES20.glFlush();

        mFirstFrameReady.set(true);
    }

    // ---------------------------------------------------------------------
    // 5) 释放
    // ---------------------------------------------------------------------
    void release() {
        if (!mReleased.compareAndSet(false, true)) return;
        mDecodeRunning = false;
        if (mDecodeThread != null) {
            try { mDecodeThread.join(500); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
            mDecodeThread = null;
        }
        try { if (mCodec != null) { mCodec.stop(); mCodec.release(); } }
        catch (Throwable t) { Log.w(TAG, "release codec: " + t.getMessage()); }
        mCodec = null;
        try { if (mExtractor != null) mExtractor.release(); }
        catch (Throwable t) { Log.w(TAG, "release extractor: " + t.getMessage()); }
        mExtractor = null;

        stopAudioInternal();

        final int oes = mOesTexId;
        final int program = mProgram;
        final int fbo = mFbo[0];
        final SurfaceTexture st = mSurfaceTexture;
        final Surface surface = mSurface;
        mOesTexId = -1; mProgram = 0; mFbo[0] = 0;
        mSurfaceTexture = null; mSurface = null;

        GlTextureManager.getInstance().runOnGlThread(() -> {
            try {
                if (program != 0) GLES20.glDeleteProgram(program);
                if (fbo != 0) GLES20.glDeleteFramebuffers(1, new int[]{fbo}, 0);
                if (oes > 0) GLES20.glDeleteTextures(1, new int[]{oes}, 0);
            } catch (Throwable t) { Log.w(TAG, "release GL: " + t.getMessage()); }
            if (surface != null) surface.release();
            if (st != null) st.release();
        });
        Log.i(TAG, "release: 会话已释放 target2D=" + mTargetTexId);
    }

    // ---------------------------------------------------------------------
    // 工具
    // ---------------------------------------------------------------------
    private static FloatBuffer toFloatBuffer(float[] data) {
        FloatBuffer fb = ByteBuffer.allocateDirect(data.length * 4)
            .order(ByteOrder.nativeOrder()).asFloatBuffer();
        fb.put(data).position(0);
        return fb;
    }

    private static int buildProgram(String vSrc, String fSrc) {
        int vs = compileShader(GLES20.GL_VERTEX_SHADER, vSrc);
        int fs = compileShader(GLES20.GL_FRAGMENT_SHADER, fSrc);
        int program = GLES20.glCreateProgram();
        GLES20.glAttachShader(program, vs);
        GLES20.glAttachShader(program, fs);
        GLES20.glLinkProgram(program);
        int[] linked = new int[1];
        GLES20.glGetProgramiv(program, GLES20.GL_LINK_STATUS, linked, 0);
        if (linked[0] == 0) {
            Log.e(TAG, "link 失败: " + GLES20.glGetProgramInfoLog(program));
            GLES20.glDeleteProgram(program);
            program = 0;
        }
        GLES20.glDeleteShader(vs);
        GLES20.glDeleteShader(fs);
        return program;
    }

    private static int compileShader(int type, String src) {
        int shader = GLES20.glCreateShader(type);
        GLES20.glShaderSource(shader, src);
        GLES20.glCompileShader(shader);
        int[] compiled = new int[1];
        GLES20.glGetShaderiv(shader, GLES20.GL_COMPILE_STATUS, compiled, 0);
        if (compiled[0] == 0) {
            Log.e(TAG, "shader 编译失败(type=" + type + "): " + GLES20.glGetShaderInfoLog(shader));
            GLES20.glDeleteShader(shader);
            return 0;
        }
        return shader;
    }
}
