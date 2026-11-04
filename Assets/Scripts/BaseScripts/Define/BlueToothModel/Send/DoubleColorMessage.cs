namespace Common.Bluetooth.Model.Send
{
    public class DoubleColorMessage : BlueBaseMessage
    {
        public static DoubleColorMessage CreateGunColorMessage(double skillTimeSeconds) {
            var colorMessage = new DoubleColorMessage();
            colorMessage.cmd = "03";
            //闪烁时长存储在shortId这个字节上
            var nTime = (int)(skillTimeSeconds / 2);
            if (nTime >= 15)
            {
                nTime = 14;
            }
            var hexValue = nTime.ToString("X");

            colorMessage.shortId = hexValue+"4";
            
            const string color = "#FF0000";
            colorMessage.btye3 = color[..2];
            colorMessage.btye4 = color.Substring(2, 2);
            colorMessage.btye5 = color.Substring(4, 2);
            colorMessage.btye6 = "FF";
            colorMessage.btye7 = "60";
            colorMessage.btye8 = "00";
        
            return colorMessage;
        }
    
        public static DoubleColorMessage CreateHeadColorMessage(double skillTimeSeconds)
        {
            var colorMessage = CreateGunColorMessage(skillTimeSeconds);
            colorMessage.cmd = "25";
            return colorMessage;
        }
    }
}
