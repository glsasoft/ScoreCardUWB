using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;

namespace LetsPiu.Base.BaseScripts.Define.BlueToothModel
{
    public enum BindDeviceErrorType
    {
        InvalidDeviceType,
        NeedLogin,
        /// <summary>
        /// 装备连接失败
        /// </summary>
        ConnectFailed,
        Fail,
        Success
    }
    
    /// <summary>
    /// 绑定结果
    /// </summary>
    public class BindDeviceResult
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public string errMsg;
        /// <summary>
        /// 是否成功
        /// </summary>
        public BindDeviceErrorType errType;
        /// <summary>
        /// 设备信息
        /// </summary>
        public DeviceInfo deviceInfo;

        public BindDeviceResult(DeviceInfo deviceInfo, string errMsg, BindDeviceErrorType errType)
        {
            this.errMsg = errMsg;
            this.errType = errType;
            this.deviceInfo = deviceInfo;
        }
    }
}