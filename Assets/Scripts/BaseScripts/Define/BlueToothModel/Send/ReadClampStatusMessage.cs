using Common.Bluetooth.Model;

namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Send
{
    /// <summary>
    /// 读取2代枪手机支架状态
    /// </summary>
    public class ReadClampStatusMessage : BlueBaseMessage
    {
        //读2代枪手机支架状态
        //5A    号码	CMD 0C	预留	预留	预留	预留	预留	预留	A5
        //5A000C000000000000A5
        public static ReadMacMessage Create(int shortId) {
            var colorMessage = new ReadMacMessage()
            {
                shortId = GetHexStringFromInt(shortId),
                cmd = "0C"
            };
            return colorMessage;
        }
    }
}