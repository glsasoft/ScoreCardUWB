using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// TARGET_REMAIN 竞速靶场剩余靶数 <c>msg.data</c>（NOTICE）。
    /// <para>仅在剩 7 / 3 / 1 三个节点各播一次；remain 为剩余待击灭靶数。</para>
    /// </summary>
    public class ScoreCardTargetRemainEventData
    {
        [JsonProperty("level")]
        public int level;

        [JsonProperty("remain")]
        public int remain;
    }
}
