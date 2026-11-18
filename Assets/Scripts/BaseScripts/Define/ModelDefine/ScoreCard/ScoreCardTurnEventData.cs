using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 竞速靶场 TURN_BEGIN / TURN_END 的 msg.data。
    /// EVENT 侧常只带 rangeData（currentPlayerId + players 全量）；NOTICE 侧常带 mainPlayer。
    /// 客户端换人动画以 rangeData.currentPlayerId 为准，mainPlayer 仅作兜底。
    /// </summary>
    public class ScoreCardTurnEventData
    {
        [JsonProperty("mainPlayer")]
        public ScoreCardPlayerPartial mainPlayer;

        [JsonProperty("rangeData")]
        public ScoreCardRangeData rangeData;
    }
}
