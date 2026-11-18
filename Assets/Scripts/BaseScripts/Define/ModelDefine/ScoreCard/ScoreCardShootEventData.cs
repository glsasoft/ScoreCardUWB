using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// SHOOT 事件 <c>msg.data</c>：射击者 / 被击中者局部玩家字段 + 事件类型 + 助攻玩家列表。
    /// </summary>
    public class ScoreCardShootEventData
    {
        /// <summary>事件类型，如 KILL。</summary>
        [JsonProperty("type")]
        public string type;

        [JsonProperty("shooter")]
        public ScoreCardPlayerPartial shooter;

        [JsonProperty("victim")]
        public ScoreCardPlayerPartial victim;

        /// <summary>助攻玩家列表：每个元素与 shooter/victim 一样，仅合并已下发字段。</summary>
        [JsonProperty("assistList")]
        public List<ScoreCardPlayerPartial> assistList;
    }

    public static class ScoreCardShootEventType
    {
        public const string Kill = "KILL";
    }
}
