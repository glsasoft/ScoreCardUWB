using LetsPiu.Common.Network.Model.Http.Play;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Event
{
    public class PlayerChangeMessage
    {
        /// <summary>
        /// 是否是实时的击杀消息， false代表是同步过来的消息可能有延迟
        /// </summary>
        public bool isRealTime = false;
        /// <summary>
        /// 死亡的时间
        /// </summary>
        public Player player;
    }
}