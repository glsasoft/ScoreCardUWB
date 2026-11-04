using UnityEngine;

namespace LetsPiu.Graphics
{
    [ExecuteAlways]
    public class SSFogController : MonoBehaviour
    {
        [Header("Material")]
        public Material fogMaterial;

        [Header("Noise Textures")]
        public Texture2D noiseTex1;
        public Texture2D noiseTex2;

        [Header("Fog Colors")]
        public Color fogColor1 = new Color(1, 1, 1, 1);
        public Color fogColor2 = new Color(1, 1, 1, 1);
        [Range(0f, 5f)] public float fogDensity = 1f;

        [Header("Height Control")]
        public float fogMinHeight = 0f;
        public float fogMaxHeight = 2f;
        [Range(0.01f, 5f)] public float fogFadeOut = 1f;

        [Header("Fog Speed")]
        public Vector2 fogSpeed1 = new Vector2(0.2f, 0.0f);
        public Vector2 fogSpeed2 = new Vector2(0.4f, 0.0f);

        [Header("Near Fade Control")]
        public float fogNear1 = 5f;
        [Range(0.01f, 5f)] public float fogNearFadeOut1 = 1f;
        public float fogNear2 = 7f;
        [Range(0.01f, 5f)] public float fogNearFadeOut2 = 1f;
        
        private static readonly int NoiseTex1 = Shader.PropertyToID("_NoiseTex1");
        private static readonly int NoiseTex2 = Shader.PropertyToID("_NoiseTex2");
        private static readonly int FogColor1 = Shader.PropertyToID("_FogColor1");
        private static readonly int FogColor2 = Shader.PropertyToID("_FogColor2");
        private static readonly int FogMinHeight = Shader.PropertyToID("_FogMinHeight");
        private static readonly int FogMaxHeight = Shader.PropertyToID("_FogMaxHeight");
        private static readonly int FogFadeOut = Shader.PropertyToID("_FogFadeOut");
        private static readonly int FogSpeed = Shader.PropertyToID("_FogSpeed");
        private static readonly int FogDensity = Shader.PropertyToID("_FogDensity");
        private static readonly int FogNear1 = Shader.PropertyToID("_FogNear1");
        private static readonly int FogNearFadeOut1 = Shader.PropertyToID("_FogNearFadeOut1");
        private static readonly int FogNear2 = Shader.PropertyToID("_FogNear2");
        private static readonly int FogNearFadeOut2 = Shader.PropertyToID("_FogNearFadeOut2");

        void Update()
        {
            if (fogMaterial == null) return;

            // 设置噪声贴图
            if (noiseTex1) fogMaterial.SetTexture(NoiseTex1, noiseTex1);
            if (noiseTex2) fogMaterial.SetTexture(NoiseTex2, noiseTex2);

            // 基础参数
            fogMaterial.SetColor(FogColor1, fogColor1);
            fogMaterial.SetColor(FogColor2, fogColor2);
            fogMaterial.SetFloat(FogMinHeight, fogMinHeight);
            fogMaterial.SetFloat(FogMaxHeight, fogMaxHeight);
            fogMaterial.SetFloat(FogFadeOut, fogFadeOut);
            
            Vector4 fogSpeed = new Vector4(fogSpeed1.x, fogSpeed1.y, fogSpeed2.x, fogSpeed2.y);
            fogMaterial.SetVector(FogSpeed, fogSpeed);
            fogMaterial.SetFloat(FogDensity, fogDensity);

            fogMaterial.SetFloat(FogNear1, fogNear1);
            fogMaterial.SetFloat(FogNearFadeOut1, fogNearFadeOut1);
            fogMaterial.SetFloat(FogNear2, fogNear2);
            fogMaterial.SetFloat(FogNearFadeOut2, fogNearFadeOut2);
        }
    }
}
