using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 生化模式分阶段战绩（人类期 <c>humanStat</c> / 丧尸期 <c>zombieStat</c>）。
    /// 玩家根字段仍是两段汇总；本结构为阶段明细。
    /// </summary>
    public class ScoreCardPlayerPhaseStat
    {
        [JsonProperty("killNum")]
        public int killNum;

        [JsonProperty("killFriendNum")]
        public int killFriendNum;

        [JsonProperty("hitNum")]
        public int hitNum;

        [JsonProperty("beKillNum")]
        public int beKillNum;

        [JsonProperty("beHitNum")]
        public int beHitNum;

        [JsonProperty("assistNum")]
        public int assistNum;

        [JsonProperty("totalHurt")]
        public int totalHurt;

        [JsonProperty("totalHeal")]
        public int totalHeal;

        [JsonProperty("outputDamage")]
        public int outputDamage;

        [JsonProperty("matchScore")]
        public int matchScore;
    }
}
