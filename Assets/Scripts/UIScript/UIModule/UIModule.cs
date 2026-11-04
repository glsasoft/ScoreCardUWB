using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using LetsPiu.UI;
using LetsPiu.UIDefine;
using System;
using System.Collections.Generic;
using LetsPiu.Base.BaseScripts.Define;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using YooAsset;

namespace LetsPiu
{
    public partial class UIModule : SingletonBase<UIModule>
    {
        #region 数据定义

        public delegate bool PanelCloseFilter(UIID uiID);

        List<UIPanelActive> m_RunningActives = new List<UIPanelActive>();
        Dictionary<int, UIPanelActive> m_RunningActivesMap = new Dictionary<int, UIPanelActive>();

        List<UIPanelActive> m_CachedActives = new List<UIPanelActive>();

        // 顶层摄像机面板
        List<UIID> m_PanelsInTopCamera = new();

        // 刷新标志
        bool m_PanelSortingOrderDirty;

        bool[] m_PanelVisibleFlags = new bool[2048];
        bool[] m_PanelAlwaysVisibleFlags = new bool[2048];

        GameObject m_BlockUIClick;
        float m_BlockUIEndTime = -1;

        const int START_LAYER = 50;  // 预留使用层级排序
        const int INCR_LAYER_ORDER = 50;

        List<AssetHandle> m_SpriteAtlasHandles = new List<AssetHandle>();

        #endregion

        // 设置处于顶层摄像机的面板
        public void AddPanelsInTopCamera(UIID panelsInTopCamera)
        {
            if (m_PanelsInTopCamera.Contains(panelsInTopCamera))
            {
                return;
            }
            m_PanelsInTopCamera.Add(panelsInTopCamera);
            UpdateAllPanelRoot();
        }

        public void RemovePanelsInTopCamera(UIID panelsInTopCamera)
        {
            if (m_PanelsInTopCamera.Contains(panelsInTopCamera))
            {
                m_PanelsInTopCamera.Remove(panelsInTopCamera);
                UpdateAllPanelRoot();
            }
        }

        public bool IsOpened(UIID uiID)
        {
            return FindRunningPanel(uiID) != null;
        }

        /// <summary>
        /// 面板是否处于激活状态
        /// </summary>
        /// <param name="uiid"></param>
        /// <returns></returns>
        public bool IsPanelActive(UIID uiid)
        {
            var panel = FindPanel(uiid);
            return panel && panel.panelActive.isVisible;
        }

        public AbstractPanel FindPanel(UIID uiID)
        {
            UIPanelActive panel = FindRunningPanel(uiID);
            if (panel != null)
            {
                return panel.panel;
            }

            return null;
        }

        // 追溯查找master界面 
        public int FindMasterPanel(int panelID, bool IsRecursion = true)
        {
            UIPanelActive panelActive = FindRunningPanelByPanelID(panelID);

            if (panelActive == null)
            {
                return 0;
            }

            if (panelActive.masterPanel == 0)
            {
                return panelActive.instanceID;
            }

            UIPanelActive masterPanelActive = FindRunningPanelByPanelID(panelActive.masterPanel);

            if (masterPanelActive == null)
            {
                return panelActive.instanceID;
            }

            if (!IsRecursion)
            {
                return masterPanelActive.instanceID;
            }

            return FindMasterPanel(masterPanelActive.instanceID, IsRecursion);
        }

        public T FindPanel<T>() where T : AbstractPanel
        {
            for (int i = 0; i < m_RunningActives.Count; ++i)
            {
                UIPanelActive active = m_RunningActives[i];
                if (active.panel != null && active.panel is T)
                {
                    return active.panel as T;
                }
            }

            return null;
        }

        public AbstractPanel FindPanelByPanelID(int panelID)
        {
            UIPanelActive panel = FindRunningPanelByPanelID(panelID);
            if (panel != null)
            {
                return panel.panel;
            }

            return null;
        }

        /// <summary>按实例 ID 获取运行中的 <see cref="UIPanelActive"/>（比分牌叠层切屏用）。</summary>
        public UIPanelActive FindPanelActiveByPanelID(int panelID)
        {
            return FindRunningPanelByPanelID(panelID);
        }

        /// <summary>将面板置于较低排序层，供下层预加载；完成后调用 <see cref="ClearPanelSortIndexOverride"/> 并 <see cref="RequestResortPanels"/>。</summary>
        public void SetPanelSortIndexOverride(int panelInstanceId, int sortIndex)
        {
            var active = FindRunningPanelByPanelID(panelInstanceId);
            if (active == null)
            {
                Debug.LogWarning($"[UIModule] SetPanelSortIndexOverride: panel {panelInstanceId} not found");
                return;
            }

            active.SetSortIndexOverride(sortIndex);
            m_PanelSortingOrderDirty = true;
            CheckResortPanel();
        }

        /// <summary>取消面板排序覆盖。</summary>
        public void ClearPanelSortIndexOverride(int panelInstanceId)
        {
            var active = FindRunningPanelByPanelID(panelInstanceId);
            if (active == null)
            {
                return;
            }

            active.ClearSortIndexOverride();
            m_PanelSortingOrderDirty = true;
            CheckResortPanel();
        }

        /// <summary>强制刷新所有运行中面板的 Canvas 排序。</summary>
        public void RequestResortPanels()
        {
            m_PanelSortingOrderDirty = true;
            CheckResortPanel();
        }

        // 查找在列表中的最上层面板
        public UIID FindTopPanelInList(List<UIID> findList)
        {
            if (findList == null || findList.Count <= 0)
            {
                return UIID.NONE;
            }

            for (int i = m_RunningActives.Count - 1; i >= 0; --i)
            {
                UIID uid = m_RunningActives[i].id;

                for (int j = 0; j < findList.Count; ++j)
                {
                    if (findList[j] == uid)
                    {
                        return uid;
                    }
                }
            }

            return UIID.NONE;
        }

        public void GetPanelList(List<UIID> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();

            for (int i = m_RunningActives.Count - 1; i >= 0; --i)
            {
                result.Add(m_RunningActives[i].id);
            }
        }

        public T FindSubPage<T>(UIID parentUiId) where T : AbstractPage
        {
            UIPanelActive panel = FindRunningPanel(parentUiId);
            if (panel == null)
            {
                return null;
            }

            T subPage = panel.FindPage<T>();

            return subPage;
        }

        public void DestroyAllPanel(PanelCloseFilter filter = null)
        {
            for (int i = m_RunningActives.Count - 1; i >= 0; --i)
            {
                if (i >= m_RunningActives.Count)
                {
                    // 回调可能导致容器变化，需要2次判定
                    continue;
                }

                UIPanelActive panel = m_RunningActives[i];
                var masterPanel = FindPanelByPanelID(panel.masterPanel);
                if (filter != null && (filter(panel.id) || (masterPanel != null && filter(masterPanel.uiID))))
                {
                    continue;
                }

                panel.Close(true);

                m_RunningActivesMap.Remove(panel.instanceID);
                m_RunningActives.Remove(panel);
            }

            for (int i = m_CachedActives.Count - 1; i >= 0; --i)
            {
                if (i >= m_CachedActives.Count)
                {
                    continue;
                }

                UIPanelActive panel = m_CachedActives[i];
                if (filter != null && filter(panel.id))
                {
                    continue;
                }

                panel.Close(true);
                m_CachedActives.RemoveAt(i);
            }

            CheckCloseDependPanel();

            CheckResortPanel();
        }

        public void DestoryAllCachePanel()
        {
            for (int i = m_CachedActives.Count - 1; i >= 0; --i)
            {
                if (i >= m_CachedActives.Count)
                {
                    continue;
                }

                UIPanelActive panel = m_CachedActives[i];

                panel.Close(true);
                m_CachedActives.RemoveAt(i);
            }
        }

        // 设置所有面板的可见，未打开的面板也会生效
        public void SetAllPanelVisible(bool visible)
        {
            bool changed = false;

            for (int i = 0; i < m_PanelVisibleFlags.Length; ++i)
            {
                if (m_PanelVisibleFlags[i] != visible)
                {
                    changed = true;
                    m_PanelVisibleFlags[i] = visible;
                }
            }

            if (changed)
            {
                RefreshPanelVisibleAndMask();
            }
        }

        // 设置单个面板可见
        public void SetPanelVisible(UIID uiID, bool visible)
        {
            int index = (int)uiID;

            if (index < 0 || index >= m_PanelVisibleFlags.Length)
            {
                return;
            }

            if (m_PanelVisibleFlags[index] != visible)
            {
                m_PanelVisibleFlags[index] = visible;
                RefreshPanelVisibleAndMask();
            }
        }

        public void SetPanelAlwaysVisible(UIID uiID, bool visible)
        {
            int index = (int)uiID;

            if (index < 0 || index >= m_PanelAlwaysVisibleFlags.Length)
            {
                return;
            }

            if (m_PanelAlwaysVisibleFlags[index] != visible)
            {
                m_PanelAlwaysVisibleFlags[index] = visible;
                RefreshPanelVisibleAndMask();
            }
        }

        // 该面板是否可见
        public bool IsPanelVisible(UIID uiID)
        {
            int index = (int)uiID;

            if (index < 0 || index >= m_PanelVisibleFlags.Length)
            {
                return false;
            }

            if (m_PanelAlwaysVisibleFlags[index])
            {
                // 面板总是可见
                return true;
            }

            return m_PanelVisibleFlags[index];
        }

        // 获取面板的实际排列顺序
        public int GetSiblingIndex(int panelID)
        {
            for (int i = 0; i < m_RunningActives.Count; ++i)
            {
                if (m_RunningActives[i].instanceID == panelID)
                {
                    return i;
                }
            }

            return -1;
        }

        // 设置面板的实际排列顺序，该操作会破坏面板的自动排序，请慎重使用
        public bool SetSiblingIndex(int panelID, int index)
        {
            int i = 0;

            for (; i < m_RunningActives.Count; ++i)
            {
                if (m_RunningActives[i].instanceID == panelID)
                {
                    break;
                }
            }

            if (i >= m_RunningActives.Count)
            {
                return false;
            }

            UIPanelActive panel = m_RunningActives[i];
            m_RunningActives.RemoveAt(i);

            if (index <= 0)
            {
                m_RunningActives.Insert(0, panel);
            }
            else if (index >= m_RunningActives.Count)
            {
                m_RunningActives.Add(panel);
            }
            else
            {
                m_RunningActives.Insert(index, panel);
            }

            m_PanelSortingOrderDirty = true;

            return true;
        }

        // 重新激活面板，会使得面板在同样索引的最上层
        public void ReactivePanel(int panelID)
        {
            AbstractPanel panel = FindPanelByPanelID(panelID);
            if (panel != null)
            {
                m_PanelSortingOrderDirty = true;
                AddRunningPanel(panel.panelActive);
            }
        }

        // 面板的Canvas在外部发生改变，需要主动调用
        public void SetCanvasChanged(int panelID)
        {
            AbstractPanel panel = FindPanelByPanelID(panelID);
            if (panel != null && panel.panelActive != null)
            {
                m_PanelSortingOrderDirty = true;
                panel.panelActive.RefreshCanvas();
            }
        }

        public void StartBlockUIClick(float seconds)
        {
            if (UIRoot.S.rootCanvas == null)
            {
                return;
            }

            if (m_BlockUIClick == null)
            {
                m_BlockUIClick = new GameObject("BlockUIClick");
                m_BlockUIClick.transform.SetParent(UIRoot.S.rootCanvas.transform, false);

                var rectNode = m_BlockUIClick.AddComponent<RectTransform>();
                rectNode.pivot = new Vector2(0.5f, 0.5f);
                rectNode.anchorMin = Vector2.zero;
                rectNode.anchorMax = Vector2.one;
                rectNode.offsetMin = Vector2.zero;
                rectNode.offsetMax = Vector2.zero;

                var canvas = m_BlockUIClick.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = short.MaxValue;

                m_BlockUIClick.AddComponent<GraphicRaycaster>();
                m_BlockUIClick.AddComponent<RaycastArea>();
            }

            m_BlockUIClick.SetActive(true);
            m_BlockUIEndTime = Time.time + seconds;
        }

        public void FinishBlockUIClick()
        {
            if (m_BlockUIClick == null)
            {
                return;
            }

            m_BlockUIEndTime = -1;
            m_BlockUIClick.SetActive(false);
        }

        public UIModule()
        {
            ResetVisible();

            UIDataTable.InitUIDataTable();

            OnLateUpdate().Forget();

            EventManager.Instance.AddEvent<int>(EventMsg.OnPanelObjectChanged, OnPanelObjectChanged);
            EventManager.Instance.AddEvent(EventMsg.LoggedOut, LoggedOut);
            EventManager.Instance.AddEvent(EventMsg.LoginSuccess, LoginSuccess);
            EventManager.Instance.AddEvent(EventMsg.RequestLoginOut, RequestLoginOut);

            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }

        void LoginSuccess()
        {
        }

        void LoggedOut()
        {
        }

        void RequestAtlas(string atlasName, Action<SpriteAtlas> callback)
        {
            Debug.Log("[UIModule] RequestAtlas: " + atlasName);
            var package = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            var loadHandle = package.LoadAssetSync<SpriteAtlas>(atlasName);
            callback.Invoke(loadHandle.AssetObject as SpriteAtlas);
            m_SpriteAtlasHandles.Add(loadHandle);
        }

        public async UniTask ReleaseSpriteAtlas()
        {
            //var package = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
            //foreach (var handle in m_SpriteAtlasHandles)
            //{
            //    handle.Dispose();
            //    package.TryUnloadUnusedAsset(handle.GetAssetInfo().Address);
            //}
            //m_SpriteAtlasHandles.Clear();
            //await Resources.UnloadUnusedAssets().ToUniTask();
            await UniTask.NextFrame();
        }

        // 帧执行
        async UniTask OnLateUpdate()
        {
            //绑定到Update中去执行
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPreLateUpdate))
            {
                CheckResortPanel();

                UpdateActive();
                UpdateBlockUIVisible();
            }
        }

        public void ResetAllPanel(PanelCloseFilter filter = null)
        {
            DestroyAllPanel(filter);
            ResetVisible();

            UpdateAllPanelRoot();
        }

        void OnPanelObjectChanged(int id)
        {
            m_PanelSortingOrderDirty = true;
        }

        void InnerClosePanel(UIPanelActive panel, bool checkResort = true)
        {
            if (panel == null)
            {
                return;
            }

            Debug.LogFormat("[UIModule:InnerClosePanel]close {0}", panel.id);

            // 检测失效的面板
            CheckInvalid();

            bool needCached = panel.data.cacheCount > 0;

            if (m_RunningActivesMap.ContainsKey(panel.instanceID))
            {
                m_RunningActivesMap.Remove(panel.instanceID);
                m_RunningActives.Remove(panel);
            }

            if (needCached)
            {
                m_CachedActives.Add(panel);
            }

            panel.Close(!needCached);

            if (checkResort)
            {
                CheckResortPanel();
            }
        }

        void UpdateAllPanelRoot()
        {
            for (int i = 0; i < m_RunningActives.Count; ++i)
            {
                UIPanelActive active = m_RunningActives[i];
                Transform root = GetPanelRoot(active.id);
                if (active.root != root)
                {
                    active.root = root;

                    // 切换根节点需要重新激活，不同UI相机的Canvas不会刷新
                    if (active.panel != null)
                    {
                        active.panel.gameObject.SetActive(false);
                        active.panel.gameObject.SetActive(true);
                    }
                }
            }

            for (int i = 0; i < m_CachedActives.Count; ++i)
            {
                m_CachedActives[i].root = GetPanelRoot(m_CachedActives[i].id);
            }
        }

        UIPanelActive GetCachedPanel(UIID id)
        {
            for (int i = 0; i < m_CachedActives.Count; ++i)
            {
                if (m_CachedActives[i].id == id)
                {
                    UIPanelActive panel = m_CachedActives[i];
                    panel.isVisible = true;
                    m_CachedActives.RemoveAt(i);
                    return panel;
                }
            }

            return null;
        }

        void OnPanelChanged(UIPanelActive panel)
        {
            m_PanelSortingOrderDirty = true;

            // 面板排列索引改变，重新加入列表
            if (panel.sortIndex != 0 && m_RunningActivesMap.ContainsKey(panel.instanceID))
            {
                AddRunningPanel(panel);
            }
        }

        void AddRunningPanel(UIPanelActive panel)
        {
            // 已经存在，先移除
            if (m_RunningActivesMap.ContainsKey(panel.instanceID))
            {
                m_RunningActives.Remove(panel);
            }
            else
            {
                m_RunningActivesMap.Add(panel.instanceID, panel);
            }

            int insertIndex = 0;
            int sortIndex = GetPanelSortIndex(panel);

            // 从顶层依次遍历，找到该面板合适的位置
            for (int i = m_RunningActives.Count - 1; i >= 0; --i)
            {
                int topSortIndex = GetPanelSortIndex(m_RunningActives[i]);

                if (sortIndex >= topSortIndex)
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            // 插入面板
            if (insertIndex < m_RunningActives.Count)
            {
                m_RunningActives.Insert(insertIndex, panel);
            }
            else
            {
                m_RunningActives.Add(panel);
            }
        }

        void CheckResortPanel()
        {
            if (!m_PanelSortingOrderDirty)
            {
                return;
            }

            m_PanelSortingOrderDirty = false;

            int siblingIndex = 0;
            int layerIndex = START_LAYER;

            for (int i = 0; i < m_RunningActives.Count; ++i)
            {
                UIPanelActive panel = m_RunningActives[i];

                if (panel.panel != null)
                {
                    panel.panel.transform.SetSiblingIndex(siblingIndex++);
                }

                Canvas[] panelCanvas = m_RunningActives[i].allCanvas;
                for (int j = 0; j < panelCanvas.Length; ++j)
                {
                    Canvas canvas = panelCanvas[j];
                    if (canvas != null)
                    {
                        canvas.overrideSorting = true;
                        canvas.pixelPerfect = false;
                        if (panel.root == UIRoot.S.topPanelRoot)
                            canvas.sortingOrder = layerIndex + 1000;
                        else
                            canvas.sortingOrder = layerIndex;
                        //canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
                        layerIndex += INCR_LAYER_ORDER;
                    }
                }
            }

            RefreshPanelVisibleAndMask();
            EventManager.Instance.Broadcast(EventMsg.OnPageCanvasOrderChanged);
        }

        void CheckCloseDependPanel()
        {
            bool needCheck = true;
            bool hideMaskPanelClosed = false;

            while (needCheck)
            {
                needCheck = false;

                for (int i = m_RunningActives.Count; i >= 0; --i)
                {
                    if (i >= m_RunningActives.Count)
                    {
                        // 回调可能导致容器变化，需要2次判定
                        continue;
                    }

                    UIPanelActive panel = m_RunningActives[i];
                    PanelHideMask hideMask = GetPanelHideMask(panel);

                    if (panel.masterPanel > 0 && !m_RunningActivesMap.ContainsKey(panel.masterPanel))
                    {
                        InnerClosePanel(panel);

                        // 可能存在多层依赖，需要再次检测
                        needCheck = true;

                        if (hideMask == PanelHideMask.Hide)
                        {
                            hideMaskPanelClosed = true;
                        }

                        break;
                    }

                    panel = null;
                }
            }

            if (hideMaskPanelClosed)
            {
                ActiveGC(PanelHideMask.Hide);
            }
        }

        void RefreshPanelVisibleAndMask()
        {
            int mask = 0;
            for (int i = m_RunningActives.Count - 1; i >= 0; --i)
            {
                UIPanelActive panel = m_RunningActives[i];
                if (panel.panel == null)
                {
                    continue;
                }

                bool visible = true;
                bool interactive = true;

                if (!IsPanelVisible(panel.id))
                {
                    visible = false;
                }

                if ((mask & (int)PanelHideMask.Hide) != 0)
                {
                    visible = false;
                }

                if ((mask & (int)PanelHideMask.UnInteractive) != 0)
                {
                    interactive = false;
                }

                // 顶层摄像机的面板强制可见，可交互
                if (IsPanelInTopCamera(panel.id))
                {
                    visible = true;
                    interactive = true;
                }

                panel.isVisible = visible;
                panel.isInteractive = interactive;

                mask |= (int)panel.panel.hideMask;
            }
        }

        int GetPanelSortIndex(UIPanelActive panel)
        {
            return panel.sortIndex != 0 ? panel.sortIndex : 0;
        }

        void UpdateActive()
        {
            for (int i = 0; i < m_RunningActives.Count; ++i)
            {
                if (i >= m_RunningActives.Count)
                {
                    continue;
                }

                m_RunningActives[i].UpdateActive();
            }
        }

        void CheckInvalid()
        {
            for (int i = m_RunningActives.Count - 1; i >= 0; --i)
            {
                if (i >= m_RunningActives.Count)
                {
                    // 回调可能导致容器变化，需要2次判定
                    continue;
                }

                UIPanelActive panel = m_RunningActives[i];

                if (panel.allCanvas.Length > 0 && panel.panel == null)
                {
                    panel.Close(true);

                    m_RunningActivesMap.Remove(panel.instanceID);
                    m_RunningActives.RemoveAt(i);

                    Debug.LogError("[UIModule:CheckInvalid]panel destroyed unmanaged, try to repair it " + panel.id);
                }
            }

            for (int i = m_CachedActives.Count - 1; i >= 0; --i)
            {
                if (i >= m_CachedActives.Count)
                {
                    // 回调可能导致容器变化，需要2次判定
                    continue;
                }

                UIPanelActive panel = m_CachedActives[i];

                if (panel.allCanvas.Length > 0 && panel.panel == null)
                {
                    panel.Close(true);

                    m_CachedActives.RemoveAt(i);

                    Debug.LogError("[UIModule:CheckInvalid]panel destroyed unmanaged, try to repair it " + panel.id);
                }
            }
        }

        // 根据UID 获取运行界面
        UIPanelActive FindRunningPanel(UIID id)
        {
            for (int i = 0; i < m_RunningActives.Count; ++i)
            {
                if (m_RunningActives[i].id == id)
                {
                    return m_RunningActives[i];
                }
            }

            return null;
        }

        // 根据PanelID(实例id)获取运行界面
        UIPanelActive FindRunningPanelByPanelID(int panelID)
        {
            if (m_RunningActivesMap.ContainsKey(panelID))
            {
                return m_RunningActivesMap[panelID];
            }

            return null;
        }

        Transform GetPanelRoot(UIID id)
        {
            if (IsPanelInTopCamera(id))
            {
                return UIRoot.S.topPanelRoot;
            }
            else
            {
                return UIRoot.S.panelRoot;
            }
        }

        bool IsPanelInTopCamera(UIID id)
        {
            if (m_PanelsInTopCamera == null)
            {
                return false;
            }

            for (int i = 0; i < m_PanelsInTopCamera.Count; ++i)
            {
                if (m_PanelsInTopCamera[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        void ResetVisible()
        {
            for (int i = 0; i < m_PanelVisibleFlags.Length; ++i)
            {
                m_PanelVisibleFlags[i] = true;
            }
        }

        void ActiveGC(PanelHideMask hideMask)
        {
            if (hideMask == PanelHideMask.Hide)
            {
                for (int i = 0; i < 1; ++i)
                {
                    GC.Collect();
                }
            }
        }

        PanelHideMask GetPanelHideMask(UIPanelActive panel)
        {
            if (panel == null || panel.panel == null)
            {
                return PanelHideMask.None;
            }

            return panel.panel.hideMask;
        }

        void UpdateBlockUIVisible()
        {
            if (m_BlockUIEndTime < 0 || Time.time < m_BlockUIEndTime)
            {
                return;
            }

            FinishBlockUIClick();
        }
    }

}
