using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read;

namespace Common.Bluetooth.Model.Read
{
    public class BlueHeadPingMessage : BlueReadMessage
    {
        public string teamId;
        public string skill;
        public int shootSpeed;
        public bool canHit;
        public bool canShoot;
        public int playId;
        public string version;
        public int power;

        public BlueHeadPingMessage(string strTemp)
        {
            messageType = BlueReadMessageType.BlueReadHeadPingType;
            strValue = strTemp;
            teamId = GetInt16FromHexString(strValue.Substring(6, 1)) + "";
            skill = GetInt16FromHexString(strValue.Substring(7, 1)) + "";
            shootSpeed = GetInt16FromHexString(GetByte4());
            canHit = strValue.Substring(10, 1).Equals("1");
            canShoot = strValue.Substring(11, 1).Equals("1");
            playId = GetInt16FromHexString(GetByte6());
            version = GetByte7();
            power = GetInt16FromHexString(GetByte8());

        }
    }
}
