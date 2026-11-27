using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色头顶血条。血条底边钉在目标头顶的屏幕投影上，偏移分成世界空间和屏幕像素两套。
/// </summary>
[DisallowMultipleComponent]
public class CHpBar : MonoBehaviour
{
    private Slider hpSlider;
    private RectTransform rectTrans;
    private RectTransform parentRectTrans;
    private Canvas parentCanvas;
    private Collider targetCollider;
    private Camera mainCam;

    [Header("追踪目标")]
    [Tooltip("追踪的角色根节点。绑定后自动取它身上的 Collider 包围盒顶部")]
    public Transform target;

    [Tooltip("头顶挂点，骨骼或空物体。填了优先用它；留空取 Collider 包围盒顶部，再留空取 target 原点")]
    public Transform headAnchor;

    [Header("偏移")]
    [Tooltip("世界空间偏移，投影到屏幕后随相机距离缩放。只做微调，要固定像素高度用 offsetPixels")]
    public Vector3 offsetPos = Vector3.zero;

    [Tooltip("屏幕像素偏移，正上方填 (0, N)。与透视和距离无关，调血条高度用这个")]
    public Vector2 offsetPixels = Vector2.zero;

    [Header("离屏")]
    [Tooltip("目标离开视野时血条移到的 y 坐标，保持在屏幕外")]
    public float offScreenY = -10000f;

    private void Awake()
    {
        hpSlider = GetComponent<Slider>();
        rectTrans = GetComponent<RectTransform>();
        parentRectTrans = rectTrans.parent as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
        mainCam = Camera.main;
    }

    private void Start()
    {
        if (target != null)
        {
            SetTarget(target);
        }

        UpdateHp(1f);
    }

    /// <summary>
    /// 绑定或切换追踪目标（适合对象池复用）
    /// </summary>
    public void SetTarget(Transform newTarget, Transform newHeadAnchor = null)
    {
        target = newTarget;
        headAnchor = newHeadAnchor;
        targetCollider = target != null ? target.GetComponent<Collider>() : null;
    }

    /// <summary>
    /// 更新血量（传入 0 ~ 1 的比例）
    /// </summary>
    public void UpdateHp(float hpPercent)
    {
        if (hpSlider != null)
        {
            hpSlider.value = Mathf.Clamp01(hpPercent);
        }
    }

    /// <summary>
    /// 更新血量（传入当前血量与最大血量）
    /// </summary>
    public void UpdateHp(float currentHp, float maxHp)
    {
        float percent = maxHp > 0f ? currentHp / maxHp : 0f;
        UpdateHp(percent);
    }

    // 使用 LateUpdate 确保角色移动完成后再更新 UI 位置，避免抖动
    private void LateUpdate()
    {
        if (target == null)
        {
            Hide();
            return;
        }

        if (mainCam == null) mainCam = Camera.main;

        if (mainCam == null)
        {
            Hide();
            return;
        }

        // 1. 头顶世界坐标
        Vector3 worldPos = GetAnchorWorldPosition() + offsetPos;

        // 2. 视野内外
        Vector3 viewPos = mainCam.WorldToViewportPoint(worldPos);
        bool isVisible = viewPos.z > 0f && viewPos.x >= 0f && viewPos.x <= 1f && viewPos.y >= 0f && viewPos.y <= 1f;

        if (!isVisible)
        {
            Hide();
            return;
        }

        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        // 3. Canvas 缺失时退化成世界坐标直接对齐屏幕像素
        if (parentCanvas == null || parentRectTrans == null)
        {
            rectTrans.position = new Vector3(screenPos.x + offsetPixels.x, screenPos.y + offsetPixels.y, 0f);
            return;
        }

        // Overlay 模式下 UI 世界坐标与屏幕像素 1:1，像素偏移直接相加即可
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rectTrans.position = new Vector3(screenPos.x + offsetPixels.x, screenPos.y + offsetPixels.y, 0f);
            return;
        }

        // ScreenSpaceCamera / WorldSpace：先换算到父节点的局部坐标，再把像素偏移折成 UI 单位
        Camera uiCamera = parentCanvas.worldCamera != null ? parentCanvas.worldCamera : mainCam;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTrans, screenPos, uiCamera, out Vector2 localPoint))
        {
            Hide();
            return;
        }

        Rect parentRect = parentRectTrans.rect;
        Vector2 uiPerPixel = new Vector2(
            parentRect.width > 0f ? parentRect.width / Mathf.Max(1f, Screen.width) : 1f,
            parentRect.height > 0f ? parentRect.height / Mathf.Max(1f, Screen.height) : 1f);

        // anchoredPosition 相对 anchor 参考点，localPoint 相对父节点 pivot，两者差一个 anchor 偏移
        Vector2 anchorRef = new Vector2(
            Mathf.Lerp(parentRect.xMin, parentRect.xMax, rectTrans.anchorMin.x),
            Mathf.Lerp(parentRect.yMin, parentRect.yMax, rectTrans.anchorMin.y));

        Vector2 pixelOffsetInUI = new Vector2(offsetPixels.x * uiPerPixel.x, offsetPixels.y * uiPerPixel.y);
        rectTrans.anchoredPosition = localPoint - anchorRef + pixelOffsetInUI;
    }

    private Vector3 GetAnchorWorldPosition()
    {
        if (headAnchor != null)
        {
            return headAnchor.position;
        }

        if (targetCollider != null)
        {
            Bounds bounds = targetCollider.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        return target.position;
    }

    private void Hide()
    {
        rectTrans.position = new Vector3(0f, offScreenY, 0f);
    }
}
