using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 双色分区填充折线图：以 <see cref="m_BaselineValue"/> 为分界（默认 Y=0），
    /// 曲线在基线之上的部分用 above 颜色对填充，之下的部分用 below 颜色对填充。
    /// 跨基线的线段会自动求交点拆分，渐变完全靠顶点色实现，无需自定义 Shader。
    ///
    /// 同时可选叠加一条"顶部描边折线"（如截图 ② ③ 中曲线本体的白色细线）。
    /// 4 个填充颜色字段中：
    /// - 把同侧两个颜色设为相同 → 纯色填充
    /// - 把同侧两个颜色 alpha 拉开 → 渐变填充
    /// - 把同侧颜色 alpha 设为 0 → 隐藏该侧填充（仅画另一侧）
    ///
    /// 适用图：② 红方积分领先曲线 / ③ 蓝方积分领先曲线（两个 Prefab 共用，仅互换颜色）
    /// </summary>
    [AddComponentMenu("UI/Chart/Split Area Chart Graphic")]
    public class SplitAreaChartGraphic : ChartGraphicBase
    {
        [Header("曲线（决定填充上下边界 + 顶部描边）")]
        [SerializeField]
        private ChartSeries m_Curve = new ChartSeries
        {
            name  = "Curve",
            color = Color.white,
            width = 2f,
            points = new List<Vector2>(),
        };

        [Header("分区基线（数据坐标系，通常为 0）")]
        [SerializeField]
        private float m_BaselineValue = 0f;

        [Header("基线之上的填充")]
        [SerializeField]
        [Tooltip("靠近曲线一侧的颜色（通常 alpha 较高，颜色饱和）。")]
        private Color m_AboveCurveColor = new Color(0.92f, 0.30f, 0.30f, 0.85f);

        [SerializeField]
        [Tooltip("靠近基线一侧的颜色（通常 alpha 较低，形成羽化）。设为相同色 alpha 一致即纯色填充。")]
        private Color m_AboveBaseColor = new Color(0.92f, 0.30f, 0.30f, 0.20f);

        [Header("基线之下的填充")]
        [SerializeField]
        private Color m_BelowCurveColor = new Color(0.40f, 0.55f, 1.00f, 0.85f);

        [SerializeField]
        private Color m_BelowBaseColor = new Color(0.40f, 0.55f, 1.00f, 0.20f);

        [Header("是否启用各模块")]
        [SerializeField]
        private bool m_FillEnabled = true;

        [SerializeField]
        [Tooltip("是否在填充之上叠加一条 Curve.color 颜色的细折线（如 ② ③ 的白色折线本体）。")]
        private bool m_StrokeEnabled = true;

        // 复用 buffer
        private static readonly List<Vector2> s_Buffer = new List<Vector2>(64);

        // -------------------------- 公共属性 --------------------------

        public ChartSeries Curve => m_Curve;

        public float BaselineValue
        {
            get => m_BaselineValue;
            set { m_BaselineValue = value; SetVerticesDirty(); }
        }

        public bool FillEnabled
        {
            get => m_FillEnabled;
            set { m_FillEnabled = value; SetVerticesDirty(); }
        }

        public bool StrokeEnabled
        {
            get => m_StrokeEnabled;
            set { m_StrokeEnabled = value; SetVerticesDirty(); }
        }

        /// <summary>替换曲线数据。</summary>
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
            SetVerticesDirty();
        }

        // -------------------------- 编辑器默认值 --------------------------

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            m_ValueRangeX = new Vector2(0f, 12f);
            m_ValueRangeY = new Vector2(-2000f, 2000f);
            m_BaselineValue = 0f;
        }
#endif

        // -------------------------- 绘制 --------------------------

        protected override void DrawChart(VertexHelper vh)
        {
            if (m_Curve == null || m_Curve.points == null || m_Curve.points.Count < 2) return;

            MapPointsToLocal(m_Curve.points, s_Buffer);

            // 基线在本地坐标系中的 Y
            float baselineLocalY = ValueToLocal(m_ValueRangeX.x, m_BaselineValue).y;

            if (m_FillEnabled)
            {
                DrawAreaSplit(vh, s_Buffer, baselineLocalY,
                    m_AboveCurveColor, m_AboveBaseColor,
                    m_BelowCurveColor, m_BelowBaseColor);
            }

            if (m_StrokeEnabled)
            {
                DrawPolylineThick(vh, s_Buffer, m_Curve.color, m_Curve.width);
            }
        }
    }
}
