using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// MULTI_KILL 连杀事件 <c>msg.data</c>：连杀数 + 射击者局部玩家字段。
    /// 服务端实际下发 mainPlayer(连杀者)/subPlayer(被杀者)；兼容旧 shooter 命名。
    /// </summary>
    public class ScoreCardMultiKillEventData
    {
        [JsonProperty("comboCount")]
        public int comboCount;

        /// <summary>连杀额外加分，从 2 连杀起服务端下发，没有额外加分时为 0。</summary>
        [JsonProperty("extraScore")]
        public int extraScore;

        [JsonProperty("shooter")]
        public ScoreCardPlayerPartial shooter;

        [JsonProperty("mainPlayer")]
        public ScoreCardPlayerPartial mainPlayer;

        [JsonProperty("subPlayer")]
        public ScoreCardPlayerPartial subPlayer;

        public ScoreCardPlayerPartial ResolveShooter() => shooter ?? mainPlayer;

        public ScoreCardPlayerPartial ResolveVictim() => subPlayer;
    }
}
