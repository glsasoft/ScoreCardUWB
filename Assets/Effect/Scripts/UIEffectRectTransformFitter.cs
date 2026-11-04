using UnityEngine;

namespace LetsPiu
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UIEffectRectTransformFitter : MonoBehaviour
    {
        public Transform effectTransform;
        public Vector2 ratio = Vector2.one;
        
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (!effectTransform) return;
#if UNITY_EDITOR
            rectTransform = GetComponent<RectTransform>();
#endif
            Rect rect = rectTransform.rect;
            effectTransform.localScale = new Vector3(rect.width * ratio.x, rect.height * ratio.y, 1);
        }
    }
}
