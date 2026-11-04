using Cysharp.Threading.Tasks;
using LetsPiu.Define;
using System;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.GameScripts.Common.UI;
using UnityEngine;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;

namespace LetsPiu.UI.UIScript.LoginScripts.Logic
{
    /// <summary>
    /// 登录面板
    /// </summary>
    public class LoginPanel : AbstractPanel
    {
        protected override void Awake()
        {
        }

        protected override void Start()
        {
            base.Start();
            CheckLocalDeviceInfo().Forget();
        }
        
        /// <summary>
        /// 检查当前本地缓存的注册设备信息是否有效
        /// </summary>
        private async UniTask CheckLocalDeviceInfo()
        {
            Debug.Log("LoginPanel fuckToken: CheckLocalDeviceInfo");
            var deviceInfo = AppStateManager.Instance.DeviceInfo5S;
            if (deviceInfo != null && deviceInfo.IsValid())
            {
                Debug.Log("LoginPanel 本地存在有效的设备信息，进入大厅加载（视频准备完成后再连接 MQTT）");
                AppStateManager.Instance.DeviceInfo5S = deviceInfo;
                GoToHall();
            }
            else
            {
                if (deviceInfo == null)
                {
                    Debug.LogWarning(
                        "LoginPanel CheckLocalDeviceInfo: DeviceInfo5S 为 null（启动时 LoadPrefs 无数据或反序列化失败，详见 [DeviceInfo5S.LoadPrefs] 日志）");
                }
                else
                {
                    Debug.LogWarning(
                        "LoginPanel CheckLocalDeviceInfo: DeviceInfo5S 对象存在但 IsValid=false，字段诊断: " +
                        deviceInfo.GetInvalidDataInfo());
                    deviceInfo.PrintDeviceInfo("LoginPanel CheckLocalDeviceInfo 无效对象详情");
                }

                Debug.LogWarning("LoginPanel 将打开注册设备面板...");
                OpenRegisterDevicePanel();
            }
        }
        
        private void OpenRegisterDevicePanel()
        {
            Debug.Log("LoginPanel 打开注册设备面板...");
            UIModule.S.OpenPanel(UIDefine.UIID.RegisterDevicePanel, panel =>
            {
                var registerDevicePanel = panel as RegisterDevicePanel;
                if (registerDevicePanel == null)
                {
                    Debug.LogError("LoginPanel RegisterDevicePanel 类型转换失败，无法订阅注册成功事件");
                    return;
                }

                registerDevicePanel.OnRegister5SDeviceSuccess -= GoToHall;
                registerDevicePanel.OnRegister5SDeviceSuccess += GoToHall;
                Debug.Log("LoginPanel RegisterDevicePanel 已打开，已绑定「注册成功 → 进入大厅(S0/Standby)」回调（须完成注册/MQTT 连接后才会切场景）");
            });
        }

        /// <summary>
        /// 删除本地设备注册数据、断开 MQTT，回到登录面板并重置其他面板；
        /// LoginPanel.Start 检测到设备信息为空后自动打开注册面板，与设备首次启动未注册的流程一致。
        /// </summary>
        public static void ClearRegisterInfoAndReturnToLogin()
        {
            AppStateManager.Instance.DeviceInfo5S = null;
            MqttConnector.Instance.Disconnect();
            UIModule.S.OpenPanel(UIDefine.UIID.LoginPanel, _ =>
            {
                UIModule.S.ResetAllPanel(uid => uid == UIDefine.UIID.LoginPanel);
            });
        }

        private void GoToHall()
        {
            Debug.Log("LoginPanel GoToHall：设备注册/连接成功，切换 Hall 场景（HomePage 将自动打开 S0/Standby）");
            SceneLoadMgr.S.SwitchScene(SceneType.Hall);
        }
        
        private void OnTVRemoteKey(RemoteKey remoteKey)
        {
            Debug.Log("LoginPanel 收到遥控器按键事件: " + remoteKey);
            //在这里处理遥控器按键事件
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("LoginPanel OnEnable 注册 NeedUpdateApp 事件");
            EventManager.Instance.AddEvent<string>(EventMsg.NeedUpdateApp, AppUpdatePanel.OpenAppUpdatePanel);
            EventManager.Instance.AddEvent<RemoteKey>(EventMsg.RemoteKeyEvent, OnTVRemoteKey);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Debug.Log("LoginPanel OnDisable 移除 NeedUpdateApp 事件");
            EventManager.Instance.RemoveEvent<string>(EventMsg.NeedUpdateApp, AppUpdatePanel.OpenAppUpdatePanel);
            EventManager.Instance.RemoveEvent<RemoteKey>(EventMsg.RemoteKeyEvent, OnTVRemoteKey);
        }
    }
}
