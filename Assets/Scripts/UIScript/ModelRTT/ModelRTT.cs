using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    [RequireComponent(typeof(RawImage))]
    public class ModelRTT : MonoBehaviour
    {
        // 展示位远离场景，并沿 X 轴错开，避免各相机互相拍到
        const float SlotBaseY = 100000f;
        const float SlotSpacing = 2000f;

        static GameObject m_Root;
        static int m_SlotIndex;


        RawImage m_RawImage;
        RenderTexture m_RenderTexture;
        Camera m_Camera;
        Transform m_Stage;
        GameObject m_Model;
        ResLoader m_ResLoader;
        Dictionary<string, GameObject> m_PrefabCache = new();

        Vector3 m_ModelLocalPosition = new Vector3(0f, 0f, 3f);
        Quaternion m_ModelLocalRotation = Quaternion.Euler(0f, 180f, 0f);
        Vector3 m_ModelLocalScale = Vector3.one;
        Vector3 m_ModelCenterOffset = Vector3.zero;
        int m_LoadSerial;

        [SerializeField]
        private string m_ModelAddress;
        public bool m_PositionBasedCenter = true;

        public GameObject model => m_Model;

        public async UniTask LoadModel(string address)
        {
            if (string.IsNullOrEmpty(address) || m_Stage == null)
                return;

            if (address == m_ModelAddress)
                return;

            m_ModelAddress = address;

            int serial = ++m_LoadSerial;
            ClearModel();

            var prefab = await LoadPrefab(address);
            if (prefab == null)
                return;

            if (this == null || serial != m_LoadSerial)
                return;

            var instance = Object.Instantiate(prefab, m_Stage, false);
            UITools.SetGameObjectLayer(instance, Layer.AVATAR_SHOW);

            m_Model = instance;

            SetupModelCenterOffset();
            ApplyModelTransform();
        }

        async UniTask<GameObject> LoadPrefab(string address)
        {
            if (m_PrefabCache.TryGetValue(address, out var cached) && cached != null)
                return cached;

            m_ResLoader ??= new ResLoader();
            m_ResLoader.AddRes(address, ResType.Prefab, path =>
            {
                var handle = m_ResLoader.GetAsset(path);
                if (handle.AssetObject is GameObject prefab)
                    m_PrefabCache[address] = prefab;
                handle.Dispose();
            });

            await m_ResLoader.StartLoad();

            if (m_PrefabCache.TryGetValue(address, out var prefabAsset) && prefabAsset != null)
                return prefabAsset;

            Debug.LogError("[ModelRTT] 模型加载失败: " + address);
            return null;
        }

        public void SetModelPosition(Vector3 localPosition)
        {
            m_ModelLocalPosition = localPosition;
            ApplyModelTransform();
        }

        public void SetModelRotation(Vector3 localEulerAngles)
        {
            m_ModelLocalRotation = Quaternion.Euler(localEulerAngles);
            ApplyModelTransform();
        }

        public void SetModelRotation(Quaternion localRotation)
        {
            m_ModelLocalRotation = localRotation;
            ApplyModelTransform();
        }

        public void SetModelScale(Vector3 localScale)
        {
            m_ModelLocalScale = localScale;
            ApplyModelTransform();
        }

        void Awake()
        {
            m_RawImage = GetComponent<RawImage>();
            m_Stage = SetupStage();
            m_Camera = EnsureCamera(m_Stage);
            m_Camera.enabled = false;
            SetupRenderTexture();

            string address = m_ModelAddress;
            m_ModelAddress = null;
            LoadModel(address).Forget();
        }

        void OnEnable()
        {
            if (m_Camera != null)
                m_Camera.enabled = true;
            SetupRenderTexture();
        }

        void OnDisable()
        {
            if (m_Camera != null)
                m_Camera.enabled = false;
        }

        void OnDestroy()
        {
            ReleaseRenderTexture();
            if (m_Stage != null)
                Destroy(m_Stage.gameObject);

            if (m_ResLoader != null)
                m_ResLoader.ReleaseAll();
            m_PrefabCache.Clear();
        }

        void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;
            SetupRenderTexture();
        }

        Transform SetupStage()
        {
            EnsureRoot();

            var stageGo = new GameObject("ModelRTTStage_" + m_SlotIndex);
            var stage = stageGo.transform;
            stage.SetParent(m_Root.transform, false);
            stage.localPosition = new Vector3(m_SlotIndex * SlotSpacing, SlotBaseY, 0);
            stage.localRotation = Quaternion.identity;
            stage.localScale = Vector3.one;
            m_SlotIndex++;

            UITools.SetGameObjectLayer(stage.gameObject, Layer.AVATAR_SHOW);
            return stage;
        }

        void EnsureRoot()
        {
            if (m_Root != null)
                return;

            m_Root = new GameObject("ModelRTTRoot");
            Object.DontDestroyOnLoad(m_Root);

            var lightGo = new GameObject("ModelRTTLight");
            lightGo.transform.SetParent(m_Root.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(40f, 180f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << Layer.AVATAR_SHOW;
        }

        Camera EnsureCamera(Transform stage)
        {
            var camera = stage.GetComponentInChildren<Camera>(true);
            if (camera == null)
                camera = CreateCamera(stage);

            camera.clearFlags = CameraClearFlags.SolidColor;
            var background = camera.backgroundColor;
            background.a = 0f;
            camera.backgroundColor = background;
            camera.cullingMask = 1 << Layer.AVATAR_SHOW;
            return camera;
        }

        Camera CreateCamera(Transform parent)
        {
            var go = new GameObject("Camera");
            go.transform.SetParent(parent, false);

            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 200f;
            camera.depth = -10;
            camera.allowMSAA = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            return camera;
        }

        void SetupModelCenterOffset()
        {
            if (m_Model == null)
                return;

            m_ModelCenterOffset = Vector3.zero;
            var renderers = m_Model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length == 0)
                return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            m_ModelCenterOffset = m_Model.transform.position - bounds.center;
        }

        void ApplyModelTransform()
        {
            if (m_Model == null)
                return;

            var modelTransform = m_Model.transform;
            modelTransform.localPosition = m_ModelLocalPosition + (m_PositionBasedCenter ? m_ModelCenterOffset : Vector3.zero);
            modelTransform.localRotation = m_ModelLocalRotation;
            modelTransform.localScale = m_ModelLocalScale;
        }

        void ClearModel()
        {
            if (m_Model == null)
                return;

            Destroy(m_Model);
            m_Model = null;
        }

        void SetupRenderTexture()
        {
            if (m_RawImage == null || m_Camera == null)
                return;

            GetTextureSize(out int width, out int height);
            if (m_RenderTexture != null && m_RenderTexture.width == width && m_RenderTexture.height == height)
                return;

            ReleaseRenderTexture();

            m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            m_RenderTexture.name = "ModelRTT_" + name;
            m_RenderTexture.antiAliasing = 1;
            m_RenderTexture.useMipMap = false;
            m_RenderTexture.autoGenerateMips = false;
            m_RenderTexture.useDynamicScale = false;
            m_RenderTexture.enableRandomWrite = false;
            m_RenderTexture.Create();

            m_Camera.targetTexture = m_RenderTexture;
            m_RawImage.texture = m_RenderTexture;
            m_RawImage.enabled = true;
        }

        void GetTextureSize(out int width, out int height)
        {
            var rect = m_RawImage.rectTransform.rect;
            width = Mathf.RoundToInt(Mathf.Abs(rect.width));
            height = Mathf.RoundToInt(Mathf.Abs(rect.height));
            if (width < 2 || height < 2)
            {
                var size = m_RawImage.rectTransform.sizeDelta;
                width = Mathf.RoundToInt(Mathf.Abs(size.x));
                height = Mathf.RoundToInt(Mathf.Abs(size.y));
            }

            if (width < 2)
                width = 2;
            if (height < 2)
                height = 2;
        }

        void ReleaseRenderTexture()
        {
            if (m_Camera != null)
                m_Camera.targetTexture = null;

            if (m_RenderTexture == null)
                return;

            m_RenderTexture.Release();
            Destroy(m_RenderTexture);
            m_RenderTexture = null;
        }
    }
}
