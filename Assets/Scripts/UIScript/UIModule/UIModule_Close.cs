using LetsPiu.UIDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public partial class UIModule
    {
        /// <summary>
        /// 关闭面板
        /// </summary>
        public void ClosePanel(UIID uiID, string info = "")
        {
            if (!string.IsNullOrEmpty(info))
            {
                Debug.Log("ClosePanel {0} {1}".SafeFormat(uiID.ToString(), info));
            }

            UIPanelActive panel = FindRunningPanel(uiID);

            if (panel == null)
            {
                return;
            }

            PanelHideMask hideMask = GetPanelHideMask(panel);

            InnerClosePanel(panel);
            CheckCloseDependPanel();

            panel = null;
            ActiveGC(hideMask);
        }

        /// <summary>
        /// 通过唯一id关闭
        /// </summary>
        public void ClosePanelByPanelID(int panelID)
        {
            UIPanelActive panel = FindRunningPanelByPanelID(panelID);

            if (panel == null)
            {
                return;
            }

            PanelHideMask hideMask = GetPanelHideMask(panel);

            InnerClosePanel(panel);
            CheckCloseDependPanel();

            panel = null;
            ActiveGC(hideMask);
        }
    }
}
