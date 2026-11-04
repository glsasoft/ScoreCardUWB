using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 从腾讯云桶拉取视频清单，把介绍片和首页广告下载到 persistentDataPath。
/// 清单格式为每行 id=文件名。同一个 id 的文件名变了就重新下载。
/// 本地文件大小必须和桶上的 Content-Length 一致，空文件或大小不对会删掉重下。
/// </summary>
public static class RemoteVideoStore
{
    public const string ManifestUrl =
        "https://scordcard-1305354732.cos.ap-shanghai.myqcloud.com/qmlz/video/manifest.txt";

    public const string IdAdHome = "ad.home";
    public const string IdIntroScore = "intro.score";
    public const string IdIntroAnnihilation = "intro.annihilation";
    public const string IdIntroFire = "intro.fire";
    public const string IdIntroDefense = "intro.defense";
    public const string IdIntroSuperCaptain = "intro.super_captain";
    public const string IdIntroZombie = "intro.zombie";
    public const string IdIntroDevice = "intro.device";
    public const string IdIntroSpeedShoot = "intro.speed_shoot";

    static readonly string[] RequiredIds =
    {
        IdAdHome,
        IdIntroScore,
        IdIntroAnnihilation,
        IdIntroFire,
        IdIntroDefense,
        IdIntroSuperCaptain,
        IdIntroZombie,
    };

    const string IndexFileName = "installed.txt";

    static Dictionary<string, string> s_Installed;
    static bool s_EntryBlocked;

    public static bool EntryBlocked => s_EntryBlocked;

    /// <summary>onProgress 参数为总进度 0~1 和界面提示文字。</summary>
    public static async UniTask<bool> EnsureReady(Action<float, string> onProgress)
    {
        if (s_EntryBlocked)
        {
            Debug.LogError("[RemoteVideoStore] 视频下载已失败，请重新打开应用");
            return false;
        }

        bool ok = await DownloadAll(onProgress);
        if (!ok)
        {
            s_EntryBlocked = true;
            Debug.LogError("[RemoteVideoStore] 视频下载失败，已禁止进入应用");
        }

        return ok;
    }

    static async UniTask<bool> DownloadAll(Action<float, string> onProgress)
    {
        onProgress?.Invoke(0f, "正在获取视频清单...");

        string manifestText = await DownloadText(ManifestUrl);
        if (string.IsNullOrEmpty(manifestText))
        {
            Debug.LogError("[RemoteVideoStore] 拉取视频清单失败");
            return false;
        }

        var entries = ParseManifest(manifestText);
        if (entries.Count == 0)
        {
            Debug.LogError("[RemoteVideoStore] 视频清单为空");
            return false;
        }

        for (int i = 0; i < RequiredIds.Length; i++)
        {
            if (!entries.ContainsKey(RequiredIds[i]))
            {
                Debug.LogError($"[RemoteVideoStore] 清单缺少 {RequiredIds[i]}");
                return false;
            }
        }

        LoadInstalled();
        Directory.CreateDirectory(CacheDir);
        string baseUrl = ManifestUrl.Substring(0, ManifestUrl.LastIndexOf('/') + 1);

        var ids = new List<string>(entries.Keys);
        var weights = new long[ids.Count];
        var remoteSizes = new long[ids.Count];
        long total = 0;
        for (int i = 0; i < ids.Count; i++)
        {
            string fileName = entries[ids[i]];
            string dst = Path.Combine(CacheDir, fileName);
            onProgress?.Invoke(0f, $"正在检查视频资源 {i + 1}/{ids.Count}");
            long remoteSize = await QuerySize(baseUrl + EscapeFileName(fileName));
            long localSize = LocalFileSize(dst);
            bool sameName = s_Installed.TryGetValue(ids[i], out string installedName) && installedName == fileName;
            bool cached = sameName && IsComplete(localSize, remoteSize);
            if (sameName && localSize > 0 && remoteSize > 0 && localSize != remoteSize)
            {
                Debug.LogWarning($"[RemoteVideoStore] {fileName} 大小不符（本地 {localSize}，桶上 {remoteSize}），重新下载");
            }

            remoteSizes[i] = remoteSize;
            long weight = cached ? localSize : (remoteSize > 0 ? remoteSize : 1);
            weights[i] = weight;
            total += weight;
        }

        long done = 0;
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            string fileName = entries[id];
            long weight = weights[i];
            long remoteSize = remoteSizes[i];
            int index = i + 1;
            if (!await EnsureOne(id, fileName, baseUrl, remoteSize, p =>
                {
                    long got = done + (long)(p * weight);
                    float overall = Mathf.Clamp01(got / (float)total);
                    onProgress?.Invoke(overall,
                        $"正在下载视频 {index}/{ids.Count}  {FormatSize(got)} / {FormatSize(total)}  {overall * 100f:F0}%");
                }))
            {
                return false;
            }

            done += weight;
        }

        RemoveUnlisted(entries);
        onProgress?.Invoke(1f, "视频资源已就绪");
        Debug.Log($"[RemoteVideoStore] 视频就绪，共 {entries.Count} 个");
        return true;
    }

    public static string GetLocalPath(string id)
    {
        LoadInstalled();
        if (s_Installed == null || !s_Installed.TryGetValue(id, out string fileName))
        {
            Debug.LogError($"[RemoteVideoStore] 本地没有视频 {id}");
            return null;
        }

        string path = Path.Combine(CacheDir, fileName);
        if (LocalFileSize(path) <= 0)
        {
            Debug.LogError($"[RemoteVideoStore] 视频文件缺失或为空 {path}");
            return null;
        }

        return path.Replace('\\', '/');
    }

    /// <summary>编辑器 VideoPlayer 用的地址。已下载视频是绝对路径，包内视频仍相对 StreamingAssets。</summary>
    public static string ToEditorVideoUrl(string path)
    {
        if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path)) return path;
        return Path.Combine(Application.streamingAssetsPath, path).Replace('\\', '/');
    }

    static async UniTask<bool> EnsureOne(string id, string fileName, string baseUrl, long remoteSize, Action<float> onProgress)
    {
        string dst = Path.Combine(CacheDir, fileName);
        s_Installed.TryGetValue(id, out string installedName);
        if (installedName == fileName && IsComplete(LocalFileSize(dst), remoteSize))
        {
            onProgress?.Invoke(1f);
            return true;
        }

        string tmp = dst + ".tmp";
        try
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RemoteVideoStore] 清理临时文件失败：{tmp} ({ex.Message})");
            return false;
        }

        string url = baseUrl + EscapeFileName(fileName);
        Debug.Log($"[RemoteVideoStore] 开始下载 {id} -> {fileName}");
        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {
            req.downloadHandler = new DownloadHandlerFile(tmp);
            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                onProgress?.Invoke(FileProgress(req));
                await UniTask.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RemoteVideoStore] 下载失败：{url} ({req.error})");
                TryDelete(tmp);
                return false;
            }

            if (remoteSize <= 0)
            {
                long.TryParse(req.GetResponseHeader("Content-Length"), out remoteSize);
            }
        }

        try
        {
            if (File.Exists(dst)) File.Delete(dst);
            File.Move(tmp, dst);
            long written = LocalFileSize(dst);
            if (!IsComplete(written, remoteSize))
            {
                Debug.LogError($"[RemoteVideoStore] 下载结果无效：{fileName}（本地 {written}，期望 {remoteSize}）");
                TryDelete(dst);
                TryDelete(tmp);
                return false;
            }
            if (!string.IsNullOrEmpty(installedName) && installedName != fileName)
            {
                TryDelete(Path.Combine(CacheDir, installedName));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RemoteVideoStore] 保存失败：{dst} ({ex.Message})");
            TryDelete(tmp);
            return false;
        }

        s_Installed[id] = fileName;
        SaveInstalled();
        Debug.Log($"[RemoteVideoStore] 已下载 {id} -> {fileName}");
        onProgress?.Invoke(1f);
        return true;
    }

    static string EscapeFileName(string fileName)
    {
        return UnityWebRequest.EscapeURL(fileName).Replace("+", "%20");
    }

    static float FileProgress(UnityWebRequest req)
    {
        string len = req.GetResponseHeader("Content-Length");
        if (long.TryParse(len, out long total) && total > 0)
        {
            return Mathf.Clamp01((float)(req.downloadedBytes / (double)total));
        }

        return Mathf.Clamp01(req.downloadProgress);
    }

    static async UniTask<long> QuerySize(string url)
    {
        using var req = UnityWebRequest.Head(url);
        await req.SendWebRequest().ToUniTask();
        if (req.result != UnityWebRequest.Result.Success) return -1;
        if (long.TryParse(req.GetResponseHeader("Content-Length"), out long size) && size > 0)
        {
            return size;
        }

        return -1;
    }

    static async UniTask<string> DownloadText(string url)
    {
        using var req = UnityWebRequest.Get(url);
        await req.SendWebRequest().ToUniTask();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[RemoteVideoStore] 请求失败：{url} ({req.error})");
            return null;
        }

        return req.downloadHandler.text;
    }

    static Dictionary<string, string> ParseManifest(string text)
    {
        var entries = new Dictionary<string, string>();
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line[0] == '#') continue;

            int eq = line.IndexOf('=');
            if (eq <= 0) continue;

            string id = line.Substring(0, eq).Trim();
            string fileName = Path.GetFileName(line.Substring(eq + 1).Trim());
            if (id.Length == 0 || string.IsNullOrEmpty(fileName) || fileName.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                continue;
            }

            entries[id] = fileName;
        }

        return entries;
    }

    static void RemoveUnlisted(Dictionary<string, string> entries)
    {
        var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in entries)
        {
            keep.Add(pair.Value);
        }

        string[] files;
        try
        {
            files = Directory.GetFiles(CacheDir);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteVideoStore] 清理旧视频失败：{ex.Message}");
            return;
        }

        for (int i = 0; i < files.Length; i++)
        {
            string name = Path.GetFileName(files[i]);
            if (name == IndexFileName || keep.Contains(name)) continue;
            TryDelete(files[i]);
        }
    }

    static void LoadInstalled()
    {
        if (s_Installed != null) return;

        s_Installed = new Dictionary<string, string>();
        string path = Path.Combine(CacheDir, IndexFileName);
        if (!File.Exists(path)) return;

        string[] lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteVideoStore] 读取本地清单失败：{ex.Message}");
            return;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            int eq = lines[i].IndexOf('=');
            if (eq <= 0) continue;
            s_Installed[lines[i].Substring(0, eq).Trim()] = lines[i].Substring(eq + 1).Trim();
        }
    }

    static void SaveInstalled()
    {
        var sb = new StringBuilder();
        foreach (var pair in s_Installed)
        {
            sb.Append(pair.Key).Append('=').Append(pair.Value).Append('\n');
        }

        File.WriteAllText(Path.Combine(CacheDir, IndexFileName), sb.ToString());
    }

    static string CacheDir => Path.Combine(Application.persistentDataPath, "remote_video");

    static string FormatSize(long bytes)
    {
        return $"{bytes / 1048576f:F1} MB";
    }

    static long LocalFileSize(string path)
    {
        if (!File.Exists(path)) return 0;
        try
        {
            return new FileInfo(path).Length;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    static bool IsComplete(long localSize, long remoteSize)
    {
        if (localSize <= 0) return false;
        if (remoteSize <= 0) return true;
        return localSize == remoteSize;
    }

    static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RemoteVideoStore] 删除失败：{path} ({ex.Message})");
        }
    }
}
