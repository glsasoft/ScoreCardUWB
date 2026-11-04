using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class AbstractUIElement : UIMonoBehaviour
    {
        /// <summary>
        /// UI创建时调用
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// UI释放时调用
        /// </summary>
        public virtual void OnDispose() { }

        /// <summary>
        /// UI打开时调用，该调用可能会存在多次
        /// </summary>
        public virtual void OnOpen(object[] args) { }

        /// <summary>
        /// UI关闭时调用，该UI可能被放入缓存，并不释放
        /// </summary>
        public virtual void OnClose() { }

        /// <summary>
        /// 可见状态改变
        /// </summary>
        public virtual void OnVisibleChanged(bool isVisible) { }

        /// <summary>
        /// 处理View事件
        /// </summary>
        /// <param name="eventID">事件ID</param>
        /// <param name="args">参数列表</param>
        public virtual void OnViewEvent(int e, object[] args) { }
    }
}

