using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 该文件打包时可排除
    /// </summary>
    public class EventMgrExample : MonoBehaviour
    {
        void OnEnable()
        {
            // 模拟消息注册，该代码通常在监听方出现时实现
            // 注册一条消息的之前，需要在EventMsg枚举中注册一个消息ID
            EventManager.Instance.AddEvent<string>(EventMsg.None, TestCount);
        }

        void OnDisable()
        {
            // 模拟消息取消注册，该代码通常在监听方销毁或者隐藏时实现
            EventManager.Instance.RemoveEvent<string>(EventMsg.None, TestCount);
        }

        // 模拟监听方收到消息后的实现
        void TestCount(string param)
        {
            Debug.LogError(param);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 模拟发送消息，该代码通常在发送方
                EventManager.Instance.Broadcast(EventMsg.None, "测试消息框架");
            }
        }
    }
}
