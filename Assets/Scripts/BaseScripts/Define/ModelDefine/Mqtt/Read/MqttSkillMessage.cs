using System;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 服务器下发的技能消息
    /// </summary>
    public class MqttSkillMessage
    {
        /// <summary>
        /// 玩家段编号
        /// </summary>
        public int shortId;
        /// <summary>
        /// 技能类型
        /// </summary>
        public SkillType type { get; set; }
        /// <summary>
        /// 技能状态
        /// </summary>
        public SkillStatus status { get; set; }
        /// <summary>
        /// 小技能冷却结束时间
        /// </summary>
        public DateTime coolDownEndTime { get; set; }
        /// <summary>
        /// 小技能持续结束时间
        /// </summary>
        public DateTime skillEndTime;
        /// <summary>
        /// 终极技能能量条
        /// </summary>
        public int ultimateEnergy { get; set; }
        /// <summary>
        /// 终极技能结束时间
        /// </summary>
        public DateTime ultimateSkillEndTime;
        /// <summary>
        /// 治疗技能子弹加成
        /// </summary>
        public int? skillShootSpeed;
        /// <summary>
        /// 特殊技能唯一标识
        /// </summary>
        private string uniqueId;
        // /// <summary>
        // /// 坐标信息
        // /// </summary>
        // private Position position;
    }
}