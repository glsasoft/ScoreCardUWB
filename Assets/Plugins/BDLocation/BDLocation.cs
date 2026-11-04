using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Plugins;
using UnityEngine;
namespace Plugins
{
    public class BDLocation : MonoBehaviour
    {
        private static bool IsResisteredIOSHandler = false;
#if UNITY_ANDROID
        private class AndroidBaiduLocationCallback : AndroidJavaProxy
        {
            public AndroidBaiduLocationCallback() : base("cn.com.glsa.app.fightgame.MainActivity$BaiduLocationCallback") { }

            private void onStartError(string message)
            {
                Debug.Log("百度定位初始化失败");
            }

            private void onReceiveLocation(double latitude, double longitude)
            {
                LocationMessageHandler(0, 0,longitude + "-" + latitude);
            }

            private void onReceiveDegree(double degree)
            {
                HeadingMessageHandler(0, 0, degree.ToString(CultureInfo.InvariantCulture));
            }
        }
#endif
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _StartLocation();

    [DllImport("__Internal")]
    private static extern void _StopLocation();
#endif
        public static event Action<string, string> LocationMessageAction = null;
        public static event Action<string> HeadingMessageAction = null;
    
        public static void StartLocation()
        {
            Debug.Log("开始定位");
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _StartLocation();
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (Application.platform == RuntimePlatform.Android)
            {
                new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity")
                    .Call("startLocation", 1100, 5.0f, new AndroidBaiduLocationCallback());
            }
#elif UNITY_ANDROID && UNITY_EDITOR
        
#endif

        }

        public static void StopLocation()
        {

            Debug.Log("结束定位");
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _StopLocation();
        }
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity")
                    .Call("stopLocation");   
            }
#endif

        }

        [AOT.MonoPInvokeCallback(typeof(BlueTooth.MessageResultHandler))]
        public static void LocationMessageHandler(int nType, int nStatus, string messageContent)
        {
            Debug.Log($"LocationMessageHandler  Args: {nType}, {nStatus}, Message: {messageContent}");
            string[] arr = messageContent.Split("-");
            if (arr.Length == 2)
            {
                if (LocationMessageAction != null)
                {
                    // Debug.Log($"LocationMessageHandler !=null arr[0]={arr[0]},arr[1]={arr[1]}");
                    LocationMessageAction(arr[0], arr[1]);
                }
            }
            else
            {
                Debug.Log($"LocationMessageHandler arr.Length != 2,不处理");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(BlueTooth.MessageResultHandler))]
        public static void HeadingMessageHandler(int nType, int nStatus, string messageContent)
        {
            Debug.Log($"HeadingMessageHandler  Args: {nType}, {nStatus}, Message: {messageContent}");
            if (LocationMessageAction != null)
            {
                HeadingMessageAction(messageContent);
            }

        }

        public static void RegisterHandler()
        {
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer && !IsResisteredIOSHandler)
            {
                // 在iOS手机环境下运行
                Debug.Log("BDLocation RegisterHandler 注册定位回调函数locationMessageHandler");
                BlueTooth.MessageResultHandler locationMessageHandler = LocationMessageHandler;
                var fp = Marshal.GetFunctionPointerForDelegate(locationMessageHandler);
                BlueTooth._RegisterDelegateToIOS("3", fp);
                Debug.Log("BDLocation RegisterHandler 注册定位回调函数headingMessageHandler");
                BlueTooth.MessageResultHandler headingMessageHandler = HeadingMessageHandler;
                fp = Marshal.GetFunctionPointerForDelegate(headingMessageHandler);
                BlueTooth._RegisterDelegateToIOS("4", fp);
                Debug.Log("BDLocation RegisterHandler 注册定位回调函数结束");
                IsResisteredIOSHandler = true;
            }
#endif
        }
    }
}