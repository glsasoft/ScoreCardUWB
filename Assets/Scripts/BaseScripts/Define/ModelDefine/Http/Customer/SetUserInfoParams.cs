namespace LetsPiu.Common.Network.Model.Http.Customer
{
    /// <summary>
    /// 设置用户信息参数
    /// </summary>
    public class SetUserInfoParams
    {
        /// <summary>
        /// 头像
        /// </summary>
        public string imageUrl;
        /// <summary>
        /// 性别
        /// </summary>
        public int sex;
        /// <summary>
        /// 名字
        /// </summary>
        public string name;
        /// <summary>
        /// 经度
        /// </summary>
        public double longitude;
        /// <summary>
        /// 纬度
        /// </summary>
        public double latitude;
    }
}