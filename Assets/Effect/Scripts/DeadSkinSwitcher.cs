using System.Collections.Generic;
using UnityEngine;

namespace GLSAQuadView
{
    /// <summary>
    /// 一个角色对象承载 活着 / 死亡 两套外观。
    /// 切换只做 sharedMaterials 替换 + PropertyBlock 写入，不碰 GameObject.SetActive，
    /// 所以不会触发骨骼重绑、skin buffer 重建和 Transform 的 OnEnable 广播。
    /// 死亡之后的「尸体腐蚀成灵魂」由 shader 的 _DissolveAmount 控制，不再是独立状态。
    /// isDead 和 dissolve 由 Timeline 的 Animation 曲线驱动。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class DeadSkinSwitcher : MonoBehaviour
    {
        [System.Serializable]
        public class MaterialSet
        {
            public SkinnedMeshRenderer renderer;
            public Material[] materials;

            public MaterialSet(SkinnedMeshRenderer r)
            {
                renderer = r;
                materials = r != null ? r.sharedMaterials : new Material[0];
            }
        }

        [Header("材质组")]
        [Tooltip("每个 SkinnedMeshRenderer 以及它活着时候的材质。可以在右键菜单里自动收集。")]
        public MaterialSet[] aliveSets;

        [Tooltip("死亡外观使用的材质（M_Dead）。溶解进度 0 = 尸体，1 = 灵魂。")]
        public Material deadMaterial;

        [Header("Timeline 驱动")]
        [Range(0f, 1f)]
        public float dissolve = 0f;

        [Tooltip("false = 活着材质，true = 死亡材质。硬切，不做渐变。")]
        public bool isDead = false;

        [Header("粒子")]
        public GameObject vfxRoot;
        public ParticleSystem[] vfxParticles;

        static readonly int DissolveId = Shader.PropertyToID("_DissolveAmount");

        MaterialPropertyBlock _mpb;
        bool _lastDead;
        float _lastDissolve = float.NaN;

        void OnEnable()
        {
            // 故意设成不可能的值，保证第一次 ApplyAll 一定会刷一遍外观
            _lastDead = !isDead;
            _lastDissolve = float.NaN;
            ApplyAll();
        }

        [ContextMenu("预热死亡材质（编辑器内手动执行）")]
        void PrewarmNow()
        {
            if (!Application.isPlaying)
            {
                DeadMaterialPrewarmer.Warm(deadMaterial);
            }
        }

        void Update()
        {
            ApplyAll();
        }

        /// <summary>
        /// Animator / Timeline 把 isDead、dissolve 写进字段之后会调这个，
        /// 编辑模式下 Update 不是每帧都跑，靠它才能实时看到切换效果。
        /// </summary>
        void OnDidApplyAnimationProperties()
        {
            ApplyAll();
        }

        void ApplyAll()
        {
            if (isDead != _lastDead)
            {
                _lastDead = isDead;
                ApplySkin(isDead);
            }

            if (float.IsNaN(_lastDissolve) || !Mathf.Approximately(dissolve, _lastDissolve))
            {
                _lastDissolve = dissolve;
                ApplyDissolve(dissolve);
            }
        }

        /// <summary>
        /// true 换成死亡材质，false 换回收集好的原始材质。
        /// </summary>
        public void ApplySkin(bool dead)
        {
            if (aliveSets == null || aliveSets.Length == 0)
            {
                return;
            }

            for (int i = 0; i < aliveSets.Length; i++)
            {
                MaterialSet set = aliveSets[i];
                if (set == null || set.renderer == null)
                {
                    continue;
                }

                if (dead && deadMaterial != null)
                {
                    // 每个 renderer 的材质槽数量保持不变，只是全部换成死亡材质
                    int slotCount = Mathf.Max(1, set.renderer.sharedMaterials.Length);
                    Material[] mats = new Material[slotCount];
                    for (int k = 0; k < slotCount; k++)
                    {
                        mats[k] = deadMaterial;
                    }
                    set.renderer.sharedMaterials = mats;

                    ApplySwitchState(set.renderer);
                }
                else
                {
                    set.renderer.sharedMaterials = set.materials;
                    set.renderer.SetPropertyBlock(null);
                }
            }

            if (dead)
            {
                PlayVFX();
            }
        }

        void ApplySwitchState(SkinnedMeshRenderer renderer)
        {
            if (_mpb == null)
            {
                _mpb = new MaterialPropertyBlock();
            }

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(DissolveId, dissolve);
            renderer.SetPropertyBlock(_mpb);
        }

        void ApplyDissolve(float value)
        {
            // 活着的时候绝对不写 PropertyBlock：
            // RoleMaterialController 的 NiloToon 溶解用的也是 _DissolveAmount（材质属性），
            // PropertyBlock 优先级更高，写上去会把那边的溶解直接盖掉。
            if (!isDead)
            {
                return;
            }

            if (aliveSets == null)
            {
                return;
            }

            if (_mpb == null)
            {
                _mpb = new MaterialPropertyBlock();
            }

            for (int i = 0; i < aliveSets.Length; i++)
            {
                MaterialSet set = aliveSets[i];
                if (set == null || set.renderer == null)
                {
                    continue;
                }

                set.renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(DissolveId, value);
                set.renderer.SetPropertyBlock(_mpb);
            }
        }

        public void PlayVFX()
        {
            if (vfxRoot != null && !vfxRoot.activeSelf)
            {
                vfxRoot.SetActive(true);
            }

            if (vfxParticles == null)
            {
                return;
            }

            for (int i = 0; i < vfxParticles.Length; i++)
            {
                ParticleSystem ps = vfxParticles[i];
                if (ps == null)
                {
                    continue;
                }
                ps.Clear(true);
                ps.Play(true);
            }
        }

        [ContextMenu("从子物体自动收集")]
        public void CollectFromChildren()
        {
            List<MaterialSet> list = new List<MaterialSet>();
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                list.Add(new MaterialSet(renderers[i]));
            }
            aliveSets = list.ToArray();
            vfxParticles = GetComponentsInChildren<ParticleSystem>(true);

            Transform vfx = transform.Find("Soldier_Dead/VFX");
            vfxRoot = vfx != null ? vfx.gameObject : null;
        }
    }

    /// <summary>
    /// 把材质先渲染一次到一张 8x8 的 RenderTexture 上，
    /// 逼 Unity 在这张材质第一次上场之前把 shader 变体编译掉。
    /// </summary>
    public static class DeadMaterialPrewarmer
    {
        const int PrewarmLayer = 31;

        static readonly HashSet<Material> _done = new HashSet<Material>();

        public static void Warm(Material material)
        {
            if (material == null || material.shader == null || _done.Contains(material))
            {
                return;
            }
            _done.Add(material);

            GameObject cameraGo = null;
            GameObject quad = null;
            RenderTexture rt = null;

            try
            {
                cameraGo = new GameObject("~DeadMatPrewarmCamera");
                cameraGo.hideFlags = HideFlags.HideAndDontSave;

                Camera cam = cameraGo.AddComponent<Camera>();
                cam.enabled = false;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                cam.orthographicSize = 0.5f;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 10f;
                cam.cullingMask = 1 << PrewarmLayer;

                rt = RenderTexture.GetTemporary(8, 8, 24);
                cam.targetTexture = rt;

                quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.hideFlags = HideFlags.HideAndDontSave;
                quad.layer = PrewarmLayer;
                quad.transform.SetParent(cameraGo.transform, false);
                quad.transform.localPosition = new Vector3(0f, 0f, 2f);
                quad.transform.localScale = Vector3.one * 0.5f;

                MeshRenderer mr = quad.GetComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;

                cam.Render();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[DeadSkinSwitcher] 材质预热失败：" + e.Message);
            }
            finally
            {
                if (quad != null)
                {
                    Object.DestroyImmediate(quad);
                }
                if (cameraGo != null)
                {
                    Object.DestroyImmediate(cameraGo);
                }
                if (rt != null)
                {
                    RenderTexture.ReleaseTemporary(rt);
                }
            }
        }
    }
}
