using System;

/// <summary>
/// 串口通信抽象接口。
/// Android 平台由 <see cref="AndroidSerialPort"/> 实现（P/Invoke libc termios）。
/// PC / Editor 平台由 <see cref="PCSerialPort"/> 实现（System.IO.Ports）。
/// 统一通过 <see cref="SerialPortManager"/> 访问，无需直接引用具体实现。
/// </summary>
public interface ISerialPort
{
    /// <summary>串口当前是否已打开。</summary>
    bool IsOpen { get; }

    /// <summary>收到原始字节数据时触发（Unity 主线程）。</summary>
    event Action<byte[]> OnDataReceived;

    /// <summary>发生错误时触发，参数为错误描述（Unity 主线程）。</summary>
    event Action<string> OnError;

    /// <summary>打开串口并启动读线程。</summary>
    bool Open(string path, int baudRate);

    /// <summary>关闭串口并等待读线程退出。</summary>
    void Close();

    /// <summary>发送原始字节数据。线程安全。</summary>
    bool Send(byte[] data);

    /// <summary>发送十六进制字符串，如 "5A 01 00 A5"。线程安全。</summary>
    bool SendHex(string hex);
}
