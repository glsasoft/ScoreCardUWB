using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// FIRST_BLOOD 首杀事件 <c>msg.data</c>。
    /// NOTICE 使用 mainPlayer / subPlayer；EVENT 使用 killer / victim（与 SHOOT 一致）。
    /// </summary>
    public class ScoreCardFirstBloodEventData
    {
        [JsonProperty("level")]
        public int level;

        [JsonProperty("killer")]
        public ScoreCardPlayerPartial killer;

        [JsonProperty("victim")]
        public ScoreCardPlayerPartial victim;

        [JsonProperty("mainPlayer")]
        public ScoreCardPlayerPartial mainPlayer;

        [JsonProperty("subPlayer")]
        public ScoreCardPlayerPartial subPlayer;

        public ScoreCardPlayerPartial ResolveKiller() => killer ?? mainPlayer;

        public ScoreCardPlayerPartial ResolveVictim() => victim ?? subPlayer;
    }
}
