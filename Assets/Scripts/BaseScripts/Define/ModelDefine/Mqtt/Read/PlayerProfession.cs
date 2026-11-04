using System;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// [服务器下发] 角色变更消息
    /// Topic: glsa/play/{房间 ID}
    /// Topic: glsa/play/{场次 ID}
    /// </summary>
    public class PlayerProfession
    {
        public int shortId;
        public string professionId;
        public string beforehandProfessionId;
        /// <summary>
        /// 小技能冷却结束时间
        /// </summary>
        public DateTime? coolDownEndTime { get; set; }
        /// <summary>
        /// 终极技能能量条
        /// </summary>
        public int? ultimateEnergy { get; set; }
    }
}