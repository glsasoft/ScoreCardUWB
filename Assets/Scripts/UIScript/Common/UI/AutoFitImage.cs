using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 自动适配Image组件，宽度填充，高度等比例缩放，超出部分居中裁剪
    /// 需要挂载在一个带有Mask或RectMask2D的父对象下的子Image对象上
    /// </summary>
    [RequireComponent(typeof(Image))]
    [ExecuteAlways]
    public class AutoFitImage : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private Sprite _lastSprite;
        private Vector2 _lastParentSize;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            
            if (transform.parent != null)
            {
                _parentRectTransform = transform.parent.GetComponent<RectTransform>();
                
                // 确保父对象有RectMask2D组件用于裁剪
                if (_parentRectTransform != null && _parentRectTransform.GetComponent<RectMask2D>() == null)
                {
                    _parentRectTransform.gameObject.AddComponent<RectMask2D>();
                }
            }
        }

        private void Start()
        {
            UpdateImageFit();
        }

        private void Update()
        {
            // 检测Sprite或父RectTransform大小是否变化
            Vector2 currentParentSize = _parentRectTransform != null ? _parentRectTransform.rect.size : Vector2.zero;
            if (_image.sprite != _lastSprite || currentParentSize != _lastParentSize)
            {
                UpdateImageFit();
            }
        }

        /// <summary>
        /// 更新Image的适配
        /// </summary>
        public void UpdateImageFit()
        {
            if (_image == null || _rectTransform == null)
            {
                Initialize();
            }

            if (_image == null || _image.sprite == null || _parentRectTransform == null)
                return;

            _lastSprite = _image.sprite;
            _lastParentSize = _parentRectTransform.rect.size;

            // 获取父容器的尺寸（实际显示区域）
            float containerWidth = _parentRectTransform.rect.width;
            float containerHeight = _parentRectTransform.rect.height;

            // 获取Sprite的原始尺寸
            Rect spriteRect = _image.sprite.rect;
            float spriteWidth = spriteRect.width;
            float spriteHeight = spriteRect.height;

            if (spriteWidth <= 0 || spriteHeight <= 0 || containerWidth <= 0 || containerHeight <= 0)
                return;

            // 计算宽度填充时的缩放比例
            float scale = containerWidth / spriteWidth;

            // 计算缩放后的实际显示尺寸
            float scaledWidth = spriteWidth * scale;
            float scaledHeight = spriteHeight * scale;

            // 设置RectTransform的尺寸（可能超出父容器）
            _rectTransform.sizeDelta = new Vector2(scaledWidth, scaledHeight);

            // 居中对齐
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;

            // 设置Image属性
            _image.preserveAspect = false;
            _image.type = Image.Type.Simple;
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateImageFit();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Initialize();
            UpdateImageFit();
        }
#endif
    }
}
