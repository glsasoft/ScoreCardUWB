namespace Common.Bluetooth.Model.Send
{
    public class GunPingMessage : BlueBaseMessage
    {
        public static GunPingMessage CreateGunPingMessage(int gunId, int team)
        {
            var pingMessage = new GunPingMessage
            {
                shortId = GetHexStringFromInt(gunId),
                //队伍
                btye3 = GetHexStringFromInt(team),
                //射速
                btye4 = GetHexStringFromInt(7)
            };
            //是否可被击中
            string strCanBeHit = "00";
            //是否可开枪
            string strCanShoot = "01";

            pingMessage.btye5 = strCanShoot;
            pingMessage.btye6 = strCanBeHit;
            return pingMessage;
        }
    }
}
