using System;
using UnityEngine;
using LetsPiu;

namespace LetsPiu.Main.Common.Scripts
{
    // 新的枚举，表示统一的遥控按键类型
    public enum RemoteKey
    {
        Unknown = 0,
        Up,
        Down,
        Left,
        Right,
        Center,
        Back
    }
    
    public class OnKeyListenerProxy : AndroidJavaProxy
    {
        private readonly Func<int, AndroidJavaObject, bool> _onKey;

        public OnKeyListenerProxy(Func<int, AndroidJavaObject, bool> onKeyCallback)
            : base("android.view.View$OnKeyListener")
        {
            _onKey = onKeyCallback;
        }

        // Java 签名：boolean onKey(View v, int keyCode, KeyEvent event)
        // 注意：方法名使用小写 'onKey' 是必须的，用于匹配 Java 接口方法签名（AndroidJavaProxy 要求），
        // 这可能会被 C# 命名约定的静态分析器标记为警告，但不能改为 'OnKey'。
        public bool onKey(AndroidJavaObject view, int keyCode, AndroidJavaObject keyEvent)
        {
            try
            {
                // 额外日志：尝试读取 KeyEvent 的 action（若可用）以便调试
                try
                {
                    int action = keyEvent.Call<int>("getAction");
                    Debug.Log($"TVRemoteKeyListener:onKey action={action} keyCode={keyCode}");
                }
                catch (Exception)
                {
                    // 忽略读取失败，继续正常流程
                }
                return _onKey != null && _onKey.Invoke(keyCode, keyEvent);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
    
    public class TVRemoteKeyListener : SingletonComponent<TVRemoteKeyListener>
    {
        // 兼容旧接口（直接传 int keyCode）
        public event Action<int> OnRemoteKeyDown;
        // 新的接口，传递枚举类型，便于上层使用
        public event Action<RemoteKey> OnRemoteKey;

        AndroidJavaObject _decorView;
        OnKeyListenerProxy _proxy;

        // 模拟用的 Android KeyEvent 常量（用于编辑器/PC）
#if UNITY_EDITOR || UNITY_STANDALONE
        private const int SimKeycodeDpadUp = 19;     // KeyEvent.KEYCODE_DPAD_UP
        private const int SimKeycodeDpadDown = 20;   // KeyEvent.KEYCODE_DPAD_DOWN
        private const int SimKeycodeDpadLeft = 21;   // KeyEvent.KEYCODE_DPAD_LEFT
        private const int SimKeycodeDpadRight = 22;  // KeyEvent.KEYCODE_DPAD_RIGHT
        private const int SimKeycodeDpadCenter = 23; // KeyEvent.KEYCODE_DPAD_CENTER
        private const int SimKeycodeBack = 4;        // KeyEvent.KEYCODE_BACK
#endif

        void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var window = activity.Call<AndroidJavaObject>("getWindow");
        _decorView = window.Call<AndroidJavaObject>("getDecorView");

        // 保证可接收按键
        _decorView.Call("setFocusableInTouchMode", true);
        _decorView.Call("requestFocus");

        // 获取 KeyEvent 常量（可改为固定数值）
        var keyEventClass = new AndroidJavaClass("android.view.KeyEvent");
        int KEYCODE_DPAD_UP = keyEventClass.GetStatic<int>("KEYCODE_DPAD_UP");
        int KEYCODE_DPAD_DOWN = keyEventClass.GetStatic<int>("KEYCODE_DPAD_DOWN");
        int KEYCODE_DPAD_LEFT = keyEventClass.GetStatic<int>("KEYCODE_DPAD_LEFT");
        int KEYCODE_DPAD_RIGHT = keyEventClass.GetStatic<int>("KEYCODE_DPAD_RIGHT");
        int KEYCODE_BACK = keyEventClass.GetStatic<int>("KEYCODE_BACK");
        int KEYCODE_DPAD_CENTER = keyEventClass.GetStatic<int>("KEYCODE_DPAD_CENTER");

        _proxy = new OnKeyListenerProxy((keyCode, keyEvent) =>
        {
            // 监听方向键和确认键，拦截处理
            if (keyCode == KEYCODE_DPAD_UP || keyCode == KEYCODE_DPAD_DOWN || keyCode == KEYCODE_DPAD_CENTER || keyCode == KEYCODE_DPAD_LEFT || keyCode == KEYCODE_DPAD_RIGHT)
            {
                Debug.Log("RemoteKeyAndroidListener 收到按键: " + keyCode);
                RaiseRemoteKeyFromInt(keyCode);
                return true; // 拦截事件
            }
            // 返回键只通知，不拦截，让系统继续处理（比如退出 Activity）
            if (keyCode == KEYCODE_BACK)
            {
                Debug.Log("RemoteKeyAndroidListener 收到 返回键: " + keyCode);
                RaiseRemoteKeyFromInt(keyCode);
                return false; // 不拦截，交还系统处理
            }
            // 未处��则返回 false 让系统继续处理
            return false;
        });

        _decorView.Call("setOnKeyListener", _proxy);
#endif
        }

        // 在编辑器或 PC 平台上，用键盘方向键和回车模拟 DPAD 事件
        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("TVRemoteKeyListener 模拟 DPAD_UP");
                RaiseRemoteKeyFromInt(SimKeycodeDpadUp);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("TVRemoteKeyListener 模拟 DPAD_DOWN");
                RaiseRemoteKeyFromInt(SimKeycodeDpadDown);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("TVRemoteKeyListener 模拟 DPAD_LEFT");
                RaiseRemoteKeyFromInt(SimKeycodeDpadLeft);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Debug.Log("TVRemoteKeyListener 模拟 DPAD_RIGHT");
                RaiseRemoteKeyFromInt(SimKeycodeDpadRight);
            }
            // 模拟返回键（Escape）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("TVRemoteKeyListener 模拟 BACK");
                RaiseRemoteKeyFromInt(SimKeycodeBack);
            }
            // 将回车/空格视为确认键（DPAD_CENTER）
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("TVRemoteKeyListener 模拟 DPAD_CENTER");
                RaiseRemoteKeyFromInt(SimKeycodeDpadCenter);
            }
#endif
        }

        void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_decorView != null)
        {
            _decorView.Call("setOnKeyListener", (AndroidJavaObject)null);
            _decorView = null;
        }
#endif
        }

        // 将整型 Android keyCode 映射为 RemoteKey 并触发事件
        private void RaiseRemoteKeyFromInt(int keyCode)
        {
            try
            {
                // 统一日志：记录来源平台与原始 keyCode
                string source;
                if (Application.platform == RuntimePlatform.Android)
                    source = "Android";
                else if (Application.isEditor)
                    source = "Editor";
                else
                    source = "Standalone";

                Debug.Log($"TVRemoteKeyListener: Received keyCode={keyCode} from {source}");

                // 先触发兼容的 int 事件
                OnRemoteKeyDown?.Invoke(keyCode);

                // 广播原始 keyCode 事件到全局 EventManager
                try
                {
                    EventManager.Instance.Broadcast(EventMsg.RemoteKeyCodeEvent, keyCode);
                    Debug.Log($"TVRemoteKeyListener: Broadcasted EventMsg.RemoteKeyCodeEvent with keyCode={keyCode}");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                // 再尝试映射为 RemoteKey 并触发新的事件
                if (TryMapKeyCode(keyCode, out RemoteKey mapped))
                {
                    Debug.Log($"TVRemoteKeyListener: Mapped keyCode={keyCode} to RemoteKey={mapped}");

                    OnRemoteKey?.Invoke(mapped);

                    // 广播枚举事件到全局 EventManager
                    try
                    {
                        EventManager.Instance.Broadcast(EventMsg.RemoteKeyEvent, mapped);
                        Debug.Log($"TVRemoteKeyListener: Broadcasted EventMsg.RemoteKeyEvent with RemoteKey={mapped}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private bool TryMapKeyCode(int keyCode, out RemoteKey remoteKey)
        {
            switch (keyCode)
            {
                case 19: // KeyEvent.KEYCODE_DPAD_UP
                    remoteKey = RemoteKey.Up; return true;
                case 20: // KeyEvent.KEYCODE_DPAD_DOWN
                    remoteKey = RemoteKey.Down; return true;
                case 21: // KeyEvent.KEYCODE_DPAD_LEFT
                    remoteKey = RemoteKey.Left; return true;
                case 22: // KeyEvent.KEYCODE_DPAD_RIGHT
                    remoteKey = RemoteKey.Right; return true;
                case 23: // KeyEvent.KEYCODE_DPAD_CENTER
                    remoteKey = RemoteKey.Center; return true;
                case 4:  // KeyEvent.KEYCODE_BACK
                    remoteKey = RemoteKey.Back; return true;
                default:
                    remoteKey = RemoteKey.Unknown; return false;
            }
        }
    }
}
