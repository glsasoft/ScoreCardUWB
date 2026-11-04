namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read
{
    public class BlueGunBeHitMessage : BlueReadMessage
    {

        public string shootId;
        public string teamId;
        public string skill;
        public BlueGunBeHitMessage(string strTemp) {
            messageType = BlueReadMessageType.BlueReadGunBeHitType;
            strValue = strTemp;
            shootId = GetInt16FromHexString(GetByte3())+"";
            teamId = GetInt16FromHexString(strValue.Substring(8, 1)) + "";
            skill = GetInt16FromHexString(strValue.Substring(9, 1)) + "";
        }

        public BlueGunBeHitMessage()
        {
            
        }
    }
}
