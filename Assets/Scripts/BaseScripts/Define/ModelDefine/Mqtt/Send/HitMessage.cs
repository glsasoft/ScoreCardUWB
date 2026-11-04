using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Send
{
    /// <summary>
    /// 客户端上报给服务器的被击中消息
    /// Topic: /glsa/player/{房间 ID}/{玩家短编号}
    /// </summary>
    public class HitMessage
    {
        /// <summary>
        /// 被击中部位
        /// </summary>
        public HitLocation positionType;
        /// <summary>
        /// 场次id
        /// </summary>
        public string playId;
        /// <summary>
        /// 射击者id
        /// </summary>
        public int attackerId;
        /// <summary>
        /// 被击中者的id
        /// </summary>
        public int suffererId;
        /// <summary>
        /// 当前子弹
        /// </summary>
        public int bullet;
    }
}