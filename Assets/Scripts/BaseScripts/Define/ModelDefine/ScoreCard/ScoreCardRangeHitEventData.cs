using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>RANGE_HIT 的 <c>data.hitType</c> 取值。</summary>
    public static class ScoreCardRangeHitType
    {
        public const string Green = "GREEN";     // 绿灯
        public const string Red = "RED";         // 红灯
        public const string Penalty = "PENALTY"; // 罚时灯（黄灯）
    }

    /// <summary>
    /// RANGE_HIT 竞速靶场命中靶灯 <c>msg.data</c>（NOTICE）。
    /// </summary>
    public class ScoreCardRangeHitEventData
    {
        [JsonProperty("level")]
        public int level;

        [JsonProperty("hitType")]
        public string hitType;

        [JsonProperty("mainPlayer")]
        public ScoreCardPlayerPartial mainPlayer;
    }
}