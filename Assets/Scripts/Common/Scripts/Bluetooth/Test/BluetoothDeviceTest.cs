using Common.Bluetooth.Model.Read;
using Common.Bluetooth.Model.Send;
using Cysharp.Threading.Tasks;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read;
using TMPro;
using UnityEngine;

namespace LetsPiu.Common.Scripts.Bluetooth.Test
{
    public class BluetoothDeviceTest : MonoBehaviour
    {

        public TMP_InputField gunCode;
        public TMP_InputField headCode;

        private void HandleBoundEvent(BindDeviceResult result)
        {
            Debug.Log("BluetoothDeviceTest 设备定完成:"+result.errType+" "+result.errMsg+" 是否是抢:"+result.deviceInfo.IsGun()+" 是否是头:"+result.deviceInfo.IsHead());
        }

        private void HandleConnectEvent()
        {
            Debug.Log("BluetoothDeviceTest 抢是否链接:"+DeviceConnector.Instance.IsGunConnected());
            Debug.Log("BluetoothDeviceTest 头是否链接:"+DeviceConnector.Instance.IsHeadConnected());
        }

        private void HandleGunBeHitMessage(BlueGunBeHitMessage message)
        {
            Debug.Log("BluetoothDeviceTest 枪被击中 玩家id:"+message.shootId+" 队伍编号:"+message.teamId);
        }
        
        private void HandleHeadBeHitMessage(BlueGunBeHitMessage message)
        {
            Debug.Log("BluetoothDeviceTest 头被击中 玩家id:"+message.shootId+" 队伍编号:"+message.teamId);
        }

        private void HandleShootEvent()
        {
            Debug.Log("BluetoothDeviceTest 按下了射击按钮..");
        }

        private void HandleEKeyPressEvent(KeyStatus status)
        {
            Debug.Log("BluetoothDeviceTest 按下了E按钮:"+status);
        }
        
        private void HandleHeadPingMessage(BlueHeadPingMessage msg)
        {
            Debug.Log("BluetoothDeviceTest 头电量:"+msg.power);
        }
        
        private void HandleGunPingMessage(BlueGunPingMessage msg)
        {
            Debug.Log("BluetoothDeviceTest 抢电量:"+msg.power);
        }
        
        private void HandleInitSuccess()
        {
            Debug.Log("BluetoothDeviceTest 设备管理器初始化成功!");
        }
        
        private void HandleInitFail(string error)
        {
            Debug.Log("BluetoothDeviceTest 设备管理器初始化失败!");
        }

        private void OnEnable()
        {
            EventManager.Instance.AddEvent<BindDeviceResult>(EventMsg.GunBoundEvent, HandleBoundEvent);
            EventManager.Instance.AddEvent<BindDeviceResult>(EventMsg.HeadBoundEvent, HandleBoundEvent);
            EventManager.Instance.AddEvent(EventMsg.GunConnectedEvent, HandleConnectEvent);
            EventManager.Instance.AddEvent(EventMsg.HeadConnectedEvent, HandleConnectEvent);
            EventManager.Instance.AddEvent(EventMsg.HeadDisconnectedEvent, HandleConnectEvent);
            EventManager.Instance.AddEvent(EventMsg.GunDisconnectedEvent, HandleConnectEvent);
            EventManager.Instance.AddEvent(EventMsg.GunConnectingEvent, HandleConnectEvent);
            EventManager.Instance.AddEvent(EventMsg.HeadConnectingEvent, HandleConnectEvent);
            EventManager.Instance.AddEvent<BlueGunBeHitMessage>(EventMsg.GunBeHitEvent, HandleGunBeHitMessage);
            EventManager.Instance.AddEvent<BlueGunBeHitMessage>(EventMsg.HeadBeHitEvent, HandleGunBeHitMessage);
            EventManager.Instance.AddEvent<KeyStatus>(EventMsg.EKeyEvent, HandleEKeyPressEvent);
            EventManager.Instance.AddEvent(EventMsg.GunShootEvent, HandleShootEvent);
            EventManager.Instance.AddEvent<BlueHeadPingMessage>(EventMsg.HeadPingEvent, HandleHeadPingMessage);
            EventManager.Instance.AddEvent<BlueGunPingMessage>(EventMsg.GunPingEvent, HandleGunPingMessage);
            EventManager.Instance.AddEvent(EventMsg.DevConnectorInitSuccessEvent, HandleInitSuccess);
            EventManager.Instance.AddEvent<string>(EventMsg.DevConnectorInitFailedEvent, HandleInitFail);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveEvent<BindDeviceResult>(EventMsg.GunBoundEvent, HandleBoundEvent);
            EventManager.Instance.RemoveEvent<BindDeviceResult>(EventMsg.HeadBoundEvent, HandleBoundEvent);
            EventManager.Instance.RemoveEvent(EventMsg.GunConnectedEvent, HandleConnectEvent);
            EventManager.Instance.RemoveEvent(EventMsg.HeadConnectedEvent, HandleConnectEvent);
            EventManager.Instance.RemoveEvent(EventMsg.HeadDisconnectedEvent, HandleConnectEvent);
            EventManager.Instance.RemoveEvent(EventMsg.GunDisconnectedEvent, HandleConnectEvent);
            EventManager.Instance.RemoveEvent(EventMsg.GunConnectingEvent, HandleConnectEvent);
            EventManager.Instance.RemoveEvent(EventMsg.HeadConnectingEvent, HandleConnectEvent);
            EventManager.Instance.RemoveEvent<BlueGunBeHitMessage>(EventMsg.GunBeHitEvent, HandleGunBeHitMessage);
            EventManager.Instance.RemoveEvent<BlueGunBeHitMessage>(EventMsg.HeadBeHitEvent, HandleGunBeHitMessage);
            EventManager.Instance.RemoveEvent<KeyStatus>(EventMsg.EKeyEvent, HandleEKeyPressEvent);
            EventManager.Instance.RemoveEvent(EventMsg.GunShootEvent, HandleShootEvent);
            EventManager.Instance.RemoveEvent<BlueHeadPingMessage>(EventMsg.HeadPingEvent, HandleHeadPingMessage);
            EventManager.Instance.RemoveEvent<BlueGunPingMessage>(EventMsg.GunPingEvent, HandleGunPingMessage);
            EventManager.Instance.RemoveEvent(EventMsg.DevConnectorInitSuccessEvent, HandleInitSuccess);
            EventManager.Instance.RemoveEvent<string>(EventMsg.DevConnectorInitFailedEvent, HandleInitFail);
        }

        private void Start()
        {
        }

        /// <summary>
        /// 必须先登录才能绑定枪
        /// </summary>
        public void TestLogin()
        {
            StartCoroutine(TestLoginAsync().ToCoroutine());
        }
        
        /// <summary>
        /// 自动发送验证码并登录一个测试账号
        /// </summary>
        private async UniTask TestLoginAsync()
        {
            // const string phone = "56565656565";
            // var r1 = await HttpConnector.Instance.SendPhoneCodeAsync("1", phone);
            // var r2 = await HttpConnector.Instance.LoginWithPhoneCodeAsync(phone, "0000");
            // Debug.Log("BluetoothDeviceTest 登录结果:"+r2.success);
            // AppStateManager.Instance.Token = r2.data.token;
            // AppStateManager.Instance.Customer = r2.data.customer;
        }
        
        /// <summary>
        /// 测试绑定枪(正常绑定是绑定扫描到的二维码)
        /// </summary>
        public void TestBindGun()
        {
            var code = gunCode.text;
            var gunQrCode = "http://piupiu.glsa.com.cn/device/1/1/"+code;
            DeviceConnector.Instance.BindDevice(gunQrCode, result =>
            {
                Debug.Log("BluetoothDeviceTest 绑定装备结果:"+result);
            });
        }
        
        /// <summary>
        /// 测试绑定头(正常绑定是绑定扫描到的二维码)
        /// </summary>
        public void TestBindHead()
        {
            var code = headCode.text;
            var gunQrCode = "http://piupiu.glsa.com.cn/device/2/1/"+code;
            DeviceConnector.Instance.BindDevice(gunQrCode, result =>
            {
                Debug.Log("BluetoothDeviceTest 绑定装备结果:"+result);
            });
        }
        
        /// <summary>
        /// 测试连接枪
        /// </summary>
        public void TestConnectGun()
        {
            DeviceConnector.Instance.ConnectGun();
        }
        
        /// <summary>
        /// 测试连接头
        /// </summary>
        public void TestConnectHead()
        {
            DeviceConnector.Instance.ConnectHead();
        }

        /// <summary>
        /// 测试断开枪
        /// </summary>
        public void TestDisconnectGun()
        {
            DeviceConnector.Instance.DisconnectGun();
        }
        
        /// <summary>
        /// 测试断开头
        /// </summary>
        public void TestDisconnectHead()
        {
            DeviceConnector.Instance.DisconnectHead();
        }
        
        /// <summary>
        /// 测试启动枪
        /// </summary>
        public void TestStartGun()
        {
            var @params = new GunInitMessage.GunInitParams
            {
                start = true,
                canShoot = true,
                canBeHit = true,
                team = 1,
                shortId = 1,
                color = Color.yellow
            };
            DeviceConnector.Instance.SetupGun(@params);
        }
        
        /// <summary>
        /// 测试启动头
        /// </summary>
        public void TestStartHead()
        {
            var @params = new GunInitMessage.GunInitParams
            {
                start = true,
                canShoot = true,
                canBeHit = true,
                shortId = 2,
                team = 2,
                color = Color.yellow
            };
            DeviceConnector.Instance.SetupHead(@params);
        }
        
        /// <summary>
        /// 测试关闭枪
        /// </summary>
        public void TestStopGun()
        {
            var @params = new GunInitMessage.GunInitParams
            {
                color = Color.red
            };
            DeviceConnector.Instance.SetupGun(@params);
        }
        
        /// <summary>
        /// 测试关闭头
        /// </summary>
        public void TestStopHead()
        {
            var @params = new GunInitMessage.GunInitParams
            {
                color = Color.red
            };
            DeviceConnector.Instance.SetupHead(@params);
        }
        
        /// <summary>
        /// 测试校枪
        /// </summary>
        public void TestCalibrationGun()
        {
            DeviceConnector.Instance.StartCalibrationGun(10);
        }
        
        /// <summary>
        /// 结束校枪
        /// </summary>
        public void TestStopCalibrationGun()
        {
            DeviceConnector.Instance.StopCalibrationGun();
        }

        /// <summary>
        /// 测试枪闪震
        /// </summary>
        public void TestGunLightAndVibration()
        {
            DeviceConnector.Instance.TriggerGunLightAndVibration(1, 500, 5, Color.white, 5000);
        }
        
        /// <summary>
        /// 测试头闪震
        /// </summary>
        public void TestHeadLightAndVibration()
        {
            DeviceConnector.Instance.TriggerHeadLightAndVibration(1, 500, 5, Color.white, 5000);
        }
        
        /// <summary>
        /// 测试头彩弹
        /// </summary>
        public void TestExplodeEgg()
        {
            DeviceConnector.Instance.ExplodeEgg(2);
        }
        
        /// <summary>
        /// 模拟触发被击中事件
        /// </summary>
        public void TestSimulateGunBeHit()
        {
            DeviceConnector.Instance.SimulateGunBeHit(1, 2);
        }
        
        /// <summary>
        /// 模拟触发被击中事件
        /// </summary>
        public void TestSimulateHeadBeHit()
        {
            DeviceConnector.Instance.SimulateHeadBeHit(1, 2);
        }
    }
}
