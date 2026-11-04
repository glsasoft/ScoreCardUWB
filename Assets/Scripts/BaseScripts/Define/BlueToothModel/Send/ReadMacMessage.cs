using Common.Bluetooth.Model;

namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Send
{
    public class ReadMacMessage : BlueBaseMessage
    {
        //读MAC	
        //5A    号码	CMD 0B	预留	预留	预留	预留	预留	预留	A5
        //5A000B000000000000A5
        public static ReadMacMessage CreateReadGunMac(int shortId) {
            var colorMessage = new ReadMacMessage()
            {
                shortId = GetHexStringFromInt(shortId),
                cmd = "0B"
            };
            return colorMessage;
        }
    }
}
