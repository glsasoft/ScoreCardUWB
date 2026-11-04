namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Send
{
    public class PingIndexMessage
    {
        public int index { get; set; }
        
        /// <summary>
        /// 当前玩家枪内的子弹数
        /// </summary>
        public int? bullet;
        
        /// <summary>
        /// 当前玩家背包内的子弹数
        /// </summary>
        public int? bulletBackpack;

        public PingIndexMessage(int index, int? bullet, int? bulletBackpack)
        {
            this.bullet = bullet;
            this.bulletBackpack = bulletBackpack;
            this.index = index;
        }
    }
}
