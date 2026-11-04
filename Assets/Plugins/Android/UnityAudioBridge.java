package com.glsa.mqttscorecard5s.unity;

import com.example.multisinkaudio.alsa.Mp3DecodeHelper;
import com.example.multisinkaudio.alsa.TinyAlsaMixer;
import com.example.multisinkaudio.alsa.TinyAlsaPlayer;

import android.content.Context;
import android.util.Log;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Unity C# ↔ Android ALSA 桥接层。
 *
 * 全部音源（Unity AudioClip 的 PCM、APK assets 里的 mp3）统一进入 {@link TinyAlsaMixer}：
 *   - mixer 独占 PCM 设备，固定输出 48000Hz / stereo
 *   - 多 voice 软件混音，解决 tinyalsa 同 PCM 节点不能并发 pcm_open 的限制
 *
 * 公开 API 与之前保持兼容：
 *   path 类  (assets 里的 mp3)：playBGM / playSFX / playSustained / stopBGM / stopSustained
 *   PCM 类   (Unity AudioClip)：playBGMFromPcm / playSFXFromPcm / playSustainedFromPcm / stopBGMPcm / stopSustainedPcm
 *
 * 调试接口：listAlsaCards / readCardsProc / mixerActiveVoices
 */
public class UnityAudioBridge {

    private static final String TAG = "UnityAudioBridge";

    // ── 输出声卡配置（默认值匹配 RK3588 当前 ROM 的实测映射）────────────
    //   card 0 [rockchiphdmiin]  HDMI 输入（不可播放）
    //   card 1 [rockchipes8388]  板载 ES8388
    //   card 2 [rockchiphdmi0]   HDMI 输出 ✓
    private static int HDMI_CARD      = 2;
    private static int HDMI_DEVICE    = 0;
    private static int BUILTIN_CARD   = 1;
    private static int BUILTIN_DEVICE = 0;

    private static int activeCard    = HDMI_CARD;     // 默认 HDMI
    private static int activeDevice  = HDMI_DEVICE;

    private static Context appContext;
    private static ExecutorService decodeExecutor;

    public static final int OUTPUT_HDMI    = 0;
    public static final int OUTPUT_BUILTIN = 1;

    // 命名 voice（BGM / Sustained）记录
    private static final String BGM_NAME = "__BGM__";
    private static final ConcurrentHashMap<String, Integer> sustainedVoices = new ConcurrentHashMap<>();

    // ════════════════════════════════════════════════════════════════════════
    //  初始化 / 声卡切换
    // ════════════════════════════════════════════════════════════════════════

    /**
     * 可选：启动时显式指定声卡映射 / 输出口。
     *
     * 在 HybridCLR 热更程序集里 [RuntimeInitializeOnLoadMethod] 不会被 Unity 触发，
     * 所以 Unity 启动时不一定调到这里。Bridge 自带默认值（HDMI=2 / Builtin=1）+ lazy auto-start，
     * 即使不调 init() 也能正常工作。这个方法仅在你的设备声卡映射不一样时才需要调。
     *
     * @param outputType  0=HDMI  1=板载声卡
     */
    public static void init(Context ctx, int outputType, int hdmiCard, int builtinCard) {
        appContext   = ctx.getApplicationContext();
        ensureDecodeExecutor();
        HDMI_CARD    = hdmiCard;
        BUILTIN_CARD = builtinCard;
        setOutput(outputType);
    }

    /** 运行时切换输出声卡（会停止当前所有播放并重启 mixer）。 */
    public static void setOutput(int outputType) {
        TinyAlsaMixer.stop();
        sustainedVoices.clear();

        if (outputType == OUTPUT_HDMI) {
            activeCard   = HDMI_CARD;
            activeDevice = HDMI_DEVICE;
        } else {
            activeCard   = BUILTIN_CARD;
            activeDevice = BUILTIN_DEVICE;
        }

        TinyAlsaMixer.start(activeCard, activeDevice);
        Log.i(TAG, "setOutput type=" + outputType + " card=" + activeCard + " dev=" + activeDevice);
    }

    /**
     * Lazy 启动：第一次播放调一次。
     * 解决 HybridCLR 热更程序集中 [RuntimeInitializeOnLoadMethod] 不会被 Unity 触发的问题——
     * 即便 Unity 没显式调 init()，也能保证 mixer 已经在默认 HDMI 上跑起来。
     */
    private static void ensureMixerStarted() {
        if (TinyAlsaMixer.isRunning()) return;
        synchronized (UnityAudioBridge.class) {
            if (TinyAlsaMixer.isRunning()) return;
            TinyAlsaMixer.start(activeCard, activeDevice);
            Log.i(TAG, "ensureMixerStarted: lazy started card=" + activeCard + " dev=" + activeDevice);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PCM 路径（Unity AudioClip → mixer）
    // ════════════════════════════════════════════════════════════════════════

    /** 单次音效（SFX / UI 音效）。 */
    public static void playSFXFromPcm(byte[] pcm, int sampleRate, int channels) {
        ensureMixerStarted();
        TinyAlsaMixer.addPcmVoice(pcm, sampleRate, channels,
                /* loop */ false, /* volume */ 1f, /* name */ null);
    }

    /** 循环 BGM。 */
    public static void playBGMFromPcm(byte[] pcm, int sampleRate, int channels) {
        ensureMixerStarted();
        TinyAlsaMixer.addPcmVoice(pcm, sampleRate, channels,
                /* loop */ true, /* volume */ 1f, BGM_NAME);
    }

    public static void stopBGMPcm() {
        TinyAlsaMixer.removeByName(BGM_NAME);
    }

    /** 持续音效，可命名 / 循环 / 按名停止。 */
    public static void playSustainedFromPcm(String name, byte[] pcm,
                                            int sampleRate, int channels, boolean loop) {
        ensureMixerStarted();
        int id = TinyAlsaMixer.addPcmVoice(pcm, sampleRate, channels, loop, 1f, name);
        if (id != 0) sustainedVoices.put(name, id);
    }

    public static void stopSustainedPcm(String name) {
        sustainedVoices.remove(name);
        TinyAlsaMixer.removeByName(name);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  路径播放（assets/ 里的 mp3 / wav）— 异步解码 → mixer
    // ════════════════════════════════════════════════════════════════════════

    public static void playBGM(String assetName) {
        decodeAndAddVoice(assetName, /* loop */ true,  /* name */ BGM_NAME);
    }

    public static void stopBGM() {
        TinyAlsaMixer.removeByName(BGM_NAME);
    }

    public static void playSustained(String name, String assetName, boolean loop) {
        decodeAndAddVoice(assetName, loop, name);
    }

    public static void stopSustained(String name) {
        sustainedVoices.remove(name);
        TinyAlsaMixer.removeByName(name);
    }

    public static void playSFX(String assetName) {
        decodeAndAddVoice(assetName, /* loop */ false, /* name */ null);
    }

    private static void decodeAndAddVoice(String assetName, boolean loop, String name) {
        Context ctx = ensureContext();
        if (ctx == null) {
            Log.e(TAG, "appContext null and unable to obtain UnityPlayer activity, ignore: " + assetName);
            return;
        }
        ensureMixerStarted();
        ensureDecodeExecutor();
        decodeExecutor.submit(() -> {
            try {
                Mp3DecodeHelper.Result r = Mp3DecodeHelper.decodeAsset(ctx, assetName);
                int id = TinyAlsaMixer.addPcmVoice(r.pcm, r.sampleRate, r.channels, loop, 1f, name);
                if (id != 0 && name != null) sustainedVoices.put(name, id);
            } catch (Exception e) {
                Log.e(TAG, "decodeAndAddVoice failed: " + assetName, e);
            }
        });
    }

    /** 拿不到 appContext 时，反射 com.unity3d.player.UnityPlayer.currentActivity 兜底。 */
    private static Context ensureContext() {
        if (appContext != null) return appContext;
        try {
            Class<?> up = Class.forName("com.unity3d.player.UnityPlayer");
            android.app.Activity activity = (android.app.Activity) up
                    .getDeclaredField("currentActivity").get(null);
            if (activity != null) {
                appContext = activity.getApplicationContext();
                return appContext;
            }
        } catch (Throwable t) {
            Log.e(TAG, "ensureContext failed", t);
        }
        return null;
    }

    private static void ensureDecodeExecutor() {
        if (decodeExecutor != null) return;
        synchronized (UnityAudioBridge.class) {
            if (decodeExecutor != null) return;
            decodeExecutor = Executors.newSingleThreadExecutor(r -> {
                Thread t = new Thread(r, "Mp3DecodePool");
                t.setPriority(Thread.NORM_PRIORITY - 1);
                return t;
            });
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  全局停止
    // ════════════════════════════════════════════════════════════════════════

    public static void stopAll() {
        TinyAlsaMixer.removeAll();
        sustainedVoices.clear();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  播放状态查询（对应 2026-05-06 库新增 hasNamedVoice）
    // ════════════════════════════════════════════════════════════════════════

    /**
     * 查询 BGM 是否正在混音池中（防重复触发）。
     * 底层：{@link TinyAlsaMixer#hasNamedVoice(String)}
     */
    public static boolean isBGMPlaying() {
        return TinyAlsaMixer.hasNamedVoice(BGM_NAME);
    }

    /**
     * 查询指定命名 voice 是否正在混音池中。
     * 可用于 Sustained / BGM 等任意命名 voice 的状态判断。
     */
    public static boolean isNamedVoicePlaying(String name) {
        return TinyAlsaMixer.hasNamedVoice(name);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  音量控制（对应 2026-05-06 库新增 masterVolume / setVoiceVolumeByName）
    // ════════════════════════════════════════════════════════════════════════

    /**
     * 设置混音器总输出音量（对所有 voice 生效）。
     *
     * @param volume 0.0=静音  1.0=原始电平（默认）  最大 1.5（软 boost，可能轻微失真）
     */
    public static void setMasterVolume(float volume) {
        TinyAlsaMixer.setMasterVolume(volume);
    }

    /** 查询当前混音器总音量。 */
    public static float getMasterVolume() {
        return TinyAlsaMixer.getMasterVolume();
    }

    /**
     * 单独调整某路命名 voice 的音量（不打断播放）。
     * 典型用途：单独降低 BGM 音量而不影响 SFX。
     *
     * @param name   voice 名称（例如 BGM 用 "__BGM__"，Sustained 用 {@link #playSustainedFromPcm} 的 name）
     * @param volume 0.0~1.0
     * @return true 成功；false 该 name 的 voice 不存在
     */
    public static boolean setVoiceVolume(String name, float volume) {
        return TinyAlsaMixer.setVoiceVolumeByName(name, volume);
    }

    /** 单独调整 BGM 音量的快捷方法。 */
    public static boolean setBGMVolume(float volume) {
        return TinyAlsaMixer.setVoiceVolumeByName(BGM_NAME, volume);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  调试接口
    // ════════════════════════════════════════════════════════════════════════

    public static String listAlsaCards() {
        try {
            java.util.List<TinyAlsaPlayer.Card> cards = TinyAlsaPlayer.listCards();
            if (cards == null || cards.isEmpty()) {
                return "（未找到任何 ALSA 声卡，可能 /proc/asound/cards 不可读）";
            }
            StringBuilder sb = new StringBuilder();
            for (TinyAlsaPlayer.Card c : cards) {
                sb.append("card ").append(c.index)
                  .append(" [").append(c.id).append("] ")
                  .append(c.name).append('\n');
            }
            sb.append("当前激活：card=").append(activeCard).append(" device=").append(activeDevice);
            sb.append("  mixer running=").append(TinyAlsaMixer.isRunning());
            sb.append("  active voices=").append(TinyAlsaMixer.activeVoiceCount());
            sb.append("  masterVolume=").append(TinyAlsaMixer.getMasterVolume());
            return sb.toString();
        } catch (Throwable t) {
            return "listAlsaCards 异常：" + t.getMessage();
        }
    }

    public static String readCardsProc() {
        try {
            String raw = TinyAlsaPlayer.nativeReadCardsProc();
            return raw != null ? raw : "（nativeReadCardsProc 返回 null）";
        } catch (Throwable t) {
            return "readCardsProc 异常：" + t.getMessage();
        }
    }

    /** 读取 /proc/asound/pcm，诊断已打开的 PCM 节点。 */
    public static String readPcmProc() {
        try {
            String raw = TinyAlsaPlayer.nativeReadPcmProc();
            return raw != null ? raw : "（nativeReadPcmProc 返回 null）";
        } catch (Throwable t) {
            return "readPcmProc 异常：" + t.getMessage();
        }
    }

    public static int mixerActiveVoices() {
        return TinyAlsaMixer.activeVoiceCount();
    }

    /**
     * 手动为指定 card 的 codec 执行解静音配置（等价于 tinymix 打开 DAC/HP/SPK 通路）。
     * 主要用于板载声卡（ES8388 等）：绕开 AudioFlinger 后 codec 默认全静音，
     * 需要主动配置一次 mixer control 才有声音。HDMI 卡无此问题可忽略。
     *
     * @param card ALSA card 索引（板载声卡通常为 1）
     */
    public static void applyCodecUnmute(int card) {
        try {
            TinyAlsaPlayer.nativeApplyCodecUnmute(card);
            Log.i(TAG, "applyCodecUnmute card=" + card);
        } catch (Throwable t) {
            Log.e(TAG, "applyCodecUnmute failed card=" + card, t);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  诊断：合成 PCM 测试音
    //  完全绕开 Unity 资源加载 / AudioClip / 任何外部数据，
    //  在 Java 端合成一段固定 sine 波直接送 mixer。
    //  保留作为 ALSA 通路出声诊断工具。
    // ════════════════════════════════════════════════════════════════════════
    public static void playTestTone() {
        ensureMixerStarted();
        // 1 秒 880Hz S16_LE stereo @ 48kHz
        int rate     = 48000;
        int channels = 2;
        int frames   = rate;          // 1 秒
        double freq  = 880.0;
        double amp   = 0.5;           // -6dB
        byte[] pcm = new byte[frames * channels * 2];
        for (int i = 0; i < frames; i++) {
            short s = (short) (Math.sin(2 * Math.PI * freq * i / rate) * amp * 32767);
            int   o = i * channels * 2;
            for (int c = 0; c < channels; c++) {
                pcm[o + c * 2]     = (byte)  (s & 0xFF);
                pcm[o + c * 2 + 1] = (byte) ((s >> 8) & 0xFF);
            }
        }
        TinyAlsaMixer.addPcmVoice(pcm, rate, channels, false, 1f, null);
    }
}
