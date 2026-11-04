using System;
using System.Text;

namespace Common.Bluetooth.Model
{
    public class BlueBaseMessage 
    {

        public string strValue;

        string begin = "5A";
        string end = "A5";
        public string shortId;
        public string cmd ;
        public string btye3="00";
        public string btye4 = "00";
        public string btye5 = "00";
        public string btye6 = "00";
        public string btye7 = "00";
        public string btye8 = "00";

        public string GetShortId() {
            return strValue.Substring(2,2);
        }

        public string GetCmd()
        {
            return strValue.Substring(4, 2);
        }
    
        public string GetByte3()
        {
            return strValue.Substring(6, 2);
        }

        public string GetByte4()
        {
            return strValue.Substring(8, 2);
        }

        public string GetByte5()
        {
            return strValue.Substring(10, 2);
        }


        public string GetByte6()
        {
            return strValue.Substring(12, 2);
        }

        public string GetByte7()
        {
            return strValue.Substring(14, 2);
        }

        public string GetByte8()
        {
            return strValue.Substring(16, 2);
        }
        
        public string GenerateMessage() {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(begin[..2]);
            stringBuilder.Append(shortId[..2]);
            stringBuilder.Append(cmd[..2]);
            stringBuilder.Append(btye3[..2]);
            stringBuilder.Append(btye4[..2]);
            stringBuilder.Append(btye5[..2]);
            stringBuilder.Append(btye6[..2]);
            stringBuilder.Append(btye7[..2]);
            stringBuilder.Append(btye8[..2]);
            stringBuilder.Append(end);
            var result = stringBuilder.ToString();
            return result;
        }
    
        public static string GetHexStringFromInt(int value) {
            // 将十进制数值转换为十六进制字符串
            var hexValue = value.ToString("X");
            hexValue = hexValue.Length switch
            {
                1 => "0" + hexValue,
                > 2 => hexValue[..2],
                _ => hexValue
            };
            return hexValue;
        }

        public static int GetInt16FromHexString(string hexValue) {
            var intValue = Convert.ToInt32(hexValue, 16);
            return intValue;
        }
    }
}
