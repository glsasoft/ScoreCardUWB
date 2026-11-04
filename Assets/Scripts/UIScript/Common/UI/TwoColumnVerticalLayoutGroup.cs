using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI
{
    /// <summary>
    /// 多列布局。一列能放下全部则单列水平居中；否则从左往右、再从上往下排（如 5 个 → 左列 1/3/5，右列 2/4）。
    /// </summary>
    [AddComponentMenu("Layout/Two Column Vertical Layout Group")]
    [ExecuteAlways]
    public class TwoColumnVerticalLayoutGroup : LayoutGroup
    {
        public enum WrapMode
        {
            /// <summary>一列高度够则单列居中；不够则从左往右、从上往下排到多列。</summary>
            ByContainerHeight = 0,
            /// <summary>同 ByContainerHeight（保留兼容）。</summary>
            Balanced = 1,
        }

        [SerializeField]
        float m_RowSpacing = 16f;

        [SerializeField]
        float m_ColumnSpacing = 40f;

        [SerializeField]
        [Min(1)]
        int m_ColumnCount = 2;

        [SerializeField]
        WrapMode m_WrapMode = WrapMode.ByContainerHeight;

        readonly List<List<int>> m_Columns = new List<List<int>>(4);
        readonly List<float> m_ColumnWidths = new List<float>(4);
        readonly List<float> m_ColumnHeights = new List<float>(4);

        public float rowSpacing
        {
            get => m_RowSpacing;
            set => SetProperty(ref m_RowSpacing, value);
        }

        public float columnSpacing
        {
            get => m_ColumnSpacing;
            set => SetProperty(ref m_ColumnSpacing, value);
        }

        public int columnCount
        {
            get => m_ColumnCount;
            set => SetProperty(ref m_ColumnCount, Mathf.Max(1, value));
        }

        public WrapMode wrapMode
        {
            get => m_WrapMode;
            set => SetProperty(ref m_WrapMode, value);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            BuildColumns();

            float width = padding.horizontal;
            for (int c = 0; c < m_Columns.Count; c++)
            {
                if (c > 0) width += m_ColumnSpacing;
                width += m_ColumnWidths[c];
            }

            SetLayoutInputForAxis(width, width, -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            BuildColumns();

            float height = padding.vertical;
            float maxColH = 0f;
            for (int c = 0; c < m_ColumnHeights.Count; c++)
            {
                maxColH = Mathf.Max(maxColH, m_ColumnHeights[c]);
            }

            height += maxColH;
            SetLayoutInputForAxis(height, height, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis();
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ColumnCount = Mathf.Max(1, m_ColumnCount);
            m_RowSpacing = Mathf.Max(0f, m_RowSpacing);
            m_ColumnSpacing = Mathf.Max(0f, m_ColumnSpacing);
        }
#endif

        void BuildColumns()
        {
            m_Columns.Clear();
            m_ColumnWidths.Clear();
            m_ColumnHeights.Clear();

            int childCount = rectChildren.Count;
            if (childCount == 0) return;

            int maxCols = Mathf.Max(1, m_ColumnCount);

            // 一列高度够放所有子项时，只用单列（后续水平居中，不用列间距）
            if (FitsInSingleColumn(childCount))
            {
                BuildSingleColumn(childCount);
            }
            else
            {
                BuildRowMajorColumns(childCount, maxCols);
            }

            for (int c = 0; c < m_Columns.Count; c++)
            {
                float colW = 0f;
                float colH = 0f;
                var col = m_Columns[c];
                for (int i = 0; i < col.Count; i++)
                {
                    var child = rectChildren[col[i]];
                    colW = Mathf.Max(colW, GetChildWidth(child));
                    if (i > 0) colH += m_RowSpacing;
                    colH += GetChildHeight(child);
                }

                m_ColumnWidths.Add(colW);
                m_ColumnHeights.Add(colH);
            }
        }

        bool FitsInSingleColumn(int childCount)
        {
            if (childCount <= 1) return true;
            if (!CanUseHeightWrap()) return false;

            float availH = rectTransform.rect.height - padding.vertical;
            float totalH = 0f;
            for (int i = 0; i < childCount; i++)
            {
                if (i > 0) totalH += m_RowSpacing;
                totalH += GetChildHeight(rectChildren[i]);
            }

            return totalH <= availH + 0.01f;
        }

        bool CanUseHeightWrap()
        {
            float avail = rectTransform.rect.height - padding.vertical;
            return avail > 1f;
        }

        void BuildSingleColumn(int childCount)
        {
            var col = new List<int>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                col.Add(i);
            }

            m_Columns.Add(col);
        }

        void BuildRowMajorColumns(int childCount, int maxCols)
        {
            int colsUsed = Mathf.Min(maxCols, childCount);
            for (int c = 0; c < colsUsed; c++)
            {
                m_Columns.Add(new List<int>());
            }

            for (int i = 0; i < childCount; i++)
            {
                m_Columns[i % colsUsed].Add(i);
            }
        }

        void SetChildrenAlongAxis()
        {
            BuildColumns();
            if (m_Columns.Count == 0) return;

            bool singleColumn = m_Columns.Count == 1;

            float contentW = 0f;
            float maxColH = 0f;
            for (int c = 0; c < m_Columns.Count; c++)
            {
                if (!singleColumn && c > 0) contentW += m_ColumnSpacing;
                contentW += m_ColumnWidths[c];
                maxColH = Mathf.Max(maxColH, m_ColumnHeights[c]);
            }

            // 单列：强制水平居中；多列：按 ChildAlignment
            float startX = singleColumn
                ? GetCenteredStartX(contentW)
                : GetStartOffset(0, contentW);
            float startY = GetStartOffset(1, maxColH);

            float x = startX;
            for (int c = 0; c < m_Columns.Count; c++)
            {
                var col = m_Columns[c];
                float colW = m_ColumnWidths[c];
                float colH = m_ColumnHeights[c];
                float y = startY + GetColumnExtraY(colH, maxColH);

                for (int i = 0; i < col.Count; i++)
                {
                    var child = rectChildren[col[i]];
                    float w = GetChildWidth(child);
                    float h = GetChildHeight(child);
                    float childX = x + GetItemExtraX(w, colW);

                    SetChildAlongAxis(child, 0, childX, w);
                    SetChildAlongAxis(child, 1, y, h);
                    y += h + m_RowSpacing;
                }

                x += colW + (singleColumn ? 0f : m_ColumnSpacing);
            }
        }

        float GetCenteredStartX(float contentWidth)
        {
            float inner = rectTransform.rect.width - padding.horizontal;
            return padding.left + Mathf.Max(0f, (inner - contentWidth) * 0.5f);
        }

        float GetColumnExtraY(float colH, float maxColH)
        {
            if (childAlignment == TextAnchor.UpperLeft
                || childAlignment == TextAnchor.UpperCenter
                || childAlignment == TextAnchor.UpperRight)
            {
                return 0f;
            }

            if (childAlignment == TextAnchor.MiddleLeft
                || childAlignment == TextAnchor.MiddleCenter
                || childAlignment == TextAnchor.MiddleRight)
            {
                return (maxColH - colH) * 0.5f;
            }

            return maxColH - colH;
        }

        float GetItemExtraX(float itemW, float colW)
        {
            if (childAlignment == TextAnchor.UpperLeft
                || childAlignment == TextAnchor.MiddleLeft
                || childAlignment == TextAnchor.LowerLeft)
            {
                return 0f;
            }

            if (childAlignment == TextAnchor.UpperCenter
                || childAlignment == TextAnchor.MiddleCenter
                || childAlignment == TextAnchor.LowerCenter)
            {
                return (colW - itemW) * 0.5f;
            }

            return colW - itemW;
        }

        static float GetChildWidth(RectTransform child)
        {
            float min = LayoutUtility.GetMinSize(child, 0);
            float pref = LayoutUtility.GetPreferredSize(child, 0);
            float w = Mathf.Max(min, pref);
            if (w <= 0.01f)
            {
                w = child.rect.width;
            }

            return Mathf.Max(0f, w);
        }

        static float GetChildHeight(RectTransform child)
        {
            float min = LayoutUtility.GetMinSize(child, 1);
            float pref = LayoutUtility.GetPreferredSize(child, 1);
            float h = Mathf.Max(min, pref);
            if (h <= 0.01f)
            {
                h = child.rect.height;
            }

            return Mathf.Max(0f, h);
        }
    }
}
