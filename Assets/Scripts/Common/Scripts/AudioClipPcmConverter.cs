using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// AudioClip → S16_LE PCM 异步转换，支持内存/磁盘缓存。
/// GetData 必须在主线程；float→S16 在线程池执行，避免大文件阻塞主线程（如 S3 倒计时期间）。
/// </summary>
public static class AudioClipPcmConverter
{
    public readonly struct PcmResult
    {
        public readonly byte[] Data;
        public readonly int SampleRate;
        public readonly int Channels;

        public PcmResult(byte[] data, int sampleRate, int channels)
        {
            Data = data;
            SampleRate = sampleRate;
            Channels = channels;
        }
    }

    const string DiskSubDir = "pcm_cache";
    const int DiskVersion = 1;
    static readonly byte[] Magic = { (byte)'P', (byte)'C', (byte)'1', (byte)'6' };

    static readonly object Gate = new object();
    static readonly Dictionary<string, PcmResult> MemoryCache = new Dictionary<string, PcmResult>();
    static readonly Dictionary<string, UniTaskCompletionSource<PcmResult>> Inflight =
        new Dictionary<string, UniTaskCompletionSource<PcmResult>>();

    /// <summary>某次转换完成（含缓存命中后的“就绪”不触发；仅新转换成功时）。cacheKey 可能为空。</summary>
    public static event Action<string, PcmResult> Converted;

    public static bool TryGetCached(string cacheKey, out PcmResult result)
    {
        result = default;
        if (string.IsNullOrEmpty(cacheKey)) return false;
        lock (Gate)
        {
            if (!MemoryCache.TryGetValue(cacheKey, out result)) return false;
            return result.Data != null && result.Data.Length > 0;
        }
    }

    /// <summary>内存或磁盘命中则返回 true，可直接 Play PCM，无需再加载 AudioClip。</summary>
    public static bool TryGetOrLoadCache(string cacheKey, out PcmResult result)
    {
        if (TryGetCached(cacheKey, out result)) return true;
        if (!IsFileCacheKey(cacheKey)) return false;
        if (!TryLoadDisk(cacheKey, out result)) return false;
        StoreMemory(cacheKey, result);
        return true;
    }

    /// <summary>
    /// 仅检查磁盘缓存是否<strong>有效</strong>并返回路径，不把 PCM 读进 C#（大文件请走 Java 侧 playFromPcm16File）。
    /// 头损坏或 pcmLen≤0 时删除坏文件并返回 false，触发重新转换。
    /// </summary>
    public static bool TryGetDiskCachePath(string cacheKey, out string path)
    {
        path = null;
        if (!IsFileCacheKey(cacheKey)) return false;
        try
        {
            path = DiskPathForKey(cacheKey);
            if (!File.Exists(path)) return false;
            if (!IsValidPcm16File(path))
            {
                Debug.LogWarning($"[AudioClipPcmConverter] 损坏的 PCM 缓存已删除: {Path.GetFileName(path)}");
                TryDeleteFile(path);
                path = null;
                return false;
            }
            return true;
        }
        catch
        {
            path = null;
            return false;
        }
    }

    /// <summary>删除指定缓存键对应的磁盘 PCM（音乐文件损坏重下时一并清理）。</summary>
    public static void InvalidateDiskCache(string cacheKey)
    {
        if (!IsFileCacheKey(cacheKey)) return;
        try
        {
            string path = DiskPathForKey(cacheKey);
            TryDeleteFile(path);
            lock (Gate) MemoryCache.Remove(cacheKey);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AudioClipPcmConverter] InvalidateDiskCache 失败: {e.Message}");
        }
    }

    /// <summary>按本地音乐绝对路径清理对应 PCM 缓存。</summary>
    public static void InvalidateDiskCacheForFile(string absoluteMusicPath)
    {
        if (string.IsNullOrEmpty(absoluteMusicPath)) return;
        InvalidateDiskCache(MakeFileCacheKey(absoluteMusicPath));
    }

    /// <summary>等待磁盘写入完成（大文件播放前确保 Java 能读到文件）。</summary>
    public static async UniTask EnsureDiskCacheAsync(string cacheKey, PcmResult result,
                                                     CancellationToken cancellationToken = default)
    {
        if (!IsFileCacheKey(cacheKey) || result.Data == null) return;
        if (TryGetDiskCachePath(cacheKey, out _)) return;
        await UniTask.RunOnThreadPool(() => TrySaveDisk(cacheKey, result), cancellationToken: cancellationToken);
    }

    public static void ClearMemoryCache()
    {
        lock (Gate) MemoryCache.Clear();
    }

    /// <summary>
    /// 用本地文件路径生成稳定缓存键（含长度与 mtime，文件更新后自动失效）。
    /// </summary>
    public static string MakeFileCacheKey(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return null;
        try
        {
            var fi = new FileInfo(absolutePath);
            if (!fi.Exists) return "file:" + absolutePath;
            return $"file:{absolutePath}|{fi.Length}|{fi.LastWriteTimeUtc.Ticks}";
        }
        catch
        {
            return "file:" + absolutePath;
        }
    }

    public static string MakeClipCacheKey(AudioClip clip)
    {
        if (clip == null) return null;
        return $"clip:{clip.GetInstanceID()}:{clip.samples}:{clip.frequency}:{clip.channels}";
    }

    /// <param name="clip">已加载的 AudioClip；磁盘缓存命中时可传 null（需提供 cacheKey）。</param>
    /// <param name="cacheKey">缓存键；文件播放请用 <see cref="MakeFileCacheKey"/>。</param>
    /// <param name="useCache">false 时强制重新转换且不写缓存。</param>
    public static async UniTask<PcmResult> ConvertAsync(
        AudioClip clip,
        string cacheKey = null,
        bool useCache = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(cacheKey) && clip != null)
            cacheKey = MakeClipCacheKey(clip);

        if (useCache && !string.IsNullOrEmpty(cacheKey) && TryGetCached(cacheKey, out var memHit))
            return memHit;

        bool diskEligible = useCache && IsFileCacheKey(cacheKey);
        if (diskEligible && TryLoadDisk(cacheKey, out var diskHit))
        {
            StoreMemory(cacheKey, diskHit);
            return diskHit;
        }

        if (clip == null)
            throw new ArgumentNullException(nameof(clip), "无缓存命中时必须提供 AudioClip");

        UniTaskCompletionSource<PcmResult> shared = null;
        bool owner = false;
        if (useCache && !string.IsNullOrEmpty(cacheKey))
        {
            lock (Gate)
            {
                if (Inflight.TryGetValue(cacheKey, out var existing))
                    shared = existing;
                else
                {
                    shared = new UniTaskCompletionSource<PcmResult>();
                    Inflight[cacheKey] = shared;
                    owner = true;
                }
            }

            if (!owner)
                return await shared.Task.AttachExternalCancellation(cancellationToken);
        }

        try
        {
            int sampleRate = clip.frequency;
            int channels = clip.channels;
            float[] samples = NativeAudio.GetClipSamples(clip);

            byte[] pcm = await UniTask.RunOnThreadPool(
                () => NativeAudio.FloatToPcm16(samples, 1f),
                cancellationToken: cancellationToken);

            if (pcm == null || pcm.Length == 0)
                throw new InvalidDataException($"PCM 转换结果为空 samples={samples?.Length ?? 0} clip={(clip != null ? clip.name : "null")}");

            var result = new PcmResult(pcm, sampleRate, channels);

            if (useCache && !string.IsNullOrEmpty(cacheKey))
            {
                // 大文件不进内存缓存，避免常驻数十 MB；磁盘缓存供 Java 侧异步播放
                if (pcm.Length < 1024 * 1024)
                    StoreMemory(cacheKey, result);
                if (diskEligible)
                    await UniTask.RunOnThreadPool(() => TrySaveDisk(cacheKey, result),
                        cancellationToken: cancellationToken);
            }

            try { Converted?.Invoke(cacheKey, result); }
            catch (Exception e) { Debug.LogWarning($"[AudioClipPcmConverter] Converted 回调异常: {e.Message}"); }

            shared?.TrySetResult(result);
            return result;
        }
        catch (Exception e)
        {
            shared?.TrySetException(e);
            throw;
        }
        finally
        {
            if (owner && !string.IsNullOrEmpty(cacheKey))
            {
                lock (Gate) Inflight.Remove(cacheKey);
            }
        }
    }

    static bool IsFileCacheKey(string cacheKey) =>
        !string.IsNullOrEmpty(cacheKey) && cacheKey.StartsWith("file:", StringComparison.Ordinal);

    static void StoreMemory(string cacheKey, PcmResult result)
    {
        if (string.IsNullOrEmpty(cacheKey) || result.Data == null) return;
        lock (Gate) MemoryCache[cacheKey] = result;
    }

    static string CacheDir()
    {
        string dir = Path.Combine(Application.persistentDataPath, DiskSubDir);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    static string DiskPathForKey(string cacheKey)
    {
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(cacheKey));
        var sb = new StringBuilder(hash.Length * 2);
        for (int i = 0; i < hash.Length; i++)
            sb.Append(hash[i].ToString("x2"));
        return Path.Combine(CacheDir(), sb + ".pcm16");
    }

    static bool TryLoadDisk(string cacheKey, out PcmResult result)
    {
        result = default;
        try
        {
            string path = DiskPathForKey(cacheKey);
            if (!File.Exists(path)) return false;
            if (!IsValidPcm16File(path))
            {
                TryDeleteFile(path);
                return false;
            }

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);
            br.ReadBytes(4); // magic，已由 IsValidPcm16File 校验
            br.ReadInt32();  // version
            int sampleRate = br.ReadInt32();
            int channels = br.ReadInt32();
            int pcmLen = br.ReadInt32();

            byte[] pcm = br.ReadBytes(pcmLen);
            if (pcm.Length != pcmLen) return false;

            result = new PcmResult(pcm, sampleRate, channels);
            Debug.Log($"[AudioClipPcmConverter] 磁盘缓存命中 keyHash={Path.GetFileName(path)} bytes={pcmLen}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AudioClipPcmConverter] 读磁盘缓存失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 校验 .pcm16 头：magic/version/pcmLen，且文件长度至少覆盖声明的 PCM。
    /// </summary>
    public static bool IsValidPcm16File(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            // magic(4)+ver(4)+rate(4)+ch(4)+len(4)=20
            if (!fi.Exists || fi.Length < 20) return false;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);
            byte[] magic = br.ReadBytes(4);
            if (magic.Length != 4 || magic[0] != Magic[0] || magic[1] != Magic[1] ||
                magic[2] != Magic[2] || magic[3] != Magic[3])
                return false;

            int version = br.ReadInt32();
            if (version != DiskVersion) return false;

            int sampleRate = br.ReadInt32();
            int channels = br.ReadInt32();
            int pcmLen = br.ReadInt32();
            if (sampleRate <= 0 || channels <= 0 || pcmLen <= 0) return false;
            if (fi.Length < 20L + pcmLen) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    static void TrySaveDisk(string cacheKey, PcmResult result)
    {
        if (result.Data == null || result.Data.Length == 0)
        {
            Debug.LogWarning("[AudioClipPcmConverter] 拒绝写入空 PCM 缓存");
            return;
        }

        try
        {
            string path = DiskPathForKey(cacheKey);
            string tmp = path + ".tmp";
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(Magic);
                bw.Write(DiskVersion);
                bw.Write(result.SampleRate);
                bw.Write(result.Channels);
                bw.Write(result.Data.Length);
                bw.Write(result.Data);
            }

            if (File.Exists(path))
                File.Delete(path);
            File.Move(tmp, path);

            if (!IsValidPcm16File(path))
            {
                Debug.LogWarning($"[AudioClipPcmConverter] 写入后校验失败，已删除: {Path.GetFileName(path)}");
                TryDeleteFile(path);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AudioClipPcmConverter] 写磁盘缓存失败: {e.Message}");
        }
    }

    static void TryDeleteFile(string path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                File.Delete(path);
        }
        catch { /* ignore */ }
    }
}
