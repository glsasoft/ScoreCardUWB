using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Room.Scripts.UI
{
    public class ScrollbarAutoHide : MonoBehaviour
    {
        public ScrollRect scrollRect;
        public UIScrollRect uIScrollRect;
        public CanvasGroup scrollbarCanvasGroup;
        public float fadeSpeed = 2f;
        private float hideTimer = 0f;
        private bool isScrolling = false;

        void Update()
        {
            if(uIScrollRect != null)
            {
                // 判断滚动速度
                if (uIScrollRect.velocity.magnitude > 0.1f)
                {
                    isScrolling = true;
                    hideTimer = 1f; // 设定隐藏倒计时
                }
                else if (isScrolling)
                {
                    hideTimer -= Time.deltaTime * fadeSpeed;
                    if (hideTimer <= 0)
                    {
                        isScrolling = false;
                    }
                }

                // 透明度变化
                scrollbarCanvasGroup.alpha = isScrolling ? 1 : Mathf.Lerp(scrollbarCanvasGroup.alpha, 0, Time.deltaTime * fadeSpeed);
            }
            if(scrollRect != null)
            {
                // 判断滚动速度
                if (scrollRect.velocity.magnitude > 0.1f)
                {
                    isScrolling = true;
                    hideTimer = 1f; // 设定隐藏倒计时
                }
                else if (isScrolling)
                {
                    hideTimer -= Time.deltaTime * fadeSpeed;
                    if (hideTimer <= 0)
                    {
                        isScrolling = false;
                    }
                }

                // 透明度变化
                scrollbarCanvasGroup.alpha = isScrolling ? 1 : Mathf.Lerp(scrollbarCanvasGroup.alpha, 0, Time.deltaTime * fadeSpeed);
            }
            
        }
    }
}