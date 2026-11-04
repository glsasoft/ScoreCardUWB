using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Graphics
{
    [ExecuteAlways]
    [DisallowMultipleComponent] // 防止组件重复添加
    public class RoleMaterialController : MonoBehaviour
    {
        private static readonly int CenterPointPosition = Shader.PropertyToID("_CenterPointPosition");
        private static readonly int OverallColorExtent = Shader.PropertyToID("_OverallColorExtent");
        private static readonly int OverallColorStrength = Shader.PropertyToID("_OverallColorStrength");
        private static readonly int OverallColor = Shader.PropertyToID("_OverallColor");
        private static readonly int DissolveMapEdge = Shader.PropertyToID("_DissolveMapEdge");
        private static readonly int DissolveTexFlowAndBevel = Shader.PropertyToID("_DissolveTexFlowAndBevel");
        private static readonly int DissolveMap = Shader.PropertyToID("_DissolveMap");
        private static readonly int DissolveDirWidth = Shader.PropertyToID("_DissolveDirWidth");
        private static readonly int DissolveDir = Shader.PropertyToID("_DissolveDir");
        private static readonly int DissolveColorStrength = Shader.PropertyToID("_DissolveColorStrength");
        private static readonly int DissolveColor = Shader.PropertyToID("_DissolveColor");
        private static readonly int DissolveMapThreshold = Shader.PropertyToID("_DissolveMapThreshold");
        private static readonly int DissolveDirThreshold = Shader.PropertyToID("_DissolveDirThreshold");
        private static readonly int DissolveFit = Shader.PropertyToID("_DissolveFit");
        private static readonly int Origin = Shader.PropertyToID("_DissolveOrigin");
        //private static readonly int DissolveOn = Shader.PropertyToID("_Dissolve_On");
        static readonly int _FaceForwardDirection = Shader.PropertyToID("_FaceForwardDirection");
        static readonly int _FaceUpDirection = Shader.PropertyToID("_FaceUpDirection");
        static readonly int _PerspectiveRemovalAmount = Shader.PropertyToID("_PerspectiveRemovalAmount");
        static readonly int _OutlineWidthExtraMultiplier = Shader.PropertyToID("_OutlineWidthExtraMultiplier");

        public enum TransformDirection
        {
            X,
            Y,
            Z,
            negX,
            negY,
            negZ
        }

        public Transform modelRoot;

        [Header("Editor 预览控制")]
        [Tooltip("在 Edit Mode 是否实时刷新材质参数")]
        public bool previewInEditor = false;
        [Tooltip("运行时（Play Mode）是否仍然用 sharedMaterials")]
        public bool useSharedMaterial = false;

        [Header("Head Bone")]
        [Tooltip("Setup - Head Bone and Face forward direction (affect face lighting and perspective removal)")]
        public Transform headBoneTransform;
        [Tooltip("Select a direction which makes the BLUE arrow point to face's forward direction")]
        public TransformDirection faceForwardDirection = TransformDirection.Z;
        [Tooltip("Select a direction which makes the GREEN arrow point to face's upward direction")]
        public TransformDirection faceUpDirection = TransformDirection.Y;
        [Tooltip("右键刷新材质")]
        [ContextMenuItem("Refresh Materials", "RefreshMaterialList")]
        public string[] faceMaterialNameContains = new string[] { "face" };

        [SerializeField]
        [HideInInspector]
        private List<Material> m_AllMaterials;
        [SerializeField]
        [HideInInspector]
        private List<Material> m_FaceMaterials;

        [Header("透视移除量")]
        [Range(0, 1)]
        public float perspectiveRemovalAmount = 0;
        [Header("描边宽度额外倍增")]
        [Range(0, 256)]
        public float outlineWidthExtraMultiplier = 1;

        // 主要参数
        [Header("控制溶解主开关")]
        [HideInInspector]
        public bool dissolveOn = false;

        [Header("溶解颜色")]
        [HideInInspector]
        [ColorUsage(true, true)] public Color dissolveColor = Color.white;
        [HideInInspector]
        [Range(0, 3)] public float dissolveColorStrength = 1;

        [Header("方向溶解")]
        [HideInInspector]
        public Transform DissolveOrigin;
        [HideInInspector]
        [Range(-10, 10)] public float dissolveFit = 0;
        [HideInInspector]
        [Range(0, 1)] public float dissolveDirThreshold = 0;
        [HideInInspector]
        public Vector3 dissolveDir = Vector3.up;
        [HideInInspector]
        [Range(0, 0.5f)] public float dissolveDirWidth = 0.1f;

        [Header("贴图溶解")]
        [HideInInspector]
        [Range(0, 1)] public float dissolveMapThreshold = 0;
        [HideInInspector]
        public Texture2D dissolveMap;
        [HideInInspector]
        [Range(0, 0.5f)] public float dissolveMapEdge = 0;
        [HideInInspector]
        public Vector4 dissolveTexFlowAndBevel = Vector4.zero;

        [Header("整体变色")]
        [HideInInspector]
        public Color overallColor = Color.black;
        [HideInInspector]
        [Range(0, 20)] public float overallColorStrength = 1;
        [HideInInspector]
        [Range(0, 1)] public float overallColorExtent = 0;

        // readonly ：地址不变，内容可能变
        private readonly List<Material> allMaterials = new List<Material>(); // 动态数组
        private readonly List<Material> faceMaterials = new List<Material>();
        Vector3 finalFaceDirectionWS_Forward;
        Vector3 finalFaceDirectionWS_Up;

        struct MaterialPropertyCache
        {
            public bool hasCenterPointPosition;
            public Vector4 centerPointPosition;
            public bool hasFaceForwardDirection;
            public Vector4 faceForwardDirection;
            public bool hasFaceUpDirection;
            public Vector4 faceUpDirection;
            public bool hasPerspectiveRemovalAmount;
            public float perspectiveRemovalAmount;
            public bool hasOutlineWidthExtraMultiplier;
            public float outlineWidthExtraMultiplier;
        }
        private readonly Dictionary<Material, MaterialPropertyCache> materialPropertyCacheDic = new Dictionary<Material, MaterialPropertyCache>();

        private Vector3 lastPosition, lastEulerAngles;

        // 服务于编辑器更改
        private void OnValidate()
        {
            if (Application.isPlaying || previewInEditor)
            {
                //RefreshMaterialList();
                if (dissolveOn) SetProperty();

                perspectiveRemovalAmount = Mathf.Clamp01(perspectiveRemovalAmount);
                outlineWidthExtraMultiplier = Mathf.Clamp(outlineWidthExtraMultiplier, 0, 256);
                foreach (var mat in allMaterials)
                {
                    if (mat)
                    {
                        mat.SetFloat(_PerspectiveRemovalAmount, perspectiveRemovalAmount);
                        mat.SetFloat(_OutlineWidthExtraMultiplier, outlineWidthExtraMultiplier);
                    }
                }
            }
        }

        private void Awake()
        {
            if (modelRoot == null) modelRoot = transform;
            m_AllMaterials = allMaterials;
            m_FaceMaterials = faceMaterials;

            RefreshMaterialList();
            DissolveInit();
        }

        private void OnEnable()
        {
            CacheMaterialProperty();

            foreach (var mat in allMaterials)
            {
                if (mat)
                {
                    mat.SetFloat(_PerspectiveRemovalAmount, perspectiveRemovalAmount);
                    mat.SetFloat(_OutlineWidthExtraMultiplier, outlineWidthExtraMultiplier);
                }
            }
        }

        private void Update()
        {
            if (dissolveOn) SetProperty();
        }

        private void LateUpdate()
        {
            if (modelRoot && modelRoot.position != lastPosition)
            {
                lastPosition = modelRoot.position;

                foreach (var mat in allMaterials)
                {
                    if (mat)
                        mat.SetVector(CenterPointPosition, lastPosition);
                }
            }

            if (headBoneTransform && headBoneTransform.eulerAngles != lastEulerAngles)
            {
                lastEulerAngles = headBoneTransform.eulerAngles;

                finalFaceDirectionWS_Forward = GetFinalFaceDirectionWS(faceForwardDirection);
                finalFaceDirectionWS_Up = GetFinalFaceDirectionWS(faceUpDirection);

                foreach (var VARIABLE in faceMaterials)
                {
                    VARIABLE.SetVector(_FaceForwardDirection, finalFaceDirectionWS_Forward);
                    VARIABLE.SetVector(_FaceUpDirection, finalFaceDirectionWS_Up);
                }
            }
        }

        private void OnDisable()
        {
            RevertMaterialProperty();
        }

        public void RefreshMaterialList()
        {
            allMaterials.Clear();
            faceMaterials.Clear();
            HashSet<Material> materialHashSet = new HashSet<Material>();
            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var rend in renderers)
            {
                // 非运行时用sharedMat，运行时用实例
                // sharedMaterials：原始材质资源
                // materials：运行时自动实例化副本（每个对象一份）
                bool forceShared = !Application.isPlaying;
                Material[] mats = (forceShared || useSharedMaterial) ? rend.sharedMaterials : rend.materials;

                if (mats != null)
                {
                    foreach (var mat in mats)
                    {
                        if (mat && materialHashSet.Add(mat))
                        {
                            allMaterials.Add(mat);

                            if (CheckFaceMaterial(mat))
                            {
                                faceMaterials.Add(mat);
                            }
                        }
                    }
                }
            }
        }

        bool CheckFaceMaterial(Material mat)
        {
            if (mat == null) return false;

            if (faceMaterialNameContains != null && faceMaterialNameContains.Length > 0)
            {
                for (int i = 0; i < faceMaterialNameContains.Length; i++)
                {
                    if (!string.IsNullOrEmpty(faceMaterialNameContains[i]) && mat.name.Contains(faceMaterialNameContains[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void CacheMaterialProperty()
        {
            bool forceShared = !Application.isPlaying;
            if (forceShared || useSharedMaterial)
            {
                materialPropertyCacheDic.Clear();

                HashSet<Material> faceMaterialHashSet = new HashSet<Material>();
                foreach (var mat in faceMaterials)
                {
                    if (mat) faceMaterialHashSet.Add(mat);
                }

                foreach (var mat in allMaterials)
                {
                    if (mat)
                    {
                        MaterialPropertyCache cache = new MaterialPropertyCache();

                        if (mat.HasVector(CenterPointPosition))
                        {
                            cache.hasCenterPointPosition = true;
                            cache.centerPointPosition = mat.GetVector(CenterPointPosition);
                            materialPropertyCacheDic[mat] = cache;
                        }
                        if (mat.HasFloat(_PerspectiveRemovalAmount))
                        {
                            cache.hasPerspectiveRemovalAmount = true;
                            cache.perspectiveRemovalAmount = mat.GetFloat(_PerspectiveRemovalAmount);
                            materialPropertyCacheDic[mat] = cache;
                        }
                        if (mat.HasFloat(_OutlineWidthExtraMultiplier))
                        {
                            cache.hasOutlineWidthExtraMultiplier = true;
                            cache.outlineWidthExtraMultiplier = mat.GetFloat(_OutlineWidthExtraMultiplier);
                            materialPropertyCacheDic[mat] = cache;
                        }

                        if (faceMaterialHashSet.Contains(mat))
                        {
                            if (mat.HasVector(_FaceForwardDirection))
                            {
                                cache.hasFaceForwardDirection = true;
                                cache.faceForwardDirection = mat.GetVector(_FaceForwardDirection);
                                materialPropertyCacheDic[mat] = cache;
                            }
                            if (mat.HasVector(_FaceUpDirection))
                            {
                                cache.hasFaceUpDirection = true;
                                cache.faceUpDirection = mat.GetVector(_FaceUpDirection);
                                materialPropertyCacheDic[mat] = cache;
                            }
                        }
                    }
                }
            }
        }

        void RevertMaterialProperty()
        {
            bool forceShared = !Application.isPlaying;
            if (forceShared || useSharedMaterial)
            {
                foreach (var mat in allMaterials)
                {
                    if (mat && materialPropertyCacheDic.TryGetValue(mat, out MaterialPropertyCache cache))
                    {
                        if (cache.hasCenterPointPosition) mat.SetVector(CenterPointPosition, cache.centerPointPosition);
                        if (cache.hasFaceForwardDirection) mat.SetVector(_FaceForwardDirection, cache.faceForwardDirection);
                        if (cache.hasFaceUpDirection) mat.SetVector(_FaceUpDirection, cache.faceUpDirection);
                        if (cache.hasPerspectiveRemovalAmount) mat.SetFloat(_PerspectiveRemovalAmount, cache.perspectiveRemovalAmount);
                        if (cache.hasOutlineWidthExtraMultiplier) mat.SetFloat(_OutlineWidthExtraMultiplier, cache.outlineWidthExtraMultiplier);
                    }
                }

                materialPropertyCacheDic.Clear();

#if UNITY_EDITOR
                //AssetDatabase.SaveAssets();
#endif
            }
        }

        private void SetProperty()
        {
            foreach (var mat in allMaterials)
            {
                if (mat.HasProperty(Origin))
                    mat.SetVector(Origin, DissolveOrigin ? DissolveOrigin.position : Vector3.zero);
                mat.SetFloat(DissolveFit, dissolveFit);
                mat.SetFloat(DissolveDirThreshold, dissolveDirThreshold);
                mat.SetFloat(DissolveMapThreshold, dissolveMapThreshold);
                mat.SetColor(DissolveColor, dissolveColor);
                mat.SetFloat(DissolveColorStrength, dissolveColorStrength);
                mat.SetVector(DissolveDir, (Vector4)transform.TransformDirection(dissolveDir));
                // mat.SetVector(DissolveDir, (Vector4)dissolveDir);
                mat.SetFloat(DissolveDirWidth, dissolveDirWidth);

                if (mat.HasProperty(DissolveMap) && dissolveMap != null)
                    mat.SetTexture(DissolveMap, dissolveMap);

                mat.SetVector(DissolveTexFlowAndBevel, dissolveTexFlowAndBevel);
                mat.SetFloat(DissolveMapEdge, dissolveMapEdge);

                mat.SetColor(OverallColor, overallColor);
                mat.SetFloat(OverallColorStrength, overallColorStrength);
                mat.SetFloat(OverallColorExtent, overallColorExtent);
            }
        }

        // 外部控制开关
        public void EnableDissolve(bool enable)
        {
            dissolveOn = enable;
            if (dissolveOn == false)
            {
                DissolveInit();
            }
        }

        // 参数初始化
        public void DissolveInit()
        {
            foreach (var mat in allMaterials)
            {
                mat.SetFloat(DissolveDirThreshold, 0.0f);
                mat.SetFloat(DissolveMapThreshold, 0.0f);
                mat.SetFloat(OverallColorExtent, 0.0f);
            }
        }

        private Vector3 GetFinalFaceDirectionWS(TransformDirection direction)
        {
            switch (direction)
            {
                case TransformDirection.X:
                    return headBoneTransform.right;
                case TransformDirection.Y:
                    return headBoneTransform.up;
                case TransformDirection.Z:
                    return headBoneTransform.forward;
                case TransformDirection.negX:
                    return -headBoneTransform.right;
                case TransformDirection.negY:
                    return -headBoneTransform.up;
                case TransformDirection.negZ:
                    return -headBoneTransform.forward;
                default:
                    throw new System.NotImplementedException();
            }
        }

#if UNITY_EDITOR
        // 右键菜单手动刷新
        [ContextMenu("Refresh Materials (Editor)")]
        private void ContextRefresh() => RefreshMaterialList();

        //[ContextMenu("Dissolve On/Off")]
        //private void DissolveRefresh() => EnableDissolve(!dissolveOn);
#endif
    }


    //----------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    [CustomEditor(typeof(RoleMaterialController))]
    public class RoleMaterialControllerEditor : Editor
    {
        SerializedProperty m_AllMaterials;
        SerializedProperty m_FaceMaterials;

        void OnEnable()
        {
            // Setup the SerializedProperties.
            m_AllMaterials = serializedObject.FindProperty("m_AllMaterials");
            m_FaceMaterials = serializedObject.FindProperty("m_FaceMaterials");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_AllMaterials, new GUIContent("All Materials"), true);
            EditorGUILayout.PropertyField(m_FaceMaterials, new GUIContent("Face Materials"), true);
            EditorGUI.EndDisabledGroup();

            base.OnInspectorGUI();
        }
    }
#endif
}
