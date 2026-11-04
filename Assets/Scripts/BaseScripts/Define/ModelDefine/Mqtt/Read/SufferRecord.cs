using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// [服务器下发]被击中消息
    /// Topic: glsa/player/{房间 ID}/{玩家短编号}
    /// </summary>
    public class SufferRecord
    {
        /// <summary>
        /// 对敌伤害值
        /// </summary>
        public int attackerHarm;

        /// <summary>
        /// 攻击方id
        /// </summary>
        public int attackerId;

        /// <summary>
        /// 序列号
        /// </summary>
        public int battleIndex;
        /// <summary>
        /// 被攻击方子弹数
        /// </summary>
        public int bullet;

        /// <summary>
        /// 击中位置
        /// </summary>
        public HitLocation positionType;

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
        /// 被击者id
        /// </summary>
        public int suffererId;

        /// <summary>
        /// 伤害类型
        /// </summary>
        public AttackType type;

        /// <summary>
        /// 来源ID
        /// </summary>
        public string sourceId;
    }
}