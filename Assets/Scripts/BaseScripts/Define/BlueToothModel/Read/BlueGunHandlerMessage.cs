using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read
{
    public class BlueGunHandlerMessage : BlueReadMessage
    {

        // 定义一个枚举类型
        public enum GunHandlerType
        {
            GunHandlerTypeNone=0,
            GunHandlerTypeChangeMap = 2,
            GunHandlerTypeSkill = 3,
            GunHandlerTypeSpeak = 4,
            GunHandlerTypePlayList = 5,
            GunHandlerTypeInspect = 6,
            GunHandlerTypeSuperior = 7,
            GunHandlerTypeRecharge = 8
        }

        public int index;
        public bool isShoot;
        public GunHandlerType type;
        public bool isPush;
        public BlueGunHandlerMessage(string strTemp)
        {
            messageType = BlueReadMessageType.BlueReadGunHandlerType;
            strValue = strTemp;
            //Debug.Log("shortId=" + getShortId());
            //Debug.Log("cmd=" + getCmd());
            //Debug.Log("3="+ getBtye4());
            //Debug.Log("5=" + getBtye5());
            //Debug.Log("6=" + getBtye6());
            type = (BlueGunHandlerMessage.GunHandlerType)GetInt16FromHexString(GetByte4());
            
            isShoot = GetByte3().Equals("01");
            isPush = GetByte5().Equals("01");
            Debug.Log("BlueGunHandlerMessage 按钮类型:"+type+" 是否射击="+isShoot+" 是否按下:"+isPush);
            index = GetInt16FromHexString(GetByte8());
        }
    }
}
