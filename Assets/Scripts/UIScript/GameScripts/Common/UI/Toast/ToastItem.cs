using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastItem : MonoBehaviour
{
    private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.92f);
    private const float TopMargin = 56f;

    [Header("引用")]
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    private Tween fadeTween;
    private void Reset()
    {
        toastText = GetComponentInChildren<TMP_Text>();
        backgroundImage = GetComponentInChildren<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    /// <summary>
    /// 【备用--弹出多条飘字】
    /// </summary> 
    // public void Init(string msg, float fadeIn, float life, float fadeOut)
    // {
    //     toastText.text = msg;
    //     FitBackgroundToText();

    //     fadeTween?.Kill();
    //     canvasGroup.alpha = 0;
    //     rectTransform.anchoredPosition = Vector2.zero;
    //     var seq = DOTween.Sequence();
    //     seq.Append(canvasGroup.DOFade(1f, fadeIn)) // 淡入
    //        .AppendInterval(life)                 // 停留
    //        .Append(canvasGroup.DOFade(0f, fadeOut)) // 淡出
    //        .OnComplete(() => ToastItemPool.Instance.RecycleItem(gameObject))
    //        .SetUpdate(true); // 忽略 timeScale
    // }
    public void Init(string msg, float fadeIn, float life, float fadeOut)
    {
        // 1. 强制杀掉所有可能残留的动画
        fadeTween?.Kill();
        DOTween.Kill(this);          // 保险：杀掉该对象上所有 Tweens

        // 2. 重置状态
        toastText.text = msg;
        canvasGroup.alpha = 0;
        ApplyTopLayout();
        ApplyVisualStyle();
        FitBackgroundToText();

        // 3. 创建全新动画序列
        var seq = DOTween.Sequence();
        seq.SetId(this);             // 方便后续 Kill(this)
        seq.Append(canvasGroup.DOFade(1f, fadeIn))
           .AppendInterval(life)
           .Append(canvasGroup.DOFade(0f, fadeOut))
           .OnComplete(() => ToastItemPool.Instance.RecycleItem(gameObject))
           .SetUpdate(true);
        fadeTween = seq;            // 保存引用
    }
    private void ApplyTopLayout()
    {
        if (rectTransform == null) return;
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot     = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -TopMargin);
    }

    private void ApplyVisualStyle()
    {
        if (backgroundImage != null)
            backgroundImage.color = BackgroundColor;
        if (toastText != null)
            toastText.color = Color.white;
    }

    private void FitBackgroundToText()
    {
        if (toastText == null || backgroundImage == null) return;

        float textWidth = toastText.preferredWidth;
        float textHeight = toastText.preferredHeight;
        var bgRt = backgroundImage.rectTransform;
        bgRt.sizeDelta = new Vector2(textWidth + 60f, Mathf.Max(textHeight + 24f, bgRt.sizeDelta.y));
        var textRt = toastText.rectTransform;
        textRt.anchorMin = textRt.anchorMax = textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = bgRt.sizeDelta;
        textRt.anchoredPosition = Vector2.zero;
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
    }
}