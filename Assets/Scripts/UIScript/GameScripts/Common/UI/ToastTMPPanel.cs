using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using LetsPiu.UIDefine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.GameScripts.Common.UI
{
    /// <summary>
    /// 如下
    /// </summary>
    public class ToastTMPPanel : AbstractPanel
    {

        [SerializeField] private RectTransform toastPanel;
        [SerializeField] private TMP_Text toastText;

        private Coroutine hideCoroutine;

        public void SetStr(string str)
        {
            if (toastText)
            {
                toastText.text = str;
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            toastPanel.gameObject.SetActive(false);
        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            UIModule.S.AddPanelsInTopCamera(this.uiID);
            UniTask.Post(async () =>
            {
                await UniTask.Delay(100);
                FadeInToast();
            });
        }

        public override void OnClose()
        {
            base.OnClose();
            UIModule.S.RemovePanelsInTopCamera(this.uiID);
        }

        private void FadeInToast()
        {
            if (!toastPanel) return;
            ClearStopCoroutine();
            FadeInMessage(toastPanel.gameObject);
            //2秒后隐藏
            hideCoroutine = StartCoroutine(StartDelayFadeOut(toastPanel.gameObject));
        }

        /// <summary>
        /// 2秒后渐隐消失
        /// </summary>
        private IEnumerator StartDelayFadeOut(GameObject item)
        {
            yield return new WaitForSeconds(2);
            //渐隐动画 messageItem
            StartCoroutine(FadeCanvasGroup(item, 1f, 0f, 0.3f)); // Fade from 1 to 0 alpha over 0.5 seconds
            yield return new WaitForSeconds(0.5f); // Wait for the fade out to complete
            item.SetActive(false); // Deactivate the object after fading out
            CloseSelf();
        }

        /// <summary>
        /// 渐显动画
        /// </summary>
        /// <param name="gameObject"></param>
        private void FadeInMessage(GameObject gameObject)
        {
            gameObject.SetActive(true); // Make sure the object is active before fading in
            StartCoroutine(FadeCanvasGroup(gameObject, 0f, 1f, 0.3f)); // Fade from 0 to 1 alpha over 0.5 seconds
        }

        /// <summary>
        /// 通用的渐隐渐显的协程方法
        /// </summary>
        /// <param name="targetGameObject">需要渐隐渐显的GameObject</param>
        /// <param name="startAlpha">起始透明度</param>
        /// <param name="endAlpha">目标透明度</param>
        /// <param name="duration">渐变持续时间</param>
        /// <returns></returns>
        private IEnumerator FadeCanvasGroup(GameObject targetGameObject, float startAlpha, float endAlpha, float duration)
        {
            var canvasRenderers = targetGameObject.GetComponentsInChildren<CanvasRenderer>();

            if (canvasRenderers == null || canvasRenderers.Length == 0)
            {
                Debug.LogError("No CanvasRenderer found on target GameObject or its children!");
                yield break;
            }

            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(time / duration);
                var alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);

                foreach (var canvasRenderer in canvasRenderers)
                {
                    canvasRenderer.SetAlpha(alpha);
                }

                yield return null;
            }

            // Ensure the final alpha value is set
            foreach (var canvasRenderer in canvasRenderers)
            {
                canvasRenderer.SetAlpha(endAlpha);
            }
        }

        /// <summary>
        /// 清除消失计时器
        /// </summary>
        private void ClearStopCoroutine()
        {
            try
            {
                if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            }
            catch (Exception e)
            {
                Debug.LogWarning("ToastPanel ClearStopCoroutine Error: " + e.Message);
            }
        }

        public static void Show(string msg)
        {
            UIModule.S.OpenPanel(UIID.ToastPanel, r =>
            {
                var panel = r as ToastTMPPanel;
                if (panel == null) return;
                panel.toastText.text = msg;
            });
        }
    }
}
