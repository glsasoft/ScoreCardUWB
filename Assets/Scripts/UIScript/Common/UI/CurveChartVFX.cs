using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.UI
{
    /// <summary>
    /// 把直线流光特效改为沿任意曲线弯折的线条（只画线，面填充由 <see cref="CurveFillMask"/> 裁切原特效实现）。
    ///
    /// 两种使用方式：
    /// 1. 图表：调用 <see cref="SetCurve"/> 传入图表平滑折线，按基线只保留本特效这一侧的曲线段。
    ///    <see cref="fillDownward"/> 为 true（红队）：保留基线之上的段；为 false（蓝队）：保留基线之下的段，线条上下镜像。
    /// 2. 预览：不调用 SetCurve 时，用 <see cref="points"/> 生成示例曲线，方便在 Prefab 里对比。
    ///
    /// 线条按原特效等比缩放到曲线横向范围（图表宽度）。
    /// 网格会换算到各目标节点自身的局部空间，因此目标节点原有的 Transform（含翻转）与材质剔除设置保持不变。
    /// 特效跟随上级 CanvasGroup 的透明度（3D 特效不吃 CanvasGroup），通过顶点色 alpha 实现。
    /// </summary>
    [ExecuteAlways]
    public class CurveChartVFX : MonoBehaviour
    {
        [Header("预览曲线（仅未调用 SetCurve 时生效，本节点局部坐标，x 需严格递增）")]
        [SerializeField] private List<Vector2> points = new List<Vector2>();
        [Tooltip("基线 y（仅预览）")]
        [SerializeField] private float baselineY;
        [SerializeField, Range(1, 64)] private int samplesPerSegment = 16;

        [Header("方向")]
        [Tooltip("true：保留基线之上的曲线（红队）；false：保留基线之下的曲线，上下镜像（蓝队）")]
        [SerializeField] private bool fillDownward = true;

        [Header("目标")]
        [Tooltip("沿曲线弯折的流光线条")]
        [SerializeField] private MeshFilter[] lineFilters;

        private sealed class Span
        {
            public readonly List<Vector2> Points = new List<Vector2>();
            public readonly List<float> Arcs = new List<float>();
        }

        private sealed class Strip
        {
            public Vector3[] Near;
            public Vector3[] Far;
            public Vector2[] UvNear;
            public Vector2[] UvFar;
        }

        private readonly List<Vector2> _samples = new List<Vector2>();
        private readonly List<Span> _spans = new List<Span>();
        private readonly Dictionary<Object, Mesh> _meshes = new Dictionary<Object, Mesh>();
        private bool _external;
        private bool _dirty;
        private float _baseline;
        private float _totalArc;
        private float _xMin;
        private float _xMax;
        private float _rangeMin;
        private float _rangeMax;
        private float _alpha = 1f;

        /// <param name="polyline">曲线折线，x 递增</param>
        /// <param name="space">折线所在的坐标空间（图表节点）</param>
        /// <param name="baseline">基线 y（space 局部坐标）</param>
        /// <param name="xMin">图表左边缘 x（space 局部坐标）。线条贴图从图表左边缘铺到右边缘，曲线只显示自己那一段</param>
        /// <param name="xMax">图表右边缘 x（space 局部坐标）</param>
        public void SetCurve(IList<Vector2> polyline, Transform space, float baseline, float xMin, float xMax)
        {
            _external = true;
            _spans.Clear();

            int n = polyline == null ? 0 : polyline.Count;
            if (n >= 2)
            {
                var local = new List<Vector2>(n);
                for (int i = 0; i < n; i++)
                    local.Add(ToLocal(space, polyline[i]));
                _baseline = ToLocal(space, new Vector2(0f, baseline)).y;
                BuildSpans(local, true);
                ApplyEdgeOffset(space);
                float left = ToLocal(space, new Vector2(xMin, 0f)).x;
                float right = ToLocal(space, new Vector2(xMax, 0f)).x;
                _rangeMin = Mathf.Min(left, right);
                _rangeMax = Mathf.Max(left, right);
            }

            if (_spans.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            bool wasActive = gameObject.activeSelf;
            gameObject.SetActive(true);
            if (wasActive) Rebuild();
        }

        private float _edgeOffset;

        /// <summary>
        /// 曲线整体沿"外侧"平移的距离（图表节点局部单位）：保留上方的特效向上移，保留下方的向下移。
        /// 曲线是按世界位置生成网格的，直接挪节点位置不起作用，需要用它。下次 SetCurve 起生效。
        /// </summary>
        public void SetEdgeOffset(float offset)
        {
            _edgeOffset = offset;
        }

        private void ApplyEdgeOffset(Transform space)
        {
            if (Mathf.Approximately(_edgeOffset, 0f)) return;

            Vector2 up = ToLocal(space, new Vector2(0f, 1f)) - ToLocal(space, Vector2.zero);
            Vector2 shift = up * (fillDownward ? _edgeOffset : -_edgeOffset);
            for (int i = 0; i < _spans.Count; i++)
            {
                var pts = _spans[i].Points;
                for (int p = 0; p < pts.Count; p++)
                    pts[p] += shift;
            }
        }

        public void Clear()
        {
            _external = true;
            _spans.Clear();
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            _dirty = true;
        }

        private void LateUpdate()
        {
            if (_dirty) Rebuild();

            float alpha = EffectiveAlpha();
            if (!Mathf.Approximately(alpha, _alpha))
            {
                _alpha = alpha;
                ApplyAlpha();
            }
        }

        private void OnDisable()
        {
            foreach (var mesh in _meshes.Values)
            {
                if (Application.isPlaying) Destroy(mesh);
                else DestroyImmediate(mesh);
            }
            _meshes.Clear();
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            _dirty = false;
            if (!isActiveAndEnabled) return;

            if (!_external)
            {
                if (points.Count < 2) return;
                SampleCurve();
                _baseline = baselineY;
                BuildSpans(_samples, false);
                _rangeMin = _xMin;
                _rangeMax = _xMax;
            }
            if (_spans.Count == 0) return;

            _alpha = EffectiveAlpha();

            foreach (var filter in lineFilters)
                WriteRendererMesh(filter, BuildLineStrips(filter));
        }

        private Vector2 ToLocal(Transform space, Vector2 p)
        {
            return transform.InverseTransformPoint(space.TransformPoint(p));
        }

        private float EffectiveAlpha()
        {
            float alpha = 1f;
            for (Transform t = transform; t != null; t = t.parent)
            {
                if (!t.TryGetComponent(out CanvasGroup group)) continue;
                alpha *= group.alpha;
                if (group.ignoreParentGroups) break;
            }
            return alpha;
        }

        // 按基线把折线拆成红/蓝两侧的曲线段，只保留本特效这一侧；弧长按整条折线累计，保证颜色切换处贴图连续
        private void BuildSpans(IList<Vector2> pts, bool splitByBaseline)
        {
            _spans.Clear();
            int n = pts.Count;
            if (n < 2) return;

            var arcs = new float[n];
            _xMin = pts[0].x;
            _xMax = pts[0].x;
            for (int i = 1; i < n; i++)
            {
                arcs[i] = arcs[i - 1] + Vector2.Distance(pts[i - 1], pts[i]);
                _xMin = Mathf.Min(_xMin, pts[i].x);
                _xMax = Mathf.Max(_xMax, pts[i].x);
            }
            _totalArc = Mathf.Max(arcs[n - 1], 1e-4f);

            if (!splitByBaseline)
            {
                var all = new Span();
                for (int i = 0; i < n; i++)
                {
                    all.Points.Add(pts[i]);
                    all.Arcs.Add(arcs[i]);
                }
                _spans.Add(all);
                return;
            }

            var current = new Span();
            bool currentAbove = IsAboveBaseline(pts[0].y);
            current.Points.Add(pts[0]);
            current.Arcs.Add(arcs[0]);

            for (int i = 1; i < n; i++)
            {
                bool above = IsAboveBaseline(pts[i].y);
                if (above != currentAbove)
                {
                    float dy = pts[i].y - pts[i - 1].y;
                    float t = Mathf.Approximately(dy, 0f) ? 0.5f : Mathf.Clamp01((_baseline - pts[i - 1].y) / dy);
                    Vector2 cross = Vector2.Lerp(pts[i - 1], pts[i], t);
                    cross.y = _baseline;
                    float crossArc = Mathf.Lerp(arcs[i - 1], arcs[i], t);

                    current.Points.Add(cross);
                    current.Arcs.Add(crossArc);
                    KeepSpan(current, currentAbove);

                    current = new Span();
                    current.Points.Add(cross);
                    current.Arcs.Add(crossArc);
                    currentAbove = above;
                }
                current.Points.Add(pts[i]);
                current.Arcs.Add(arcs[i]);
            }
            KeepSpan(current, currentAbove);
        }

        // 正好落在基线上的点算作本特效保留的那一侧，否则一条贴着基线的直线（数据全为 0）会被另一侧的特效丢掉
        private bool IsAboveBaseline(float y)
        {
            const float eps = 1e-3f;
            return fillDownward ? y >= _baseline - eps : y > _baseline + eps;
        }

        private void KeepSpan(Span span, bool above)
        {
            if (above == fillDownward && span.Points.Count >= 2)
                _spans.Add(span);
        }

        // 沿曲线的细条带，near 为外侧（红队向上、蓝队向下）
        private List<Strip> BuildLineStrips(MeshFilter quad)
        {
            var strips = new List<Strip>(_spans.Count);
            if (quad == null) return strips;

            float sign = fillDownward ? 1f : -1f;
            RootBounds(quad, out Vector2 min, out Vector2 max);
            float half = (max.y - min.y) * 0.5f;
            float scale = ChartScale(max.x - min.x);
            float rangeWidth = Mathf.Max(_rangeMax - _rangeMin, 1e-4f);
            float alongStart = (_xMin - _rangeMin) / rangeWidth;
            float alongSpan = (_xMax - _xMin) / rangeWidth;
            foreach (var span in _spans)
            {
                int n = span.Points.Count;
                var strip = NewStrip(n);
                for (int i = 0; i < n; i++)
                {
                    Vector2 dir = (span.Points[Mathf.Min(i + 1, n - 1)] - span.Points[Mathf.Max(i - 1, 0)]).normalized;
                    Vector2 outward = new Vector2(-dir.y, dir.x) * sign;
                    strip.Near[i] = span.Points[i] + outward * half * scale;
                    strip.Far[i] = span.Points[i] - outward * half * scale;
                    // 段内按弧长分布，陡坡处贴图不挤压；整段在图表上的起止位置按 x 换算
                    float along = alongStart + span.Arcs[i] / _totalArc * alongSpan;
                    strip.UvNear[i] = QuadUv(quad, along, sign * half);
                    strip.UvFar[i] = QuadUv(quad, along, -sign * half);
                }
                strips.Add(strip);
            }
            return strips;
        }

        // 原 Quad 在本节点局部坐标下的包围矩形
        private void RootBounds(MeshFilter quad, out Vector2 min, out Vector2 max)
        {
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);
            for (int c = 0; c < 4; c++)
            {
                var corner = new Vector3(c < 2 ? -0.5f : 0.5f, c % 2 == 0 ? -0.5f : 0.5f, 0f);
                Vector2 p = transform.InverseTransformPoint(quad.transform.TransformPoint(corner));
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
        }

        // 等比缩放系数：把原 Quad 的宽度缩放到图表宽度
        private float ChartScale(float quadWidth)
        {
            return (_rangeMax - _rangeMin) / Mathf.Max(quadWidth, 1e-4f);
        }

        // 原特效里线所在的 y（本节点局部坐标）；没有线条时取该 Quad 自身中心
        private float LineY(MeshFilter quad)
        {
            Transform line = lineFilters.Length > 0 && lineFilters[0] != null ? lineFilters[0].transform : quad.transform;
            return transform.InverseTransformPoint(line.position).y;
        }

        // 原 Quad 上的贴图坐标：沿图表方向固定为 along（0 = 原 Quad 左端，1 = 右端）；
        // 垂直方向取原直线特效里"离线 offsetY 处"那一点在原 Quad 上的坐标，保持原 Quad 的朝向与尺寸
        private Vector2 QuadUv(MeshFilter quad, float along, float offsetY)
        {
            Transform t = quad.transform;
            Vector3 center = transform.InverseTransformPoint(t.position);
            Vector3 local = t.InverseTransformPoint(transform.TransformPoint(new Vector3(center.x, LineY(quad) + offsetY, 0f)));
            var uv = new Vector2(local.x + 0.5f, local.y + 0.5f);

            Vector3 axis = t.InverseTransformVector(transform.TransformVector(Vector3.right));
            if (Mathf.Abs(axis.x) >= Mathf.Abs(axis.y))
                uv.x = axis.x >= 0f ? along : 1f - along;
            else
                uv.y = axis.y >= 0f ? along : 1f - along;
            return uv;
        }

        private static Strip NewStrip(int n)
        {
            return new Strip
            {
                Near = new Vector3[n],
                Far = new Vector3[n],
                UvNear = new Vector2[n],
                UvFar = new Vector2[n],
            };
        }

        private void WriteRendererMesh(MeshFilter filter, List<Strip> strips)
        {
            if (filter == null) return;
            Matrix4x4 rootToLocal = filter.transform.worldToLocalMatrix * transform.localToWorldMatrix;
            // 原 Quad 法线为 -Z；目标节点若被翻转成背面朝向镜头，网格也保持背面朝向，才能与材质的剔除设置匹配
            bool faceCamera = Vector3.Dot(filter.transform.TransformDirection(Vector3.back), transform.TransformDirection(Vector3.back)) > 0f;
            Mesh mesh = GetMesh(filter);
            // 蓝队整体上下镜像，条带近侧在下，三角形绕序要反过来才能保持朝向
            WriteStrips(mesh, rootToLocal, strips, faceCamera == fillDownward);
            filter.sharedMesh = mesh;
        }

        private void WriteStrips(Mesh mesh, Matrix4x4 matrix, List<Strip> strips, bool order)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var colors = new List<Color32>();
            var triangles = new List<int>();

            var color = new Color32(255, 255, 255, AlphaByte());
            Vector3 normal = matrix.MultiplyVector(Vector3.back).normalized;

            foreach (var strip in strips)
            {
                int n = strip.Near.Length;
                int start = vertices.Count;
                for (int i = 0; i < n; i++)
                {
                    vertices.Add(matrix.MultiplyPoint3x4(strip.Near[i]));
                    vertices.Add(matrix.MultiplyPoint3x4(strip.Far[i]));
                    uvs.Add(strip.UvNear[i]);
                    uvs.Add(strip.UvFar[i]);
                    normals.Add(normal);
                    normals.Add(normal);
                    colors.Add(color);
                    colors.Add(color);
                }
                for (int i = 0; i < n - 1; i++)
                {
                    int a = start + i * 2, b = a + 2, c = a + 1, d = a + 3;
                    if (order)
                    {
                        triangles.Add(a); triangles.Add(b); triangles.Add(c);
                        triangles.Add(b); triangles.Add(d); triangles.Add(c);
                    }
                    else
                    {
                        triangles.Add(a); triangles.Add(c); triangles.Add(b);
                        triangles.Add(b); triangles.Add(c); triangles.Add(d);
                    }
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private byte AlphaByte()
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(_alpha) * 255f);
        }

        private void ApplyAlpha()
        {
            var color = new Color32(255, 255, 255, AlphaByte());
            foreach (var mesh in _meshes.Values)
            {
                if (mesh == null) continue;
                var colors = new Color32[mesh.vertexCount];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = color;
                mesh.colors32 = colors;
            }
        }

        private Mesh GetMesh(Object owner)
        {
            if (!_meshes.TryGetValue(owner, out var mesh) || mesh == null)
            {
                mesh = new Mesh { name = "CurveChartVFX_" + owner.name, hideFlags = HideFlags.DontSave };
                mesh.MarkDynamic();
                _meshes[owner] = mesh;
            }
            return mesh;
        }

        private void SampleCurve()
        {
            int n = points.Count;

            // Fritsch-Carlson 单调三次插值的切线
            var tangents = new float[n];
            for (int i = 0; i < n; i++)
            {
                float prev = Slope(Mathf.Max(i - 1, 0));
                float next = Slope(Mathf.Min(i, n - 2));
                tangents[i] = prev * next <= 0f ? 0f : (prev + next) * 0.5f;
            }
            for (int i = 0; i < n - 1; i++)
            {
                float slope = Slope(i);
                if (Mathf.Approximately(slope, 0f))
                {
                    tangents[i] = tangents[i + 1] = 0f;
                    continue;
                }
                float a = tangents[i] / slope;
                float b = tangents[i + 1] / slope;
                float s = a * a + b * b;
                if (s > 9f)
                {
                    float tau = 3f / Mathf.Sqrt(s);
                    tangents[i] = tau * a * slope;
                    tangents[i + 1] = tau * b * slope;
                }
            }

            _samples.Clear();
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p0 = points[i];
                Vector2 p1 = points[i + 1];
                float h = p1.x - p0.x;
                for (int k = 0; k < samplesPerSegment; k++)
                {
                    float t = (float)k / samplesPerSegment;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    float y = (2f * t3 - 3f * t2 + 1f) * p0.y
                              + (t3 - 2f * t2 + t) * h * tangents[i]
                              + (-2f * t3 + 3f * t2) * p1.y
                              + (t3 - t2) * h * tangents[i + 1];
                    _samples.Add(new Vector2(p0.x + h * t, y));
                }
            }
            _samples.Add(points[n - 1]);
        }

        private float Slope(int i)
        {
            return (points[i + 1].y - points[i].y) / (points[i + 1].x - points[i].x);
        }
    }
}
