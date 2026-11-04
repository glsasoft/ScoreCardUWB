using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace LetsPiu.Effect
{
    public class MultiObjectRepeatingEffect : RepeatingEffectBase
    {
        public GameObject beginningObject;
        public GameObject repeatingObject;
        public GameObject endingObject;

        [FormerlySerializedAs("beginningLength")] public float startLength = 0.5f;
        [FormerlySerializedAs("endingLength")] public float endLength = 0.5f;

        private ParticleSystem beginningParticles;
        private ParticleSystem repeatingParticles;
        private ParticleSystem endingParticles;

        public override float GetBeginningDuration() => startLength;
        public override float GetEndingDuration() => endLength;

        CancellationTokenSource m_CancelToken;

        protected void Awake()
        {
            if (beginningObject) beginningParticles = beginningObject.GetComponent<ParticleSystem>();
            if (repeatingObject) repeatingParticles = repeatingObject.GetComponent<ParticleSystem>();
            if (endingObject) endingParticles = endingObject.GetComponent<ParticleSystem>();
            OnBeginning();
            //DeactivateAllSegments();
        }

        public void SetEffectToRepeatingState()
        {
            if (m_CancelToken?.Token.CanBeCanceled == true)
            {
                m_CancelToken.Cancel();
                m_CancelToken.Dispose();
            }
            m_CancelToken = new CancellationTokenSource();
            state = EffectState.Repeating;
            beginningObject.SetActive(false);
            repeatingObject.SetActive(true);
            endingObject.SetActive(false);
            //SetEffectTime(repeatingObject, 0);
        }

        public override void ResetEffect()
        {
            if (m_CancelToken?.Token.CanBeCanceled == true)
            {
                m_CancelToken.Cancel();
                m_CancelToken.Dispose();
            }
            m_CancelToken = new CancellationTokenSource();
            DeactivateAllSegments();
            state = EffectState.Idle;
        }

        protected override void OnBeginning()
        {
            if (m_CancelToken?.Token.CanBeCanceled == true)
            {
                m_CancelToken.Cancel();
                m_CancelToken.Dispose();
            }
            m_CancelToken = new CancellationTokenSource();
            EffectLifecycle().Forget();
        }

        protected override void OnEnding()
        {
            if (m_CancelToken?.Token.CanBeCanceled == true)
            {
                m_CancelToken.Cancel();
                m_CancelToken.Dispose();
            }
            m_CancelToken = new CancellationTokenSource();
            EndEffectImmediately().Forget();
        }

        private async UniTask EffectLifecycle()
        {
            state = EffectState.Beginning;

            // 激活开始段
            beginningObject.SetActive(true);
            repeatingObject.SetActive(false);
            endingObject.SetActive(false);
            SetEffectTime(beginningObject, 0);

            await UniTask.WaitForSeconds(startLength, cancellationToken: m_CancelToken.Token);

            // === 循环阶段 ===
            state = EffectState.Repeating;

            // 激活循环段
            beginningObject.SetActive(false);
            repeatingObject.SetActive(true);
            SetEffectTime(repeatingObject, 0);

            // 如果是非循环特效，等待循环完成
            // if (!isLoopingEffect)
            // {
            //     // 等待循环粒子完成
            //     if (repeatingParticles != null)
            //     {
            //         yield return new WaitWhile(() => repeatingParticles.isPlaying);
            //     }

            //     // 非循环特效直接结束
            //     ResetEffect();
            //     yield break;
            // }
        }

        private async UniTask EndEffectImmediately()
        {
            // === 结束阶段 ===
            state = EffectState.Ending;

            // 停用其他段
            beginningObject.SetActive(false);
            repeatingObject.SetActive(false);

            // 激活结束段
            endingObject.SetActive(true);
            SetEffectTime(endingObject, 0);

            // 等待结束阶段完成
            await UniTask.WaitForSeconds(endLength, cancellationToken: m_CancelToken.Token);

            ResetEffect();
        }

        protected override void DeactivateSegment(EffectSegment segment)
        {
            GameObject target = segment switch
            {
                EffectSegment.Beginning => beginningObject,
                EffectSegment.Repeating => repeatingObject,
                EffectSegment.Ending => endingObject,
                _ => null
            };

            if (!target) return;

            target.SetActive(false);

            ParticleSystem ps = target.GetComponent<ParticleSystem>();
            if (ps)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        protected override void ActivateSegment(EffectSegment segment, float startTime)
        {
            GameObject target = segment switch
            {
                EffectSegment.Beginning => beginningObject,
                EffectSegment.Repeating => repeatingObject,
                EffectSegment.Ending => endingObject,
                _ => null
            };

            if (!target) return;

            target.SetActive(true);
            SetEffectTime(target, startTime);
        }

        protected override void DeactivateAllSegments()
        {
            //TODO：通过对象池移除特效
            if (beginningObject)
            {
                beginningObject.SetActive(false);
                if (beginningParticles) beginningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (repeatingObject)
            {
                repeatingObject.SetActive(false);
                if (repeatingParticles) repeatingParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (endingObject)
            {
                endingObject.SetActive(false);
                if (endingParticles) endingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        protected override void SetEffectTime(GameObject effect, float time)
        {
            if (!effect) return;

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps)
            {
                // 停止并清除粒子
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                // 对于循环特效，设置循环属性
                if (effect == repeatingObject)
                {
                    var main = ps.main;
                    main.loop = true;
                }
                else
                {
                    var main = ps.main;
                    main.loop = false;
                }

                // 模拟时间并播放
                ps.Simulate(time, true, true);
                ps.Play();
            }
        }
    }
}