using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>播报音频要输出到的通道。</summary>
[Flags]
public enum AudioChannel
{
    /// <summary>Unity 自身音频引擎（AudioSource / TTS 播放），走系统默认输出，当前设备即 HDMI。</summary>
    Hdmi = 1 << 0,

    /// <summary>耳机口独立声卡，底层复用 <see cref="ExternalDeviceAudioPlayer"/>（Android ALSA / Windows WASAPI）。</summary>
    Headphone = 1 << 1,

    Both = Hdmi | Headphone,
}

/// <summary>
/// 双通道播报统一入口。HDMI 就是调用方本来就在用的 Unity AudioSource / XFVoiceCloneService，
/// 不需要经过这里；这里只负责按 <see cref="AudioChannel"/> 需要时把同一份音效/语音"额外复制一份到耳机口"
/// （底层复用现成的 <see cref="ExternalDeviceAudioPlayer"/>，与 <see cref="MusicDownloadManager"/>/<see cref="TTSManager"/>
/// 播战场音乐/音效走的是同一条通路，可与它们同时混音播放，互不冲突）。
///
/// 任何一路失败只记日志，不影响另一路继续播放；未来新增的音频只要调用这里的方法，
/// 就能按需选择"只 HDMI / 只耳机口 / 两者都播"，无需关心底层细节。
/// </summary>
public static class DualChannelAudio
{
    static string HeadphoneCard => BoardProfile.AudioCardSelector;

    // 底层每次 Play/PlayFromClip 都会为该 key 重建一路新 voice（音量重置为默认 1.0），
    // 故须记住每个 key 期望的音量，并在每次开播后重新应用，才能让"独立调整某路音量"跨多次播放持续生效。
    static readonly Dictionary<string, float> s_HeadphoneKeyVolumes = new Dictionary<string, float>();

    /// <summary>开发板是否识别到独立耳机声卡。识别不到时整条耳机链路都不存在。</summary>
    public static bool HeadphoneAvailable => !string.IsNullOrEmpty(HeadphoneCard);

    /// <summary>
    /// 这一路是否真的会出声：既要调用方点名了耳机口，也要板子上确实有那块声卡。
    /// 两个条件缺一不可——只看 channel 的话，Editor 和无声卡的机器上调用方会为一段
    /// 根本没发生的播放白等，把播报的忙窗口拖长好几秒。
    /// </summary>
    public static bool WantsHeadphone(AudioChannel channel) =>
        (channel & AudioChannel.Headphone) != 0 && HeadphoneAvailable;

    /// <summary>耳机口播放一段固定音效（wav/mp3 解出的 AudioClip）。key 相同会替换上一路同 key 的播放。</summary>
    public static void PlayHeadphoneClip(AudioChannel channel, string key, AudioClip clip)
    {
        if (!WantsHeadphone(channel) || clip == null) return;
        PlayHeadphoneClipAsync(key, clip).Forget();
    }

    static async UniTaskVoid PlayHeadphoneClipAsync(string key, AudioClip clip)
    {
        if (string.IsNullOrEmpty(HeadphoneCard))
        {
            Debug.LogWarning($"[DualChannelAudio] 未识别开发板耳机声卡，跳过耳机口播放 key={key}");
            return;
        }

        try
        {
            await ExternalDeviceAudioPlayer.PlayFromClipAsync(HeadphoneCard, key, clip);
            ApplyCachedKeyVolume(key);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DualChannelAudio] 耳机口播放失败 key={key}: {e.Message}");
        }
    }

    /// <summary>耳机口播放一段已经合成好的完整 PCM(S16_LE)，例如 TTS 音色克隆的输出。</summary>
    public static void PlayHeadphonePcm(AudioChannel channel, string key, byte[] pcm, int sampleRate, int channels)
    {
        if (!WantsHeadphone(channel) || pcm == null || pcm.Length == 0) return;
        RunOnHeadphone(key, () =>
        {
            ExternalDeviceAudioPlayer.Play(HeadphoneCard, key, pcm, sampleRate, channels);
            ApplyCachedKeyVolume(key);
        });
    }

    /// <summary>停止耳机口指定 key 的播放（用于主通道被打断时保持两路一致）。</summary>
    public static void StopHeadphone(string key)
    {
        try { ExternalDeviceAudioPlayer.Stop(key); }
        catch (Exception e) { Debug.LogWarning($"[DualChannelAudio] 耳机口停止失败 key={key}: {e.Message}"); }
    }

    /// <summary>
    /// 设置耳机口指定 key 的音量（0~1），立即应用于当前播放中的该 key（若存在），
    /// 并记住此音量，供该 key 之后每次重新开播时自动重新应用。
    /// </summary>
    public static void SetHeadphoneKeyVolume(string key, float volume)
    {
        if (string.IsNullOrEmpty(key)) return;

        volume = Mathf.Clamp01(volume);
        s_HeadphoneKeyVolumes[key] = volume;
        ApplyCachedKeyVolume(key);
    }

    static void ApplyCachedKeyVolume(string key)
    {
        if (!s_HeadphoneKeyVolumes.TryGetValue(key, out var volume)) return;

        try { ExternalDeviceAudioPlayer.SetVoiceVolume(key, volume); }
        catch (Exception e) { Debug.LogWarning($"[DualChannelAudio] 耳机口设置音量失败 key={key}: {e.Message}"); }
    }

    static void RunOnHeadphone(string key, Action action)
    {
        if (string.IsNullOrEmpty(HeadphoneCard))
        {
            Debug.LogWarning($"[DualChannelAudio] 未识别开发板耳机声卡，跳过耳机口播放 key={key}");
            return;
        }

        try { action(); }
        catch (Exception e) { Debug.LogWarning($"[DualChannelAudio] 耳机口播放失败 key={key}: {e.Message}"); }
    }
}
