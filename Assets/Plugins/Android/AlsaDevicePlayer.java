package com.glsa.mqttscorecard5s.unity;

import android.content.Context;
import android.util.Log;

import com.example.multisinkaudio.alsa.Mp3DecodeHelper;
import com.example.multisinkaudio.alsa.TinyAlsaMixer;
import com.example.multisinkaudio.alsa.TinyAlsaPlayer;

import java.io.BufferedInputStream;
import java.io.DataInputStream;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * 独立的 ALSA 声卡过滤播放工具。
 *
 * 通过声卡名称关键词（如 "hdmi"）自动过滤并选择目标声卡，
 * 与 Unity 原生音频系统完全独立（不调用 AudioSource / AudioMixer）。
 *
 * 底层复用 {@link TinyAlsaMixer} 软件混音器：
 *   - 以 key 为 voice 命名，天然支持并发多路叠加
 *   - pause = 移除 voice 并保存 PCM 状态；resume = 重新加入 voice
 *   - 同一 key 重复 play 会替换旧 voice
 *
 * 线程安全：所有公开方法可从任意线程调用。
 */
public class AlsaDevicePlayer {

    private static final String TAG = "AlsaDevicePlayer";

    /** 当前已找到并使用的声卡索引，-1 表示尚未初始化。 */
    private static volatile int activeCard   = -1;
    private static volatile int activeDevice =  0;

    /** 每个 key 对应的播放状态（PCM 数据 + 参数），用于 pause/resume。 */
    private static final ConcurrentHashMap<String, VoiceState> voiceStates =
            new ConcurrentHashMap<>();

    private static ExecutorService decodeExecutor;

    // ════════════════════════════════════════════════════════════════════
    //  内部数据结构
    // ════════════════════════════════════════════════════════════════════

    private static final class VoiceState {
        final byte[] pcm;
        final int    sampleRate;
        final int    channels;
        final boolean loop;
        volatile boolean paused;

        VoiceState(byte[] pcm, int sampleRate, int channels, boolean loop) {
            this.pcm        = pcm;
            this.sampleRate = sampleRate;
            this.channels   = channels;
            this.loop       = loop;
            this.paused     = false;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  声卡查找与 Mixer 初始化
    // ════════════════════════════════════════════════════════════════════

    /**
     * 根据名称关键词查找声卡并启动 Mixer（如需要）。
     *
     * 注意：TinyAlsaMixer 是全局单例；如果已运行于同一张 card，本调用幂等。
     * 若目标 card 与当前不同，会重启 Mixer（同时清空所有已有 voice）。
     *
     * @param filterName 声卡名称关键词，不区分大小写，例如 "hdmi"、"es8388"
     * @return 找到的 card 索引，-1 表示未找到
     */
    public static int findAndStartMixer(String filterName) {
        try {
            List<TinyAlsaPlayer.Card> cards = TinyAlsaPlayer.listCards();
            if (cards == null || cards.isEmpty()) {
                Log.e(TAG, "findAndStartMixer: no ALSA cards found");
                return -1;
            }
            String lower = filterName.toLowerCase();
            for (TinyAlsaPlayer.Card c : cards) {
                if (c.id.toLowerCase().contains(lower)
                        || c.name.toLowerCase().contains(lower)) {
                    activeCard   = c.index;
                    activeDevice = 0;
                    // start() 对相同 card/device 幂等；不同则自动重启
                    TinyAlsaMixer.start(activeCard, activeDevice);
                    if (c.id.toLowerCase().contains("es8316")) {
                        ensureEs8316Output(c.index);
                    }
                    Log.i(TAG, "findAndStartMixer: filter='" + filterName
                            + "' → card=" + c.index + " [" + c.id + "] " + c.name
                            + " running=" + TinyAlsaMixer.isRunning());
                    return c.index;
                }
            }
            Log.e(TAG, "findAndStartMixer: no card matches filter '" + filterName + "'");
            return -1;
        } catch (Throwable t) {
            Log.e(TAG, "findAndStartMixer failed", t);
            return -1;
        }
    }

    private static int es8316CheckedCard = -1;

    /**
     * es8316 耳机孔通路配置：固件默认可能把 DAC 到耳机的开关关掉、DAC 数字音量置 0，导致耳机孔无声；
     * 有的固件开机设成 3 3 又偏小。这里不管当前值一律写入 App 统一的通路与音量（每个 card 每个进程一次）。
     * 依赖 app 以 system uid 运行并且 /system/bin/tinymix 存在。
     */
    private static synchronized void ensureEs8316Output(final int card) {
        if (es8316CheckedCard == card) return;
        es8316CheckedCard = card;
        new Thread(new Runnable() {
            @Override public void run() {
                setMixer(card, "Left Headphone Mixer Left DAC Switch", "1");
                setMixer(card, "Right Headphone Mixer Right DAC Switch", "1");
                setMixer(card, "Headphone Mixer Volume", "4 4");
                setMixer(card, "ES8316 Speaker DAC Playback Volume", "192 192");
            }
        }, "es8316-unmute").start();
    }

    private static void setMixer(int card, String control, String value) {
        try {
            // 多声道值必须拆成独立参数（"4 4" 作为单个参数会被 tinymix 当字符串拒绝）
            String[] values = value.trim().split("\\s+");
            String[] args = new String[3 + values.length];
            args[0] = "-D";
            args[1] = String.valueOf(card);
            args[2] = control;
            System.arraycopy(values, 0, args, 3, values.length);
            String out = runTinymix(args);
            if (out != null && out.contains("Error")) {
                Log.e(TAG, "es8316 设置失败: " + control + " -> " + value + " : " + out);
                return;
            }
            Log.i(TAG, "es8316 已设置: " + control + " -> " + value);
        } catch (Throwable t) {
            Log.e(TAG, "es8316 设置失败: " + control, t);
        }
    }

    private static String runTinymix(String... args) throws Exception {
        String[] cmd = new String[args.length + 1];
        cmd[0] = "/system/bin/tinymix";
        System.arraycopy(args, 0, cmd, 1, args.length);
        Process p = new ProcessBuilder(cmd).redirectErrorStream(true).start();
        java.io.BufferedReader r = new java.io.BufferedReader(new java.io.InputStreamReader(p.getInputStream()));
        StringBuilder sb = new StringBuilder();
        String line;
        while ((line = r.readLine()) != null) sb.append(line).append('\n');
        p.waitFor();
        return sb.toString().trim();
    }

    /** 确保 Mixer 在已找到的声卡上运行（懒初始化兜底）。 */
    private static boolean ensureMixer(String filterName) {
        if (TinyAlsaMixer.isRunning()) return true;
        return findAndStartMixer(filterName) >= 0 && TinyAlsaMixer.isRunning();
    }

    // ════════════════════════════════════════════════════════════════════
    //  PCM 直接播放
    // ════════════════════════════════════════════════════════════════════

    /**
     * 播放 PCM 字节数组到指定声卡。
     *
     * @param filterName 声卡过滤关键词，例如 "hdmi"
     * @param key        播放标识符（用于 pause/resume/stop）；相同 key 会替换旧 voice
     * @param pcm        S16_LE PCM 数据
     * @param sampleRate 采样率（Hz）
     * @param channels   声道数（1=mono，2=stereo）
     * @param loop       是否循环播放
     */
    public static void play(String filterName, String key, byte[] pcm,
                            int sampleRate, int channels, boolean loop) {
        play(filterName, key, pcm, sampleRate, channels, loop, 1.0f);
    }

    public static void play(String filterName, String key, byte[] pcm,
                            int sampleRate, int channels, boolean loop, float volume) {
        if (!ensureMixer(filterName)) {
            Log.e(TAG, "play: mixer not available for filter='" + filterName + "'");
            return;
        }
        VoiceState state = new VoiceState(pcm, sampleRate, channels, loop);
        voiceStates.put(key, state);
        // 同名旧 voice 由 TinyAlsaMixer.addPcmVoice 的 name 机制自动替换
        TinyAlsaMixer.addPcmVoice(pcm, sampleRate, channels, loop, 1.0f, key);
        if (volume >= 0f && volume != 1.0f) {
            TinyAlsaMixer.setVoiceVolumeByName(key, volume);
        }
        Log.i(TAG, "play: key=" + key + " filter=" + filterName
                + " rate=" + sampleRate + " ch=" + channels + " loop=" + loop
                + " bytes=" + (pcm != null ? pcm.length : 0) + " vol=" + volume);
    }

    /**
     * 在后台线程读取 C# {@code AudioClipPcmConverter} 写出的 .pcm16 缓存并播放。
     * 避免 Unity 主线程通过 JNI 搬运数十 MB 的 byte[]（会卡顿/白屏）。
     *
     * 文件格式（小端）：magic "PC16" + version(int) + sampleRate + channels + pcmLen + pcm
     */
    public static void playFromPcm16File(String filterName, String key, String path,
                                         boolean loop, float volume) {
        if (path == null || path.isEmpty()) {
            Log.e(TAG, "playFromPcm16File: path empty key=" + key);
            return;
        }
        if (!ensureMixer(filterName)) {
            Log.e(TAG, "playFromPcm16File: mixer not available for filter='" + filterName + "'");
            return;
        }
        ensureDecodeExecutor();
        final String finalKey = key;
        final String finalPath = path;
        final boolean finalLoop = loop;
        final float finalVol = volume;
        decodeExecutor.submit(() -> {
            try {
                Pcm16Cache cached = readPcm16Cache(finalPath);
                VoiceState state = new VoiceState(cached.pcm, cached.sampleRate, cached.channels, finalLoop);
                voiceStates.put(finalKey, state);
                TinyAlsaMixer.addPcmVoice(cached.pcm, cached.sampleRate, cached.channels,
                        finalLoop, 1.0f, finalKey);
                if (finalVol >= 0f && finalVol != 1.0f) {
                    TinyAlsaMixer.setVoiceVolumeByName(finalKey, finalVol);
                }
                Log.i(TAG, "playFromPcm16File ok: key=" + finalKey + " path=" + finalPath
                        + " rate=" + cached.sampleRate + " ch=" + cached.channels
                        + " bytes=" + cached.pcm.length + " loop=" + finalLoop + " vol=" + finalVol);
            } catch (Exception e) {
                Log.e(TAG, "playFromPcm16File failed: key=" + finalKey + " path=" + finalPath, e);
            }
        });
    }

    // ════════════════════════════════════════════════════════════════════
    //  Assets MP3 异步播放
    // ════════════════════════════════════════════════════════════════════

    /**
     * 异步解码 APK assets 里的 MP3/WAV 并播放到指定声卡。
     *
     * @param ctx        Android Context（由 C# 通过 UnityPlayer.currentActivity 传入）
     * @param filterName 声卡过滤关键词
     * @param key        播放标识符
     * @param assetPath  assets 相对路径，例如 "music/bg.mp3"
     * @param loop       是否循环
     */
    public static void playFromAsset(Context ctx, String filterName, String key,
                                     String assetPath, boolean loop) {
        if (!ensureMixer(filterName)) {
            Log.e(TAG, "playFromAsset: mixer not available for filter='" + filterName + "'");
            return;
        }
        Context appCtx = ctx != null ? ctx.getApplicationContext() : ensureContext();
        if (appCtx == null) {
            Log.e(TAG, "playFromAsset: context is null");
            return;
        }
        ensureDecodeExecutor();
        final Context finalCtx = appCtx;
        decodeExecutor.submit(() -> {
            try {
                Mp3DecodeHelper.Result r = Mp3DecodeHelper.decodeAsset(finalCtx, assetPath);
                VoiceState state = new VoiceState(r.pcm, r.sampleRate, r.channels, loop);
                voiceStates.put(key, state);
                TinyAlsaMixer.addPcmVoice(r.pcm, r.sampleRate, r.channels, loop, 1.0f, key);
                Log.i(TAG, "playFromAsset ok: key=" + key + " asset=" + assetPath
                        + " rate=" + r.sampleRate + " ch=" + r.channels
                        + " bytes=" + r.pcm.length + " loop=" + loop);
            } catch (Exception e) {
                Log.e(TAG, "playFromAsset failed: key=" + key + " asset=" + assetPath, e);
            }
        });
    }

    // ════════════════════════════════════════════════════════════════════
    //  暂停 / 恢复 / 停止
    // ════════════════════════════════════════════════════════════════════

    /**
     * 暂停播放：从 Mixer 中移除该 voice，但保留 PCM 状态。
     * 调用 {@link #resume(String)} 可从头重新开始播放。
     */
    public static void pause(String key) {
        VoiceState state = voiceStates.get(key);
        if (state != null) {
            state.paused = true;
            TinyAlsaMixer.removeByName(key);
            Log.i(TAG, "pause: key=" + key);
        } else {
            Log.w(TAG, "pause: key not found: " + key);
        }
    }

    /**
     * 恢复播放：重新将已暂停的 voice 加入 Mixer（从头开始）。
     *
     * @param filterName 声卡过滤关键词（用于确保 Mixer 运行中）
     */
    public static void resume(String filterName, String key) {
        VoiceState state = voiceStates.get(key);
        if (state == null) {
            Log.w(TAG, "resume: key not found: " + key);
            return;
        }
        if (!state.paused) {
            Log.w(TAG, "resume: key not paused: " + key);
            return;
        }
        state.paused = false;
        if (!ensureMixer(filterName)) {
            Log.e(TAG, "resume: mixer not available");
            return;
        }
        TinyAlsaMixer.addPcmVoice(state.pcm, state.sampleRate, state.channels,
                state.loop, 1.0f, key);
        Log.i(TAG, "resume: key=" + key);
    }

    /**
     * 停止并清除指定 key 的播放状态。
     */
    public static void stop(String key) {
        voiceStates.remove(key);
        TinyAlsaMixer.removeByName(key);
        Log.i(TAG, "stop: key=" + key);
    }

    /**
     * 停止所有由 AlsaDevicePlayer 管理的 voice。
     * 不影响 {@link UnityAudioBridge} 管理的 voice。
     */
    public static void stopAll() {
        for (String key : voiceStates.keySet()) {
            TinyAlsaMixer.removeByName(key);
        }
        voiceStates.clear();
        Log.i(TAG, "stopAll: cleared " + voiceStates.size() + " voices");
    }

    /**
     * 查询某个 key 是否正在播放（已注册且未暂停）。
     */
    public static boolean isPlaying(String key) {
        VoiceState state = voiceStates.get(key);
        return state != null && !state.paused && TinyAlsaMixer.hasNamedVoice(key);
    }

    /**
     * 查询某个 key 是否已暂停。
     */
    public static boolean isPaused(String key) {
        VoiceState state = voiceStates.get(key);
        return state != null && state.paused;
    }

    // ════════════════════════════════════════════════════════════════════
    //  状态查询（直通 TinyAlsaMixer）
    // ════════════════════════════════════════════════════════════════════

    /**
     * 查询指定名字的 voice 是否在混音池中（未完成且未被 stop）。
     * 可用于防止重复触发同一段音频。
     */
    public static boolean isNamedVoicePlaying(String name) {
        return TinyAlsaMixer.hasNamedVoice(name);
    }

    // ════════════════════════════════════════════════════════════════════
    //  音量控制（直通 TinyAlsaMixer）
    // ════════════════════════════════════════════════════════════════════

    /**
     * 设置混音器总输出音量。
     *
     * @param volume 0.0=静音  1.0=原始电平（默认）  最大 1.5（软 boost）
     *               超过 1.0 时混音超过 32767 会被 clamp，可能产生轻微失真。
     */
    public static void setMasterVolume(float volume) {
        TinyAlsaMixer.setMasterVolume(volume);
        Log.i(TAG, "setMasterVolume: " + volume);
    }

    /** 查询当前混音器总输出音量。 */
    public static float getMasterVolume() {
        return TinyAlsaMixer.getMasterVolume();
    }

    /**
     * 单独调整某路命名 voice 的音量（不打断播放）。
     *
     * @param key    播放时传入的 key
     * @param volume 0.0~1.0
     * @return true 成功；false 该 key 对应的 voice 不存在
     */
    public static boolean setVoiceVolume(String key, float volume) {
        boolean ok = TinyAlsaMixer.setVoiceVolumeByName(key, volume);
        Log.i(TAG, "setVoiceVolume key=" + key + " vol=" + volume + " ok=" + ok);
        return ok;
    }

    // ════════════════════════════════════════════════════════════════════
    //  硬件 codec 控制（直通 TinyAlsaPlayer）
    // ════════════════════════════════════════════════════════════════════

    /**
     * 手动为指定声卡的 codec 执行解静音配置。
     *
     * 绕开 AudioFlinger 后，板载 codec（如 ES8388）默认全静音；
     * 调用本方法等价于在 shell 里跑 tinymix 打开 DAC/HP/SPK 通路。
     * HDMI 声卡无此问题，可不调。
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

    // ════════════════════════════════════════════════════════════════════
    //  调试 / 工具
    // ════════════════════════════════════════════════════════════════════

    /** 返回当前所有可用 ALSA 声卡的字符串列表。 */
    public static String listCards() {
        try {
            List<TinyAlsaPlayer.Card> cards = TinyAlsaPlayer.listCards();
            if (cards == null || cards.isEmpty()) return "No ALSA cards found";
            StringBuilder sb = new StringBuilder();
            for (TinyAlsaPlayer.Card c : cards) {
                sb.append("card ").append(c.index)
                  .append(" [").append(c.id).append("] ")
                  .append(c.name).append('\n');
            }
            sb.append("AlsaDevicePlayer activeCard=").append(activeCard)
              .append(" mixer running=").append(TinyAlsaMixer.isRunning())
              .append(" voices=").append(TinyAlsaMixer.activeVoiceCount())
              .append(" masterVolume=").append(TinyAlsaMixer.getMasterVolume());
            return sb.toString();
        } catch (Throwable t) {
            return "listCards error: " + t.getMessage();
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有工具
    // ════════════════════════════════════════════════════════════════════

    /** 无 Context 时，反射 UnityPlayer.currentActivity 兜底。 */
    private static Context ensureContext() {
        try {
            Class<?> up = Class.forName("com.unity3d.player.UnityPlayer");
            android.app.Activity act = (android.app.Activity)
                    up.getDeclaredField("currentActivity").get(null);
            return act != null ? act.getApplicationContext() : null;
        } catch (Throwable t) {
            Log.e(TAG, "ensureContext failed", t);
            return null;
        }
    }

    private static void ensureDecodeExecutor() {
        if (decodeExecutor != null) return;
        synchronized (AlsaDevicePlayer.class) {
            if (decodeExecutor != null) return;
            decodeExecutor = Executors.newSingleThreadExecutor(r -> {
                Thread t = new Thread(r, "AlsaDecodePool");
                t.setPriority(Thread.NORM_PRIORITY - 1);
                return t;
            });
        }
    }

    private static final class Pcm16Cache {
        final byte[] pcm;
        final int sampleRate;
        final int channels;

        Pcm16Cache(byte[] pcm, int sampleRate, int channels) {
            this.pcm = pcm;
            this.sampleRate = sampleRate;
            this.channels = channels;
        }
    }

    /** 读取 C# AudioClipPcmConverter 写入的小端 .pcm16 缓存。 */
    private static Pcm16Cache readPcm16Cache(String path) throws IOException {
        try (DataInputStream in = new DataInputStream(
                new BufferedInputStream(new FileInputStream(path), 1 << 20))) {
            byte[] magic = new byte[4];
            in.readFully(magic);
            if (magic[0] != 'P' || magic[1] != 'C' || magic[2] != '1' || magic[3] != '6') {
                throw new IOException("bad pcm16 magic");
            }
            int version = readIntLE(in);
            if (version != 1) {
                throw new IOException("unsupported pcm16 version=" + version);
            }
            int sampleRate = readIntLE(in);
            int channels = readIntLE(in);
            int pcmLen = readIntLE(in);
            if (pcmLen <= 0) {
                throw new IOException("bad pcmLen=" + pcmLen);
            }
            byte[] pcm = new byte[pcmLen];
            in.readFully(pcm);
            return new Pcm16Cache(pcm, sampleRate, channels);
        }
    }

    private static int readIntLE(DataInputStream in) throws IOException {
        int b0 = in.readUnsignedByte();
        int b1 = in.readUnsignedByte();
        int b2 = in.readUnsignedByte();
        int b3 = in.readUnsignedByte();
        return b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
    }
}
