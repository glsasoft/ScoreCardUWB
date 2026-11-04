using UnityEngine;
using System;

namespace LetsPiu.Graphics
{
    public class NotifyOnExitBehaviour : StateMachineBehaviour
    {
        // 静态事件，外部可订阅（按需也可以做成实例事件）
        public static event Action<string> OnAnimationStateExit;
        
        [Tooltip("该状态的唯一标识（将作为事件传出参数）")]
        public string stateTag;
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!string.IsNullOrEmpty(stateTag))
            {
                Debug.Log($"[NotifyOnExitBehaviour] 状态退出：{stateTag}");
                OnAnimationStateExit?.Invoke(stateTag);
            }
        }
    }
}
