namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌游戏模式常量（对应 <see cref="ScoreCardMatchData.modeType"/> 字符串取值）。
    /// 已完整对接 S1~S8：积分 / 歼灭；已对接 S1、S4：超级队长、生化；S1 开场已接入：火力积分、攻防积分。
    /// </summary>
    public static class ScoreCardGameMode
    {
        /// <summary>荣耀积分模式（默认模式）。</summary>
        public const string Score = "0";

        /// <summary>歼灭模式。</summary>
        public const string Annihilation = "1";

        /// <summary>超级队长模式。</summary>
        public const string SuperCaptain = "7";

        /// <summary>生化模式。</summary>
        public const string Zombie = "9";

        /// <summary>火力积分模式（S1 开场/转场已接入；其余页面暂沿用荣耀积分）。</summary>
        public const string Fire = "11";

        /// <summary>攻防积分模式（S1 开场/转场已接入；其余页面暂沿用荣耀积分）。</summary>
        public const string Defense = "12";

        /// <summary>竞速靶场模式（S1 开场与 S1-3 转场已接入）。</summary>
        public const string SpeedShooting = "13";
    }
}
