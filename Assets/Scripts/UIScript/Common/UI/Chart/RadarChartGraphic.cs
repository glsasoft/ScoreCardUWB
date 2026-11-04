using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 多维雷达图（默认 5 维）。自绘网格 + 多条样本（描边 / 半透明填充）。
    /// 中心点与 5 个外顶点使用相对 Chart 矩形的归一化坐标（0~1），可在 Inspector 中微调对齐美术图。
    /// 顶点顺序：D1 顶 → D2 右 → D3 右下 → D4 左下 → D5 左（顺时针）。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Chart/Radar Chart Graphic")]
    public class RadarChartGraphic : MaskableGraphic
    {
        public const int MaxPlayers = 5;
        public const int DefaultDimensionCount = 5;

        static readonly Color s_GridColor = new Color(0x7E / 255f, 0x7E / 255f, 0x7E / 255f, 1f);

        static readonly Color[] s_DefaultPlayerColors =
        {
            new Color(0xFA / 255f, 0x58 / 255f, 0x18 / 255f, 1f),
            new Color(0x18 / 255f, 0x9D / 255f, 0xFA / 255f, 1f),
            new Color(0xAD / 255f, 0x18 / 255f, 0xFA / 255f, 1f),
            new Color(0x18 / 255f, 0xFA / 255f, 0x58 / 255f, 1f),
            new Color(0xFA / 255f, 0xF8 / 255f, 0x18 / 255f, 1f),
        };

        /// <summary>
        /// 默认外顶点（归一化）：顶可偏 X，左右偏 Y，底两点偏 X（略内收，避免贴死边角）。
        /// </summary>
        static readonly Vector2[] s_DefaultOuterNormalized =
        {
            new Vector2(0.50f, 1.00f), // D1 顶
            new Vector2(1.00f, 0.52f), // D2 右（Y 可下调）
            new Vector2(0.78f, 0.00f), // D3 右下（X 可内收）
            new Vector2(0.22f, 0.00f), // D4 左下（X 可内收）
            new Vector2(0.00f, 0.52f), // D5 左（Y 可下调）
        };

        [Header("雷达图")]
        [SerializeField]
        [Min(3)]
        int m_DimensionCount = DefaultDimensionCount;

        [SerializeField]
        Vector2 m_ValueRange = new Vector2(0f, 100f);

        [Header("几何（归一化 0~1，相对 Chart 矩形；可在 Editor 微调对齐美术）")]
        [SerializeField]
        [Tooltip("中心点。x: 0=左 1=右；y: 0=底 1=顶。略大于 0.5 的 Y 即中心上偏。")]
        Vector2 m_CenterNormalized = new Vector2(0.5f, 0.62f);

        [SerializeField]
        [Tooltip("外圈 5 顶点，顺序 D1顶→D2右→D3右下→D4左下→D5左。顶调 X，左右调 Y，底两点调 X。")]
        Vector2[] m_OuterVerticesNormalized = CreateDefaultOuterVertices();

        [Header("网格")]
        [SerializeField]
        bool m_DrawGrid = true;

        [SerializeField]
        [Min(1)]
        int m_GridLevels = 8;

        [SerializeField]
        Color m_GridColor = s_GridColor;

        [SerializeField]
        float m_GridLineWidth = 2f;

        [SerializeField]
        bool m_DrawAxisLines = true;

        [SerializeField]
        float m_AxisLineWidth = 2f;

        [Header("样本（最多 5 个玩家）")]
        [SerializeField]
        List<RadarSeries> m_Series = new List<RadarSeries>();

        readonly List<Vector2> m_PointBuffer = new List<Vector2>(8);
        readonly List<Vector2> m_RingBuffer = new List<Vector2>(8);

        public int DimensionCount
        {
            get => m_DimensionCount;
            set
            {
                m_DimensionCount = Mathf.Max(3, value);
                EnsureOuterVertexCount();
                SetVerticesDirty();
            }
        }

        public Vector2 ValueRange
        {
            get => m_ValueRange;
            set
            {
                m_ValueRange = value;
                SetVerticesDirty();
            }
        }

        /// <summary>中心点归一化坐标（0~1）。</summary>
        public Vector2 CenterNormalized
        {
            get => m_CenterNormalized;
            set
            {
                m_CenterNormalized = value;
                SetVerticesDirty();
            }
        }

        public int SeriesCount => m_Series.Count;

        public RadarSeries GetSeries(int index) => m_Series[index];

        public static Color GetDefaultPlayerColor(int index)
        {
            if (index < 0 || index >= s_DefaultPlayerColors.Length)
            {
                return Color.white;
            }

            return s_DefaultPlayerColors[index];
        }

        public static Color GetDefaultFillColor(int index, float alpha = 0.28f)
        {
            var c = GetDefaultPlayerColor(index);
            c.a = alpha;
            return c;
        }

        /// <summary>设置某个外顶点的归一化坐标（0~1）。</summary>
        public void SetOuterVertexNormalized(int index, Vector2 normalized)
        {
            EnsureOuterVertexCount();
            if (index < 0 || index >= m_OuterVerticesNormalized.Length) return;
            m_OuterVerticesNormalized[index] = normalized;
            SetVerticesDirty();
        }

        public Vector2 GetOuterVertexNormalized(int index)
        {
            EnsureOuterVertexCount();
            if (index < 0 || index >= m_OuterVerticesNormalized.Length) return Vector2.zero;
            return m_OuterVerticesNormalized[index];
        }

        /// <summary>设置玩家数量（0~5），并按默认色补齐 Series 配置。</summary>
        public void SetPlayerCount(int count)
        {
            count = Mathf.Clamp(count, 0, MaxPlayers);
            while (m_Series.Count < count)
            {
                int i = m_Series.Count;
                m_Series.Add(CreateDefaultSeries(i));
            }

            while (m_Series.Count > count)
            {
                m_Series.RemoveAt(m_Series.Count - 1);
            }

            for (int i = 0; i < m_Series.Count; i++)
            {
                EnsureValueSlotCount(m_Series[i]);
                m_Series[i].visible = true;
            }

            SetVerticesDirty();
        }

        /// <summary>写入指定玩家的各维数值（长度不足会按最小值补齐，超出截断）。</summary>
        public void SetPlayerValues(int playerIndex, IList<float> values)
        {
            if (playerIndex < 0 || playerIndex >= m_Series.Count)
            {
                Debug.LogWarning($"[RadarChartGraphic] playerIndex {playerIndex} out of range (count={m_Series.Count}).");
                return;
            }

            var s = m_Series[playerIndex];
            EnsureValueSlotCount(s);
            for (int i = 0; i < m_DimensionCount; i++)
            {
                s.values[i] = values != null && i < values.Count ? values[i] : m_ValueRange.x;
            }

            s.visible = true;
            SetVerticesDirty();
        }

        /// <summary>一次设置全部玩家数据；players.Count 即为显示人数（1~5）。</summary>
        public void SetPlayers(IList<IList<float>> players)
        {
            int count = players == null ? 0 : Mathf.Min(players.Count, MaxPlayers);
            SetPlayerCount(count);
            for (int i = 0; i < count; i++)
            {
                SetPlayerValues(i, players[i]);
            }
        }

        public void ClearPlayers()
        {
            m_Series.Clear();
            SetVerticesDirty();
        }

        public void SetSeriesStyle(int playerIndex, Color lineColor, Color fillColor, float lineWidth,
            bool drawLine = true, bool drawFill = true)
        {
            if (playerIndex < 0 || playerIndex >= m_Series.Count) return;

            var s = m_Series[playerIndex];
            s.lineColor = lineColor;
            s.fillColor = fillColor;
            s.lineWidth = lineWidth;
            s.drawLine = drawLine;
            s.drawFill = drawFill;
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false;
            color = Color.white;
            m_DimensionCount = DefaultDimensionCount;
            m_ValueRange = new Vector2(0f, 100f);
            m_CenterNormalized = new Vector2(0.5f, 0.62f);
            m_OuterVerticesNormalized = CreateDefaultOuterVertices();
            m_DrawGrid = true;
            m_GridLevels = 8;
            m_GridColor = s_GridColor;
            m_GridLineWidth = 2f;
            m_DrawAxisLines = true;
            m_AxisLineWidth = 2f;
            m_Series.Clear();
            for (int i = 0; i < MaxPlayers; i++)
            {
                m_Series.Add(CreateDefaultSeries(i));
                FillPreviewValues(m_Series[i], i);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_DimensionCount = Mathf.Max(3, m_DimensionCount);
            m_GridLevels = Mathf.Max(1, m_GridLevels);
            if (m_ValueRange.x > m_ValueRange.y)
            {
                (m_ValueRange.x, m_ValueRange.y) = (m_ValueRange.y, m_ValueRange.x);
            }

            EnsureOuterVertexCount();

            for (int i = 0; i < m_Series.Count; i++)
            {
                if (m_Series[i] == null)
                {
                    m_Series[i] = CreateDefaultSeries(i);
                }

                EnsureValueSlotCount(m_Series[i]);
            }

            SetVerticesDirty();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!IsActive()) return;

            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0.01f || rect.height <= 0.01f) return;

            EnsureOuterVertexCount();
            Vector2 center = NormalizedToLocal(rect, m_CenterNormalized);

            if (m_DrawGrid)
            {
                DrawGrid(vh, rect, center);
            }

            for (int i = 0; i < m_Series.Count; i++)
            {
                var s = m_Series[i];
                if (s == null || !s.visible || !s.drawFill) continue;
                if (!BuildSeriesPoints(s, rect, center, m_PointBuffer)) continue;
                DrawPolygonFill(vh, center, m_PointBuffer, s.fillColor);
            }

            for (int i = 0; i < m_Series.Count; i++)
            {
                var s = m_Series[i];
                if (s == null || !s.visible || !s.drawLine) continue;
                if (!BuildSeriesPoints(s, rect, center, m_PointBuffer)) continue;
                DrawClosedPolylineThick(vh, m_PointBuffer, s.lineColor, s.lineWidth);
            }
        }

        void DrawGrid(VertexHelper vh, Rect rect, Vector2 center)
        {
            int levels = Mathf.Max(1, m_GridLevels);
            for (int level = 1; level <= levels; level++)
            {
                float t = level / (float)levels;
                BuildRingPoints(rect, center, t, m_RingBuffer);
                DrawClosedPolylineThick(vh, m_RingBuffer, m_GridColor, m_GridLineWidth);
            }

            if (!m_DrawAxisLines) return;

            for (int i = 0; i < m_DimensionCount; i++)
            {
                Vector2 outer = DimensionToLocal(rect, center, i, 1f);
                m_PointBuffer.Clear();
                m_PointBuffer.Add(center);
                m_PointBuffer.Add(outer);
                DrawPolylineThick(vh, m_PointBuffer, m_GridColor, m_AxisLineWidth);
            }
        }

        bool BuildSeriesPoints(RadarSeries series, Rect rect, Vector2 center, List<Vector2> output)
        {
            output.Clear();
            if (series.values == null || series.values.Count == 0) return false;

            EnsureValueSlotCount(series);
            for (int i = 0; i < m_DimensionCount; i++)
            {
                float t = NormalizeValue(series.values[i]);
                output.Add(DimensionToLocal(rect, center, i, t));
            }

            return output.Count >= 3;
        }

        void BuildRingPoints(Rect rect, Vector2 center, float normalized, List<Vector2> output)
        {
            output.Clear();
            for (int i = 0; i < m_DimensionCount; i++)
            {
                output.Add(DimensionToLocal(rect, center, i, normalized));
            }
        }

        Vector2 DimensionToLocal(Rect rect, Vector2 center, int dimensionIndex, float normalized)
        {
            float t = Mathf.Clamp01(normalized);
            Vector2 outer = NormalizedToLocal(rect, m_OuterVerticesNormalized[dimensionIndex]);
            return Vector2.LerpUnclamped(center, outer, t);
        }

        static Vector2 NormalizedToLocal(Rect rect, Vector2 normalized)
        {
            return new Vector2(
                Mathf.LerpUnclamped(rect.xMin, rect.xMax, normalized.x),
                Mathf.LerpUnclamped(rect.yMin, rect.yMax, normalized.y));
        }

        float NormalizeValue(float value)
        {
            float min = m_ValueRange.x;
            float max = m_ValueRange.y;
            if (Mathf.Approximately(min, max)) return 0f;
            return Mathf.Clamp01((value - min) / (max - min));
        }

        void EnsureOuterVertexCount()
        {
            if (m_OuterVerticesNormalized == null || m_OuterVerticesNormalized.Length == 0)
            {
                m_OuterVerticesNormalized = CreateDefaultOuterVertices();
            }

            int need = Mathf.Max(3, m_DimensionCount);
            if (m_OuterVerticesNormalized.Length == need) return;

            var next = new Vector2[need];
            for (int i = 0; i < need; i++)
            {
                if (i < m_OuterVerticesNormalized.Length)
                {
                    next[i] = m_OuterVerticesNormalized[i];
                }
                else if (i < s_DefaultOuterNormalized.Length)
                {
                    next[i] = s_DefaultOuterNormalized[i];
                }
                else
                {
                    float a = i / (float)need;
                    next[i] = new Vector2(0.5f + 0.5f * Mathf.Cos(a * Mathf.PI * 2f),
                        0.5f + 0.5f * Mathf.Sin(a * Mathf.PI * 2f));
                }
            }

            m_OuterVerticesNormalized = next;
        }

        static Vector2[] CreateDefaultOuterVertices()
        {
            var arr = new Vector2[s_DefaultOuterNormalized.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = s_DefaultOuterNormalized[i];
            }

            return arr;
        }

        static void DrawPolygonFill(VertexHelper vh, Vector2 center, IList<Vector2> pts, Color color)
        {
            int n = pts.Count;
            if (n < 3 || color.a <= 0.001f) return;

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                AddTri(vh, center, color, pts[i], color, pts[j], color);
            }
        }

        static void DrawPolylineThick(VertexHelper vh, IList<Vector2> localPts, Color color, float width, float miterLimit = 4f)
        {
            int n = localPts.Count;
            if (n < 2 || width <= 0f || color.a <= 0.001f) return;

            float halfW = width * 0.5f;
            int baseIdx = vh.currentVertCount;

            for (int i = 0; i < n; i++)
            {
                Vector2 p = localPts[i];
                Vector2 offset;

                if (i == 0)
                {
                    Vector2 dir = SafeNormalize(localPts[1] - localPts[0]);
                    offset = new Vector2(-dir.y, dir.x) * halfW;
                }
                else if (i == n - 1)
                {
                    Vector2 dir = SafeNormalize(localPts[n - 1] - localPts[n - 2]);
                    offset = new Vector2(-dir.y, dir.x) * halfW;
                }
                else
                {
                    Vector2 dirPrev = SafeNormalize(localPts[i] - localPts[i - 1]);
                    Vector2 dirNext = SafeNormalize(localPts[i + 1] - localPts[i]);
                    Vector2 normalPrev = new Vector2(-dirPrev.y, dirPrev.x);
                    Vector2 miter = SafeNormalize(normalPrev + new Vector2(-dirNext.y, dirNext.x));
                    float dot = Vector2.Dot(miter, normalPrev);
                    float len = Mathf.Abs(dot) > 0.01f
                        ? Mathf.Min(halfW / dot, halfW * miterLimit)
                        : halfW;
                    offset = miter * len;
                }

                vh.AddVert((Vector3)(p + offset), color, Vector2.zero);
                vh.AddVert((Vector3)(p - offset), color, Vector2.zero);
            }

            for (int i = 0; i < n - 1; i++)
            {
                int v0 = baseIdx + i * 2;
                int v1 = baseIdx + i * 2 + 1;
                int v2 = baseIdx + (i + 1) * 2;
                int v3 = baseIdx + (i + 1) * 2 + 1;
                vh.AddTriangle(v0, v2, v1);
                vh.AddTriangle(v1, v2, v3);
            }
        }

        static void DrawClosedPolylineThick(
            VertexHelper vh, IList<Vector2> localPts, Color color, float width, float miterLimit = 4f)
        {
            int n = localPts.Count;
            if (n < 3 || width <= 0f || color.a <= 0.001f) return;

            float halfW = width * 0.5f;
            int baseIdx = vh.currentVertCount;

            for (int i = 0; i < n; i++)
            {
                Vector2 p = localPts[i];
                Vector2 prev = localPts[(i - 1 + n) % n];
                Vector2 next = localPts[(i + 1) % n];

                Vector2 dirPrev = SafeNormalize(p - prev);
                Vector2 dirNext = SafeNormalize(next - p);
                Vector2 normalPrev = new Vector2(-dirPrev.y, dirPrev.x);
                Vector2 miter = SafeNormalize(normalPrev + new Vector2(-dirNext.y, dirNext.x));

                float dot = Vector2.Dot(miter, normalPrev);
                float len = Mathf.Abs(dot) > 0.01f
                    ? Mathf.Min(halfW / dot, halfW * miterLimit)
                    : halfW;
                Vector2 offset = miter * len;

                vh.AddVert((Vector3)(p + offset), color, Vector2.zero);
                vh.AddVert((Vector3)(p - offset), color, Vector2.zero);
            }

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                int v0 = baseIdx + i * 2;
                int v1 = baseIdx + i * 2 + 1;
                int v2 = baseIdx + j * 2;
                int v3 = baseIdx + j * 2 + 1;
                vh.AddTriangle(v0, v2, v1);
                vh.AddTriangle(v1, v2, v3);
            }
        }

        static void AddTri(
            VertexHelper vh,
            Vector2 p0, Color c0,
            Vector2 p1, Color c1,
            Vector2 p2, Color c2)
        {
            int idx = vh.currentVertCount;
            vh.AddVert((Vector3)p0, c0, Vector2.zero);
            vh.AddVert((Vector3)p1, c1, Vector2.zero);
            vh.AddVert((Vector3)p2, c2, Vector2.zero);
            vh.AddTriangle(idx, idx + 1, idx + 2);
        }

        static Vector2 SafeNormalize(Vector2 v)
        {
            float m = v.magnitude;
            return m < 1e-6f ? Vector2.right : v / m;
        }

        void EnsureValueSlotCount(RadarSeries series)
        {
            if (series.values == null)
            {
                series.values = new List<float>(m_DimensionCount);
            }

            while (series.values.Count < m_DimensionCount)
            {
                series.values.Add(m_ValueRange.x);
            }

            while (series.values.Count > m_DimensionCount)
            {
                series.values.RemoveAt(series.values.Count - 1);
            }
        }

        static RadarSeries CreateDefaultSeries(int index)
        {
            return new RadarSeries
            {
                name = $"Player{index + 1}",
                values = new List<float>(),
                lineColor = GetDefaultPlayerColor(index),
                fillColor = GetDefaultFillColor(index),
                lineWidth = 3f,
                drawLine = true,
                drawFill = true,
                visible = true,
            };
        }

#if UNITY_EDITOR
        static void FillPreviewValues(RadarSeries series, int index)
        {
            series.values = new List<float>
            {
                55 + (index * 7) % 40,
                70 - (index * 5) % 35,
                45 + (index * 9) % 45,
                80 - (index * 6) % 40,
                60 + (index * 8) % 30,
            };
        }
#endif

        [Serializable]
        public class RadarSeries
        {
            public string name;
            public List<float> values = new List<float>();
            public Color lineColor = Color.white;
            public Color fillColor = new Color(1f, 1f, 1f, 0.28f);
            public float lineWidth = 3f;
            public bool drawLine = true;
            public bool drawFill = true;
            public bool visible = true;
        }
    }
}
