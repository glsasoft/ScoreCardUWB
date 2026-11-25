using Cysharp.Threading.Tasks;
using LetsPiu.Define;
using System;
using Common.Network.Model.Http;
using LetsPiu.Common.UI;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.GameScripts.Common.UI;
using UnityEngine;
using UnityEngine.UI;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using LetsPiu.Base;
using TMPro;

namespace LetsPiu.UI.UIScript.LoginScripts.Logic
{
    /// <summary>
    /// 设备注册面板
    /// </summary>
    public class RegisterDevicePanel : AbstractPanel
    {
        
        public RectTransform qrcodeViewerTransform;
        public Image qrcodeViewerImage;
        public TMP_InputField qrcodeHttpURLText;
        
        public event Action OnRegister5SDeviceSuccess;

        Action _registerSuccessHandler;
        bool _registerSuccessNotified;

        // Local HTTP server instance (optional, created on demand)
        private LocalDeviceHttpServer _localServer;
        
        private bool _isRegistering = false;

        static readonly string[] s_TestScoreCardButtonNames =
        {
            "TestScoreCard1",
            "TestScoreCard2",
            "TestScoreCard3",
            "TestScoreCard4",
        };

        // 测试比分牌按钮的父节点；隐藏后 Menu 的 VerticalLayoutGroup 只剩「开始注册」按钮，自动居中
        // Release 预设入口改到登录页双击设置面板，此处仍仅 Editor 显示原按钮
        const string TestScoreCardRootName = "TestScoreCards";
        
        protected override void Awake()
        {
#if !UNITY_EDITOR
            var testRoot = FindChildTransformByName(transform, TestScoreCardRootName);
            if (testRoot != null) testRoot.gameObject.SetActive(false);
#endif
            RefreshTestScoreCardButtonLabels();
        }

        protected override void Start()
        {
            base.Start();
            RefreshTestScoreCardButtonLabels();
        }

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            _registerSuccessNotified = false;
            if (args != null && args.Length > 0 && args[0] is Action handler)
            {
                SetRegisterSuccessHandler(handler);
            }
        }

        public void SetRegisterSuccessHandler(Action handler)
        {
            _registerSuccessHandler = handler;
        }

        void NotifyRegisterSuccess()
        {
            if (_registerSuccessNotified)
            {
                Debug.LogWarning("RegisterDevicePanel 注册成功回调已触发，忽略重复通知");
                return;
            }

            _registerSuccessNotified = true;
            Debug.Log("RegisterDevicePanel 设备注册/连接成功，准备进入大厅(S0/Standby)...");
            try
            {
                OnRegister5SDeviceSuccess?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("RegisterDevicePanel OnRegister5SDeviceSuccess 回调异常: " + e);
            }

            try
            {
                _registerSuccessHandler?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("RegisterDevicePanel _registerSuccessHandler 回调异常: " + e);
            }
        }
        
        public void HandleClickTestScoreCard0()
        {
            RegisterWithTestDevice(0);
        }
        
        public void HandleClickTestScoreCard1()
        {
            RegisterWithTestDevice(1);
        }
        
        public void HandleClickTestScoreCard2()
        {
            RegisterWithTestDevice(2);
        }
        
        public void HandleClickTestScoreCard3()
        {
            RegisterWithTestDevice(3);
        }

        void RegisterWithTestDevice(int index)
        {
            ApplyTestDevicePreset(index);
        }

        void ApplyTestDevicePreset(int index)
        {
            if (!ScoreCardTestDevicePresets.TryApply(index, out string error))
            {
                DialogManager.OpenAlertDialog("温馨提示", error ?? "预注册失败", "确定");
                return;
            }

            NotifyRegisterSuccess();
        }

        void RefreshTestScoreCardButtonLabels()
        {
            for (var i = 0; i < ScoreCardTestDevicePresets.Count; i++)
            {
                var buttonName = i < s_TestScoreCardButtonNames.Length
                    ? s_TestScoreCardButtonNames[i]
                    : null;
                if (string.IsNullOrEmpty(buttonName))
                {
                    continue;
                }

                var buttonTransform = FindChildTransformByName(transform, buttonName);
                if (buttonTransform == null)
                {
                    Debug.LogWarning($"RegisterDevicePanel 未找到测试按钮: {buttonName}");
                    continue;
                }

                var label = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label == null)
                {
                    Debug.LogWarning($"RegisterDevicePanel 测试按钮缺少 TMP 文本: {buttonName}");
                    continue;
                }

                label.text = ScoreCardTestDevicePresets.GetDeviceCode(i);
            }
        }

        static Transform FindChildTransformByName(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            RefreshTestScoreCardButtonLabels();
        }
#endif
        
        public void HandleClickAutoRegister5SDevice()
        {
            if (_isRegistering)
            {
                ToastTMPPanel.Show("设备注册中，请稍候...");
                return;
            }

            Register5SDevice().Forget();
        }
        
        public void HandleClickManualRegister5SDevice()
        {
            Debug.Log("RegisterDevicePanel 手动注册5S设备...");
            //在 Scripts/GameLogicScripts/GameScripts 下创建一个新的脚本，用于接收局域网的http请求提交的设备数据，功能如下：
            
            //第一步：启动一个局域网HTTP服务器，首页提供一个简单的HTML页面，页面有一个表单用于输入设备数据
            //表单包含以下字段:
            /*
                 "umeng_flag": "store1-1",
                "uri": "tcp://tx0000000000000003.wogemu.com:1883",
                "device_name": "20251127094410",
                "product_key": "/store1",
                "device_pass": "ffed2b"
             */
            //第二步：将局域网的url通过二维码展示出来，用户使用手机扫码访问该页面
            //第三步：用户在手机页面输入设备数据并提交，HTTP服务器接收到数据后，自动填充到DeviceInfo5S对象中，并自动填充topic，然后保存到本地。
            // StartLocalServer();
        }
        
        private async UniTask Register5SDevice()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var deviceCode = DeviceIdentifier.DeviceId;
#else
            var deviceCode = DateTime.Now.ToString("yyyyMMddHHmmss");
#endif
            await Register5SDeviceWithCode(deviceCode);
        }

        private async UniTask Register5SDeviceWithCode(string deviceCode)
        {
            if (_isRegistering)
            {
                ToastTMPPanel.Show("设备注册中，请稍候...");
                return;
            }

            _isRegistering = true;
            Debug.Log("RegisterDevicePanel 开始注册5S设备，设备码: " + deviceCode);
            ToastTMPPanel.Show("正在自动注册设备，请稍候...");
            
            HttpJsonResponse<DeviceInfo5S> resp;
            try
            {
                resp = await HttpConnector.Instance.RegisterDeviceAsync(deviceCode, DeviceType5S.UnityScoreCard);
            }
            catch (Exception e)
            {
                Debug.LogError("RegisterDevicePanel 自动注册请求异常: " + e);
                DialogManager.OpenAlertDialog("温馨提示", "设备注册失败，网络请求异常：" + e.Message, "确定");
                _isRegistering = false;
                return;
            }
            
            Debug.Log("RegisterDevicePanel 注册5S设备返回 success = " + resp.success + " Msg=" + resp.msg);
            
            if (!resp.success || resp.data == null)
            {
                Debug.LogError("RegisterDevicePanel 注册5S设备失败: " + resp.msg + " resp.data==null?" + (resp.data == null));
                DialogManager.OpenAlertDialog("温馨提示", "设备注册失败，请检查网络后重试！错误原因:" + resp.msg, "确定");
                _isRegistering = false;
                return;
            }
            if (!resp.data.IsValid())
            {
                Debug.LogError("RegisterDevicePanel 注册5S设备返回的数据无效: " + resp.data.GetInvalidDataInfo());
                DialogManager.OpenAlertDialog("温馨提示", "设备注册失败，返回的数据无效:" + resp.data.GetInvalidDataInfo(), "确定");
                _isRegistering = false;
                return;
            }
            resp.data.PrintDeviceInfo("RegisterDevicePanel Register5SDevice 自动注册成功");
            Debug.Log("RegisterDevicePanel 自动注册 5S设备注册成功，进入大厅加载（视频准备完成后再连接 MQTT）");
            _isRegistering = false;
            AppStateManager.Instance.DeviceInfo5S = resp.data;
            NotifyRegisterSuccess();
        }
        
        private void OnTVRemoteKey(RemoteKey remoteKey)
        {
            Debug.Log("RegisterDevicePanel 收到遥控器按键事件: " + remoteKey);
            //在这里处理遥控器按键事件
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshTestScoreCardButtonLabels();
            EventManager.Instance.AddEvent<RemoteKey>(EventMsg.RemoteKeyEvent, OnTVRemoteKey);
            // 登录页设置面板的 1~4 号预注册 → 与面板按钮同一条成功回调进大厅
            ScoreCardTestDevicePresets.EnterHallRequested -= OnPresetEnterHallFromSettings;
            ScoreCardTestDevicePresets.EnterHallRequested += OnPresetEnterHallFromSettings;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Instance.RemoveEvent<RemoteKey>(EventMsg.RemoteKeyEvent, OnTVRemoteKey);
            ScoreCardTestDevicePresets.EnterHallRequested -= OnPresetEnterHallFromSettings;
            // StopLocalServer();
        }

        void OnPresetEnterHallFromSettings()
        {
            NotifyRegisterSuccess();
        }
        
        // /// <summary>
        // /// 点击：启动局域网设备注册 HTTP 服务器并展示二维码
        // /// </summary>
        // private void StartLocalServer()
        // {
        //     try
        //     {
        //         if (_localServer == null)
        //         {
        //             // 在运行时创建一个 GameObject 并挂载 LocalDeviceHttpServer
        //             var go = new GameObject("LocalDeviceHttpServer");
        //             DontDestroyOnLoad(go);
        //             _localServer = go.AddComponent<LocalDeviceHttpServer>();
        //         }
        //
        //         if (!_localServer.IsRunning)
        //         {
        //             _localServer.StartServer();
        //         }
        //         else
        //         {
        //             Debug.Log("RegisterDevicePanel: LocalDeviceHttpServer already running.");
        //         }
        //
        //         // 生成并显示二维码（如果有 UI Image 可用）
        //         if (qrcodeViewerImage != null && qrcodeViewerTransform != null)
        //         {
        //             var tex = _localServer.GetServerQrTexture(512);
        //             if (tex != null)
        //             {
        //                 qrcodeViewerTransform.gameObject.SetActive(true);
        //                 var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        //                 qrcodeViewerImage.sprite = sprite;
        //                 qrcodeViewerImage.enabled = true;
        //             }
        //         }
        //
        //         Debug.Log("RegisterDevicePanel: LocalDeviceHttpServer started by user.");
        //         ToastTMPPanel.Show("局域网设备注册服务已启动，扫码访问页面完成注册");
        //         
        //         // 订阅本地服务器的提交事件
        //         _localServer.DeviceSubmitted += OnLocalDeviceSubmitted;
        //         if (qrcodeHttpURLText != null)
        //         {
        //             var url = (_localServer != null && _localServer.IsRunning) ? _localServer.ServerUrl : null;
        //             qrcodeHttpURLText.text = "访问地址: " + (string.IsNullOrEmpty(url) ? "(未知)" : url);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError("HandleClickStartLocalServer error: " + ex);
        //         ToastTMPPanel.Show("启动本地服务器失败: " + ex.Message);
        //     }
        // }

        // /// <summary>
        // /// 点击：停止局域网设备注册 HTTP 服务器并隐藏二维码
        // /// </summary>
        // public void StopLocalServer()
        // {
        //     Debug.Log("RegisterDevicePanel StopLocalServer called.");
        //     try
        //     {
        //         if (_localServer != null)
        //         {
        //             _localServer.DeviceSubmitted -= OnLocalDeviceSubmitted;
        //             _localServer.StopServer();
        //             Debug.Log("RegisterDevicePanel: LocalDeviceHttpServer stopped by user.");
        //         }
        //
        //         if (qrcodeViewerImage != null)
        //         {
        //             qrcodeViewerImage.sprite = null;
        //             qrcodeViewerImage.enabled = false;
        //         }
        //
        //         if (qrcodeViewerTransform != null)
        //         {
        //             qrcodeViewerTransform.gameObject.SetActive(false);   
        //         }
        //
        //         if (qrcodeHttpURLText != null)
        //         {
        //             qrcodeHttpURLText.text = "访问地址: ";
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError("HandleClickStopLocalServer error: " + ex);
        //     }
        // }
        //
        // private void OnLocalDeviceSubmitted(DeviceInfo5S device)
        // {
        //     Debug.Log("RegisterDevicePanel OnLocalDeviceSubmitted 收到设备提交事件，准备停止本地服务器...");
        //     StopLocalServer();
        //     
        //     if(device == null || !device.IsValid())
        //     {
        //         Debug.LogError("OnLocalDeviceSubmitted 收到无效的设备信息: " + (device == null ? "null" : device.GetInvalidDataInfo()));
        //         DialogManager.OpenAlertDialog("温馨提示", "收到的设备信息无效，请重新注册:"+(device == null ? "null" : device.GetInvalidDataInfo()), "确定");
        //         return;
        //     }
        //     
        //     try
        //     {
        //         Debug.Log("RegisterDevicePanel 收到 LocalDeviceHttpServer 的设备提交事件");
        //         device.PrintDeviceInfo("RegisterDevicePanel  OnLocalDeviceSubmitted");
        //         TestDeviceMqttConnectionAndContinue(device);
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError("OnLocalDeviceSubmitted error: " + ex);
        //     }
        // }
    }
}
