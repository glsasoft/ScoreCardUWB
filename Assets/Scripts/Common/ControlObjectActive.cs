using System;
using UnityEngine;

namespace LetsPiu
{
    public class ControlObjectActive : StateMachineBehaviour
    {
        [Serializable]
        public class ActiveItem
        {
            public string path;
            public bool active;
            [Tooltip("模糊查找（只针对最后一级的名字）")]
            public bool fuzzySearch;
        }

        public ActiveItem[] m_OnStateEnter;
        public ActiveItem[] m_OnStateExit;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ExecuteActive(animator, m_OnStateEnter);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ExecuteActive(animator, m_OnStateExit);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}

        void ExecuteActive(Animator animator, ActiveItem[] items)
        {
            if (animator == null || items == null)
                return;

            foreach (var item in items)
            {
                if (item != null && !string.IsNullOrEmpty(item.path))
                {
                    item.path = item.path.Trim('/');
                    if (string.IsNullOrEmpty(item.path))
                        continue;

                    if (item.fuzzySearch)
                    {
                        int index = item.path.LastIndexOf('/');
                        if (index < 0)
                        {
                            foreach (Transform t in animator.transform)
                            {
                                if (t.name.Contains(item.path) || item.path.Contains(t.name))
                                {
                                    t.gameObject.SetActive(item.active);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            string parentPath = item.path.Substring(0, index);
                            string itemName = item.path.Substring(index + 1);
                            Transform parent = animator.transform.Find(parentPath);
                            if (parent)
                            {
                                foreach (Transform t in parent)
                                {
                                    if (t.name.Contains(itemName) || itemName.Contains(t.name))
                                    {
                                        t.gameObject.SetActive(item.active);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Transform child = animator.transform.Find(item.path);
                        if (child && child != animator.transform)
                            child.gameObject.SetActive(item.active);
                    }
                }
            }
        }
    }
}
