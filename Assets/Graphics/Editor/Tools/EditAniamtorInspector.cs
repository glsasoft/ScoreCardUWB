using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LetsPiu
{
    [CustomEditor(typeof(EditAnimator))]
    public class EditAnimatorInspector : UnityEditor.Editor
    {
        private Animator m_Animator;
        private AnimationClip[] animationClips = new AnimationClip[0];
        private double lastUpdateTime;

        // 定义动画名称的数组
        private string[] animationOptions = { };

        private EditAnimator editAnimator
        {
            get { return target as EditAnimator; }
        }

        private Animator animator => m_Animator ?? (m_Animator = editAnimator.GetComponent<Animator>());

        public override void OnInspectorGUI()
        {
            if (animationOptions.Length == 0)
            {
                GetAllAnimation();
            }

            // 获取当前动画时长
            float currentAnimationLength = 1f;
            if (animationClips != null && editAnimator.selectedAnimationIndex >= 0 && editAnimator.selectedAnimationIndex < animationClips.Length)
            {
                currentAnimationLength = animationClips[editAnimator.selectedAnimationIndex].length;
            }

            // 动画选择下拉菜单
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("选择动画", editAnimator.selectedAnimationIndex, animationOptions);
            if (EditorGUI.EndChangeCheck())
            {
                editAnimator.selectedAnimationIndex = newIndex;
                editAnimator.m_CurTime = 0f;
                if (animationClips != null && newIndex < animationClips.Length)
                {
                    animator.Play(animationClips[newIndex].name, 0, 0f);
                    animator.Update(0);
                }
            }

            // 时间滑动条
            EditorGUI.BeginChangeCheck();
            float newTime = EditorGUILayout.Slider("时间:", editAnimator.m_CurTime, 0f, currentAnimationLength);
            if (EditorGUI.EndChangeCheck())
            {
                editAnimator.m_CurTime = newTime;
                if (editAnimator.isPlaying)
                {
                    editAnimator.isPlaying = false;
                    EditorApplication.update -= OnUpdate;
                }
            }

            // 播放/暂停按钮
            if (GUILayout.Button(editAnimator.isPlaying ? "暂停" : "播放"))
            {
                editAnimator.isPlaying = !editAnimator.isPlaying;
                if (editAnimator.isPlaying)
                {
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                    EditorApplication.update += OnUpdate;
                }
                else
                {
                    EditorApplication.update -= OnUpdate;
                }
            }

            // 循环开关
            editAnimator.loop = EditorGUILayout.Toggle("循环", editAnimator.loop);

            // 刷新动画按钮
            if (GUILayout.Button("刷新动画列表"))
            {
                GetAllAnimation();
            }
        }

        private void GetAllAnimation()
        {
            if (animator.runtimeAnimatorController is AnimatorController controller)
            {
                animationClips = controller.animationClips;
                animationOptions = new string[animationClips.Length];
                for (int i = 0; i < animationClips.Length; i++)
                {
                    animationOptions[i] = animationClips[i].name;
                }
            }
            else
            {
                animationClips = new AnimationClip[0];
                animationOptions = new string[0];
            }
        }

        private void OnUpdate()
        {
            if (!editAnimator.isPlaying)
            {
                EditorApplication.update -= OnUpdate;
                return;
            }

            int selectedIndex = editAnimator.selectedAnimationIndex;
            if (selectedIndex < 0 || selectedIndex >= animationClips.Length)
            {
                editAnimator.isPlaying = false;
                EditorApplication.update -= OnUpdate;
                return;
            }

            AnimationClip clip = animationClips[selectedIndex];
            float duration = clip.length;

            double now = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(now - lastUpdateTime);
            lastUpdateTime = now;

            editAnimator.m_CurTime += deltaTime;

            if (editAnimator.m_CurTime >= duration)
            {
                if (editAnimator.loop)
                {
                    editAnimator.m_CurTime %= duration;
                }
                else
                {
                    editAnimator.m_CurTime = duration;
                    editAnimator.isPlaying = false;
                    EditorApplication.update -= OnUpdate;
                }
            }

            animator.Play(clip.name, 0, editAnimator.m_CurTime / duration);
            animator.Update(0);

            Repaint();
        }
    }
}