using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// COMBO_HIT 竞速靶场连击高光 <c>msg.data</c>（NOTICE）。
    /// <para>服务端从 2 连起、仅在刷新本回合最高连击时下发；客户端 wav 从 2 连起播。</para>
    /// </summary>
    public class ScoreCardComboHitEventData
    {
        [JsonProperty("level")]
        public int level;

        [JsonProperty("comboCount")]
        public int comboCount;

        [JsonProperty("mainPlayer")]
        public ScoreCardPlayerPartial mainPlayer;

        [JsonProperty("subPlayer")]
        public ScoreCardPlayerPartial subPlayer;
    }
}
