using System.Collections.Generic;
using LetsPiu.Base;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace LetsPiu.UI
{
    /// <summary>
    /// 把面特效裁成"曲线与基线之间"的形状：特效本身的位置、缩放、材质参数都不改，只限制显示区域。
    /// 本节点生成只写模板缓冲的遮罩网格，面特效的材质（运行时复制一份）改为只在遮罩区域内显示。
    /// 遮罩网格在本节点局部坐标下生成，节点放在图表的同一坐标系下即可（不需要和面特效对齐）。
    /// </summary>
    public class CurveFillMask : MonoBehaviour
    {
        [Tooltip("被裁切的面特效根节点")]
        [SerializeField] private GameObject fill;
        [Tooltip("true：保留基线之上（红队）；false：保留基线之下（蓝队）")]
        [SerializeField] private bool above = true;
        [Tooltip("模板位，各遮罩互不相同；UGUI Mask 从低位开始用，这里用高位")]
        [SerializeField, Range(0, 7)] private int stencilBit = 6;
        [SerializeField] private Shader maskShader;
        [Header("调试：运行时取消勾选，查看面特效原本 0.6 缩放的完整大小")]
        [Tooltip("关闭后不按曲线裁切，显示面特效原本的完整大小（运行时调试用）")]
        [FormerlySerializedAs("clip")]
        [SerializeField] private bool clipByCurve = true;

        public Transform FillTransform => fill != null ? fill.transform : null;

        private static readonly int StencilId = Shader.PropertyToID("_Stencil");
        private static readonly int StencilCompId = Shader.PropertyToID("_StencilComp");
        private static readonly int StencilOpId = Shader.PropertyToID("_StencilOp");
        private static readonly int StencilReadMaskId = Shader.PropertyToID("_StencilReadMask");
        private static readonly int StencilWriteMaskId = Shader.PropertyToID("_StencilWriteMask");

        private readonly List<Material> _fillMaterials = new List<Material>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();
        private Mesh _mesh;
        private MeshRenderer _maskRenderer;
        private bool _inUse;
        private bool _hasArea;

        private void Awake()
        {
            Init();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _mesh != null) Apply();
        }

        // 页面可能先于本组件 Awake 调用接口
        private void Init()
        {
            if (_mesh != null) return;

            int bit = 1 << stencilBit;
            var maskMaterial = new Material(maskShader);
            maskMaterial.SetFloat(StencilId, bit);

            _mesh = new Mesh { name = "CurveFillMask" };
            _mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _maskRenderer = gameObject.AddComponent<MeshRenderer>();
            _maskRenderer.sharedMaterial = maskMaterial;

            foreach (var r in fill.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var m in r.materials)
                {
                    m.SetFloat(StencilId, bit);
                    m.SetFloat(StencilOpId, (float)StencilOp.Keep);
                    m.SetFloat(StencilReadMaskId, bit);
                    m.SetFloat(StencilWriteMaskId, 0f);
                    _fillMaterials.Add(m);
                }
            }
            Clear();
        }

        // 不裁切时只要在用就整块显示；裁切时还要本侧有面积
        private void Apply()
        {
            var comp = clipByCurve ? CompareFunction.Equal : CompareFunction.Always;
            foreach (var m in _fillMaterials)
                m.SetFloat(StencilCompId, (float)comp);
            fill.SetActive(_inUse && (_hasArea || !clipByCurve));
        }

        /// <param name="polyline">曲线折线，x 递增</param>
        /// <param name="space">折线所在的坐标空间（图表节点）</param>
        /// <param name="baseline">基线 y（space 局部坐标）</param>
        public void SetCurve(IList<Vector2> polyline, Transform space, float baseline)
        {
            Init();
            _vertices.Clear();
            _triangles.Clear();

            float b = ToLocal(space, new Vector2(0f, baseline)).y;
            bool hasArea = false;
            Vector2 prev = default;
            for (int i = 0; i < polyline.Count; i++)
            {
                Vector2 p = ToLocal(space, polyline[i]);
                if (i > 0 && (prev.y > b) != (p.y > b))
                    AddColumn(Vector2.Lerp(prev, p, (b - prev.y) / (p.y - prev.y)), b);
                hasArea |= above ? p.y > b : p.y < b;
                AddColumn(p, b);
                prev = p;
            }

            _mesh.Clear();
            if (hasArea)
            {
                _mesh.SetVertices(_vertices);
                _mesh.SetTriangles(_triangles, 0);
                _mesh.RecalculateBounds();
            }
            _inUse = true;
            _hasArea = hasArea;
            Apply();
        }

        public void Clear()
        {
            Init();
            _mesh.Clear();
            _inUse = false;
            _hasArea = false;
            Apply();
        }

        /// <summary>遮罩排在面特效之前绘制，返回用到的最大层级</summary>
        public int SetSortingOrder(int order, List<Renderer> renderers = null)
        {
            Init();
            _maskRenderer.sortingOrder = ++order;
            return EffectUtility.SetEffectOrder(fill, order, renderers);
        }

        // 每个点补一列竖条：本侧取曲线，另一侧压到基线上（面积为 0）
        private void AddColumn(Vector2 p, float b)
        {
            float y = above ? Mathf.Max(p.y, b) : Mathf.Min(p.y, b);
            int start = _vertices.Count;
            _vertices.Add(new Vector3(p.x, y));
            _vertices.Add(new Vector3(p.x, b));
            if (start < 2) return;
            _triangles.Add(start - 2); _triangles.Add(start); _triangles.Add(start - 1);
            _triangles.Add(start); _triangles.Add(start + 1); _triangles.Add(start - 1);
        }

        private Vector2 ToLocal(Transform space, Vector2 p)
        {
            return transform.InverseTransformPoint(space.TransformPoint(p));
        }

        private void OnDestroy()
        {
            foreach (var m in _fillMaterials)
                Destroy(m);
            if (_maskRenderer != null) Destroy(_maskRenderer.sharedMaterial);
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
