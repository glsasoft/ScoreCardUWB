using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device
{
    public class DeviceInfo5S
    {

        /// <summary>
        /// 比分牌编号
        /// </summary>
        [JsonProperty("id")]
        [CanBeNull] private string _id; //"1"、"2"...

        /// <summary>
        /// 友盟标识
        /// </summary>
        [JsonProperty("umeng_flag")]
        [CanBeNull] private string _umengFlag; //"测试1号"
        
        /// <summary>
        /// 服务器地址
        /// </summary>
        [JsonProperty("uri")]
        [CanBeNull] private string _uri;
        
        /// <summary>
        /// 设备名称
        /// </summary>
        [JsonProperty("device_name")]
        [CanBeNull] private string _deviceName;
        
        /// <summary>
        /// 设备密码
        /// </summary>
        [JsonProperty("device_pass")]
        [CanBeNull] private string _devicePass;
        
        /// <summary>
        /// 发送消息主题
        /// </summary>
        [JsonProperty("publish_topic")]
        [CanBeNull] private string _publishTopic;
        
        /// <summary>
        /// 接收消息主题
        /// </summary>
        [JsonProperty("subscribe_topic")]
        [CanBeNull] private string  _subscribeTopic;
        
        /// <summary>
        /// 产品类型
        /// store9 长风 test 世博 test1 世博2号
        /// </summary>
        [JsonProperty("product_key")]
        [CanBeNull] private string _productKey;

        /// <summary>
        /// 设备信息是否有效
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return _umengFlag != null
                && _uri != null
                && _deviceName != null
                && _devicePass != null
                && _publishTopic != null
                && _subscribeTopic != null;
        }
        
        /// <summary>
        /// 返回字段信息，如果字段为空，返回字段名称:null，否则返回字段名称:字段值
        /// </summary>
        /// <returns></returns>
        public string GetInvalidDataInfo()
        {
            //返回字段信息，如果字段为空，返回字段名称:null，否则返回字段名称:字段值
            var info = "";
            info += "_umengFlag:" + (_umengFlag ?? "null") + ", ";
            info += "_uri:" + (_uri ?? "null") + ", ";
            info += "_deviceName:" + (_deviceName ?? "null") + ", ";
            info += "_devicePass:" + (_devicePass ?? "null") + ", ";
            info += "_publishTopic:" + (_publishTopic ?? "null") + ", ";
            info += "_subscribeTopic:" + (_subscribeTopic ?? "null");
            return info;
        }
        
        public string GetUmengFlag()
        {
            return _umengFlag ?? "测试1号";
        }

        /// <summary>比分牌编号（服务端注册返回，如 "1"、"2"）。</summary>
        public string GetId()
        {
            return _id;
        }
        
        public string GetUri()
        {
            return _uri ?? "tcp://tx3.wogemu.com:1883";
        }
        
        public string GetDeviceName()
        {
            return _deviceName ?? "xxx";
        }
        
        public string GetDevicePass()
        {
            return _devicePass ?? "xxx";
        }
        
        public string GetProductKey()
        {
            return _productKey ?? "xxx";
        }

        /// <summary>门店编号，从 product_key（如 "store22"）解析出数字部分；未注册或格式不符时返回 false。</summary>
        public bool TryGetStoreId(out int storeId)
        {
            storeId = 0;
            if (string.IsNullOrWhiteSpace(_productKey))
            {
                return false;
            }

            string text = _productKey.Trim().Trim('/');
            if (text.StartsWith("store", System.StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring("store".Length);
            }

            return int.TryParse(text, out storeId) && storeId > 0;
        }

        public string GetPublishTopic()
        {
            if (_publishTopic != null) return _publishTopic;
            return "/" + GetProductKey().TrimStart('/') + "/" + GetDeviceName() + "/user/update";
        }
        
        public string GetSubscribeTopic()
        {
            if (_subscribeTopic != null) return _subscribeTopic;
            return "/" + GetProductKey().TrimStart('/') + "/" + GetDeviceName() + "/user/get";
        }

        public void PrintDeviceInfo(string tag)
        {
            UnityEngine.Debug.Log(tag+" DeviceInfo5S: " +
                                  "id=" + (GetId() ?? "null") + ", " +
                                  "umeng_flag=" + GetUmengFlag() + ", " +
                                  "uri=" + GetUri() + ", " +
                                  "device_name=" + GetDeviceName() + ", " +
                                  "device_pass=" + GetDevicePass() + ", " +
                                  "product_key=" + GetProductKey() + ", " +
                                  "publish_topic=" + GetPublishTopic() + ", " +
                                  "subscribe_topic=" + GetSubscribeTopic());
        }
    }
}