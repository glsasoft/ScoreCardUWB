using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    /// <summary>
    /// 更新玩家选配参数
    /// </summary>
    public class UpdatePlayerAccessoriesParams
    {
        /// <summary>
        /// 选配ID 必填
        /// </summary>
        [JsonProperty("accessoriesId")]
        public string AccessoriesId { get; set; }

        /// <summary>
        /// 场次号 必填
        /// </summary>
        [JsonProperty("playId")]
        public string PlayId { get; set; }

        /// <summary>
        /// 玩家短ID 必填
        /// </summary>
        [JsonProperty("shortId")]
        public int ShortId { get; set; }
    }
}