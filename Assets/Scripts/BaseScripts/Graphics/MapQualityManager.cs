using System.Collections.Generic;
using LetsPiu.Base;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LetsPiu.Graphics
{
    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }
    public class MapQualityManager : MonoBehaviour
    {
        // 单例
        public static MapQualityManager Instance { get; private set; }

        public QualityLevel currentQuality = QualityLevel.Medium;
        private QualityLevel currentFlag;

        [Header("相机设置")] public Camera mainCamera;
        public UniversalAdditionalCameraData cameraData;

        [Header("管线设置")] public UniversalRenderPipelineAsset urpAsset;

        [Header("Bloom设置")] public Volume postProcessingVolume; // Bloom所在的Volume
        public Bloom bloomComponent;

        public ScriptableRendererFeature tonemappingRendererFeature;
        public ScriptableRendererFeature piuFinalBlitRendererFeature;

        public GameObject quadDecal;

        [Header("LOD")]
        public CameraBasedLODController cameraBasedLODController;

        [Header("画质分级")]
        public MapMaterialGroupProfile mapMaterialGroupProfile;


        public GameObject scenePrefab;
        public string matPath;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // 避免重复实例
                return;
            }
            Instance = this;

        }
        void Start()
        {
            Init();
            currentFlag = currentQuality;
            ApplyQualitySettings(currentQuality);
        }

        private void Update()
        {
            if (currentFlag != currentQuality)
            {
                currentFlag = currentQuality;
                SetQualityLevel(currentQuality);
            }
        }

        private void Init()
        {
            postProcessingVolume = FindObjectOfType<Volume>();
            postProcessingVolume.profile.TryGet(out bloomComponent);

            urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            cameraBasedLODController = FindObjectOfType<CameraBasedLODController>();

            mainCamera = CameraMgr.S ? CameraMgr.S.mainCamera : Camera.main;
        }

        // 外部接口
        public void SetQualityLevel(QualityLevel level)
        {
            Debug.Log($"当前画质：{level}");
            currentQuality = level;
            ApplyQualitySettings(level);
        }

        private void ApplyQualitySettings(QualityLevel level)
        {
            ApplyFPS(level); // 帧数
            ApplyResolution(level); // 分辨率
            ApplyBloom(level); // Bloom
            ApplyAntiAliasing(level); // 抗锯齿
            ApplyToneMapping(level); // ToneMapping
            ApplyMaterials(level);// 贴图分级+shader分级
            ApplyQuadDecal(level);// Mesh贴花
            ApplyLOD(level);
        }

        private void ApplyLOD(QualityLevel level)
        {
            if (cameraBasedLODController == null)
                return;

            List<float> thresholds = cameraBasedLODController.screenSizeThresholds;
            thresholds.Clear();

            switch (level)
            {
                case QualityLevel.Low:
                    thresholds.AddRange(new[] { 140f, 90f, 2f });
                    break;
                case QualityLevel.Medium:
                    thresholds.AddRange(new[] { 90f, 70f, 2f });
                    break;
                case QualityLevel.High:
                    thresholds.AddRange(new[] { 70f, 45f, 2f });
                    break;
            }
        }

        private void ApplyFPS(QualityLevel level)
        {
            //switch (level)
            //{
            //    case QualityLevel.Low:
            //        Application.targetFrameRate = 30;
            //        break;
            //    case QualityLevel.Medium:
            //        Application.targetFrameRate = 60;
            //        break;
            //    case QualityLevel.High:
            //        Application.targetFrameRate = 60;
            //        break;
            //}
        }

        private void ApplyResolution(QualityLevel level)
        {
            if (urpAsset == null) return;
            float res = level switch
            {
                QualityLevel.Low => 0.4f,
                QualityLevel.Medium => 0.7f,
                QualityLevel.High => 1.0f,
                _ => 0.6f
            };
            urpAsset.renderScale = res;
        }

        private void ApplyBloom(QualityLevel level)
        {
            if (bloomComponent == null) return;

            switch (level)
            {
                case QualityLevel.Low:
                    bloomComponent.active = false;
                    break;
                case QualityLevel.Medium:
                    bloomComponent.active = true;
                    bloomComponent.intensity.Override(6);
                    bloomComponent.scatter.Override(0.8f);
                    bloomComponent.maxIterations.Override(3);
                    break;
                case QualityLevel.High:
                    bloomComponent.active = true;
                    bloomComponent.intensity.Override(6);
                    bloomComponent.scatter.Override(0.8f);
                    bloomComponent.maxIterations.Override(3);
                    break;
            }
        }

        private void ApplyToneMapping(QualityLevel level)
        {
            if (!RendererFeatureManager.Instance.initialized)
            {
                Debug.LogWarning("RendererFeatureManager 未初始化");
                return;
            }

            var features = RendererFeatureManager.Instance.rendererFeatures;
            if (!features.TryGetValue("ToneMappingRenderFeature", out tonemappingRendererFeature) 
                || !features.TryGetValue("PiuFinalBlitRenderFeature", out piuFinalBlitRendererFeature))
            {
                Debug.LogWarning("RendererFeatureManager 尚未初始化，无法应用 ToneMapping 设置");
                return;
            }
            
            piuFinalBlitRendererFeature.SetActive(level == QualityLevel.Low);
            tonemappingRendererFeature.SetActive(level != QualityLevel.Low);
        }

        private void ApplyMaterials(QualityLevel level)
        {
            if (scenePrefab == null) return;

            switch (level)
            {
                case QualityLevel.Low:
                    MaterialReplace(level);
                    break;
                case QualityLevel.Medium:
                    MaterialReplace(level);
                    break;
                case QualityLevel.High:
                    MaterialReplace(level);
                    break;
            }
        }

        private void ApplyAntiAliasing(QualityLevel level)
        {
            if (mainCamera == null) return;
            if (mainCamera.TryGetComponent(out cameraData))
            {
                switch (level)
                {
                    case QualityLevel.Low:
                        cameraData.antialiasing = AntialiasingMode.None;
                        break;
                    case QualityLevel.Medium:
                        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        cameraData.antialiasingQuality = AntialiasingQuality.Medium;
                        break;
                    case QualityLevel.High:
                        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        cameraData.antialiasingQuality = AntialiasingQuality.High;
                        break;
                }
            }
        }

        private void ApplyQuadDecal(QualityLevel level)
        {
            if (quadDecal == null)
                quadDecal = GameObject.Find("Quad_Decal");

            if (quadDecal == null) return;

            bool active = (level != QualityLevel.Low);
            quadDecal.SetActive(active);
        }

        public void MaterialReplace(QualityLevel level)
        {
            if (scenePrefab == null || mapMaterialGroupProfile == null)
            {
                Debug.LogWarning("请设置 targetInstance 和 mapMaterialGroupProfile");
                return;
            }

            List<Material> loadedMats = mapMaterialGroupProfile.GetMaterialList(level);

            Dictionary<string, Material> mediumMatDict = new Dictionary<string, Material>();

            foreach (Material mat in loadedMats)
            {
                string baseName = GetMaterialBaseName(mat.name);

                if (!mediumMatDict.ContainsKey(baseName))
                    mediumMatDict.Add(baseName, mat);
            }

            Renderer[] renderers = scenePrefab.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                var mats = renderer.materials; // 注意：运行时用 materials 而不是 sharedMaterials
                bool changed = false;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;

                    string baseName = GetMaterialBaseName(mats[i].name);
                    if (mediumMatDict.TryGetValue(baseName, out Material newMat))
                    {
                        mats[i] = newMat;
                        changed = true;
                    }
                }

                if (changed)
                {
                    renderer.materials = mats;
                }
            }

            // Debug.Log("运行时材质替换完成");
        }

        private string GetMaterialBaseName(string name)
        {
            name = name.Replace(" (Instance)", "");
            int lastUnderscore = name.LastIndexOf('_');
            return (lastUnderscore > 0) ? name.Substring(0, lastUnderscore) : name;
        }

    }

}


