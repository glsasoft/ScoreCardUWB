using System;
using System.Runtime.InteropServices;
using AOT;

#if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
using LetsPiu.BluetoothPC.Plugins.BluetoothPC;
#endif
using UnityEngine;

namespace Plugins
{
    public delegate void BlueToothNotifyEventHandler(int nStatus,string message);

    public class BlueTooth : MonoBehaviour
    {
        public static bool DEBUG = true;
        
        /**蓝牙连接状态**/
        public enum BlueToothStatus
        {
            BlueToothStatusNone = 0,        //未连接
            BlueToothStatusConnected = 1,        //已连接
            BlueToothStatusDeviceConnected = 2,  //已连接并订阅成功
            BlueToothStatusConnectFailed = 3,        //连接失败
            BlueToothStatusDisConnect = 4,        //被动断开
            BlueToothStatusCutOff = 5,        //用户手动断开蓝牙
            BlueToothStatusConnecting = 6,        //链接中
            BlueToothStatusConnectOver = 7,        //链接超时
            BlueToothStatusPowerOff = 8,       //蓝牙已关闭
            BlueToothStatusUpdate = 9,       //蓝牙升级中
            BlueToothStatusUpdated = 10,      //蓝牙升级成功
            BlueToothStatusUpdateFail = 11       //蓝牙升级失败
        };

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _StartConnect(int type, string deviceName);

    [DllImport("__Internal")]
    private static extern void _SendMessageToDevice(int type, string message);

    [DllImport("__Internal")]
    private static extern void _Disconnect(int type);

    [DllImport("__Internal")]
    private static extern void _DisconnectAll();

    [DllImport("__Internal")]
    public static extern void _RegisterDelegateToIOS(string messageId, IntPtr resultHandler);

    [DllImport("__Internal")]
    private static extern bool _CheckAudioSessionOutputVolume();

    [DllImport("__Internal")]
    private static extern void _StartUpload(int type, string filePath);

    [DllImport("__Internal")]
    private static extern void _StartForceUpload(int type, string deviceName, string filePath);

    [DllImport("__Internal")]
    public static extern bool _IsBluetoothEnabled();
    
    /// <summary>
    /// IOS回调只注册一次
    /// </summary>
    private static bool IsRegisteredIOSHandler = false;

#elif UNITY_ANDROID
    private static readonly AndroidJavaClass bleToolsClass = new AndroidJavaClass("cn.com.glsa.app.fightgame.GLSABlueToothDevice");
    private static readonly AndroidCallback androidCallback = new AndroidCallback();
    
    private class AndroidCallback : AndroidJavaProxy
    {
        public AndroidCallback() : base("cn.com.glsa.app.fightgame.GLSABlueToothDevice$Callback") { }

        private void onStatusChange(int deviceType, int deviceStatus, string messageContent)
        {
            StatusHandler(deviceType, deviceStatus, messageContent);
        }

        private void onMessage(int deviceType, int deviceStatus, string messageContent)
        {
            MessageHandler(deviceType, deviceStatus, messageContent);
        }
    }

#endif
        public static BlueToothStatus gunStatus = BlueToothStatus.BlueToothStatusNone;
        public static BlueToothStatus headStatus = BlueToothStatus.BlueToothStatusNone;
        public static BlueToothStatus matrixStatus = BlueToothStatus.BlueToothStatusNone;
        public static BlueToothStatus coinPusherStatus = BlueToothStatus.BlueToothStatusNone;
        public static BlueToothNotifyEventHandler GunStatusEvent;
        public static BlueToothNotifyEventHandler HeadStatusEvent;
        public static BlueToothNotifyEventHandler GunMessageEvent;
        public static BlueToothNotifyEventHandler HeadMessageEvent;
    
        /**
     * 蓝牙矩阵状态
     */
        public static event BlueToothNotifyEventHandler MatrixStatusEvent;
        /**
     * 蓝牙矩阵消息
     */
        public static event BlueToothNotifyEventHandler MatrixMessageEvent;
    
        /**
     * 蓝牙吐币机状态
     */
        public static event BlueToothNotifyEventHandler CoinPusherStatusEvent;
        /**
     * 蓝牙吐币机消息
     */
        public static event BlueToothNotifyEventHandler CoinPusherMessageEvent;
    
        // Starts lookup for some bonjour registered service inside specified domain
        public static void StartConnect(int type, string deviceName)
        {

            if(DEBUG) Debug.Log("BlueTooth 联接蓝牙:type=" + type + "    deviceName = " + deviceName);
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _StartConnect(type, deviceName);
        }
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            //链接蓝牙设备，搜索超时时间为10秒
            var timeoutMillis = 20*1000;
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            bleToolsClass.CallStatic("startConnect", activity, type, deviceName, timeoutMillis, androidCallback);   
        }
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            #if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            RegisterHandler();
            BluetoothPC.StartConnect(type, deviceName);
            #endif
        }
#elif UNITY_EDITOR_WIN
            RegisterHandler();
#if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.BluetoothPC.StartConnect(type, deviceName);
#endif
#endif
        }

        public static void SendMessageToDevice(int type, string message)
        {

            if(DEBUG) Debug.Log("BlueTooth 蓝牙发送消息:type=" + type + "    message = " + message);
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            
            _SendMessageToDevice(type, message);
        }
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            #if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.SendMessageToDevice(type, message);
            #endif
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            bleToolsClass.CallStatic("sendMessageToDevice", type, message);
        }
#elif UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.BluetoothPC.SendMessageToDevice(type, message);
#endif

        }

        public static void Disconnect(int type)
        {
#if UNITY_IOS
        _Disconnect(type);
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            #if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.DisconnectAll();
            #endif
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            bleToolsClass.CallStatic("disconnect", type);
        }
#elif UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.BluetoothPC.DisconnectAll();
#endif
        }

        public static void DisconnectAll()
        {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _DisconnectAll();
        }
        
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            #if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.DisconnectAll();
            #endif
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            bleToolsClass.CallStatic("disconnectAll");
        }
#elif UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.BluetoothPC.DisconnectAll();
#endif
        }
        
        /// <summary>
        /// PIU2固件升级代码
        /// </summary>
        /// <param name="bleDeviceName"></param>
        /// <param name="filePath"></param>
        public static void StartUploadV2(int type, string bleDeviceName, string filePath) {
            if(DEBUG) Debug.Log("BlueTooth 蓝牙发送升级消息:bleDeviceName=" + bleDeviceName + "   filePath = " + filePath);
            #if UNITY_EDITOR
            Debug.LogError("BlueTooth StartUploadV2 在编辑器中不支持，请在手机上运行！");
            return;
            #endif
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // _StartUploadV2(type, bleDeviceName, filePath);
        }
#elif UNITY_ANDROID
            if (Application.platform != RuntimePlatform.Android) return;
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            bleToolsClass.CallStatic("startUploadV2", activity, type, bleDeviceName, filePath, androidCallback);
#endif
        }

        public static void StartUpload(int type, string filePath) {
            if(DEBUG) Debug.Log("BlueTooth 蓝牙发送升级消息:type=" + type + "    filePath = " + filePath);
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _StartUpload(type, filePath);
        }
#elif UNITY_ANDROID
        if (Application.platform != RuntimePlatform.Android) return;
        var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        bleToolsClass.CallStatic("startUpload", activity, type, filePath, androidCallback);
#endif
        }
    
        /// <summary>
        /// 强制升级
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deviceName"></param>
        /// <param name="filePath"></param>
        public static void StartForceUpload(int type, string deviceName, string filePath) {
            if(DEBUG) Debug.Log("BlueTooth 蓝牙发送升级消息:type=" + type + "    filePath = " + filePath);
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {

            _StartForceUpload(type, deviceName, filePath);
        }
#elif UNITY_ANDROID
        if (Application.platform != RuntimePlatform.Android) return;
        var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        bleToolsClass.CallStatic("startForceUpload", activity, type, deviceName, filePath, androidCallback);
#endif
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MessageResultHandler(int nDeviceType, int nDeviceStatus, string messageContent);

        [MonoPInvokeCallback(typeof(MessageResultHandler))]
        public static void StatusHandler(int nDeviceType, int nDeviceStatus, string messageContent)
        {
            if(DEBUG) Debug.Log($"BlueTooth StatusHandler  Args: {nDeviceType}, {nDeviceStatus}, Message: {messageContent}");
            if (nDeviceType == 1)
            {
                gunStatus = (BlueToothStatus)nDeviceStatus;
                if(DEBUG) Debug.Log("BlueTooth gunStatus="+ gunStatus.ToString());
                GunStatusEvent?.Invoke(nDeviceStatus, messageContent);
            }
            else if (nDeviceType == 2)
            {
                headStatus = (BlueToothStatus)nDeviceStatus;
                if(DEBUG) Debug.Log("BlueTooth headStatus=" + gunStatus.ToString());
                HeadStatusEvent?.Invoke(nDeviceStatus, messageContent);
            }else if (nDeviceType == 3)
            {
                //矩阵
                matrixStatus = (BlueToothStatus)nDeviceStatus;
                if(DEBUG) Debug.Log("BlueTooth matrixStatus=" + matrixStatus);
                MatrixStatusEvent?.Invoke(nDeviceStatus, messageContent);
            }else if (nDeviceType == 4)
            {
                //吐币机
                coinPusherStatus = (BlueToothStatus)nDeviceStatus;
                if(DEBUG) Debug.Log("BlueTooth coinPusherStatus=" + coinPusherStatus);
                CoinPusherStatusEvent?.Invoke(nDeviceStatus, messageContent);
            }
        }

        [MonoPInvokeCallback(typeof(MessageResultHandler))]
        public static void MessageHandler(int nDeviceType, int nDeviceStatus, string messageContent)
        {
            if(DEBUG) Debug.Log($"BlueTooth MessageHandler 接收到原始消息  Args: {nDeviceType}, {nDeviceStatus}, Message: {messageContent}");
            if (nDeviceType == 1)
            {
                if(DEBUG) Debug.Log("BlueTooth 回调GunMessageEvent. gunStatus=" + gunStatus.ToString());
                GunMessageEvent?.Invoke(nDeviceStatus, messageContent);
            }
            else if (nDeviceType == 2)
            {
                if(DEBUG) Debug.Log("BlueTooth 回调HeadMessageEvent. headStatus=" + headStatus.ToString());
                HeadMessageEvent?.Invoke(nDeviceStatus, messageContent);
            }else if (nDeviceType == 3)
            {
                //矩阵
                matrixStatus = (BlueToothStatus)nDeviceStatus;
                if(DEBUG) Debug.Log("BlueTooth 回调MatrixMessageEvent matrixStatus=" + matrixStatus);
                MatrixMessageEvent?.Invoke(nDeviceStatus, messageContent);
            }else if (nDeviceType == 4)
            {
                //吐币机
                coinPusherStatus = (BlueToothStatus)nDeviceStatus;
                if(DEBUG) Debug.Log("BlueTooth 回调CoinPusherMessageEvent coinPusherStatus=" + coinPusherStatus);
                CoinPusherMessageEvent?.Invoke(nDeviceStatus, messageContent);
            }
        }

        public static void RegisterHandler()
        {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer && !IsRegisteredIOSHandler)
        {
            // 在iOS手机环境下运行
            Debug.Log("DeviceConnector Init 蓝牙初始化...11");
            MessageResultHandler statusHandler = StatusHandler;
            Debug.Log("DeviceConnector Init 蓝牙初始化...12");
            IntPtr fp = Marshal.GetFunctionPointerForDelegate(statusHandler);
            Debug.Log("DeviceConnector Init 蓝牙初始化...13");
            _RegisterDelegateToIOS("1", fp);
            Debug.Log("DeviceConnector Init 蓝牙初始化...14");
            MessageResultHandler messageHandler = MessageHandler;
            Debug.Log("DeviceConnector Init 蓝牙初始化...15");
            fp = Marshal.GetFunctionPointerForDelegate(messageHandler);
            Debug.Log("DeviceConnector Init 蓝牙初始化...16");
            _RegisterDelegateToIOS("2", fp);
            Debug.Log("DeviceConnector Init 蓝牙初始化...17");
            IsRegisteredIOSHandler = true;
        }
#elif UNITY_ANDROID
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            #if UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.SetStatusAction(StatusHandler);
            BluetoothPC.SetMessageAction(MessageHandler);
            #endif
        }
#elif UNITY_EDITOR_WIN && ENABLE_BLUETOOTH_PC
            BluetoothPC.BluetoothPC.SetStatusAction(StatusHandler);
            BluetoothPC.BluetoothPC.SetMessageAction(MessageHandler);
#endif
        }

#if UNITY_IOS
    public static bool CheckAudioSessionOutputVolume()
    {

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return _CheckAudioSessionOutputVolume();
        }
        return false;

    }
#endif

        // public static string ColorName(string color)
        // {
        //     return color switch
        //     {
        //         "1" => "曜石黑",
        //         "2" => "皓月白",
        //         "3" => "粉晶色",
        //         "4" => "蒂芙尼蓝",
        //         "5" => "中国红",
        //         _ => "皓月白"
        //     };
        // }

        /// <summary>
        /// 枪的E、Q键是否要反过来
        /// </summary>
        /// <param name="code"></param>
        /// <param name="invertedCodeAsset"></param>
        /// <returns></returns>
        public static bool ConvertEQButton(string code, TextAsset invertedCodeAsset)
        {
            if (invertedCodeAsset == null) return false;
            try
            {
                // PQ00000621-PQ00002020 E/R键的消息反过来
                if (int.Parse(code.Replace("PQ", "")) is >= 621 and <= 2020)
                {
                    return true;
                }
            
                if (invertedCodeAsset != null && invertedCodeAsset.text.Contains(code))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                if(DEBUG) Debug.LogError("convertEQButton:"+e.Message);
            }
            return false;
        }
    }
}

