using LetsPiu.UIDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public partial class UIModule
    {
        public int OpenDependPanel(int panelID, UIID uiID, Action<AbstractPanel> callBack, params object[] args)
        {
            UIPanelActive panelActive = FindRunningPanelByPanelID(panelID);

            if (panelActive == null)
            {
                Debug.LogError("[UIModule:OpenDependPanel]can't find master panel " + panelID);
                return 0;
            }

            return InnerOpenPanel(uiID, panelID, panelActive.sortLayer, callBack, args);
        }

        public int OpenPanel(UIID uiID)
        {
            return InnerOpenPanel(uiID, 0, PanelSortLayer.Auto, null, new object[0]);
        }

        public int OpenPanel(UIID uiID, params object[] args)
        {
            return InnerOpenPanel(uiID, 0, PanelSortLayer.Auto, null, args);
        }

        public int OpenPanel(UIID uiID, Action<AbstractPanel> callBack, params object[] args)
        {
            return InnerOpenPanel(uiID, 0, PanelSortLayer.Auto, callBack, args);
        }

        public int OpenPanelCustom(UIID uiID, int masterPanel, PanelSortLayer layer, Action<AbstractPanel> callBack, params object[] args)
        {
            return InnerOpenPanel(uiID, masterPanel, layer, callBack, args);
        }

        int InnerOpenPanel(UIID uiID, int masterPanel, PanelSortLayer sortLayer, Action<AbstractPanel> onOpened, params object[] args)
        {
            Debug.LogFormat("[UIModule:InnerOpenPanel]open {0}", uiID);

            UIData data = UIDataTable.Get(uiID);
            if (data == null)
            {
                Debug.LogError("[UIModule:OpenPanel]can't find ui id int table " + uiID);
                return 0;
            }

            // 检测失效的面板
            CheckInvalid();

            UIPanelActive panel = null;

            if (data.isSingleton)
            {
                panel = FindRunningPanel(uiID);
            }

            if (panel == null)
            {
                panel = GetCachedPanel(uiID);
            }

            if (panel == null)
            {
                panel = new UIPanelActive(uiID);
                panel.onPanelChanged.AddListener(OnPanelChanged);
            }

            // 设置参数
            panel.masterPanel = masterPanel;
            panel.sortLayer = sortLayer;

            // 加入面板
            AddRunningPanel(panel);

            // 打开面板
            panel.Open(GetPanelRoot(uiID), onOpened, args);

            // 检测重新排列面板
            CheckResortPanel();

            return panel.instanceID;
        }
    }
}
