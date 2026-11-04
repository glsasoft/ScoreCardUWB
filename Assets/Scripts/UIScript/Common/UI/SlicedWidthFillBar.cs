using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI
{
    /// <summary>
    /// 通过缩放前景宽度实现进度，保留九宫格圆角（不裁切）。
    /// 前景需 Image Type = Sliced，Pivot 建议在左侧。
    /// </summary>
    public class SlicedWidthFillBar : MonoBehaviour
    {
        [SerializeField] RectTransform fillRect;
        [SerializeField] float fullWidth = -1f;
        [SerializeField] [Range(0f, 1f)] float percent = 1f;
        [SerializeField] float minWidth;

        public float Percent
        {
            get => percent;
            set => SetPercent(value);
        }

        void Awake()
        {
            CaptureFullWidthIfNeeded();
            Apply();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            CaptureFullWidthIfNeeded();
            Apply();
        }
#endif

        public void SetPercent(float value)
        {
            percent = Mathf.Clamp01(value);
            Apply();
        }

        void CaptureFullWidthIfNeeded()
        {
            if (fillRect == null) return;
            if (fullWidth < 0f)
            {
                fullWidth = fillRect.sizeDelta.x;
            }
        }

        void Apply()
        {
            if (fillRect == null) return;

            CaptureFullWidthIfNeeded();
            if (fullWidth <= 0f) return;

            float width = fullWidth * percent;
            if (minWidth > 0f && percent > 0f)
            {
                width = Mathf.Max(width, minWidth);
            }

            var size = fillRect.sizeDelta;
            size.x = width;
            fillRect.sizeDelta = size;
        }
    }
}
