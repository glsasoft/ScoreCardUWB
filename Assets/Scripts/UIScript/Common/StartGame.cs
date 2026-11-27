using LetsPiu.Main;
using UnityEngine;
using Object = UnityEngine.Object;
using LetsPiu;
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

        // 热更条继续留着：进厅由 SceneLoadMgr 在 DefaultLoading 面板打开后再关，避免中间黑屏；
        // 未注册设备走注册页时由 LoginPanel 关闭。
        UIModule.S.OpenPanel(LetsPiu.UIDefine.UIID.LoginPanel);
    }
}
