using LetsPiu.Define;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Profession
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public string id;

        /// <summary>
        /// 角色名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 类型(1工程 2侦察 3治疗 4突击)
        /// </summary>
        [JsonProperty("type")]
        public ProfessionType Type { get; set; }

        /// <summary>
        /// 阵营
        /// </summary>
        [JsonProperty("faction")]
        public FactionType Faction { get; set; }

        /// <summary>
        /// 基础生命值
        /// </summary>
        [JsonProperty("initBlood")]
        public int InitBlood { get; set; }

        /// <summary>
        /// 基础护盾
        /// </summary>
        [JsonProperty("initArmor")]
        public int InitArmor { get; set; }

        /// <summary>
        /// 护盾类型
        /// </summary>
        [JsonProperty("armorType")]
        public int ArmorType { get; set; }

        /// <summary>
        /// 受伤加速系数
        /// </summary>
        [JsonProperty("beHarmRetard")]
        public double BeHarmRetard { get; set; }

        /// <summary>
        /// 受伤枪口上移
        /// </summary>
        [JsonProperty("beHarmMuzzleUp")]
        public double BeHarmMuzzleUp { get; set; }

        /// <summary>
        /// 站立走速度
        /// </summary>
        [JsonProperty("standAndWalk")]
        public double StandAndWalk { get; set; }

        /// <summary>
        /// 站立跑速度
        /// </summary>
        [JsonProperty("standingRun")]
        public double StandingRun { get; set; }

        /// <summary>
        /// 蹲下走速度
        /// </summary>
        [JsonProperty("lowWalk")]
        public double LowWalk { get; set; }

        /// <summary>
        /// 蹲下跑速度
        /// </summary>
        [JsonProperty("squatRun")]
        public double SquatRun { get; set; }

        /// <summary>
        /// 跳跃初速度
        /// </summary>
        [JsonProperty("jumpInitialVelocity")]
        public double? JumpInitialVelocity { get; set; }

        /// <summary>
        /// 跳跃置空速度
        /// </summary>
        [JsonProperty("jumpEmptySpeed")]
        public double JumpEmptySpeed { get; set; }

        /// <summary>
        /// 滑铲距离系数
        /// </summary>
        [JsonProperty("shovelDistanceCoef")]
        public double ShovelDistanceCoef { get; set; }

        /// <summary>
        /// 坠落伤害曲线
        /// </summary>
        [JsonProperty("fallingInjuryCurve")]
        public double FallingInjuryCurve { get; set; }

        /// <summary>
        /// 信息暴露半径
        /// </summary>
        [JsonProperty("massageExposureRadius")]
        public double MassageExposureRadius { get; set; }

        /// <summary>
        /// 模型名称
        /// </summary>
        [JsonProperty("avatarName")]
        public string AvatarName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonProperty("remarks")]
        public string Remarks { get; set; }

        /// <summary>
        /// 线上线下可用（1线下，2线上）
        /// </summary>
        public LineType lineType { get; set; }
    }
}