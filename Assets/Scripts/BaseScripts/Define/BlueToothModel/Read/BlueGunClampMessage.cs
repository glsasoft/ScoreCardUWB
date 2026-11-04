namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read
{
    /// <summary>
    /// 手机夹子是否安装
    /// </summary>
    public class BlueGunClampMessage : BlueReadMessage
    {
        /// <summary>
        /// 手机夹子是否安装(2代枪)
        /// </summary>
        public bool isInstalled;

        public BlueGunClampMessage(string strTemp)
        {
            messageType = BlueReadMessageType.BlueReadClampStatusType;
            strValue = strTemp;
            
            /*
             读手机检测按键	5A	号码	CMD 0C	预留	预留	预留	预留	预留	预留	A5										
            返回手机检测按键	5A	0C	0C	01	预留	预留	预留	预留	预留	预留	预留	预留	预留	预留	预留	预留	预留	预留	预留	A5

             */
            //解析出 安装状态
            if (strTemp.Length >= 8)
            {
                //mac地址是16进制字符串，从第7个字符开始取2个字符
                var val = strTemp.Substring(6, 2);
                isInstalled = val == "01";
            }
            else
            {
                isInstalled = false;
            }
        }
    }
}