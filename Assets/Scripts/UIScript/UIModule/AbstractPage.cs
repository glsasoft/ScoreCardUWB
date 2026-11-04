using LetsPiu.UIDefine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace LetsPiu
{
    public class AbstractPage : AbstractUIElement
    {
        [SerializeField]
        SpriteAtlas[] m_SpritesAtlas;

        static List<AbstractPage> s_TeamPageList = new List<AbstractPage>();

        protected UIPanelActive m_PanelActive;
        protected AbstractPage m_ParentPage;

        private Dictionary<Canvas, int> m_childCanvas = new Dictionary<Canvas, int>();
        private List<KeyValuePair<Canvas, int>> tempCanvas = new List<KeyValuePair<Canvas, int>>();

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.AddEvent(EventMsg.OnPageCanvasOrderChanged, OnPageCanvasOrderChanged);

            OnPageCanvasOrderChanged();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Instance.RemoveEvent(EventMsg.OnPageCanvasOrderChanged, OnPageCanvasOrderChanged);
        }

        // 图集数据
        public SpriteAtlas[] spritesAtlas
        {          
            get { return m_SpritesAtlas; }
        }

        // 设置面板的活动控件
        public UIPanelActive panelActive
        {
            get
            {
                return m_PanelActive;
            }

            set
            {
                m_PanelActive = value;
                m_ParentPage = m_PanelActive.panel;
                if (m_ParentPage == this)
                {
                    // 面板父页面不能是自己，所以没有父页面
                    m_ParentPage = null;
                }
            }
        }

        // 面板的UIID
        public UIID uiID
        {
            get { return m_PanelActive.id; }
        }

        // 页面所在的面板实体ID
        public int panelID
        {
            get { return m_PanelActive.instanceID; }
        }

        // 当前页面UIID，如果是面板，该ID和面板ID相同
        public UIID pageUIID { get; set; }

        // 当前页面实体ID，如果是面板，该ID和面板ID相同
        public int pageID { get; set; }

        // 当前面板是否已经打开
        public bool hasOpened
        {
            get
            {
                return m_PanelActive.IsPageOpened(pageID);
            }
        }

        // 基于面板设置图片精灵
        public bool SetSprite(Image image, string spriteName, bool isNative = false)
        {
            if (image == null || string.IsNullOrEmpty(spriteName))
            {
                return false;
            }

            AbstractPage currentPage = this;

            for (int i = 0; i < 10; ++i)
            {
                if (SpritesHandler.SetSprite(currentPage.m_SpritesAtlas, image, spriteName, isNative))
                {
                    return true;
                }

                currentPage = currentPage.m_ParentPage;
                if (currentPage == null)
                {
                    break;
                }
            }

            Debug.LogWarning("Not Find SpriteName:" + spriteName);

            return false;
        }

        // 关闭当前页面，如果当前页面是主页面，会关闭面板
        public virtual void CloseSelf()
        {
            if (m_PanelActive != null)
            {
                m_PanelActive.DettachPageWithInstanceID(this.pageID);
            }
        }

        // 关闭当前面板
        public void CloseSelfPanel()
        {
            UIModule.S.ClosePanelByPanelID(this.panelID);
        }

        // 挂载当前面板的页面
        public int AttachPage(UIID pageUIID, Transform parent, Action<AbstractPage> listener = null, bool singleton = true)
        {
            if (m_PanelActive != null)
            {
                return m_PanelActive.AttachPage(pageUIID, parent, listener, singleton, null);
            }
            return 0;
        }

        // 挂载当前面板的页面
        public int AttachPage(UIID pageUIID, Transform parent, Action<AbstractPage> listener, bool singleton, params object[] args)
        {
            if (m_PanelActive != null)
            {
                return m_PanelActive.AttachPage(pageUIID, parent, listener, singleton, args);
            }

            return 0;
        }

        // 卸载当前面板的页面
        public void DettachPage(UIID pageUIID)
        {
            if (m_PanelActive != null)
            {
                m_PanelActive.DettachPage(pageUIID);
            }
        }

        // 卸载当前面板的页面
        public void DettachPage(int pageID)
        {
            if (m_PanelActive != null)
            {
                m_PanelActive.DettachPageWithInstanceID(pageID);
            }
        }

        // 当前面板是否挂载该页面
        public bool IsPageAttached(UIID pageUIID)
        {
            if (m_PanelActive != null)
            {
                return m_PanelActive.IsPageAttached(pageUIID);
            }

            return false;
        }

        public bool IsPageOpened(UIID pageUIID)
        {
            if (m_PanelActive != null)
            {
                return m_PanelActive.IsPageOpened(pageUIID);
            }

            return false;
        }

        public T FindPage<T>(bool includeCached = false) where T : AbstractPage
        {
            if (m_PanelActive != null)
            {
                return m_PanelActive.FindPage<T>(includeCached);
            }

            return null;
        }

        // 对当前面板打开依赖面板
        public void OpenDependPanel(UIID uiID, Action<AbstractPanel> callBack, params object[] args)
        {
            UIModule.S.OpenDependPanel(this.panelID, uiID, callBack, args);
        }

        // 对当前面板打开依赖面板
        public void OpenDependPanel(UIID uiID, params object[] args)
        {
            UIModule.S.OpenDependPanel(this.panelID, uiID, null, args);
        }

        // 关闭当前面板的被宿主面板
        public void CloseMasterPanel()
        {
            int masterPanelID = UIModule.S.FindMasterPanel(this.panelID, true);
            UIModule.S.ClosePanelByPanelID(masterPanelID);
        }

        // 发送UI事件，该事件会在当前面板的所有页面产生OnViewEvent回调
        public void SendViewEvent<T>(T key, params object[] args) where T : IConvertible
        {
            int eventID = key.ToInt32(null);

            // 获取页面列表
            m_PanelActive.GetPageList(s_TeamPageList);

            // 通知所有页面
            for (int i = 0; i < s_TeamPageList.Count; ++i)
            {
                if (s_TeamPageList[i] != null)
                {
                    s_TeamPageList[i].OnViewEvent(eventID, args);
                }
            }
        }

        // 调用页面的SetActive后，不会立刻进入OnOpen和OnClose回调
        // 主动调用刷新，会立刻进入OnOpen和OnClose回调
        public void UpdateActive()
        {
            if (m_PanelActive != null)
            {
                m_PanelActive.UpdateActive();
            }
        }

        void OnPageCanvasOrderChanged()
        {
            tempCanvas.Clear();
            foreach (var item in m_childCanvas)
            {
                if (item.Key)
                    tempCanvas.Add(item);    
            }
            m_childCanvas.Clear();

            foreach (var item in tempCanvas)
            {
                SetChildCanvasOrder(item.Key, item.Value);
            }
        }

        public void SetChildCanvasOrder(Canvas childCanvas, int offsetOrder)
        {
            if (childCanvas == null || !childCanvas.transform.IsChildOf(transform))
                return;

            m_childCanvas[childCanvas] = offsetOrder;

            Canvas canvas = null;
            Transform parent = transform;
            while (parent)
            {
                canvas = parent.GetComponent<Canvas>();
                if (canvas && canvas != childCanvas && canvas.GetComponent<AbstractPage>())
                {
                    break;
                }
                else
                {
                    canvas = null;
                    parent = parent.parent;
                }    
            }
            if (canvas)
            {
                childCanvas.sortingOrder = canvas.sortingOrder + offsetOrder;
            }
            else if (childCanvas.rootCanvas != childCanvas)
            {
                childCanvas.sortingOrder = childCanvas.rootCanvas.sortingOrder + offsetOrder;
            }
        }
    }
}

