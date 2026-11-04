using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

namespace LetsPiu.Component
{
    [AddComponentMenu("UI/Single Scroll View", 39)]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class SingleScrollView : ScrollView
    {
        struct CellData
        {
            public Vector2 cellSize;
            public Vector2 cellPos;
        }

        List<CellData> m_CellDataList = new List<CellData>();
        bool m_IsVertical;

        public delegate int CellGetSizeDelegate(int index);

        // 获取元素高度回调
        public CellGetSizeDelegate onGetCellHeight;
        // 获取元素宽度回调
        public CellGetSizeDelegate onGetCellWidth;

        // 单元格之间的间距
        public int m_CellPadding = 0;

        /// <summary>
        /// 是否 滚动到最后一个cell
        /// </summary>
        public bool IsScrollAtEndCell()
        {
            if (m_IsVertical)
            {
                return IsScrollVerticalAtEndCell();
            }
            
            return IsScrollHorizontalAtEndCell();
        }
        
        /// <summary>
        /// 是否 滚动到第一个cell
        /// </summary>
        public bool IsScrollAtStartCell()
        {
            if (m_IsVertical)
            {
                return IsScrollVerticalAtStartCell();
            }
            
            return IsScrollHorizontalAtStartCell();
        }
        
        /// <summary>
        /// 滚动几个元素值
        /// </summary>
        public override void ScrollCell(int cellCount, float duration = 0.4f, Action onCompleted = null)
        {
            GetCellStartNormalizedPosition(1, out float cell1Horizontal, out float cell2Vertical);

            float endHorizontal = GetEndValue(horizontalNormalizedPosition, cell1Horizontal, cellCount);
            float endVertical = GetEndValue(verticalNormalizedPosition, cell2Vertical, cellCount);

            ScrollTo(endHorizontal, endVertical, duration, onCompleted);
        }
        
        /// <summary>
        /// 获取要滚动到的进度条位置
        /// </summary>
        /// <param name="curValue"> 当前进度值 </param>
        /// <param name="oneCellValue"> 一个元素占用的进度值 </param>
        /// <param name="cellCount"> 要滚动的元素数量，可以是负数 </param>
        /// <returns> 滚动到的目标进度值 </returns>
        float GetEndValue(float curValue, float oneCellValue, int cellCount)
        {
            // 当前已经滚动了多少个（加上半个元素宽度计算，防止显示区域在一半的位置）
            int curCellCount = (int)((curValue + (oneCellValue * 0.5f)) / oneCellValue);
            float endHorizontal = (curCellCount + cellCount) * oneCellValue;
            return Mathf.Clamp01(endHorizontal);
        }

        protected override void OnRefillCell(int index)
        {
            RefreshSizeAndPos();
        }

        protected override void OnRefreshSize()
        {
            m_IsVertical = this.vertical;

            if (m_IsVertical)
            {
                RefreshVerticalSize();
            }
            else
            {
                RefreshHorizontalSize();
            }
        }

        protected override Vector2 OnGetPos(int row, int col)
        {
            int index = m_IsVertical ? row : col;
            if (index >= m_CellDataList.Count)
            {
                return Vector2.zero;
            }

            CellData cellData = m_CellDataList[index];

            Vector2 pos = cellData.cellPos;
            // 处理居中
            pos += new Vector2(cellData.cellSize.x * 0.5f, -cellData.cellSize.y * 0.5f);
            // 处理偏移
            pos += new Vector2(m_Padding.left, -m_Padding.top);

            return pos;
        }

        protected override bool OnGetWidthHeight(int row, int col, out Vector2 sizeDelta)
        {
            sizeDelta = Vector2.zero;
            int index = m_IsVertical ? row : col;
            if (index >= m_CellDataList.Count)
            {
                return true;
            }

            CellData cellData = m_CellDataList[index];
            sizeDelta = cellData.cellSize;
            return true;
        }

        protected override int OnGetPosIndex(Vector2 pos)
        {
            int count = m_CellDataList.Count;
            if (count <= 0)
            {
                return -1;
            }

            pos -= new Vector2(m_Padding.left, -m_Padding.top);

            if (m_IsVertical)
            {
                if (pos.y > 0)
                {
                    return -1;
                }

                for (int i = 0; i < count; ++i)
                {
                    float slotEnd = (i + 1 < count)
                        ? m_CellDataList[i + 1].cellPos.y
                        : m_CellDataList[i].cellPos.y - m_CellDataList[i].cellSize.y;

                    if (pos.y >= slotEnd)
                    {
                        return i;
                    }
                }
            }
            else
            {
                if (pos.x < 0)
                {
                    return -1;
                }

                for (int i = 0; i < count; ++i)
                {
                    float slotEnd = (i + 1 < count)
                        ? m_CellDataList[i + 1].cellPos.x
                        : m_CellDataList[i].cellPos.x + m_CellDataList[i].cellSize.x;

                    if (pos.x <= slotEnd)
                    {
                        return i;
                    }
                }
            }

            return count - 1;
        }

        void RefreshVerticalSize()
        {
            m_RowCount = m_CellCount;
            m_ColCount = 1;

            int contentHeight = 0;
            m_CellDataList.Clear();

            for (int i = 0; i < m_CellCount; ++i)
            {
                int cellHeight = this.onGetCellHeight != null ? this.onGetCellHeight(i) : m_CellHeight;

                CellData data = new CellData();
                data.cellSize = new Vector2(m_CellWidth, cellHeight);
                data.cellPos = new Vector2(0, -contentHeight);
                m_CellDataList.Add(data);

                contentHeight += cellHeight + m_CellPadding; // 添加间距
            }

            int veiwWidth = Mathf.CeilToInt(this.viewport.rect.width);
            int contentWidth = veiwWidth - m_Padding.left - m_Padding.right;

            contentHeight = contentHeight + m_Padding.top + m_Padding.bottom;

            this.content.sizeDelta = new Vector2(contentWidth, contentHeight);

            if (m_DisableDragIfFits)
            {
                bool fits = this.content.sizeDelta.y <= this.viewport.rect.height;
                this.movementType = (fits ? MovementType.Clamped : MovementType.Elastic);
            }
        }

        void RefreshHorizontalSize()
        {
            m_RowCount = 1;
            m_ColCount = m_CellCount;

            int contentWidth = 0;
            m_CellDataList.Clear();

            for (int i = 0; i < m_CellCount; ++i)
            {
                int cellWidth = this.onGetCellWidth != null ? this.onGetCellWidth(i) : m_CellWidth;

                CellData data = new CellData();
                data.cellSize = new Vector2(cellWidth, m_CellHeight);
                data.cellPos = new Vector2(contentWidth, 0);
                m_CellDataList.Add(data);

                contentWidth += cellWidth + m_CellPadding; // 添加间距
            }

            contentWidth = contentWidth + m_Padding.left + m_Padding.right;

            int viewHeight = Mathf.CeilToInt(this.viewport.rect.height);
            int contentHeight = viewHeight - m_Padding.top - m_Padding.bottom;

            this.content.sizeDelta = new Vector2(contentWidth, contentHeight);

            if (m_DisableDragIfFits)
            {
                bool fits = this.content.sizeDelta.x <= this.viewport.rect.width;
                this.movementType = (fits ? MovementType.Clamped : MovementType.Elastic);
            }
        }

        void RefreshSizeAndPos()
        {
            OnRefreshSize();

            for (int i = 0; i < m_CellCount; ++i)
            {
                Transform cellTrans = GetCell(i);
                if (cellTrans == null)
                {
                    continue;
                }

                if (cellTrans is RectTransform)
                {
                    (cellTrans as RectTransform).anchoredPosition = m_IsVertical ? OnGetPos(i, 0) : OnGetPos(0, i);
                }
            }
        }
    }
}
