using UnityEngine;
using UnityEngine.EventSystems;

public class ImageDragger : MonoBehaviour, IBeginDragHandler, IDragHandler,IScrollHandler,IPointerDownHandler, IPointerUpHandler
{
    //需要裁切的图片RectTransform
    private RectTransform rectTransform;
    private Vector2 lastMousePosition;
    //裁切框RectTransform
    public RectTransform cropArea; 

    private bool isDragging = false;
    private bool isPinching = false;
    private float initialPinchDistance;
    private Vector3 initialScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerId);
        if (eventData.pointerId == 0) // 单指按下
        {
            lastMousePosition = eventData.position;
            isDragging = true;
        }
        else if (eventData.pointerId == 1) // 双指按下
        {
            initialPinchDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            initialScale = rectTransform.localScale;
            isPinching = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == 0) //单指抬起
        {
            isDragging = false;
        }
        else if (eventData.pointerId == 1) //双指抬起
        {
            isPinching = false;
        }
        var newpos = rectTransform.anchoredPosition;
        if (newpos.x > (rectTransform.sizeDelta.x * rectTransform.localScale.x - cropArea.sizeDelta.x) / 2)
        {
            newpos.x = (rectTransform.sizeDelta.x * rectTransform.localScale.x - cropArea.sizeDelta.x) / 2;
        }
        if (newpos.x < (-rectTransform.sizeDelta.x * rectTransform.localScale.x + cropArea.sizeDelta.x) / 2)
        {
            newpos.x = (-rectTransform.sizeDelta.x * rectTransform.localScale.x + cropArea.sizeDelta.x) / 2;
        }
        if (newpos.y > (rectTransform.sizeDelta.y * rectTransform.localScale.y - cropArea.sizeDelta.y) / 2)
        {
            newpos.y = (rectTransform.sizeDelta.y * rectTransform.localScale.y - cropArea.sizeDelta.y) / 2;
        }
        if (newpos.y < (-rectTransform.sizeDelta.y * rectTransform.localScale.y + cropArea.sizeDelta.y) / 2)
        {
            newpos.y = (-rectTransform.sizeDelta.y * rectTransform.localScale.y + cropArea.sizeDelta.y) / 2;
        }
        rectTransform.anchoredPosition = newpos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isPinching && eventData.pointerId == 0) // 开始拖动记录位置
        {
            lastMousePosition = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPinching)
        {
            if (Input.touchCount == 2)//双指放大缩小
            {
                float currentPinchDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                float scaleFactor = currentPinchDistance / initialPinchDistance;
                Vector3 scale = initialScale * scaleFactor;
                if (rectTransform.sizeDelta.x < rectTransform.sizeDelta.y)
                {
                    if (scale.x * rectTransform.sizeDelta.x > cropArea.sizeDelta.x)
                    {
                        rectTransform.localScale = scale;
                    }
                }
                else
                {
                    if (scale.y * rectTransform.sizeDelta.y > cropArea.sizeDelta.y)
                    {
                        rectTransform.localScale = scale;
                    }
                }
            }
        }
        else if (isDragging && eventData.pointerId == 0) //单指移动图片
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - lastMousePosition;
            var newpos = rectTransform.anchoredPosition;
            newpos += diff;
            if (newpos.x > (rectTransform.sizeDelta.x * rectTransform.localScale.x - cropArea.sizeDelta.x) / 2)
            {
                newpos.x = (rectTransform.sizeDelta.x * rectTransform.localScale.x - cropArea.sizeDelta.x) / 2;
            }
            if (newpos.x < (-rectTransform.sizeDelta.x * rectTransform.localScale.x + cropArea.sizeDelta.x) / 2)
            {
                newpos.x = (-rectTransform.sizeDelta.x * rectTransform.localScale.x + cropArea.sizeDelta.x) / 2;
            }
            if (newpos.y > (rectTransform.sizeDelta.y * rectTransform.localScale.y - cropArea.sizeDelta.y) / 2)
            {
                newpos.y = (rectTransform.sizeDelta.y * rectTransform.localScale.y - cropArea.sizeDelta.y) / 2;
            }
            if (newpos.y < (-rectTransform.sizeDelta.y * rectTransform.localScale.y + cropArea.sizeDelta.y) / 2)
            {
                newpos.y = (-rectTransform.sizeDelta.y * rectTransform.localScale.y + cropArea.sizeDelta.y) / 2;
            }
            rectTransform.anchoredPosition = newpos;
            lastMousePosition = currentMousePosition;
        }
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    lastMousePosition = eventData.position;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    Vector2 currentMousePosition = eventData.position;
    //    Vector2 diff = currentMousePosition - lastMousePosition;
    //    var newpos = rectTransform.anchoredPosition;
    //    newpos += diff;
    //    if (newpos.x > (rectTransform.sizeDelta.x * rectTransform.localScale.x - cropArea.sizeDelta.x) / 2)
    //    {
    //        newpos.x = (rectTransform.sizeDelta.x * rectTransform.localScale.x - cropArea.sizeDelta.x) / 2;
    //    }
    //    if (newpos.x < (-rectTransform.sizeDelta.x * rectTransform.localScale.x + cropArea.sizeDelta.x) / 2)
    //    {
    //        newpos.x = (-rectTransform.sizeDelta.x * rectTransform.localScale.x + cropArea.sizeDelta.x) / 2;
    //    }
    //    if (newpos.y > (rectTransform.sizeDelta.y * rectTransform.localScale.y - cropArea.sizeDelta.y) / 2)
    //    {
    //        newpos.y = (rectTransform.sizeDelta.y * rectTransform.localScale.y - cropArea.sizeDelta.y) / 2;
    //    }
    //    if (newpos.y < (-rectTransform.sizeDelta.y * rectTransform.localScale.y +cropArea.sizeDelta.y) / 2)
    //    {
    //        newpos.y = (-rectTransform.sizeDelta.y * rectTransform.localScale.y + cropArea.sizeDelta.y) / 2;
    //    }
    //    rectTransform.anchoredPosition = newpos;
    //    lastMousePosition = currentMousePosition;
    //}

    public void OnScroll(PointerEventData eventData)
    {
        float scrollDelta = eventData.scrollDelta.y;
        Vector3 scale = rectTransform.localScale;
        scale += Vector3.one * scrollDelta * 0.1f;
        scale = Vector3.Max(scale, Vector3.one * 0.1f); 
        if(rectTransform.sizeDelta.x< rectTransform.sizeDelta.y)
        {
            if(scale.x* rectTransform.sizeDelta.x>cropArea.sizeDelta.x)
            {
                rectTransform.localScale = scale;
            }
        }
        else
        {
            if (scale.y * rectTransform.sizeDelta.y > cropArea.sizeDelta.y)
            {
                rectTransform.localScale = scale;
            }
        }
        
    }
}
