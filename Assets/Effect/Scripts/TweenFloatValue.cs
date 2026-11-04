using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace LetsPiu.Base
{
    public class TweenFloatValue : MonoBehaviour
    {
        public bool autoPlay = true;
        public bool restartOnEnable = false;
        public float duration = 1;
        public float delay = 0;
        public Ease easeType = Ease.OutQuad;
        public LoopType loopType = LoopType.Restart;
        [Tooltip("-1为无限循环")]
        public int loops = 1;
        public float startValue = 0;
        public float endValue = 1;
        [Space(20)]
        public UnityEvent<float> onValueChanged;

        [NonSerialized]
        public Tween tween;

        private void OnEnable()
        {
            if (tween != null)
            {
                if (restartOnEnable && tween.active) tween.Restart();
                return;
            }

            tween = DOTween.To(() => startValue, onValueChanged.Invoke, endValue, duration);

            tween.SetTarget(gameObject).SetDelay(delay).SetLoops(loops, loopType).SetAutoKill(false).OnKill(() => tween = null);
            tween.SetEase(easeType);

            if (autoPlay || restartOnEnable)
                tween.Play();
            else
                tween.Pause();
        }

        private void OnDisable()
        {
            if (tween != null && tween.active) tween.Pause();
        }

        private void OnDestroy()
        {
            if (tween != null && tween.active) tween.Kill();
            tween = null;
        }

        [ContextMenu("DORewind")]
        public void DORewind()
        {
            // DOTween.Rewind(gameObject);
            if (tween != null && tween.IsInitialized()) tween.Rewind();
        }

        [ContextMenu("DORestart")]
        public void DORestart()
        {
            DOTween.Restart(gameObject);
        }

        [ContextMenu("DOComplete")]
        public void DOComplete()
        {
            DOTween.Complete(gameObject);
        }
    }
}
