using System;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// [服务器下发]击杀消息
    /// Topic: /glsa/player/{房间 ID}/{玩家短编号}
    /// </summary>
    public class KillRecord
    {
        /// <summary>
        /// 攻击方id
        /// </summary>
        public int attackerId;

        public int blood;

        public string playId;

        /// <summary>
        /// 舔包子弹数(被击杀者的剩余子弹数)
        /// </summary>
        public int? bullet;

        /// <summary>
        /// 连杀数
        /// </summary>
        public int continuousKillNumber;
        
        /// <summary>
        /// 击杀时间
        /// </summary>
        public DateTime killDate;
        
        /// <summary>
        /// ?
        /// </summary>
        public int killIndex;
        /// <summary>
        /// ?
        /// </summary>
        public int roundId;
        
        /// <summary>
        /// 击中者的击杀数
        /// </summary>
        public int killNumber;
        
        /// <summary>
        /// 能量点
        /// </summary>
        public int? sp;
        
        /// <summary>
        /// 击杀时的击中位置
        /// </summary>
        public HitLocation positionType;
        
        /// <summary>
        /// 子类型
        /// </summary>
        public AttackSubType subType;
        
        /// <summary>
        /// 被击杀者id
        /// </summary>
        public int suffererId;
        /// <summary>
        /// 被击杀者的死亡数量
        /// </summary>
        public int deathNumber;
        /// <summary>
        /// 伤害类型
        /// </summary>
        public AttackType type;
        
        /// <summary>
        /// 复活时间
        /// </summary>
        public DateTime? reviveTime;
    }
}