using LetsPiu.Main;
using UnityEngine;
using Object = UnityEngine.Object;
using LetsPiu;
using LetsPiu.UI.UIScript.LoginScripts.Logic;
using LetsPiu.Base;
using Cysharp.Threading.Tasks;
using YooAsset;
using LetsPiu.GameLogic;

public static class StartGame
{
    static AssetHandle s_ToastPanelHandle;
    static AssetHandle s_UIDevHandle;

    public static async UniTask OnBeforeCreation()
    {
        // 按开发板型号自动识别串口/耳机声卡（首次访问即生效，这里提前触发并打印日志）
        BoardProfile.LogProfile();
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log($"[SerialPort] Unity 启动，LORA 串口: {BoardProfile.SerialPort} " +
                  $"(型号={BoardProfile.DetectedModel}, 板型默认={BoardProfile.DefaultSerialPort}" +
                  $"{(BoardProfile.HasSerialPortOverride ? $", 手动覆盖={BoardProfile.SerialPortOverride}" : "")}，进入主页后自动打开)");
#endif

        var uiRoot = GameObject.Find("UIRoot");
        var constantContainer = GameObject.Find("ConstantContainer");
        uiRoot.AddComponent<UIRoot>();
        constantContainer.AddComponent<ConstantContainer>();

        Camera.main.gameObject.AddComponent<CameraMgr>();

        await TableManager.S.LoadTable();

        ResLoader loader = new ResLoader();
        loader.AddRes("ToastCanvas", ResType.Prefab);
        await loader.StartLoad();
        s_ToastPanelHandle = loader.GetAsset("ToastCanvas");
        var toastPanel = GameObject.Instantiate(s_ToastPanelHandle.AssetObject as GameObject, UIRoot.S.transform);
        s_ToastPanelHandle.AttachToGameObject(toastPanel);
        toastPanel.name = "ToastCanvas";

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

        SettingMgr.S.InitSettings().Forget();

        #region 加载 UIDev
#if UIDEV
        loader.AddRes("UIDev", ResType.Prefab);
        await loader.StartLoad();
        s_UIDevHandle = loader.GetAsset("UIDev");
        var go = GameObject.Instantiate((s_UIDevHandle.AssetObject as GameObject), UIRoot.S.transform);
        s_UIDevHandle.AttachToGameObject(go);
        go.name = "UIDev";
#endif
        #endregion

        if (!uiRoot.GetComponent<PlayClickEffect>())
        {
            GameObject gameObject = new GameObject("PlayClickEffect");
            gameObject.AddComponent<PlayClickEffect>();
            Object.DontDestroyOnLoad(gameObject);
        }
    }
}