/*
一个发光文字效果的TMP组件
可设置发光颜色，以及发光范围参数
*/

using TMPro;
using UnityEngine;

namespace LetsPiu.UI.UIScript.Common.UI
{
    /// <summary>
    /// 发光文字效果的TextMeshPro组件
    /// 支持自定义发光颜色和发光范围
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GlowingTMP : MonoBehaviour
    {
        [Header("发光设置")]
        [SerializeField]
        [Tooltip("发光颜色")]
        private Color glowColor = new Color(1f, 1f, 0f, 1f); // 默认黄色
        
        [SerializeField]
        [Range(0f, 2f)]
        [Tooltip("发光强度（控制发光的亮度）")]
        private float glowIntensity = 1f;
        
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("发光扩散距离（0=无扩散，1=最大扩散）")]
        private float glowSpread = 0.5f;
        
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("发光柔和度（控制边缘渐变的平滑程度）")]
        private float glowSoftness = 0.5f;
        
        [SerializeField]
        [Tooltip("是否启用发光效果")]
        private bool enableGlow = true;

        private TextMeshProUGUI textComponent;
        private Material materialInstance;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (textComponent != null && enableGlow)
            {
                ApplyGlowEffect();
            }
        }

        private void OnValidate()
        {
            // 编辑器中实时预览效果
            if (textComponent == null)
            {
                textComponent = GetComponent<TextMeshProUGUI>();
            }

            if (textComponent != null)
            {
                // 在编辑器模式下也需要创建材质实例
                if (materialInstance == null && textComponent.fontSharedMaterial != null)
                {
                    materialInstance = new Material(textComponent.fontSharedMaterial);
                    textComponent.fontMaterial = materialInstance;
                }
                
                ApplyGlowEffect();
            }
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void Initialize()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            
            if (textComponent != null)
            {
                // 创建材质实例，避免影响其他文本
                if (textComponent.fontSharedMaterial != null)
                {
                    materialInstance = new Material(textComponent.fontSharedMaterial);
                    textComponent.fontMaterial = materialInstance;
                }
                
                ApplyGlowEffect();
            }
            else
            {
                Debug.LogError("GlowingTMP: 未找到TextMeshProUGUI组件！");
            }
        }

        /// <summary>
        /// 应用发光效果
        /// </summary>
        private void ApplyGlowEffect()
        {
            if (textComponent == null || materialInstance == null) return;

            if (enableGlow)
            {
                // 使用Underlay实现真正的发光散射效果
                materialInstance.EnableKeyword("UNDERLAY_ON");
                
                // 设置底层颜色为发光颜色，并根据强度调整alpha
                Color underlayColor = glowColor;
                underlayColor.a *= glowIntensity;
                materialInstance.SetColor("_UnderlayColor", underlayColor);
                
                // 使用Dilate创建扩散效果
                // glowSpread越大，扩散距离越远
                float dilate = Mathf.Lerp(-1f, 1f, glowSpread);
                materialInstance.SetFloat("_UnderlayDilate", dilate);
                
                // 使用Softness创建柔和的边缘渐变
                // glowSoftness越大，边缘越模糊，越像散射光
                float softness = Mathf.Lerp(0f, 1f, glowSoftness);
                materialInstance.SetFloat("_UnderlaySoftness", softness);
                
                // 设置偏移为0，让发光从字体边缘开始
                materialInstance.SetFloat("_UnderlayOffsetX", 0f);
                materialInstance.SetFloat("_UnderlayOffsetY", 0f);
                
                // 如果材质有FaceDilate属性，可以稍微扩展字体本身
                if (materialInstance.HasProperty("_FaceDilate"))
                {
                    materialInstance.SetFloat("_FaceDilate", 0.1f);
                }
                
                // 禁用Glow和Outline以避免冲突
                materialInstance.DisableKeyword("GLOW_ON");
                materialInstance.DisableKeyword("OUTLINE_ON");
            }
            else
            {
                // 禁用所有效果
                materialInstance.DisableKeyword("UNDERLAY_ON");
                materialInstance.DisableKeyword("GLOW_ON");
                materialInstance.DisableKeyword("OUTLINE_ON");
            }
        }

        /// <summary>
        /// 设置发光颜色
        /// </summary>
        public void SetGlowColor(Color color)
        {
            glowColor = color;
            ApplyGlowEffect();
        }

        /// <summary>
        /// 设置发光强度
        /// </summary>
        public void SetGlowIntensity(float intensity)
        {
            glowIntensity = Mathf.Clamp(intensity, 0f, 2f);
            ApplyGlowEffect();
        }

        /// <summary>
        /// 设置发光扩散距离
        /// </summary>
        public void SetGlowSpread(float spread)
        {
            glowSpread = Mathf.Clamp01(spread);
            ApplyGlowEffect();
        }

        /// <summary>
        /// 设置发光柔和度
        /// </summary>
        public void SetGlowSoftness(float softness)
        {
            glowSoftness = Mathf.Clamp01(softness);
            ApplyGlowEffect();
        }

        /// <summary>
        /// 启用/禁用发光效果
        /// </summary>
        public void SetGlowEnabled(bool enabled)
        {
            enableGlow = enabled;
            ApplyGlowEffect();
        }

        /// <summary>
        /// 获取当前发光颜色
        /// </summary>
        public Color GetGlowColor()
        {
            return glowColor;
        }

        /// <summary>
        /// 获取当前发光强度
        /// </summary>
        public float GetGlowIntensity()
        {
            return glowIntensity;
        }

        /// <summary>
        /// 获取当前发光扩散距离
        /// </summary>
        public float GetGlowSpread()
        {
            return glowSpread;
        }

        /// <summary>
        /// 获取当前发光柔和度
        /// </summary>
        public float GetGlowSoftness()
        {
            return glowSoftness;
        }

        private void OnDestroy()
        {
            // 清理材质实例
            if (materialInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(materialInstance);
                }
                else
                {
                    DestroyImmediate(materialInstance);
                }
            }
        }
    }
}
