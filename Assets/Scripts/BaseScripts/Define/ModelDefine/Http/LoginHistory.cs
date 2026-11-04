using System;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http
{
    /// <summary>
    /// 历史登录记录
    /// </summary>
    public class LoginHistory
    {
        /// <summary>
        /// 历史登录用户
        /// </summary>
        public global::Common.Network.Model.Http.Customer customer;
        /// <summary>
        /// 历史登录时间
        /// </summary>
        public DateTime loginTime;
        public string token;
    }
}