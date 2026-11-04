#if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LetsPiu.BluetoothPC.Plugins.BluetoothPC
{
    
    /// <summary>
    /// PC端蓝牙实现
    /// </summary>
    public class BluetoothPC : MonoBehaviour
    {
        public static bool DEBUG = true;
        private const string SecondGunHeadServiceUuid = "0000ffe0-0000-1000-8000-00805f9b34fb";
        private const string SecondGunHeadWriteUuid = "0000ffe1-0000-1000-8000-00805f9b34fb";
        private const string SecondGunHeadNotifyUuid = "0000ffe2-0000-1000-8000-00805f9b34fb";
        // 1代枪的uuid
        private const string GunHeadServiceUuid = "0000fff0-0000-1000-8000-00805f9b34fb";
        private const string GunHeadWriteUuid = "0000fff6-0000-1000-8000-00805f9b34fb";
        private const string GunHeadNotifyUuid = "0000fff6-0000-1000-8000-00805f9b34fb";  
        private const string MatrixServiceUuid = "0000ffe0-0000-1000-8000-00805f9b34fb";
        private const string MatrixWriteUuid = "0000ffe1-0000-1000-8000-00805f9b34fb";
        private const string MatrixNotifyUuid = "0000ffe1-0000-1000-8000-00805f9b34fb";
        private const string CoinPusherServiceUuid = "0000ffb0-0000-1000-8000-00805f9b34fb";
        private const string CoinPusherWriteUuid = "0000ffb1-0000-1000-8000-00805f9b34fb";
        private const string CoinPusherNotifyUuid = "0000ffb2-0000-1000-8000-00805f9b34fb";

        
        private static readonly Dictionary<int, string[]> DevicesUUID = new Dictionary<int, string[]>();
        
#if UNITY_EDITOR
        
        [InitializeOnLoadMethod]
        static void OnEditorLoad()
        {
            if(DEBUG) Debug.LogWarning("BluetoothPC OnEditorLoad>>>>>>>>>>>>>> !!!!!");
            DestroyBle();
            EditorApplication.update += () =>
            {
            };
            
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    // 编辑模式退出时执行的代码
                    DestroyBle();
                }
                else if (state == PlayModeStateChange.EnteredEditMode)
                {
                    // 进入编辑模式时执行的代码
                    DestroyBle();
                }
            };
            
            AssemblyReloadEvents.beforeAssemblyReload += DestroyBle;
            AssemblyReloadEvents.afterAssemblyReload += OpenBluetooth;
        }
#endif
        
        // 声明Rust函数
        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void register_callback(
            Callback messageCallback,
            Callback statusCallback
        );
        
        // 声明Rust函数
        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void start_connect(int type_id, string name, string service_uuid, string write_uuid, string notify_uuid);

        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void send_message_to_device(int type_id, string msg);

        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void disconnect(int type_id);

        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void disconnect_all();
        
        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void destroy();
        
        [DllImport("BLEPlug.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void init_log();

        // 定义回调函数的委托类型
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Callback(int deviceType, int deviceStatus, IntPtr strPtr);
        
        // 实现回调函数
        private static void MessageCallback(int deviceType, int deviceStatus, IntPtr messageContentPtr)
        {
            var message = Marshal.PtrToStringAnsi(messageContentPtr);
            if(DEBUG) Debug.Log("BluetoothPC MessageCallback "+deviceType+" status="+deviceStatus+" "+message);
            UniTask.Post(() =>
            {
                messageHandler?.Invoke(deviceType, deviceStatus, message);
            });
        }

        private static void StatusCallback(int deviceType, int deviceStatus, IntPtr messageContentPtr)
        {
            var message = Marshal.PtrToStringAnsi(messageContentPtr);
            if(DEBUG) Debug.Log("BluetoothPC StatusCallback "+deviceType+" status="+deviceStatus+" "+message);
            UniTask.Post(() =>
            {
                statusHandler?.Invoke(deviceType, deviceStatus, message);
            });
        }

        public delegate void BlueToothPCStatusHandler(int deviceType, int deviceStatus,string messageContent);
    
        // private const bool ENABLED = true;
        private static BlueToothPCStatusHandler statusHandler;
        private static BlueToothPCStatusHandler messageHandler;
        private static readonly Dictionary<int, string> Devices = new Dictionary<int, string>();

        private static void RegisterCallback()
        {
            register_callback(MessageCallback, StatusCallback);
        }
    
#if UNITY_EDITOR_WIN || UNITY_ANDROID

        private static void DestroyBle()
        {
            try
            {
                if(DEBUG) Debug.LogWarning("BluetoothPC destroy BLE..");
                destroy();
                if(DEBUG) Debug.LogWarning("BluetoothPC destroy BLE OK..");
            }
            catch (Exception e)
            {
                if(DEBUG) Debug.LogError("BluetoothPC destroy() error! "+e.Message);
            }
        }

        private static void InitUuid(string gunName)
        {
            DevicesUUID[1] = new[] { GunHeadServiceUuid, GunHeadWriteUuid, GunHeadNotifyUuid };
            DevicesUUID[2] = new[] { GunHeadServiceUuid, GunHeadWriteUuid, GunHeadNotifyUuid };
            DevicesUUID[3] = new[] { MatrixServiceUuid, MatrixWriteUuid, MatrixNotifyUuid };
            DevicesUUID[4] = new[] { CoinPusherServiceUuid, CoinPusherWriteUuid, CoinPusherNotifyUuid };
            
            //第2代枪的uuid变了
            if (gunName != null && gunName.StartsWith("PN"))
            {
                DevicesUUID[1] = new[] { SecondGunHeadServiceUuid, SecondGunHeadWriteUuid, SecondGunHeadNotifyUuid };
            }
        }

        private static void OpenBluetooth()
        {

            init_log();

            InitUuid(null);
            
            RegisterCallback();
            
        if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                return;
            }
            var list = Devices.ToList();
            foreach (var keyValuePair in list)
            {
                StartConnect(keyValuePair.Key, keyValuePair.Value);
            }
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += (state) =>
            {
                if(DEBUG) Debug.Log("BluetoothPC EditorApplication.playModeStateChanged:"+state);
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    DisconnectAll();
                    if(DEBUG) Debug.Log("BluetoothPC Editor暂停，断开所有PC蓝牙连接！");
                }
            };
            #endif
        }
        
        public static void StartConnect(int type, string deviceName)
        {
            
            InitUuid(deviceName);
            
            if(DEBUG) Debug.Log("BluetoothPC StartConnect "+deviceName);
            Devices[type] = deviceName;
            var uuid = DevicesUUID[type];
            if (deviceName.StartsWith("BLE SPP"))
            {
                if(DEBUG) Debug.Log("BluetoothPC 连接BLE SPP设备");
                uuid = new[] { "0000fee0-0000-1000-8000-00805f9b34fb", "0000fee2-0000-1000-8000-00805f9b34fb", "0000fee1-0000-1000-8000-00805f9b34fb" };
            }
            start_connect(type, deviceName, uuid[0], uuid[1], uuid[2]);
        }

        public static void SendMessageToDevice(int type, string message)
        {
            send_message_to_device(type, message);
        }

        public static string Disconnect(int type, string deviceName)
        {
            disconnect(type);
            return "success";
        }

        public static string DisconnectAll()
        {
            disconnect_all();
            return "success";
        }
    
        /// <summary>
        /// 设置设备连接状态回调函数
        /// </summary>
        /// <param name="action"></param>
        public static void SetStatusAction(BlueToothPCStatusHandler action)
        {
            statusHandler = action;
            RegisterCallback();
        }
        public static void SetMessageAction(BlueToothPCStatusHandler action)
        {
            messageHandler = action;
            RegisterCallback();
        }
#endif
    }
}

#endif