namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 小奖 code 常量（与 Java <c>SmallPrizeEnum</c> 一致）。
    /// </summary>
    public static class ScoreCardSmallPrizeCode
    {
        public const string FireSuppression = "FIRE_SUPPRESSION";
        public const string StreakDominator = "STREAK_DOMINATOR";
        public const string SynergyVanguard = "SYNERGY_VANGUARD";
        public const string PlatinumGunner = "PLATINUM_GUNNER";
        public const string SurvivalExpert = "SURVIVAL_EXPERT";
        public const string CombatCore = "COMBAT_CORE";
        public const string RookieStar = "ROOKIE_STAR";

        /// <summary>生化：人类挽歌（存活最久人类）；value 为存活秒数。</summary>
        public const string HumanElegy = "HUMAN_ELEGY";

        /// <summary>生化：感染核心（感染最强丧尸）。</summary>
        public const string InfectionCore = "INFECTION_CORE";

        /// <summary>生化：阵营英雄（阵营贡献最大）。</summary>
        public const string CampHero = "CAMP_HERO";

        /// <summary>S7-1 中央颁奖区槽位数（SmallPrizePlayer × 4）。</summary>
        public const int UiSlotCount = 4;

        /// <summary>
        /// 积分模式（modeType=0）四槽顺序：助攻 → 终结 → 承伤 → 新秀。
        /// 展示文案以 MQTT <c>name</c>/<c>desc</c>/<c>value</c> 为准。
        /// </summary>
        public static readonly string[] ScoreModeSlotOrder =
        {
            SynergyVanguard,
            StreakDominator,
            FireSuppression,
            RookieStar,
        };

        /// <summary>
        /// 歼灭模式（modeType=1）四槽顺序：终结 → 承伤(存活) → 战斗核心 → 新秀。
        /// 实际下发 3~4 个奖时，无对应 code 的槽位由 UI 隐藏；同一玩家可持有多项 smallPrize。
        /// </summary>
        public static readonly string[] AnnihilationModeSlotOrder =
        {
            PlatinumGunner,
            SurvivalExpert,
            CombatCore,
            RookieStar,
        };
    }
}
