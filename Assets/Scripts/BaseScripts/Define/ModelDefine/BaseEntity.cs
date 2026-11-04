using System;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine
{
    public class BaseEntity
    {
        /// <summary>
        /// ID
        /// </summary>
        public string id;
        
        // /// <summary>
        // /// 创建时间
        // /// </summary>
        // public DateTime gmtCreate;
        
        /// <summary>
        /// 更新者
        /// </summary>
        public DateTime? gmtModify;
        /// <summary>
        /// 创建者
        /// </summary>
        public string createUser;
        /// <summary>
        /// 创建者
        /// </summary>
        public string modifyUser;
    }
}
