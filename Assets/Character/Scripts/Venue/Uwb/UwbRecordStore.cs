using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 真机按日会话文件：uwb_yyyyMMdd.jsonl（同日追加）。
    /// </summary>
    public static class UwbRecordStore
    {
        static readonly Regex DayFile = new Regex(@"^uwb_(\d{8})(?:_(\d{6}))?\.jsonl$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string DefaultDirectory =>
            Path.Combine(Application.persistentDataPath, "uwb_records");

        public static string DailyFilePath(string directory, DateTime localDay)
        {
            if (string.IsNullOrEmpty(directory))
                directory = DefaultDirectory;
            return Path.Combine(directory, "uwb_" + localDay.ToString("yyyyMMdd") + ".jsonl");
        }

        public static string TodayFilePath(string directory)
        {
            return DailyFilePath(directory, DateTime.Now.Date);
        }

        /// <summary>
        /// 返回有录制文件的日期列表，新→旧。元素为 yyyyMMdd。
        /// </summary>
        public static List<string> ListDates(string directory)
        {
            var set = new SortedSet<string>(Comparer<string>.Create((a, b) => string.CompareOrdinal(b, a)));
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return new List<string>();

            foreach (string path in Directory.GetFiles(directory, "uwb_*.jsonl"))
            {
                string name = Path.GetFileName(path);
                Match m = DayFile.Match(name ?? "");
                if (m.Success)
                    set.Add(m.Groups[1].Value);
            }

            return new List<string>(set);
        }

        /// <summary>
        /// 某日相关文件（含旧版带时分秒的文件），新→旧。
        /// </summary>
        public static List<UwbRecordFileInfo> ListFilesForDate(string directory, string yyyyMMdd)
        {
            var list = new List<UwbRecordFileInfo>();
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory) || string.IsNullOrEmpty(yyyyMMdd))
                return list;

            foreach (string path in Directory.GetFiles(directory, "uwb_" + yyyyMMdd + "*.jsonl"))
            {
                var fi = new FileInfo(path);
                list.Add(new UwbRecordFileInfo
                {
                    path = path,
                    fileName = fi.Name,
                    dateKey = yyyyMMdd,
                    sizeBytes = fi.Length,
                    lastWrite = fi.LastWriteTime
                });
            }

            list.Sort((a, b) => b.lastWrite.CompareTo(a.lastWrite));
            return list;
        }

        public static bool TryProbeTimeRange(string path, out double minT, out double maxT, out int lineCount)
        {
            minT = 0;
            maxT = 0;
            lineCount = 0;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            bool any = false;
            try
            {
                foreach (string line in File.ReadLines(path, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    try
                    {
                        JObject obj = JObject.Parse(line);
                        double? t = obj.Value<double?>("t");
                        if (!t.HasValue)
                            continue;
                        if (!any)
                        {
                            minT = maxT = t.Value;
                            any = true;
                        }
                        else
                        {
                            if (t.Value < minT) minT = t.Value;
                            if (t.Value > maxT) maxT = t.Value;
                        }

                        lineCount++;
                    }
                    catch
                    {
                        // skip
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[UwbRecordStore] probe fail: " + e.Message);
                return false;
            }

            return any;
        }

        public static string FormatDuration(double seconds)
        {
            if (seconds < 0) seconds = 0;
            int s = (int)Math.Floor(seconds);
            int h = s / 3600;
            int m = (s % 3600) / 60;
            int r = s % 60;
            if (h > 0)
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}", h, m, r);
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}", m, r);
        }
    }

    [Serializable]
    public class UwbRecordFileInfo
    {
        public string path;
        public string fileName;
        public string dateKey;
        public long sizeBytes;
        public DateTime lastWrite;
    }
}
