using LetsPiu.GameLogic;
using LetsPiu.UI.UIScript.SettingScripts.UI;
using LetsPiu.UIDefine;
using System.Collections.Generic;
using LetsPiu.Common.UI;
using UnityEngine;

namespace LetsPiu.UI.UIScript.SettingScripts.Logic
{
    public class SettingPanel : AbstractPanel
    {
        public SettingPanelUI settingPanelUI;
        private UIID currentPageID = UIID.SettingBasicPage;
        private UIID[] settingPageIDs = new UIID[] 
        { 
            UIID.SettingBasicPage
        };

        protected override void Awake()
        {
            settingPanelUI.TabSelectedEvent += OnTabSelected;
            base.Awake();
            if (settingPanelUI.btnLogout)
                settingPanelUI.btnLogout.onClick.SetListener(HandleLogoutButtonClick);
            if (settingPanelUI.btnExit)
                settingPanelUI.btnExit.onClick.SetListener(HandleExitButtonClick);
        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            if (settingPanelUI.btnLogout && settingPanelUI.btnExit)
            {
                bool isHall = SceneLoadMgr.S.currentSceneType == Define.SceneType.Hall || SceneLoadMgr.S.currentSceneType == Define.SceneType.Room;
                isHall = isHall && UIModule.S.IsPanelActive(UIDefine.UIID.HomePagePanel);
                bool isPlaying = AppStateManager.Instance.IsPlaying();
                settingPanelUI.btnLogout.gameObject.SetActive(isHall);
                settingPanelUI.btnExit.gameObject.SetActive(!isHall && isPlaying);
            }
        }

        protected override void Start()
        {
            AttachPage(currentPageID, settingPanelUI.tabContainer);
        }

        private void OnTabSelected(string tabName, int tabIndex)
        {
            Debug.Log($"SettingPanel 选中了 {tabName} 标签页");
            DettachPage(currentPageID);
            
            if (tabIndex >= 0 && tabIndex < settingPageIDs.Length)
            {
                currentPageID = settingPageIDs[tabIndex];

                Debug.Log("SettingPanel 当前选中的Tab索引: " + tabIndex + " 当前页面ID: " + currentPageID);

                AttachPage(currentPageID, settingPanelUI.tabContainer);   
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            SettingMgr.S.SaveSettings().Forget();
        }
        
        
        public void HandleLogoutButtonClick()
        {
            Debug.Log("SettingPanel 点击了退出登录按钮");
            //发送请求退出登录消息
            EventManager.Instance.Broadcast(EventMsg.RequestLoginOut);
        }

        public void HandleExitButtonClick()
        {
            if (!AppStateManager.Instance.IsPlaying())
                return;

            var playerList = AppStateManager.Instance.PlayerList;

            Debug.Log("SettingPanel 点击了退出游戏按钮");
           
        }
    }
}
