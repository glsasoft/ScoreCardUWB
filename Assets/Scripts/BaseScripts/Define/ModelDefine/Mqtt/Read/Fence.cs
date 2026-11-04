using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    public class Fence
    {
        /// <summary>
        /// 状态 1：离开栅栏范围（开启死亡倒计时） 0：进入栅栏范围（结束倒计时）
        /// </summary>
        public FenceStatus status = FenceStatus.Inside;
        public int? deathCountdown; // 死亡倒计时（秒）
    }
}