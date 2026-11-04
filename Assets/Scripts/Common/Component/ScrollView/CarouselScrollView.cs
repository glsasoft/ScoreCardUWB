using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using LetsPiu.Base;

namespace LetsPiu.Component
{
    [AddComponentMenu("UI/Carousel Scroll View", 39)]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class CarouselScrollView : ScrollRect
    {
        [SerializeField]
        protected RectOffset m_Padding = new RectOffset();
        [SerializeField]
        protected GameObject m_CellTemplate;
        [SerializeField]
        protected int m_MaxWidth;
        [SerializeField]
        protected int m_CellWidth = 50;
        [SerializeField]
        protected int m_CellHeight = 50;
        [SerializeField]
        protected int m_MaxCreateCountPerFrame = 1;
        [SerializeField]
        protected bool m_DisableDragIfFits;
        [SerializeField]
        protected bool m_IsResizeCell;  // 是否重新设置cell大小
        [SerializeField]
        protected GameObject m_NotDataObject;   // 无数据时显示节点

        protected int m_CellCount;
        protected int m_ColCount;
        protected int m_RowCount;

        List<GameObject> m_AllCells = new List<GameObject>();
        ObjectCache m_CellCache;

        List<GameObject> m_DestroyingList = new List<GameObject>();

        int m_ShowStartIndex = -1;
        int m_ShowEndIndex = -1;

        bool m_NeedCreateNew;
        bool m_IsDragging;
        bool m_DragEnable = true;

        DragStateEvent m_DragStateEvent = new DragStateEvent();

        public class DragStateEvent : UnityEvent<bool> { };
        public delegate void CellUpdateDelegate(Transform root, int index);

        // 元素刷新回调
        public CellUpdateDelegate onCellUpdate;

        public RectOffset padding { get { return m_Padding; } }

        // 拖拽事件
        public DragStateEvent onDragStateChanged { get { return m_DragStateEvent; } }

        // 元素模版
        public GameObject cellTemplate
        {
            get { return m_CellTemplate; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogWarning("Scroll view can only set cell template in editor mode.");
                    return;
                }

                m_CellTemplate = value;

                if (m_CellTemplate != null)
                {
                    m_CellTemplate.SetActive(false);
                }
            }
        }

        // 元素总数量
        public int totalCount
        {
            get { return m_CellCount; }
            set
            {
                m_CellCount = value;
            }
        }

        public int cellWidth
        {
            get { return m_CellWidth; }
        }
        
        public int cellHeight
        {
            get { return m_CellHeight; }
        }

        // 每帧最大创建数量
        public int maxCreateCountPerFrame
        {
            get { return m_MaxCreateCountPerFrame; }
            set { m_MaxCreateCountPerFrame = value; }
        }

        public bool isDragging
        {
            get { return m_IsDragging; }
        }

        /// <summary>
        /// 最大宽度
        /// </summary>
        public int maxWidth 
        {
            get { return m_MaxWidth; }
            set { m_MaxWidth = value; }
        }
        
        /// <summary>
        /// 是否开启滑动
        /// </summary>
        public bool scrollEnable 
        {
            get { return m_DragEnable; }
            set { m_DragEnable = value; }
        }


        public int defaultCellHeight
        {
            get { return m_CellHeight; }
        }
        
        public int showStartIndex
        {
            get { return Math.Max(m_ShowStartIndex, 0); }
        }
        
        public int showEndIndex
        {
            get { return Math.Min(m_ShowEndIndex, m_CellCount - 1); }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_CellCache != null)
            {
                m_CellCache.DestroyAll();
            }
        }

        /// <summary>
        /// 刷新全部列表(设置resetPosition和JumpTo有冲突，如果Refresh后要调用JumpToStart或JumpToEnd请设置resetPosition为false)
        /// </summary>
        public void Refresh(int count, bool resetPosition = true)
        {
            count = Mathf.Max(count, 0);
            OnRefreshAll(count, resetPosition);
        }

        // 重新填充所有元素
        public void Refill()
        {
            OnRefreshAll(m_CellCount, false);
        }

        // 重新填充某个元素
        public void RefillCell(int index)
        {
            Transform cell = GetCell(index);
            if (cell != null && this.onCellUpdate != null)
            {
                this.onCellUpdate(cell, index);
            }

            OnRefillCell(index);
        }

        // 获取元素对象，该元素不在显示区间时，会不存在的
        public Transform GetCell(int index)
        {
            if (index < 0 || index >= m_AllCells.Count)
            {
                return null;
            }

            GameObject cell = m_AllCells[index];

            if (cell == null)
            {
                return null;
            }

            return cell.transform;
        }

        public List<GameObject> GetAllCell()
        {
            return m_AllCells;
        }

        // 跳到该元素起始位置
        public void JumpToStart(int index)
        {
            if (m_ColCount <= 0)
            {
                return;
            }

            float horizontalPosition, verticalPosition;
            GetCellStartNormalizedPosition(index, out horizontalPosition, out verticalPosition);
            this.horizontalNormalizedPosition = horizontalPosition;
            this.verticalNormalizedPosition = verticalPosition;

            // 刷新显示
            RefreshShowCell(false);
        }

        /// <summary>
        /// 滚动几个元素值
        /// </summary>
        public virtual void ScrollCell(int cellCount, float duration = 0.5f, Action onCompleted = null)
        {
            Debug.Log("[ScrollView:ScrollCell] ScrollView is not implemented ScrollCell");
        }

        
        /// <summary>
        /// 是否 水平滚动到最后一个cell
        /// </summary>
        public virtual bool IsScrollHorizontalAtEndCell()
        {
            int totalWidth = m_CellCount * cellWidth;
            float currentPos = horizontalNormalizedPosition * totalWidth;
            
            return (totalWidth - currentPos) <= cellWidth;
        }
        
        /// <summary>
        /// 是否 水平滚动到第一个cell
        /// </summary>
        public virtual bool IsScrollHorizontalAtStartCell()
        {
            int totalWidth = m_CellCount * cellWidth;
            float currentPos = horizontalNormalizedPosition * totalWidth;
            
            return currentPos <= cellWidth;
        }
        
        /// <summary>
        /// 是否 竖直滚动到最后一个cell
        /// </summary>
        public virtual bool IsScrollVerticalAtEndCell()
        {
            int totalHeight = m_CellCount * cellHeight;
            float currentPos = verticalNormalizedPosition * totalHeight;
            
            return (totalHeight - currentPos) <= cellHeight;
        }
        
        /// <summary>
        /// 是否 竖直滚动到第一个cell
        /// </summary>
        /// <returns></returns>
        public virtual bool IsScrollVerticalAtStartCell()
        {
            int totalHeight = m_CellCount * cellHeight;
            float currentPos = horizontalNormalizedPosition * totalHeight;
            
            return currentPos <= cellHeight;
        }
        
        /// <summary>
        /// 滚动显示到目标位置
        /// </summary>
        protected void ScrollTo(float targetHorizontalPosition, float targetVerticalPosition, float duration, Action onCompleted = null)
        {
            if (m_ColCount <= 0)
            {
                return;
            }

            content.DOKill();

            var targetNormalizedPos = new Vector2(targetHorizontalPosition, targetVerticalPosition);

            var tweener = DOTween.To(() => normalizedPosition, targetPos => normalizedPosition = targetPos,
                targetNormalizedPos, duration).SetTarget(content);

            if (onCompleted == null)
            {
                return;
            }
            
            tweener.OnComplete(() =>
            {
                if (onCompleted != null)
                {
                    onCompleted.Invoke();
                }
            });
        }

        protected void  GetCellStartNormalizedPosition(int index, out float endX, out float endY)
        {
            endX = this.horizontalNormalizedPosition;
            endY = this.verticalNormalizedPosition;
            
            if (m_ColCount <= 0)
            {
                return;
            }

            int row = index / m_ColCount;
            int col = index % m_ColCount;

            Vector2 pos = OnGetPos(row, col);

            Vector2 contentSize = this.content.sizeDelta;
            Vector2 viewSize = this.viewRect.rect.size;
            Vector2 cellSize;
            if (!OnGetWidthHeight(row, col, out cellSize))
            {
                cellSize.x = m_CellWidth;
                cellSize.y = m_CellHeight;
            }

            
            if (this.vertical)
            {
                float startHeight = -pos.y - cellSize.y * 0.5f - m_Padding.top;
                float moveHeight = contentSize.y - viewSize.y;

                if (moveHeight > 0)
                {
                    if (startHeight > moveHeight)
                    {
                        endY = 0;
                    }
                    else
                    {
                        endY = 1f - startHeight / moveHeight;
                    }
                }
            }

            if (this.horizontal)
            {
                float startWidth = pos.x - cellSize.x * 0.5f - m_Padding.left;
                float moveWidth = contentSize.x - viewSize.x;

                if (moveWidth > 0)
                {
                    if (startWidth > moveWidth)
                    {
                        endX = 1;
                    }
                    else
                    {
                        endX = startWidth / moveWidth;
                    }
                }
            }
        }

        // 跳到该元素终止位置
        public void JumpToEnd(int index)
        {
            if (m_ColCount <= 0)
            {
                return;
            }

            float horizontalPosition, verticalPosition;
            GetCellEndNormalizedPosition(index, out horizontalPosition, out verticalPosition);
            this.horizontalNormalizedPosition = horizontalPosition;
            this.verticalNormalizedPosition = verticalPosition;

            // 刷新显示
            RefreshShowCell(false);
        }

        void GetCellEndNormalizedPosition(int index, out float endX, out float endY)
        {
            endX = this.horizontalNormalizedPosition;
            endY = this.verticalNormalizedPosition;

            if (m_ColCount <= 0)
            {
                return;
            }

            int row = index / m_ColCount;
            int col = index % m_ColCount;

            Vector2 pos = OnGetPos(row, col);

            Vector2 contentSize = this.content.sizeDelta;
            Vector2 viewSize = this.viewRect.rect.size;
            Vector2 cellSize;
            if (!OnGetWidthHeight(row, col, out cellSize))
            {
                cellSize.x = m_CellWidth;
                cellSize.y = m_CellHeight;
            }

            if (this.vertical)
            {
                float startHeight = -pos.y + cellSize.y * 0.5f - m_Padding.top - viewSize.y;
                float moveHeight = contentSize.y - viewSize.y;

                if (moveHeight > 0)
                {
                    if (startHeight < 0)
                    {
                        endY = 1;
                    }
                    else if (startHeight > moveHeight)
                    {
                        endY = 0;
                    }
                    else
                    {
                        endY = 1f - startHeight / moveHeight;
                    }
                }
            }

            if (this.horizontal)
            {
                float startWidth = pos.x + cellSize.x * 0.5f - m_Padding.left - viewSize.x;
                float moveWidth = contentSize.x - viewSize.x;

                if (moveWidth > 0)
                {
                    if (startWidth < 0)
                    {
                        endX = 0;
                    }
                    else if (startWidth > moveWidth)
                    {
                        endX = 1;
                    }
                    else
                    {
                        endX = startWidth / moveWidth;
                    }
                }
            }
        }

        /// <summary>
        /// 显示内容超过显示范围时, 开启竖直滚动; 否则关闭竖直滚动
        /// </summary>
        public void ClampVerticalScrolling()
        {
            vertical = viewport.rect.height < content.rect.height;
        }

        /// <summary>
        /// 显示内容超过显示范围时, 开启水平滚动; 否则关闭水平滚动
        /// </summary>
        public void ClampHorizontalScrolling()
        {
            horizontal = viewport.rect.width < content.rect.width;
        }

        protected override void Awake()
        {
            base.Awake();

            ValidateTransfroms();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (m_NeedCreateNew)
            {
                UpdateCreateNew();
            }

            if (!Application.isPlaying)
            {
                for (int i = 0; i < m_DestroyingList.Count; ++i)
                {
                    if (m_DestroyingList[i] != null)
                    {
                        GameObject.DestroyImmediate(m_DestroyingList[i]);
                    }
                }

                m_DestroyingList.Clear();
            }
        }

        protected virtual void OnRefreshAll(int count, bool resetPosition)
        {
            if (m_CellTemplate == null)
            {
                Debug.LogWarning("[ScrollView:Refresh]template is null.");
                return;
            }

            // 回收全部使用过的
            ReleaseAllUsed();

            // 重新设置数量
            ResizeCellCount(count);

            // 刷新无数据节点显示
            RefreshNotDataObj(count);

            // 刷新大小
            OnRefreshSize();

            if (m_MaxCreateCountPerFrame <= 0)
            {
                m_MaxCreateCountPerFrame = m_ColCount;
            }

            if (resetPosition)
            {
                JumpToStart(0);
                this.content.anchoredPosition = Vector2.zero;
            }

            RefreshShowCell(true);
        }

        protected virtual void OnRefillCell(int index)
        {

        }

        protected virtual void OnRefreshSize()
        {
            int maxWidth = m_MaxWidth;
            if (maxWidth <= 0)
            {
                maxWidth = Mathf.CeilToInt(this.viewport.rect.width);
            }

            int width = maxWidth - m_Padding.left - m_Padding.right;
            m_ColCount = Mathf.CeilToInt(width / (float)m_CellWidth) - 1;
            m_ColCount = Mathf.Clamp(m_ColCount, 1, 1000);
            m_RowCount = Mathf.CeilToInt(m_CellCount / (float)m_ColCount);

            int maxHeight = m_RowCount * m_CellHeight + m_Padding.top + m_Padding.bottom;
            this.content.sizeDelta = new Vector2(maxWidth, maxHeight);

            if (Application.isPlaying && m_DisableDragIfFits)
            {
                bool fits = (!this.horizontal || this.content.sizeDelta.x <= this.viewport.rect.width) &&
                    (!this.vertical || this.content.sizeDelta.y <= this.viewport.rect.height);
                this.movementType = (fits ? MovementType.Clamped : MovementType.Elastic);
            }
        }

        protected virtual Vector2 OnGetPos(int row, int col)
        {
            int x = col * m_CellWidth + m_Padding.left;
            int y = row * m_CellHeight + m_Padding.top;

            return new Vector2(x + m_CellWidth * 0.5f, -y - m_CellHeight * 0.5f);
        }

        /// <summary>
        /// 获取cell的宽和高
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>true:需要刷新cell的宽和高; fasle:不需要刷新cell的宽高</returns>
        protected virtual bool OnGetWidthHeight(int row, int col, out Vector2 sizeDelta)
        {
            sizeDelta = Vector2.zero;

            // 默认不需要刷新
            return false;
        }

        protected virtual int OnGetPosIndex(Vector2 pos)
        {
            int col = (int)((pos.x - m_Padding.left) / m_CellWidth);
            int row = (int)((-pos.y - m_Padding.top) / m_CellHeight);

            col = Mathf.Clamp(col, 0, m_ColCount - 1);
            row = Mathf.Clamp(row, 0, m_RowCount - 1);

            int index = row * m_ColCount + col;

            return Mathf.Max(index, -1);
        }

        protected virtual bool OnCreateCell(int row, int col)
        {
            int cellIndex = row * m_ColCount + col;
            return CreateCellWithIndex(cellIndex, row, col);
        }

        protected bool CreateCellWithIndex(int cellIndex, int row, int col)
        {
            if (cellIndex < 0 || cellIndex >= m_AllCells.Count)
            {
                return false;
            }

            if (m_AllCells[cellIndex] != null)
            {
                return false;
            }

            GameObject go = m_CellCache.Get();
            m_AllCells[cellIndex] = go;

            RectTransform rectTransfrom = go.transform as RectTransform;
            rectTransfrom.localRotation = m_CellTemplate.transform.localRotation;
            rectTransfrom.localScale = m_CellTemplate.transform.localScale;
            rectTransfrom.anchoredPosition = OnGetPos(row, col);
            Vector2 sizeDelta;
            if (m_IsResizeCell && OnGetWidthHeight(row, col, out sizeDelta))
            {
                rectTransfrom.sizeDelta = sizeDelta;
            }

            if (this.onCellUpdate != null)
            {
                try
                {
                    this.onCellUpdate(go.transform, cellIndex);
                }
                catch(System.Exception ex)
                {
                    Debug.LogError(ex.StackTrace);
                    Debug.LogError(ex.Message);
                }
            }

            return true;
        }

        protected CarouselScrollView()
        {
            this.onValueChanged.AddListener(OnValueChanged);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
            {
                return;
            }

            ValidateTransfroms();
            Refresh(m_CellCount, false);
        }
#endif

        public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!m_DragEnable)
            {
                return;
            }
            
            base.OnBeginDrag(eventData);
            m_IsDragging = true;
            m_DragStateEvent.Invoke(true);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!m_DragEnable)
            {
                return;
            }
            
            base.OnDrag(eventData);
        }

        public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!m_DragEnable)
            {
                return;
            }
            
            base.OnEndDrag(eventData);
            m_IsDragging = false;
            m_DragStateEvent.Invoke(false);
        }

        void OnValueChanged(Vector2 value)
        {
            RefreshShowCell(false);
        }

        void ValidateTransfroms()
        {
            if (m_CellTemplate != null)
            {
                m_CellTemplate.SetActive(false);

                RectTransform rectTransform = m_CellTemplate.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                    rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                    rectTransform.pivot     = new Vector2(0.5f, 0.5f);
                }
            }

            if (viewport != null)
            {
                viewport.anchorMin = new Vector2(0.0f, 0.0f);
                viewport.anchorMax = new Vector2(1.0f, 1.0f);
                viewport.pivot     = new Vector2(0.5f, 0.5f);
            }

            if (content != null)
            {
                content.anchorMin = new Vector2(0.0f, 0.0f);
                content.anchorMax = new Vector2(1.0f, 1.0f);
                content.pivot     = new Vector2(0.5f, 0.5f);
            }
        }

        bool IsInShowRange(int index, int startIndex, int endIndex)
        {
            int row = index / m_ColCount;
            int col = index % m_ColCount;

            int startRow = startIndex / m_ColCount;
            int startCol = startIndex % m_ColCount;

            if (row < startRow || col < startCol)
            {
                return false;
            }

            int endRow = endIndex / m_ColCount;
            int endCol = endIndex % m_ColCount;

            if (row > endRow || col > endCol)
            {
                return false;
            }

            return true;
        }

        void RefreshShowCell(bool forceUpdate)
        {
            if (m_CellCount <= 0)
            {
                return;
            }

            // 获取新的显示区间
            int startIndex;
            int endIndex;
            CalcShowRange(out startIndex, out endIndex);

            // 显示区间未改变
            if (!forceUpdate && m_ShowStartIndex == startIndex && m_ShowEndIndex == endIndex)
            {
                return;
            }

            // 回收无效的
            for (int i = m_ShowStartIndex; i <= m_ShowEndIndex; ++i)
            {
                if (i < 0 || i >= m_AllCells.Count)
                {
                    continue;
                }

                if (!IsInShowRange(i, startIndex, endIndex))
                {
                    if (m_AllCells[i] != null)
                    {
                        ReleaseCell(m_AllCells[i]);
                        m_AllCells[i] = null;
                    }
                }
            }

            // 记录新的显示区间
            m_ShowStartIndex = startIndex;
            m_ShowEndIndex = endIndex;

            // 创建新的
            UpdateCreateNew();
        }

        void CalcShowRange(out int startIndex, out int endIndex)
        {
            // 内容起始位置
            Vector2 startPos = this.content.anchoredPosition * -1;
            // 内容结束位置
            Vector2 endPos = startPos + new Vector2(this.viewport.rect.width, -this.viewport.rect.height);

            startIndex = OnGetPosIndex(startPos);
            endIndex = OnGetPosIndex(endPos);
        }

        /// <summary>
        /// 刷新无数据节点
        /// </summary>
        void RefreshNotDataObj(int dataCount)
        {
            if (m_NotDataObject != null)
            {
                m_NotDataObject.SetActive(dataCount <= 0);
            }
        }

        void ResizeCellCount(int cellCount)
        {
            if (m_CellCache == null)
            {
                m_CellCache = new ObjectCache();
                m_CellCache.autoUnactive = false;
                m_CellCache.Start(this.content, m_CellTemplate);
            }

            if (m_AllCells.Count < cellCount)
            {
                int addCount = cellCount - m_AllCells.Count;
                for (int i = 0; i < addCount; ++i)
                {
                    m_AllCells.Add(null);
                }
            }
            else if (m_AllCells.Count > cellCount)
            {
                for (int i = m_AllCells.Count - 1; i >= cellCount; --i)
                {
                    ReleaseCell(m_AllCells[i]);
                    m_AllCells.RemoveAt(i);
                }
            }

            m_CellCount = m_AllCells.Count;
        }

        void UpdateCreateNew()
        {
            if (m_CellCount <= 0)
            {
                return;
            }

            m_NeedCreateNew = true;

            int startRow = m_ShowStartIndex / m_ColCount;
            int startCol = m_ShowStartIndex % m_ColCount;

            int endRow = m_ShowEndIndex / m_ColCount;
            int endCol = m_ShowEndIndex % m_ColCount;

            int createCount = m_MaxCreateCountPerFrame + m_CellCache.count;

            for (int r = startRow; r <= endRow; ++r)
            {
                if (r < 0)
                {
                    continue;
                }

                for (int c = startCol; c <= endCol; ++c)
                {
                    if (c < 0)
                    {
                        continue;
                    }

                    if (OnCreateCell(r, c))
                    {
                        if (Application.isPlaying && --createCount <= 0)
                        {
                            return;
                        }
                    }
                }
            }

            m_NeedCreateNew = false;
        }

        void ReleaseAllUsed()
        {
            for (int i = m_AllCells.Count - 1; i >= 0; --i)
            {
                if (m_AllCells[i] != null)
                {
                    ReleaseCell(m_AllCells[i]);
                    m_AllCells[i] = null;
                }
            }
        }

        void ReleaseCell(GameObject cell)
        {
            if (cell == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                if (m_CellCache != null)
                {
                    m_CellCache.Release(cell);
                }
            }
            else
            {
                m_DestroyingList.Add(cell);
            }
        }
    }
}
