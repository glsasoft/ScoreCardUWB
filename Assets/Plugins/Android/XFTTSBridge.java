package com.glsa.mqttscorecard5s.unity;

import android.app.Activity;
import android.os.Bundle;
import android.util.Base64;
import android.util.Log;

import com.iflytek.cloud.SpeechConstant;
import com.iflytek.cloud.SpeechError;
import com.iflytek.cloud.SpeechSynthesizer;
import com.iflytek.cloud.SpeechUtility;
import com.iflytek.cloud.SynthesizerListener;
import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.io.FileInputStream;

/**
 * 科大讯飞语音合成 Android 端桥接层。
 *
 * 关键设计：
 *   - SpeechUtility.createUtility()   必须最先调用（在 init 内部完成，不依赖 Application）
 *   - SpeechSynthesizer.createSynthesizer() / startSpeaking() 必须在 Android UI 线程运行，
 *     因此所有 SDK 操作均通过 activity.runOnUiThread() 派发。
 *
 * 回调路径：
 *   SynthesizerListener → UnityPlayer.UnitySendMessage → XFTTSPlayer（Unity 主线程）
 */
public class XFTTSBridge {

    private static final String TAG    = "XFTTSBridge";
    private static final String APP_ID = "604ef9ce";

    private static SpeechSynthesizer mTts;
    private static Activity          sActivity;

    /** 接收回调的 Unity GameObject 名称（由 C# 初始化时传入）。 */
    private static String sCallbackObject = "";

    // ────────────────────────────────────────────────────────────────────
    //  初始化
    // ────────────────────────────────────────────────────────────────────

    /**
     * 初始化语音合成引擎。
     *
     * 内部已包含 SpeechUtility.createUtility()，无需在 Application.onCreate 预调。
     * 所有 SDK 操作均在 Android UI 线程执行。
     *
     * @param activity           当前 Activity（UnityPlayer.currentActivity）
     * @param callbackGameObject Unity 场景中接收回调的 GameObject 名称
     * @param voiceName          发音人，例如 "xiaoyan"
     * @param speed              语速 0~100
     * @param volume             音量 0~100
     */
    public static void init(final Activity activity, final String callbackGameObject,
                            final String voiceName, final int speed, final int volume) {
        if (activity == null) {
            Log.e(TAG, "init: activity is null");
            return;
        }
        sActivity      = activity;
        sCallbackObject = callbackGameObject != null ? callbackGameObject : "";

        // SpeechUtility 可在任意线程调用，且幂等（多次调用无副作用）
        SpeechUtility.createUtility(activity.getApplicationContext(), "appid=" + APP_ID);

        // createSynthesizer 必须在 Android UI 线程执行
        activity.runOnUiThread(() -> {
            if (mTts != null) {
                mTts.stopSpeaking();
                mTts.destroy();
                mTts = null;
            }

            mTts = SpeechSynthesizer.createSynthesizer(activity, null);
            if (mTts == null) {
                Log.e(TAG, "init: SpeechSynthesizer.createSynthesizer 返回 null");
                sendToUnity("OnTTSError", "init_failed:SpeechSynthesizer is null");
                return;
            }

            mTts.setParameter(SpeechConstant.VOICE_NAME, voiceName);
            mTts.setParameter(SpeechConstant.SPEED,      String.valueOf(speed));
            mTts.setParameter(SpeechConstant.VOLUME,     String.valueOf(volume));
            mTts.setParameter(SpeechConstant.ENGINE_TYPE, SpeechConstant.TYPE_CLOUD);

            Log.i(TAG, "init ok (UI thread): callbackObj=" + sCallbackObject
                    + " voice=" + voiceName + " speed=" + speed + " volume=" + volume);
        });
    }

    // ────────────────────────────────────────────────────────────────────
    //  播报控制
    // ────────────────────────────────────────────────────────────────────

    /**
     * 开始合成并播放文本（走 Android 默认音频输出，即系统 AudioTrack）。
     * 在 Android UI 线程执行。
     *
     * @param text 要合成的文字
     */
    public static void speak(final String text) {
        if (sActivity == null) {
            Log.e(TAG, "speak: 未调用 init()，sActivity 为 null");
            sendToUnity("OnTTSError", "not_initialized");
            return;
        }

        sActivity.runOnUiThread(() -> {
            if (mTts == null) {
                Log.w(TAG, "speak: mTts 为 null，可能 init 尚未完成，跳过本次播报");
                sendToUnity("OnTTSError", "not_initialized");
                return;
            }
            // 确保 TTS_AUDIO_PATH 为空，防止外置模式的残留设置影响正常播放
            mTts.setParameter(SpeechConstant.TTS_AUDIO_PATH, "");
            mTts.stopSpeaking();
            int ret = mTts.startSpeaking(text, sTtsListener);
            Log.i(TAG, "speak: text=" + text + " ret=" + ret);
        });
    }

    /**
     * 合成文本为 PCM 音频文件，完成后通过 Base64 回传 Unity。
     *
     * 适用场景：需要将 TTS 语音路由到 ALSA 指定声卡（战场音效声卡）时使用。
     * 合成完成后触发 Unity 回调 OnTTSPcmReady(String base64Pcm)，
     * Unity 侧收到后调用 AlsaDevicePlayer 播放。
     *
     * PCM 格式：16000Hz、16-bit LE、单声道（iFlytek 云端 TTS 默认输出格式）。
     *
     * @param text 要合成的文字
     */
    public static void speakToExternal(final String text) {
        if (sActivity == null) {
            Log.e(TAG, "speakToExternal: 未调用 init()，sActivity 为 null");
            sendToUnity("OnTTSError", "not_initialized");
            return;
        }

        sActivity.runOnUiThread(() -> {
            if (mTts == null) {
                Log.w(TAG, "speakToExternal: mTts 为 null，可能 init 尚未完成");
                sendToUnity("OnTTSError", "not_initialized");
                return;
            }

            // 设置 PCM 输出路径（写入 App 私有缓存目录，无需存储权限）
            sPcmOutputPath = sActivity.getCacheDir().getAbsolutePath() + "/xftts_temp.pcm";
            mTts.setParameter(SpeechConstant.TTS_AUDIO_PATH, sPcmOutputPath);

            mTts.stopSpeaking();
            // 当前 Msc.jar 版本只暴露 synthesizeToUri(text, uri, listener)；
            // 没有早期/其他版本的 synthesize(text, listener) 重载。
            // 这里直接把 sPcmOutputPath 当作输出 uri 传入，行为与设置
            // SpeechConstant.TTS_AUDIO_PATH 后调用 synthesize 等价。
            int ret = mTts.synthesizeToUri(text, sPcmOutputPath, sSynthesizeToFileListener);
            Log.i(TAG, "speakToExternal: text=" + text + " ret=" + ret + " pcm=" + sPcmOutputPath);
        });
    }

    private static String sPcmOutputPath = "";

    /** 合成到文件的监听器（外置声卡模式专用）。 */
    private static final SynthesizerListener sSynthesizeToFileListener = new SynthesizerListener() {

        @Override
        public void onSpeakBegin() {
            Log.d(TAG, "[External] onSpeakBegin (synthesizing)");
            sendToUnity("OnTTSSpeakBegin", "");
        }

        @Override
        public void onSpeakProgress(int percent, int beginPos, int endPos) {
            sendToUnity("OnTTSProgress", String.valueOf(percent));
        }

        @Override public void onSpeakPaused()  {}
        @Override public void onSpeakResumed() {}
        @Override public void onBufferProgress(int percent, int beginPos, int endPos, String info) {}

        @Override
        public void onCompleted(SpeechError error) {
            // 合成完成后，清除 TTS_AUDIO_PATH，避免影响后续 startSpeaking() 调用
            if (mTts != null) {
                mTts.setParameter(SpeechConstant.TTS_AUDIO_PATH, "");
            }

            if (error != null) {
                String desc = error.getPlainDescription(true);
                Log.e(TAG, "[External] onCompleted error: " + desc);
                sendToUnity("OnTTSError", desc);
                return;
            }

            // 读取合成的 PCM 文件，Base64 编码后发给 Unity
            try {
                File f = new File(sPcmOutputPath);
                if (!f.exists() || f.length() == 0) {
                    Log.e(TAG, "[External] PCM 文件不存在或为空: " + sPcmOutputPath);
                    sendToUnity("OnTTSError", "pcm_file_empty");
                    return;
                }

                byte[] pcm = new byte[(int) f.length()];
                FileInputStream fis = new FileInputStream(f);
                int read = fis.read(pcm);
                fis.close();
                f.delete(); // 读完即删，避免临时文件堆积

                if (read <= 0) {
                    sendToUnity("OnTTSError", "pcm_read_zero_bytes");
                    return;
                }

                String base64 = Base64.encodeToString(pcm, Base64.NO_WRAP);
                Log.i(TAG, "[External] PCM ready: " + read + " bytes -> Base64 len=" + base64.length());
                sendToUnity("OnTTSPcmReady", base64);

            } catch (Exception ex) {
                Log.e(TAG, "[External] 读取 PCM 文件失败: " + ex.getMessage());
                sendToUnity("OnTTSError", "pcm_read_failed:" + ex.getMessage());
            }
        }

        @Override public void onEvent(int eventType, int arg1, int arg2, Bundle obj) {}
    };

    /**
     * 停止当前正在播放的语音（UI 线程执行）。
     */
    public static void stopSpeaking() {
        if (sActivity == null || mTts == null) return;
        sActivity.runOnUiThread(() -> {
            mTts.stopSpeaking();
            Log.i(TAG, "stopSpeaking");
        });
    }

    /**
     * 动态修改发音人（UI 线程执行）。
     */
    public static void setVoiceName(final String voiceName) {
        if (sActivity == null || mTts == null) return;
        sActivity.runOnUiThread(() ->
                mTts.setParameter(SpeechConstant.VOICE_NAME, voiceName));
    }

    /**
     * 动态修改语速（0~100，UI 线程执行）。
     */
    public static void setSpeed(final int speed) {
        if (sActivity == null || mTts == null) return;
        sActivity.runOnUiThread(() ->
                mTts.setParameter(SpeechConstant.SPEED, String.valueOf(speed)));
    }

    /**
     * 动态修改音量（0~100，UI 线程执行）。
     */
    public static void setVolume(final int volume) {
        if (sActivity == null || mTts == null) return;
        sActivity.runOnUiThread(() ->
                mTts.setParameter(SpeechConstant.VOLUME, String.valueOf(volume)));
    }

    /**
     * 释放 TTS 引擎资源（UI 线程执行）。
     */
    public static void destroy() {
        if (sActivity == null || mTts == null) return;
        sActivity.runOnUiThread(() -> {
            mTts.stopSpeaking();
            mTts.destroy();
            mTts = null;
            Log.i(TAG, "destroy");
        });
    }

    // ────────────────────────────────────────────────────────────────────
    //  SynthesizerListener 实现
    // ────────────────────────────────────────────────────────────────────

    private static final SynthesizerListener sTtsListener = new SynthesizerListener() {

        @Override
        public void onSpeakBegin() {
            Log.d(TAG, "onSpeakBegin");
            sendToUnity("OnTTSSpeakBegin", "");
        }

        @Override
        public void onSpeakProgress(int percent, int beginPos, int endPos) {
            sendToUnity("OnTTSProgress", String.valueOf(percent));
        }

        @Override
        public void onSpeakPaused()  { }

        @Override
        public void onSpeakResumed() { }

        @Override
        public void onBufferProgress(int percent, int beginPos, int endPos, String info) { }

        @Override
        public void onCompleted(SpeechError error) {
            if (error == null) {
                Log.d(TAG, "onCompleted: success");
                sendToUnity("OnTTSCompleted", "");
            } else {
                String desc = error.getPlainDescription(true);
                Log.e(TAG, "onCompleted error: " + desc);
                sendToUnity("OnTTSError", desc);
            }
        }

        @Override
        public void onEvent(int eventType, int arg1, int arg2, Bundle obj) { }
    };

    // ────────────────────────────────────────────────────────────────────
    //  Unity 消息工具
    // ────────────────────────────────────────────────────────────────────

    private static void sendToUnity(String method, String msg) {
        if (sCallbackObject == null || sCallbackObject.isEmpty()) return;
        try {
            UnityPlayer.UnitySendMessage(sCallbackObject, method, msg != null ? msg : "");
        } catch (Throwable t) {
            Log.e(TAG, "sendToUnity failed: " + method + " / " + msg, t);
        }
    }
}
