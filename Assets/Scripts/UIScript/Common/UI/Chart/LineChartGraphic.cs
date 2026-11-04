using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 折线图：支持任意条数的线（单线 / 双线 / 多线）。
    /// 每条线由一个 <see cref="ChartGraphicBase.ChartSeries"/> 描述，可在 Inspector 中独立配置颜色、宽度、数据点。
    ///
    /// 适用图：
    /// - ① 红队击杀曲线（1 条 Series）
    /// - 红蓝双队击杀对比曲线（2 条 Series：红队 + 蓝队）
    /// </summary>
    [AddComponentMenu("UI/Chart/Line Chart Graphic")]
    public class LineChartGraphic : ChartGraphicBase
    {
        [Header("全局分色模式（适配折线版的“上半红 / 下半蓝”）")]
        [SerializeField]
        [Tooltip("打开后忽略 Series.color，所有 Series 都用 AboveColor / BelowColor 沿 BaselineValue 拆分。")]
        private bool m_SplitLineColorByBaseline = false;

        [SerializeField]
        private float m_BaselineValue = 0f;

        [SerializeField]
        private Color m_AboveLineColor = new Color(0.92f, 0.30f, 0.30f, 1f);

        [SerializeField]
        private Color m_BelowLineColor = new Color(0.40f, 0.55f, 1.00f, 1f);

        [Header("折线 Series（增加列表项即可画多条线）")]
        [SerializeField]
        private List<ChartSeries> m_Series = new List<ChartSeries>
        {
            // 默认示例：红色击杀曲线（与截图 ① 风格一致），让空 Prefab 挂载即可预览
            new ChartSeries
            {
                name  = "RedTeam",
                color = new Color(0.93f, 0.20f, 0.20f, 1f),
                width = 4f,
                points = new List<Vector2>
                {
                    new Vector2(0, 5),  new Vector2(1, 10), new Vector2(2, 20),
                    new Vector2(3, 25), new Vector2(4, 33), new Vector2(5, 35),
                    new Vector2(6, 40), new Vector2(7, 45), new Vector2(8, 50),
                    new Vector2(9, 58), new Vector2(10, 65),
                },
            },
        };

        // 复用 buffer
        private static readonly List<Vector2> s_Buffer = new List<Vector2>(64);

        // -------------------------- 公共 API --------------------------

        /// <summary>已配置的 Series 数量。</summary>
        public int SeriesCount => m_Series.Count;

        /// <summary>访问指定 Series（用于改色、改线宽）。</summary>
        public ChartSeries GetSeries(int index) => m_Series[index];

        /// <summary>替换"第一条"Series 的数据点（最常用：单线场景）。</summary>
        public void SetData(IList<Vector2> points)
        {
            EnsureSeries(1);
            SetSeriesData(0, points);
        }

        /// <summary>替换指定 Series 的数据点。</summary>
        public void SetSeriesData(int seriesIndex, IList<Vector2> points)
        {
            if (seriesIndex < 0 || seriesIndex >= m_Series.Count)
            {
                Debug.LogWarning($"[LineChartGraphic] SeriesIndex {seriesIndex} out of range (count={m_Series.Count}).");
                return;
            }

            var s = m_Series[seriesIndex];
            s.points.Clear();
            if (points != null)
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    s.points.Add(points[i]);
                }
            }
            SetVerticesDirty();
        }

        /// <summary>追加一条新 Series。返回该 Series，可继续往里 Add 数据点。</summary>
        public ChartSeries AddSeries(string name, Color color, float width)
        {
            var s = new ChartSeries { name = name, color = color, width = width };
            m_Series.Add(s);
            SetVerticesDirty();
            return s;
        }

        /// <summary>清空所有 Series。</summary>
        public void ClearSeries()
        {
            m_Series.Clear();
            SetVerticesDirty();
        }

        /// <summary>清空所有 Series 的数据点（保留 Series 配置）。</summary>
        public void ClearAllPoints()
        {
            for (int i = 0; i < m_Series.Count; ++i)
            {
                m_Series[i].points.Clear();
            }
            SetVerticesDirty();
        }

        /// <summary>确保至少有 N 条 Series，不够时自动用默认色补足。</summary>
        public void EnsureSeries(int count)
        {
            while (m_Series.Count < count)
            {
                m_Series.Add(new ChartSeries
                {
                    name = $"Series{m_Series.Count}",
                    color = Color.white,
                    width = 4f,
                });
            }
            SetVerticesDirty();
        }

        // -------------------------- 编辑器默认值 --------------------------

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            m_ValueRangeX = new Vector2(0f, 10f);
            m_ValueRangeY = new Vector2(0f, 80f);
        }
#endif

        // -------------------------- 绘制 --------------------------

        protected override void DrawChart(VertexHelper vh)
        {
            float baselineLocalY = m_SplitLineColorByBaseline
                ? ValueToLocal(m_ValueRangeX.x, m_BaselineValue).y
                : 0f;

            for (int i = 0; i < m_Series.Count; ++i)
            {
                var s = m_Series[i];
                if (s == null || s.points == null || s.points.Count < 2) continue;

                MapPointsToLocal(s.points, s_Buffer);

                if (m_SplitLineColorByBaseline)
                {
                    DrawPolylineThickSplit(vh, s_Buffer, baselineLocalY,
                        m_AboveLineColor, m_BelowLineColor, s.width);
                }
                else
                {
                    DrawPolylineThick(vh, s_Buffer, s.color, s.width);
                }
            }
        }
    }
}
