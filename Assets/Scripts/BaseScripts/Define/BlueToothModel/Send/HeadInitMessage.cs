namespace Common.Bluetooth.Model.Send
{
    public class HeadInitMessage : BlueBaseMessage
    {
        public static HeadInitMessage CreateInitMessage(GunInitMessage.GunInitParams @params)
        {
            HeadInitMessage initMessage = new HeadInitMessage
            {
                shortId = GetHexStringFromInt(@params.shortId),
                cmd = "20",
                //队伍
                btye3 = GetHexStringFromInt(@params.team),
                //射速
                btye4 = "00",
                //是否启动
                btye5 = @params.start ? "01" : "00"
            };
            var canBeHit = "00";
            if (@params.canBeHit)
            {
                canBeHit = "01";
            }
            //是否可被击中，是否可开枪
            initMessage.btye6 = canBeHit;
            //场次号
            initMessage.btye7 = GetHexStringFromInt(@params.GetShortPlayId());
            initMessage.btye8 = "00";
            return initMessage;
        }

    }
}
