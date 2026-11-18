using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// RED/BLUE_BUFF_CAPTURED 夺取 BUFF 事件 <c>msg.data</c>。
    /// </summary>
    public class ScoreCardBuffCapturedEventData
    {
        [JsonProperty("level")]
        public int level;

        /// <summary>NOTICE 消息仅带 name/vestId/id。</summary>
        [JsonProperty("mainPlayer")]
        public ScoreCardPlayerPartial mainPlayer;

        /// <summary>EVENT 消息携带的夺取者全量快照。</summary>
        [JsonProperty("killer")]
        public ScoreCardPlayerPartial killer;

        public ScoreCardPlayerPartial ResolveCapturer() => killer ?? mainPlayer;
    }
}
