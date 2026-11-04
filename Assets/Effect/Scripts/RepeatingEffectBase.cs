using UnityEngine;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Effect
{
    public abstract class RepeatingEffectBase : MonoBehaviour
    {
        public enum EffectState
        {
            Idle, Beginning, Repeating, Ending
        }

        private void OnDisable()
        {
            ResetEffect();
        }

        public EffectState state { get; protected set; } = EffectState.Idle;

        public virtual void Begin()
        {
            if (state != EffectState.Idle) return;
            state = EffectState.Beginning;
            OnBeginning();
        }

        public virtual void End()
        {
            if (state != EffectState.Repeating) return;
            state = EffectState.Ending;
            OnEnding();
        }
        
        public void Play(float startTime, float totalLength)
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }
            
            ResetEffect();
            playRoutine = StartCoroutine(PlayEffectRoutine(startTime, totalLength));
        }
        
        public virtual void ResetEffect(){}
        
        protected abstract void OnBeginning();
        protected abstract void OnEnding();
        
        public abstract float GetBeginningDuration();

        public abstract float GetEndingDuration();
        
        protected virtual void ActivateSegment(EffectSegment segment, float startTime) {}
        
        protected virtual void DeactivateAllSegments() {}

        protected virtual void DeactivateSegment(EffectSegment segment) {}

        protected virtual void SetEffectTime(GameObject effect, float time){}

        private Coroutine playRoutine;
        
        private IEnumerator PlayEffectRoutine(float startTime, float totalLength)
        {
            state = EffectState.Beginning;
        
            // 获取首尾段时长
            float startDuration = GetBeginningDuration();
            float endDuration = GetEndingDuration();
            
            // 计算循环段时长（总时长 - 开始段 - 结束段）
            float repeatingDuration = Mathf.Max(0, totalLength - startDuration - endDuration);
                  
            // 计算实际开始位置（处理循环）
            float actualStartTime = Mathf.Repeat(startTime, totalLength);

            // 确定起始段
            EffectSegment currentSegment = EffectSegment.Beginning;
            // 已经走过的时间
            float segmentElapsed = 0;
            
            // 确定起始段和已用时间
            if (actualStartTime < startDuration)
            {
                currentSegment = EffectSegment.Beginning;
                segmentElapsed = actualStartTime;
            }
            else if (actualStartTime < startDuration + repeatingDuration)
            {
                currentSegment = EffectSegment.Repeating;
                segmentElapsed = actualStartTime - startDuration;
            }
            else
            {
                currentSegment = EffectSegment.Ending;
                segmentElapsed = actualStartTime - (startDuration + repeatingDuration);
            }
            
            DeactivateAllSegments();
            // 激活起始段
            ActivateSegment(currentSegment, segmentElapsed);
            
            // 主播放循环
            float remainingTime = totalLength - actualStartTime;
            while (remainingTime > 0)
            {
                // 获取当前段剩余时间
                float segmentRemaining = 0;
                switch (currentSegment)
                {
                    case EffectSegment.Beginning:
                        segmentRemaining = startDuration - segmentElapsed;
                        break;
                    case EffectSegment.Repeating:
                        segmentRemaining = repeatingDuration - segmentElapsed;
                        break;
                    case EffectSegment.Ending:
                        segmentRemaining = endDuration - segmentElapsed;
                        break;
                }
                
                // 计算实际前进时间（取剩余时间和段剩余时间的最小值）
                float timeToAdvance = Mathf.Min(remainingTime, segmentRemaining);
                
                // 等待段完成或时间到
                yield return new WaitForSeconds(timeToAdvance);
                
                // 更新时间和进度
                remainingTime -= timeToAdvance;
                segmentElapsed += timeToAdvance;
                
                // 更新状态
                UpdateState(currentSegment);
                
                // 检查是否需要切换到下一段
                bool shouldTransition = false;
                
                switch (currentSegment)
                {
                    case EffectSegment.Beginning:
                        shouldTransition = segmentElapsed >= startDuration;
                        break;
                    case EffectSegment.Repeating:
                        shouldTransition = segmentElapsed >= repeatingDuration;
                        break;
                    case EffectSegment.Ending:
                        shouldTransition = segmentElapsed >= endDuration;
                        break;
                }
                
                if (shouldTransition && remainingTime > 0)
                {
                    DeactivateSegment(currentSegment);

                    // 转移到下一段
                    currentSegment = GetNextSegment(currentSegment);
                    segmentElapsed = 0;
                    
                    // 激活新段
                    ActivateSegment(currentSegment, 0);
                }
            }
            
            // 播放结束
            DeactivateAllSegments();
            state = EffectState.Idle;
        }
        
        private void UpdateState(EffectSegment segment)
        {
            switch (segment)
            {
                case EffectSegment.Beginning:
                    state = EffectState.Beginning;
                    break;
                case EffectSegment.Repeating:
                    state = EffectState.Repeating;
                    break;
                case EffectSegment.Ending:
                    state = EffectState.Ending;
                    break;
            }
        }

        // 获取下一段
        private EffectSegment GetNextSegment(EffectSegment current)
        {
            return current switch
            {
                EffectSegment.Beginning => EffectSegment.Repeating,
                EffectSegment.Repeating => EffectSegment.Ending,
                EffectSegment.Ending => EffectSegment.Beginning, // 循环回到开始
                _ => EffectSegment.Repeating
            };
        }
        
        // 特效段枚举（基类内部使用）
        public enum EffectSegment
        {
            Beginning,
            Repeating,
            Ending
        }
        
#if UNITY_EDITOR
        [CustomEditor(typeof(RepeatingEffectBase), true)]
        private class RepeatingEffectEditor : Editor
        {
            private RepeatingEffectBase effect;

            private void OnEnable()
            {
                effect = (RepeatingEffectBase)target;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                
                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                if (GUILayout.Button("Begin")) effect.Begin();
                if (GUILayout.Button("End")) effect.End();
                EditorGUI.EndDisabledGroup();
            }
        }
#endif
    }
}
