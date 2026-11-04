using UnityEngine;

namespace Common.Bluetooth.Model.Send
{
    public class ColorMessage : BlueBaseMessage
    {
        public static  ColorMessage CreateGunColorMessage(GunInitMessage.GunInitParams @params) {
            var colorMessage = new ColorMessage
            {
                shortId = GetHexStringFromInt(@params.shortId),
                cmd = "03"
            };
            // 将颜色转换为十六进制字符串（不带透明度）
            var colorStr = ColorUtility.ToHtmlStringRGB(@params.color);
            colorMessage.btye3 = colorStr[..2];
            colorMessage.btye4 = colorStr.Substring(2, 2);
            colorMessage.btye5 = colorStr.Substring(4, 2);
            colorMessage.btye6 = "00";
            colorMessage.btye7 = "00";
            colorMessage.btye8 = "00";
        
            return colorMessage;
        }
    
        public static ColorMessage CreateHeadColorMessage(GunInitMessage.GunInitParams @params)
        {
            var colorMessage = CreateGunColorMessage(@params);
            colorMessage.cmd = "24";
            return colorMessage;
        }
    }
}
