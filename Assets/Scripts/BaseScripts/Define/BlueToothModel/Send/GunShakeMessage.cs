using Common.Bluetooth.Model;

namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Send
{
    /// <summary>
    /// 2代枪的闪烁指令
    /// </summary>
    public class GunShakeMessage : BlueBaseMessage
    {
        /// <summary>
        /// 控制闪烁和震动(指定颜色，根队伍颜色间隔闪烁)
        /// </summary>
        /// <param name="shortId"></param>
        /// <param name="vibrationDurationMillis">震动时长毫秒</param>
        /// <returns></returns>
        public static GunShakeMessage CreateGunColor(int shortId, int vibrationDurationMillis) {

            var gunShakeMessage = new GunShakeMessage
            {
                shortId = GetHexStringFromInt(shortId),
                cmd = "04",
                //震动时长10ms倍数
                btye3 = GetHexStringFromInt(vibrationDurationMillis/10),
            };
            return gunShakeMessage;
        }
    }
}
