using Cysharp.Threading.Tasks;
using LetsPiu.UIDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using LetsPiu.Base.BaseScripts.Define;
using UnityEngine;
using UnityEngine.Events;
using YooAsset;

namespace LetsPiu
{
    public class UIPanelActive
    {
        #region 数据定义

        public class PanelChangeEvent : UnityEvent<UIPanelActive> { }

        // 页面信息
        class PageInfo
        {
            public UIID id;
            public int instanceID;
            public UIData data;

            public AbstractPage page;

            public ResLoader loader;

            // 打开相关参数
            public Transform root;
            public bool isOpened;
            public object[] openArgs;
            public Action<AbstractPage> onOpened;

            // 是否唯一
            public bool isSingleton = true;

            // 是否在缓存状态，该状态面板已关闭，但是没有被销毁
            public bool isCached;

            // 是否内部页面，内部页面是面板自带的，关闭时不会销毁
            public bool isInner;

            public PageInfo(UIID id)
            {
                this.id = id;
                this.instanceID = ++m_SerialID;
                this.data = UIDataTable.Get(id);

                if (this.data != null)
                {
                    this.isSingleton = this.data.isSingleton;
                }
            }
        }

        static int m_SerialID;

        // 面板数据
        AbstractPanel m_Panel;
        PageInfo m_MainPage;

        // 面板打开回调
        Action<AbstractPanel> m_OnPanelOpened;

        // 是否可见
        bool m_IsVisible = true;
        // 是否可以交互
        bool m_IsInteractive = true;

        // 页面列表
        List<PageInfo> m_PageList = new List<PageInfo>();
        List<AssetHandle> m_HandleList = new List<AssetHandle>();
        // 面板改变事件
        PanelChangeEvent m_OnPanelChanged = new PanelChangeEvent();

        // 面板原始位置，面板显示时的位置
        // 隐藏面板会将面板位置移出，显示时恢复该位置
        Vector3 m_OriginalPos;

        // 所有的Canvas
        Canvas[] m_AllCanvas = new Canvas[0];

        // 所有的Renderer（含Canvas之外的特效Renderer），随RefreshCanvas同步刷新，避免逐帧扫描
        Renderer[] m_AllRenderers = new Renderer[0];

        // 正在打开的页面列表
        List<PageInfo> m_OpenningList = new List<PageInfo>();
        bool m_IsOpenning;

        // int.MinValue 表示未覆盖，沿用 Panel.sortIndex
        int m_SortIndexOverride = int.MinValue;

        #endregion

        // 面板的UIID
        public UIID id
        {
            get { return m_MainPage.id; }
        }

        // 实体唯一ID
        public int instanceID
        {
            get { return m_MainPage.instanceID; }
        }

        // 面板对象
        public AbstractPanel panel
        {
            get { return m_Panel; }
        }

        // 面板的配置数据
        public UIData data
        {
            get { return m_MainPage.data; }
        }

        // 面板根节点
        public Transform root
        {
            get { return m_MainPage.root; }
            set
            {
                m_MainPage.root = value;

                if (m_Panel != null)
                {
                    RefreshParent(m_MainPage);
                }
            }
        }

        // 是否可见
        public bool isVisible
        {
            get { return m_IsVisible; }
            set
            {
                if (m_IsVisible != value)
                {
                    // 记录原始位置
                    if (m_IsVisible)
                    {
                        if (m_Panel.transform is RectTransform)
                        {
                            m_OriginalPos = (m_Panel.transform as RectTransform).anchoredPosition;
                        }
                        else
                        {
                            m_OriginalPos = m_Panel.transform.localPosition;
                        }
                    }

                    m_IsVisible = value;
                    RefreshPanelVisible();
                }
            }
        }

        // 是否可交互
        public bool isInteractive
        {
            get { return m_IsInteractive; }
            set
            {
                if (m_IsInteractive != value)
                {
                    m_IsInteractive = value;
                    RefreshPanelInteractive();
                }
            }
        }

        // 是否已经打开
        public bool hasOpened
        {
            get { return m_MainPage.isOpened; }
        }

        // 宿主面板
        public int masterPanel { get; set; }

        // 面板排列层级
        public PanelSortLayer sortLayer { get; set; }

        // 面板排列索引（可被 <see cref="SetSortIndexOverride"/> 临时覆盖，用于叠层预加载）
        public int sortIndex
        {
            get
            {
                if (m_SortIndexOverride != int.MinValue)
                {
                    return m_SortIndexOverride;
                }

                if (m_Panel != null)
                {
                    return m_Panel.sortIndex;
                }

                return 0;
            }
        }

        /// <summary>是否有子 Page 正在 OpenPageList 队列中处理。</summary>
        public bool IsOpeningPages => m_IsOpenning;

        /// <summary>临时覆盖排序索引（低于当前屏），叠层预加载完成后应 <see cref="ClearSortIndexOverride"/>。</summary>
        public void SetSortIndexOverride(int sortIndex)
        {
            m_SortIndexOverride = sortIndex;
            m_OnPanelChanged.Invoke(this);
        }

        /// <summary>取消排序覆盖，恢复为 Panel 自带 sortIndex。</summary>
        public void ClearSortIndexOverride()
        {
            if (m_SortIndexOverride == int.MinValue)
            {
                return;
            }

            m_SortIndexOverride = int.MinValue;
            m_OnPanelChanged.Invoke(this);
        }

        // 所有的Canvas
        public Canvas[] allCanvas
        {
            get { return m_AllCanvas; }
        }

        // 所有的Renderer
        public Renderer[] allRenderers
        {
            get { return m_AllRenderers; }
        }

        // 改变监听器
        public PanelChangeEvent onPanelChanged
        {
            get { return m_OnPanelChanged; }
        }

        public UIPanelActive(UIID id)
        {
            m_MainPage = new PageInfo(id);
            m_MainPage.isInner = true;
            m_PageList.Add(m_MainPage);
        }

        // 打开面板
        public bool Open(Transform root, Action<AbstractPanel> onOpened, object[] args)
        {
            m_MainPage.root = root;
            m_MainPage.openArgs = args;

            m_OnPanelOpened = onOpened;

            if (m_MainPage.root == null)
            {
                Debug.LogWarning("[UIPanelActive:Open]root is null.");
                return false;
            }

            if (m_MainPage.data == null)
            {
                Debug.LogWarning("[UIPanelActive:Open]can't find ui data " + m_MainPage.id);
                return false;
            }

            if (m_MainPage.loader == null || !m_MainPage.loader.isFinish)
            {
                // 开始加载面板
                LoadPage(m_MainPage, () =>
                {
                    CreatePanel();
                });
            }
            else
            {
                if (m_Panel == null)
                {
                    // 创建面板
                    CreatePanel();
                }
                else
                {
                    // 打开面板
                    OpenPanel();
                }
            }

            return true;
        }

        // 关闭面板，destroy为true时，会销毁面板，否则面板会缓存起来
        public bool Close(bool destroy)
        {
            // 关闭所有页面
            for (int i = m_PageList.Count - 1; i >= 0; --i)
            {
                if (i >= m_PageList.Count)
                {
                    // 回调会导致容器变化，需要2次判定
                    continue;
                }

                PageInfo pageInfo = m_PageList[i];

                // 关闭界面
                ClosePage(pageInfo);

                if (destroy)
                {
                    // 销毁回调
                    if (pageInfo.page != null)
                    {
                        ERunner.Run(pageInfo.page.OnDispose);
                    }

                    // 清理资源
                    if (pageInfo.loader != null)
                    {
                        pageInfo.loader.ReleaseAll();
                        pageInfo.loader = null;
                    }
                }
            }

            // 销毁并清理数据
            if (destroy)
            {
                if (m_Panel != null)
                {
                    GameObject.Destroy(m_Panel.gameObject);
                    m_Panel = null;
                }

                m_PageList.Clear();

                for (int i = 0; i < m_HandleList.Count; i++)
                {
                    m_HandleList[i].Dispose();
                }
                m_HandleList.Clear();
            }
            else
            {
                m_MainPage.isCached = true;

                if (m_Panel != null)
                {
                    m_Panel.gameObject.SetActive(false);
                }
            }

            // 面板改变事件
            m_OnPanelChanged.Invoke(this);

            EventManager.Instance.Broadcast(EventMsg.OnPanelClose, this.id, this.instanceID);

            return true;
        }

        // 挂载新的页面
        public int AttachPage(UIID pageUIID, Transform root, Action<AbstractPage> onOpened, bool isSingleton, object[] args)
        {
            UIData data = UIDataTable.Get(pageUIID);
            if (data == null)
            {
                Debug.LogWarning("[UIPanelActive:AttachPage]can't find ui data in table " + pageUIID);
                return 0;
            }

            PageInfo pageInfo = FindPageForAttach(pageUIID, isSingleton);
            if (pageInfo == null)
            {
                pageInfo = new PageInfo(pageUIID);
                m_PageList.Add(pageInfo);
            }

            pageInfo.isCached = false;
            pageInfo.isSingleton = isSingleton;
            pageInfo.root = root;
            pageInfo.openArgs = args;
            pageInfo.onOpened = onOpened;

            // 面板已经存在，重新打开
            if (pageInfo.page != null)
            {
                pageInfo.page.gameObject.SetActive(true);

                // 如果当前正在打开，从列表中移除，防止二次打开
                if (m_IsOpenning)
                {
                    m_OpenningList.Remove(pageInfo);
                }

                OpenPage(pageInfo);

                return pageInfo.instanceID;
            }

            // 加载资源，创建新的页面
            LoadPage(pageInfo, () =>
            {
                CreatePage(pageInfo);
            });

            return pageInfo.instanceID;
        }

        // 卸载页面
        public bool DettachPage(UIID pageUIID)
        {
            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if (m_PageList[i].id == pageUIID)
                {
                    DettachPageWithInstanceID(m_PageList[i].instanceID);
                }
            }

            return true;
        }

        // 卸载页面
        public bool DettachPageWithInstanceID(int pageID)
        {
            PageInfo pageInfo = FindPage(pageID);
            if (pageInfo == null)
            {
                return false;
            }

            pageInfo.isCached = true;
            if (pageInfo.page != null)
            {
                pageInfo.page.gameObject.SetActive(false);
            }

            ClosePage(pageInfo);

            // 面板改变事件
            m_OnPanelChanged.Invoke(this);

            return true;
        }

        // 页面是否已经挂载
        public bool IsPageAttached(UIID pageUUID)
        {
            PageInfo pageInfo = FindPage(pageUUID);
            if (pageInfo != null && !pageInfo.isCached)
            {
                return true;
            }

            return false;
        }

        // 页面是否已经打开
        public bool IsPageOpened(UIID pageUUID)
        {
            PageInfo pageInfo = FindPage(pageUUID);
            if (pageInfo != null && pageInfo.isOpened)
            {
                return true;
            }

            return false;
        }

        // 页面是否已经打开
        public bool IsPageOpened(int instanceID)
        {
            PageInfo pageInfo = FindPage(instanceID);
            if (pageInfo != null && pageInfo.isOpened)
            {
                return true;
            }

            return false;
        }

        // 获取页面列表
        public void GetPageList(List<AbstractPage> pageList)
        {
            if (pageList == null)
            {
                return;
            }

            pageList.Clear();

            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if (!m_PageList[i].isCached && m_PageList[i].page != null)
                {
                    pageList.Add(m_PageList[i].page);
                }
            }
        }

        public T FindPage<T>(bool includeCached = false) where T : AbstractPage
        {
            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if ((includeCached || !m_PageList[i].isCached) &&
                    m_PageList[i].page != null &&
                    m_PageList[i].page is T)
                {
                    return m_PageList[i].page as T;
                }
            }

            return null;
        }

        // 刷新页面的活跃状态，用来保持和外部调用SetActive后的同步
        public void UpdateActive()
        {
            float time = Time.time;

            for (int i = m_PageList.Count - 1; i >= 0; --i)
            {
                if (i >= m_PageList.Count)
                {
                    // 回调可能导致容器变化，需要2次判定
                    continue;
                }

                PageInfo pageInfo = m_PageList[i];
                if (pageInfo.isCached)
                {
                    continue;
                }

                if (pageInfo.page == null)
                {
                    continue;
                }

                bool active = pageInfo.page.gameObject.activeInHierarchy;
                if (pageInfo.isOpened != active)
                {
                    // 打开状态和活跃状态保持一致
                    if (active)
                    {
                        OpenPage(pageInfo);
                    }
                    else
                    {
                        if (m_MainPage == pageInfo)
                        {
                            pageInfo.page.CloseSelf();
                        }
                        else
                        {
                            ClosePage(pageInfo);
                        }
                    }
                }

                // 刷新定时器
                if (pageInfo.isOpened && pageInfo.page != null)
                {
                    pageInfo.page.UpdateTimer(time);
                }
            }
        }

        public void RefreshCanvas()
        {
            if (m_Panel != null)
            {
                m_AllCanvas = m_Panel.GetComponentsInChildren<Canvas>(true);

                List<Canvas> list = new List<Canvas>();
                foreach (Canvas c in m_AllCanvas)
                { 
                    if(c.GetComponent<AbstractPage>())
                        list.Add(c);
                }
                m_AllCanvas = list.ToArray();

                m_AllRenderers = m_Panel.GetComponentsInChildren<Renderer>(true);
            }
        }

        //收集预加载资源
        void PrepareResource(PageInfo pageInfo)
        {
            if (pageInfo.data.panelClassType == null)
            {
                return;
            }

            var methodInfo = pageInfo.data.panelClassType.GetMethod("OnPrepareResource", System.Reflection.BindingFlags.Static |
                            System.Reflection.BindingFlags.Public);
            if (methodInfo == null)
            {
                return;
            }

            object result = methodInfo.Invoke(null, null);
            if (result == null)
            {
                return;
            }

            if (result is List<UIPrepareResData> resList)
            {
                for (int i = 0; i < resList.Count; i++)
                {
                    pageInfo.loader.AddRes(resList[i].address, resList[i].type);
                }

            }
            else if (result is UIPrepareResData ret)
            {
                pageInfo.loader.AddRes(ret.address, ret.type);
            }
        }

        void CreatePanel()
        {
            if (m_Panel != null)
            {
                Debug.LogWarning("[UIPanelActive:CreatePanel]panel is already created " + m_MainPage.data.name);
                return;
            }

            List<PageInfo> allPages = CreateAllPages(m_MainPage);
            if (allPages == null)
            {
                return;
            }

            // 记录原始位置
            if (m_Panel.transform is RectTransform)
            {
                m_OriginalPos = (m_Panel.transform as RectTransform).anchoredPosition;
            }
            else
            {
                m_OriginalPos = m_Panel.transform.localPosition;
            }

            // 刷新Canvas
            RefreshCanvas();

            // 打开面板
            OpenPanel();
        }

        void OpenPanel()
        {
            if (m_Panel == null)
            {
                Debug.LogWarning("[UIPanelActive:OpenPanel]panel object is null.");
                return;
            }

            // 强制面板可见
            m_MainPage.isCached = false;
            m_Panel.gameObject.SetActive(true);

            // 打开页面列表
            // 该接口可能产生递归调用，需要注意
            OpenPageList(m_PageList);

            RefreshPanelVisible();
            RefreshPanelInteractive();

            // 执行打开回调
            if (m_OnPanelOpened != null)
            {
                ERunner.Run(m_OnPanelOpened, m_Panel);
            }

            // 面板改变事件
            m_OnPanelChanged.Invoke(this);
            EventManager.Instance.Broadcast(EventMsg.OnPanelOpen, this.id, this.instanceID);
        }

        void LoadPage(PageInfo pageInfo, Action onFinish)
        {
            if (pageInfo == null)
            {
                return;
            }

            if (pageInfo.loader != null)
            {
                // 已经开始加载
                pageInfo.loader.StartLoad(() =>
                {
                    if (!pageInfo.isCached)
                    {
                        onFinish();
                    }
                }).Forget();
                return;
            }

            pageInfo.loader = new ResLoader();

            // 准备动态资源
            PrepareResource(pageInfo);
            // 准备面板资源
            pageInfo.loader.AddRes(pageInfo.data.name, ResType.Prefab);

            // 开始加载
            pageInfo.loader.StartLoad(() =>
            {
                if (!pageInfo.isCached)
                {
                    onFinish();
                }
            }).Forget();
        }

        void CreatePage(PageInfo pageInfo)
        {
            List<PageInfo> allPages = CreateAllPages(pageInfo);
            if (allPages == null)
            {
                return;
            }

            // 创建回调
            // 该接口可能产生递归调用，需要注意
            OpenPageList(allPages);

            // 刷新Canvas
            RefreshCanvas();

            // 面板改变事件
            m_OnPanelChanged.Invoke(this);
        }

        List<PageInfo> CreateAllPages(PageInfo pageInfo)
        {
            if (pageInfo.page != null)
            {
                Debug.LogWarning("[UIPanelActive:CreateAllSubPages]page has already created " + pageInfo.id);
                return null;
            }

            var handle = pageInfo.loader.GetAsset(pageInfo.data.name);
            m_HandleList.Add(handle);
            GameObject prefab = handle.AssetObject as GameObject;
            handle.Dispose();
            if (prefab == null)
            {
                Debug.LogWarning("[UIPanelActive:CreateAllSubPages]can't find resource " + pageInfo.data.name);
                return null;
            }

            GameObject clone = GameObject.Instantiate(prefab, pageInfo.root, false);

            pageInfo.page = clone.GetComponent<AbstractPage>();
            if (pageInfo.page == null)
            {
                Debug.LogWarning("[UIPanelActive:CreateAllSubPages]can't find abstract page component " + pageInfo.data.name);
                return null;
            }

            // 初始化层级和Raycaster状态
            UITools.SetGameObjectLayer(pageInfo.page.gameObject, pageInfo.root.gameObject.layer, true);
            UITools.SetGraphicRaycasterState(pageInfo.page.gameObject, m_IsInteractive);

            if (m_MainPage == pageInfo)
            {
                m_Panel = m_MainPage.page as AbstractPanel;

                if (m_Panel == null)
                {
                    Debug.LogWarning("[UIPanelActive:CreateAllSubPages]can't find abstract panel component " + pageInfo.data.name);
                    return null;
                }
            }

            pageInfo.page.panelActive = this;
            pageInfo.page.pageUIID = pageInfo.id;
            pageInfo.page.pageID = pageInfo.instanceID;
            pageInfo.page.gameObject.SetActive(true);

            List<PageInfo> subPageList = new()
            {
                pageInfo
            };

            // 获取所有的挂载页面
            AbstractPage[] pageList = pageInfo.page.GetComponentsInChildren<AbstractPage>(true);
            for (int i = 0; i < pageList.Length; ++i)
            {
                AbstractPage page = pageList[i];
                if (page == pageInfo.page)
                {
                    // 跳过面板本身
                    continue;
                }

                UIData data = GetInnerPageUIData(pageInfo.page.name, page.name);
                if (data == null)
                {
                    continue;
                }

                PageInfo subPageInfo = new PageInfo(data.uiID);
                subPageInfo.page = page;
                subPageInfo.root = page.transform.parent;
                subPageInfo.isInner = true;

                page.panelActive = this;
                page.pageUIID = subPageInfo.id;
                page.pageID = subPageInfo.instanceID;

                subPageList.Add(subPageInfo);
                m_PageList.Add(subPageInfo);
            }

            // 创建回调
            for (int i = 0; i < subPageList.Count; ++i)
            {
                ERunner.Run(subPageList[i].page.OnCreate);
            }

            return subPageList;
        }

        // 该接口可能产生递归调用，需要注意
        void OpenPageList(List<PageInfo> pageList)
        {
            // 可能产生递归调用，加入列表即可
            if (m_IsOpenning)
            {
                // 如果当前正在打开，加入打开列表即可
                for (int i = 0; i < pageList.Count; ++i)
                {
                    if (!m_OpenningList.Contains(pageList[i]))
                    {
                        m_OpenningList.Add(pageList[i]);
                    }
                }

                return;
            }

            m_IsOpenning = true;
            m_OpenningList.Clear();
            m_OpenningList.AddRange(pageList);

            while (m_OpenningList.Count > 0)
            {
                PageInfo pageInfo = m_OpenningList[0];
                m_OpenningList.RemoveAt(0);

                OpenPage(pageInfo, false);
            }

            m_IsOpenning = false;
        }

        void OpenPage(PageInfo pageInfo, bool noticeChanged = true)
        {
            if (pageInfo.page == null)
            {
                Debug.LogWarning("[UIPanelActive:OpenPage]page is null " + pageInfo.id);
                return;
            }

            if (pageInfo.isCached)
            {
                // 面板正在缓存，无法打开
                return;
            }

            RefreshParent(pageInfo);

            if (!pageInfo.page.gameObject.activeInHierarchy)
            {
                // 面板不活跃状态，无法打开
                return;
            }

            pageInfo.page.gameObject.SetActive(true);
            pageInfo.isOpened = true;

            if (pageInfo.page is AbstractPanel)
            {
                (pageInfo.page as AbstractPanel).PlayOpenSound();
            }

            ERunner.Run(pageInfo.page.OnOpen, pageInfo.openArgs);

            if (pageInfo.onOpened != null)
            {
                Action<AbstractPage> onOpened = pageInfo.onOpened;
                pageInfo.onOpened = null;
                ERunner.Run(onOpened, pageInfo.page);
            }

            // 面板改变事件
            if (noticeChanged)
            {
                m_OnPanelChanged.Invoke(this);
            }

            EventManager.Instance.Broadcast(EventMsg.OnOpenPage, (int)pageInfo.id, pageInfo.instanceID);
        }

        void ClosePage(PageInfo pageInfo)
        {
            if (pageInfo.page != null)
            {
                if (pageInfo.isOpened)
                {
                    //清理
                    pageInfo.page.ClearTimer();
                    if (pageInfo.page is AbstractPanel)
                    {
                        (pageInfo.page as AbstractPanel).PlayCloseSound();
                    }
                    pageInfo.isOpened = false;
                    ERunner.Run(pageInfo.page.OnClose);
                }
            }
        }

        void RefreshPanelVisible()
        {
            if (m_Panel == null)
            {
                return;
            }

            int layer = m_IsVisible ? Layer.UI : Layer.HIDE_UI;
            UITools.SetGameObjectLayer(m_Panel.gameObject, layer, true);
            Vector3 localPos = m_IsVisible ? m_OriginalPos : Constants.INVISIBLE_POS;

            if (m_Panel.transform is RectTransform)
            {
                (m_Panel.transform as RectTransform).anchoredPosition = localPos;
            }
            else
            {
                m_Panel.transform.localPosition = localPos;
            }

            NotfiVisibleChangedEvent();
        }

        /// <summary>
        /// 通知面板可见度改变事件
        /// </summary>
        void NotfiVisibleChangedEvent()
        {
            for (int i = 0; i < m_PageList.Count; i++)
            {
                var data = m_PageList[i];

                if (data != null && data.page != null)
                {
                    data.page.OnVisibleChanged(m_IsVisible);
                }
            }
        }

        void RefreshPanelInteractive()
        {
            if (m_Panel == null)
            {
                return;
            }

            UITools.SetGraphicRaycasterState(m_Panel.gameObject, m_IsInteractive);
        }

        PageInfo FindPage(UIID id)
        {
            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if (m_PageList[i].id == id)
                {
                    return m_PageList[i];
                }
            }

            return null;
        }

        PageInfo FindPage(int instanceID)
        {
            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if (m_PageList[i].instanceID == instanceID)
                {
                    return m_PageList[i];
                }
            }

            return null;
        }

        PageInfo FindPage(AbstractPage page)
        {
            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if (m_PageList[i].page == page)
                {
                    return m_PageList[i];
                }
            }

            return null;
        }

        PageInfo FindPageForAttach(UIID id, bool isSingleton)
        {
            for (int i = 0; i < m_PageList.Count; ++i)
            {
                if (m_PageList[i].id == id)
                {
                    // 已经缓存的面板
                    if (m_PageList[i].isCached)
                    {
                        return m_PageList[i];
                    }

                    // 独立面板
                    if (isSingleton && m_PageList[i].isSingleton)
                    {
                        return m_PageList[i];
                    }
                }
            }

            return null;
        }

        UIData GetInnerPageUIData(string mainPageName, string pageName)
        {
            string innerUIName = string.Format("{0}:{1}", mainPageName, pageName);

            UIData data = UIDataTable.GetByName(innerUIName);
            if (data != null)
            {
                return data;
            }

            // 添加新的内置页面数据
            UIDataTable.AddInnerPageData(innerUIName);

            return UIDataTable.GetByName(innerUIName);
        }

        void RefreshParent(PageInfo pageInfo)
        {
            // 重新绑定根节点
            if (pageInfo.page != null && pageInfo.root != pageInfo.page.transform.parent)
            {
                pageInfo.page.transform.SetParent(pageInfo.root, false);
                UITools.SetGameObjectLayer(pageInfo.page.gameObject, pageInfo.root.gameObject.layer, true);
                UITools.SetGraphicRaycasterState(pageInfo.page.gameObject, m_IsInteractive);
            }
        }
    }
}
