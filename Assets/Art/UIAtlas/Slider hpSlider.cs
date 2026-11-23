using UnityEngine;
using UnityEngine.UI;

public class CHpBar : MonoBehaviour
{
    private Slider hpSlider;
    private RectTransform rectTrans;
    private RectTransform parentRectTrans;
    private Canvas parentCanvas;
    private Collider targetCollider;
    private Camera mainCam;

    public Transform target;
    public Vector3 offsetPos = new Vector3(0f, 0.3f, 0f); // 头顶偏移量

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

        // 初始化默认满血
        UpdateHp(1f);
    }

    /// <summary>
    /// 绑定或切换追踪目标（适合对象池复用）
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
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
            // 目标已销毁时隐藏或移出屏幕
            rectTrans.anchoredPosition = Vector2.up * 3000f;
            return;
        }

        if (mainCam == null) mainCam = Camera.main;

        // 1. 计算头顶世界坐标（有 Collider 取包围盒顶部，否则取物体原点）
        Vector3 topAnchor = targetCollider != null
            ? new Vector3(targetCollider.bounds.center.x, targetCollider.bounds.max.y, targetCollider.bounds.center.z)
            : target.position;

        Vector3 worldPos = topAnchor + offsetPos;

        // 2. 判断是否在摄像机视野内
        Vector3 viewPos = mainCam.WorldToViewportPoint(worldPos);
        bool isVisible = viewPos.z > 0f && viewPos.x >= 0f && viewPos.x <= 1f && viewPos.y >= 0f && viewPos.y <= 1f;

        if (isVisible)
        {
            Vector2 screenPos = mainCam.WorldToScreenPoint(worldPos);

            // 兼容 Overlay 和 Screen Space - Camera 两种 Canvas 模式
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay && parentRectTrans != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRectTrans, screenPos, parentCanvas.worldCamera, out Vector2 localPos);
                rectTrans.anchoredPosition = localPos;
            }
            else
            {
                rectTrans.position = screenPos;
            }
        }
        else
        {
            // 不在可视窗口的模型，移动到视线外
            rectTrans.anchoredPosition = Vector2.up * 3000f;
        }
    }
}
