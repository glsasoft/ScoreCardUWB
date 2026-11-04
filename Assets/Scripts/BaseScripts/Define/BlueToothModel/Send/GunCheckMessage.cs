namespace Common.Bluetooth.Model.Send
{
    public class CheckGunMessage : BlueBaseMessage
    {
        public static CheckGunMessage CreateCheckGunMessage(int nTime)
        {
            var colorMessage = new CheckGunMessage
            {
                shortId = GetHexStringFromInt(0),
                cmd = "02",
                btye3 = GetHexStringFromInt(nTime)
            };
            return colorMessage;
        }
    }
}
