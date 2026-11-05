using LetsPiu.UIDefine;
using System;
using System.Collections.Generic;
using UnityEngine;
using LetsPiu.UI.UIScript.GamesHallScripts.Offline.Logic;
using LetsPiu.UI.UIScript.LoginScripts.Logic;
using LetsPiu.UI.UIScript.ScanQrCode.Logic;
using LetsPiu.Common.UI;
using LetsPiu.UI.UIScript.Common.UI;
using LetsPiu.UI.UIScript.SceneLoad.UIPnlScript;

namespace LetsPiu
{
    public class UIDataTable
    {
        #region 数据定义
        static Dictionary<UIID, UIData> m_UIDataMap = new Dictionary<UIID, UIData>();
        static Dictionary<string, UIData> m_UINameDataMap = new Dictionary<string, UIData>();

        static UIID m_AutoInnerPageID = UIID.InnerPageBegin;
        #endregion

        public static void InitUIDataTable()
        {
            //           介是enum           介是Script       介是Prefab
            AddPanelData(UIID.LoginPanel, typeof(LoginPanel), "LoginPanel");
            AddPanelData(UIID.InputFieldDialogPanel, typeof(InputFieldDialogPanel), "InputFieldDialogPanel");
            AddPanelData(UIID.RegisterDevicePanel, typeof(RegisterDevicePanel), "RegisterDevicePanel");
            AddPanelData(UIID.AppUpdatePanel, typeof(AppUpdatePanel), "AppUpdatePanel");

            #region 游戏大厅页面定义
            AddPanelData(UIID.OfflinePage, typeof(OfflinePageLogic), "OfflinePage");
            #endregion

            #region loading面板定义

            AddPanelData(UIID.DefaultLoadingPanel, typeof(DefaultLoadingPanel), "DefaultLoadingPanel");

            #endregion

            #region 通用面板

            AddPanelData(UIID.ToastPanel, typeof(ToastPanel), "ToastPanel");
            AddPanelData(UIID.ScanQrCodePanel, typeof(ScanQrCodePanel), "ScanQrCodePanel");

            #endregion
          
            //公共弹窗面板定义
            AddPanelData(UIID.DialogManagerPanel, typeof(DialogManager), "DialogManagerPanel");
        }
        /// <summary>
        /// 面板定义    UIDataTable.AddPanelData(介是Enum,介是Script, 介是Prefab)                
        /// </summary>
        /// <param name="uiID">介是Enum</param>
        /// <param name="pType">介是Script</param>
        /// <param name="pName">介是Prefab</param>
        /// <param name="singleton"></param>
        /// <param name="cacheCount"></param>
        public static void AddPanelData(UIID uiID, Type pType, string pName, int cacheCount = 0, bool singleton=true)
        {
            Add(new PanelUIData(uiID, pType, pName, cacheCount,singleton));
        }

        public static void AddInnerPageData(string pName)
        {
            Add(new InnerPageData(++m_AutoInnerPageID, null, pName));
        }

        public static bool Remove(UIID uiID)
        {
            UIData data;
            if (!m_UIDataMap.TryGetValue(uiID, out data))
            {
                return false;
            }

            m_UIDataMap.Remove(uiID);

            m_UINameDataMap.Remove(data.name);
            return true;
        }

        public static void Add(UIData data)
        {
            if (data == null)
            {
                return;
            }

            if (m_UIDataMap.ContainsKey(data.uiID))
            {
                Debug.LogWarning("[UIDataTable:Add]Already Add UIData:" + data.uiID);
                return;
            }

            if (m_UINameDataMap.ContainsKey(data.name))
            {
                Debug.LogWarning("[UIDataTable:Add]ui name repeated:" + data.name);
                return;
            }

            m_UIDataMap.Add(data.uiID, data);
            m_UINameDataMap.Add(data.name, data);

#if UNITY_EDITOR

            if (data.uiID > UIID.NONE && data.uiID.ToString() != data.name)
            {
                Debug.LogWarning(string.Format("UIID <color=blue>{0}</color> 与资源名 <color=blue>{1}</color> 不一致", data.uiID.ToString(), data.name));
            }
#endif
        }

        public static UIData Get(UIID uiID)
        {
            UIData result = null;

            if (m_UIDataMap.TryGetValue(uiID, out result))
            {
                return result;
            }

            return null;
        }

        public static UIData GetByName(string name)
        {
            UIData result = null;
            if (m_UINameDataMap.TryGetValue(name, out result))
            {
                return result;
            }

            return null;
        }

        public static UIID PanelName2UIID(string name)
        {
            UIData data = GetByName(name);
            if (data != null)
            {
                return data.uiID;
            }

            return UIID.NONE;
        }
    }
}
