using LetsPiu.Base;
using LetsPiu.UIDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    [RequireComponent(typeof(Canvas))]
    public class AbstractPanel : AbstractPage
    {
        [SerializeField]
        PanelHideMask m_HideMask = PanelHideMask.None;
        [SerializeField]
        AudioDefine m_OpenAudioID = AudioDefine.UI_Open;      // 特性打开面板
        [SerializeField]
        AudioDefine m_CloseAudioID = AudioDefine.UI_Close;     // 特定关闭面板

        //需要自己控制层级的面板重写改函数(返回值大于0生效)
        public virtual int sortIndex
        {
            get { return 0; }
        }

        //面板影藏属性,作用于其下层面板
        public PanelHideMask hideMask
        {
            get { return m_HideMask; }
            set
            {
                if (m_HideMask == value)
                {
                    return;
                }

                m_HideMask = value;

                if (m_PanelActive.panel == this)
                {
                    EventManager.Instance.Broadcast<int>(EventMsg.OnPanelObjectChanged, this.panelID);
                }
            }
        }

        

        public void PlayOpenSound()
        {
            // 旧版 UI_Open 面板切换音效已废弃（比分牌不使用）
        }

        public void PlayCloseSound()
        {
            // 旧版 UI_Close 面板切换音效已废弃（比分牌不使用）
        }

        // 关闭当前面板
        public sealed override void CloseSelf()
        {
            CloseSelfPanel();
        }
        
#if UNITY_EDITOR 
        protected override void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // 只处理游戏运行时
                return;
            }

            if (m_PanelActive != null && m_PanelActive.panel == this)
            {
                EventManager.Instance.Broadcast<int>(EventMsg.OnPanelObjectChanged, this.panelID);
            }
        }
#endif
    }
}

