using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LetsPiu.Base.Localization
{
    /// <summary>
    /// 解析 UI_L10N.csv（UTF-8 BOM、Tab 分隔、第一行为表头 ID/zh-CN/en-US/Remarks/...）。
    /// 解析结果：
    ///   - languages：所有语言列名（按列序，不含 ID/Remarks）。
    ///   - table[lang][id] = text；text 中字面 "\n" / "\t" / "\\" 已转义为真实控制字符。
    /// 容错：
    ///   - 空行跳过；ID 列空白跳过并 LogWarning（不打到运行时高频路径）。
    ///   - 行内字段数 &lt; 表头列数：缺失列视为空字符串。
    /// </summary>
    public static class LocalizationCsvParser
    {
        const string ID_HEADER = "ID";
        const string REMARKS_HEADER = "Remarks";

        public static void Parse(
            byte[] bytes,
            out List<string> languages,
            out Dictionary<string, Dictionary<string, string>> table)
        {
            languages = new List<string>();
            table = new Dictionary<string, Dictionary<string, string>>();

            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("[Localization] CSV 字节为空");
                return;
            }

            // 去 UTF-8 BOM
            int start = 0;
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                start = 3;

            string text = Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
            string[] lines = text.Split('\n');

            if (lines.Length == 0)
            {
                Debug.LogError("[Localization] CSV 内容为空");
                return;
            }

            string headerLine = StripLineEnding(lines[0]);
            string[] headers = headerLine.Split('\t');

            int idCol = -1;
            var langCols = new List<int>();
            var langNames = new List<string>();

            for (int i = 0; i < headers.Length; i++)
            {
                string col = headers[i].Trim();
                if (col == ID_HEADER) idCol = i;
                else if (col == REMARKS_HEADER) { /* 跳过：备注列不进字典 */ }
                else if (!string.IsNullOrEmpty(col))
                {
                    langCols.Add(i);
                    langNames.Add(col);
                }
            }

            if (idCol < 0)
            {
                Debug.LogError("[Localization] CSV 表头缺少 ID 列");
                return;
            }
            if (langNames.Count == 0)
            {
                Debug.LogError("[Localization] CSV 表头未识别到任何语言列（除 ID/Remarks 外）");
                return;
            }

            languages = langNames;
            foreach (var lang in langNames)
                table[lang] = new Dictionary<string, string>(256);

            for (int row = 1; row < lines.Length; row++)
            {
                string raw = StripLineEnding(lines[row]);
                if (raw.Length == 0) continue;

                string[] cols = raw.Split('\t');
                if (idCol >= cols.Length)
                {
                    Debug.LogWarning($"[Localization] CSV 第 {row + 1} 行字段数不足，跳过");
                    continue;
                }

                string id = cols[idCol].Trim();
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogWarning($"[Localization] CSV 第 {row + 1} 行 ID 列为空，跳过");
                    continue;
                }

                for (int i = 0; i < langCols.Count; i++)
                {
                    int colIdx = langCols[i];
                    string val = colIdx < cols.Length ? cols[colIdx] : string.Empty;
                    table[langNames[i]][id] = Unescape(val);
                }
            }
        }

        static string StripLineEnding(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s[s.Length - 1] == '\r') return s.Substring(0, s.Length - 1);
            return s;
        }

        /// <summary>
        /// 把 CSV 中字面 "\\n" / "\\t" / "\\\\" 还原为真实控制字符。
        /// 注：直接遍历比正则便宜得多，启动期跑一次即可。
        /// </summary>
        static string Unescape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.IndexOf('\\') < 0) return s;

            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[i + 1];
                    if (n == 'n') { sb.Append('\n'); i++; continue; }
                    if (n == 't') { sb.Append('\t'); i++; continue; }
                    if (n == '\\') { sb.Append('\\'); i++; continue; }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
