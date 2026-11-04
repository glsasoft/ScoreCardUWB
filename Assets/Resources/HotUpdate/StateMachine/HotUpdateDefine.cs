using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public static class HotUpdateDefine
    {
        public const string Key_PREFS_ONLINT_PACKAGE_STATE = "onlinePackageState";

        /// <summary>
        /// 全局变量：默认资源包名称
        /// </summary>
        public static readonly string DEFAULT_PACKAGE = "DefaultPackage";
        /// <summary>
        /// 全局变量：初始资源包标签 标签下资源默认入包
        /// </summary>
        public static readonly string TAG_INITIAL = "Initial";
        /// <summary>
        /// 全局变量：线下资源包标签
        /// </summary>
        public static readonly string TAG_OFFLINE = "Offline";
        /// <summary>
        /// 全局变量：线上资源包标签
        /// </summary>
        public static readonly string TAG_ONLINE = "Online";
        
        /// <summary>
        /// 将版本好缓存到本地，以便下次加载失败后直接使用
        /// </summary>
        public static readonly string LOCAL_PACKAGE_VERSION_KEY = "LocalPackageVersionKey";
    }
}
