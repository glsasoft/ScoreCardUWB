using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// [服务器下发]击中消息
    /// Topic: glsa/player/{房间 ID}/{玩家短编号}
    /// </summary>
    public class AttackRecord
    {
        /// <summary>
        /// 攻击方护甲值
        /// </summary>
        public int attackerArmorValue;
        /// <summary>
        /// 攻击方护盾值
        /// </summary>
        public int attackerShieldValue;
        /// <summary>
        /// 伤害类型
        /// </summary>
        public AttackType type;
        /// <summary>
        /// 攻击方当前血量
        /// </summary>
        public int attackerBlood;

        /// <summary>
        /// 攻击方id
        /// </summary>
        public int attackerId;
        /// <summary>
        /// 来源ID（道具，卡牌）
        /// </summary>
        public string sourceId;
        
        /// <summary>
        /// 击中位置
        /// </summary>
        private HitLocation positionType;
        
        /// <summary>
        /// 子类型
        /// </summary>
        public AttackSubType subType;
        
        /// <summary>
        /// 收击方护甲值
        /// </summary>
        public int suffererArmorValue;
        /// <summary>
        /// 收击方护盾值
        /// </summary>
        public int suffererShieldValue;
        /// <summary>
        /// 被攻击方当前血量
        /// </summary>
        public int suffererBlood;
        /// <summary>
        /// 对自己伤害值(正数代表伤害，负数代表治疗)
        /// </summary>
        public int suffererHarm;
        /// <summary>
        /// 对敌伤害值(正数代表伤害，负数代表治疗)
        /// </summary>
        public int attackerHarm;
        /// <summary>
        /// 被击者id
        /// </summary>
        public int suffererId;
        
        /// <summary>
        /// 序列号
        /// </summary>
        public int battleIndex;
        /// <summary>
        /// 被攻击方子弹
        /// </summary>
        public int bullet;
    }
}