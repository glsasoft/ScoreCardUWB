using LetsPiu.Define;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App
{
    public class Skill
    {
        /// <summary>
        /// 角色id
        /// </summary>
        [JsonProperty("professionId")]
        public string ProfessionId { get; set; }

        /// <summary>
        /// 技能名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 类型(1被动 2小技能 3终极 4打靶技能)
        /// </summary>
        [JsonProperty("type")]
        public SkillType Type { get; set; }

        /// <summary>
        /// 1线下2线上
        /// </summary>
        [JsonProperty("lineType")]
        public LineType LineType { get; set; }

        /// <summary>
        /// 冷却时间 (CD)
        /// </summary>
        [JsonProperty("cd")]
        public int Cd { get; set; }

        /// <summary>
        /// 充能点数(能量上限, 用于游戏面板下方技能区域显示用户大技充能进度条)
        /// </summary>
        [JsonProperty("chargingPoints")]
        public float ChargingPoints { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("detail")]
        public string Detail { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("remarks")]
        public string Remarks { get; set; }
    }
}