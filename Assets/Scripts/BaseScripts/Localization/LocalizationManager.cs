using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define;
using UnityEngine;
using YooAsset;

namespace LetsPiu.Base.Localization
{
    /// <summary>
    /// 多国语言运行时单例。
    ///
    /// 加载时机：由 <c>TableManager.LoadTable()</c> 末尾调用 <see cref="LoadAsync"/>。
    /// 运行时入口：业务代码 <see cref="GetText(string)"/> / <see cref="GetText(string, object[])"/>；
    ///             UI 上挂 <c>LocalizedText</c> 组件订阅 <see cref="OnLanguageChanged"/>。
    /// 切语言：UIDevPanel 按钮、未来 MQTT 消息均调 <see cref="SetLanguage"/>，
    ///         触发 <see cref="OnLanguageChanged"/>，全屏 LocalizedText 自动刷新。
    ///
    /// 找不到 ID 的降级链：当前语言 → zh-CN → "[id]"（屏幕上一眼可见漏配）。
    /// </summary>
    public class LocalizationManager
    {
        const string CSV_PATH = "Assets/TableConfigRes/Localization/UI_L10N.csv";

        public static LocalizationManager S { get; } = new LocalizationManager();

        readonly List<string> m_Languages = new List<string>();
        Dictionary<string, Dictionary<string, string>> m_Table = new Dictionary<string, Dictionary<string, string>>();
        string m_CurrentLanguage = LocalizationLanguage.DEFAULT;
        bool m_Loaded;

        /// <summary>当前生效语言代码（BCP 47，如 "zh-CN" / "en-US"）。</summary>
        public string CurrentLanguage => m_CurrentLanguage;

        /// <summary>CSV 表头中识别到的全部语言列名（不含 ID/Remarks）。</summary>
        public IReadOnlyList<string> SupportedLanguages => m_Languages;

        /// <summary>CSV 是否已成功加载。未加载时 <see cref="GetText(string)"/> 仅返回 "[id]"。</summary>
        public bool IsLoaded => m_Loaded;

        /// <summary>语言切换时触发；UI 组件应订阅此事件刷新文本。</summary>
        public event Action OnLanguageChanged;

        LocalizationManager() { }

        /// <summary>
        /// 启动期加载 CSV。由 TableManager.LoadTable() 末尾 await 调用一次。
        /// </summary>
        public async UniTask LoadAsync()
        {
            var package = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            if (package == null)
            {
                Debug.LogError("[Localization] DefaultPackage 未就绪，CSV 加载失败");
                return;
            }
            if (!package.CheckLocationValid(CSV_PATH))
            {
                Debug.LogError($"[Localization] CSV 路径无效：{CSV_PATH}");
                return;
            }

            var handle = package.LoadAssetAsync<TextAsset>(CSV_PATH);
            await handle.ToUniTask();
            var asset = handle.AssetObject as TextAsset;
            byte[] bytes = asset != null ? asset.bytes : null;
            handle.Dispose();

            LocalizationCsvParser.Parse(bytes, out var langs, out var table);
            m_Languages.Clear();
            m_Languages.AddRange(langs);
            m_Table = table;
            m_Loaded = true;

            // 初始化 CurrentLanguage：PlayerPrefs → CSV 第一列 → DEFAULT
            string saved = PlayerPrefs.GetString(LocalizationLanguage.PLAYER_PREFS_KEY, null);
            if (!string.IsNullOrEmpty(saved) && m_Table.ContainsKey(saved))
            {
                m_CurrentLanguage = saved;
            }
            else if (m_Languages.Count > 0 && m_Table.ContainsKey(LocalizationLanguage.DEFAULT))
            {
                m_CurrentLanguage = LocalizationLanguage.DEFAULT;
            }
            else if (m_Languages.Count > 0)
            {
                m_CurrentLanguage = m_Languages[0];
            }
        }

        /// <summary>Editor / 工具用：从磁盘重新解析 CSV。运行时不要走这条路径。</summary>
        public void ReloadFromDisk()
        {
            LoadAsync().Forget();
        }

        /// <summary>取译文，无参形式。</summary>
        public string GetText(string id)
        {
            if (string.IsNullOrEmpty(id)) return string.Empty;

            if (m_Loaded)
            {
                if (m_Table.TryGetValue(m_CurrentLanguage, out var langDict)
                    && langDict.TryGetValue(id, out var hit)
                    && !string.IsNullOrEmpty(hit))
                {
                    return hit;
                }
                if (m_CurrentLanguage != LocalizationLanguage.ZH_CN
                    && m_Table.TryGetValue(LocalizationLanguage.ZH_CN, out var zhDict)
                    && zhDict.TryGetValue(id, out var zh)
                    && !string.IsNullOrEmpty(zh))
                {
                    return zh;
                }
            }

            return $"[{id}]";
        }

        /// <summary>取译文，带 string.Format 参数。</summary>
        public string GetText(string id, params object[] args)
        {
            string template = GetText(id);
            if (args == null || args.Length == 0) return template;
            return template.SafeFormat(args);
        }

        /// <summary>切语言。命中失败保持原值并 LogError。</summary>
        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            if (code == m_CurrentLanguage) return;
            if (m_Loaded && !m_Table.ContainsKey(code))
            {
                Debug.LogError($"[Localization] 不支持的语言代码 {code}（CSV 中未识别到该列）");
                return;
            }

            m_CurrentLanguage = code;
            PlayerPrefs.SetString(LocalizationLanguage.PLAYER_PREFS_KEY, code);
            PlayerPrefs.Save();

            OnLanguageChanged?.Invoke();
        }

        /// <summary>当前语言下是否存在指定 ID（用于 CR 抽查）。</summary>
        public bool Contains(string id)
        {
            if (!m_Loaded || string.IsNullOrEmpty(id)) return false;
            return m_Table.TryGetValue(m_CurrentLanguage, out var dict) && dict.ContainsKey(id);
        }
    }
}
