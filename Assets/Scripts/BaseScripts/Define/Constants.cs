using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read;
using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define
{
    /// <summary>
    /// LetsPiu的存储常量
    /// </summary>
    public class Constants
    {
        public const string HTTP_SERVER_URL = "http://5s.wogemu.com:10000/";
        
        /// <summary>
        /// 5S 设备自动注册接口地址（与主游戏服务器独立）
        /// 对应 Android 端 Constant.storeAdminServerUrl
        /// </summary>
        public const string HTTP_REGISTER_5S_DEVICE_URL = "https://5s.qmlz.store/server/api/registerDevi";

        /// <summary>门店服务器时间。query：storeId、clientSendMs。data 是内嵌 JSON 字符串。</summary>
        public const string HTTP_SERVER_TIME_URL = "https://5s.qmlz.store/server/api/serverTime";
        
        //-------------- 测试服务器 ---------------------------
        // public const string HTTP_SERVER_URL = "http://110.40.187.106:9979";
        // public const string HTTP_SERVER_URL = "https://piupiu.glsa.com.cn:442";

        #region 服务器的返回码

        /// <summary>
        /// 服务器返回TOKEN登录超时
        /// </summary>
        public const int HttpReturnCodeLoginTimeout = 606;

        #endregion
        
        public const string KEY_PREFS_CUSTOMER_INFO = "customerInfo";
        public const string KEY_PREFS_DEVICEINFO5S_INFO = "deviceInfo5SInfo";
        public const string KEY_PREFS_TOKEN_INFO = "tokenInfo";
        /// <summary>
        /// 测试用，设置服务器地址
        /// </summary>
        public const string KEY_PREFS_SERVER_ADDRESS = "serverAddress";
        /// <summary>
        /// 准星坐标点
        /// </summary>
        public const string KEY_PREFS_AIM_POSITION = "aimPosition";
        /// <summary>
        /// 准星颜色
        /// </summary>
        public const string KEY_PREFS_AIM_COLOR = "aimColor";
        
        public const string KEY_PREFS_TEMP_COUNTRY_CODE = "tmpCountryCode";
        public const string KEY_PREFS_TEMP_LOGIN_PHONE = "tmpLoginPhone";
        /// <summary>
        /// 用户历史登录信息
        /// </summary>
        public const string KEY_PREFS_LOGIN_HISTORY = "loginHistory";
        
        public const int HTTP_RETURN_CODE_NEED_LOGIN = 606;

        public static readonly Vector3 INVISIBLE_POS = new Vector3(100000, 100000, 100000);
        public static readonly Vector2 INVISIBLE_POS_2D = new Vector3(100000, 100000);

        /// <summary>
        /// 全局变量：默认资源包名称
        /// </summary>
        public static readonly string DEFAULT_PACKAGE = "DefaultPackage";

        public static readonly string StreamingAssetBaseUrl = "";
    }
}