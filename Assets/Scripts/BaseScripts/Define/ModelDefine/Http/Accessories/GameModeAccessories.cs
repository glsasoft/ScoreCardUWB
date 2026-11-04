using System.Collections.Generic;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories 
{ 
    public class GameModeAccessories
    {
        /// <summary>
        /// 游戏模式id
        /// </summary>
        [JsonProperty("gameModeId")]
        public string gameModeId;
        /// <summary>
        /// 枪械配件id
        /// </summary>
        [JsonProperty("accessoriesId")]
        public string accessoriesId;
        /// <summary>
        /// 基础值
        /// </summary>
        [JsonProperty("baseValue")]
        public double baseValue;
        /// <summary>
        /// 效果计算公式
        /// </summary>
        [JsonProperty("effectFormula")]
        public string effectFormula;
        /// <summary>
        /// 最大权重值
        /// </summary>
        [JsonProperty("maxWeightValue")]
        public int maxWeightValue;
        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("remark")]
        public string remark;
        /// <summary>
        /// back1
        /// </summary>
        [JsonProperty("back1")]
        public string back1;
        /// <summary>
        /// back2
        /// </summary>
        [JsonProperty("back2")]
        public string back2;
        /// <summary>
        /// back3
        /// </summary>
        [JsonProperty("back3")]
        public string back3;
        /// <summary>
        /// back4
        /// </summary>
        [JsonProperty("back4")]
        public string back4;
        /// <summary>
        /// 权重
        /// </summary>
        [JsonProperty("weight")]
        public int? weight;
        [JsonProperty("gunAccessories")]
        public GunAccessories gunAccessories;
        /// <summary>
        /// *100显示值
        /// </summary>
        [JsonProperty("showValue")]
        public double showValue;
        /// <summary>
        /// 计算值
        /// </summary>
        [JsonProperty("computedValue")]
        public double computedValue;
    }
}