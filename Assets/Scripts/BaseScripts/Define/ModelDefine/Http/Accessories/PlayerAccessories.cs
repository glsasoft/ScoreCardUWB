

using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    public class PlayerAccessories
    {
        public string id;
        /// <summary>
        /// 场次id
        /// </summary>
        [JsonProperty("playId")]
        public string PlayId { get; set; }

        /// <summary>
        /// 回合id
        /// </summary>
        [JsonProperty("roundId")]
        public string RoundId { get; set; }

        /// <summary>
        /// 玩家id
        /// </summary>
        [JsonProperty("playerId")]
        public string PlayerId { get; set; }

        /// <summary>
        /// type1选配词条(枪管)
        /// </summary>
        [JsonProperty("type1AccessoriesId")]
        public string? Type1AccessoriesId { get; set; }

        /// <summary>
        /// type2选配词条(瞄准镜)
        /// </summary>
        [JsonProperty("type2AccessoriesId")]
        public string? Type2AccessoriesId { get; set; }

        /// <summary>
        /// type3选配词条(弹夹)
        /// </summary>
        [JsonProperty("type3AccessoriesId")]
        public string? Type3AccessoriesId { get; set; }

        /// <summary>
        /// type4选配词条(枪托)
        /// </summary>
        [JsonProperty("type4AccessoriesId")]
        public string? Type4AccessoriesId { get; set; }
    }
}