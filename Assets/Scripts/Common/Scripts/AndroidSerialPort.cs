using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

/// <summary>
/// Android 串口通信管理器 — 纯 C# 实现，零 Java 代码。
///
/// 实现原理：
///   • P/Invoke 调用 Android bionic libc.so，执行与 SerialPort.c (Cedric Priscal) 完全相同的
///     termios 配置流程（cfmakeraw 手动展开 → cfsetispeed/ospeed → tcsetattr）。
///   • 读写均直接 P/Invoke libc read()/write()，不经过 FileStream。
///     （FileStream 包装字符设备 fd 时，IL2CPP 会对其调用 lseek()，
///       字符设备返回 ESPIPE 后 IL2CPP 内部出现空指针崩溃，故放弃 FileStream。）
///   • 后台读线程 + Update() 主线程队列，保证 Unity 事件在主线程触发。
///
/// 依赖条件：
///   • AndroidManifest.xml 已声明 android:sharedUserId="android.uid.system"
///   • APK 以系统签名安装（否则无法访问 /dev/ttyS* 设备节点）
///
/// 使用示例：
/// <code>
///   var serial = gameObject.AddComponent&lt;AndroidSerialPort&gt;();
///   serial.OnDataReceived += bytes => Debug.Log($"收到 {bytes.Length} 字节");
///   serial.Open(SerialPortManager.DefaultAndroidPort, SerialPortManager.DefaultBaudRate);
///   serial.Send(new byte[] { 0x5A, 0x01, 0x00, 0xA5 });
/// </code>
/// </summary>
public class AndroidSerialPort : MonoBehaviour, ISerialPort
{
    // ════════════════════════════════════════════════════════════════════
    //  P/Invoke — Android bionic libc.so
    //
    //  termios 结构体（Android bionic ARM32 / ARM64 完全相同，44 字节）：
    //    uint c_iflag, c_oflag, c_cflag, c_lflag  → 16 字节
    //    byte c_line                               →  1 字节
    //    byte c_cc[19]  (NCCS=19)                 → 19 字节
    //    uint c_ispeed, c_ospeed  (bionic 扩展)   →  8 字节
    //  合计 44 字节，ARM32 与 ARM64 无差异，无需分平台。
    // ════════════════════════════════════════════════════════════════════

    [StructLayout(LayoutKind.Sequential)]
    private struct Termios
    {
        public uint c_iflag;
        public uint c_oflag;
        public uint c_cflag;
        public uint c_lflag;
        public byte c_line;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public byte[] c_cc;
        public uint c_ispeed;
        public uint c_ospeed;
    }

    // open() flags（Linux ARM）
    private const int O_RDWR   = 2;
    private const int O_NOCTTY = 256; // 0400 octal

    // tcsetattr 时机
    private const int TCSANOW = 0;

    // ioctl 请求码（Linux ARM generic，asm-generic/ioctls.h）
    private const int TCFLSH    = 0x540B;
    private const int TCIOFLUSH = 2;

    // c_cflag 位
    private const uint CREAD  = 0x00000080U;
    private const uint CLOCAL = 0x00000800U;

    // cfmakeraw 展开所需位掩码（Linux ARM，termios.h）
    private const uint IGNBRK = 0x001U;
    private const uint BRKINT = 0x002U;
    private const uint PARMRK = 0x008U;
    private const uint ISTRIP = 0x020U;
    private const uint INLCR  = 0x040U;
    private const uint IGNCR  = 0x080U;
    private const uint ICRNL  = 0x100U;
    private const uint IXON   = 0x400U;
    private const uint OPOST  = 0x001U;
    private const uint ECHO   = 0x008U;
    private const uint ECHONL = 0x040U;
    private const uint ICANON = 0x002U;
    private const uint ISIG   = 0x001U;
    private const uint IEXTEN = 0x8000U;
    private const uint CSIZE  = 0x030U;
    private const uint PARENB = 0x100U;
    private const uint CS8    = 0x030U;

    // c_cc 索引（Linux ARM）
    private const int VTIME = 5;
    private const int VMIN  = 6;

    [DllImport("libc", SetLastError = true, EntryPoint = "open")]
    private static extern int LibcOpen(string path, int flags);

    [DllImport("libc", SetLastError = true, EntryPoint = "close")]
    private static extern int LibcClose(int fd);

    // 直接读写 fd，不经过 FileStream，避免 IL2CPP 对字符设备调 lseek 崩溃
    [DllImport("libc", SetLastError = true, EntryPoint = "read")]
    private static extern int LibcRead(int fd, byte[] buf, int count);

    [DllImport("libc", SetLastError = true, EntryPoint = "write")]
    private static extern int LibcWrite(int fd, byte[] buf, int count);

    [DllImport("libc", SetLastError = true, EntryPoint = "tcgetattr")]
    private static extern int TcGetAttr(int fd, ref Termios termios);

    [DllImport("libc", SetLastError = true, EntryPoint = "tcsetattr")]
    private static extern int TcSetAttr(int fd, int actions, ref Termios termios);

    [DllImport("libc", SetLastError = true, EntryPoint = "cfsetispeed")]
    private static extern int CfSetISpeed(ref Termios termios, uint speed);

    [DllImport("libc", SetLastError = true, EntryPoint = "cfsetospeed")]
    private static extern int CfSetOSpeed(ref Termios termios, uint speed);

    [DllImport("libc", SetLastError = true, EntryPoint = "ioctl")]
    private static extern int Ioctl(int fd, int request, int arg);

    [DllImport("libc", EntryPoint = "usleep")]
    private static extern int Usleep(uint microseconds);

    // 等待所有已写入内核缓冲区的数据物理发送完成（等同于 Java OutputStream.flush()）
    [DllImport("libc", SetLastError = true, EntryPoint = "tcdrain")]
    private static extern int TcDrain(int fd);

    // ════════════════════════════════════════════════════════════════════
    //  Inspector 参数
    // ════════════════════════════════════════════════════════════════════

    [Header("串口配置")]
    [Tooltip("串口设备路径")]
    [SerializeField] private string _portPath = SerialPortManager.DefaultAndroidPort;

    [Tooltip("波特率")]
    [SerializeField] private int _baudRate = 115200;

    [Tooltip("Start() 时自动打开串口（通过 SerialPortManager 使用时保持 false）")]
    [SerializeField] private bool _openOnStart = false;

    // ════════════════════════════════════════════════════════════════════
    //  公开事件（均在 Unity 主线程触发）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>收到原始字节数据时触发（Unity 主线程）。</summary>
    public event Action<byte[]> OnDataReceived;

    /// <summary>发生错误时触发，参数为错误描述（Unity 主线程）。</summary>
    public event Action<string> OnError;

    // ════════════════════════════════════════════════════════════════════
    //  状态
    // ════════════════════════════════════════════════════════════════════

    /// <summary>串口当前是否已打开。</summary>
    public bool IsOpen => _fd >= 0;

    private int  _fd = -1;
    private Thread _readThread;
    private volatile bool _isRunning;

    private readonly object _writeLock = new();

    private readonly System.Collections.Generic.Queue<(byte[] payload, bool isError)>
        _mainQueue = new();
    private readonly object _queueLock = new();

    // ════════════════════════════════════════════════════════════════════
    //  Unity 生命周期
    // ════════════════════════════════════════════════════════════════════

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_openOnStart) Open(_portPath, _baudRate);
#endif
    }

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

    /// <summary>打开串口并启动读线程。</summary>
    public bool Open(string path = null, int baudRate = 0)
    {
        if (IsOpen)
        {
            Debug.LogWarning("[Serial] 串口已打开，请先调用 Close()");
            return false;
        }

        if (!string.IsNullOrEmpty(path)) _portPath = path;
        if (baudRate > 0)               _baudRate = baudRate;

#if UNITY_ANDROID && !UNITY_EDITOR
        return OpenInternal();
#else
        Debug.Log($"[Serial] Editor 模式，跳过真实打开：{_portPath} @ {_baudRate}");
        return true;
#endif
    }

    /// <summary>关闭串口并等待读线程退出（最多 500ms）。</summary>
    public void Close()
    {
        if (!IsOpen) return;

        _isRunning = false;
        _readThread?.Join(500);
        _readThread = null;

        if (_fd >= 0)
        {
            LibcClose(_fd);
            _fd = -1;
        }

        Debug.Log("[Serial] 串口已关闭");
    }

    /// <summary>发送原始字节数据。线程安全。</summary>
    public bool Send(byte[] data)
    {
        if (!IsOpen || data == null || data.Length == 0) return false;

        lock (_writeLock)
        {
            int written = LibcWrite(_fd, data, data.Length);
            int errno   = Marshal.GetLastWin32Error(); // 立即读取，避免被后续调用覆盖

            if (written < 0)
            {
                Debug.LogError($"[Serial] write() 失败，errno={errno}，fd={_fd}");
                return false;
            }

            if (written == 0)
            {
                Debug.LogWarning($"[Serial] write() 写入 0 字节（errno={errno}），数据未实际发出");
                return false;
            }

            if (written < data.Length)
                Debug.LogWarning($"[Serial] write() 部分写入：请求={data.Length}，实际写入={written}");
            else
                Debug.Log($"[Serial] write() 成功：{written} 字节已进入内核缓冲，fd={_fd}");

            // 等待内核缓冲区数据物理发送完成，对应 Java OutputStream.flush()。
            // 注意：generic usbserial 驱动(/dev/ttyUSB*)上 tcdrain() 可能永久阻塞，
            // 在主线程调用会导致 App 卡死；故 USB 转串口跳过 drain，硬串口 ttyS* 照常。
            if (_portPath == null || !_portPath.Contains("ttyUSB"))
                TcDrain(_fd);

            return true;
        }
    }

    /// <summary>发送十六进制字符串，如 "5A 01 00 A5"。线程安全。</summary>
    public bool SendHex(string hex)
    {
        hex = hex?.Replace(" ", "").Replace("-", "") ?? "";
        if (hex.Length == 0 || hex.Length % 2 != 0)
        {
            Debug.LogWarning($"[Serial] SendHex: 非法 HEX 字符串 '{hex}'");
            return false;
        }
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return Send(bytes);
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有：打开与配置（复刻 SerialPort.c 流程）
    // ════════════════════════════════════════════════════════════════════

    private bool OpenInternal()
    {
        _fd = LibcOpen(_portPath, O_RDWR | O_NOCTTY);
        if (_fd < 0)
        {
            PostError($"open({_portPath}) 失败，errno={Marshal.GetLastWin32Error()}。" +
                      "请确认设备路径正确且 App 以系统签名安装。");
            return false;
        }

        // 与 SerialPort.c 一致：open 后 100ms 清缓冲，再等 100ms
        Usleep(100_000);
        Ioctl(_fd, TCFLSH, TCIOFLUSH);
        Usleep(100_000);

        if (!ConfigureTermios())
        {
            LibcClose(_fd);
            _fd = -1;
            return false;
        }

        _isRunning = true;
        _readThread = new Thread(ReadLoop)
        {
            IsBackground = true,
            Name = "SerialReadThread"
        };
        _readThread.Start();

        Debug.Log($"[Serial] 串口已打开：{_portPath} @ {_baudRate} baud，fd={_fd}");
        return true;
    }

    /// <summary>
    /// 配置 termios。
    /// cfmakeraw 手动展开（等价于 bionic cfmakeraw），避免 P/Invoke 该函数时
    /// 因 struct 传递方式差异导致问题。
    /// </summary>
    private bool ConfigureTermios()
    {
        var cfg = new Termios { c_cc = new byte[19] };

        if (TcGetAttr(_fd, ref cfg) != 0)
        {
            PostError($"tcgetattr 失败，errno={Marshal.GetLastWin32Error()}");
            return false;
        }

        // cfmakeraw 展开（等价于 bionic/glibc cfmakeraw 实现）
        cfg.c_iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP | INLCR | IGNCR | ICRNL | IXON);
        cfg.c_oflag &= ~OPOST;
        cfg.c_lflag &= ~(ECHO | ECHONL | ICANON | ISIG | IEXTEN);
        cfg.c_cflag &= ~(CSIZE | PARENB);
        cfg.c_cflag |= CS8;

        // 波特率
        uint speed = BaudRateToLinuxSpeed(_baudRate);
        CfSetISpeed(ref cfg, speed);
        CfSetOSpeed(ref cfg, speed);

        // 启用接收，忽略调制解调器状态线
        cfg.c_cflag |= CREAD | CLOCAL;

        // VMIN=0, VTIME=1：read() 无数据时最多等 100ms 后返回 0，
        // 让读线程每 100ms 检查一次 _isRunning，实现干净退出。
        cfg.c_cc[VMIN]  = 0;
        cfg.c_cc[VTIME] = 1;

        if (TcSetAttr(_fd, TCSANOW, ref cfg) != 0)
        {
            PostError($"tcsetattr 失败，errno={Marshal.GetLastWin32Error()}");
            return false;
        }

        return true;
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有：读线程（直接 LibcRead，不经过 FileStream）
    // ════════════════════════════════════════════════════════════════════

    private void ReadLoop()
    {
        var buf = new byte[256];

        while (_isRunning)
        {
            if (_fd < 0) break;

            int n = LibcRead(_fd, buf, buf.Length);

            if (n < 0)
            {
                int err = Marshal.GetLastWin32Error();
                if (!_isRunning) break;          // 正常关闭时 close(fd) 导致 EBADF，忽略
                if (err == 4) continue;           // EINTR：被信号打断，重试
                PostError($"read 失败，errno={err}");
                break;
            }

            if (n == 0) continue; // VTIME 超时，无数据

            var received = new byte[n];
            Buffer.BlockCopy(buf, 0, received, 0, n);
            EnqueueData(received);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有：主线程队列
    // ════════════════════════════════════════════════════════════════════

    private void EnqueueData(byte[] data)
    {
        lock (_queueLock)
            _mainQueue.Enqueue((data, false));
    }

    private void PostError(string msg)
    {
        Debug.LogError($"[Serial] {msg}");
        lock (_queueLock)
            _mainQueue.Enqueue((System.Text.Encoding.UTF8.GetBytes(msg), true));
    }

    // ════════════════════════════════════════════════════════════════════
    //  私有：波特率 → Linux termios speed_t 常量
    //  对应 SerialPort.c 中 getBaudrate() 的完整枚举
    // ════════════════════════════════════════════════════════════════════

    private static uint BaudRateToLinuxSpeed(int baud) => baud switch
    {
        0       => 0x00000000U, // B0
        50      => 0x00000001U,
        75      => 0x00000002U,
        110     => 0x00000003U,
        134     => 0x00000004U,
        150     => 0x00000005U,
        200     => 0x00000006U,
        300     => 0x00000007U,
        600     => 0x00000008U,
        1200    => 0x00000009U,
        1800    => 0x0000000AU,
        2400    => 0x0000000BU,
        4800    => 0x0000000CU,
        9600    => 0x0000000DU,
        19200   => 0x0000000EU,
        38400   => 0x0000000FU,
        57600   => 0x00001001U,
        115200  => 0x00001002U,
        230400  => 0x00001003U,
        460800  => 0x00001004U,
        500000  => 0x00001005U,
        576000  => 0x00001006U,
        921600  => 0x00001007U,
        1000000 => 0x00001008U,
        1152000 => 0x00001009U,
        1500000 => 0x0000100AU,
        2000000 => 0x0000100BU,
        2500000 => 0x0000100CU,
        3000000 => 0x0000100DU,
        3500000 => 0x0000100EU,
        4000000 => 0x0000100FU,
        _       => 0x00001002U, // 默认 B115200
    };
}
