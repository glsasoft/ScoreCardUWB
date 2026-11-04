using System;
using UnityEngine;

/// <summary>
/// 基于 Android USB Host API 的串口实现（对接 CP210x / CP2102N）。
///
/// 与 <see cref="AndroidSerialPort"/>（走 /dev/ttyS* 内核串口节点）不同，本类通过
/// Java 桥接 <c>UsbSerialBridge</c> 直接用 USB Host API 跟 CP210x 芯片通信：
///   • 不需要 /dev/ttyUSB 节点、不需要内核 cp210x 驱动、不需要 root
///   • 能正确下发波特率（通用 usbserial 驱动做不到这点）
///
/// 收发约定：
///   • 接收：Java 端后台读线程累积数据，本类在 Update() 轮询 pollReceived() 取走。
///   • 发送：直接调用 Java write()。
///
/// 仅 Android 真机生效；Editor/PC 下为空操作。
/// </summary>
public class UsbHostSerialPort : MonoBehaviour, ISerialPort
{
    // CP2102N 的 VID/PID
    private const int DefaultVid = 0x10C4;
    private const int DefaultPid = 0xEA60;

    public event Action<byte[]> OnDataReceived;
    public event Action<string> OnError;

    public bool IsOpen => _open;
    private bool _open;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _bridge;
    private bool _opening;
    private int _baudRate = 115200;
#endif

    public bool Open(string path = null, int baudRate = 115200)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_bridge != null)
        {
            Debug.LogWarning("[UsbSerial] 已存在桥接实例，请先 Close()");
            return false;
        }

        _baudRate = baudRate > 0 ? baudRate : 115200;

        try
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            _bridge = new AndroidJavaObject("com.glsa.mqttscorecard5s.unity.UsbSerialBridge", activity);

            bool started = _bridge.Call<bool>("requestOpen", DefaultVid, DefaultPid, _baudRate);
            if (!started)
            {
                string err = _bridge.Call<string>("lastError");
                PostError($"未能发起 USB 串口打开: {err}");
                _bridge.Dispose();
                _bridge = null;
                return false;
            }

            // 权限/连接是异步的，标记进行中，由 Update() 轮询 isOpen() 翻转 _open
            _opening = true;
            Debug.Log($"[UsbSerial] 已发起打开 CP210x(10C4:EA60) @ {_baudRate} baud，等待权限/连接…");
            return true;
        }
        catch (Exception e)
        {
            PostError($"打开 USB 串口异常: {e.Message}");
            _bridge = null;
            return false;
        }
#else
        Debug.Log($"[UsbSerial] 非 Android 真机，跳过打开：{path} @ {baudRate}");
        return false;
#endif
    }

    public void Close()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _opening = false;
        _open = false;
        if (_bridge != null)
        {
            try { _bridge.Call("close"); }
            catch (Exception e) { Debug.LogWarning($"[UsbSerial] close 异常: {e.Message}"); }
            _bridge.Dispose();
            _bridge = null;
        }
        Debug.Log("[UsbSerial] 串口已关闭");
#endif
    }

    public bool Send(byte[] data)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!_open || _bridge == null || data == null || data.Length == 0) return false;
        try
        {
            // Java byte[] 对应 Unity 的 sbyte[]；用 byte[] 会触发 JNI 封送 obsolete 告警
            var sdata = new sbyte[data.Length];
            Buffer.BlockCopy(data, 0, sdata, 0, data.Length);
            int written = _bridge.Call<int>("write", sdata);
            if (written < 0)
            {
                Debug.LogError("[UsbSerial] write() 失败");
                return false;
            }
            Debug.Log($"[UsbSerial] write() 成功：{written} 字节");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UsbSerial] write 异常: {e.Message}");
            return false;
        }
#else
        return false;
#endif
    }

    public bool SendHex(string hex)
    {
        hex = hex?.Replace(" ", "").Replace("-", "") ?? "";
        if (hex.Length == 0 || hex.Length % 2 != 0)
        {
            Debug.LogWarning($"[UsbSerial] SendHex: 非法 HEX 字符串 '{hex}'");
            return false;
        }
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return Send(bytes);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void Update()
    {
        if (_bridge == null) return;

        // 等待异步连接完成
        if (_opening)
        {
            try
            {
                if (_bridge.Call<bool>("isOpen"))
                {
                    _opening = false;
                    _open = true;
                    Debug.Log($"[UsbSerial] CP210x 连接成功 @ {_baudRate} baud");
                }
                else
                {
                    string err = _bridge.Call<string>("lastError");
                    if (!string.IsNullOrEmpty(err))
                    {
                        _opening = false;
                        PostError($"USB 串口连接失败: {err}");
                    }
                }
            }
            catch (Exception e)
            {
                _opening = false;
                PostError($"轮询连接状态异常: {e.Message}");
            }
        }

        if (!_open) return;

        // 轮询接收数据。Java byte[] 对应 Unity 的 sbyte[]，避免 byte[] 封送 obsolete 告警刷屏
        try
        {
            sbyte[] rx = _bridge.Call<sbyte[]>("pollReceived");
            if (rx != null && rx.Length > 0)
            {
                var data = new byte[rx.Length];
                Buffer.BlockCopy(rx, 0, data, 0, rx.Length);
                OnDataReceived?.Invoke(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[UsbSerial] pollReceived 异常: {e.Message}");
        }
    }

    private void OnDestroy() => Close();
#endif

    private void PostError(string msg)
    {
        Debug.LogError($"[UsbSerial] {msg}");
        OnError?.Invoke(msg);
    }
}
