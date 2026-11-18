using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// LAST_SECONDS 倒计时提醒 <c>msg.data</c>（原 LAST_30S / LAST_10S 合并）。
    /// <para>seconds：剩余秒数；常规模式多为 30，竞速靶场回合倒计时为 10。</para>
    /// </summary>
    public class ScoreCardLastSecondsEventData
    {
        [JsonProperty("level")]
        public int level;

        [JsonProperty("seconds")]
        public int seconds;
    }
}
