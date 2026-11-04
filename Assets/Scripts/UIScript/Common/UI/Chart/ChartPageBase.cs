using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.UI.UIScript.Common.UI.Chart
{
    /// <summary>
    /// 图表 Page 抽象基类（继承自 <see cref="AbstractPage"/>）：
    /// - 统一持有 KeyPoints 容器与关键点 Prefab
    /// - 提供按对象池复用的关键点摆位（<see cref="SetKeyPoints"/>）和清理（<see cref="ClearKeyPoints"/>）
    /// - 子类通过 <see cref="Chart"/> 暴露具体的 ChartGraphic 实例，本基类负责坐标映射
    ///
    /// Prefab 节点要求（详见 Docs/0511_图表组件Prefab协作规范.md）：
    /// - <see cref="m_KeyPointsRoot"/> 必须和 ChartArea RectTransform 完全重合（同 Anchor / Size / Pivot）
    /// - 关键点 Prefab 自身 Pivot 推荐 (0.5, 0.5)，否则 anchoredPosition 计算会偏移
    /// </summary>
    public abstract class ChartPageBase : AbstractPage
    {
        [Header("关键点（趋势变化点 / 重要数据点 高亮特效占位）")]
        [SerializeField]
        [Tooltip("关键点容器，必须与 ChartArea 的 RectTransform 完全重合。")]
        protected RectTransform m_KeyPointsRoot;

        [SerializeField]
        [Tooltip("关键点 Prefab（美术做好的圆点 + 出现动画）。Pivot 推荐 (0.5, 0.5)。")]
        protected GameObject m_KeyPointPrefab;

        // 关键点对象池（按需 Instantiate，多余的 SetActive(false)）
        private readonly List<RectTransform> m_KeyPointPool = new List<RectTransform>();

        /// <summary>
        /// 子类返回自己持有的图表组件（用于 ValueToLocal 坐标映射）。
        /// </summary>
        protected abstract WinRateCurveChart Chart { get; }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);

            if (Chart != null && m_KeyPointsRoot != null)
            {
                m_KeyPointsRoot.position = Chart.rectTransform.position;
            }
        }

        /// <summary>
        /// 在指定数据点位置摆放关键点（自动池化复用，多余的隐藏）。
        /// </summary>
        public void SetKeyPoints(IList<Vector2> dataPoints)
        {
            if (Chart == null || m_KeyPointsRoot == null)
            {
                return;
            }

            int n = dataPoints?.Count ?? 0;
            EnsureKeyPointCount(n);

            for (int i = 0; i < m_KeyPointPool.Count; i++)
            {
                var rt = m_KeyPointPool[i];
                if (rt == null) continue;

                bool active = i < n;
                rt.gameObject.SetActive(active);
                if (active)
                {
                    rt.localPosition = Chart.ValueToLocal(dataPoints[i]);
                }
            }
        }

        /// <summary>
        /// 隐藏所有已实例化的关键点（保留池，供下次复用）。
        /// </summary>
        public void ClearKeyPoints()
        {
            for (int i = 0; i < m_KeyPointPool.Count; i++)
            {
                var rt = m_KeyPointPool[i];
                if (rt != null)
                {
                    rt.gameObject.SetActive(false);
                }
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            ClearKeyPoints();
        }

        private void EnsureKeyPointCount(int count)
        {
            if (m_KeyPointPrefab == null || m_KeyPointsRoot == null) return;

            while (m_KeyPointPool.Count < count)
            {
                var go = Instantiate(m_KeyPointPrefab, m_KeyPointsRoot);
                m_KeyPointPool.Add(go.transform as RectTransform);
            }
        }
    }
}
