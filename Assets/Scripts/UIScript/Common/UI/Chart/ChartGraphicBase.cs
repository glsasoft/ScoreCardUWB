using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 折线/曲线图表的抽象基类，提供：
    /// 1. 数据值域 → 本组件 RectTransform 局部坐标的映射（<see cref="ValueToLocal"/>）
    /// 2. 通用几何工具方法：粗折线、矩形带、跨基线双色填充、单侧渐变填充
    /// 3. UGUI 重绘触发（继承自 <see cref="MaskableGraphic"/>，OnValidate 自动重绘，编辑器内可即时预览）
    ///
    /// 子类只需要重写 <see cref="DrawChart"/>，根据自己的数据形态调用工具方法即可。
    /// 当前内置子类：
    /// - <see cref="LineChartGraphic"/>      : 折线（支持多 Series → 单/双/多线）
    /// - <see cref="SplitAreaChartGraphic"/> : 折线 + 上下双色分区填充
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    [DisallowMultipleComponent]
    public abstract class ChartGraphicBase : MaskableGraphic
    {
        // -------------------------- 序列化字段 --------------------------

        [Header("数据值域（必须与 Prefab 上写死的坐标轴文字保持一致）")]
        [SerializeField]
        [Tooltip("X 轴的数据范围（最小值, 最大值）。例：时间轴 0~12 分钟。")]
        protected Vector2 m_ValueRangeX = new Vector2(0f, 10f);

        [SerializeField]
        [Tooltip("Y 轴的数据范围（最小值, 最大值）。例：积分领先 -2000 ~ +2000。")]
        protected Vector2 m_ValueRangeY = new Vector2(0f, 100f);

        [SerializeField]
        [Tooltip("超出值域的数据点处理策略。")]
        protected ClampMode m_ClampMode = ClampMode.ClampToEdge;

        // -------------------------- 公共属性 --------------------------

        public Vector2 ValueRangeX
        {
            get => m_ValueRangeX;
            set { m_ValueRangeX = value; SetVerticesDirty(); }
        }

        public Vector2 ValueRangeY
        {
            get => m_ValueRangeY;
            set { m_ValueRangeY = value; SetVerticesDirty(); }
        }

        public ClampMode ClampType
        {
            get => m_ClampMode;
            set { m_ClampMode = value; SetVerticesDirty(); }
        }

        // -------------------------- 坐标映射 --------------------------

        /// <summary>
        /// 将数据坐标映射到本组件 RectTransform 的局部坐标（受 Pivot 影响）。
        /// 外层（Page 脚本）放置关键点 GameObject 时也使用此方法，保证位置和曲线对齐。
        /// </summary>
        public Vector2 ValueToLocal(Vector2 dataPoint)
        {
            return ValueToLocal(dataPoint.x, dataPoint.y);
        }

        public Vector2 ValueToLocal(float x, float y)
        {
            Rect r = GetPixelAdjustedRect();

            float tx = SafeInverseLerp(m_ValueRangeX.x, m_ValueRangeX.y, x);
            float ty = SafeInverseLerp(m_ValueRangeY.x, m_ValueRangeY.y, y);

            if (m_ClampMode == ClampMode.ClampToEdge)
            {
                tx = Mathf.Clamp01(tx);
                ty = Mathf.Clamp01(ty);
            }

            return new Vector2(
                Mathf.LerpUnclamped(r.xMin, r.xMax, tx),
                Mathf.LerpUnclamped(r.yMin, r.yMax, ty));
        }

        /// <summary>
        /// 把数据点列表批量映射成本地坐标，写入复用列表。
        /// </summary>
        protected void MapPointsToLocal(IList<Vector2> input, List<Vector2> output)
        {
            output.Clear();
            if (input == null) return;
            for (int i = 0; i < input.Count; ++i)
            {
                output.Add(ValueToLocal(input[i]));
            }
        }

        private static float SafeInverseLerp(float a, float b, float v)
        {
            if (Mathf.Approximately(a, b)) return 0f;
            return (v - a) / (b - a);
        }

        // -------------------------- Mesh 绘制入口 --------------------------

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!IsActive())
            {
                return;
            }

            DrawChart(vh);
        }

        /// <summary>
        /// 子类实现：根据自身数据生成图表 Mesh。
        /// vh 已被基类 Clear，子类只管 AddVert/AddTriangle。
        /// </summary>
        protected abstract void DrawChart(VertexHelper vh);

        // -------------------------- 通用绘制工具：粗折线 --------------------------

        /// <summary>
        /// 在已经映射到本地坐标的折线点序列上画一条粗线（带斜接拐点）。
        /// </summary>
        /// <param name="vh">VertexHelper</param>
        /// <param name="localPts">折线点（本地坐标）</param>
        /// <param name="color">线条颜色</param>
        /// <param name="width">线条宽度（像素）</param>
        /// <param name="miterLimit">斜接长度上限倍数，过尖锐拐点时退化为截断，避免长矛状刺出</param>
        protected static void DrawPolylineThick(
            VertexHelper vh, IList<Vector2> localPts, Color color, float width, float miterLimit = 4f)
        {
            int n = localPts.Count;
            if (n < 2 || width <= 0f) return;

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
                int v0 = baseIdx + i * 2;        // i 上侧
                int v1 = baseIdx + i * 2 + 1;    // i 下侧
                int v2 = baseIdx + (i + 1) * 2;  // i+1 上侧
                int v3 = baseIdx + (i + 1) * 2 + 1; // i+1 下侧

                vh.AddTriangle(v0, v2, v1);
                vh.AddTriangle(v1, v2, v3);
            }
        }

        // -------------------------- 通用绘制工具：粗折线沿基线拆分双色 --------------------------

        // 拆色绘制时复用：把同一段颜色的连续折线收进这里，逐"色段"调 DrawPolylineThick
        private static readonly List<Vector2> s_RunBuffer = new List<Vector2>(64);

        /// <summary>
        /// 沿基线把折线"按色"拆段绘制：曲线在基线之上的部分用 <paramref name="aboveColor"/>，
        /// 之下的部分用 <paramref name="belowColor"/>，跨基线段会自动求交点拆开，
        /// 形成"上半红 / 下半蓝"这种线条本身按 Y 值变色的效果（适用 S4 胜率预测）。
        ///
        /// 注意：每个"色段"独立做 miter join，跨基线交点处两段各自有平角端帽，
        /// 在曲线密集采样下视觉上几乎无缝；折线场景如果交点角度变化剧烈会有 1~2px 的视觉断口，
        /// 通常被 ChartArea 的 Mask 或线宽掩盖。
        /// </summary>
        protected static void DrawPolylineThickSplit(
            VertexHelper vh, IList<Vector2> localPts, float baselineLocalY,
            Color aboveColor, Color belowColor, float width, float miterLimit = 4f)
        {
            int n = localPts.Count;
            if (n < 2 || width <= 0f) return;

            s_RunBuffer.Clear();
            bool runIsAbove = localPts[0].y >= baselineLocalY;
            s_RunBuffer.Add(localPts[0]);

            for (int i = 1; i < n; i++)
            {
                Vector2 prev = localPts[i - 1];
                Vector2 curr = localPts[i];
                bool currIsAbove = curr.y >= baselineLocalY;

                if (currIsAbove == runIsAbove)
                {
                    s_RunBuffer.Add(curr);
                    continue;
                }

                // 跨基线，求交点
                float t = Mathf.Approximately(curr.y, prev.y)
                    ? 0.5f
                    : (baselineLocalY - prev.y) / (curr.y - prev.y);
                Vector2 cross = new Vector2(prev.x + (curr.x - prev.x) * t, baselineLocalY);

                s_RunBuffer.Add(cross);
                DrawPolylineThick(vh, s_RunBuffer,
                    runIsAbove ? aboveColor : belowColor, width, miterLimit);

                s_RunBuffer.Clear();
                s_RunBuffer.Add(cross);
                s_RunBuffer.Add(curr);
                runIsAbove = currIsAbove;
            }

            if (s_RunBuffer.Count >= 2)
            {
                DrawPolylineThick(vh, s_RunBuffer,
                    runIsAbove ? aboveColor : belowColor, width, miterLimit);
            }
        }

        // -------------------------- 通用绘制工具：跨基线分区填充 --------------------------

        /// <summary>
        /// 填充方向枚举，用于限制距离填充。
        /// </summary>
        protected enum FillDirection
        {
            ToTop,
            ToBottom,
        }

        /// <summary>
        /// 沿折线生成"曲线 → 远端水平线"之间的填充（用于平滑曲线的单侧渐变）。
        /// 远端 Y 由 farLocalY 指定（通常是 ChartArea 的上/下边）。
        /// </summary>
        protected static void DrawAreaToFarEdge(
            VertexHelper vh, IList<Vector2> localPts, float farLocalY,
            Color nearCurveColor, Color farColor)
        {
            int n = localPts.Count;
            if (n < 2) return;

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 a = localPts[i];
                Vector2 b = localPts[i + 1];

                AddQuad(vh,
                    a, nearCurveColor,
                    b, nearCurveColor,
                    new Vector2(b.x, farLocalY), farColor,
                    new Vector2(a.x, farLocalY), farColor);
            }
        }

        /// <summary>
        /// 沿折线生成"曲线向外延伸固定距离"之间的填充。
        /// 与 DrawAreaToFarEdge 的区别是：远端不是边缘，而是一个相对曲线固定距离的位置。
        /// </summary>
        /// <param name="vh">VertexHelper</param>
        /// <param name="localPts">曲线点序列（本地坐标）</param>
        /// <param name="direction">填充方向：ToTop 向上延伸，ToBottom 向下延伸</param>
        /// <param name="nearCurveColor">曲线侧颜色</param>
        /// <param name="farColor">远端颜色</param>
        /// <param name="distanceLimit">从曲线向外延伸的距离（像素）</param>
        protected static void DrawAreaToLimit(
            VertexHelper vh, IList<Vector2> localPts, FillDirection direction,
            Color nearCurveColor, Color farColor, float distanceLimit)
        {
            int n = localPts.Count;
            if (n < 2 || distanceLimit <= 0f) return;

            // 确定远端 Y：向上填充时，远端 = curveY + distanceLimit
            //                       向下填充时，远端 = curveY - distanceLimit
            // 但需要处理跨段情况，所以对每段单独计算
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 a = localPts[i];
                Vector2 b = localPts[i + 1];

                float farY_A, farY_B;

                if (direction == FillDirection.ToTop)
                {
                    farY_A = a.y + distanceLimit;
                    farY_B = b.y + distanceLimit;
                }
                else // ToBottom
                {
                    farY_A = a.y - distanceLimit;
                    farY_B = b.y - distanceLimit;
                }

                // 如果远端超出边界，做一次裁切（避免绘制到图表外）
                // 但保留原始计算值，因为距离限制模式本身就不依赖边界

                AddQuad(vh,
                    a, nearCurveColor,
                    b, nearCurveColor,
                    new Vector2(b.x, farY_B), farColor,
                    new Vector2(a.x, farY_A), farColor);
            }
        }

        /// <summary>
        /// 沿折线生成"曲线 ↔ 基线"之间的填充区域，遇到曲线跨过基线的段会自动求交点拆分，
        /// 上方区域使用 above 色对，下方区域使用 below 色对。
        /// 顶点颜色完全靠 vh 的 color 通道实现渐变，无需自定义 Shader。
        /// </summary>
        /// <param name="aboveCurveColor">曲线在基线之上时，靠近曲线一侧的颜色</param>
        /// <param name="aboveBaseColor">曲线在基线之上时，靠近基线一侧的颜色</param>
        /// <param name="belowCurveColor">曲线在基线之下时，靠近曲线一侧的颜色</param>
        /// <param name="belowBaseColor">曲线在基线之下时，靠近基线一侧的颜色</param>
        protected static void DrawAreaSplit(
            VertexHelper vh, IList<Vector2> localPts, float baselineLocalY,
            Color aboveCurveColor, Color aboveBaseColor,
            Color belowCurveColor, Color belowBaseColor)
        {
            int n = localPts.Count;
            if (n < 2) return;

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 a = localPts[i];
                Vector2 b = localPts[i + 1];

                bool aAbove = a.y >= baselineLocalY;
                bool bAbove = b.y >= baselineLocalY;

                if (aAbove == bAbove)
                {
                    Color cCurve = aAbove ? aboveCurveColor : belowCurveColor;
                    Color cBase = aAbove ? aboveBaseColor : belowBaseColor;

                    AddQuad(vh,
                        a, cCurve,
                        b, cCurve,
                        new Vector2(b.x, baselineLocalY), cBase,
                        new Vector2(a.x, baselineLocalY), cBase);
                }
                else
                {
                    float t = Mathf.Approximately(b.y, a.y)
                        ? 0.5f
                        : (baselineLocalY - a.y) / (b.y - a.y);
                    Vector2 c = new Vector2(a.x + (b.x - a.x) * t, baselineLocalY);

                    Color aCurve = aAbove ? aboveCurveColor : belowCurveColor;
                    Color aBase = aAbove ? aboveBaseColor : belowBaseColor;
                    Color bCurve = bAbove ? aboveCurveColor : belowCurveColor;
                    Color bBase = bAbove ? aboveBaseColor : belowBaseColor;

                    AddTri(vh,
                        a, aCurve,
                        c, aBase,
                        new Vector2(a.x, baselineLocalY), aBase);

                    AddTri(vh,
                        c, bBase,
                        b, bCurve,
                        new Vector2(b.x, baselineLocalY), bBase);
                }
            }
        }

        /// <summary>
        /// 沿折线生成"曲线 ↔ 基线"之间的填充区域，但限制填充厚度不超过 distanceLimit。
        /// 填充始终朝向基线方向，当曲线进入 distanceLimit 范围内时填充变薄，触及基线。
        /// </summary>
        protected static void DrawAreaSplitWithLimit(
            VertexHelper vh, IList<Vector2> localPts, float baselineLocalY,
            Color aboveCurveColor, Color aboveBaseColor,
            Color belowCurveColor, Color belowBaseColor,
            float distanceLimit)
        {
            int n = localPts.Count;
            if (n < 2 || distanceLimit <= 0f) return;

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 a = localPts[i];
                Vector2 b = localPts[i + 1];
                bool aAbove = a.y >= baselineLocalY;
                bool bAbove = b.y >= baselineLocalY;

                if (aAbove == bAbove)
                {
                    // 同侧：填充朝向基线，厚度不超过 distanceLimit
                    float farY_A = aAbove
                        ? Mathf.Max(baselineLocalY, a.y - distanceLimit)
                        : Mathf.Min(baselineLocalY, a.y + distanceLimit);
                    float farY_B = aAbove
                        ? Mathf.Max(baselineLocalY, b.y - distanceLimit)
                        : Mathf.Min(baselineLocalY, b.y + distanceLimit);

                    Color cCurve = aAbove ? aboveCurveColor : belowCurveColor;
                    //Color cBase = aAbove ? aboveBaseColor  : belowBaseColor;

                    Color cBaseA = aAbove
                        ? Color.Lerp(aboveCurveColor, aboveBaseColor, Mathf.Clamp01((a.y - baselineLocalY) / distanceLimit))
                        : Color.Lerp(belowCurveColor, belowBaseColor, Mathf.Clamp01((baselineLocalY - a.y) / distanceLimit));
                    Color cBaseB = aAbove
                        ? Color.Lerp(aboveCurveColor, aboveBaseColor, Mathf.Clamp01((b.y - baselineLocalY) / distanceLimit))
                        : Color.Lerp(belowCurveColor, belowBaseColor, Mathf.Clamp01((baselineLocalY - b.y) / distanceLimit));

                    AddQuad(vh,
                        a, cCurve,
                        b, cCurve,
                        new Vector2(b.x, farY_B), cBaseB,
                        new Vector2(a.x, farY_A), cBaseA);
                }
                else
                {
                    // 跨基线：求交点，分两段各自朝基线填充
                    float t = Mathf.Approximately(b.y, a.y)
                        ? 0.5f
                        : (baselineLocalY - a.y) / (b.y - a.y);
                    Vector2 c = new Vector2(a.x + (b.x - a.x) * t, baselineLocalY);

                    // A→C 段（aAbove 侧，填充朝基线方向）
                    float farY_A = aAbove
                        ? Mathf.Max(baselineLocalY, a.y - distanceLimit)
                        : Mathf.Min(baselineLocalY, a.y + distanceLimit);
                    Color cCurveA = aAbove ? aboveCurveColor : belowCurveColor;
                    //Color cBaseA  = aAbove ? aboveBaseColor  : belowBaseColor;
                    Color cBaseA = aAbove
                        ? Color.Lerp(aboveCurveColor, aboveBaseColor, Mathf.Clamp01((a.y - baselineLocalY) / distanceLimit))
                        : Color.Lerp(belowCurveColor, belowBaseColor, Mathf.Clamp01((baselineLocalY - a.y) / distanceLimit));
                    // C 在基线上，该端填充厚度为 0，用三角形即可
                    AddTri(vh,
                        a, cCurveA,
                        c, cCurveA,
                        new Vector2(a.x, farY_A), cBaseA);

                    // C→B 段（bAbove 侧，填充朝基线方向）
                    float farY_B = bAbove
                        ? Mathf.Max(baselineLocalY, b.y - distanceLimit)
                        : Mathf.Min(baselineLocalY, b.y + distanceLimit);
                    Color cCurveB = bAbove ? aboveCurveColor : belowCurveColor;
                    //Color cBaseB  = bAbove ? aboveBaseColor  : belowBaseColor;
                    Color cBaseB = bAbove
                        ? Color.Lerp(aboveCurveColor, aboveBaseColor, Mathf.Clamp01((b.y - baselineLocalY) / distanceLimit))
                        : Color.Lerp(belowCurveColor, belowBaseColor, Mathf.Clamp01((baselineLocalY - b.y) / distanceLimit));
                    AddTri(vh,
                        c, cCurveB,
                        b, cCurveB,
                        new Vector2(b.x, farY_B), cBaseB);
                }
            }
        }

        // -------------------------- VertexHelper 工具 --------------------------

        protected static void AddQuad(
            VertexHelper vh,
            Vector2 p0, Color c0,
            Vector2 p1, Color c1,
            Vector2 p2, Color c2,
            Vector2 p3, Color c3)
        {
            int idx = vh.currentVertCount;
            vh.AddVert((Vector3)p0, c0, Vector2.zero);
            vh.AddVert((Vector3)p1, c1, Vector2.zero);
            vh.AddVert((Vector3)p2, c2, Vector2.zero);
            vh.AddVert((Vector3)p3, c3, Vector2.zero);
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }

        protected static void AddTri(
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

        private static Vector2 SafeNormalize(Vector2 v)
        {
            float m = v.magnitude;
            return m < 1e-6f ? Vector2.right : v / m;
        }

        // -------------------------- 编辑器支持 --------------------------

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_ValueRangeX.x > m_ValueRangeX.y)
            {
                (m_ValueRangeX.x, m_ValueRangeX.y) = (m_ValueRangeX.y, m_ValueRangeX.x);
            }
            if (m_ValueRangeY.x > m_ValueRangeY.y)
            {
                (m_ValueRangeY.x, m_ValueRangeY.y) = (m_ValueRangeY.y, m_ValueRangeY.x);
            }

            SetVerticesDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetVerticesDirty();
        }
#endif

        // -------------------------- 内嵌类型 --------------------------

        /// <summary>
        /// 数据点超出值域时的处理策略。
        /// </summary>
        public enum ClampMode
        {
            /// <summary>截断到值域边界（推荐，与美术稿对齐）。</summary>
            ClampToEdge = 0,
            /// <summary>不截断，允许画到 ChartArea 矩形之外（依赖外层 RectMask2D 裁剪）。</summary>
            None = 1,
        }

        /// <summary>
        /// 一条曲线序列（颜色、宽度、数据点）。
        /// 同一图表上画多条线时，每条线对应一个 ChartSeries。
        /// </summary>
        [Serializable]
        public class ChartSeries
        {
            [Tooltip("仅 Inspector 区分用。")]
            public string name = "Series";

            [Tooltip("线条颜色。")]
            public Color color = Color.white;

            [Min(0.5f)]
            [Tooltip("线条宽度（像素）。")]
            public float width = 4f;

            [Tooltip("数据点（数据坐标系，可在 Inspector 直接填入用于编辑器预览）。")]
            public List<Vector2> points = new List<Vector2>();
        }
    }
}
