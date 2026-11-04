using System.Collections.Generic;
using LetsPiu.Define;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    public class GunAccessories
    {

        public string id;
        
        /// <summary>
        /// 1:枪管 2:瞄准镜 3:弹匣 4:枪托
        /// </summary>
        [JsonProperty("type")]
        public WeaponPart Type { get; set; }

        /// <summary>
        /// 词条
        /// </summary>
        [JsonProperty("headWord")]
        public string HeadWord { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("detail")]
        public string Detail { get; set; }

        /// <summary>
        /// 属性名：值映射
        /// </summary>
        [JsonProperty("attributesKv")]
        public string AttributesKv { get; set; }

        /// <summary>
        /// 枪械版本1:piu  2:piu2
        /// </summary>
        [JsonProperty("gunVersion")]
        public int GunVersion { get; set; }
    }
}