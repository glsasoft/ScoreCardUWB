using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Graphics
{
    public static class SpecialRenderingEffect
    {
        #region Shader Property ID

        static readonly int _FaceForwardDirection = Shader.PropertyToID("_FaceForwardDirection");
        static readonly int _FaceUpDirection = Shader.PropertyToID("_FaceUpDirection");


        static readonly int _DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        static readonly int _DissolveThresholdMap = Shader.PropertyToID("_DissolveThresholdMap");
        static readonly int _DissolveBorderRange = Shader.PropertyToID("_DissolveBorderRange");
        static readonly int _DissolveBorderTintColor = Shader.PropertyToID("_DissolveBorderTintColor");
        static readonly int _DissolveTopBorderRange = Shader.PropertyToID("_DissolveTopBorderRange");
        static readonly int _DissolveTopBorderTintColor = Shader.PropertyToID("_DissolveTopBorderTintColor");

        static readonly int _PerCharEffectRimColor = Shader.PropertyToID("_PerCharEffectRimColor");
        static readonly int _PerCharEffectRimSharpnessPower = Shader.PropertyToID("_PerCharEffectRimSharpnessPower");
        
        static readonly int _OutlineWidthExtraMultiplier = Shader.PropertyToID("_OutlineWidthExtraMultiplier");

        #endregion


        /// <summary>
        /// 设置面部材质参数，将面部朝前方向和朝上方向设置到材质中
        /// </summary>
        /// <param name="mats">面部材质列表</param>
        /// <param name="forwardDirection">朝前方向</param>
        /// <param name="upDirection">朝上方向</param>
        public static void SetFaceDirection(Material[] mats, Vector3 forwardDirection, Vector3 upDirection)
        {
            foreach (var VARIABLE in mats)
            {
                VARIABLE.SetVector(_FaceForwardDirection, forwardDirection);
                VARIABLE.SetVector(_FaceUpDirection, upDirection);
            }
        }

        /// <summary>
        /// 设置溶解材质参数，将溶解开关、方向阈值、映射阈值和整体颜色范围设置到材质中
        /// </summary>
        /// <param name="mats">需要设置溶解材质的材质列表</param>
        /// <param name="dissolveAmount">溶解强度</param>
        /// <param name="dissolveThresholdMap">溶解贴图</param>
        /// <param name="dissolveBorderRange">溶解边缘范围</param>
        /// <param name="dissolveBorderTintColor">溶解边缘颜色</param>
        /// <param name="dissolveTopBorderRange">溶解顶部边缘范围</param>
        /// <param name="dissolveTopBorderTintColor">溶解顶部边缘颜色</param>
        public static void SetDissolveMaterial(List<Material> mats, float dissolveAmount, Texture2D dissolveThresholdMap,
            float dissolveBorderRange, Color dissolveBorderTintColor, float dissolveTopBorderRange,
            Color dissolveTopBorderTintColor)
        {
            foreach (var VARIABLE in mats)
            {
                if (!VARIABLE.IsKeywordEnabled("_NILOTOON_DISSOLVE"))
                    VARIABLE.EnableKeyword("_NILOTOON_DISSOLVE");
                VARIABLE.SetFloat(_DissolveAmount, dissolveAmount);
                VARIABLE.SetTexture(_DissolveThresholdMap, dissolveThresholdMap);
                VARIABLE.SetFloat(_DissolveBorderRange, dissolveBorderRange);
                VARIABLE.SetColor(_DissolveBorderTintColor, dissolveBorderTintColor);
                VARIABLE.SetFloat(_DissolveTopBorderRange, dissolveTopBorderRange);
                VARIABLE.SetColor(_DissolveTopBorderTintColor, dissolveTopBorderTintColor);
            }
        }

        
        /// <summary>
        /// 清除溶解效果
        /// </summary>
        /// <param name="mats"></param>
        public static void RemoveDissolve(List<Material> mats)
        {
            foreach (var VARIABLE in mats)
            {
                VARIABLE.DisableKeyword("_NILOTOON_DISSOLVE");
            }
        }

        /// <summary>
        /// 设置边缘光
        /// </summary>
        /// <param name="mats"></param>
        /// <param name="perCharEffectRimColor">角色效果边缘光颜色</param>
        /// <param name="perCharEffectRimSharpnessPower">角色效果边缘光锐利度</param>
        public static void SetRimMaterial(List<Material> mats,Color perCharEffectRimColor ,float perCharEffectRimSharpnessPower)
        {
            foreach (var VARIABLE in mats)
            {
                VARIABLE.SetColor(_PerCharEffectRimColor, perCharEffectRimColor);
                VARIABLE.SetFloat(_PerCharEffectRimSharpnessPower, perCharEffectRimSharpnessPower);
            }
        }
        
        /// <summary>
        /// 清除边缘光效果
        /// </summary>
        /// <param name="mats"></param>
        public static void RemoveRim(List<Material> mats)
        {
            foreach (var VARIABLE in mats)
            {
                VARIABLE.SetColor(_PerCharEffectRimColor, Color.black);
                VARIABLE.SetFloat(_PerCharEffectRimSharpnessPower, 4f);
            }
        }
        
        /// <summary>
        /// 设置轮廓宽度额外乘数
        /// </summary>
        /// <param name="mats"></param>
        /// <param name="outlineWidth"></param>
        public static void SetOutlineWidthExtraMultiplier(List<Material> mats, float outlineWidthExtraMultiplier)
        {
            foreach (var VARIABLE in mats)
            {
                VARIABLE.SetFloat(_OutlineWidthExtraMultiplier, outlineWidthExtraMultiplier);
            }
        }
        
    }
}