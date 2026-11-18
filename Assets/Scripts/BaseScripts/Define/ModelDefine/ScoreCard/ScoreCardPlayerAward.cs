namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>MVP / SVP 类型（<see cref="ScoreCardPlayer.mvpType"/>）。</summary>
    public static class ScoreCardMvpType
    {
        public const int None = 0;
        public const int Mvp  = 1;
        public const int Svp  = 2;
    }

    /// <summary>小奖类型（历史 int 枚举，已由 <see cref="ScoreCardSmallPrizeCode"/> 字符串 code 替代）。</summary>
    [System.Obsolete("使用 ScoreCardSmallPrizeCode 与 ScoreCardSmallPrize 对象结构")]
    public static class ScoreCardSmallPrizeType
    {
        /// <summary>助攻最多</summary>
        public const int MostAssist = 1;

        /// <summary>终结最多</summary>
        public const int MostKill = 2;

        /// <summary>承伤最多</summary>
        public const int MostDamageTaken = 3;

        /// <summary>低等级玩家积分最高</summary>
        public const int LowLevelTop = 4;
    }
}
