using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// PLAYER_REVIVE 事件 <c>msg.data</c>：复活玩家的局部字段，结构同 SHOOT 的 victim。
    /// </summary>
    public class ScoreCardReviveEventData
    {
        [JsonProperty("player")]
        public ScoreCardPlayerPartial player;
    }
}
