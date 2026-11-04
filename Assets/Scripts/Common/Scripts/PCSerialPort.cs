// PCSerialPort 仅在 PC / Editor 平台编译，Android 设备上不编译此文件。
#if !UNITY_ANDROID || UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

/// <summary>
/// PC / Editor 串口通信实现（System.IO.Ports.SerialPort）。
/// 请通过 <see cref="SerialPortManager"/> 访问，不要直接实例化。
/// </summary>
public class PCSerialPort : MonoBehaviour, ISerialPort
{
    // ════════════════════════════════════════════════════════════════════
    //  ISerialPort 实现
    // ════════════════════════════════════════════════════════════════════

    public bool IsOpen => _port != null && _port.IsOpen;

    public event Action<byte[]> OnDataReceived;
    public event Action<string> OnError;

    private SerialPort    _port;
    private Thread        _readThread;
    private volatile bool _isRunning;

    private readonly object _writeLock = new();
    private readonly Queue<(byte[] payload, bool isError)> _mainQueue = new();
    private readonly object _queueLock = new();

    // ════════════════════════════════════════════════════════════════════
    //  Unity 生命周期
    // ════════════════════════════════════════════════════════════════════

    private void Update()
    {
        lock (_queueLock)
        {
            while (_mainQueue.Count > 0)
            {
                var (payload, isError) = _mainQueue.Dequeue();
                if (isError)
                    OnError?.Invoke(System.Text.Encoding.UTF8.GetString(payload));
                else
                    OnDataReceived?.Invoke(payload);
            }
        }
    }

    private void OnDestroy() => Close();

    // ════════════════════════════════════════════════════════════════════
    //  公开方法
    // ════════════════════════════════════════════════════════════════════

    public bool Open(string path, int baudRate)
    {
        if (IsOpen)
        {
            Debug.LogWarning($"[Serial-PC] 串口已打开，请先 Close()");
            return false;
        }

        try
        {
            _port = new SerialPort(path, baudRate, Parity.None, 8, StopBits.One)
            {
                // 100ms 读超时：无数据时 Read() 抛 TimeoutException 返回，
                // 让读线程每 100ms 检查一次 _isRunning，可干净退出。
                ReadTimeout  = 100,
                WriteTimeout = 500
            };
            _port.Open();
        }
        catch (Exception ex)
        {
            PostError($"打开 {path} 失败: {ex.Message}");
            _port = null;
            return false;
        }

        _isRunning = true;
        _readThread = new Thread(ReadLoop)
        {
            IsBackground = true,
            Name = "PCSerialReadThread"
        };
        _readThread.Start();

        Debug.Log($"[Serial-PC] 串口已打开：{path} @ {baudRate} baud");
        return true;
    }

    public void Close()
    {
        if (!IsOpen) return;

        _isRunning = false;
        _readThread?.Join(500);
        _readThread = null;

        try { _port?.Close(); } catch { /* ignore */ }
        _port = null;

        Debug.Log("[Serial-PC] 串口已关闭");
    }

    public bool Send(byte[] data)
    {
        if (!IsOpen || data == null || data.Length == 0) return false;

        lock (_writeLock)
        {
            try
            {
                _port.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Serial-PC] 发送失败: {ex.Message}");
                return false;
            }
        }
    }

    public bool SendHex(string hex)
    {
        hex = hex?.Replace(" ", "").Replace("-", "") ?? "";
        if (hex.Length == 0 || hex.Length % 2 != 0)
        {
            Debug.LogWarning($"[Serial-PC] 非法 HEX 字符串: '{hex}'");
            return false;
        }
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return Send(bytes);
    }

    // ════════════════════════════════════════════════════════════════════
    //  静态工具
    // ════════════════════════════════════════════════════════════════════

    /// <summary>获取系统所有可用串口名（如 "COM3"、"COM5"）。</summary>
    public static string[] GetPortNames()
    {
        try   { return SerialPort.GetPortNames(); }
        catch { return Array.Empty<string>(); }
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有：读线程
    // ════════════════════════════════════════════════════════════════════

    private void ReadLoop()
    {
        var buf = new byte[256];

        while (_isRunning)
        {
            if (_port == null || !_port.IsOpen) break;

            try
            {
                int n = _port.Read(buf, 0, buf.Length);
                if (n > 0)
                {
                    var received = new byte[n];
                    Buffer.BlockCopy(buf, 0, received, 0, n);
                    EnqueueData(received);
                }
            }
            catch (TimeoutException)
            {
                // 正常读超时，继续循环检查 _isRunning
            }
            catch (Exception ex)
            {
                if (!_isRunning) break; // 正常关闭时触发，忽略
                PostError($"读取异常: {ex.Message}");
                break;
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有：队列工具
    // ════════════════════════════════════════════════════════════════════

    private void EnqueueData(byte[] data)
    {
        lock (_queueLock) _mainQueue.Enqueue((data, false));
    }

    private void PostError(string msg)
    {
        Debug.LogError($"[Serial-PC] {msg}");
        lock (_queueLock)
            _mainQueue.Enqueue((System.Text.Encoding.UTF8.GetBytes(msg), true));
    }
}

#endif // !UNITY_ANDROID || UNITY_EDITOR
