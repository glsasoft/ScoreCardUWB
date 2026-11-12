using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

namespace LetsPiu
{
    public class HUD_CountDown : SingletonBaseWithMono<HUD_CountDown>
    {
        public SequenceFrameNumber number1;
        public SequenceFrameNumber number2;

        private bool m_IsShow = false;
        private Tweener m_TweenScale = null;
        private Tweener m_TweenAlpha = null;

        public Ease scaleEase = Ease.OutQuad;
        private Ease currentScaleEase = Ease.OutQuad;

        // Start is called before the first frame update
        void Start()
        {
            Hide();
            currentScaleEase = scaleEase;
        }

        private void Update()
        {
            if (m_TweenScale != null && scaleEase != currentScaleEase)
            {
                currentScaleEase = scaleEase;
                m_TweenScale.SetEase(scaleEase);
            }
        }

        private void OnDestroy()
        {
            m_TweenScale?.Kill();
            m_TweenAlpha?.Kill();
        }

        public void Show()
        {
            number1.SafeSetActive(true);
            number2.SafeSetActive(true);
            m_IsShow = true;
        }

        public void Hide()
        {
            number1.SafeSetActive(false);
            number2.SafeSetActive(false);
            m_IsShow = false;
        }

        public void StartCountDown(int seconds, Action endCallback = null, Action<int> updateCallback = null)
        {
            if (seconds <= 0)
            {
                endCallback?.Invoke();
                return;
            }
            StopAllCoroutines();
            StartCoroutine(CountDown(seconds, endCallback, updateCallback));
        }

        IEnumerator CountDown(int seconds, Action endCallback, Action<int> updateCallback = null)
        {
            WaitForSeconds wfs = new WaitForSeconds(1);
            while (true)
            {
                DisplayNumber(seconds);
                updateCallback?.Invoke(seconds);
                yield return wfs;
                seconds--;
                if (seconds < 0)
                {
                    Hide();
                    endCallback?.Invoke();
                    break;
                }
            }
        }

        void DisplayNumber(int num)
        {
            if (num < 0 || num >= 100)
                return;

            if (!m_IsShow)
                Show();

            int num1 = num / 10;
            int num2 = num % 10;

            number1.ShowNum(num1);
            number2.ShowNum(num2);

            if (m_TweenScale == null)
            {
                Vector3 endValue = this.transform.localScale;
                this.transform.localScale *= 2;
                m_TweenScale = this.transform.DOScale(endValue, 0.5f);
                m_TweenScale.SetEase(scaleEase);
                m_TweenScale.SetAutoKill(false);
            }
            else
            {
                //m_TweenScale.Rewind();
                m_TweenScale.Restart();
            }

            if (m_TweenAlpha == null && number1 && number2)
            {
                Color start = number1.color;
                start.a = 0;

                m_TweenAlpha = DOTween.ToAlpha(() => start, delegate (Color c)
                {
                    number1.color = c;
                    number2.color = c;
                }, 1, 0.5f);

                m_TweenAlpha.SetEase(Ease.Linear);
                m_TweenAlpha.SetAutoKill(false);
            }
            else
            {
                //m_TweenAlpha?.Rewind();
                m_TweenAlpha?.Restart();
            }
        }

        [ContextMenu("CountDownTest")]
        void CountDownTest()
        {
            StartCountDown(99);
        }
    }
}
