using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 串口通信统一入口（MonoBehaviour 单例）。
///
/// 自动根据运行平台选择底层实现：
///   • Android 设备  → <see cref="AndroidSerialPort"/>（P/Invoke libc termios）
///   • PC / Editor   → <see cref="PCSerialPort"/>（System.IO.Ports）
///
/// 使用示例：
/// <code>
///   SerialPortManager.Instance.OnDataReceived += bytes => Debug.Log(BitConverter.ToString(bytes));
///   SerialPortManager.Instance.Open();                         // Android，默认 DefaultAndroidPort
///   SerialPortManager.Instance.Open("COM5");                 // PC
///   SerialPortManager.Instance.Send(new byte[]{0x5A,0xA5});
///   SerialPortManager.Instance.Close();
/// </code>
/// </summary>
public class SerialPortManager : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════════
    //  常量
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Android 串口路径兜底常量。实际默认路径由 <see cref="BoardProfile.SerialPort"/>
    /// 按开发板型号自动给出；本常量仅作历史回退与字段初值用。
    /// </summary>
    public const string DefaultAndroidPort = "/dev/ttyS0";

    /// <summary>默认波特率。</summary>
    public const int DefaultBaudRate = 115200;

    // ════════════════════════════════════════════════════════════════════
    //  单例
    // ════════════════════════════════════════════════════════════════════

    private static SerialPortManager _instance;

    /// <summary>
    /// 全局单例，首次访问时自动创建 GameObject。
    /// </summary>
    public static SerialPortManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[SerialPortManager]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SerialPortManager>();
            return _instance;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开事件（均在 Unity 主线程触发）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>收到原始字节数据时触发。</summary>
    public event Action<byte[]> OnDataReceived;

    /// <summary>发生错误时触发，参数为错误描述。</summary>
    public event Action<string> OnError;

    // ════════════════════════════════════════════════════════════════════
    //  状态
    // ════════════════════════════════════════════════════════════════════

    /// <summary>串口当前是否已打开。</summary>
    public bool IsOpen => _impl?.IsOpen ?? false;

    /// <summary>当前使用的底层实现（只读，用于调试）。</summary>
    public ISerialPort Implementation => _impl;

    /// <summary>当前已打开（或最近一次尝试打开）的串口路径。</summary>
    public string CurrentPortPath => _currentPortPath;

    /// <summary>当前已打开（或最近一次尝试打开）的波特率。</summary>
    public int CurrentBaudRate => _currentBaudRate;

    private ISerialPort _impl;
    private string _currentPortPath;
    private int _currentBaudRate;

    // ════════════════════════════════════════════════════════════════════
    //  Unity 生命周期
    // ════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Close();
        if (_instance == this) _instance = null;
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开方法
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 打开串口。
    /// Android 不传 path 时自动使用 <see cref="DefaultAndroidPort"/>。
    /// </summary>
    /// <param name="path">端口路径（Android 默认 <see cref="DefaultAndroidPort"/>，PC: "COM5"）</param>
    /// <param name="baudRate">波特率，默认 115200</param>
    public bool Open(string path = null, int baudRate = DefaultBaudRate)
    {
        if (IsOpen)
        {
            Debug.LogWarning("[SerialMgr] 串口已打开，请先调用 Close()");
            return false;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        path ??= BoardProfile.SerialPort;
        Debug.Log($"[SerialMgr] 准备打开串口: {path} @ {baudRate} " +
                  $"(板型默认={BoardProfile.DefaultSerialPort}" +
                  $"{(BoardProfile.HasSerialPortOverride ? $", 手动覆盖={BoardProfile.SerialPortOverride}" : "")})");
        // path 以 "usb" 开头 → 走 USB Host API（CP210x），否则走 /dev/ttyS* 内核串口
        ISerialPort impl = (path != null && path.StartsWith("usb"))
            ? gameObject.AddComponent<UsbHostSerialPort>()
            : gameObject.AddComponent<AndroidSerialPort>();
#else
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[SerialMgr] PC 模式必须指定 COM 端口");
            return false;
        }
        ISerialPort impl = gameObject.AddComponent<PCSerialPort>();
#endif

        impl.OnDataReceived += data =>
        {
            if (data != null && data.Length > 0)
                Debug.Log($"[SerialMgr] RX ← {path} ({data.Length}字节): {BytesToHex(data)}");
            OnDataReceived?.Invoke(data);
        };
        impl.OnError        += err  => OnError?.Invoke(err);
        _impl = impl;
        _currentPortPath = path;
        _currentBaudRate = baudRate;

        bool ok = _impl.Open(path, baudRate);
        if (!ok)
        {
            if (impl is MonoBehaviour mb) Destroy(mb);
            _impl = null;
            Debug.LogError($"[SerialMgr] 打开串口失败: {path} @ {baudRate}");
        }
        else
        {
            Debug.Log($"[SerialMgr] 串口已打开，当前使用: {path} @ {baudRate} baud " +
                      $"(实现={impl.GetType().Name})");
        }
        return ok;
    }

    /// <summary>关闭串口，销毁底层组件。</summary>
    public void Close()
    {
        if (_impl == null) return;
        Debug.Log($"[SerialMgr] 关闭串口: {_currentPortPath}");
        _impl.Close();
        if (_impl is MonoBehaviour mono && mono != null) Destroy(mono);
        _impl = null;
        _currentPortPath = null;
        _currentBaudRate = 0;
    }

    /// <summary>发送原始字节数据。线程安全。</summary>
    public bool Send(byte[] data)
    {
        if (!IsOpen)
        {
            Debug.LogWarning("[SerialMgr] 串口未打开");
            return false;
        }
        if (data != null && data.Length > 0)
            Debug.Log($"[SerialMgr] TX → {_currentPortPath} ({data.Length}字节): {BytesToHex(data)}");
        return _impl.Send(data);
    }

    /// <summary>发送十六进制字符串，如 "5A 01 00 A5"。线程安全。</summary>
    public bool SendHex(string hex) => _impl?.SendHex(hex) ?? false;

    // ════════════════════════════════════════════════════════════════════
    //  静态工具
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取可用串口列表。
    /// PC 返回系统 COM 口；Android 扫描 /dev/ttyS* 并附带当前生效串口与 usb-cp210x 选项。
    /// </summary>
    public static string[] GetAvailablePorts()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return BuildAndroidPortList();
#else
        return PCSerialPort.GetPortNames();
#endif
    }

    /// <summary>扫描 Android 系统中存在的 /dev/ttyS* 设备节点。</summary>
    public static string[] EnumerateAndroidTtySPorts()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var found = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            foreach (string path in Directory.GetFiles("/dev", "ttyS*"))
            {
                if (!string.IsNullOrEmpty(path)) found.Add(NormalizeDevPath(path));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SerialMgr] 扫描 /dev/ttyS* 失败（将逐个探测）: {e.Message}");
        }

        if (found.Count == 0)
        {
            for (int i = 0; i <= 31; i++)
            {
                string p = $"/dev/ttyS{i}";
                if (File.Exists(p)) found.Add(p);
            }
        }

        var list = new List<string>(found);
        list.Sort(StringComparer.Ordinal);
        Debug.Log($"[SerialMgr] 扫描到 {list.Count} 个 ttyS 串口: {string.Join(", ", list)}");
        return list.ToArray();
#else
        return Array.Empty<string>();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static string[] BuildAndroidPortList()
    {
        var ports = new List<string>();
        void Add(string p)
        {
            if (!string.IsNullOrEmpty(p) && !ports.Contains(p)) ports.Add(p);
        }

        foreach (string p in EnumerateAndroidTtySPorts()) Add(p);
        Add(BoardProfile.DefaultSerialPort);
        Add(BoardProfile.SerialPort);
        Add("usb-cp210x");
        ports.Sort(StringComparer.Ordinal);
        if (ports.Contains("usb-cp210x"))
        {
            ports.Remove("usb-cp210x");
            ports.Add("usb-cp210x");
        }
        return ports.ToArray();
    }

    static string NormalizeDevPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        return path.StartsWith("/dev/") ? path : $"/dev/{Path.GetFileName(path)}";
    }
#endif

    static string BytesToHex(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return "";
        var sb = new StringBuilder(bytes.Length * 3);
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(bytes[i].ToString("X2"));
        }
        return sb.ToString();
    }
}
