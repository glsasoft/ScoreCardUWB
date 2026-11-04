using System;
using JetBrains.Annotations;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 服务器下发的玩家位置消息
    /// </summary>
    public class DownMap
    {
        /// <summary>
        /// 经度
        /// </summary>
        public double longitude { get; set; }
        /// <summary>
        /// 维度
        /// </summary>
        public double latitude { get; set; }
        /// <summary>
        /// 玩家手机传感器角度
        /// </summary>
        public float degree { get; set; }
        
        /// <summary>
        /// 序号
        /// </summary>
        public int? mapIndex { get; set; }
        
        /// <summary>
        /// 短编号
        /// </summary>
        public int shortId { get; set; }
        
        /// <summary>
        /// 场次id
        /// </summary>
        [CanBeNull]
        public string playId { get; set; }
        
        /// <summary>
        /// 类型 1：玩家 2：道具 3：技能 4：卡牌
        /// </summary>
        public DownMapType? type { get; set; }

        /// <summary>
        /// 子类型
        /// </summary>
        public DownMapSubType? subType { get; set; }

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 持续时长
        /// </summary>
        public int? duration { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// 范围半径
        /// </summary>
        public int? radius { get; set; }
    }
}