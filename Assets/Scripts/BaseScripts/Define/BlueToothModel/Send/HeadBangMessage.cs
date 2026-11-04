using Unity.VisualScripting;

namespace Common.Bluetooth.Model.Send
{
    /// <summary>
    /// 引爆彩蛋
    /// </summary>
    public class HeadBangMessage : BlueBaseMessage
    {
        public static HeadBangMessage CreateGunBangMessage(int shortId)
        {
            var bangMessage = new HeadBangMessage
            {
                shortId = GetHexStringFromInt(shortId),
                cmd = "23",
                btye3 = "40"
            };
            return bangMessage;
        }
    }
}
