using Common.Bluetooth.Model.Read;
using Common.Bluetooth.Model.Send;
using Cysharp.Threading.Tasks;
using LetsPiu;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Send;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Event;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Device;
using LetsPiu.Define;
using LetsPiu.Main.Common.Scripts.Bluetooth;
using LetsPiu.Utils;
using Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using LetsPiu.Base.BaseScripts.Define;
using LetsPiu.Main.Common.Scripts;
using UnityEngine;
using DeviceType = LetsPiu.Define.DeviceType;
#if (UNITY_ANDROID && !UNITY_EDITOR)
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Android;
#endif
public class DeviceConnector : SingletonComponent<DeviceConnector>
{
    /// <summary>
    /// 记录当前按键的按下状态
    /// </summary>
    class KeyPressStatusInfo
    {
        public bool isKeyDown;
        /// <summary>
        /// 按下时间
        /// </summary>
        public DateTime? keyDownTime;
        /// <summary>
        /// 释放时间
        /// </summary>
        public DateTime? keyUpTime;
    }
    
    /// <summary>
    /// 释放按键的判断结果
    /// </summary>
    enum KeyPressResult
    {
        /// <summary>
        /// 无需处理
        /// </summary>
        None,
        /// <summary>
        /// 处理短按事件
        /// </summary>
        Press,
        /// <summary>
        /// 处理长按事件
        /// </summary>
        LongPress,
    }
    
    public const int DeviceTypeGun = 1;
    public const int DeviceTypeHead = 2;
    public const int DeviceTypeMatrix = 3;
    public const int DeviceTypeCoinDispenser = 4;

    /// <summary>
    /// 为 false 时关闭全部蓝牙硬件交互：不初始化、不注册 JNI、不弹系统蓝牙/权限相关界面。
    /// 比分牌等无硬件场景可保持 false；需要枪/头时再改为 true 并恢复 CheckPermissionAsync 内被 #if false 屏蔽的权限申请代码。
    /// </summary>
    public const bool BluetoothHardwareEnabled = false;

    //蓝牙是否授权
    private bool bluetoothPermissionsGranted = false;

    //需要反转E/R键的装备编号的文本文档
    private TextAsset invertedCodeAsset;
    /// <summary>
    /// 管理单发和连发的脚本
    /// </summary>
    private ShootingModeScript shootingModeScript;

    /// <summary>
    /// 上次被击中时间
    /// </summary>
    private DateTime lastShootDateTime;
    /// <summary>
    /// 上次被击中类型
    /// </summary>
    private BlueReadMessage.BlueReadMessageType lastBeShootType;

    private bool initSuccess = false;
    private bool debug = true;

    /// <summary>
    /// 夹子按钮按钮的按下状态
    /// </summary>
    private bool _PhoneClampIsPushed = false;
    private CancellationTokenSource phoneClampCts;
        
    private Dictionary<KeyCode, KeyPressStatusInfo> keyStatusInfos = new Dictionary<KeyCode, KeyPressStatusInfo>();
    
    /// <summary>
    /// 获取当前键的按下状态
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns>KeyPressStatusInfo</returns>
    private KeyPressStatusInfo GetKeyPressStatusInfo(KeyCode keyCode)
    {
        if (!keyStatusInfos.ContainsKey(keyCode))
        {
            keyStatusInfos[keyCode] = new KeyPressStatusInfo()
            {
                isKeyDown = false
            };
        }
        return keyStatusInfos[keyCode];
    }
    
    /// <summary>
    /// 更新当前键的按下状态
    /// </summary>
    /// <param name="keyCode"></param>
    /// <param name="keyStatus"></param>
    /// <returns>是否触发了长按、短按</returns>
    private KeyPressResult UpdateKeyPressStatusInfo(KeyCode keyCode, KeyStatus keyStatus)
    {
        var pressStatusInfo = GetKeyPressStatusInfo(keyCode);
        if (keyStatus == KeyStatus.KeyDown)
        {
            //按键按下了，设置状态并记录按下时间
            pressStatusInfo.isKeyDown = true;
            pressStatusInfo.keyDownTime = DateTime.Now;
            return KeyPressResult.None;
        }
        else
        {
            //检查是否0.1秒内反复弹起
            if(pressStatusInfo.keyUpTime != null && DateTime.Now - pressStatusInfo.keyUpTime < TimeSpan.FromSeconds(0.1f))
            {
                return KeyPressResult.None;
            }
            //释放了按键
            pressStatusInfo.isKeyDown = false;
            var now = DateTime.Now;
            pressStatusInfo.keyUpTime = now;
            //判断是长按还是短按
            var pressedDuration = 0d;
            if (pressStatusInfo.keyDownTime != null)
            {
                pressedDuration = (now - pressStatusInfo.keyDownTime.Value).TotalSeconds;   
            }
            else
            {
                Debug.LogWarning("DeviceConnector 没有触发KeyDown直接出发了KeyUp事件，算为短按!! pressedDuration="+pressedDuration+" keyCode="+keyCode);
            }
            if (pressedDuration > 0.6)
            {
                //触发长按事件
                return KeyPressResult.LongPress;
            }
            else
            {
                //触发短按事件
                return KeyPressResult.Press;
            }
        }
    }
    
    /// <summary>
    /// 某个按钮是否处于按下状态
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool IsKeyDown(KeyCode keyCode)
    {
        var pressStatusInfo = GetKeyPressStatusInfo(keyCode);
        return pressStatusInfo.isKeyDown;
    }
    
    // 等待发送的蓝牙数据列表
    private List<(int, string)> bluetoothGunMessageList = new List<(int, string)>();
    private List<(int, string)> bluetoothHeadMessageList = new List<(int, string)>();
    // 每隔50ms定时器
    private float bluetoothMessageSendTimer = 0f;
    private const float bluetoothMessageSendInterval = 0.06f;

    private void Awake()
    {
        //Debug.Log("DeviceConnector Awake...");
        shootingModeScript = gameObject.AddComponent<ShootingModeScript>();
    }

    private void OnEnable()
    {
        //Debug.Log("DeviceConnector OnEnable...");
    }

    private void OnDisable()
    {
        //Debug.Log("DeviceConnector OnDisable...");
    }

    private void OnDestroy()
    {
        //Debug.Log("DeviceConnector OnDestroy...");
    }
    
    /// <summary>
    /// 模拟F键按下事件
    /// </summary>
    public void EmulateFKeyDown()
    {
        EventManager.Instance.Broadcast(EventMsg.FKeyEvent, KeyEvent.KeyDown);
        EventManager.Instance.Broadcast(EventMsg.FKeyDownEvent);
        UpdateKeyPressStatusInfo(KeyCode.F, KeyStatus.KeyDown);
    }
    
    /// <summary>
    /// 模拟F键释放事件
    /// </summary>
    public void EmulateFKeyUp()
    {
        EventManager.Instance.Broadcast(EventMsg.FKeyEvent, KeyEvent.KeyUp);
        EventManager.Instance.Broadcast(EventMsg.FKeyUpEvent);
        var result = UpdateKeyPressStatusInfo(KeyCode.F, KeyStatus.KeyUp);
        if (result == KeyPressResult.Press)
        {
            EventManager.Instance.Broadcast(EventMsg.FKeyEvent, KeyEvent.KeyPress);
            EventManager.Instance.Broadcast(EventMsg.FKeyPressEvent);
        }else if (result == KeyPressResult.LongPress)
        {
            EventManager.Instance.Broadcast(EventMsg.FKeyEvent, KeyEvent.KeyLongPress);
            EventManager.Instance.Broadcast(EventMsg.FKeyLongPressEvent);
        }
    }
    
    private void Update()
    {
        //延迟一段时间发送一条蓝牙命令，防止发送过快导致消息丢失
        bluetoothMessageSendTimer += Time.deltaTime;
        if (bluetoothMessageSendTimer >= bluetoothMessageSendInterval)
        {
            bluetoothMessageSendTimer = 0f;
            if (!BluetoothHardwareEnabled)
            {
                bluetoothGunMessageList.Clear();
                bluetoothHeadMessageList.Clear();
            }
            else
            {
                //从蓝牙数据顶端弹出一个字符串并发送 bluetoothMessageList
                if (bluetoothGunMessageList.Count > 0)
                {
                    var message = bluetoothGunMessageList[0];
                    bluetoothGunMessageList.RemoveAt(0);
                    BlueTooth.SendMessageToDevice(message.Item1, message.Item2);
                }
                if (bluetoothHeadMessageList.Count > 0)
                {
                    var message = bluetoothHeadMessageList[0];
                    bluetoothHeadMessageList.RemoveAt(0);
                    BlueTooth.SendMessageToDevice(message.Item1, message.Item2);
                }
            }
        }
        
#if UNITY_EDITOR
        // 模拟按钮事件，仅在编辑器模式下有效 (F/Q/W/E/R)
if (AppStateManager.Instance.IsPlaying()) { 
        if (Input.GetKeyDown(KeyCode.F))
        {
            EmulateFKeyDown();
        } else if (Input.GetKeyUp(KeyCode.F))
        {
            EmulateFKeyUp();
        }

        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[DeviceConnector] 按下鼠标左键 模拟射击!");
            EventManager.Instance.Broadcast(EventMsg.GunShootEvent);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[DeviceConnector] 按下空格键 模拟跳跃!");

            //TODO: welkin 删除临时测试用代码

            GameObject targetObj = GameObject.Find("AR Character Input Provider");
            if (targetObj == null)
            {
                Debug.Log("[DeviceConnector] 没找到人物 不跳");
                return;
            }
            string fullTypeName = "LetsPiu.Character.Inputs.CharacterInputProvider, LetsPiu.Character";

            Type providerType = Type.GetType(fullTypeName);
            if (providerType == null)
            {
                Debug.Log("[DeviceConnector] 没找到providerType 不跳");
                return;
            }

            Component component = targetObj.GetComponent(providerType);
            if (component != null)
            {
                FieldInfo onJumpEventField = providerType.GetField("OnJumpEvent", BindingFlags.Public | BindingFlags.Instance);
                if (onJumpEventField != null)
                {
                    Action<float> onJumpEvent = (Action<float>)onJumpEventField.GetValue(component);
                    onJumpEvent?.Invoke(0.6f); // 触发跳跃事件
                    Debug.Log("[DeviceConnector] 跳跃事件已经触发");
                }

            }
            else
            {
                Debug.Log("[DeviceConnector] 没找到component 不跳");
            }

        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[DeviceConnector] 按下鼠T键 模拟装弹!");
            EventManager.Instance.Broadcast(EventMsg.GunReloadEvent);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyDown);
            EventManager.Instance.Broadcast(EventMsg.QKeyDownEvent);
            UpdateKeyPressStatusInfo(KeyCode.Q, KeyStatus.KeyDown);
        } else if (Input.GetKeyUp(KeyCode.Q))
        {
            EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyUp);
            EventManager.Instance.Broadcast(EventMsg.QKeyUpEvent);
            var result = UpdateKeyPressStatusInfo(KeyCode.Q, KeyStatus.KeyUp);
            if (result == KeyPressResult.Press)
            {
                EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyPress);
                EventManager.Instance.Broadcast(EventMsg.QKeyPressEvent);
            }else if (result == KeyPressResult.LongPress)
            {
                EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyLongPress);
                EventManager.Instance.Broadcast(EventMsg.QKeyLongPressEvent);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            EventManager.Instance.Broadcast(EventMsg.WKeyEvent, KeyEvent.KeyDown);
            EventManager.Instance.Broadcast(EventMsg.WKeyDownEvent);
            UpdateKeyPressStatusInfo(KeyCode.W, KeyStatus.KeyDown);
        } else if (Input.GetKeyUp(KeyCode.W))
        {
            EventManager.Instance.Broadcast(EventMsg.WKeyEvent, KeyEvent.KeyUp);
            EventManager.Instance.Broadcast(EventMsg.WKeyUpEvent);
            var result = UpdateKeyPressStatusInfo(KeyCode.W, KeyStatus.KeyUp);
            if (result == KeyPressResult.Press)
            {
                EventManager.Instance.Broadcast(EventMsg.WKeyEvent, KeyEvent.KeyPress);
                EventManager.Instance.Broadcast(EventMsg.WKeyPressEvent);
            }else if (result == KeyPressResult.LongPress)
            {
                EventManager.Instance.Broadcast(EventMsg.WKeyEvent, KeyEvent.KeyLongPress);
                EventManager.Instance.Broadcast(EventMsg.WKeyLongPressEvent);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EventManager.Instance.Broadcast(EventMsg.OnTabKeyDown);
        }
        
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            EventManager.Instance.Broadcast(EventMsg.OnTabKeyUp);
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Instance.Broadcast(EventMsg.EKeyEvent, KeyEvent.KeyDown);
            EventManager.Instance.Broadcast(EventMsg.EKeyDownEvent);
            UpdateKeyPressStatusInfo(KeyCode.E, KeyStatus.KeyDown);
        } else if (Input.GetKeyUp(KeyCode.E))
        {
            EventManager.Instance.Broadcast(EventMsg.EKeyEvent, KeyEvent.KeyUp);
            EventManager.Instance.Broadcast(EventMsg.EKeyUpEvent);
            var result = UpdateKeyPressStatusInfo(KeyCode.E, KeyStatus.KeyUp);
            if (result == KeyPressResult.Press)
            {
                EventManager.Instance.Broadcast(EventMsg.WKeyEvent, KeyEvent.KeyPress);
                EventManager.Instance.Broadcast(EventMsg.EKeyPressEvent);
            }else if (result == KeyPressResult.LongPress)
            {
                EventManager.Instance.Broadcast(EventMsg.EKeyEvent, KeyEvent.KeyLongPress);
                EventManager.Instance.Broadcast(EventMsg.EKeyLongPressEvent);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            EventManager.Instance.Broadcast(EventMsg.RKeyEvent, KeyEvent.KeyDown);
            EventManager.Instance.Broadcast(EventMsg.RKeyDownEvent);
            UpdateKeyPressStatusInfo(KeyCode.R, KeyStatus.KeyDown);
        } else if (Input.GetKeyUp(KeyCode.R))
        {
            EventManager.Instance.Broadcast(EventMsg.RKeyEvent, KeyEvent.KeyUp);
            EventManager.Instance.Broadcast(EventMsg.RKeyUpEvent);
            var result = UpdateKeyPressStatusInfo(KeyCode.R, KeyStatus.KeyUp);
            if (result == KeyPressResult.Press)
            {
                EventManager.Instance.Broadcast(EventMsg.RKeyEvent, KeyEvent.KeyPress);
                EventManager.Instance.Broadcast(EventMsg.RKeyPressEvent);
            }else if (result == KeyPressResult.LongPress)
            {
                EventManager.Instance.Broadcast(EventMsg.RKeyEvent, KeyEvent.KeyLongPress);   
                EventManager.Instance.Broadcast(EventMsg.RKeyLongPressEvent);
            }
        }
}
#endif
    }

    /// <summary>
    /// 异步等待初始化
    /// </summary>
    /// <returns></returns>
    public async UniTask<(bool, string)> InitAsync()
    {
        if (!BluetoothHardwareEnabled)
        {
            Debug.Log("[DeviceConnector] BluetoothHardwareEnabled=false，跳过 InitAsync（不初始化蓝牙栈）。");
            return (true, null);
        }

        Debug.Log("DeviceConnector InitAsync start...");
        var (success, errStr) = await CheckPermissionAsync();
        await UniTask.SwitchToMainThread();
        if (success)
        {
            EventManager.Instance.Broadcast(EventMsg.DevConnectorInitSuccessEvent);
        }
        else
        {
            EventManager.Instance.Broadcast(EventMsg.DevConnectorInitFailedEvent, errStr);
        }
        return (success, errStr);
    }

    public void Init()
    {
        InitAsync().Forget();
    }

    /// <summary>
    /// 初始化，申请权限、加载设备信息，以及相关的资源
    /// </summary>
    public async UniTask<(bool, string)> CheckPermissionAsync()
    {
        if (!BluetoothHardwareEnabled)
        {
            Debug.Log("[DeviceConnector] BluetoothHardwareEnabled=false，跳过 CheckPermissionAsync。");
            return (true, null);
        }

        if (initSuccess)
        {
            Debug.Log("[DeviceConnector] CheckPermissionAsync 已初始化成功!");
            EventManager.Instance.Broadcast(EventMsg.DevConnectorInitSuccessEvent);
            return (true, null);
        }

        if (GetGunInfo() != null)
        {
            Debug.Log("[DeviceConnector] CheckPermissionAsync 已绑定的抢:" + GetGunInfo().code);
        }
        else
        {
            Debug.Log("[DeviceConnector] CheckPermissionAsync 未绑定的抢!");
        }

        if (GetHeadInfo() != null)
        {
            Debug.Log("[DeviceConnector] CheckPermissionAsync 已绑定的头:" + GetHeadInfo().code);
        }
        else
        {
            Debug.Log("[DeviceConnector] CheckPermissionAsync 未绑定的头!");
        }

        BlueTooth.RegisterHandler();
        BlueTooth.GunMessageEvent = HandleGunRawMessage;
        BlueTooth.GunStatusEvent = HandleGunRawStatus;
        BlueTooth.HeadMessageEvent = HandleHeadRawMessage;
        BlueTooth.HeadStatusEvent = HandleHeadRawStatus;

        await LoadInvertedCodeAsset();
        
        
        //申请权限
        if (!bluetoothPermissionsGranted)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        // 暂时关闭：不在此流程中弹出蓝牙/定位运行时权限；需要硬件连接时再恢复下方 #if false 内代码并删除下一行。
        bluetoothPermissionsGranted = true;
#if false
        Debug.Log("DeviceConnector CheckPermissionAsync 开始申请蓝牙权限...");
        var bluetoothPermissionsRequestComplete = false;
        var permissions = new List<string>();
        var version = new AndroidJavaClass("android.os.Build$VERSION");
        var sdkInt = version.GetStatic<int>("SDK_INT");
        // 定义Android版本常量（因为不能直接使用Android的Build.VERSION_CODES）
        const int r = 30; // Android 11 (Build.VERSION_CODES.R)
        const int s = 31; // Android 12 (Build.VERSION_CODES.S)
        switch (sdkInt)
        {
            case <= r:
                permissions.Add("android.permission.BLUETOOTH");
                permissions.Add("android.permission.BLUETOOTH_ADMIN");
                break;
            case >= s:
                permissions.Add("android.permission.BLUETOOTH_CONNECT");
                permissions.Add("android.permission.BLUETOOTH_SCAN");
                permissions.Add("android.permission.BLUETOOTH_ADVERTISE");
                break;
        }
        permissions.Add("android.permission.ACCESS_COARSE_LOCATION");
        permissions.Add("android.permission.ACCESS_FINE_LOCATION");
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += (permission) =>
        {
            Debug.Log($"DeviceConnector 权限已授权 {permission} granted");
            
            //检查是否所有权限已经授权
            var grantNum = permissions.Count(Permission.HasUserAuthorizedPermission);

            if (grantNum == permissions.Count)
            {
                bluetoothPermissionsGranted = true;
            }
            bluetoothPermissionsRequestComplete = true;
        };
        callbacks.PermissionDenied += (permission) =>
        {
            Debug.LogWarning($"DeviceConnector 权限已经拒绝! {permission} denied");
            bluetoothPermissionsRequestComplete = true;
        };
        callbacks.PermissionDeniedAndDontAskAgain += (permission) =>
        {
            Debug.LogError($"DeviceConnector 禁止询问权限 {permission} permanently denied");
            bluetoothPermissionsRequestComplete = true;
        };
        Permission.RequestUserPermissions(permissions.ToArray(), callbacks);
        try
        {
            await UniTask.WaitUntil(() => bluetoothPermissionsRequestComplete).Timeout(TimeSpan.FromSeconds(20));
        }
        catch (Exception e)
        {
            bluetoothPermissionsRequestComplete = true;
            Debug.LogError("DeviceConnector CheckPermissionAsync 蓝牙权限申请失败:" + e.Message);
        }
#endif
#elif UNITY_EDITOR || UNITY_IOS
            // 编辑器模式下直接授权
            bluetoothPermissionsGranted = true;
#endif
        }
        
        if (!bluetoothPermissionsGranted)
        {
            return (false, "请授权蓝牙权限");
        }
        
        //检查蓝牙是否已打开
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var bluetoothAdapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
                {
                    var adapter = bluetoothAdapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");
                    var isEnabled = adapter != null && adapter.Call<bool>("isEnabled");
                    Debug.Log("DeviceConnector 蓝牙是否已打开: " + isEnabled);
                    if (!isEnabled)
                    {
                        // 不调用 ACTION_REQUEST_ENABLE，避免系统自动弹出「打开蓝牙」；需要硬件时由用户在系统设置中手动开启。
                        Debug.LogWarning("DeviceConnector 系统蓝牙未开启，已跳过自动弹出打开蓝牙界面；请在设置中手动打开蓝牙。");
                        return (false, "请打开系统蓝牙开关");
                    }
                }

                //Android中需要系统定位是否开启，未开启定位也无法连接蓝牙
                using (var locationManager = activity.Call<AndroidJavaObject>("getSystemService", "location"))
                {
                    bool isLocationEnabled = false;
                    if (locationManager != null)
                    {
                        isLocationEnabled = locationManager.Call<bool>("isProviderEnabled", "gps") ||
                                            locationManager.Call<bool>("isProviderEnabled", "network");
                    }
                    Debug.Log("DeviceConnector 定位是否已打开: " + isLocationEnabled);
                    if (!isLocationEnabled)
                    {
                        return (false, "请打开系统定位开关");
                    }
                }
            }
        }
    }
    catch (Exception e)
    {
        Debug.LogError("DeviceConnector 检查蓝牙状态异常: " + e.Message);
    }
#endif
        
#if UNITY_IOS && !UNITY_EDITOR
        if (!Plugins.BlueTooth._IsBluetoothEnabled())
        {
            Debug.LogError("DeviceConnector 蓝牙未打开，请在系统设置中打开蓝牙！");
            return (false, "请打开系统蓝牙开关");
        }
#endif
        initSuccess = true;
        return (true, null);
    }

    /// <summary>
    /// 读取按键反转配置文件
    /// </summary>
    private async UniTask LoadInvertedCodeAsset()
    {
        var asset = await Resources.LoadAsync<TextAsset>("inverted_codes");
        Debug.Log("DeviceConnector 反转列表==null? "+(asset == null));
        invertedCodeAsset = asset as TextAsset;
    }

    /// <summary>
    /// 解析枪的消息并下发事件
    /// </summary>
    /// <param name="status"></param>
    /// <param name="msg"></param>
    private void HandleGunRawMessage(int status, string msg)
    {
        UniTask.Post(() =>
        {
            // try
            // {
                var readMessage = BlueReadMessage.parseMessage(msg);
                if (readMessage == null)
                {
                    Debug.Log("DeviceConnector HandleGunRawMessage 解析枪消息失败(readMessage为空): " + msg);
                    return;
                }
                switch (readMessage.messageType)
                {
                    case BlueReadMessage.BlueReadMessageType.BlueReadGunMacType:
                        var gunMacMessage = (BlueGunMacMessage)readMessage;
                        Debug.Log("DeviceConnector HandleGunRawMessage 枪的mac地址: " + gunMacMessage.macAddress);
                        //广播枪的mac地址
                        EventManager.Instance.Broadcast(EventMsg.GunReadMacEvent, gunMacMessage);
                        break;
                    case BlueReadMessage.BlueReadMessageType.BlueReadClampStatusType:
                        var gunClampMessage = (BlueGunClampMessage)readMessage;
                        Debug.Log("DeviceConnector HandleGunRawMessage 读取到了手机夹子状态消息 是否已安装:"+gunClampMessage.isInstalled);
                        EventManager.Instance.Broadcast(EventMsg.GunReadClampStatusEvent, gunClampMessage);
                        //广播手机夹子事件
                        HandleGunRawMessage(status, "5A00110007"+(gunClampMessage.isInstalled?"01":"00")+"000000A5");
                        break;
                    case BlueReadMessage.BlueReadMessageType.BlueReadGunPingType:
                        var pingMessage = (BlueGunPingMessage)readMessage;
                        if (GetGunInfo() != null)
                        {
                            GetGunInfo().battery = pingMessage.power;
                        }
                        EventManager.Instance.Broadcast(EventMsg.GunPingEvent, pingMessage);
                        break;
                    case BlueReadMessage.BlueReadMessageType.BlueReadGunBeHitType:
                        if (CheckLastShootAvailable(BlueReadMessage.BlueReadMessageType.BlueReadGunBeHitType))
                        {
                            var beHitMessage = (BlueGunBeHitMessage)readMessage;
                            Debug.Log("DeviceConnector HandleGunRawMessage 击中测试 枪被击中: " + beHitMessage.shootId);
                            EventManager.Instance.Broadcast(EventMsg.GunBeHitEvent, beHitMessage);
                        }
                        else
                        {
                            Debug.LogWarning("DeviceConnector HandleGunRawMessage 枪和头同时被谁击中了!");
                        }
                        break;
                    case BlueReadMessage.BlueReadMessageType.BlueReadGunHandlerType:
                        var handlerMessage = (BlueGunHandlerMessage)readMessage;
                        var isPush = handlerMessage.isPush;
                        if (handlerMessage.isShoot)
                        {
                            EventManager.Instance.Broadcast(EventMsg.GunShootEvent);
                        }
                        else
                        {
                            switch (handlerMessage.type)
                            {
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeNone:
                                    Debug.Log("DeviceConnector 未知按钮! isPush:"+isPush);
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeChangeMap:
                                    Debug.Log("DeviceConnector W按钮! isPush:"+isPush);
                                    //W键
                                    EventManager.Instance.Broadcast(EventMsg.WKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                    EventManager.Instance.Broadcast(isPush
                                        ? EventMsg.WKeyDownEvent
                                        : EventMsg.WKeyUpEvent);
                                    if (isPush)
                                    {
                                        UpdateKeyPressStatusInfo(KeyCode.W, KeyStatus.KeyDown);
                                    }
                                    else
                                    {
                                        var result = UpdateKeyPressStatusInfo(KeyCode.W, KeyStatus.KeyUp);
                                        if (result == KeyPressResult.Press)
                                        {
                                            EventManager.Instance.Broadcast(EventMsg.WKeyPressEvent);
                                        }else if (result == KeyPressResult.LongPress)
                                        {
                                            EventManager.Instance.Broadcast(EventMsg.WKeyLongPressEvent);
                                        }
                                    }
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeSkill:
                                    //Q键
                                    if (BlueTooth.ConvertEQButton(GetGunInfo().code, invertedCodeAsset))
                                    {
                                        Debug.Log("DeviceConnector E按钮! isPush:"+isPush);
                                        EventManager.Instance.Broadcast(EventMsg.EKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                        EventManager.Instance.Broadcast(isPush
                                            ? EventMsg.EKeyDownEvent
                                            : EventMsg.EKeyUpEvent);
                                        if (isPush)
                                        {
                                            UpdateKeyPressStatusInfo(KeyCode.E, KeyStatus.KeyDown);
                                        }
                                        else
                                        {
                                            var result = UpdateKeyPressStatusInfo(KeyCode.E, KeyStatus.KeyUp);
                                            if (result == KeyPressResult.Press)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.EKeyPressEvent);
                                            }else if (result == KeyPressResult.LongPress)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.EKeyEvent, KeyEvent.KeyLongPress);
                                                EventManager.Instance.Broadcast(EventMsg.EKeyLongPressEvent);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("DeviceConnector Q按钮! isPush:"+isPush);
                                        EventManager.Instance.Broadcast(EventMsg.QKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                        EventManager.Instance.Broadcast(isPush
                                            ? EventMsg.QKeyDownEvent
                                            : EventMsg.QKeyUpEvent);
                                        if (isPush)
                                        {
                                            UpdateKeyPressStatusInfo(KeyCode.Q, KeyStatus.KeyDown);
                                        }
                                        else
                                        {
                                            var result = UpdateKeyPressStatusInfo(KeyCode.Q, KeyStatus.KeyUp);
                                            if (result == KeyPressResult.Press)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.QKeyPressEvent);
                                            }else if (result == KeyPressResult.LongPress)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyLongPress);
                                                EventManager.Instance.Broadcast(EventMsg.QKeyLongPressEvent);
                                            }
                                        }
                                    }
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeSpeak:
                                    //F键
                                    Debug.Log("DeviceConnector F按钮! isPush:"+isPush);
                                    EventManager.Instance.Broadcast(EventMsg.FKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                    EventManager.Instance.Broadcast(isPush
                                        ? EventMsg.FKeyDownEvent
                                        : EventMsg.FKeyUpEvent);
                                    if (isPush)
                                    {
                                        UpdateKeyPressStatusInfo(KeyCode.F, KeyStatus.KeyDown);
                                    }
                                    else
                                    {
                                        var result = UpdateKeyPressStatusInfo(KeyCode.F, KeyStatus.KeyUp);
                                        if (result == KeyPressResult.Press)
                                        {
                                            EventManager.Instance.Broadcast(EventMsg.FKeyEvent, KeyEvent.KeyPress);
                                            EventManager.Instance.Broadcast(EventMsg.FKeyPressEvent);
                                        }else if (result == KeyPressResult.LongPress)
                                        {
                                            EventManager.Instance.Broadcast(EventMsg.FKeyEvent, KeyEvent.KeyLongPress);
                                            EventManager.Instance.Broadcast(EventMsg.FKeyLongPressEvent);
                                        }
                                    }
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypePlayList:
                                    //R键
                                    Debug.Log("DeviceConnector R按钮! isPush:"+isPush);
                                    EventManager.Instance.Broadcast(EventMsg.RKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                    EventManager.Instance.Broadcast(isPush
                                        ? EventMsg.RKeyDownEvent
                                        : EventMsg.RKeyUpEvent);
                                    if (isPush)
                                    {
                                        UpdateKeyPressStatusInfo(KeyCode.R, KeyStatus.KeyDown);
                                    }
                                    else
                                    {
                                        var result = UpdateKeyPressStatusInfo(KeyCode.R, KeyStatus.KeyUp);
                                        if (result == KeyPressResult.Press)
                                        {
                                            EventManager.Instance.Broadcast(EventMsg.RKeyEvent, KeyEvent.KeyPress);
                                            EventManager.Instance.Broadcast(EventMsg.RKeyPressEvent);
                                        }else if (result == KeyPressResult.LongPress)
                                        {
                                            EventManager.Instance.Broadcast(EventMsg.RKeyEvent, KeyEvent.KeyLongPress);
                                            EventManager.Instance.Broadcast(EventMsg.RKeyLongPressEvent);
                                        }
                                    }
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeInspect:
                                    //E键
                                    if (BlueTooth.ConvertEQButton(GetGunInfo().code, invertedCodeAsset))
                                    {
                                        Debug.Log("DeviceConnector Q按钮! isPush:"+isPush);
                                        EventManager.Instance.Broadcast(EventMsg.QKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                        EventManager.Instance.Broadcast(isPush
                                            ? EventMsg.QKeyDownEvent
                                            : EventMsg.QKeyUpEvent);
                                        if (isPush)
                                        {
                                            UpdateKeyPressStatusInfo(KeyCode.Q, KeyStatus.KeyDown);
                                        }
                                        else
                                        {
                                            var result = UpdateKeyPressStatusInfo(KeyCode.Q, KeyStatus.KeyUp);
                                            if (result == KeyPressResult.Press)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyPress);
                                                EventManager.Instance.Broadcast(EventMsg.QKeyPressEvent);
                                            }else if (result == KeyPressResult.LongPress)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.QKeyEvent, KeyEvent.KeyLongPress);
                                                EventManager.Instance.Broadcast(EventMsg.QKeyLongPressEvent);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("DeviceConnector E按钮! isPush:"+isPush);
                                        EventManager.Instance.Broadcast(EventMsg.EKeyEvent, isPush ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                        EventManager.Instance.Broadcast(isPush
                                            ? EventMsg.EKeyDownEvent
                                            : EventMsg.EKeyUpEvent);
                                        if (isPush)
                                        {
                                            UpdateKeyPressStatusInfo(KeyCode.E, KeyStatus.KeyDown);
                                        }
                                        else
                                        {
                                            var result = UpdateKeyPressStatusInfo(KeyCode.E, KeyStatus.KeyUp);
                                            if (result == KeyPressResult.Press)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.EKeyEvent, KeyEvent.KeyPress);
                                                EventManager.Instance.Broadcast(EventMsg.EKeyPressEvent);
                                            }else if (result == KeyPressResult.LongPress)
                                            {
                                                EventManager.Instance.Broadcast(EventMsg.EKeyEvent, KeyEvent.KeyLongPress);
                                                EventManager.Instance.Broadcast(EventMsg.EKeyLongPressEvent);
                                            }
                                        }
                                    }
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeSuperior:
                                    Debug.Log("DeviceConnector 夹子按钮! isPush:"+isPush);

                                    _PhoneClampIsPushed = isPush;
                                    //最快稳定1秒后刷新界面
                                    // 取消之前的任务
                                    phoneClampCts?.Cancel();
                                    phoneClampCts = new CancellationTokenSource();
                                    UniTask.Post(async () =>
                                    {
                                        try
                                        {
                                            await UniTask.WaitForSeconds(1, cancellationToken: phoneClampCts.Token);
                                            EventManager.Instance.Broadcast(EventMsg.PhoneClampEvent, _PhoneClampIsPushed ? KeyStatus.KeyDown : KeyStatus.KeyUp);
                                            //手机夹子按键按下
                                            var newClampState = _PhoneClampIsPushed
                                                ? EventMsg.PhoneClampOpenedEvent
                                                : EventMsg.PhoneClampClosedEvent;
                                            EventManager.Instance.Broadcast(newClampState);
                                            Debug.Log("DeviceConnector 广播手机夹子状态消息: " + newClampState);
                                            //临时存储高手模式状态
                                            AppStateManager.Instance.SetVariable("PhoneClampOpenedEvent", newClampState);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.LogWarning("DeviceConnector 手机夹子抖动过快");
                                        }
                                    });
                                    break;
                                case BlueGunHandlerMessage.GunHandlerType.GunHandlerTypeRecharge:
                                    Debug.Log("DeviceConnector 广播装弹消息 GunReloadEvent! isPush:"+isPush);
                                    EventManager.Instance.Broadcast(EventMsg.GunReloadEvent);
                                    break;
                                default:
                                    Debug.LogWarning("DeviceConnector 未处理的枪蓝牙消���类型:" + msg);
                                    break;
                            }
                        }
                        break;
                    default:
                        Debug.LogWarning("DeviceConnector 未处理的枪蓝牙消息类型:" + msg);
                        break;
                }
            // }
            // catch (Exception e)
            // {
            //     Debug.LogError("DeviceConnector 枪消息解析失败:" + e.Message);
            // }
        });
    }

    /// <summary>
    /// 解析枪的状态消息，并下发事件
    /// </summary>
    /// <param name="status"></param>
    /// <param name="msg"></param>
    private void HandleGunRawStatus(int status, string msg)
    {
        if (debug)
        {
            Debug.Log("DeviceConnector HandleGunRawStatus:" + status + " msg=" + msg);
        }
        //设备搜索超时，立即手动断开，然后重新连接
        if (AppStateManager.Instance.IsPlaying() && status is 7 or 3)
        {
            UniTask.Post(async () =>
            {
                Debug.LogError("DeviceConnector 游戏中>> 枪连接超时，断开重连...");
                DisconnectGun();
                await UniTask.WaitForSeconds(0.5f);
                ConnectGun();
            });
        }
        UniTask.Post(() =>
        {
            switch (BlueTooth.gunStatus)
            {
                case BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected:
                    UniTask.Post(async () =>
                    {
                        await UniTask.Delay(500);
                        await UniTask.SwitchToMainThread();
                        EventManager.Instance.Broadcast(EventMsg.GunConnectedEvent);
                        //延迟读取2代枪的手机支架安装状态
                        UniTask.Post(async () =>
                        {
                            //连接成功后，发送读取枪的mac地址指令，多尝试几次
                            await UniTask.WaitForSeconds(1);
                            BlueTooth.SendMessageToDevice(DeviceTypeGun, ReadClampStatusMessage.Create(0).GenerateMessage());
                            await UniTask.WaitForSeconds(1);
                            BlueTooth.SendMessageToDevice(DeviceTypeGun, ReadClampStatusMessage.Create(0).GenerateMessage());
                            await UniTask.WaitForSeconds(1);
                            BlueTooth.SendMessageToDevice(DeviceTypeGun, ReadClampStatusMessage.Create(0).GenerateMessage());
                        });
                    });
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusDisConnect:
                    EventManager.Instance.Broadcast(EventMsg.GunDisconnectedEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusConnectFailed:
                    EventManager.Instance.Broadcast(EventMsg.GunConnectFailEvent);
                    EventManager.Instance.Broadcast(EventMsg.GunConnectFailEventMessage, msg);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusCutOff:
                    EventManager.Instance.Broadcast(EventMsg.GunDisconnectedEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusConnecting:
                    EventManager.Instance.Broadcast(EventMsg.GunConnectingEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusConnectOver:
                    EventManager.Instance.Broadcast(EventMsg.GunConnectFailEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusUpdate:
                    EventManager.Instance.Broadcast(EventMsg.GunFirmwareUpdateProgress, float.Parse(msg));
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusUpdateFail:
                    EventManager.Instance.Broadcast(EventMsg.GunFirmwareUpdateFailed);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusUpdated:
                    EventManager.Instance.Broadcast(EventMsg.GunFirmwareUpdateSuccess);
                    break;
                default:
                    Debug.Log("DeviceConnector 未捕获的蓝牙状态消息 gunStatus:" + BlueTooth.gunStatus);
                    break;
            }
        });
    }

    /// <summary>
    /// 接收头设备的消息，解析并下发事件
    /// </summary>
    /// <param name="status"></param>
    /// <param name="msg"></param>
    private void HandleHeadRawMessage(int status, string msg)
    {
        //设备搜索超时，立即手动断开，然后重新连接
        if (AppStateManager.Instance.IsPlaying() && (status is 7 or 3))
        {
            UniTask.Post(async () =>
            {
                Debug.LogError("DeviceConnector 游戏中>> 头连接超时，断开重连...");
                DisconnectHead();
                await UniTask.WaitForSeconds(0.5f);
                ConnectHead();
            });
        }
        UniTask.Post(() =>
        {
            try
            {
                var readMessage = BlueReadMessage.parseMessage(msg);
                if (readMessage == null) return;
                switch (readMessage.messageType)
                {
                    case BlueReadMessage.BlueReadMessageType.BlueReadHeadBeHitType:
                        if (CheckLastShootAvailable(BlueReadMessage.BlueReadMessageType.BlueReadHeadBeHitType))
                        {
                            var hitMessage = (BlueHeadBeHitMessage)readMessage;
                            Debug.Log("DeviceConnector HandleHeadRawMessage 击中测试 头被击中: " + hitMessage);
                            EventManager.Instance.Broadcast(EventMsg.HeadBeHitEvent, hitMessage);
                        }
                        else
                        {
                            Debug.LogWarning("DeviceConnector HandleHeadRawMessage 枪和头同时被谁击中了!");
                        }
                        break;
                    case BlueReadMessage.BlueReadMessageType.BlueReadHeadPingType:
                        var pingMessage = (BlueHeadPingMessage)readMessage;
                        if (GetHeadInfo() != null)
                        {
                            GetHeadInfo().battery = pingMessage.power;
                        }
                        EventManager.Instance.Broadcast(EventMsg.HeadPingEvent, pingMessage);
                        break;
                    default:
                        Debug.LogWarning("DeviceConnector 未处理的头蓝牙消息类型:" + msg);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("DeviceConnector 头消息解析失败:" + e.Message);
            }
        });
    }

    /// <summary>
    /// 接收头的状态信息，解析并下发事件
    /// </summary>
    /// <param name="status"></param>
    /// <param name="msg"></param>
    private void HandleHeadRawStatus(int status, string msg)
    {
        if (debug)
        {
            Debug.Log("DeviceConnector HandleHeadRawStatus:" + status + " msg=" + msg);
        }
        UniTask.Post(() =>
        {
            switch (BlueTooth.headStatus)
            {
                case BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected:
                    EventManager.Instance.Broadcast(EventMsg.HeadConnectedEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusDisConnect:
                    EventManager.Instance.Broadcast(EventMsg.HeadDisconnectedEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusConnectFailed:
                    EventManager.Instance.Broadcast(EventMsg.HeadConnectFailEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusCutOff:
                    EventManager.Instance.Broadcast(EventMsg.HeadDisconnectedEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusConnecting:
                    EventManager.Instance.Broadcast(EventMsg.HeadConnectingEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusConnectOver:
                    EventManager.Instance.Broadcast(EventMsg.HeadConnectFailEvent);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusUpdate:
                    EventManager.Instance.Broadcast(EventMsg.HeadFirmwareUpdateProgress, float.Parse(msg));
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusUpdateFail:
                    EventManager.Instance.Broadcast(EventMsg.HeadFirmwareUpdateFailed);
                    break;
                case BlueTooth.BlueToothStatus.BlueToothStatusUpdated:
                    EventManager.Instance.Broadcast(EventMsg.HeadFirmwareUpdateSuccess);
                    break;
                default:
                    Debug.Log("DeviceConnector 未捕获的蓝牙状态消息 headStatus:" + BlueTooth.headStatus);
                    break;
            }
        });
    }

    /// <summary>
    /// 获取绑定的枪的信息
    /// </summary>
    /// <returns></returns>
    public DeviceInfo GetGunInfo()
    {
        // return AppStateManager.Instance.GetCustomerGunInfo();
        return null;
    }

    /// <summary>
    /// 枪是否连接成功
    /// </summary>
    /// <returns></returns>
    public bool IsGunConnected()
    {
        return GetGunInfo() != null && BlueTooth.gunStatus == BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected;
    }

    /// <summary>
    /// 获��绑定的头的信息
    /// </summary>
    /// <returns></returns>
    public DeviceInfo GetHeadInfo()
    {
        // return AppStateManager.Instance.GetCustomerHeadInfo();
        return null;
    }

    /// <summary>
    /// 头是否连接成功
    /// </summary>
    /// <returns></returns>
    public bool IsHeadConnected()
    {
        return GetHeadInfo() != null && BlueTooth.headStatus == BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected;
    }
    
    /// <summary>
    /// 连接枪
    /// </summary>
    public void ConnectGun(DeviceInfo deviceInfo)
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        // BlueTooth.StartConnect(DeviceTypeGun, deviceInfo.code, 20);
        BlueTooth.StartConnect(DeviceTypeGun, deviceInfo.code);
    }

    /// <summary>
    /// 连接枪
    /// </summary>
    public void ConnectGun()
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        if (IsGunConnected())
        {
            Debug.Log("DeviceConnector 蓝牙 BlueTooth ConnectGun: 枪已连接, 无需重复连接");
            return;
        }
        if (GetGunInfo() == null)
        {
            Debug.LogError("DeviceConnector BlueTooth ConnectGun: 枪未绑定");
            EventManager.Instance.Broadcast(EventMsg.GunConnectFailEvent);
            return;
        }

        if (BlueTooth.gunStatus != BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected)
        {
            EventManager.Instance.Broadcast(EventMsg.GunConnectingEvent);
        }
        // BlueTooth.StartConnect(DeviceTypeGun, GetGunInfo().code, 20);
        BlueTooth.StartConnect(DeviceTypeGun, GetGunInfo().code);
    }

    /// <summary>
    /// 设置枪（启动、结束、被击中、灯光）
    /// </summary>
    /// <param name="params"></param>
    public void SetupGun(GunInitMessage.GunInitParams @params)
    {
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, GunInitMessage.CreateInitMessage(@params).GenerateMessage());
        EventManager.Instance.Broadcast(@params.canShoot? EventMsg.NoticeGunCanFire: EventMsg.NoticeGunNotFire);
        Debug.Log("DeviceConnector SetupGun 启动枪: " + (@params.canShoot ? "可射击" : "不可射击") + " 场次ID:"+@params.playId);
        SendMessageToDeviceToQueue(DeviceTypeGun, GunInitMessage.CreateInitMessage(@params).GenerateMessage());
    }

    public void SendMessageToDeviceToQueue(int deviceType, string message)
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        Debug.Log("DeviceConnector 发送-蓝牙消息到队列 deviceType=" + deviceType + ", message=" + message);
        UniTask.Post(async () =>
        {
            await UniTask.SwitchToMainThread();
            //确保设备已连接，才插入队列
            if (deviceType == DeviceTypeGun && IsGunConnected())
            {
                //如果队列中已经想一模一样的指令，不在添加
                if (bluetoothGunMessageList.Any(m => m.Item2 == message))
                {
                    Debug.Log("DeviceConnector SendMessageToDeviceToQueue: 队列中已存在相同的消息，不再添加 deviceType=" + deviceType + ", message=" + message);
                    return;
                }
                bluetoothGunMessageList.Add((deviceType, message));
            }
            else if (deviceType == DeviceTypeHead && IsHeadConnected())
            {
                //如果队列中已经想一模一样的指令，不在添加
                if (bluetoothHeadMessageList.Any(m => m.Item2 == message))
                {
                    Debug.Log("DeviceConnector SendMessageToDeviceToQueue: 队列中已存在相同的消息，不再添加 deviceType=" + deviceType + ", message=" + message);
                    return;
                }
                bluetoothHeadMessageList.Add((deviceType, message));
            }
            else
            {
                Debug.LogWarning($"DeviceConnector SendMessageToDeviceToQueue: 设备未连接，无法发送消息 deviceType={deviceType}, message={message}");
            }
        });
    }
    
    /// <summary>
    /// 设置枪的灯光
    /// </summary>
    /// <param name="params"></param>
    public void SetupGunColor(GunInitMessage.GunInitParams @params)
    {
        var device = GetGunInfo();
        if (device.subType == SubDeviceType.FirstGen)
        {
            //设置1代枪的颜色
            // BlueTooth.SendMessageToDevice(DeviceTypeGun, ColorMessage.CreateGunColorMessage(@params).GenerateMessage());
            SendMessageToDeviceToQueue(DeviceTypeGun, ColorMessage.CreateGunColorMessage(@params).GenerateMessage());
        }
        else
        {
            //设置2代枪的颜色
            if (@params.isMainPlayerDead)
            {
                //死亡状态
                BluetoothLightController.SetPlayerDeadLightsState(AppDataUtil.GetBlueToothLightTeamColor(@params.team));
            }
            else
            {
                //存活状态
                BluetoothLightController.SetPlayerAliveLightsState(AppDataUtil.GetBlueToothLightTeamColor(@params.team));
            }
        }
    }
    
    /// <summary>
    /// 给枪发送原始蓝牙消息
    /// </summary>
    /// <param name="message"></param>
    public void SendRawMessageToGun(string message)
    {
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, message);
        SendMessageToDeviceToQueue(DeviceTypeGun, message);
    }
    
    /// <summary>
    /// 给头发送原始蓝牙消息
    /// </summary>
    /// <param name="message"></param>
    public void SendMessageToHead(string message)
    {
        // BlueTooth.SendMessageToDevice(DeviceTypeHead, message);
        SendMessageToDeviceToQueue(DeviceTypeHead, message);
    }

    /// <summary>
    /// 设置头（启动、结束、被击中）
    /// </summary>
    /// <param name="params"></param>
    public void SetupHead(GunInitMessage.GunInitParams @params)
    {
        // BlueTooth.SendMessageToDevice(DeviceTypeHead, HeadInitMessage.CreateInitMessage(@params).GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeHead, HeadInitMessage.CreateInitMessage(@params).GenerateMessage());
    }
    
    /// <summary>
    /// 设置头的灯光
    /// </summary>
    /// <param name="params"></param>
    public void SetupHeadColor(GunInitMessage.GunInitParams @params)
    {
        // BlueTooth.SendMessageToDevice(DeviceTypeHead, ColorMessage.CreateHeadColorMessage(@params).GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeHead, ColorMessage.CreateHeadColorMessage(@params).GenerateMessage());
    }

    /// <summary>
    /// 启动校枪
    /// </summary>
    /// <param name="durationSeconds">时间长度</param>
    public void StartCalibrationGun(int durationSeconds)
    {
        var flashMessage = CheckGunMessage.CreateCheckGunMessage(durationSeconds);
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, flashMessage.GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeGun, flashMessage.GenerateMessage());
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, GunInitMessage.CreateCalibrationGunMessage().GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeGun, GunInitMessage.CreateCalibrationGunMessage().GenerateMessage());
    }

    /// <summary>
    /// 结束校枪
    /// </summary>
    public void StopCalibrationGun()
    {
        var flashMessage = CheckGunMessage.CreateCheckGunMessage(0);
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, flashMessage.GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeGun, flashMessage.GenerateMessage());
    }

    /// <summary>
    /// 控制枪闪烁和震动(指定颜色，根启动枪时设置的队伍颜色间隔闪烁) 注意2代枪颜色无效，只能控制震动
    /// <br/>单条指令
    /// <br/>使用场景：例如玩家死亡后
    /// </summary>
    /// <param name="shortId"></param>
    /// <param name="durationMillis">闪烁时���毫秒</param>
    /// <param name="blinkTimes">��烁次数</param>
    /// <param name="blinkColor">闪烁的颜色</param>
    /// <param name="vibrationDurationMillis">震动时长</param>
    /// <returns></returns>
    public void TriggerGunLightAndVibration(int shortId, int durationMillis, int blinkTimes, Color blinkColor, int vibrationDurationMillis)
    {
        //控枪闪
        var flashMessage = ColorFlashAndShakeMessage.CreateGunColor(shortId, durationMillis, blinkTimes, blinkColor, vibrationDurationMillis);
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, flashMessage.GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeGun, flashMessage.GenerateMessage());
    }
    
    //
    // /// <summary>
    // /// 控制枪震动
    // /// <br/>单条指令
    // /// <br/>使用场景：例如玩家死亡后
    // /// </summary>
    // /// <param name="shortId"></param>
    // /// <param name="vibrationDurationMillis">震动时长</param>
    // /// <returns></returns>
    // public void TriggerGunVibration(int shortId, int vibrationDurationMillis)
    // {
    //     //控枪闪
    //     var shakeMessage = GunShakeMessage.CreateGunColor(shortId, vibrationDurationMillis);
    //     BlueTooth.SendMessageToDevice(DeviceTypeGun, shakeMessage.GenerateMessage());
    // }
    
    /// <summary>
    /// 控制枪的双色闪烁(技能释放后)
    /// </summary>
    public void SetFirstGenGunDoubleColorBlink(double blinkTimeDurationSeconds)
    {
        var msg = DoubleColorMessage.CreateGunColorMessage(blinkTimeDurationSeconds).GenerateMessage();
        // BlueTooth.SendMessageToDevice(DeviceTypeGun, msg);
        SendMessageToDeviceToQueue(DeviceTypeGun, msg);
    }
    
    /// <summary>
    /// 控制头的双色闪烁(技能释放后)
    /// </summary>
    public void SetFirstGenHeadDoubleColorBlink(double blinkTimeDurationSeconds)
    {
        var msg = DoubleColorMessage.CreateHeadColorMessage(blinkTimeDurationSeconds).GenerateMessage();
        // BlueTooth.SendMessageToDevice(DeviceTypeHead, msg);
        SendMessageToDeviceToQueue(DeviceTypeHead, msg);
    }

    /// <summary>
    /// 控制头闪烁(指定颜色，根启动枪时设置的队伍颜色间隔闪烁)
    /// <br/>单条指令
    /// <br/>使用场景：例如玩家死亡后
    /// </summary>
    /// <param name="shortId"></param>
    /// <param name="durationMillis">闪烁时长毫秒</param>
    /// <param name="blinkTimes">闪烁次数</param>
    /// <param name="blinkColor">闪烁的颜色</param>
    /// <param name="vibrationDurationSeconds">震动时长秒</param>
    /// <returns></returns>
    public void TriggerHeadLightAndVibration(int shortId, int durationMillis, int blinkTimes, Color blinkColor, int vibrationDurationSeconds)
    {
        var headMessage = ColorFlashAndShakeMessage.CreateHeadColor(shortId, durationMillis, blinkTimes, blinkColor, vibrationDurationSeconds);
        // BlueTooth.SendMessageToDevice(DeviceTypeHead, headMessage.GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeHead, headMessage.GenerateMessage());
    }

    /// <summary>
    /// 引爆彩蛋(头)
    /// </summary>
    /// <param name="shortId">玩家队伍短编号</param>
    public void ExplodeEgg(int shortId)
    {
        var bangMessage = HeadBangMessage.CreateGunBangMessage(shortId);
        // BlueTooth.SendMessageToDevice(DeviceTypeHead, bangMessage.GenerateMessage());
        SendMessageToDeviceToQueue(DeviceTypeHead, bangMessage.GenerateMessage());
    }

    /// <summary>
    /// 异步连接设备
    /// </summary>
    /// <param name="deviceInfo"></param>
    /// <returns></returns>
    public async UniTask<bool> ConnectDeviceAsync(DeviceInfo deviceInfo, float timeOutSeconds = 20f)
    {
        if (!BluetoothHardwareEnabled)
        {
            Debug.Log("[DeviceConnector] BluetoothHardwareEnabled=false，ConnectDeviceAsync 忽略。");
            return false;
        }

        if (debug)
        {
            Debug.Log("DeviceConnector ConnectDeviceAsync start...");
        }

        var complete = false;
        var success = false;
        //注册事件
        Action deviceConnectedEvent = () =>
        {
            success = true;
            complete = true;
        };
        Action deviceConnectFailEvent = () =>
        {
            complete = true;
        };
        if (deviceInfo.IsGun())
        {
            DisconnectGun();
            //延迟1秒钟
            await UniTask.WaitForSeconds(0.5f);
            EventManager.Instance.AddEvent(EventMsg.GunConnectedEvent, deviceConnectedEvent);
            EventManager.Instance.AddEvent(EventMsg.GunConnectFailEvent, deviceConnectFailEvent);
            ConnectGun(deviceInfo);
        }
        else
        {
            DisconnectHead();
            //延迟1秒钟
            await UniTask.WaitForSeconds(0.5f);
            EventManager.Instance.AddEvent(EventMsg.HeadConnectedEvent, deviceConnectedEvent);
            EventManager.Instance.AddEvent(EventMsg.HeadConnectFailEvent, deviceConnectFailEvent);
            ConnectHead(deviceInfo);
        }

        try
        {
            await UniTask.WaitUntil(() => complete).Timeout(TimeSpan.FromSeconds(timeOutSeconds));
        }
        catch (Exception e)
        {
            Debug.LogError("DeviceConnector ConnectDeviceAsync 连接超时:" + e.Message);
        }
        //注销事件
        if (deviceInfo.IsGun())
        {
            EventManager.Instance.RemoveEvent(EventMsg.GunConnectedEvent, deviceConnectedEvent);
            EventManager.Instance.RemoveEvent(EventMsg.GunConnectFailEvent, deviceConnectFailEvent);
        }
        else
        {
            EventManager.Instance.RemoveEvent(EventMsg.HeadConnectedEvent, deviceConnectedEvent);
            EventManager.Instance.RemoveEvent(EventMsg.HeadConnectFailEvent, deviceConnectFailEvent);
        }
        Debug.Log("DeviceConnector ConnectDeviceAsync success:" + success);
        return success;
    }
    
    /// <summary>
    /// 连接头
    /// </summary>
    public void ConnectHead(DeviceInfo deviceInfo)
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        // BlueTooth.StartConnect(DeviceTypeHead, deviceInfo.code, 20);
        BlueTooth.StartConnect(DeviceTypeHead, deviceInfo.code);
    }

    /// <summary>
    /// 连接头
    /// </summary>
    public void ConnectHead()
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        if (IsHeadConnected())
        {
            Debug.Log("DeviceConnector 蓝牙 ConnectHead: 头已连接, 无需重复连接");
            return;
        }
        if (GetHeadInfo() == null)
        {
            Debug.LogWarning("DeviceConnector ConnectHead: 头未绑定");
            EventManager.Instance.Broadcast(EventMsg.GunConnectFailEvent);
            return;
        }
        if (BlueTooth.headStatus != BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected)
        {
            EventManager.Instance.Broadcast(EventMsg.HeadConnectingEvent);
        }
        // BlueTooth.StartConnect(DeviceTypeHead, GetHeadInfo().code, 20);
        BlueTooth.StartConnect(DeviceTypeHead, GetHeadInfo().code);
    }

    /// <summary>
    /// 断开枪的蓝牙连接
    /// </summary>
    public void DisconnectGun()
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        Debug.Log("DeviceConnector 蓝牙 DisconnectGun");
        BlueTooth.Disconnect(DeviceTypeGun);
    }

    /// <summary>
    /// 断开头的蓝牙连接
    /// </summary>
    public void DisconnectHead()
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        Debug.Log("DeviceConnector 蓝牙 DisconnectHead");
        BlueTooth.Disconnect(DeviceTypeHead);
    }

    /// <summary>
    /// 断开全部蓝牙连接
    /// </summary>
    public void Disconnect()
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        Debug.Log("DeviceConnector 蓝牙 Disconnect 断开所有...");
        DisconnectGun();
        DisconnectHead();
    }
    
    public async UniTask<string> ReadGunMacAsync()
    {
        if (!BluetoothHardwareEnabled)
        {
            return null;
        }

        Debug.Log("DeviceConnector ReadGunMacAsync start...");
        var complete = false;
        string mac = null;
        Action<BlueGunMacMessage> gunMacReadEvent = (macAddressMsg) =>
        {
            mac = macAddressMsg.macAddress;
            complete = true;
        };
        
        EventManager.Instance.AddEvent(EventMsg.GunReadMacEvent, gunMacReadEvent);
        
        //发送读取枪的mac地址指令
        BlueTooth.SendMessageToDevice(DeviceTypeGun, ReadMacMessage.CreateReadGunMac(0).GenerateMessage());
        
        try
        {
            await UniTask.WaitUntil(() => complete).Timeout(TimeSpan.FromSeconds(5));
        }
        catch (Exception e)
        {
            Debug.LogError("DeviceConnector ReadGunMacAsync 读取枪的mac地址超时:" + e.Message);
        }
        
        EventManager.Instance.RemoveEvent(EventMsg.GunReadMacEvent, gunMacReadEvent);
        
        Debug.Log("DeviceConnector ReadGunMacAsync end, mac=" + mac);
        return mac;
    }
    
    /// <summary>
    /// 绑定蓝牙设备(在服务器端验证设备合法性)
    /// </summary>
    /// <param name="qrCode"></param>
    /// <returns></returns>
    public async UniTask<(DeviceInfo, BindDeviceResult)> BindDeviceAsync(string qrCode)
    {
        Debug.Log("BindDeviceAsync qrCode=" + qrCode);
        var result = new BindDeviceResult(null, "代码已经注释了", BindDeviceErrorType.InvalidDeviceType);
        return (null, result);
//         BindDeviceResult result;
//         DeviceInfo _deviceInfo = null;
//         //初始化蓝牙
//         var (initResult, errStr) = await Instance.InitAsync();
//         if (!initResult)
//         {
//             Debug.LogError("BindDeviceAsync 蓝牙初始化失败!!");
//             result = new BindDeviceResult(null, errStr, BindDeviceErrorType.NeedLogin);
//         }
//         else if (AppStateManager.Instance.Customer == null)
//         {
//             Debug.LogError("BindDeviceAsync 请重新登录!!");
//             result = new BindDeviceResult(null, "请重新登录", BindDeviceErrorType.NeedLogin);
//         }
//         else
//         {
//             //首先调用查询装备信息(验证)接口
//             var resp = await HttpConnector.Instance.GetDeviceDetailAsync(qrCode);
//             if (resp.return_code == Constants.HTTP_RETURN_CODE_NEED_LOGIN)
//             {
//                 Debug.LogError("BindDeviceAsync 请重新登录!!");
//                 result = new BindDeviceResult(null, "请重新登录", BindDeviceErrorType.NeedLogin);
//             }
//             else
//             {
//                 if (!resp.success || resp.data == null)
//                 {
//                     Debug.LogError("BindDeviceAsync 查询装备接口返回空!!");
//                     result = new BindDeviceResult(null, resp.msg, BindDeviceErrorType.Fail);
//                 }
//                 else
//                 {
//                     var deviceInfo = resp.data;
//                     _deviceInfo = deviceInfo;
//
//                     if (deviceInfo.IsGun())
//                     {
//                         //断开已有的枪
//                         DisconnectGun();
//                         await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
//                         //必须验证连接成功!
// #if UIDEV
//                         if (GameDataManager.Instance.CanSkipBlueToothBind_ForBindGun ? true : await ConnectDeviceAsync(deviceInfo))
// #else
//                         if (await ConnectDeviceAsync(deviceInfo))
// #endif
//                         {
//                             var macAddress = "";
//                             if (deviceInfo.code.StartsWith("PN"))
//                             {
//                                 //2代枪需要读取mac地址
//                                 macAddress = await DeviceConnector.Instance.ReadGunMacAsync();  
//                             }
//                             //数据保存到服务器上
//                             var bindResp = await HttpConnector.Instance.BindDeviceAsync(qrCode, macAddress);
//                             if (bindResp.return_code == Constants.HTTP_RETURN_CODE_NEED_LOGIN)
//                             {
//                                 Debug.LogError("BindDeviceAsync BindDeviceAsync请重新登录!!");
//                                 result = new BindDeviceResult(null, "请重新登录", BindDeviceErrorType.NeedLogin);
//                             }
//                             else if (!bindResp.success || bindResp.data == null)
//                             {
//                                 Debug.LogError("BindDeviceAsync BindDeviceAsync绑定失败!!");
//                                 result = new BindDeviceResult(null, bindResp.msg, BindDeviceErrorType.Fail);
//                             }
//                             else
//                             {
//                                 AppStateManager.Instance.Customer.gunInfo = null;//强制刷新
//                                 AppStateManager.Instance.Customer.gunCode = deviceInfo.code;
//                                 AppStateManager.Instance.Customer.gunColor = deviceInfo.colour;
//                                 AppStateManager.Instance.Customer.gunType = deviceInfo.subType;
//                                 result = new BindDeviceResult(deviceInfo, bindResp.msg, BindDeviceErrorType.Success);
//                             }
//                         }
//                         else
//                         {
//                             Debug.LogError("DeviceConnector 装备绑定失败! 设备连接失败!");
//                             resp.msg = "设备连接失败，请确保设备电量充足，开关已打开，并授权蓝牙、系统定位权限!";
//                             result = new BindDeviceResult(null, resp.msg, BindDeviceErrorType.ConnectFailed);
//                         }
//                     }
//                     else if (deviceInfo.IsHead())
//                     {
//                         //断开已有的头
//                         DisconnectHead();
//                         await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
//                         //必须验证连接成功!
// #if UIDEV
//                         if (GameDataManager.Instance.CanSkipBlueToothBind_ForBindGun ? true : await ConnectDeviceAsync(deviceInfo))
// #else
//                         if (await ConnectDeviceAsync(deviceInfo))
// #endif
//                         {
//                             var bindResp = await HttpConnector.Instance.BindDeviceAsync(qrCode, "");
//                             if (bindResp.return_code == Constants.HTTP_RETURN_CODE_NEED_LOGIN)
//                             {
//                                 Debug.LogError("BindDeviceAsync BindDeviceAsync请重新登录!!");
//                                 result = new BindDeviceResult(null, "请重新登录", BindDeviceErrorType.NeedLogin);
//                             }
//                             else if (!bindResp.success || bindResp.data == null)
//                             {
//                                 Debug.LogError("BindDeviceAsync BindDeviceAsync绑定失败!!");
//                                 result = new BindDeviceResult(null, bindResp.msg, BindDeviceErrorType.Fail);
//                             }
//                             else
//                             {
//                                 AppStateManager.Instance.Customer.headInfo = null;//强制刷新
//                                 AppStateManager.Instance.Customer.headCode = deviceInfo.code;
//                                 result = new BindDeviceResult(deviceInfo, bindResp.msg, BindDeviceErrorType.Success);
//                             }
//                         }
//                         else
//                         {
//                             Debug.LogError("DeviceConnector BindDeviceAsync 装备绑定失败! 设备连接失败!");
//                             resp.msg = "设备连接失败，请确保设备电量充足，开关已打开，并授权蓝牙、系统定位权限!";
//                             result = new BindDeviceResult(null, resp.msg, BindDeviceErrorType.ConnectFailed);
//                         }
//                     }
//                     else
//                     {
//                         Debug.LogError("DeviceConnector BindDeviceAsync 错误的设备类型!");
//                         result = new BindDeviceResult(null, resp.msg, BindDeviceErrorType.InvalidDeviceType);
//                     }
//                 }
//             }
//         }
//
//         var isGun = qrCode.ToLower().Contains("/pq") || qrCode.ToLower().Contains("/pn");
//         Debug.Log("DeviceConnector 发送设备绑定结果的广播 isGun=" + isGun + " result=" + result.errType);
//         EventManager.Instance.Broadcast(isGun
//             ? EventMsg.GunBoundEvent
//             : EventMsg.HeadBoundEvent, result);
//
//         return (_deviceInfo, result);
    }
    
    /// <summary>
    /// 开始强制升级
    /// </summary>
    /// <param name="deviceType"></param>
    /// <param name="binFilePath"></param>
    public void StartForceUpload(DeviceType deviceType, string binFilePath)
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        BlueTooth.StartForceUpload((int)deviceType, "SimpleBLEPeripheral", binFilePath);
    }
    
    /// <summary>
    /// 开始常规升级
    /// </summary>
    /// <param name="deviceType"></param>
    /// <param name="binFilePath"></param>
    public void StartUpload(DeviceType deviceType, string binFilePath)
    {
        if (!BluetoothHardwareEnabled)
        {
            return;
        }

        BlueTooth.StartUpload((int)deviceType, binFilePath);
    }
    
    // 由于IOS暂无实现2代枪的原生OTA升级代码，这里注释掉。统一使用C#版本的YModem代码升级。安卓底层的OTA升级代码只保留，没有实际使用。
    // /// <summary>
    // /// 开始2代枪的OTA升级
    // /// </summary>
    // /// <param name="deviceInfo"></param>
    // /// <param name="binFilePath"></param>
    // public void StartUploadV2(DeviceInfo deviceInfo, string binFilePath)
    // {
    //     //断开原有蓝牙连接
    //     Disconnect();
    //     var deviceType = DeviceTypeGun;
    //     if (deviceInfo.IsHead())
    //     {
    //         deviceType = DeviceTypeHead;
    //     }
    //     BlueTooth.StartUploadV2(deviceType, deviceInfo.code, binFilePath);
    // }

    /// <summary>
    /// 绑定蓝牙设备(在服务器端验证设备合法性)
    /// </summary>
    /// <param name="qrCode"></param>
    /// <param name="callback"></param>
    public void BindDevice(string qrCode, Action<(DeviceInfo, BindDeviceResult)> callback = null)
    {
        StartCoroutine(BindDeviceAsync(qrCode).ToCoroutine(callback));
    }

    /// <summary>
    /// 解绑并删除本地的设备信息(断开连接)
    /// </summary>
    /// <param name="deviceInfo"></param>
    public void UnBindDevice(DeviceInfo deviceInfo)
    {
        // if (deviceInfo.IsGun())
        // {
        //     DisconnectGun();
        //     AppStateManager.Instance.Customer.gunCode = null;
        //     AppStateManager.Instance.Customer.gunColor = DeviceColor.None;
        //     AppStateManager.Instance.Customer.gunType = SubDeviceType.None;
        //     AppStateManager.Instance.Customer.gunInfo = null;
        //     AppStateManager.Instance.StoreCustomer();
        //     EventManager.Instance.Broadcast(EventMsg.GunUnboundEvent);
        // }
        // else if (deviceInfo.IsHead())
        // {
        //     DisconnectHead();
        //     AppStateManager.Instance.Customer.headCode = null;
        //     AppStateManager.Instance.Customer.headInfo = null;
        //     AppStateManager.Instance.StoreCustomer();
        //     EventManager.Instance.Broadcast(EventMsg.HeadUnboundEvent);
        // }
    }

    /// <summary>
    /// 模拟一次枪��击中事件
    /// </summary>
    /// <param name="shootId"></param>
    /// <param name="teamId"></param>
    public void SimulateGunBeHit(int shootId, int teamId)
    {
        Debug.Log("DeviceConnector 击中测试 SimulateGunBeHit shootId=" + shootId + " teamId=" + teamId);
        EventManager.Instance.Broadcast(EventMsg.GunBeHitEvent, new BlueGunBeHitMessage
        {
            messageType = BlueReadMessage.BlueReadMessageType.BlueReadGunBeHitType,
            strValue = "",
            shootId = shootId.ToString(),
            teamId = teamId.ToString(),
            skill = "0"
        });
    }

    /// <summary>
    /// 模拟一次头被击中事件
    /// </summary>
    /// <param name="shootId"></param>
    /// <param name="teamId"></param>
    public void SimulateHeadBeHit(int shootId, int teamId)
    {
        Debug.Log("DeviceConnector 击中测试 SimulateHeadBeHit shootId=" + shootId + " teamId=" + teamId);
        EventManager.Instance.Broadcast(EventMsg.HeadBeHitEvent, new BlueHeadBeHitMessage
        {
            messageType = BlueReadMessage.BlueReadMessageType.BlueReadHeadBeHitType,
            strValue = "",
            shootId = shootId.ToString(),
            teamId = teamId.ToString(),
            skill = "0"
        });
    }

    /// <summary>
    /// 检查击中是否有效(头和枪同时被击中，只触发一个)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool CheckLastShootAvailable(BlueReadMessage.BlueReadMessageType type)
    {
        if (type is BlueReadMessage.BlueReadMessageType.BlueReadHeadBeHitType or BlueReadMessage.BlueReadMessageType.BlueReadGunBeHitType)
        {
            if ((lastBeShootType != type))
            {
                var nIntervalMS = AppUtils.MillisecondsInterval(lastShootDateTime, DateTime.Now);
                // Debug.Log("nIntervalMS="+ nIntervalMS);

                if (nIntervalMS <= 300)
                {
                    lastShootDateTime = DateTime.Now;
                    lastBeShootType = type;
                    return false;
                }
            }
        }
        lastBeShootType = type;
        lastShootDateTime = DateTime.Now;
        return true;
    }
}

