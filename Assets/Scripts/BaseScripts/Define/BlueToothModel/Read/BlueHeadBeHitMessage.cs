using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read;

namespace Common.Bluetooth.Model.Read
{
    public class BlueHeadBeHitMessage : BlueReadMessage
    {
        public string shootId;
        public string teamId;
        public string skill;
        public BlueHeadBeHitMessage(string strTemp)
        {
            messageType = BlueReadMessageType.BlueReadHeadBeHitType;
            strValue = strTemp;
            shootId = GetInt16FromHexString(GetByte3()) + "";
            teamId = GetInt16FromHexString(strValue.Substring(8, 1)) + "";
            skill = GetInt16FromHexString(strValue.Substring(9, 1)) + "";
        }

        public BlueHeadBeHitMessage()
        {
            
        }
    }
}
