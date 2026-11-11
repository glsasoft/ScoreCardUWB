using LetsPiu.Main;
using UnityEngine;
using Object = UnityEngine.Object;
using LetsPiu;
using LetsPiu.UI.UIScript.LoginScripts.Logic;
using LetsPiu.Base;
using Cysharp.Threading.Tasks;
using YooAsset;

public static class StartGame
{
    static AssetHandle s_ToastPanelHandle;
    static AssetHandle s_UIDevHandle;

    public static async UniTask OnBeforeCreation()
    {
        var uiRoot = GameObject.Find("UIRoot");
        var constantContainer = GameObject.Find("ConstantContainer");
        uiRoot.AddComponent<UIRoot>();
        constantContainer.AddComponent<ConstantContainer>();

        Camera.main.gameObject.AddComponent<CameraMgr>();

        await TableManager.S.LoadTable();

        UIModule.S.OpenPanel(LetsPiu.UIDefine.UIID.LoginPanel, pnl =>
        {
            UniTask.Post(async () => 
            {
                await UniTask.NextFrame();
                await UniTask.NextFrame();
                var loadPanel = UIRoot.S.panelRoot.Find("HotUpdatePanel(Clone)");
                Object.Destroy(loadPanel.gameObject);
            });
        });
    }
}