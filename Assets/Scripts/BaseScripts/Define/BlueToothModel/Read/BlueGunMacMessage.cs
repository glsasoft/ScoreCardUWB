namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read
{
    public class BlueGunMacMessage : BlueReadMessage
    {
        public string macAddress;

        public BlueGunMacMessage(string strTemp)
        {
            messageType = BlueReadMessageType.BlueReadGunMacType;
            strValue = strTemp;
            //解析出 mac 地址
            //5a 0b 0b 85 45 12 41 50 34 4e 37 34 39 0111 04 3a 47 47 a5
            //字符串长度总共40，其中5a,0b,0b 后边一直到a5结束的字符串都是 mac 地址
            if (strTemp.Length >= 38) // 6+32=38，确保有足够的长度
            {
                //mac地址是16进制字符串，从第7个字符开始取32个字符, 在每两个字符之间加上冒号
                var rawMac = strTemp.Substring(6, 32);
                // 每两个字符插入冒号
                macAddress = FormatMacAddress(rawMac);
            }
            else
            {
                macAddress = string.Empty; // 如果字符串长度不足，设置为空
            }
        }

        private string FormatMacAddress(string rawMac)
        {
            if (string.IsNullOrEmpty(rawMac) || rawMac.Length % 2 != 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < rawMac.Length; i += 2)
            {
                if (i != 0)
                    sb.Append(":");

                if (i + 2 <= rawMac.Length)
                {
                    sb.Append(rawMac.Substring(i, 2));
                }
            }

            return sb.ToString().ToUpper(); // 通常MAC地址用大写表示
        }
    }
}