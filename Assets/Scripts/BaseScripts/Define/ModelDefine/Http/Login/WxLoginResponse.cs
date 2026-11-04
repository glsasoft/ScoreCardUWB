namespace Common.Network.Model.Http.Login
{
    /// <summary>
    /// 微信登录接口返回的数据
    /// </summary>
    public class WxLoginResponse
    {
        public Customer customer;
        public string token;
        public string openid;
        public string unionid;
    }
}