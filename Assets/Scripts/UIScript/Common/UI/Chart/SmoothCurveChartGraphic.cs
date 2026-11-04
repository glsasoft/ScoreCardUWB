using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 平滑曲线 + 渐变/纯色填充：
    /// 数据点之间用 Catmull-Rom 样条或单调三次样条插值生成密集采样点，再画粗曲线本体；
    /// 同时把曲线与"远端"围成的区域做填充。
    ///
    /// 填充模式 <see cref="FillMode"/>：
    /// - None              : 不填充，只画线
    /// - GradientToBottom  : 曲线 → ChartArea 底边（适合 ④ AI 胜率向下渐变）
    /// - GradientToTop     : 曲线 → ChartArea 顶边（适合"剩余血量"等向上填充的场景）
    /// - SplitByBaseline   : 以 <see cref="m_BaselineValue"/> 为界，上下分别用两套颜色填充（与 SplitArea 一致，但曲线是平滑的）
    ///
    /// 填充范围类型 <see cref="FillRangeType"/>：
    /// - FullToEdge        : 从曲线填满到边缘/基准线（默认，经典效果）
    /// - LimitedToDistance : 从曲线向外延伸固定距离，而非填满到边缘
    ///
    /// 曲线插值方式 <see cref="CurveMethod"/>：
    /// - CatmullRom    : 经典 Catmull-Rom 样条，流畅但可能在极值点产生轻微过冲。
    /// - MonotoneCubic : 单调三次样条（Fritsch-Carlson），保证曲线不超出原始数据点的 Y 轴值域，适合金融/胜率等对越界敏感的场景。
    ///
    /// 任意一组"近曲线色 / 远端色"设为相同色 → 纯色填充；不同 alpha → 渐变。
    /// </summary>
    [AddComponentMenu("UI/Chart/Smooth Curve Chart Graphic")]
    public class SmoothCurveChartGraphic : ChartGraphicBase
    {
        public enum FillMode
        {
            None = 0,
            GradientToBottom = 1,
            GradientToTop = 2,
            SplitByBaseline = 3,
        }

        /// <summary>
        /// 填充范围类型。
        /// </summary>
        public enum FillRangeType
        {
            /// <summary>填满到边缘（从曲线到 ChartArea 上/下边或基线）。</summary>
            FullToEdge = 0,
            /// <summary>限制距离填充（从曲线向外延伸指定距离，而非填满到边缘）。</summary>
            LimitedToDistance = 1,
        }

        /// <summary>曲线插值算法选择。</summary>
        public enum CurveMethod
        {
            /// <summary>Catmull-Rom 样条：流畅，但极值点附近可能轻微过冲。</summary>
            CatmullRom = 0,
            /// <summary>单调三次样条（Fritsch-Carlson）：保证曲线严格不超出原始点的 Y 值域。</summary>
            MonotoneCubic = 1,
        }

        [Header("曲线（颜色 / 宽度 / 数据点）")]
        [SerializeField]
        private ChartSeries m_Curve = new ChartSeries
        {
            name  = "Curve",
            color = new Color(1f, 0.55f, 0.20f, 1f),
            width = 5f,
            // 默认示例（适配 ValueRangeY=(0,1) 的 AI 胜率视觉效果）
            points = new List<Vector2>
            {
                new Vector2(0f,   0.00f),
                new Vector2(1f,   0.55f),
                new Vector2(2f,   0.40f),
                new Vector2(3.5f, 0.05f),
                new Vector2(5f,   0.70f),
                new Vector2(6f,   0.60f),
                new Vector2(8f,   0.95f),
            },
        };

        [Header("曲线插值方法")]
        [SerializeField]
        [Tooltip("CatmullRom：平滑、流畅，极值点可能轻微过冲。\nMonotoneCubic：单调三次样条，严格不超出原始数据点 Y 值域（推荐用于胜率/金融等敏感场景）。")]
        private CurveMethod m_CurveMethod = CurveMethod.CatmullRom;

        [Header("Catmull-Rom 平滑参数（仅 CatmullRom 模式生效）")]
        [SerializeField]
        [Range(2, 32)]
        [Tooltip("每两个相邻原始数据点之间插值的细分段数，越大越平滑也越费性能。")]
        private int m_SegmentsPerSpan = 12;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("张力系数。0.5 = 标准 Catmull-Rom（推荐）；越大越接近折线。")]
        private float m_Tension = 0.5f;

        [Header("单调三次样条参数（仅 MonotoneCubic 模式生效）")]
        [SerializeField]
        [Range(2, 32)]
        [Tooltip("每两个相邻原始数据点之间插值的细分段数，越大越平滑。")]
        private int m_MonotoneSegmentsPerSpan = 12;

        [Header("填充设置")]
        [SerializeField]
        private FillMode m_FillMode = FillMode.GradientToBottom;

        [Header("填充范围类型")]
        [SerializeField]
        [Tooltip("FullToEdge：从曲线填满到边缘/基准线（现有行为）。\nLimitedToDistance：从曲线向外延伸指定距离，而非填满到边缘。")]
        private FillRangeType m_FillRangeType = FillRangeType.FullToEdge;

        [Header("限制距离填充参数（仅 LimitedToDistance 模式生效）")]
        [SerializeField]
        [Min(1f)]
        [Tooltip("从曲线向外延伸的填充距离（像素）。在此距离之外不显示填充。")]
        private float m_FillDistanceLimit = 50f;

        [Header("基线设置")]
        [Tooltip("仅 SplitByBaseline 模式生效：分区基线（数据坐标系）。")]
        [SerializeField]
        private float m_BaselineValue = 0f;

        [Header("渐变填充颜色（GradientToBottom / GradientToTop 模式）")]
        [SerializeField]
        [Tooltip("靠近曲线一侧的颜色（通常 alpha 较高）。")]
        private Color m_FillNearCurveColor = new Color(1f, 0.55f, 0.20f, 0.70f);

        [SerializeField]
        [Tooltip("远端的颜色（通常 alpha 接近 0 → 形成渐变；与近端色相同 alpha → 纯色）。")]
        private Color m_FillFarColor = new Color(1f, 0.55f, 0.20f, 0.00f);

        [Header("分区填充颜色（仅 SplitByBaseline 模式生效）")]
        [SerializeField]
        private Color m_AboveCurveColor = new Color(0.92f, 0.30f, 0.30f, 0.85f);
        [SerializeField]
        private Color m_AboveBaseColor = new Color(0.92f, 0.30f, 0.30f, 0.20f);
        [SerializeField]
        private Color m_BelowCurveColor = new Color(0.40f, 0.55f, 1.00f, 0.85f);
        [SerializeField]
        private Color m_BelowBaseColor = new Color(0.40f, 0.55f, 1.00f, 0.20f);

        [Header("是否启用各模块")]
        [SerializeField]
        private bool m_StrokeEnabled = true;

        [Header("曲线分色（适配 S4 胜率预测：基线之上红、之下蓝）")]
        [SerializeField]
        [Tooltip("打开后忽略 Curve.color，改用下面 AboveLineColor / BelowLineColor 沿 BaselineValue 拆分。")]
        private bool m_SplitLineColorByBaseline = false;

        [SerializeField]
        [Tooltip("曲线在基线之上时的颜色（如：红队领先 → 红色）。")]
        private Color m_AboveLineColor = new Color(0.92f, 0.30f, 0.30f, 1f);

        [SerializeField]
        [Tooltip("曲线在基线之下时的颜色（如：蓝队领先 → 蓝色）。")]
        private Color m_BelowLineColor = new Color(0.40f, 0.55f, 1.00f, 1f);

        // 复用 buffer
        private static readonly List<Vector2> s_Local = new List<Vector2>(64);
        private static readonly List<Vector2> s_Smooth = new List<Vector2>(512);

        // 曲线求值器（数据坐标系）
        private SplineEvaluator m_Evaluator;

        // -------------------------- 公共属性 --------------------------

        public ChartSeries Curve => m_Curve;
        public FillMode Mode { get => m_FillMode; set { m_FillMode = value; SetVerticesDirty(); } }
        public FillRangeType RangeType { get => m_FillRangeType; set { m_FillRangeType = value; SetVerticesDirty(); } }
        public float FillDistanceLimit { get => m_FillDistanceLimit; set { m_FillDistanceLimit = Mathf.Max(1f, value); SetVerticesDirty(); } }
        public float BaselineValue { get => m_BaselineValue; set { m_BaselineValue = value; SetVerticesDirty(); } }

        /// <summary>
        /// 根据数据坐标系的 X 值，求出对应的曲线 Y 值（数据坐标系）。
        /// 两种插值模式均支持。X 必须在原始数据点的 [xMin, xMax] 范围内；
        /// 超出范围时自动 Clamp 到端点 Y 值，并返回 false。
        /// </summary>
        /// <param name="dataX">数据坐标系的 X</param>
        /// <param name="dataY">输出：对应的曲线 Y 值（数据坐标系）</param>
        /// <returns>X 在有效范围内时返回 true</returns>
        public bool TryEvaluateY(float dataX, out float dataY)
        {
            EnsureEvaluator();
            if (m_Evaluator == null) { dataY = 0f; return false; }
            return m_Evaluator.TryEvaluate(dataX, out dataY);
        }

        /// <summary>
        /// 根据数据坐标系的 X 值，求出对应的曲线 Y 值（数据坐标系）。
        /// X 超出范围时返回最近端点 Y 值。
        /// </summary>
        public float EvaluateY(float dataX)
        {
            TryEvaluateY(dataX, out float y);
            return y;
        }

        public void SetData(IList<Vector2> points)
        {
            m_Curve.points.Clear();
            if (points != null)
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    m_Curve.points.Add(points[i]);
                }
            }
            m_Evaluator = null; // 数据变化，下次求值时重建
            SetVerticesDirty();
        }

        // 保证求值器与当前数据/方法同步
        private void EnsureEvaluator()
        {
            if (m_Evaluator != null) return;
            if (m_Curve == null || m_Curve.points == null || m_Curve.points.Count < 2) return;

            switch (m_CurveMethod)
            {
                case CurveMethod.MonotoneCubic:
                    m_Evaluator = SplineEvaluator.BuildMonotone(m_Curve.points);
                    break;
                default:
                    m_Evaluator = SplineEvaluator.BuildCatmullRom(
                        m_Curve.points, m_SegmentsPerSpan, m_Tension);
                    break;
            }
        }

        // -------------------------- 编辑器默认值 --------------------------

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            m_ValueRangeX = new Vector2(0f, 8f);
            m_ValueRangeY = new Vector2(0f, 1f);
            m_BaselineValue = 0f;
        }
#endif

        // -------------------------- 绘制 --------------------------

        protected override void DrawChart(VertexHelper vh)
        {
            if (m_Curve == null || m_Curve.points == null || m_Curve.points.Count < 2) return;

            // 重绘时旧求值器可能已过期（Inspector 修改了参数），清空让下次 EnsureEvaluator 重建
            m_Evaluator = null;

            MapPointsToLocal(m_Curve.points, s_Local);

            // 根据插值方法选择平滑算法
            switch (m_CurveMethod)
            {
                case CurveMethod.MonotoneCubic:
                    BuildSmoothPolylineMonotone(s_Local, s_Smooth, m_MonotoneSegmentsPerSpan);
                    break;
                default: // CatmullRom
                    BuildSmoothPolyline(s_Local, s_Smooth, m_SegmentsPerSpan, m_Tension);
                    break;
            }

            if (s_Smooth.Count < 2) return;

            // 1) 填充
            if (m_FillMode != FillMode.None)
            {
                Rect r = GetPixelAdjustedRect();
                switch (m_FillMode)
                {
                    case FillMode.GradientToBottom:
                        if (m_FillRangeType == FillRangeType.LimitedToDistance)
                        {
                            DrawAreaToLimit(vh, s_Smooth, FillDirection.ToBottom,
                                m_FillNearCurveColor, m_FillFarColor, m_FillDistanceLimit);
                        }
                        else
                        {
                            DrawAreaToFarEdge(vh, s_Smooth, r.yMin,
                                m_FillNearCurveColor, m_FillFarColor);
                        }
                        break;

                    case FillMode.GradientToTop:
                        if (m_FillRangeType == FillRangeType.LimitedToDistance)
                        {
                            DrawAreaToLimit(vh, s_Smooth, FillDirection.ToTop,
                                m_FillNearCurveColor, m_FillFarColor, m_FillDistanceLimit);
                        }
                        else
                        {
                            DrawAreaToFarEdge(vh, s_Smooth, r.yMax,
                                m_FillNearCurveColor, m_FillFarColor);
                        }
                        break;

                    case FillMode.SplitByBaseline:
                    {
                        float baselineLocalY = ValueToLocal(m_ValueRangeX.x, m_BaselineValue).y;
                        if (m_FillRangeType == FillRangeType.LimitedToDistance)
                        {
                            DrawAreaSplitWithLimit(vh, s_Smooth, baselineLocalY,
                                m_AboveCurveColor, m_AboveBaseColor,
                                m_BelowCurveColor, m_BelowBaseColor,
                                m_FillDistanceLimit);
                        }
                        else
                        {
                            DrawAreaSplit(vh, s_Smooth, baselineLocalY,
                                m_AboveCurveColor, m_AboveBaseColor,
                                m_BelowCurveColor, m_BelowBaseColor);
                        }
                        break;
                    }
                }
            }

            // 2) 曲线本体
            if (m_StrokeEnabled)
            {
                if (m_SplitLineColorByBaseline)
                {
                    float baselineLocalY = ValueToLocal(m_ValueRangeX.x, m_BaselineValue).y;
                    DrawPolylineThickSplit(vh, s_Smooth, baselineLocalY,
                        m_AboveLineColor, m_BelowLineColor, m_Curve.width);
                }
                else
                {
                    DrawPolylineThick(vh, s_Smooth, m_Curve.color, m_Curve.width);
                }
            }
        }

        // -------------------------- Catmull-Rom 实现 --------------------------

        /// <summary>
        /// 把原始折线插值成密集平滑折线。端点用复制端点策略避免越界。
        /// </summary>
        private static void BuildSmoothPolyline(
            IList<Vector2> input, List<Vector2> output, int segments, float tension)
        {
            output.Clear();
            int n = input.Count;
            if (n == 0) return;
            if (n == 1) { output.Add(input[0]); return; }

            // 张力转 alpha：0 张力 → 0.5（标准 Catmull-Rom）；1 张力 → 0（直线）
            float alpha = 0.5f * (1f - tension);

            output.Add(input[0]);

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p0 = (i == 0) ? input[i] : input[i - 1];
                Vector2 p1 = input[i];
                Vector2 p2 = input[i + 1];
                Vector2 p3 = (i + 2 < n) ? input[i + 2] : input[i + 1];

                for (int s = 1; s <= segments; s++)
                {
                    float t = (float)s / segments;
                    output.Add(CatmullRom(p0, p1, p2, p3, t, alpha));
                }
            }
        }

        private static Vector2 CatmullRom(
            Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t, float alpha)
        {
            // 标准 Catmull-Rom（α=0.5）公式，alpha 控制切线幅度
            float t2 = t * t;
            float t3 = t2 * t;

            Vector2 m1 = (p2 - p0) * alpha;
            Vector2 m2 = (p3 - p1) * alpha;

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            return h00 * p1 + h10 * m1 + h01 * p2 + h11 * m2;
        }

        // -------------------------- 单调三次样条实现（Fritsch-Carlson）--------------------------

        /// <summary>
        /// 单调三次 Hermite 样条（Fritsch-Carlson 修正）。
        /// 保证插值结果严格不超出原始数据点的 Y 值域，适合对"不过冲"有要求的场景。
        /// 输入点按 X 升序排列（<see cref="MapPointsToLocal"/> 转换后通常满足，如不满足会自动排序）。
        /// </summary>
        private static void BuildSmoothPolylineMonotone(
            IList<Vector2> input, List<Vector2> output, int segments)
        {
            output.Clear();
            int n = input.Count;
            if (n == 0) return;
            if (n == 1) { output.Add(input[0]); return; }

            // 1. 把输入复制到临时数组，并按 X 排序
            var pts = new Vector2[n];
            for (int i = 0; i < n; i++) pts[i] = input[i];
            System.Array.Sort(pts, (a, b) => a.x.CompareTo(b.x));

            // 2. 计算相邻段斜率 delta[i] = (y[i+1]-y[i])/(x[i+1]-x[i])
            var delta = new float[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                float dx = pts[i + 1].x - pts[i].x;
                delta[i] = Mathf.Approximately(dx, 0f) ? 0f : (pts[i + 1].y - pts[i].y) / dx;
            }

            // 3. 初始切线 m（Hermite 导数）
            var m = new float[n];
            m[0]     = delta[0];
            m[n - 1] = delta[n - 2];
            for (int i = 1; i < n - 1; i++)
                m[i] = (delta[i - 1] + delta[i]) * 0.5f;

            // 4. Fritsch-Carlson 修正：保证区间单调性
            for (int i = 0; i < n - 1; i++)
            {
                if (Mathf.Abs(delta[i]) < 1e-6f)
                {
                    // 平台段，两端切线置 0
                    m[i]     = 0f;
                    m[i + 1] = 0f;
                    continue;
                }
                float alpha = m[i]     / delta[i];
                float beta  = m[i + 1] / delta[i];
                float tau   = alpha * alpha + beta * beta;
                if (tau > 9f)
                {
                    // 缩放切线，保证不过冲
                    float scale = 3f / Mathf.Sqrt(tau);
                    m[i]     = scale * alpha * delta[i];
                    m[i + 1] = scale * beta  * delta[i];
                }
            }

            // 5. 生成采样点（分段 Hermite 插值）
            output.Add(pts[0]);
            for (int i = 0; i < n - 1; i++)
            {
                float x0 = pts[i].x, y0 = pts[i].y;
                float x1 = pts[i + 1].x, y1 = pts[i + 1].y;
                float h   = x1 - x0;

                for (int s = 1; s <= segments; s++)
                {
                    float t  = (float)s / segments;
                    float t2 = t * t;
                    float t3 = t2 * t;

                    // 标准三次 Hermite 基函数
                    float h00 =  2f * t3 - 3f * t2 + 1f;
                    float h10 =       t3 - 2f * t2 + t;
                    float h01 = -2f * t3 + 3f * t2;
                    float h11 =       t3 -      t2;

                    float x = Mathf.Lerp(x0, x1, t);
                    float y = h00 * y0 + h10 * h * m[i]
                            + h01 * y1 + h11 * h * m[i + 1];

                    output.Add(new Vector2(x, y));
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_SegmentsPerSpan         = Mathf.Clamp(m_SegmentsPerSpan, 2, 32);
            m_MonotoneSegmentsPerSpan = Mathf.Clamp(m_MonotoneSegmentsPerSpan, 2, 32);
            m_FillDistanceLimit       = Mathf.Max(1f, m_FillDistanceLimit);
        }
#endif

        // -------------------------- 曲线求值器（数据坐标系 x → y）--------------------------

        /// <summary>
        /// 封装"按数据坐标 X 求曲线 Y"的逻辑，与渲染路径共享相同算法。
        /// 外部通过 <see cref="SmoothCurveChartGraphic.TryEvaluateY"/> 使用。
        /// </summary>
        private sealed class SplineEvaluator
        {
            // 排好序的原始数据点（数据坐标系）
            private readonly float[] _xs;
            private readonly float[] _ys;

            // MonotoneCubic 专用：每段 Hermite 切线 m[i]
            private readonly float[] _ms;

            // CatmullRom 专用：稠密采样点（数据坐标系）
            private readonly Vector2[] _samples;

            // ---- 构建：单调三次样条 ----
            public static SplineEvaluator BuildMonotone(IList<Vector2> rawPts)
            {
                int n = rawPts.Count;
                var pts = new Vector2[n];
                for (int i = 0; i < n; i++) pts[i] = rawPts[i];
                System.Array.Sort(pts, (a, b) => a.x.CompareTo(b.x));

                float[] xs = new float[n];
                float[] ys = new float[n];
                for (int i = 0; i < n; i++) { xs[i] = pts[i].x; ys[i] = pts[i].y; }

                // 斜率 delta
                var delta = new float[n - 1];
                for (int i = 0; i < n - 1; i++)
                {
                    float dx = xs[i + 1] - xs[i];
                    delta[i] = Mathf.Approximately(dx, 0f) ? 0f : (ys[i + 1] - ys[i]) / dx;
                }

                // 切线 m
                var ms = new float[n];
                ms[0]     = delta[0];
                ms[n - 1] = delta[n - 2];
                for (int i = 1; i < n - 1; i++)
                    ms[i] = (delta[i - 1] + delta[i]) * 0.5f;

                // Fritsch-Carlson 修正
                for (int i = 0; i < n - 1; i++)
                {
                    if (Mathf.Abs(delta[i]) < 1e-6f) { ms[i] = 0f; ms[i + 1] = 0f; continue; }
                    float a = ms[i] / delta[i], b = ms[i + 1] / delta[i];
                    float tau = a * a + b * b;
                    if (tau > 9f)
                    {
                        float sc = 3f / Mathf.Sqrt(tau);
                        ms[i]     = sc * a * delta[i];
                        ms[i + 1] = sc * b * delta[i];
                    }
                }

                return new SplineEvaluator(xs, ys, ms, null);
            }

            // ---- 构建：Catmull-Rom（以稠密采样点存储，二分查询）----
            public static SplineEvaluator BuildCatmullRom(
                IList<Vector2> rawPts, int segments, float tension)
            {
                int n = rawPts.Count;
                var pts = new Vector2[n];
                for (int i = 0; i < n; i++) pts[i] = rawPts[i];
                System.Array.Sort(pts, (a, b) => a.x.CompareTo(b.x));

                float alpha = 0.5f * (1f - tension);
                var list = new List<Vector2>((n - 1) * segments + 1);
                list.Add(pts[0]);
                for (int i = 0; i < n - 1; i++)
                {
                    Vector2 p0 = (i == 0) ? pts[i] : pts[i - 1];
                    Vector2 p1 = pts[i];
                    Vector2 p2 = pts[i + 1];
                    Vector2 p3 = (i + 2 < n) ? pts[i + 2] : pts[i + 1];
                    for (int s = 1; s <= segments; s++)
                    {
                        float t  = (float)s / segments;
                        float t2 = t * t, t3 = t2 * t;
                        Vector2 m1 = (p2 - p0) * alpha;
                        Vector2 m2 = (p3 - p1) * alpha;
                        float h00 =  2f*t3 - 3f*t2 + 1f;
                        float h10 =     t3 - 2f*t2 + t;
                        float h01 = -2f*t3 + 3f*t2;
                        float h11 =     t3 -    t2;
                        list.Add(h00*p1 + h10*m1 + h01*p2 + h11*m2);
                    }
                }

                // 取原始点 xs/ys 用于范围检测
                float[] xs = new float[n];
                float[] ys = new float[n];
                for (int i = 0; i < n; i++) { xs[i] = pts[i].x; ys[i] = pts[i].y; }

                return new SplineEvaluator(xs, ys, null, list.ToArray());
            }

            private SplineEvaluator(float[] xs, float[] ys, float[] ms, Vector2[] samples)
            {
                _xs = xs; _ys = ys; _ms = ms; _samples = samples;
            }

            /// <summary>
            /// 按数据坐标 X 求 Y。超出范围时 Clamp 到端点 Y 并返回 false。
            /// </summary>
            public bool TryEvaluate(float x, out float y)
            {
                float xMin = _xs[0], xMax = _xs[_xs.Length - 1];
                bool inRange = x >= xMin && x <= xMax;

                if (x <= xMin) { y = _ys[0]; return inRange; }
                if (x >= xMax) { y = _ys[_xs.Length - 1]; return inRange; }

                if (_ms != null)
                    y = EvalMonotone(x);
                else
                    y = EvalCatmullRomSamples(x);

                return true;
            }

            // 单调三次样条精确解析求值
            private float EvalMonotone(float x)
            {
                // 找区间（二分）
                int lo = 0, hi = _xs.Length - 2;
                while (lo < hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (_xs[mid + 1] < x) lo = mid + 1; else hi = mid;
                }
                int i = lo;
                float h  = _xs[i + 1] - _xs[i];
                float t  = Mathf.Approximately(h, 0f) ? 0f : (x - _xs[i]) / h;
                float t2 = t * t, t3 = t2 * t;
                float h00 =  2f*t3 - 3f*t2 + 1f;
                float h10 =     t3 - 2f*t2 + t;
                float h01 = -2f*t3 + 3f*t2;
                float h11 =     t3 -    t2;
                return h00 * _ys[i] + h10 * h * _ms[i]
                     + h01 * _ys[i+1] + h11 * h * _ms[i+1];
            }

            // Catmull-Rom：在稠密采样点中二分找相邻段，线性插值
            private float EvalCatmullRomSamples(float x)
            {
                var s = _samples;
                int lo = 0, hi = s.Length - 2;
                while (lo < hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (s[mid + 1].x < x) lo = mid + 1; else hi = mid;
                }
                float dx = s[lo + 1].x - s[lo].x;
                float t  = Mathf.Approximately(dx, 0f) ? 0f : (x - s[lo].x) / dx;
                return Mathf.Lerp(s[lo].y, s[lo + 1].y, t);
            }
        }
    }
}
