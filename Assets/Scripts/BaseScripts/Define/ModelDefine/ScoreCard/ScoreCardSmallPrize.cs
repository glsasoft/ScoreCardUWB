using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 玩家小奖条目（<see cref="ScoreCardPlayer.smallPrize"/> 元素）。
    /// 全场至多 3 个不同 code 的小奖，可挂在同一或不同玩家下。
    /// </summary>
    public class ScoreCardSmallPrize
    {
        /// <summary>小奖枚举 code，如 PLATINUM_GUNNER。</summary>
        [JsonProperty("code")]
        public string code;

        /// <summary>小奖名称，如「铂金枪手」。</summary>
        [JsonProperty("name")]
        public string name;

        /// <summary>小奖统计值（击杀数、存活秒数、效率等，由服务端定义）。</summary>
        [JsonProperty("value")]
        public double value;

        /// <summary>大屏简短描述，如「击杀最多」。</summary>
        [JsonProperty("desc")]
        public string desc;
    }
}
