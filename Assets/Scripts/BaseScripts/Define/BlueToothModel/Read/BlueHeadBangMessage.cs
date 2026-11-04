using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read;

namespace Common.Bluetooth.Model.Read
{
    public class BlueHeadBangMessage : BlueReadMessage
    {
        public BlueHeadBangMessage(string strTemp)
        {
            messageType = BlueReadMessageType.BlueReadHeadBangType;
            strValue = strTemp;
        }
    }
}
