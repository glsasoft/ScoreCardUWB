namespace Common.Bluetooth.Model.Send
{
    public class ColorFlashMessage : BlueBaseMessage
    {
        public static ColorFlashMessage CreateGunColor(int gunId) {
            var colorMessage = new ColorFlashMessage
            {
                shortId = GetHexStringFromInt(gunId),
                cmd = "06",
                //闪烁时长20ms
                btye3 = "05",
                //闪烁次数
                btye4 = "03",
                //567为颜色
                btye5 = "FF",
                btye6 = "FF",
                btye7 = "FF",
                //震动时间
                btye8 = "32"
            };

            return colorMessage;
        }
    
        public static ColorFlashMessage CreateHeadColor(int gunId)
        {
            var colorMessage = CreateGunColor(gunId);
            colorMessage.cmd = "26";
            return colorMessage;
        }
    }
}
