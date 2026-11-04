namespace LetsPiu.Base.Localization
{
    /// <summary>
    /// BCP 47 语言代码常量。新增语言只需追加常量并在 CSV 增加对应列名（见 UI_L10N.csv）。
    /// </summary>
    public static class LocalizationLanguage
    {
        public const string ZH_CN = "zh-CN";
        public const string EN_US = "en-US";
        public const string ZH_TW = "zh-TW";
        public const string JA_JP = "ja-JP";
        public const string KO_KR = "ko-KR";

        /// <summary>未持久化时的默认语言（强制 zh-CN，不跟随系统）。</summary>
        public const string DEFAULT = ZH_CN;

        /// <summary>PlayerPrefs 持久化键名。</summary>
        public const string PLAYER_PREFS_KEY = "LocalizationLang";
    }
}
