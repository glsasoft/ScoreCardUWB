using System;
using Cysharp.Threading.Tasks;
using LetsPiu.Common.UI;
using LetsPiu.UIDefine;
using UnityEngine;

namespace LetsPiu.UI.UIScript.LoginScripts.Logic
{
    /// <summary>
    /// App更新对话框
    /// </summary>
    public class AppUpdatePanel : AbstractPanel
    {
        
        protected override void Awake()
        {
            base.Awake();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                OpenAppUpdateDialog();
            }
        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            OpenAppUpdateDialog();
        }

        private void OpenAppUpdateDialog()
        {
            DialogManager.OpenAlertDialog("温馨提示", "App版本太低，请升级！", "前往", () =>
            {
                Application.OpenURL("https://piupiu.glsa.com.cn/device/1/1/NULL");
                UniTask.Post(async () =>
                {
                    await UniTask.WaitForSeconds(0.1f);
                    OpenAppUpdateDialog();
                });
            });
        }
        
        public static void OpenAppUpdatePanel(string appVersion)
        {
#if !UIDEV
            Debug.Log("OpenAppUpdatePanel NeedUpdateApp appVersion:" + appVersion);
            UIModule.S.OpenPanel(UIID.AppUpdatePanel, a =>
            {
                Debug.Log("OpenAppUpdatePanel NeedUpdateApp 打开AppUpdatePanel成功");
            }); 
#endif
        }
    }
}
