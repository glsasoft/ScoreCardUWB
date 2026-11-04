package com.glsa.mqttscorecard5s.unity;

import android.app.Activity;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.util.Base64;
import android.util.Log;

import com.iflytek.sparkchain.core.SparkChain;
import com.iflytek.sparkchain.core.SparkChainConfig;
import com.iflytek.sparkchain.core.tts.PersonateTTS;
import com.iflytek.sparkchain.core.tts.TTS;
import com.iflytek.sparkchain.core.tts.TTSCallbacks;
import com.unity3d.player.UnityPlayer;

import java.io.ByteArrayOutputStream;

/**
 * 科大讯飞 SparkChain 超拟人语音合成 Android 桥接层。
 *
 * 与 {@link XFTTSBridge}（普通在线 TTS，基于 Msc.jar）并存，
 * 通过 SparkChain.aar + Codec.aar 调用更高质量的超拟人发音人。
 *
 * 关键设计：
 *   - SparkChain.getInst().init() 必须先调用，全局只初始化一次（{@link #ensureSparkChainInited}）。
 *   - 每次 speak() 创建一个新的 {@link PersonateTTS} 实例（与官方 Demo 一致）。
 *   - PersonateTTS.aRun() 是异步流式接口；onResult 在 SDK 工作线程中被多次回调，
 *     {@code status} 取值：0=开始, 1=中间, 2=结束。
 *
 * 输出模式：
 *   - {@link #speak}            走 Android AudioTrack 流式播放（默认声卡/电视）。
 *   - {@link #speakToExternal}  累积 PCM，结束时通过 Base64 一次性回传 Unity，
 *                                由 {@code AlsaDevicePlayer} 路由到指定 ALSA 声卡。
 *
 * 回调路径：
 *   PersonateTTS.TTSCallbacks → UnityPlayer.UnitySendMessage → XFPersonateTTSPlayer
 */
public class XFPersonateTTSBridge {

    private static final String TAG = "XFPersonateTTSBridge";

    /** 接收 Unity 回调的 GameObject 名称（由 init 传入）。 */
    private static String sCallbackObject = "";

    private static Activity sActivity;

    /** SparkChain SDK 是否已成功初始化（全局一次）。 */
    private static volatile boolean sSparkChainInited = false;
    /** SparkChain 初始化返回值，0=成功，非0=失败（错误码见集成文档第15章）。 */
    private static volatile int sSparkChainInitCode = -1;

    /** 默认参数，由 init() 设置，每次 speak 时使用。 */
    private static String sVcn         = "x6_lingxiaoxuan_pro";
    private static int    sSpeed       = 50;
    private static int    sPitch       = 50;
    private static int    sVolume      = 50;
    private static String sOralLevel   = "mid";
    private static boolean sSparkAssist = true;

    /** 当前正在合成的实例（用于 stop 时取消）。 */
    private static volatile PersonateTTS sCurrentTTS;

    // ────────────────────────────────────────────────────────────────────
    //  AudioTrack 流式播放（UnityAudio 模式）
    //  每次 speak 重建一次 AudioTrack；SDK 在子线程回调 onResult，
    //  AudioTrack.write 是线程安全的，可直接在回调线程写入。
    // ────────────────────────────────────────────────────────────────────

    private static volatile AudioTrack sAudioTrack;

    private static final int    SAMPLE_RATE     = 16000;
    private static final int    CHANNEL_CONFIG  = AudioFormat.CHANNEL_OUT_MONO;
    private static final int    AUDIO_FORMAT    = AudioFormat.ENCODING_PCM_16BIT;

    /** ExternalAlsa 模式下累积的 PCM 数据。 */
    private static ByteArrayOutputStream sExternalPcmBuffer;

    // ────────────────────────────────────────────────────────────────────
    //  初始化
    // ────────────────────────────────────────────────────────────────────

    /**
     * 初始化超拟人 SDK。
     *
     * 注意：SparkChain.init 内部会做网络鉴权，可能耗时几百毫秒。
     * 多次调用本方法只会触发一次真正的 SDK 初始化（首次成功后跳过）。
     *
     * @param activity           Unity 当前 Activity（{@code UnityPlayer.currentActivity}）
     * @param callbackGameObject 接收 UnitySendMessage 回调的 GameObject 名
     * @param appId              讯飞控制台的 AppID（与普通 TTS 共用 "604ef9ce"）
     * @param apiKey             SparkChain 控制台分配的 ApiKey（必须）
     * @param apiSecret          SparkChain 控制台分配的 ApiSecret（必须）
     * @param vcn                默认发音人，例如 "x4_lingxiaoxuan_oral"
     * @param speed              默认语速 0~100
     * @param pitch              默认语调 0~100
     * @param volume             默认音量 0~100
     */
    public static void init(final Activity activity, final String callbackGameObject,
                            final String appId, final String apiKey, final String apiSecret,
                            final String vcn, final int speed, final int pitch, final int volume) {
        if (activity == null) {
            Log.e(TAG, "init: activity is null");
            return;
        }
        sActivity       = activity;
        sCallbackObject = callbackGameObject != null ? callbackGameObject : "";
        sVcn            = (vcn != null && !vcn.isEmpty()) ? vcn : sVcn;
        sSpeed          = clamp(speed,  0, 100);
        sPitch          = clamp(pitch,  0, 100);
        sVolume         = clamp(volume, 0, 100);

        ensureSparkChainInited(activity, appId, apiKey, apiSecret);

        Log.i(TAG, "init: callbackObj=" + sCallbackObject + " appId=" + appId
                + " vcn=" + sVcn + " speed=" + sSpeed + " pitch=" + sPitch
                + " volume=" + sVolume + " sparkChainCode=" + sSparkChainInitCode);
    }

    /**
     * SparkChain SDK 全局初始化（懒加载，首次成功后跳过）。
     */
    private static synchronized void ensureSparkChainInited(Activity activity,
                                                            String appId, String apiKey, String apiSecret) {
        if (sSparkChainInited) return;
        try {
            if (appId == null || appId.isEmpty()
                    || apiKey == null || apiKey.isEmpty()
                    || apiSecret == null || apiSecret.isEmpty()) {
                Log.e(TAG, "ensureSparkChainInited: appId/apiKey/apiSecret 不能为空，请在 Unity 端配置");
                sSparkChainInitCode = -1;
                return;
            }

            SparkChainConfig config = SparkChainConfig.builder()
                    .appID(appId)
                    .apiKey(apiKey)
                    .apiSecret(apiSecret);
            int ret = SparkChain.getInst().init(activity.getApplicationContext(), config);
            sSparkChainInitCode = ret;
            sSparkChainInited   = (ret == 0);
            Log.i(TAG, "SparkChain.init ret=" + ret + " (0=成功, 非0=失败, 错误码见集成文档第15章)");
        } catch (Throwable t) {
            sSparkChainInitCode = -2;
            Log.e(TAG, "ensureSparkChainInited error", t);
        }
    }

    /** 查询 SparkChain 初始化返回值，便于 Unity 端排查问题。 */
    public static int getInitCode() { return sSparkChainInitCode; }

    /** 查询 SparkChain 是否已初始化成功。 */
    public static boolean isInited() { return sSparkChainInited; }

    // ────────────────────────────────────────────────────────────────────
    //  播报（UnityAudio 模式 = AudioTrack 流式播放）
    // ────────────────────────────────────────────────────────────────────

    /**
     * 合成并播放文本，PCM 数据通过 AudioTrack 流式输出到系统默认声卡。
     */
    public static void speak(final String text) {
        if (!preCheck("speak")) return;
        // 重置 AudioTrack（每次 speak 用新实例，避免上次残留）
        recreateAudioTrack();
        sExternalPcmBuffer = null;

        new Thread(() -> {
            try {
                stopCurrentTTS();
                PersonateTTS tts = buildPersonateTTS();
                tts.registerCallbacks(buildCallbacks(false));
                sCurrentTTS = tts;
                int ret = tts.aRun(text);
                Log.i(TAG, "speak aRun ret=" + ret + " text=" + text);
            } catch (Throwable t) {
                Log.e(TAG, "speak failed", t);
                sendToUnityPersonate("OnPersonateTTSError", "speak_exception:" + t.getMessage());
            }
        }, "PersonateTTS-Speak").start();
    }

    /**
     * 合成 PCM 并累积，结束后通过 Base64 一次性回传 Unity。
     * Unity 收到后调用 AlsaDevicePlayer 播放到指定声卡。
     */
    public static void speakToExternal(final String text) {
        if (!preCheck("speakToExternal")) return;
        // 关闭可能存在的 AudioTrack（外置模式不走默认声卡）
        releaseAudioTrack();
        sExternalPcmBuffer = new ByteArrayOutputStream(64 * 1024);

        new Thread(() -> {
            try {
                stopCurrentTTS();
                PersonateTTS tts = buildPersonateTTS();
                tts.registerCallbacks(buildCallbacks(true));
                sCurrentTTS = tts;
                int ret = tts.aRun(text);
                Log.i(TAG, "speakToExternal aRun ret=" + ret + " text=" + text);
            } catch (Throwable t) {
                Log.e(TAG, "speakToExternal failed", t);
                sendToUnityPersonate("OnPersonateTTSError", "speak_exception:" + t.getMessage());
            }
        }, "PersonateTTS-SpeakExt").start();
    }

    /** 停止当前播报（也会停止 AudioTrack）。 */
    public static void stop() {
        new Thread(() -> {
            stopCurrentTTS();
            releaseAudioTrack();
            sExternalPcmBuffer = null;
            Log.i(TAG, "stop");
        }, "PersonateTTS-Stop").start();
    }

    /** 释放 SparkChain SDK 资源（应用退出时调用）。 */
    public static void destroy() {
        new Thread(() -> {
            stopCurrentTTS();
            releaseAudioTrack();
            sExternalPcmBuffer = null;
            // 注意：unInit() 会停止整个 SparkChain SDK；如果其他模块也在使用，
            // 应避免在此处调用。本项目目前仅 TTS 使用 SparkChain，可以释放。
            try {
                if (sSparkChainInited) {
                    SparkChain.getInst().unInit();
                    sSparkChainInited   = false;
                    sSparkChainInitCode = -1;
                    Log.i(TAG, "SparkChain.unInit done");
                }
            } catch (Throwable t) {
                Log.e(TAG, "unInit error", t);
            }
        }, "PersonateTTS-Destroy").start();
    }

    // ────────────────────────────────────────────────────────────────────
    //  动态参数设置（下次 speak 生效）
    // ────────────────────────────────────────────────────────────────────

    public static void setVoiceName(String vcn)        { if (vcn != null && !vcn.isEmpty()) sVcn = vcn; }
    public static void setSpeed(int speed)             { sSpeed  = clamp(speed,  0, 100); }
    public static void setPitch(int pitch)             { sPitch  = clamp(pitch,  0, 100); }
    public static void setVolume(int volume)           { sVolume = clamp(volume, 0, 100); }
    public static void setOralLevel(String level)      { if (level != null && !level.isEmpty()) sOralLevel = level; }
    public static void setSparkAssist(boolean enable)  { sSparkAssist = enable; }

    // ────────────────────────────────────────────────────────────────────
    //  内部：实例构建 / 回调
    // ────────────────────────────────────────────────────────────────────

    private static boolean preCheck(String tag) {
        if (sActivity == null) {
            Log.e(TAG, tag + ": 未调用 init()，sActivity 为 null");
            sendToUnityPersonate("OnPersonateTTSError", "not_initialized:activity_null");
            return false;
        }
        if (!sSparkChainInited) {
            Log.e(TAG, tag + ": SparkChain SDK 未成功初始化，code=" + sSparkChainInitCode);
            sendToUnityPersonate("OnPersonateTTSError",
                    "sparkchain_init_failed:" + sSparkChainInitCode);
            return false;
        }
        return true;
    }

    private static PersonateTTS buildPersonateTTS() {
        PersonateTTS tts = new PersonateTTS(sVcn);
        tts.speed(sSpeed);
        tts.pitch(sPitch);
        tts.volume(sVolume);
        tts.oralLevel(sOralLevel);
        tts.sparkAssist(sSparkAssist);
        // 输出 raw PCM 16k/16bit/mono（默认值，显式声明便于排查）
        tts.sampleRate(SAMPLE_RATE);
        tts.encoding("raw");
        tts.bitDepth(16);
        tts.channels(1);
        return tts;
    }

    private static void stopCurrentTTS() {
        PersonateTTS prev = sCurrentTTS;
        if (prev != null) {
            try { prev.stop(); } catch (Throwable ignored) { }
            sCurrentTTS = null;
        }
    }

    private static TTSCallbacks buildCallbacks(final boolean external) {
        return new TTSCallbacks() {
            private boolean speakBeginSent = false;

            @Override
            public void onResult(TTS.TTSResult result, Object usrTag) {
                if (result == null) return;

                if (!speakBeginSent) {
                    speakBeginSent = true;
                    sendToUnityPersonate("OnPersonateTTSSpeakBegin", "");
                }

                byte[] data   = result.getData();
                int    status = result.getStatus(); // 0=开始, 1=中间, 2=结束
                int    seq    = result.getSeq();

                if (data != null && data.length > 0) {
                    if (external) {
                        ByteArrayOutputStream buf = sExternalPcmBuffer;
                        if (buf != null) {
                            try { buf.write(data); }
                            catch (Throwable t) { Log.e(TAG, "buffer write failed", t); }
                        }
                    } else {
                        AudioTrack track = sAudioTrack;
                        if (track != null) {
                            try {
                                if (track.getPlayState() != AudioTrack.PLAYSTATE_PLAYING) {
                                    track.play();
                                }
                                track.write(data, 0, data.length);
                            } catch (Throwable t) {
                                Log.e(TAG, "AudioTrack.write failed", t);
                            }
                        }
                    }
                }

                // 进度估计：将 seq 折算到 0~99，结束时再发 100
                int percent = status == 2 ? 100 : Math.min(99, seq * 10);
                sendToUnityPersonate("OnPersonateTTSProgress", String.valueOf(percent));

                if (status == 2) {
                    if (external) {
                        ByteArrayOutputStream buf = sExternalPcmBuffer;
                        if (buf != null && buf.size() > 0) {
                            byte[] pcm = buf.toByteArray();
                            String base64 = Base64.encodeToString(pcm, Base64.NO_WRAP);
                            Log.i(TAG, "[External] PCM ready: " + pcm.length + " bytes, base64=" + base64.length());
                            sendToUnityPersonate("OnPersonateTTSPcmReady", base64);
                        } else {
                            Log.w(TAG, "[External] empty PCM buffer at status=2");
                            sendToUnityPersonate("OnPersonateTTSError", "pcm_empty");
                        }
                        sExternalPcmBuffer = null;
                    } else {
                        // UnityAudio 模式：等 AudioTrack 把缓存写完再发完成
                        flushAndStopAudioTrack();
                        sendToUnityPersonate("OnPersonateTTSCompleted", "");
                    }
                    sCurrentTTS = null;
                }
            }

            @Override
            public void onError(TTS.TTSError ttsError, Object usrTag) {
                int    code = ttsError != null ? ttsError.getCode()   : -1;
                String msg  = ttsError != null ? ttsError.getErrMsg() : "unknown";
                Log.e(TAG, "onError code=" + code + " msg=" + msg);
                sendToUnityPersonate("OnPersonateTTSError", code + ":" + msg);
                if (!external) flushAndStopAudioTrack();
                sExternalPcmBuffer = null;
                sCurrentTTS = null;
            }
        };
    }

    // ────────────────────────────────────────────────────────────────────
    //  AudioTrack 工具
    // ────────────────────────────────────────────────────────────────────

    private static synchronized void recreateAudioTrack() {
        releaseAudioTrack();
        try {
            int minBuf = AudioTrack.getMinBufferSize(SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_FORMAT);
            // 缓冲扩大到 4 倍最小值，减少首包欠载导致的卡顿
            int bufSize = Math.max(minBuf * 4, minBuf);
            sAudioTrack = new AudioTrack(
                    AudioManager.STREAM_MUSIC,
                    SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_FORMAT,
                    bufSize, AudioTrack.MODE_STREAM);
            Log.i(TAG, "AudioTrack created, bufSize=" + bufSize);
        } catch (Throwable t) {
            Log.e(TAG, "recreateAudioTrack failed", t);
            sAudioTrack = null;
        }
    }

    private static synchronized void flushAndStopAudioTrack() {
        AudioTrack track = sAudioTrack;
        if (track == null) return;
        try {
            // 写入一段静音，确保最后一帧被解码出去
            byte[] silence = new byte[1024];
            track.write(silence, 0, silence.length);
            track.stop();
        } catch (Throwable ignored) { }
    }

    private static synchronized void releaseAudioTrack() {
        AudioTrack track = sAudioTrack;
        if (track != null) {
            try {
                if (track.getPlayState() == AudioTrack.PLAYSTATE_PLAYING) track.stop();
                track.release();
            } catch (Throwable ignored) { }
            sAudioTrack = null;
        }
    }

    // ────────────────────────────────────────────────────────────────────
    //  Unity 通信
    // ────────────────────────────────────────────────────────────────────

    private static void sendToUnityPersonate(String method, String msg) {
        if (sCallbackObject == null || sCallbackObject.isEmpty()) return;
        try {
            UnityPlayer.UnitySendMessage(sCallbackObject, method, msg != null ? msg : "");
        } catch (Throwable t) {
            Log.e(TAG, "sendToUnityPersonate failed: " + method, t);
        }
    }

    private static int clamp(int v, int min, int max) {
        if (v < min) return min;
        if (v > max) return max;
        return v;
    }
}
