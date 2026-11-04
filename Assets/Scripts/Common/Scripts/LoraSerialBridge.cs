using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Lora 串口中继桥。
///
/// 职责：
///   1. 订阅 <see cref="SerialPortManager.OnDataReceived"/>，对字节流做帧边界检测。
///   2. 检测到完整 Lora 帧（0x5A … 0xA5，11~14 字节可变长）后，转换为 HEX 字符串并对外派发。
///   3. 对外提供 <see cref="SendFrame"/> 接口，将 HEX 字符串帧转为字节发到串口。
///   4. 预留 MQTT 对接入口（<see cref="OnFrameReadyForMqtt"/> / <see cref="DispatchMqttLoraCommand"/>），
///      后续 MQTT 消息格式确定后直接挂载/填充即可，无需修改本类。
///
/// 帧格式（可变 11~14 字节）：
/// <code>
///   [0] 0x5A        — 起始标志
///   [1..n-2]        — 数据区（透传，不解析）
///   [n-1] 0xA5      — 结束标志
/// </code>
///
/// 检测算法（与 Android SerialHelper.java 一致，扩展为可变长）：
///   逐字节追加到滚动缓冲区；当前字节为 0xA5 且缓冲区已有 ≥ 11 字节时，
///   在 11~14 字节范围内自近及远查找最近的 0x5A 帧头；命中则提取帧、清空缓冲区；
///   缓冲区超过 1000 字节时强制清空（防止脏数据堆积）。
///
/// 使用示例：
/// <code>
///   // 接收：挂载事件
///   LoraSerialBridge.Instance.OnFrameReceived += hex => Debug.Log("收到帧: " + hex);
///
///   // 发送（由 MQTT 下发触发）：
///   LoraSerialBridge.Instance.SendFrame("5A010203040506070809A5");
///
///   // 后续 MQTT 对接（此处无需改动本类，只需在 MQTT 消息处理器里调用）：
///   LoraSerialBridge.Instance.DispatchMqttLoraCommand(mqttHexPayload);
/// </code>
/// </summary>
public class LoraSerialBridge : MonoBehaviour
{
    // ════════════════════════════════════════════════════════════════════
    //  帧协议常量
    // ════════════════════════════════════════════════════════════════════

    /// <summary>帧起始字节。</summary>
    public const byte FrameStart = 0x5A;

    /// <summary>帧结束字节。</summary>
    public const byte FrameEnd = 0xA5;

    /// <summary>帧最小字节长度（0x5A + 9 字节数据 + 0xA5）。</summary>
    public const int MinFrameLength = 11;

    /// <summary>帧最大字节长度（0x5A + 12 字节数据 + 0xA5）。</summary>
    public const int MaxFrameLength = 14;

    /// <summary>兼容旧代码：等同 <see cref="MinFrameLength"/>。</summary>
    public const int FrameLength = MinFrameLength;

    /// <summary>滚动缓冲区最大容量；超出后自动清空（防止脏数据堆积）。</summary>
    private const int BufferMax = 1000;

    // ════════════════════════════════════════════════════════════════════
    //  单例
    // ════════════════════════════════════════════════════════════════════

    private static LoraSerialBridge _instance;

    /// <summary>全局单例，首次访问时自动创建 GameObject。</summary>
    public static LoraSerialBridge Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var go = new GameObject("[LoraSerialBridge]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<LoraSerialBridge>();
            return _instance;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开事件（均在 Unity 主线程触发）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 从串口收到完整 Lora 帧时触发。
    /// 参数为大写 HEX 字符串，如 "5A010203040506070809A5"（无空格）。
    /// </summary>
    public event Action<string> OnFrameReceived;

    /// <summary>
    /// 收到完整帧、准备转发给 MQTT（上行）时触发。
    /// 参数格式与 <see cref="OnFrameReceived"/> 相同。
    ///
    /// 已由 <c>MqttConnector.WireLoraUplinkOnce</c> 订阅：收到帧后按上行协议
    /// <c>{"type":"lora","value":"&lt;hex&gt;"}</c> 发布到设备上行 topic。
    /// </summary>
    public event Action<string> OnFrameReadyForMqtt;

    // ════════════════════════════════════════════════════════════════════
    //  状态
    // ════════════════════════════════════════════════════════════════════

    private readonly byte[] _buffer = new byte[BufferMax + 16]; // 略大于 BufferMax 防越界
    private int _bufferSize;

    // ════════════════════════════════════════════════════════════════════
    //  发送队列 + 帧间延迟
    //  说明：Lora 模块的射频空中时间远大于 UART 传输时间，若把多帧以 UART
    //  速度连灌进去，模块来不及把上一包发完就会丢帧。这里在 C# 层做统一的
    //  串行排队 + 最小帧间隔，与底层串口实现（/dev/ttyS*、USB-CP210x 等）无关。
    // ════════════════════════════════════════════════════════════════════

    /// <summary>帧间最小延迟默认值（毫秒）。</summary>
    public const int DefaultSendIntervalMs = 80;

    private const string PrefSendIntervalMs = "Lora_SendIntervalMs";

    private int _sendIntervalMs = -1;

    /// <summary>
    /// 帧间最小延迟（毫秒）。每发完一帧后至少间隔本值再发下一帧，
    /// 给 Lora 模块留足空中发送时间。运行时修改立即生效，并持久化到 PlayerPrefs。
    /// </summary>
    public int SendIntervalMs
    {
        get
        {
            if (_sendIntervalMs < 0)
                _sendIntervalMs = PlayerPrefs.GetInt(PrefSendIntervalMs, DefaultSendIntervalMs);
            return _sendIntervalMs;
        }
        set
        {
            _sendIntervalMs = Mathf.Max(0, value);
            PlayerPrefs.SetInt(PrefSendIntervalMs, _sendIntervalMs);
            PlayerPrefs.Save();
        }
    }

    private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
    private bool _senderRunning;
    private float _nextSendTime; // 下一帧最早可发送时刻（Time.unscaledTime）

    /// <summary>当前发送队列中待发帧数（调试用）。</summary>
    public int PendingSendCount => _sendQueue.Count;

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

    private void OnEnable()
    {
        var mgr = SerialPortManager.Instance;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!mgr.IsOpen)
        {
            Debug.Log($"[LoraSerialBridge] 自动打开 LORA 串口: {BoardProfile.SerialPort} " +
                      $"@ {SerialPortManager.DefaultBaudRate} baud " +
                      $"(板型默认={BoardProfile.DefaultSerialPort}" +
                      $"{(BoardProfile.HasSerialPortOverride ? $", 手动覆盖={BoardProfile.SerialPortOverride}" : "")})");
            bool ok = mgr.Open();
            Debug.Log($"[LoraSerialBridge] 自动打开串口: {(ok ? "成功" : "失败")}");
        }
        else
        {
            Debug.Log($"[LoraSerialBridge] 串口已打开，当前使用: {mgr.CurrentPortPath} @ {mgr.CurrentBaudRate} baud");
        }
#endif
        mgr.OnDataReceived += HandleRawBytes;
    }

    private void OnDisable()
    {
        // 注意：若 SerialPortManager 已被销毁则跳过，避免空引用
        if (_instance != null)
            SerialPortManager.Instance.OnDataReceived -= HandleRawBytes;

        // GameObject 被禁用时协程会停止，复位发送标志以便重新启用后能重启队列
        _senderRunning = false;
    }

    private void OnDestroy()
    {
        _sendQueue.Clear();
        _senderRunning = false;
        if (_instance == this) _instance = null;
    }

    // ════════════════════════════════════════════════════════════════════
    //  帧检测（由 SerialPortManager.OnDataReceived 在主线程触发）
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 处理来自串口的原始字节块。
    /// 与 Android <c>SerialHelper.ReadThread</c> 逐字节追加 + 帧尾触发检测的逻辑完全对应。
    /// </summary>
    private void HandleRawBytes(byte[] data)
    {
        if (data == null || data.Length == 0) return;

        foreach (byte b in data)
        {
            // 缓冲区溢出保护：超过 BufferMax 字节直接清空（对应 Android 的 >= 1000 清空逻辑）
            if (_bufferSize >= BufferMax)
            {
                Debug.LogWarning("[LoraSerialBridge] 缓冲区溢出，已清空（可能存在脏数据）");
                _bufferSize = 0;
            }

            _buffer[_bufferSize++] = b;

            // 当前字节为帧尾 0xA5，且已有足够字节时触发帧检测
            if (b == FrameEnd && _bufferSize >= MinFrameLength)
            {
                int frameLen = TryDetectFrameLength(_bufferSize);
                if (frameLen > 0)
                {
                    var frame = new byte[frameLen];
                    Array.Copy(_buffer, _bufferSize - frameLen, frame, 0, frameLen);

                    string hex = BytesToHex(frame);

                    // 清空缓冲区（帧已提取完毕）
                    _bufferSize = 0;

                    Debug.Log($"[LoraSerialBridge] 收到完整帧({frameLen}字节): {hex}");

                    OnFrameReceived?.Invoke(hex);
                    OnFrameReadyForMqtt?.Invoke(hex);
                }
                // 若帧头不匹配（0xA5 出现在非预期位置），保留缓冲区继续等待
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  公开发送接口
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 将 HEX 字符串帧解析为字节并加入发送队列（按帧间延迟串行发送）。
    /// 支持带空格/连字符的格式，如 "5A 01 02 03 04 05 06 07 08 A5" 或 "5A0102030405060708A5"。
    /// </summary>
    /// <param name="hexFrame">Lora 帧 HEX 字符串（长度随协议变化，原样透传）</param>
    /// <returns>是否成功入队（HEX 合法即返回 true，实际发送在队列协程中异步进行）</returns>
    public bool SendFrame(string hexFrame)
    {
        if (string.IsNullOrEmpty(hexFrame))
        {
            Debug.LogWarning("[LoraSerialBridge] SendFrame: 空帧，已忽略");
            return false;
        }

        byte[] bytes = HexToBytes(hexFrame);
        if (bytes == null)
        {
            Debug.LogWarning($"[LoraSerialBridge] SendFrame: HEX 解析失败 -> \"{hexFrame}\"");
            return false;
        }

        EnqueueSend(bytes);
        return true;
    }

    /// <summary>
    /// 将字节帧加入发送队列，并在需要时启动发送协程。
    /// 入队应在主线程进行（MQTT 回调经 UniTask.Post 已切回主线程）。
    /// </summary>
    private void EnqueueSend(byte[] bytes)
    {
        _sendQueue.Enqueue(bytes);
        Debug.Log($"[LoraSerialBridge] 帧入队，当前队列长度={_sendQueue.Count}，帧间隔={SendIntervalMs}ms");

        if (!_senderRunning)
            StartCoroutine(SendLoop());
    }

    /// <summary>
    /// 队列发送协程：逐帧出队发送，相邻两帧之间保证至少 <see cref="SendIntervalMs"/> 毫秒间隔。
    /// 使用 unscaledTime，不受 Time.timeScale 影响。
    /// </summary>
    private IEnumerator SendLoop()
    {
        _senderRunning = true;
        try
        {
            while (_sendQueue.Count > 0)
            {
                // 等到允许发送的时刻，保证与上一帧的最小间隔（即使两帧入队相隔很近）
                while (Time.unscaledTime < _nextSendTime)
                    yield return null;

                byte[] bytes = _sendQueue.Dequeue();
                bool ok = SerialPortManager.Instance.Send(bytes);
                Debug.Log($"[LoraSerialBridge] 队列发送({bytes.Length}字节) {(ok ? "成功" : "失败")}，剩余={_sendQueue.Count}");

                // 记录下一帧最早可发送时刻
                _nextSendTime = Time.unscaledTime + Mathf.Max(0, SendIntervalMs) / 1000f;
            }
        }
        finally
        {
            _senderRunning = false;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  预留：MQTT 对接入口
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 由 MQTT 消息处理器调用，将服务器下发（下行）的 Lora 命令转发到串口。
    /// 载荷由 <c>MqttConnector</c> 按下行协议 <c>{"msgType":"lora","msg":{"value":"&lt;hex&gt;"}}</c>
    /// 解析出 <c>msg.value</c> 后传入；本方法只负责把 HEX 写到串口。
    /// </summary>
    /// <param name="hexFrame">来自 MQTT 的 Lora 帧 HEX 字符串</param>
    public void DispatchMqttLoraCommand(string hexFrame)
    {
        SendFrame(hexFrame);
    }

    // ════════════════════════════════════════════════════════════════════
    //  静态工具方法：HEX ↔ Byte 互转
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 字节数组转 HEX 字符串。
    /// </summary>
    /// <param name="bytes">源字节数组</param>
    /// <param name="uppercase">是否大写，默认 true</param>
    /// <param name="separator">字节间分隔符，默认无（传入 " " 可得 "5A 01 ... A5" 格式）</param>
    /// <returns>HEX 字符串，如 "5A0102030405060708A5"</returns>
    public static string BytesToHex(byte[] bytes, bool uppercase = true, string separator = "")
    {
        if (bytes == null || bytes.Length == 0) return string.Empty;

        string fmt = uppercase ? "X2" : "x2";
        var sb = new StringBuilder(bytes.Length * (2 + separator.Length));
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0 && separator.Length > 0) sb.Append(separator);
            sb.Append(bytes[i].ToString(fmt));
        }
        return sb.ToString();
    }

    /// <summary>
    /// HEX 字符串转字节数组。
    /// 自动忽略空格与连字符，如 "5A 01-02 A5" → byte[]{0x5A,0x01,0x02,0xA5}。
    /// </summary>
    /// <param name="hex">十六进制字符串</param>
    /// <returns>字节数组；格式非法时返回 null 并打印警告。</returns>
    public static byte[] HexToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return null;

        hex = hex.Replace(" ", "").Replace("-", "");

        if (hex.Length % 2 != 0)
        {
            Debug.LogWarning($"[LoraSerialBridge] HexToBytes: HEX 字符数为奇数 ({hex.Length})，无法解析");
            return null;
        }

        var result = new byte[hex.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            try
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            catch
            {
                Debug.LogWarning($"[LoraSerialBridge] HexToBytes: 非法 HEX 字符在位置 {i * 2} -> \"{hex.Substring(i * 2, 2)}\"");
                return null;
            }
        }
        return result;
    }

    /// <summary>
    /// 在收到帧尾 0xA5 时，于 11~14 字节范围内查找最近的 0x5A 帧头。
    /// </summary>
    /// <returns>帧字节长度；未找到时返回 0。</returns>
    private int TryDetectFrameLength(int bufferSize)
    {
        int maxLen = Math.Min(MaxFrameLength, bufferSize);
        for (int len = MinFrameLength; len <= maxLen; len++)
        {
            if (_buffer[bufferSize - len] == FrameStart)
                return len;
        }
        return 0;
    }

    /// <summary>
    /// 校验字节数组是否为合法的 Lora 帧（帧头 0x5A、帧尾 0xA5，长度 11~14 字节）。
    /// </summary>
    public static bool IsValidFrame(byte[] frame)
    {
        return frame != null
               && frame.Length >= MinFrameLength
               && frame.Length <= MaxFrameLength
               && frame[0] == FrameStart
               && frame[frame.Length - 1] == FrameEnd;
    }

    /// <summary>
    /// 校验 HEX 字符串是否为合法 Lora 帧。
    /// </summary>
    public static bool IsValidFrameHex(string hexFrame)
    {
        if (string.IsNullOrEmpty(hexFrame)) return false;
        string clean = hexFrame.Replace(" ", "").Replace("-", "").ToUpperInvariant();
        int byteLen = clean.Length / 2;
        return clean.Length % 2 == 0
               && byteLen >= MinFrameLength
               && byteLen <= MaxFrameLength
               && clean.StartsWith("5A")
               && clean.EndsWith("A5");
    }
}
