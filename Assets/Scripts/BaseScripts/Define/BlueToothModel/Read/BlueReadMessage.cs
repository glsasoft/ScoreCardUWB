using Common.Bluetooth.Model;
using Common.Bluetooth.Model.Read;
using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read
{
    public class BlueReadMessage : BlueBaseMessage
    {
        public enum BlueReadMessageType
        {
            BlueReadGunBeHitType,
            BlueReadGunHandlerType,
            BlueReadGunPingType,
            BlueReadHeadBeHitType,
            BlueReadHeadPingType,
            BlueReadHeadBangType,
            /// <summary>
            /// 读取mac地址
            /// </summary>
            BlueReadGunMacType,
            /// <summary>
            /// 读取手机夹子状态
            /// </summary>
            BlueReadClampStatusType,
        }

        public BlueReadMessageType messageType;

        public static BlueReadMessage parseMessage(string strTemp) {
            if (strTemp.Length == 20)
            {
                switch (strTemp.Substring(4,2))
                {
                    case "10":
                    {
                        return new BlueGunBeHitMessage(strTemp);
                    }
                    case "11":
                    {
                        return new BlueGunHandlerMessage(strTemp);
                    }
                    case "12":
                    {
                        return new BlueGunPingMessage(strTemp);
                    }
                    case "30":
                    {
                        return new BlueHeadBeHitMessage(strTemp);
                    }
                    case "31":
                    {
                        return new BlueHeadPingMessage(strTemp);
                    }
                    case "32":
                    {
                        return new BlueHeadBangMessage(strTemp);
                    }
                    default:
                        return null;
                }

            
            }else if (strTemp.Length == 40)
            {
                switch (strTemp.Substring(4,2).ToLower())
                {
                    //返回枪的mac地址消息:0x 5a 0b 0b 85 45 12 41 50 34 4e 37 34 39 0111 04 3a 47 47 a5
                    case "0b":
                    {
                        return new BlueGunMacMessage(strTemp);
                    }
                    case "0c":
                    {
                        return new BlueGunClampMessage(strTemp);
                    }
                    default:
                        Debug.LogError("BlueReadMessage parseMessage error: " + strTemp+" 长度=40");
                        return null;
                }
            }
            else {
                Debug.LogError("BlueReadMessage parseMessage error: " + strTemp+ " length:"+ strTemp.Length);
                return null;
            }
        
        }
    }
}
