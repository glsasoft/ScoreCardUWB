using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI
{
    /// <summary>
    /// 极简加载进度条（从左到右填充）。前景不依赖图片，通过拉伸右边缘表示进度。
    /// </summary>
    public class LoadingProgressBar : MonoBehaviour
    {
        [SerializeField] private Image progressBarBg;
        [SerializeField] private Image progressBarFg;

        private void Awake()
        {
            if (progressBarFg)
            {
                progressBarFg.type = Image.Type.Simple;
            }

            SetProgress(0f);
        }

        public void SetProgress(float progress)
        {
            if (progressBarFg == null) return;

            var rect = progressBarFg.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
