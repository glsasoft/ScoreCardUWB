using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 胜率曲线的数据与坐标映射，不是 UGUI Graphic，自身不绘制任何内容。
    /// 挂在图表区域节点（GraphImage）上，提供：
    /// 数据坐标 → 本地坐标映射、单调三次样条平滑折线、基线位置。
    /// 曲线的视觉由 <see cref="LetsPiu.UI.CurveChartVFX"/> 特效负责。
    /// </summary>
    public class WinRateCurveChart : MonoBehaviour
    {
        [Header("数据值域（必须与 Prefab 上写死的坐标轴文字保持一致）")]
        [SerializeField] private Vector2 m_ValueRangeX = new Vector2(0f, 8f);
        [SerializeField] private Vector2 m_ValueRangeY = new Vector2(-1f, 1f);

        [Header("平滑")]
        [SerializeField, Range(2, 32)]
        [Tooltip("每两个相邻数据点之间插值的细分段数，越大越平滑。")]
        private int m_SegmentsPerSpan = 12;

        [Header("基线（数据坐标系），红蓝特效以它为界")]
        [SerializeField] private float m_BaselineValue;

        private readonly List<Vector2> m_Points = new List<Vector2>();
        private readonly List<Vector2> m_Local = new List<Vector2>();

        public RectTransform rectTransform => (RectTransform)transform;

        public Vector2 ValueRangeX { get => m_ValueRangeX; set => m_ValueRangeX = value; }
        public Vector2 ValueRangeY { get => m_ValueRangeY; set => m_ValueRangeY = value; }

        /// <summary>基线在本地坐标系中的 Y。</summary>
        public float BaselineLocalY => ValueToLocal(m_ValueRangeX.x, m_BaselineValue).y;

        /// <summary>基线到图表上下边缘的最大距离（本地坐标），可作为"从曲线填满到基线"的最大深度。</summary>
        public float MaxDistanceFromBaseline
        {
            get
            {
                float baseline = BaselineLocalY;
                float top = ValueToLocal(m_ValueRangeX.x, m_ValueRangeY.y).y;
                float bottom = ValueToLocal(m_ValueRangeX.x, m_ValueRangeY.x).y;
                return Mathf.Max(Mathf.Abs(top - baseline), Mathf.Abs(baseline - bottom));
            }
        }

        public void SetData(IList<Vector2> points)
        {
            m_Points.Clear();
            if (points != null)
                m_Points.AddRange(points);
        }

        /// <summary>
        /// 把数据坐标映射到本节点 RectTransform 的局部坐标（受 Pivot 影响）。
        /// 摆放转折点时也使用它，保证位置和曲线对齐。
        /// </summary>
        public Vector2 ValueToLocal(Vector2 dataPoint)
        {
            return ValueToLocal(dataPoint.x, dataPoint.y);
        }

        public Vector2 ValueToLocal(float x, float y)
        {
            Rect r = rectTransform.rect;
            return new Vector2(
                Mathf.LerpUnclamped(r.xMin, r.xMax, InverseLerp(m_ValueRangeX.x, m_ValueRangeX.y, x)),
                Mathf.LerpUnclamped(r.yMin, r.yMax, InverseLerp(m_ValueRangeY.x, m_ValueRangeY.y, y)));
        }

        /// <summary>
        /// 把当前数据平滑成密集折线，输出本地坐标（本节点 RectTransform 局部空间），x 递增。
        /// 采用单调三次样条（Fritsch-Carlson），曲线不会超出相邻数据点的 Y 范围。
        /// </summary>
        public void GetSmoothedLocalPoints(List<Vector2> output)
        {
            output.Clear();
            int n = m_Points.Count;
            if (n < 2) return;

            m_Local.Clear();
            for (int i = 0; i < n; i++)
                m_Local.Add(ValueToLocal(m_Points[i]));
            m_Local.Sort((a, b) => a.x.CompareTo(b.x));

            var delta = new float[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                float dx = m_Local[i + 1].x - m_Local[i].x;
                delta[i] = Mathf.Approximately(dx, 0f) ? 0f : (m_Local[i + 1].y - m_Local[i].y) / dx;
            }

            var m = new float[n];
            m[0] = delta[0];
            m[n - 1] = delta[n - 2];
            for (int i = 1; i < n - 1; i++)
                m[i] = (delta[i - 1] + delta[i]) * 0.5f;

            for (int i = 0; i < n - 1; i++)
            {
                if (Mathf.Abs(delta[i]) < 1e-6f)
                {
                    m[i] = 0f;
                    m[i + 1] = 0f;
                    continue;
                }
                float alpha = m[i] / delta[i];
                float beta = m[i + 1] / delta[i];
                float tau = alpha * alpha + beta * beta;
                if (tau > 9f)
                {
                    float scale = 3f / Mathf.Sqrt(tau);
                    m[i] = scale * alpha * delta[i];
                    m[i + 1] = scale * beta * delta[i];
                }
            }

            output.Add(m_Local[0]);
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p0 = m_Local[i];
                Vector2 p1 = m_Local[i + 1];
                float h = p1.x - p0.x;
                for (int s = 1; s <= m_SegmentsPerSpan; s++)
                {
                    float t = (float)s / m_SegmentsPerSpan;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    float y = (2f * t3 - 3f * t2 + 1f) * p0.y
                              + (t3 - 2f * t2 + t) * h * m[i]
                              + (-2f * t3 + 3f * t2) * p1.y
                              + (t3 - t2) * h * m[i + 1];
                    output.Add(new Vector2(Mathf.Lerp(p0.x, p1.x, t), y));
                }
            }
        }

        private static float InverseLerp(float a, float b, float v)
        {
            return Mathf.Approximately(a, b) ? 0f : (v - a) / (b - a);
        }
    }
}
