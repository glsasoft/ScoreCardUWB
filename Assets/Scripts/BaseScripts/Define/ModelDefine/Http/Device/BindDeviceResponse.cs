using LetsPiu.Define;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device
{
    public class DeviceInfo
    {
        /// <summary>
        /// 设备二维码，维码扫码后关注公众号，可跳入小程序绑定装备
        /// </summary>
        public string qrcode;

        /// <summary>
        /// 设备类型 1：枪 2：头盔 3：胸甲 4：裤
        /// </summary>
        public DeviceType type;

        /// <summary>
        /// 子类型 (一代二代)
        /// </summary>
        public SubDeviceType subType;

        /// <summary>
        /// 颜色 1曜石黑、2皓月白、3粉晶色、4蒂芙尼蓝、5中国红
        /// </summary>
        public DeviceColor colour;

        /// <summary>
        /// 设备的蓝牙名字
        /// </summary>
        public string code;
        
        /// <summary>
        /// mac地址
        /// </summary>
        public string macAddress;
        
        /// <summary>
        /// 设备的电量(非服务器字段)
        /// </summary>
        [JsonIgnore]
        public int battery { get; set; }

        public bool IsGun()
        {
            return type == DeviceType.Gun;
        }

        public bool IsHead()
        {
            return type == DeviceType.Head;
        }
    }
    
    public class BindDeviceResponse
    {
        public DeviceInfo device;
    }
}