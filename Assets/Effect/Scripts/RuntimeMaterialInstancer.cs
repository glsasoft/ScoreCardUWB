using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Graphics
{
    /// <summary>
    /// 角色运行时材质实例化器。
    ///
    /// 进入 Play 时，把这个角色下每个 Renderer 的 sharedMaterials 全部换成
    /// <c>new Material(source)</c> 出来的副本，同一份源材质在同一个角色内只实例化一次，
    /// 之后所有脚本（RoleMaterialController / DeadSkinSwitcher / TweenEffectAlpha / 画质那套）
    /// 读写到的都是这份副本，磁盘上的 .mat 资源一个字节都不会被改。
    ///
    /// 退出 Play（或对象销毁）时把 sharedMaterials 还原成进入 Play 时抓到的原始引用，
    /// 并 Destroy 掉自己创建的副本。
    ///
    /// 编辑器预览（Tools/GLSA/角色材质/编辑器预览）：编辑模式也走同一套，副本改成
    /// HideAndDontSave，保存场景前由编辑器脚本统一 Revert，保存后再 Apply。
    /// 所以拖 Timeline 能实时看到死亡 / 溶解变化，但场景和 .mat 都不会被写脏。
    ///
    /// 之所以要它：RoleMaterialController 和 DeadSkinSwitcher 都是 [ExecuteAlways]，
    /// 以前在编辑模式里直接对着 sharedMaterial（即资源本身）SetFloat / 换材质，
    /// 于是 .mat 被写上 _CenterPointPosition、_EnableDissolve、_NILOTOON_DISSOLVE，
    /// 场景里 6 个角色共用同一批资源，一个动全身跟着变。
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class RuntimeMaterialInstancer : MonoBehaviour
    {
        [Header("收集范围")]
        [Tooltip("连 MeshRenderer 一起处理。角色身上一般只有 SkinnedMeshRenderer，默认关。")]
        public bool includeMeshRenderers = false;

        [Tooltip("包含未激活的子物体。死亡后被隐藏的 Mask 槽也一起换成实例，切回来的时候才不会突然串回资源。")]
        public bool includeInactive = true;

        [Tooltip("粒子 / 拖尾 / 线条这类 Renderer 也换。默认关，特效材质通常不需要按角色隔离。")]
        public bool includeParticleRenderers = false;

        readonly Dictionary<Material, Material> _instanceOf = new Dictionary<Material, Material>();
        readonly HashSet<Material> _ownInstances = new HashSet<Material>();
        readonly List<Material> _created = new List<Material>();
        readonly List<Renderer> _renderers = new List<Renderer>();
        readonly List<Material[]> _originalSets = new List<Material[]>();

        /// <summary>是否已经完成一次实例化替换。</summary>
        public bool Applied { get; private set; }

        /// <summary>已经建出来的副本数量，调试用。</summary>
        public int InstanceCount { get { return _created.Count; } }

        void OnEnable()
        {
            if (MaterialPreviewState.Active)
            {
                Apply();
            }
        }

        void OnDisable()
        {
            Revert();
        }

        void OnDestroy()
        {
            Revert();
        }

        /// <summary>
        /// 抓一次所有 Renderer 的材质，逐个换成实例。重复调用是安全的。
        /// Play 模式、或者编辑器预览打开时才会真正执行。
        /// </summary>
        public void Apply()
        {
            if (Applied || !MaterialPreviewState.Active)
            {
                return;
            }

            CollectRenderers();

            for (int i = 0; i < _renderers.Count; i++)
            {
                Renderer r = _renderers[i];
                if (r == null)
                {
                    _originalSets.Add(null);
                    continue;
                }

                Material[] source = r.sharedMaterials;
                _originalSets.Add(source);

                if (source == null || source.Length == 0)
                {
                    continue;
                }

                Material[] instanced = new Material[source.Length];
                for (int k = 0; k < source.Length; k++)
                {
                    instanced[k] = Get(source[k]);
                }
                r.sharedMaterials = instanced;
            }

            Applied = true;
        }

        /// <summary>
        /// 取某个源材质在本角色内的实例。没有就现场建一个（比如死亡材质是运行时才换上的）。
        /// 编辑模式且没开预览时直接返回源材质，不创建任何东西。
        /// </summary>
        public Material Get(Material source)
        {
            if (source == null)
            {
                return null;
            }

            if (!MaterialPreviewState.Active)
            {
                return source;
            }

            // 传进来的本来就是本组件建的副本（renderer 已经被换过一轮了），直接返回，
            // 否则会再套一层实例，参数就写到永远不会被渲染的那份上。
            if (_ownInstances.Contains(source))
            {
                return source;
            }

            Material existing;
            if (_instanceOf.TryGetValue(source, out existing) && existing != null)
            {
                return existing;
            }

            Material copy = new Material(source);
            copy.name = source.name + " (Runtime)";
            // 运行时用 DontSave；编辑器预览再加 Hide，免得出现在 Project 面板/撤销栈里。
            // 两者都不进场景，保存前会被 Revert 掉。
            copy.hideFlags = MaterialPreviewState.EditorOnly
                ? HideFlags.HideAndDontSave
                : HideFlags.DontSave;
            _instanceOf[source] = copy;
            _ownInstances.Add(copy);
            _created.Add(copy);
            return copy;
        }

        /// <summary>
        /// 保存场景前调用：还原 sharedMaterials、销毁副本，但保留预览意愿。
        /// 保存完成后调 <see cref="ResumePreview"/> 再挂回去。
        /// </summary>
        public void SuspendForSceneSave()
        {
            Revert();
        }

        /// <summary>保存/进入编辑模式之后，如果预览还开着就把副本重新挂上。</summary>
        public void ResumePreview()
        {
            if (MaterialPreviewState.Active)
            {
                Apply();
            }
        }

        /// <summary>
        /// 还原 sharedMaterials 并销毁副本。Play 结束、组件禁用、对象销毁时都会被调用。
        /// </summary>
        public void Revert()
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                Renderer r = _renderers[i];
                if (r == null || i >= _originalSets.Count)
                {
                    continue;
                }

                Material[] original = _originalSets[i];
                if (original != null)
                {
                    r.sharedMaterials = original;
                }
            }

            _renderers.Clear();
            _originalSets.Clear();
            Applied = false;
            DestroyCreated();
        }

        void CollectRenderers()
        {
            _renderers.Clear();

            Renderer[] all = GetComponentsInChildren<Renderer>(includeInactive);
            for (int i = 0; i < all.Length; i++)
            {
                Renderer r = all[i];
                if (r == null)
                {
                    continue;
                }

                if (r is SkinnedMeshRenderer)
                {
                    _renderers.Add(r);
                    continue;
                }

                if (r is MeshRenderer && includeMeshRenderers)
                {
                    _renderers.Add(r);
                    continue;
                }

                if (includeParticleRenderers &&
                    (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer))
                {
                    _renderers.Add(r);
                }
            }
        }

        void DestroyCreated()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                Material m = _created[i];
                if (m == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(m);
                }
                else
                {
                    DestroyImmediate(m);
                }
            }

            _created.Clear();
            _instanceOf.Clear();
            _ownInstances.Clear();
        }
    }
}
