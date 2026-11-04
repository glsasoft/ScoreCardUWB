using UnityEngine;

namespace Common.Bluetooth.Model.Send
{
    /// <summary>
    /// 震动+闪烁命令
    /// </summary>
    public class ColorFlashAndShakeMessage : BlueBaseMessage
    {
        /// <summary>
        /// 控制闪烁和震动(指定颜色，根队伍颜色间隔闪烁)
        /// </summary>
        /// <param name="shortId"></param>
        /// <param name="durationMillis">闪烁时长毫秒</param>
        /// <param name="blinkTimes">闪烁次数</param>
        /// <param name="blinkColor">闪烁的颜色</param>
        /// <param name="vibrationDurationMillis">震动时长毫秒</param>
        /// <returns></returns>
        public static ColorFlashAndShakeMessage CreateGunColor(int shortId, int durationMillis, int blinkTimes, Color blinkColor, int vibrationDurationMillis) {

            // 将颜色转换为十六进制字符串（不带透明度）
            var colorStr = ColorUtility.ToHtmlStringRGB(blinkColor);
            
            var colorAndShakeMessage = new ColorFlashAndShakeMessage
            {
                shortId = GetHexStringFromInt(shortId),
                cmd = "06",
                //闪烁时长20ms倍数
                btye3 = GetHexStringFromInt(durationMillis/20),
                //闪烁次数
                btye4 = GetHexStringFromInt(blinkTimes),
                //567为颜色
                btye5 = colorStr[..2],
                btye6 = colorStr.Substring(2, 2),
                btye7 = colorStr.Substring(4, 2),
                //震动时间20ms倍数
                btye8 = GetHexStringFromInt(vibrationDurationMillis/20)
            };

            return colorAndShakeMessage;
        }

        /// <summary>
        /// 控制闪烁和震动(指定颜色，根队伍颜色间隔闪烁)
        /// </summary>
        /// <param name="shortId"></param>
        /// <param name="durationMillis">闪烁时长毫秒</param>
        /// <param name="blinkTimes">闪烁次数</param>
        /// <param name="blinkColor">闪烁的颜色</param>
        /// <param name="vibrationDurationSeconds">震动时长秒</param>
        /// <returns></returns>
        public static ColorFlashAndShakeMessage CreateHeadColor(int shortId, int durationMillis, int blinkTimes, Color blinkColor, int vibrationDurationSeconds)
        {
            var colorAndShakeMessage = CreateGunColor(shortId, durationMillis, blinkTimes, blinkColor, vibrationDurationSeconds);
            colorAndShakeMessage.cmd = "26";
            return colorAndShakeMessage;
        }
    }
}
