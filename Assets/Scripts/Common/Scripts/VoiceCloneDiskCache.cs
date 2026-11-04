using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Utils;
using UnityEngine;

/// <summary>
/// 一句话复刻合成结果磁盘缓存（WAV / 16bit PCM / 24k 单声道）。
/// 内存索引 O(1) 查找 + LRU 淘汰；读写均在后台线程，主线程不阻塞。
/// </summary>
public sealed class VoiceCloneDiskCache
{
    public const int DefaultMaxEntries = 10000;
    const int IndexVersion = 1;
    const int FlushEveryStores = 20;

    [Serializable]
    class IndexFile
    {
        public int version = IndexVersion;
        public Entry[] entries = Array.Empty<Entry>();
    }

    [Serializable]
    class Entry
    {
        public string key;
        public string file;
        public int bytes;
        public long lastAccess;
    }

    readonly object _lock = new();
    readonly Dictionary<string, Entry> _index = new();
    readonly LinkedList<string> _lru = new();

    string _rootDir;
    int _maxEntries;
    long _maxTotalBytes;
    long _totalBytes;
    int _storesSinceFlush;
    bool _indexDirty;
    volatile bool _initialized;

    /// <summary>磁盘缓存根目录（Android: /sdcard/GLSA/VoiceCloneCache）。</summary>
    public string RootDirectory => _rootDir;

    public int Count
    {
        get { lock (_lock) { return _index.Count; } }
    }

    public long TotalBytes => Interlocked.Read(ref _totalBytes);

    /// <param name="maxEntries">条数上限，默认 10000</param>
    /// <param name="maxTotalBytes">磁盘总大小上限（字节），0 表示不限制</param>
    public void Initialize(int maxEntries = DefaultMaxEntries, long maxTotalBytes = 500L * 1024 * 1024)
    {
        if (_initialized) return;

        _maxEntries = Math.Max(1, maxEntries);
        _maxTotalBytes = maxTotalBytes;
        _rootDir = Path.Combine(DeviceInfoStorage.GlsaRootDirectory, "VoiceCloneCache");
        Directory.CreateDirectory(_rootDir);

        LoadIndex();
        TryMigrateLegacyCache();
        _initialized = true;
        Debug.Log($"[VoiceClone][DISK] 初始化完成 dir={_rootDir} entries={Count} bytes={TotalBytes}");
    }

    public bool Contains(string cacheKey)
    {
        if (!_initialized || string.IsNullOrEmpty(cacheKey)) return false;
        lock (_lock) { return _index.ContainsKey(cacheKey); }
    }

    /// <summary>后台读取 WAV 并返回 PCM16 字节；未命中或损坏返回 null。</summary>
    public async Task<byte[]> TryLoadPcmAsync(string cacheKey, CancellationToken ct = default)
    {
        if (!_initialized || string.IsNullOrEmpty(cacheKey)) return null;

        string path;
        lock (_lock)
        {
            if (!_index.TryGetValue(cacheKey, out var entry)) return null;
            TouchLocked(cacheKey, entry);
            path = Path.Combine(_rootDir, entry.file);
        }

        try
        {
            byte[] wav = await Task.Run(() => File.ReadAllBytes(path), ct);
            if (ct.IsCancellationRequested) return null;

            if (!TryExtractPcm16FromWav(wav, out byte[] pcm) || pcm.Length < 2)
            {
                Debug.LogWarning($"[VoiceClone][DISK] 读取失败/格式无效，移除: {cacheKey}");
                RemoveEntry(cacheKey, deleteFile: true);
                return null;
            }

            Debug.Log($"[VoiceClone][DISK] hit key={PreviewKey(cacheKey)} pcm={pcm.Length}B");
            return pcm;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[VoiceClone][DISK] 读取异常 key={PreviewKey(cacheKey)} err={ex.Message}");
            RemoveEntry(cacheKey, deleteFile: true);
            return null;
        }
    }

    /// <summary>异步写入 WAV；主线程调用后立即返回。</summary>
    public void StoreAsync(string cacheKey, byte[] pcm16, int sampleRate)
    {
        if (!_initialized || string.IsNullOrEmpty(cacheKey) || pcm16 == null || pcm16.Length < 2) return;

        Task.Run(() => StoreWorker(cacheKey, pcm16, sampleRate));
    }

    public void FlushIndex()
    {
        if (!_initialized) return;
        lock (_lock)
        {
            if (!_indexDirty) return;
            WriteIndexLocked();
            _indexDirty = false;
            _storesSinceFlush = 0;
        }
    }

    public void ClearAll()
    {
        if (!_initialized) return;
        lock (_lock)
        {
            foreach (var e in _index.Values)
            {
                TryDeleteFile(e.file);
            }
            _index.Clear();
            _lru.Clear();
            _totalBytes = 0;
            _indexDirty = true;
            WriteIndexLocked();
            _indexDirty = false;
        }
        Debug.Log("[VoiceClone][DISK] 已清空全部磁盘缓存");
    }

    void StoreWorker(string cacheKey, byte[] pcm16, int sampleRate)
    {
        string fileName = KeyToFileName(cacheKey);
        string fullPath = Path.Combine(_rootDir, fileName);
        List<string> evictFiles = null;

        try
        {
            WritePcm16Wav(fullPath, pcm16, sampleRate);

            lock (_lock)
            {
                if (_index.TryGetValue(cacheKey, out var old))
                {
                    if (!string.Equals(old.file, fileName, StringComparison.Ordinal))
                        TryDeleteFile(old.file);
                    _totalBytes -= old.bytes;
                    _lru.Remove(cacheKey);
                }

                var entry = new Entry
                {
                    key = cacheKey,
                    file = fileName,
                    bytes = pcm16.Length,
                    lastAccess = NowUnix()
                };
                _index[cacheKey] = entry;
                _lru.AddLast(cacheKey);
                _totalBytes += pcm16.Length;

                evictFiles = EvictOverflowLocked();
                _indexDirty = true;
                _storesSinceFlush++;
                if (_storesSinceFlush >= FlushEveryStores)
                {
                    WriteIndexLocked();
                    _indexDirty = false;
                    _storesSinceFlush = 0;
                }
            }

            if (evictFiles != null)
            {
                foreach (var f in evictFiles) TryDeleteFile(f);
            }

            Debug.Log($"[VoiceClone][DISK] store key={PreviewKey(cacheKey)} pcm={pcm16.Length}B total={Count}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[VoiceClone][DISK] 写入失败 key={PreviewKey(cacheKey)} err={ex.Message}");
        }
    }

    List<string> EvictOverflowLocked()
    {
        var deleted = new List<string>();
        while (_index.Count > _maxEntries || (_maxTotalBytes > 0 && _totalBytes > _maxTotalBytes))
        {
            string evictKey = FindOldestEvictableKeyLocked();
            if (evictKey == null) break;

            if (_index.TryGetValue(evictKey, out var old))
            {
                deleted.Add(old.file);
                _totalBytes -= old.bytes;
            }
            _lru.Remove(evictKey);
            _index.Remove(evictKey);
        }
        return deleted;
    }

    string FindOldestEvictableKeyLocked()
    {
        for (var node = _lru.First; node != null; node = node.Next)
        {
            if (VoiceCloneDiskCache.IsEvictablePlayerVoice(node.Value))
                return node.Value;
        }
        return null;
    }

    void RemoveEntry(string cacheKey, bool deleteFile)
    {
        lock (_lock)
        {
            if (!_index.TryGetValue(cacheKey, out var entry)) return;
            _index.Remove(cacheKey);
            _lru.Remove(cacheKey);
            _totalBytes -= entry.bytes;
            _indexDirty = true;
            if (deleteFile) TryDeleteFile(entry.file);
        }
    }

    void TouchLocked(string cacheKey, Entry entry)
    {
        entry.lastAccess = NowUnix();
        _lru.Remove(cacheKey);
        _lru.AddLast(cacheKey);
    }

    void LoadIndex()
    {
        string indexPath = Path.Combine(_rootDir, "index.json");
        _index.Clear();
        _lru.Clear();
        _totalBytes = 0;

        if (!File.Exists(indexPath))
        {
            WriteIndexLocked();
            return;
        }

        try
        {
            string json = File.ReadAllText(indexPath);
            var data = JsonUtility.FromJson<IndexFile>(json);
            if (data?.entries == null) return;

            Array.Sort(data.entries, (a, b) => a.lastAccess.CompareTo(b.lastAccess));
            foreach (var e in data.entries)
            {
                if (string.IsNullOrEmpty(e.key) || string.IsNullOrEmpty(e.file)) continue;
                string path = Path.Combine(_rootDir, e.file);
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[VoiceClone][DISK] 索引文件缺失，跳过: {e.file}");
                    continue;
                }

                if (e.bytes <= 0)
                {
                    try { e.bytes = (int)Math.Max(0, new FileInfo(path).Length - 44); }
                    catch { continue; }
                }

                _index[e.key] = e;
                _lru.AddLast(e.key);
                _totalBytes += e.bytes;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[VoiceClone][DISK] 索引加载失败，重建: {ex.Message}");
            _index.Clear();
            _lru.Clear();
            _totalBytes = 0;
            WriteIndexLocked();
        }
    }

    /// <summary>从 app 私有目录迁移旧缓存到外置存储（仅首次、且外置为空时）。</summary>
    void TryMigrateLegacyCache()
    {
        if (Count > 0) return;

        string legacyDir = Path.Combine(Application.persistentDataPath, "VoiceCloneCache");
        if (string.Equals(legacyDir, _rootDir, StringComparison.OrdinalIgnoreCase)) return;
        if (!File.Exists(Path.Combine(legacyDir, "index.json"))) return;

        Debug.Log($"[VoiceClone][DISK] 检测到旧缓存，迁移至外置存储 legacy={legacyDir}");

        try
        {
            foreach (var file in Directory.GetFiles(legacyDir))
            {
                string dest = Path.Combine(_rootDir, Path.GetFileName(file));
                if (File.Exists(dest)) continue;
                File.Copy(file, dest);
            }

            LoadIndex();
            Debug.Log($"[VoiceClone][DISK] 迁移完成 entries={Count} bytes={TotalBytes}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[VoiceClone][DISK] 迁移失败: {ex.Message}");
        }
    }

    void WriteIndexLocked()
    {
        var entries = new Entry[_index.Count];
        int i = 0;
        foreach (var kv in _index) entries[i++] = kv.Value;

        var data = new IndexFile { version = IndexVersion, entries = entries };
        string json = JsonUtility.ToJson(data, prettyPrint: false);
        string indexPath = Path.Combine(_rootDir, "index.json");
        File.WriteAllText(indexPath, json);
    }

    void TryDeleteFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        try
        {
            string path = Path.Combine(_rootDir, fileName);
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }

    static string KeyToFileName(string cacheKey)
    {
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(cacheKey));
        var sb = new StringBuilder(16);
        for (int i = 0; i < 8; i++) sb.Append(hash[i].ToString("x2"));
        return sb + ".wav";
    }

    static string PreviewKey(string key) =>
        key.Length <= 48 ? key : key.Substring(0, 48) + "…";

    /// <summary>
    /// 系统内置显示名（战士/玩家+编号），战场 TTS 缓存不参与 LRU 淘汰。
    /// </summary>
    public static bool IsSystemBuiltinDisplayName(string displayName)
    {
        if (string.IsNullOrEmpty(displayName)) return false;
        return MatchPrefixDigitsName(displayName, "战士") || MatchPrefixDigitsName(displayName, "玩家");
    }

    /// <summary>
    /// 是否可被 LRU 淘汰：仅玩家自定义昵称相关条目；固定播报与系统内置名默认钉选。
    /// 新 key 格式：{0|1}|resId|Vcn|lang|style|impact|text
    /// </summary>
    public static bool IsEvictablePlayerVoice(string cacheKey)
    {
        if (string.IsNullOrEmpty(cacheKey)) return false;

        if (cacheKey.Length >= 2 && cacheKey[1] == '|' && (cacheKey[0] == '0' || cacheKey[0] == '1'))
            return cacheKey[0] == '1';

        // 旧版 key（无 policy 前缀）：连杀 customName+「已完成」仍按启发式淘汰
        if (!TryExtractSynthTextLegacy(cacheKey, out string text)) return false;
        return IsLegacyEvictableCustomPlayerTts(text);
    }

    static bool MatchPrefixDigitsName(string name, string prefix)
    {
        if (!name.StartsWith(prefix)) return false;
        int i = prefix.Length;
        while (i < name.Length && char.IsDigit(name[i])) i++;
        return i > prefix.Length && i == name.Length;
    }

    static bool TryExtractSynthTextLegacy(string cacheKey, out string text)
    {
        text = null;
        if (string.IsNullOrEmpty(cacheKey)) return false;

        int pipes = 0;
        for (int i = 0; i < cacheKey.Length; i++)
        {
            if (cacheKey[i] != '|') continue;
            pipes++;
            if (pipes == 5)
            {
                text = cacheKey.Substring(i + 1);
                return text.Length > 0;
            }
        }

        return false;
    }

    const string MultiKillSuffix = "已完成";

    static bool IsLegacyEvictableCustomPlayerTts(string text)
    {
        if (string.IsNullOrEmpty(text) || !text.EndsWith(MultiKillSuffix, StringComparison.Ordinal))
            return false;

        string name = text.Substring(0, text.Length - MultiKillSuffix.Length);
        return !string.IsNullOrEmpty(name) && !IsSystemBuiltinDisplayName(name);
    }

    static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    static void WritePcm16Wav(string path, byte[] pcm16, int sampleRate)
    {
        int dataBytes = pcm16.Length;
        int byteRate = sampleRate * 2;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var w = new BinaryWriter(fs);

        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataBytes);
        w.Write(Encoding.ASCII.GetBytes("WAVE"));
        w.Write(Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);
        w.Write((short)1);
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)2);
        w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(dataBytes);
        w.Write(pcm16);
    }

    static bool TryExtractPcm16FromWav(byte[] wav, out byte[] pcm16)
    {
        pcm16 = null;
        if (wav == null || wav.Length < 44) return false;
        if (Encoding.ASCII.GetString(wav, 0, 4) != "RIFF") return false;
        if (Encoding.ASCII.GetString(wav, 8, 4) != "WAVE") return false;

        int pos = 12;
        while (pos + 8 <= wav.Length)
        {
            string chunkId = Encoding.ASCII.GetString(wav, pos, 4);
            int chunkSize = BitConverter.ToInt32(wav, pos + 4);
            pos += 8;
            if (pos + chunkSize > wav.Length) break;

            if (chunkId == "data")
            {
                pcm16 = new byte[chunkSize];
                Buffer.BlockCopy(wav, pos, pcm16, 0, chunkSize);
                return true;
            }

            pos += chunkSize + (chunkSize & 1);
        }

        return false;
    }
}
