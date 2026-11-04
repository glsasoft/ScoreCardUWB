package com.glsa.mqttscorecard5s.unity;

import android.content.Context;
import android.graphics.Point;
import android.hardware.display.DisplayManager;
import android.hardware.display.VirtualDisplay;
import android.media.AudioFormat;
import android.media.AudioRecord;
import android.media.MediaCodec;
import android.media.MediaCodecInfo;
import android.media.MediaFormat;
import android.media.MediaMuxer;
import android.media.MediaRecorder;
import android.util.Log;
import android.view.Surface;
import android.view.WindowManager;

import com.unity3d.player.UnityPlayer;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.concurrent.atomic.AtomicBoolean;

/**
 * 原生录屏 + 录内部声音，写成一个 mp4，不经过 MediaProjection 用户授权弹窗。
 *
 * 依赖：本 App 以 android.uid.system 运行（见 AndroidManifest sharedUserId）且以平台签名安装，
 * AndroidManifest 已声明 CAPTURE_VIDEO_OUTPUT / CAPTURE_AUDIO_OUTPUT（均为 signature|system
 * 保护级别权限，system uid + 平台签名下安装即自动授予，不会弹运行时权限对话框）。
 *
 * 画面：VirtualDisplay 镜像主屏内容（VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR）→ Surface → MediaCodec H264 编码。
 * 声音：AudioRecord(AudioSource.REMOTE_SUBMIX，系统"提交混音"内部音频)→ MediaCodec AAC 编码。
 * 两路编码器输出统一喂给同一个 MediaMuxer 写进一个 mp4：
 *   - 视频帧时间戳由 SurfaceFlinger 在合成时按系统单调时钟(CLOCK_MONOTONIC)打点；
 *   - 音频时间戳用 System.nanoTime() 在每次 AudioRecord.read() 时打点，同一时钟域；
 *   - 因此两路天然对齐，不需要额外的"打板"校准，也没有 adb screenrecord 单段 180 秒的限制。
 */
public class ScoreCardNativeRecorder {

    private static final String TAG = "ScoreCardNativeRecorder";
    private static final String VIDEO_MIME = "video/avc";
    private static final String AUDIO_MIME = "audio/mp4a-latm";
    private static final int AUDIO_SAMPLE_RATE = 48000;
    private static final int AUDIO_CHANNELS = 2;
    private static final int AUDIO_BIT_RATE = 192_000;
    private static final long DEQUEUE_TIMEOUT_US = 10_000;
    private static final int DRAIN_EOS_SAFETY_LOOPS = 200;

    private static volatile boolean isRecording = false;
    private static final AtomicBoolean stopRequested = new AtomicBoolean(false);

    private static MediaCodec videoEncoder;
    private static MediaCodec audioEncoder;
    private static VirtualDisplay virtualDisplay;
    private static AudioRecord audioRecord;
    private static MediaMuxer muxer;

    private static Thread videoThread;
    private static Thread audioThread;

    private static final Object muxerLock = new Object();
    private static int videoTrackIndex = -1;
    private static int audioTrackIndex = -1;
    private static volatile boolean muxerStarted = false;

    public static boolean isRecording() {
        return isRecording;
    }

    /**
     * 开始录制。width/height 传 0 表示使用设备当前物理分辨率（推荐，开发板原生 7680x1080）。
     *
     * @return 是否成功启动；失败时已自动释放所有已分配资源。
     */
    public static synchronized boolean start(String outputPath, int width, int height, int bitRate, int frameRate) {
        if (isRecording) {
            Log.w(TAG, "start: 已在录制中，忽略重复调用");
            return false;
        }

        try {
            Context ctx = UnityPlayer.currentActivity.getApplicationContext();

            if (width <= 0 || height <= 0) {
                WindowManager wm = (WindowManager) ctx.getSystemService(Context.WINDOW_SERVICE);
                Point size = new Point();
                wm.getDefaultDisplay().getRealSize(size);
                width = size.x;
                height = size.y;
            }

            java.io.File outFile = new java.io.File(outputPath);
            java.io.File parent = outFile.getParentFile();
            if (parent != null) parent.mkdirs();

            stopRequested.set(false);
            muxerStarted = false;
            videoTrackIndex = -1;
            audioTrackIndex = -1;

            muxer = new MediaMuxer(outputPath, MediaMuxer.OutputFormat.MUXER_OUTPUT_MPEG_4);

            setupVideoEncoder(ctx, width, height, bitRate, frameRate);
            setupAudioEncoder();

            isRecording = true;
            videoThread = new Thread(ScoreCardNativeRecorder::pumpVideo, "ScoreCardRecorder-Video");
            audioThread = new Thread(ScoreCardNativeRecorder::pumpAudio, "ScoreCardRecorder-Audio");
            videoThread.start();
            audioThread.start();

            Log.i(TAG, "start: 开始录制 " + outputPath + " " + width + "x" + height
                    + " bitRate=" + bitRate + " fps=" + frameRate);
            return true;
        } catch (Exception e) {
            Log.e(TAG, "start 失败", e);
            releaseAll();
            isRecording = false;
            return false;
        }
    }

    /** 停止录制并阻塞等待两路编码线程收尾（写完 mp4 的 moov box），2 秒超时保护。 */
    public static synchronized void stop() {
        if (!isRecording) return;
        Log.i(TAG, "stop: 请求停止录制");
        stopRequested.set(true);

        try { if (videoThread != null) videoThread.join(2000); } catch (InterruptedException ignored) {}
        try { if (audioThread != null) audioThread.join(2000); } catch (InterruptedException ignored) {}

        releaseAll();
        isRecording = false;
        Log.i(TAG, "stop: 录制已停止，文件已落盘");
    }

    // ════════════════════════════════════════════════════════════════════
    //  视频：VirtualDisplay(镜像主屏) → MediaCodec Surface 输入
    // ════════════════════════════════════════════════════════════════════

    private static void setupVideoEncoder(Context ctx, int width, int height, int bitRate, int frameRate) throws IOException {
        MediaFormat format = MediaFormat.createVideoFormat(VIDEO_MIME, width, height);
        format.setInteger(MediaFormat.KEY_COLOR_FORMAT, MediaCodecInfo.CodecCapabilities.COLOR_FormatSurface);
        format.setInteger(MediaFormat.KEY_BIT_RATE, bitRate);
        format.setInteger(MediaFormat.KEY_FRAME_RATE, frameRate);
        format.setInteger(MediaFormat.KEY_I_FRAME_INTERVAL, 2);

        videoEncoder = MediaCodec.createEncoderByType(VIDEO_MIME);
        videoEncoder.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);
        Surface inputSurface = videoEncoder.createInputSurface();
        videoEncoder.start();

        DisplayManager dm = (DisplayManager) ctx.getSystemService(Context.DISPLAY_SERVICE);
        int densityDpi = ctx.getResources().getDisplayMetrics().densityDpi;
        virtualDisplay = dm.createVirtualDisplay(
                "ScoreCardRecording",
                width, height, densityDpi,
                inputSurface,
                DisplayManager.VIRTUAL_DISPLAY_FLAG_AUTO_MIRROR);
    }

    private static void pumpVideo() {
        MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
        while (!stopRequested.get()) {
            drainVideoOnce(info);
        }

        try { videoEncoder.signalEndOfInputStream(); } catch (Exception e) { Log.e(TAG, "signalEndOfInputStream 失败", e); }
        boolean eos = false;
        for (int i = 0; i < DRAIN_EOS_SAFETY_LOOPS && !eos; i++) {
            eos = drainVideoOnce(info);
        }
    }

    /** @return 本次是否读到了 END_OF_STREAM 标记 */
    private static boolean drainVideoOnce(MediaCodec.BufferInfo info) {
        int index = videoEncoder.dequeueOutputBuffer(info, DEQUEUE_TIMEOUT_US);
        if (index == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
            synchronized (muxerLock) {
                videoTrackIndex = muxer.addTrack(videoEncoder.getOutputFormat());
                maybeStartMuxerLocked();
            }
            return false;
        }
        if (index < 0) return false;

        boolean isEos = (info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0;
        if (info.size > 0 && muxerStarted) {
            ByteBuffer buffer = videoEncoder.getOutputBuffer(index);
            buffer.position(info.offset);
            buffer.limit(info.offset + info.size);
            synchronized (muxerLock) {
                muxer.writeSampleData(videoTrackIndex, buffer, info);
            }
        }
        videoEncoder.releaseOutputBuffer(index, false);
        return isEos;
    }

    // ════════════════════════════════════════════════════════════════════
    //  音频：AudioRecord(REMOTE_SUBMIX，系统内部混音) → MediaCodec Buffer 输入
    // ════════════════════════════════════════════════════════════════════

    private static void setupAudioEncoder() throws IOException {
        int channelConfig = AUDIO_CHANNELS == 2 ? AudioFormat.CHANNEL_IN_STEREO : AudioFormat.CHANNEL_IN_MONO;
        int minBufSize = AudioRecord.getMinBufferSize(AUDIO_SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_16BIT);
        int bufSize = Math.max(minBufSize, AUDIO_SAMPLE_RATE); // 至少留够约 1 秒缓冲

        audioRecord = new AudioRecord(
                MediaRecorder.AudioSource.REMOTE_SUBMIX,
                AUDIO_SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_16BIT, bufSize);
        if (audioRecord.getState() != AudioRecord.STATE_INITIALIZED) {
            throw new IOException("AudioRecord(REMOTE_SUBMIX) 初始化失败，请确认已声明 CAPTURE_AUDIO_OUTPUT 权限");
        }

        MediaFormat format = MediaFormat.createAudioFormat(AUDIO_MIME, AUDIO_SAMPLE_RATE, AUDIO_CHANNELS);
        format.setInteger(MediaFormat.KEY_AAC_PROFILE, MediaCodecInfo.CodecProfileLevel.AACObjectLC);
        format.setInteger(MediaFormat.KEY_BIT_RATE, AUDIO_BIT_RATE);
        format.setInteger(MediaFormat.KEY_MAX_INPUT_SIZE, bufSize);

        audioEncoder = MediaCodec.createEncoderByType(AUDIO_MIME);
        audioEncoder.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);
        audioEncoder.start();
        audioRecord.startRecording();
    }

    private static void pumpAudio() {
        byte[] pcmBuf = new byte[4096];
        MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
        long startNanos = System.nanoTime();

        while (!stopRequested.get()) {
            int read = audioRecord.read(pcmBuf, 0, pcmBuf.length);
            if (read > 0) {
                feedAudioEncoder(pcmBuf, read, (System.nanoTime() - startNanos) / 1000, false);
            }
            drainAudioOnce(info);
        }

        try { audioRecord.stop(); } catch (Exception e) { Log.e(TAG, "audioRecord.stop 失败", e); }
        feedAudioEncoder(pcmBuf, 0, (System.nanoTime() - startNanos) / 1000, true);
        boolean eos = false;
        for (int i = 0; i < DRAIN_EOS_SAFETY_LOOPS && !eos; i++) {
            eos = drainAudioOnce(info);
        }
    }

    private static void feedAudioEncoder(byte[] pcm, int size, long ptsUs, boolean endOfStream) {
        int index = audioEncoder.dequeueInputBuffer(DEQUEUE_TIMEOUT_US);
        if (index < 0) return;

        ByteBuffer input = audioEncoder.getInputBuffer(index);
        input.clear();
        if (size > 0) input.put(pcm, 0, size);
        audioEncoder.queueInputBuffer(index, 0, size, ptsUs,
                endOfStream ? MediaCodec.BUFFER_FLAG_END_OF_STREAM : 0);
    }

    /** @return 本次是否读到了 END_OF_STREAM 标记 */
    private static boolean drainAudioOnce(MediaCodec.BufferInfo info) {
        int index = audioEncoder.dequeueOutputBuffer(info, DEQUEUE_TIMEOUT_US);
        if (index == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
            synchronized (muxerLock) {
                audioTrackIndex = muxer.addTrack(audioEncoder.getOutputFormat());
                maybeStartMuxerLocked();
            }
            return false;
        }
        if (index < 0) return false;

        boolean isEos = (info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0;
        if (info.size > 0 && muxerStarted) {
            ByteBuffer buffer = audioEncoder.getOutputBuffer(index);
            buffer.position(info.offset);
            buffer.limit(info.offset + info.size);
            synchronized (muxerLock) {
                muxer.writeSampleData(audioTrackIndex, buffer, info);
            }
        }
        audioEncoder.releaseOutputBuffer(index, false);
        return isEos;
    }

    /** 调用前必须已持有 muxerLock。 */
    private static void maybeStartMuxerLocked() {
        if (!muxerStarted && videoTrackIndex >= 0 && audioTrackIndex >= 0) {
            muxer.start();
            muxerStarted = true;
            Log.i(TAG, "maybeStartMuxerLocked: muxer 已启动 videoTrack=" + videoTrackIndex + " audioTrack=" + audioTrackIndex);
        }
    }

    private static void releaseAll() {
        try { if (virtualDisplay != null) virtualDisplay.release(); } catch (Exception e) { Log.e(TAG, "release virtualDisplay 失败", e); }
        try { if (videoEncoder != null) { videoEncoder.stop(); videoEncoder.release(); } } catch (Exception e) { Log.e(TAG, "release videoEncoder 失败", e); }
        try { if (audioRecord != null) audioRecord.release(); } catch (Exception e) { Log.e(TAG, "release audioRecord 失败", e); }
        try { if (audioEncoder != null) { audioEncoder.stop(); audioEncoder.release(); } } catch (Exception e) { Log.e(TAG, "release audioEncoder 失败", e); }
        try {
            synchronized (muxerLock) {
                if (muxer != null) {
                    if (muxerStarted) muxer.stop();
                    muxer.release();
                }
            }
        } catch (Exception e) { Log.e(TAG, "release muxer 失败", e); }

        virtualDisplay = null;
        videoEncoder = null;
        audioEncoder = null;
        audioRecord = null;
        muxer = null;
        videoThread = null;
        audioThread = null;
    }
}
