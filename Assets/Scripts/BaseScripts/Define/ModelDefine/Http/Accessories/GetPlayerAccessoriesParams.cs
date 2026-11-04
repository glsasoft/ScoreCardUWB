using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    /// <summary>
    /// 获取玩家选配参数
    /// </summary>
    public class GetPlayerAccessoriesParams
    {
        /// <summary>
        /// 场次号 必填
        /// </summary>
        [JsonProperty("playId")]
        public string PlayId { get; set; }

        /// <summary>
        /// 玩家短ID 必填
        /// </summary>
        [JsonProperty("shortId")]
        public string ShortId { get; set; }
    }
}