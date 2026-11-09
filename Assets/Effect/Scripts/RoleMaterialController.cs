using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Graphics
{
    /// <summary>
    /// 角色材质参数控制器：把一批"每个角色都要设一遍"的材质参数，从角色根节点统一下发给所有材质。
    ///
    /// 从 H:\ScoreCardUnity\Assets\Effect\Scripts\RoleMaterialController.cs 搬过来的，
    /// 去掉了两类东西：
    ///   1. 当前工程 shader 里已经不存在的参数（老 PBR 溶解 / 整体变色那一套）：
    ///        _DissolveFit、_DissolveDir、_DissolveDirWidth、_DissolveDirThreshold、
    ///        _DissolveMap、_DissolveMapThreshold、_DissolveMapEdge、_DissolveTexFlowAndBevel、
    ///        _DissolveColor、_DissolveColorStrength、_DissolveOrigin、
    ///        _OverallColor、_OverallColorStrength、_OverallColorExtent
    ///   2. 面部朝向下发：_FaceForwardDirection / _FaceUpDirection 固定是模型本地空间的 +Z / +Y，
    ///      材质默认值就是它，shader 里自己 TransformObjectToWorldDir 转世界，这里不用管。
    ///
    /// 保留：_CenterPointPosition、_PerspectiveRemovalAmount、_OutlineWidthExtraMultiplier、
    /// NiloToon 上下溶解（_DissolveAmount 那组）、受击闪白（_HitFlash* 那组）。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class RoleMaterialController : MonoBehaviour
    {
        static readonly int CenterPointPosition = Shader.PropertyToID("_CenterPointPosition");
        static readonly int _PerspectiveRemovalAmount = Shader.PropertyToID("_PerspectiveRemovalAmount");
        static readonly int _OutlineWidthExtraMultiplier = Shader.PropertyToID("_OutlineWidthExtraMultiplier");

        // NiloToon 上下溶解（Body shader）
        static readonly int NiloDissolveAmount = Shader.PropertyToID("_DissolveAmount");
        static readonly int NiloDissolveMinY = Shader.PropertyToID("_DissolveMinY");
        static readonly int NiloDissolveMaxY = Shader.PropertyToID("_DissolveMaxY");
        static readonly int NiloDissolveRootY = Shader.PropertyToID("_DissolveRootY");
        static readonly int NiloEnableDissolve = Shader.PropertyToID("_EnableDissolve");
        static readonly int NiloDissolveBorderRange = Shader.PropertyToID("_DissolveBorderRange");
        const string NiloDissolveKeyword = "_NILOTOON_DISSOLVE";

        // 受击闪白（独立一层菲涅尔）
        static readonly int HitFlashColor = Shader.PropertyToID("_HitFlashColor");
        static readonly int HitFlashIntensity = Shader.PropertyToID("_HitFlashIntensity");
        static readonly int HitFlashPower = Shader.PropertyToID("_HitFlashPower");

        [Header("角色根节点（留空则用自己）")]
        public Transform modelRoot;

        [Header("Editor 预览控制")]
        [Tooltip("在 Edit Mode 是否实时刷新材质参数")]
        public bool previewInEditor = false;
        [Tooltip("运行时（Play Mode）是否仍然用 sharedMaterials")]
        public bool useSharedMaterial = false;

        [SerializeField, HideInInspector]
        private List<Material> m_AllMaterials;

        [Header("透视移除量")]
        [Range(0, 1)]
        public float perspectiveRemovalAmount = 0;
        [Header("描边宽度额外倍增")]
        [Range(0, 256)]
        public float outlineWidthExtraMultiplier = 1;

        [Header("NiloToon 上下溶解")]
        [Tooltip("统一开关：给角色所有材质开/关 _NILOTOON_DISSOLVE")]
        public bool niloDissolveOn = false;
        [Tooltip("0 = 头顶先消失，1 = 脚底最后消失")]
        [Range(0, 1)]
        public float niloDissolveAmount = 0;
        [Tooltip("溶解高度下限（相对角色根节点的 Y）")]
        public float niloDissolveMinY = 0;
        [Tooltip("溶解高度上限（相对角色根节点的 Y）")]
        public float niloDissolveMaxY = 1.515f;
        [Tooltip("溶解边缘染色/亮边的宽度，也是边缘网点的作用范围")]
        [Range(0.01f, 1)]
        public float niloDissolveBorderRange = 0.04f;

        [Header("受击闪白（独立一层菲涅尔）")]
        [Tooltip("0 = 关闭。受击时拉起来就是全身闪白")]
        [Range(0, 10)]
        public float hitFlashIntensity = 0;
        [Tooltip("闪白颜色，HDR 可以拉超过 1")]
        [ColorUsage(true, true)]
        public Color hitFlashColor = Color.white;
        [Tooltip("菲涅尔指数：越大越集中在边缘，越小越整片")]
        [Range(0.1f, 16)]
        public float hitFlashPower = 2f;

        private readonly List<Material> allMaterials = new List<Material>();

        struct MaterialPropertyCache
        {
            public bool hasCenterPointPosition;
            public Vector4 centerPointPosition;
            public bool hasPerspectiveRemovalAmount;
            public float perspectiveRemovalAmount;
            public bool hasOutlineWidthExtraMultiplier;
            public float outlineWidthExtraMultiplier;
        }

        private readonly Dictionary<Material, MaterialPropertyCache> materialPropertyCacheDic =
            new Dictionary<Material, MaterialPropertyCache>();

        private Vector3 lastPosition;
        private bool niloDirty = true;

        private void OnValidate()
        {
            niloDirty = true;
            perspectiveRemovalAmount = Mathf.Clamp01(perspectiveRemovalAmount);
            outlineWidthExtraMultiplier = Mathf.Clamp(outlineWidthExtraMultiplier, 0, 256);

            if (Application.isPlaying || previewInEditor)
            {
                foreach (var mat in allMaterials)
                {
                    if (!mat) continue;
                    mat.SetFloat(_PerspectiveRemovalAmount, perspectiveRemovalAmount);
                    mat.SetFloat(_OutlineWidthExtraMultiplier, outlineWidthExtraMultiplier);
                }
            }
        }

        private void Awake()
        {
            if (modelRoot == null) modelRoot = transform;
            m_AllMaterials = allMaterials;
            RefreshMaterialList();
        }

        private void OnEnable()
        {
            CacheMaterialProperty();
            niloDirty = true;

            foreach (var mat in allMaterials)
            {
                if (!mat) continue;
                mat.SetFloat(_PerspectiveRemovalAmount, perspectiveRemovalAmount);
                mat.SetFloat(_OutlineWidthExtraMultiplier, outlineWidthExtraMultiplier);
            }
        }

        private void Update()
        {
            // 开着的时候每帧同步，_DissolveRootY 要跟角色移动；
            // 关着的时候只在参数改动后刷一次，把 keyword 收干净。
            if (niloDissolveOn || niloDirty)
            {
                ApplyNiloDissolve();
                niloDirty = false;
            }

            // 受击闪白经常在几帧内来回变，每帧同步一次最省心
            ApplyHitFlash();
        }

        private void LateUpdate()
        {
            if (modelRoot == null) return;
            if (modelRoot.position == lastPosition) return;

            lastPosition = modelRoot.position;
            foreach (var mat in allMaterials)
            {
                if (mat) mat.SetVector(CenterPointPosition, lastPosition);
            }
        }

        private void OnDisable()
        {
            RevertMaterialProperty();
        }

        public void RefreshMaterialList()
        {
            allMaterials.Clear();
            HashSet<Material> seen = new HashSet<Material>();

            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var rend in renderers)
            {
                if (!rend) continue;

                // 非运行时用 sharedMaterials（原始材质资源），运行时用 materials（实例化副本）
                bool forceShared = !Application.isPlaying;
                Material[] mats = (forceShared || useSharedMaterial) ? rend.sharedMaterials : rend.materials;
                if (mats == null) continue;

                foreach (var mat in mats)
                {
                    if (mat && seen.Add(mat)) allMaterials.Add(mat);
                }
            }
        }

        private void CacheMaterialProperty()
        {
            bool forceShared = !Application.isPlaying;
            if (!(forceShared || useSharedMaterial)) return;

            materialPropertyCacheDic.Clear();

            foreach (var mat in allMaterials)
            {
                if (!mat) continue;

                MaterialPropertyCache cache = new MaterialPropertyCache();
                bool any = false;

                if (mat.HasVector(CenterPointPosition))
                {
                    cache.hasCenterPointPosition = true;
                    cache.centerPointPosition = mat.GetVector(CenterPointPosition);
                    any = true;
                }
                if (mat.HasFloat(_PerspectiveRemovalAmount))
                {
                    cache.hasPerspectiveRemovalAmount = true;
                    cache.perspectiveRemovalAmount = mat.GetFloat(_PerspectiveRemovalAmount);
                    any = true;
                }
                if (mat.HasFloat(_OutlineWidthExtraMultiplier))
                {
                    cache.hasOutlineWidthExtraMultiplier = true;
                    cache.outlineWidthExtraMultiplier = mat.GetFloat(_OutlineWidthExtraMultiplier);
                    any = true;
                }

                if (any) materialPropertyCacheDic[mat] = cache;
            }
        }

        private void RevertMaterialProperty()
        {
            bool forceShared = !Application.isPlaying;
            if (!(forceShared || useSharedMaterial)) return;

            foreach (var mat in allMaterials)
            {
                if (!mat || !materialPropertyCacheDic.TryGetValue(mat, out MaterialPropertyCache cache)) continue;

                if (cache.hasCenterPointPosition) mat.SetVector(CenterPointPosition, cache.centerPointPosition);
                if (cache.hasPerspectiveRemovalAmount) mat.SetFloat(_PerspectiveRemovalAmount, cache.perspectiveRemovalAmount);
                if (cache.hasOutlineWidthExtraMultiplier) mat.SetFloat(_OutlineWidthExtraMultiplier, cache.outlineWidthExtraMultiplier);
            }

            materialPropertyCacheDic.Clear();
        }

        /// <summary>
        /// 把上下溶解的参数统一下发给角色所有材质。
        /// 只对带 _DissolveMinY 的材质生效（Body shader），脸和眼睛那些不会被动到。
        /// </summary>
        public void ApplyNiloDissolve()
        {
            // 域重载 / 场景重载后 allMaterials 会被清空，而 Awake 不一定补得回来。
            // 这里兜一次底，避免溶解参数打到一个空列表上、看着像「没效果」。
            if (allMaterials.Count == 0)
            {
                RefreshMaterialList();
            }

            float rootY = modelRoot ? modelRoot.position.y : transform.position.y;

            foreach (var mat in allMaterials)
            {
                if (mat == null || !mat.HasProperty(NiloDissolveMinY)) continue;

                if (niloDissolveOn) mat.EnableKeyword(NiloDissolveKeyword);
                else mat.DisableKeyword(NiloDissolveKeyword);

                mat.SetFloat(NiloEnableDissolve, niloDissolveOn ? 1f : 0f);
                mat.SetFloat(NiloDissolveAmount, niloDissolveAmount);
                mat.SetFloat(NiloDissolveMinY, niloDissolveMinY);
                mat.SetFloat(NiloDissolveMaxY, niloDissolveMaxY);
                mat.SetFloat(NiloDissolveRootY, rootY);
                mat.SetFloat(NiloDissolveBorderRange, niloDissolveBorderRange);
            }
        }

        /// <summary>
        /// 把受击闪白参数下发给角色所有材质（body / face / eye 都带这组属性）。
        /// 受击时把 hitFlashIntensity 拉起来，再缓动降回 0。
        /// </summary>
        public void ApplyHitFlash()
        {
            foreach (var mat in allMaterials)
            {
                if (mat == null || !mat.HasProperty(HitFlashIntensity)) continue;
                mat.SetFloat(HitFlashIntensity, hitFlashIntensity);
                mat.SetFloat(HitFlashPower, hitFlashPower);
                mat.SetColor(HitFlashColor, hitFlashColor);
            }
        }

        /// <summary>
        /// 按当前所有渲染器的世界包围盒自动填溶解高度上下限。
        /// 手举过头顶这类姿势也能覆盖到，不用手填数字。
        /// </summary>
        [ContextMenu("自动适配溶解高度")]
        public void AutoFitDissolveHeight()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);

            if (b.size.y <= 0.0001f) return;

            float rootY = modelRoot ? modelRoot.position.y : transform.position.y;
            niloDissolveMinY = b.min.y - rootY;
            niloDissolveMaxY = b.max.y - rootY;
            niloDirty = true;

#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh Materials (Editor)")]
        private void ContextRefresh() => RefreshMaterialList();
#endif
    }


    //----------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    [CustomEditor(typeof(RoleMaterialController))]
    public class RoleMaterialControllerEditor : Editor
    {
        SerializedProperty m_AllMaterials;

        void OnEnable()
        {
            m_AllMaterials = serializedObject.FindProperty("m_AllMaterials");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_AllMaterials, new GUIContent("All Materials"), true);
            EditorGUI.EndDisabledGroup();

            base.OnInspectorGUI();

            EditorGUILayout.Space(6);
            var controller = (RoleMaterialController)target;
            if (GUILayout.Button("自动适配溶解高度（按当前包围盒）", GUILayout.Height(24)))
            {
                Undo.RecordObject(controller, "Auto Fit Dissolve Height");
                controller.AutoFitDissolveHeight();
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }
            EditorGUILayout.HelpBox(
                "把角色摆成需要的姿势（比如手举过头顶），再点上面这个按钮，" +
                "会用当前所有渲染器的世界包围盒自动填好溶解高度上下限。",
                MessageType.Info);
        }
    }
#endif
}
