using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// TOWER_KILL 事件 <c>msg.data</c>：被防御靶灯击杀的玩家局部字段（服务器不下发防御灯所属队伍）。
    /// </summary>
    public class ScoreCardTowerKillEventData
    {
        [JsonProperty("victim")]
        public ScoreCardPlayerPartial victim;
    }
}
