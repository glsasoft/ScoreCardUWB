using System;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 服务器下发的护盾变更消息
    /// </summary>
    public class MqttShieldMessage
    {
        /// <summary>
        /// 玩家段编号
        /// </summary>
        public int shortId;
        public double shieldValue;
    }
}