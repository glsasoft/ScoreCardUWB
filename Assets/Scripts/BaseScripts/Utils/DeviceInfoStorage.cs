using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Common.Utils
{
    /// <summary>
    /// 设备注册信息的外部存储。
    /// 写到 /sdcard/GLSA 下，可抗"清除数据 / 卸载重装"（不在 app 私有目录，只有恢复出厂会清）。
    /// 采用 临时文件 + 强制落盘(fsync) + 主备双写 来对抗开发板断电导致的文件损坏。
    /// </summary>
    public static class DeviceInfoStorage
    {
        private const string DirName = "GLSA";

        /// <summary>GLSA 外置存储根目录（Android 设备为 /sdcard/GLSA，Editor 为 persistentDataPath/GLSA）。</summary>
        public static string GlsaRootDirectory =>
#if UNITY_ANDROID && !UNITY_EDITOR
            "/sdcard/" + DirName;
#else
            Path.Combine(Application.persistentDataPath, DirName);
#endif

        private static string RootDir => GlsaRootDirectory;

        public static string MainPath => Path.Combine(RootDir, "device.json");
        public static string BackupPath => Path.Combine(RootDir, "device.bak.json");

        public static bool Write(string json)
        {
            try
            {
                Directory.CreateDirectory(RootDir);

                // 写新内容前，把当前主文件备份为"上一次的好数据"，以便新写入断电损坏时回退。
                if (File.Exists(MainPath))
                {
                    WriteAllTextFsync(BackupPath, File.ReadAllText(MainPath));
                }

                var tmp = MainPath + ".tmp";
                WriteAllTextFsync(tmp, json);

                if (File.Exists(MainPath))
                {
                    File.Delete(MainPath);
                }
                File.Move(tmp, MainPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceInfoStorage] 写入失败 path={MainPath} err={e}");
                return false;
            }
        }

        /// <summary>
        /// 依次尝试主文件、备份文件，返回第一个通过 isValid 校验的原始 JSON；都不可用返回空串。
        /// </summary>
        public static string ReadValid(Func<string, bool> isValid)
        {
            foreach (var path in new[] { MainPath, BackupPath })
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    var raw = File.ReadAllText(path);
                    if (!string.IsNullOrEmpty(raw) && isValid(raw))
                    {
                        return raw;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DeviceInfoStorage] 读取失败 path={path} err={e}");
                }
            }

            return "";
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(MainPath))
                {
                    File.Delete(MainPath);
                }
                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceInfoStorage] 删除失败 err={e}");
            }
        }

        private static void WriteAllTextFsync(string path, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush(true);
        }
    }
}
