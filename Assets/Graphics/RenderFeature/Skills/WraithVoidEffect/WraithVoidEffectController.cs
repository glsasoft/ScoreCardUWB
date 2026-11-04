using UnityEngine;

namespace LetsPiu {
    [ExecuteAlways]
    public class WraithVoidEffectController : MonoBehaviour {
        [Header("Effect Target")]
        public Material materialInstance;

        private static readonly int _Distortion = Shader.PropertyToID("_Distortion");
        private static readonly int _Scale = Shader.PropertyToID("_Scale");

        private static readonly int _Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int _Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int _Contrast = Shader.PropertyToID("_Contrast");
        private static readonly int _ColorReverse = Shader.PropertyToID("_ColorReverse");
        private static readonly int _Tint = Shader.PropertyToID("_Tint");

        private static readonly int _EdgeStart = Shader.PropertyToID("_EdgeStart");
        private static readonly int _EdgeFade = Shader.PropertyToID("_EdgeFade");
        private static readonly int _EdgeStrength = Shader.PropertyToID("_EdgeStrength");
        private static readonly int _EdgeColor = Shader.PropertyToID("_EdgeColor");

        private static readonly int _voidRange = Shader.PropertyToID("_voidRange");
        private static readonly int _voidFade = Shader.PropertyToID("_voidFade");

        private static readonly int _gridDensity = Shader.PropertyToID("_gridDensity");
        private static readonly int _gridLineWidth = Shader.PropertyToID("_gridLineWidth");
        private static readonly int _gridColor = Shader.PropertyToID("_gridColor");
        private static readonly int _gridRange = Shader.PropertyToID("_gridRange");
        private static readonly int _gridFade = Shader.PropertyToID("_gridFade");

        [Header("透镜扭曲")]
        [Range(-3f, 3f)] public float distortion = 0;
        [Range(0f, 2f)] public float scale = 1;

        [Header("颜色处理")]
        [Tooltip("亮度")]
        [Range(0f, 1f)] public float brightness = 0;
        [Tooltip("饱和度")]
        [Range(0f, 2f)] public float saturation = 1;
        [Tooltip("对比度")]
        [Range(0f, 2f)] public float contrast = 1;
        [Tooltip("颜色翻转")]
        [Range(0f, 1f)] public float colorReverse = 0.5f;
        [Tooltip("颜色叠加")]
        public Color tint = new Color(1,0.7f,0.4f,1);

        [Header("边缘泛光控制")]
        [Tooltip("模糊开始（0中心清晰）")]
        [Range(0f, 1f)] public float edgeStart = 0.5f;
        [Tooltip("模糊过渡宽度")]
        [Range(0f, 1f)] public float edgeFade = 0.5f;
        [Tooltip("边缘泛光强度")]
        [Range(0f, 1f)] public float edgeStrength = 0.8f;
        [Tooltip("边缘光颜色")]
        public Color edgeColor = new Color(1,0.7f,0.4f,1);

        [Header("虚空领域")]
        [Tooltip("虚空范围")]
        [Range(0f, 500f)] public float voidRange = 100f;
        [Tooltip("虚空衰减")]
        [Range(0f, 50f)] public float voidFade = 20f;

        [Header("网格设置")]
        [Tooltip("网格密度")]
        [Range(0f, 1.5f)] public float gridDensity = 0.5f;
        [Tooltip("网格线宽度")]
        [Range(0f, 0.2f)] public float gridLineWidth = 0.01f;
        [Tooltip("网格线颜色")]
        public Color gridColor = new Color(1,0.7f,0.4f,1);
        [Tooltip("网格范围")]
        [Range(0f, 15f)] public float gridRange = 5f;
        [Tooltip("网格衰减")]
        [Range(0f, 50f)] public float gridFade = 15f;

        private void Update() {
            if (materialInstance == null) return;

            // Distortion
            materialInstance.SetFloat(_Distortion, distortion);
            materialInstance.SetFloat(_Scale, scale);

            // Color
            materialInstance.SetFloat(_Brightness, brightness);
            materialInstance.SetFloat(_Saturation, saturation);
            materialInstance.SetFloat(_Contrast, contrast);
            materialInstance.SetFloat(_ColorReverse, colorReverse);
            materialInstance.SetColor(_Tint, tint);

            // Edge
            materialInstance.SetFloat(_EdgeStart, edgeStart);
            materialInstance.SetFloat(_EdgeFade, edgeFade);
            materialInstance.SetFloat(_EdgeStrength, edgeStrength);
            materialInstance.SetColor(_EdgeColor, edgeColor);

            // Void
            materialInstance.SetFloat(_voidRange, voidRange);
            materialInstance.SetFloat(_voidFade, voidFade);

            // Grid
            materialInstance.SetFloat(_gridDensity, gridDensity);
            materialInstance.SetFloat(_gridLineWidth, gridLineWidth);
            materialInstance.SetColor(_gridColor, gridColor);
            materialInstance.SetFloat(_gridRange, gridRange);
            materialInstance.SetFloat(_gridFade, gridFade);
        }
    }
}
