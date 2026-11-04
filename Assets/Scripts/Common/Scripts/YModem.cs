using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace LetsPiu.Main.Common.Scripts
{
    /// <summary>
    /// YModem protocol implementation for Unity C# with BLE OTA (Over-The-Air) Support
    /// 基于Unity C#的YModem协议实现，支持蓝牙低功耗(BLE)无线固件升级(OTA)
    /// 
    /// ═══════════════════════════════════════════════════════════════════════════════════════
    /// 🚀 C# BLE OTA升级协议完整说明文档
    /// ═══════════════════════════════════════════════════════════════════════════════════════
    /// 
    /// 【概述】
    /// OTA (Over-The-Air) 升级是通过蓝牙低功耗(BLE)连接对设备固件进行无线更新的技术。
    /// 本实现采用YModem协议作为数据传输层，确保固件传输的可靠性和完整性。
    /// 
    /// 【架构设计】
    /// ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
    /// │   应用层(UI)    │    │  协议处理层      │    │   通信层(BLE)   │
    /// │                 │    │                 │    │                 │
    /// │ FirmwareUpdate  │───▶│   YModem.cs     │───▶│  BlueTooth.cs   │
    /// │ Page.cs         │    │                 │    │                 │
    /// │                 │    │ BluetoothOta    │    │                 │
    /// │                 │    │ Manager.cs      │    │                 │
    /// └─────────────────┘    └─────────────────┘    └─────────────────┘
    /// 
    /// 【OTA升级完整流程】
    /// 
    /// 阶段1️⃣: 准备阶段
    /// ─────────────────
    /// 1. 用户选择固件文件(.bin)
    /// 2. 系统验证文件完整性和设备兼容性
    /// 3. 初始化蓝牙连接和YModem协议栈
    /// 
    /// 阶段2️⃣: 设备连接
    /// ─────────────────
    /// 1. 扫描并发现目标BLE设备
    /// 2. 建立GATT连接
    /// 3. 配置特征通知
    /// 
    /// 阶段3️⃣: 进入OTA模式
    /// ─────────────────
    /// PC端 → 设备: OTA指令 (5A0007230510000000A5)
    /// 设备 → PC端: 确认响应 + 连续'C'字符(CRC模式请求)
    /// 
    /// 阶段4️⃣: YModem握手
    /// ─────────────────
    /// 设备 → PC端: 'C' (请求CRC模式)
    /// PC端 → 设备: 文件名包 [SOH 00 FF "filename.bin" size NUL... CRC CRC]
    /// 设备 → PC端: ACK + 'C' (确认文件信息，请求文件数据)
    /// 
    /// 阶段5️⃣: 文件数据传输
    /// ─────────────────
    /// FOR 每个1024字节数据块:
    ///   PC端 → 设备: 数据包 [STX seq ~seq data[1024] CRC CRC]
    ///   设备 → PC端: ACK (确认接收) 或 NAK (请求重发)
    /// 
    /// 阶段6️⃣: 传输完成
    /// ─────────────────
    /// PC端 → 设备: EOT (传输结束)
    /// 设备 → PC端: ACK (确认完成)
    /// PC端 → 设备: 空包 [SOH 00 FF NUL[128] CRC CRC]
    /// 设备 → PC端: ACK (最终确认)
    /// 
    /// 【BLE数据分包机制】
    /// 
    /// 由于BLE MTU限制，大数据包需要分包发送（已优化到64字节MTU）：
    /// 
    /// 原始YModem包: [133字节]
    /// ┌─────────────────────────────────────────────────────────────────┐
    /// │ STX(1) + SEQ(1) + ~SEQ(1) + DATA(128) + CRC(2) = 133字节      │
    /// └─────────────────────────────────────────────────────────────────┘
    ///                              ↓ 分包处理（优化后）
    /// BLE分包: 3个包（减少分包数量，提升传输效率）
    /// ┌──────────────┐┌──────────────┐┌─────────────────┐
    /// │64字节包1     ││64字节包2     ││5字节包3         │
    /// └──────────────┘└──────────────┘└─────────────────┘
    /// 
    /// 分包间隔: 15ms (优化后，平衡速度与稳定性)
    /// 
    /// 
    /// 【关键技术特性】
    /// 
    /// 🔒 可靠性保证:
    /// • CRC16校验确保数据完整性
    /// • ACK/NAK机制实现错误重传
    /// • 包序号防止数据丢失和重复
    /// • 最大重试次数限制(6次)
    /// 
    /// ⚡ 性能优化:
    /// • 异步数据处理避免UI阻塞
    /// • 优化MTU到64字节，减少分包数量（从7包减少到3包）
    /// • 优化时序控制(15ms分包间隔，提升约1倍传输速度)
    /// • 进度回调实时更新传输状态
    /// • 减少重试延时，平衡速度与稳定性
    /// 
    /// 🛡️ 错误处理:
    /// • Unity主线程/后台线程兼容性
    /// • BLE连接断开自动恢复
    /// • 数据包损坏检测和修复
    /// • 超时机制防止无限等待
    /// 
    /// 💡 兼容性增强:
    /// • 处理BLE数据包合并情况
    /// • 支持带额外字符的ACK响应
    /// • 连续'C'字符智能识别
    /// • 跨平台Unity支持(Windows/Android/iOS)
    /// 
    /// ========================================================================================
    /// YMODEM协议技术细节:
    /// ========================================================================================
    /// 
    /// 控制字符定义:
    /// SOH = 0x01 (Start of Header, 128字节数据包)
    /// STX = 0x02 (Start of Text, 1024字节数据包)  
    /// EOT = 0x04 (End of Transmission)
    /// ACK = 0x06 (Acknowledge)
    /// NAK = 0x15 (Not Acknowledge) 
    /// CAN = 0x18 (Cancel)
    /// 'C' = 0x43 (CRC模式请求)
    /// 
    /// 数据包格式:
    /// ┌─────┬─────┬──────┬──────────┬─────────┐
    /// │ SOH │ SEQ │ ~SEQ │ DATA[n]  │ CRC[2]  │
    /// │ STX │     │      │          │         │
    /// └─────┴─────┴──────┴──────────┴─────────┘
    /// 
    /// 传输流程详解:
    /// Send 0x05 >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>* 发送握手
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< C   接收CRC请求
    /// SOH 00 FF "file.bin" "size" CRC CRC >>>>>   发送文件名
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ACK 确认文件名
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< C   请求数据
    /// STX 01 FE data[1024] CRC CRC >>>>>>>>>>>>   发送数据块1
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ACK 确认数据块1
    /// STX 02 FD data[1024] CRC CRC >>>>>>>>>>>>   发送数据块2
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ACK 确认数据块2
    /// ... (重复直到文件传输完成)
    /// EOT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>   发送结束信号
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< NAK 拒绝结束
    /// EOT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>   重发结束信号
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ACK 确认结束
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< C   请求批次结束
    /// SOH 00 FF NUL[128] CRC CRC >>>>>>>>>>>>>>>   发送空包
    /// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ACK 传输完成确认
    /// ========================================================================================
    /// 
    /// 【实际应用示例】
    /// 
    /// ```csharp
    /// // 1. 启动OTA升级
    /// BluetoothOtaManager.StartOTA(
    ///     firmwareFilePath,    // 固件文件路径
    ///     deviceName,          // 目标设备名称
    ///     new OtaCallback()    // 升级状态回调
    /// );
    /// 
    /// // 2. 处理升级进度
    /// public class OtaCallback : BluetoothOtaManager.IOTACallback
    /// {
    ///     public void OnProgress(int currentSent, int total)
    ///     {
    ///         float progress = (float)currentSent / total * 100f;
    ///         UpdateProgressUI(progress);
    ///     }
    ///     
    ///     public void OnSuccess()
    ///     {
    ///         ShowSuccessMessage("固件升级成功！");
    ///     }
    ///     
    ///     public void OnFailed(int errorCode, string reason)
    ///     {
    ///         ShowErrorMessage($"升级失败[{errorCode}]: {reason}");
    ///     }
    /// }
    /// 
    /// // 3. 基础YModem使用
    /// YModem yModem = new YModem.Builder()
    ///     .FilePath("/path/to/firmware.bin")
    ///     .FileName("firmware.bin")
    ///     .CheckMd5("")
    ///     .SendSize(1024)
    ///     .Callback(new YModemListenerImpl())
    ///     .Build();
    /// 
    /// // Start transmission
    /// yModem.Start(null);
    /// 
    /// // Handle received data from terminal/device
    /// yModem.OnReceiveData(receivedBytes);
    /// 
    /// // Stop transmission when needed
    /// yModem.Stop();
    /// ```
    /// 
    /// 【版本历史】
    /// version v2.0.0 - Java原版YModem协议实现
    /// version v2.0.1 - 增加文件传输优化
    /// version v2.0.2 - 修复CRC校验问题
    /// version v3.0.0 - C# Unity移植版本，增加BLE OTA支持
    /// version v3.0.1 - 修复Unity线程兼容性问题
    /// version v3.0.2 - 优化BLE分包时序，解决设备超时问题
    /// version v3.0.3 - 增强BLE数据包合并处理能力
    /// version v3.0.4 - 改进ACK响应解析的健壮性
    /// version v3.1.0 - 【UniTask转换】使用UniTask替代协程，彻底解决主线程调用问题
    ///                 - 所有异步操作使用UniTask.SwitchToMainThread()确保在主线程中执行
    ///                 - 移除对MonoBehaviour协程运行器的依赖
    ///                 - 简化线程管理，提高代码可维护性
    /// version v3.1.1 - 【连接超时】增加设备连接超时功能
    ///                 - 设备连接超过60秒自动失败并返回CONNECTION_TIMEOUT错误
    ///                 - 使用UniTask和CancellationToken实现精确的超时控制
    ///                 - 连接成功或失败时自动取消超时计时器
    /// 
    /// 【技术支持】
    /// 本实现基于成熟的Java YModem协议移植而来，经过大量测试验证。
    /// 适用于各种支持BLE通信的嵌入式设备固件升级场景。
    /// 
    /// This is a C# port of the original Java implementation.
    /// Enhanced for Unity with comprehensive BLE OTA firmware upgrade support.
    /// ═══════════════════════════════════════════════════════════════════════════════════════
    /// </summary>
    public class YModem : IFileStreamThreadDataReaderListener
    {
        #region Constants
        
        private const int STEP_HELLO = 0x00;
        private const int STEP_FILE_NAME = 0x01;
        private const int STEP_FILE_BODY = 0x02;
        private const int STEP_EOT = 0x03;
        private const int STEP_END = 0x04;
        
        private const byte ACK = 0x06; /* ACKnowlege */
        private const byte NAK = 0x15; /* Negative AcKnowlege */
        private const byte CAN = 0x18; /* CANcel character */
        private const byte ST_C = (byte)'C';
        private const string MD5_OK = "MD5_OK";
        private const string MD5_ERR = "MD5_ERR";
        
        private const int MAX_PACKAGE_SEND_ERROR_TIMES = 6;
        private const int PACKAGE_TIME_OUT = 6000; // milliseconds
        
        #endregion
        
        #region Fields
        
        private static int CURR_STEP = STEP_HELLO;
        private static int mSize = 1024;
        
        private readonly string filePath;
        private readonly string fileNameString;
        private readonly string fileMd5String;
        private readonly IYModemListener listener;
        
        private readonly TimeOutHelper timerHelper = new TimeOutHelper();
        private FileStreamThread streamThread;
        
        // bytes has been sent of this transmission
        private int bytesSent = 0;
        // package data of current sending, used for retry case
        private byte[] currSending = null;
        // 【新增】当前发送包的实际数据长度（不包括协议头和CRC）
        private int currSendingDataLength = 0;
        private int packageErrorTimes = 0;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Private constructor, use Builder to create instance
        /// </summary>
        private YModem(string filePath, string fileNameString, string fileMd5String, 
                       int size, IYModemListener listener)
        {
            this.filePath = filePath;
            this.fileNameString = fileNameString;
            this.fileMd5String = fileMd5String;
            if (size == 0)
            {
                size = 1024;
            }
            mSize = size;
            this.listener = listener;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Start the transmission
        /// </summary>
        public void Start(string data)
        {
            SendData(data);
        }
        
        /// <summary>
        /// Stop the transmission when you don't need it or shut it down in an accident
        /// </summary>
        public void Stop()
        {
            bytesSent = 0;
            currSending = null;
            currSendingDataLength = 0;
            packageErrorTimes = 0;
            if (streamThread != null)
            {
                streamThread.Release();
            }
            timerHelper.StopTimer();
            timerHelper.UnRegisterListener();
        }
        
        /// <summary>
        /// Method for the outer caller when received data from the terminal
        /// </summary>
        public void OnReceiveData(byte[] respData)
        {
            // Stop the package timer
            timerHelper.StopTimer();
            if (respData != null && respData.Length > 0)
            {
                Debug.Log($"YModem received {respData.Length} bytes.");
                switch (CURR_STEP)
                {
                    case STEP_HELLO:
                        HandleData(respData);
                        break;
                    case STEP_FILE_NAME:
                        HandleFileName(respData);
                        break;
                    case STEP_FILE_BODY:
                        HandleFileBody(respData);
                        break;
                    case STEP_EOT:
                        HandleEOT(respData);
                        break;
                    case STEP_END:
                        HandleEnd(respData);
                        break;
                }
            }
            else
            {
                Debug.Log("The terminal do responsed something, but received nothing??");
            }
        }
        
        #endregion
        
        #region Private Methods - Data Sending
        
        private void SendData(string data)
        {
            streamThread = new FileStreamThread(filePath, this);
            if (data != null)
            {
                CURR_STEP = STEP_HELLO;
                Debug.Log("StartData!!!");
                byte[] hello = YModemUtil.GetYModelData(data);
                SendPackageData(hello);
            }
            else
            {
                packageErrorTimes = 0;
                SendFileName();
            }
        }
        
        private void SendFileName()
        {
            CURR_STEP = STEP_FILE_NAME;
            Debug.Log("sendFileName");
            try
            {
                long fileByteSize = streamThread.GetFileByteSize();
                byte[] fileNamePackage = YModemUtil.GetFileNamePackage(fileNameString, fileByteSize, fileMd5String);
                SendPackageData(fileNamePackage);
            }
            catch (IOException e)
            {
                Debug.LogError(e.ToString());
            }
        }
        
        private void StartSendFileData()
        {
            CURR_STEP = STEP_FILE_BODY;
            Debug.Log("startSendFileData");
            streamThread.Start();
        }
        
        // Callback from the data reading thread when a data package is ready
        public void OnDataReady(byte[] data, int actualDataLength = 0)
        {
            // 记录当前包的实际数据长度，用于准确计算传输进度
            currSendingDataLength = actualDataLength;
            SendPackageData(data);
        }
        
        private void SendEOT()
        {
            CURR_STEP = STEP_EOT;
            Debug.Log("sendEOT");
            if (listener != null)
            {
                listener.OnDataReady(YModemUtil.GetEOT());
            }
        }
        
        private void SendEND()
        {
            CURR_STEP = STEP_END;
            Debug.Log("sendEND");
            if (listener != null)
            {
                try
                {
                    listener.OnDataReady(YModemUtil.GetEnd());
                }
                catch (IOException e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
        
        private void SendPackageData(byte[] packageData)
        {
            if (listener != null && packageData != null)
            {
                currSending = packageData;
                // Start the timer, it will be cancelled when reponse received,
                // or trigger the timeout and resend the current package data
                timerHelper.StartTimer(OnTimeOut, PACKAGE_TIME_OUT);
                listener.OnDataReady(packageData);
            }
        }
        
        #endregion
        
        #region Private Methods - Response Handling
        
        private void HandleData(byte[] value)
        {
            int character = value[0];
            if (character == ST_C) // Receive "C" for "HELLO"
            {
                Debug.Log("Received 'C'");
                packageErrorTimes = 0;
                SendFileName();
            }
            else
            {
                HandleOthers(character);
            }
        }
        
        // The file name package was responsed
        private void HandleFileName(byte[] value)
        {
            // 【关键修复】检查是否以ACK+C开头，而不是精确匹配长度
            if (value.Length >= 2 && value[0] == ACK && value[1] == ST_C) // Receive 'ACK C' (可能带额外字符)
            {
                Debug.Log($"Received 'ACK C' (with {value.Length} total bytes)");
                packageErrorTimes = 0;
                StartSendFileData();
            }
            else if (value.Length == 1 && value[0] == ST_C) // 单个'C'字符
            {
                Debug.Log("Received 'C'");
                HandlePackageFail("Received 'C' without 'ACK' after sent file name");
            }
            else if (value.Length > 1 && IsAllSameCharacter(value, ST_C)) // 处理多个连续的'C'字符
            {
                Debug.Log($"Received {value.Length} consecutive 'C' characters, device requesting CRC mode");
                // 多个连续的'C'是正常的，设备在请求CRC模式，继续等待ACK+C
                // 不需要特殊处理，只记录日志即可
            }
            else
            {
                HandleOthers(value[0]);
            }
        }
        
        private void HandleFileBody(byte[] value)
        {
            if (value.Length == 1 && value[0] == ACK) // Receive ACK for file data
            {
                Debug.Log($"Received 'ACK' for data package, actual data bytes: {currSendingDataLength}");
                packageErrorTimes = 0;
                
                // 【关键修复】只累加实际的文件数据字节，不包括协议开销
                // 对于数据包，只计算实际的文件内容字节数
                if (currSendingDataLength > 0)
                {
                    bytesSent += currSendingDataLength;
                    Debug.Log($"Total file bytes sent so far: {bytesSent}");
                }
                else
                {
                    // 如果是协议包（如文件名包），使用包的数据部分长度
                    // currSending格式：[Header 3字节] + [Data] + [CRC 2字节]
                    int dataPartLength = currSending.Length - 5; // 减去协议头3字节和CRC2字节
                    if (dataPartLength > 0)
                    {
                        // 这里不累加，因为文件名包不是文件内容
                        Debug.Log($"Non-data package ACK received, package size: {currSending.Length}");
                    }
                }
                
                try
                {
                    if (listener != null)
                    {
                        listener.OnProgress(bytesSent, (int)streamThread.GetFileByteSize());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                streamThread.KeepReading();
            }
            else if (value.Length == 1 && value[0] == ST_C)
            {
                Debug.Log("Received 'C'");
                // Receive C for file data, the ymodem cannot handle this circumstance, transmission failed...
                HandlePackageFail("Received 'C' after sent file data");
            }
            else if (value.Length > 1 && IsAllSameCharacter(value, ST_C)) // 【新增】处理多个连续的'C'字符
            {
                Debug.Log($"Received {value.Length} consecutive 'C' characters during data transfer");
                // 在数据传输过程中收到多个'C'，这通常表示设备在等待下一个数据包
                // 可以忽略，继续正常的传输流程
            }
            else
            {
                HandleOthers(value[0]);
            }
        }
        
        private void HandleEOT(byte[] value)
        {
            if (value[0] == ACK)
            {
                Debug.Log("Received 'ACK'");
                packageErrorTimes = 0;
                SendEND();
            }
            else if (value[0] == ST_C) // As we haven't received ACK, we should resend EOT
            {
                HandlePackageFail("Received 'C' after sent EOT");
            }
            else if (value[0] == NAK) // 如果是NAK的话 再次发送一次EOT数据
            {
                SendEOT();
            }
            else
            {
                HandleOthers(value[0]);
            }
        }
        
        private void HandleEnd(byte[] character)
        {
            if (character[0] == ACK) // The last ACK represents that the transmission has been finished, but we should validate the file
            {
                Debug.Log("Received 'ACK'");
                packageErrorTimes = 0;
                // 发送已经成功，完全结束
                if (listener != null)
                {
                    listener.OnSuccess();
                }
            }
            else if (System.Text.Encoding.UTF8.GetString(character).Equals(MD5_OK)) // The file data has been checked,Well Done!
            {
                Debug.Log("Received 'MD5_OK'");
                Stop();
                if (listener != null)
                {
                    listener.OnSuccess();
                }
            }
            else if (System.Text.Encoding.UTF8.GetString(character).Equals(MD5_ERR)) // Oops...Transmission Failed...
            {
                Debug.Log("Received 'MD5_ERR'");
                Stop();
                if (listener != null)
                {
                    listener.OnFailed("MD5 check failed!!!");
                }
            }
            else
            {
                HandleOthers(character[0]);
            }
        }
        
        private void HandleOthers(int character)
        {
            if (character == NAK) // We need to resend this package as the terminal failed when checking the crc
            {
                Debug.Log("Received 'NAK'");
                HandlePackageFail("Received NAK");
            }
            else if (character == CAN) // Some big problem occurred, transmission failed...
            {
                Debug.Log("Received 'CAN'");
                if (listener != null)
                {
                    listener.OnFailed("Received CAN");
                }
                Stop();
            }
        }
        
        /// <summary>
        /// 检查字节数组是否全部为相同的字符
        /// </summary>
        private bool IsAllSameCharacter(byte[] data, byte character)
        {
            if (data == null || data.Length == 0) return false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != character) return false;
            }
            return true;
        }
        
        // Handle a failed package data ,resend it up to MAX_PACKAGE_SEND_ERROR_TIMES times.
        // If still failed, then the transmission failed.
        private void HandlePackageFail(string reason)
        {
            packageErrorTimes++;
            Debug.Log($"Fail:{reason} for {packageErrorTimes} times");
            if (packageErrorTimes < MAX_PACKAGE_SEND_ERROR_TIMES)
            {
                SendPackageData(currSending);
            }
            else
            {
                // Still, we stop the transmission, release the resources
                Stop();
                if (listener != null)
                {
                    listener.OnFailed(reason);
                }
            }
        }
        
        #endregion
        
        #region IFileStreamThreadDataReaderListener Implementation
        
        // The InputStream data reading thread was done
        public void OnFinish()
        {
            SendEOT();
        }
        
        #endregion
        
        #region Timeout Handler
        
        private void OnTimeOut()
        {
            Debug.Log("------ time out ------");
            if (currSending != null)
            {
                HandlePackageFail("package timeout...");
            }
        }
        
        #endregion
        
        #region Static Properties
        
        public static int MSize => mSize;
        
        #endregion
        
        #region Builder Pattern
        
        public class Builder
        {
            private string filePath;
            private string fileNameString;
            private string fileMd5String;
            private int size;
            private IYModemListener listener;
            
            public Builder FilePath(string filePath)
            {
                this.filePath = filePath;
                return this;
            }
            
            public Builder FileName(string fileName)
            {
                this.fileNameString = fileName;
                return this;
            }
            
            public Builder SendSize(int size)
            {
                this.size = size;
                return this;
            }
            
            public Builder CheckMd5(string fileMd5String)
            {
                this.fileMd5String = fileMd5String;
                return this;
            }
            
            public Builder Callback(IYModemListener listener)
            {
                this.listener = listener;
                return this;
            }
            
            public YModem Build()
            {
                return new YModem(filePath, fileNameString, fileMd5String, size, listener);
            }
        }
        
        #endregion
    }
    
    #region Supporting Interfaces
    
    /// <summary>
    /// Listener of the transmission process
    /// </summary>
    public interface IYModemListener
    {
        /// <summary>
        /// The data package has been encapsulated
        /// </summary>
        void OnDataReady(byte[] data);
        
        /// <summary>
        /// Just the file data progress
        /// </summary>
        void OnProgress(int currentSent, int total);
        
        /// <summary>
        /// The file has been correctly sent to the terminal
        /// </summary>
        void OnSuccess();
        
        /// <summary>
        /// The task has failed with several remedial measures like retrying some times
        /// </summary>
        void OnFailed(string reason);
    }
    
    /// <summary>
    /// Interface for FileStreamThread data reader callback
    /// </summary>
    public interface IFileStreamThreadDataReaderListener
    {
        void OnDataReady(byte[] data, int actualDataLength = 0);
        void OnFinish();
    }
    
    #endregion
    
    #region Supporting Classes
    
    /// <summary>
    /// A timer util for counting the time past after we sent a package to the terminal
    /// </summary>
    public class TimeOutHelper
    {
        public delegate void TimeOutDelegate();
        
        private TimeOutDelegate listener;
        private CancellationTokenSource cancellationTokenSource;
        
        public void StartTimer(TimeOutDelegate timeoutListener, long delay)
        {
            listener = timeoutListener;
            cancellationTokenSource = new CancellationTokenSource();
            
            Task.Delay((int)delay, cancellationTokenSource.Token).ContinueWith(task =>
            {
                if (!task.IsCanceled && listener != null)
                {
                    listener.Invoke();
                }
            });
        }
        
        public void StopTimer()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
        
        public void UnRegisterListener()
        {
            listener = null;
        }
    }
    
    /// <summary>
    /// Thread for reading input Stream and encapsulating into a ymodem package
    /// </summary>
    public class FileStreamThread
    {
        private FileStream fileStream = null;
        private IFileStreamThreadDataReaderListener listener;
        private readonly string filePath;
        private volatile bool isDataAcknowledged = false;
        private volatile bool isKeepRunning = false;
        private long fileByteSize = 0;
        private Thread thread;
        
        public FileStreamThread(string filePath, IFileStreamThreadDataReaderListener listener)
        {
            this.filePath = filePath;
            this.listener = listener;
        }
        
        public long GetFileByteSize()
        {
            if (fileByteSize == 0 || fileStream == null)
            {
                InitStream();
            }
            return fileByteSize;
        }
        
        public void Start()
        {
            thread = new Thread(PrepareData);
            thread.Start();
        }
        
        private void PrepareData()
        {
            try
            {
                InitStream();
                byte[] block = new byte[YModem.MSize];
                int dataLength;
                byte blockSequence = 1; // The data package of a file is actually started from 1
                isDataAcknowledged = true;
                isKeepRunning = true;
                
                while (isKeepRunning)
                {
                    if (!isDataAcknowledged)
                    {
                        try
                        {
                            // We need to sleep for a while as the sending 1024 bytes data from ble would take several seconds
                            Thread.Sleep(100);
                        }
                        catch (ThreadAbortException)
                        {
                            break;
                        }
                        continue;
                    }
                    
                    if ((dataLength = fileStream.Read(block, 0, block.Length)) == 0)
                    {
                        Debug.Log("The file data has all been read...");
                        if (listener != null)
                        {
                            OnStop();
                            listener.OnFinish();
                        }
                        break;
                    }
                    
                    byte[] pack = YModemUtil.GetDataPackage(block, dataLength, blockSequence);
                    
                    if (listener != null)
                    {
                        // 传递实际数据长度给YModem，用于准确的进度计算
                        listener.OnDataReady(pack, dataLength);
                    }
                    
                    blockSequence++;
                    isDataAcknowledged = false;
                }
            }
            catch (IOException e)
            {
                Debug.LogError(e.ToString());
            }
        }
        
        /// <summary>
        /// When received response from the terminal, we should keep the thread keep going
        /// </summary>
        public void KeepReading()
        {
            isDataAcknowledged = true;
        }
        
        public void Release()
        {
            OnStop();
            listener = null;
        }
        
        private void OnStop()
        {
            isKeepRunning = false;
            isDataAcknowledged = false;
            fileByteSize = 0;
            OnReadFinished();
        }
        
        private void InitStream()
        {
            if (fileStream == null)
            {
                try
                {
                    fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    fileByteSize = fileStream.Length;
                }
                catch (IOException e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
        
        private void OnReadFinished()
        {
            if (fileStream != null)
            {
                try
                {
                    fileStream.Close();
                    fileStream.Dispose();
                    fileStream = null;
                }
                catch (IOException e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }
    }
    
    /// <summary>
    /// Util for encapsulating data package of ymodem protocol
    /// </summary>
    public static class YModemUtil
    {
        private const byte SOH = 0x01; /* Start Of Header with data size :128*/
        private const byte STX = 0x02; /* Start Of Header with data size : 1024*/
        private const byte EOT = 0x04; /* End Of Transmission */
        private const byte CPMEOF = 0x1A;/* Fill the last package if not long enough */
        private static readonly CRC16 crc16 = new CRC16();
        
        /// <summary>
        /// Get the first package data for hello with a terminal
        /// </summary>
        public static byte[] GetYModelData(string dynamicData)
        {
            return System.Text.Encoding.UTF8.GetBytes(dynamicData);
        }
        
        /// <summary>
        /// Get the file name package data
        /// </summary>
        public static byte[] GetFileNamePackage(string fileNameString, long fileByteSize, string fileMd5String)
        {
            byte seperator = 0x0;
            string fileSize = fileByteSize.ToString();
            byte[] byteFileSize = System.Text.Encoding.UTF8.GetBytes(fileSize);
            
            byte[] fileNameBytes1 = Concat(System.Text.Encoding.UTF8.GetBytes(fileNameString),
                new byte[] { seperator },
                byteFileSize);
            
            byte[] fileNameBytes2 = new byte[128];
            byte[] tempBytes = Concat(fileNameBytes1,
                new byte[] { seperator },
                System.Text.Encoding.UTF8.GetBytes(fileMd5String));
            
            Array.Copy(tempBytes, 0, fileNameBytes2, 0, Math.Min(tempBytes.Length, 128));
            
            byte seq = 0x00;
            return GetDataPackage(fileNameBytes2, 128, seq);
        }
        
        /// <summary>
        /// Get a encapsulated package data block
        /// </summary>
        public static byte[] GetDataPackage(byte[] block, int dataLength, byte sequence)
        {
            // 选择合适的包头类型：SOH (128字节) 或 STX (1024字节)
            byte headerType = (block.Length == 128) ? SOH : (block.Length == 1024) ? STX : SOH;
            byte[] header = GetDataHeader(sequence, headerType);
            
            // 填充剩余数据为 CPMEOF（如果数据不足）
            if (dataLength < block.Length)
            {
                int startFil = dataLength;
                while (startFil < block.Length)
                {
                    block[startFil] = CPMEOF;
                    startFil++;
                }
            }
            
            // 计算CRC校验
            ushort crc = (ushort)crc16.CalcCRC(block);
            byte[] crcBytes = new byte[2];
            crcBytes[0] = (byte)((crc >> 8) & 0xFF);
            crcBytes[1] = (byte)(crc & 0xFF);
            
            // 返回完整的包（包头 + 数据 + CRC）
            return Concat(header, block, crcBytes);
        }
        
        /// <summary>
        /// Get the EOT package
        /// </summary>
        public static byte[] GetEOT()
        {
            return new byte[] { EOT };
        }
        
        /// <summary>
        /// Get the Last package
        /// </summary>
        public static byte[] GetEnd()
        {
            byte seq = 0x00;
            return GetDataPackage(new byte[128], 128, seq);
        }
        
        private static byte[] GetDataHeader(byte sequence, byte start)
        {
            // The serial number of the package increases Cyclically up to 256
            byte modSequence = (byte)(sequence % 0x256);
            byte complementSeq = (byte)~modSequence;
            
            return Concat(new byte[] { start },
                new byte[] { modSequence },
                new byte[] { complementSeq });
        }
        
        private static byte[] Concat(byte[] a, byte[] b, byte[] c)
        {
            int aLen = a.Length;
            int bLen = b.Length;
            int cLen = c.Length;
            byte[] concated = new byte[aLen + bLen + cLen];
            Array.Copy(a, 0, concated, 0, aLen);
            Array.Copy(b, 0, concated, aLen, bLen);
            Array.Copy(c, 0, concated, aLen + bLen, cLen);
            return concated;
        }
    }
    
    /// <summary>
    /// Uses table for irreducible polynomial: 1 + x^2 + x^15 + x^16
    /// </summary>
    public class CRC16
    {
        private static readonly int[] table = {
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
            0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
            0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
            0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
            0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
            0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
            0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
            0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
            0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
            0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
            0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
            0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
            0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
            0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
            0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
            0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
            0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
            0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
            0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
            0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
            0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
            0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
            0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
            0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
            0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
            0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
            0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
            0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
            0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
            0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
            0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
            0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0,
        };
        
        public int GetCRCLength()
        {
            return 2;
        }
        
        public long CalcCRC(byte[] block)
        {
            int crc = 0x0000;
            foreach (byte b in block)
            {
                crc = ((crc << 8) ^ table[((crc >> 8) ^ (0xff & b))]) & 0xFFFF;
            }
            return crc;
        }
        
        public static int Crc16Byte(int crc, byte b)
        {
            int[] crc16Table = {
                0x0000, 0xCC01, 0xD801, 0x1400,
                0xF001, 0x3C00, 0x2800, 0xE401,
                0xA001, 0x6C00, 0x7800, 0xB401,
                0x5000, 0x9C01, 0x8801, 0x4400
            };
            int temp;
            // Compute checksum of lower four bits of a byte.
            temp = crc16Table[crc & 0xF];
            crc = (crc >> 4) & 0x0FFF;
            crc = crc ^ temp ^ crc16Table[b & 0xF];
            // Now compute checksum of upper four bits of a byte.
            temp = crc16Table[crc & 0xF];
            crc = (crc >> 4) & 0x0FFF;
            crc = crc ^ temp ^ crc16Table[(b >> 4) & 0xF];
            return crc;
        }
    }
    
    #endregion
    
    #region OTA Upgrade Example using Bluetooth Plugin
    
    /// <summary>
    /// OTA升级管理器 - 基于Unity Bluetooth插件的C#版本
    /// 参考Java版BleOtaManager实现
    /// 
    /// 使用示例：
    /// ```csharp
    /// BluetoothOtaManager.StartOTA(
    ///     "/path/to/firmware.bin", 
    ///     "DeviceName", 
    ///     new MyOTACallback()
    /// );
    /// ```
    /// </summary>
    public static class BluetoothOtaManager
    {
        private static readonly string TAG = "BluetoothOtaManager";
        
        #region Constants and Enums
        
        private const int MAX_PACKET_SIZE = 64; // 适度提升MTU到64字节，减少分包数量
        private const string DEFAULT_OTA_COMMAND = "5A0007230510000000A5";
        private const int OTA_DEVICE_TYPE = 5; // 为OTA升级定义一个特殊的设备类型
        private const float PACKET_SEND_DELAY = 0.04f; // 适度优化发送延时到40ms (减半)
        private const float INTER_PACKET_DELAY = 0.015f; // 分包间隔优化到15ms (减半)
        private const int MAX_RETRY_TIMES = 3; // 单个数据包的最大重试次数
        private const int CONNECTION_TIMEOUT_SECONDS = 120; // 设备连接超时时间（秒）
        
        /// <summary>
        /// OTA升级步骤枚举
        /// </summary>
        public enum OtaStep
        {
            Initializing = 0,           // 初始化
            ConnectingDevice = 1,       // 连接设备
            SendingOtaCommand = 2,      // 发送OTA指令
            StartingYmodem = 3,         // 启动YModem
            TransferringData = 4,       // 传输数据
            Completing = 5,             // 完成升级
            Failed = 6                  // 升级失败
        }
        
        /// <summary>
        /// 错误编号定义
        /// </summary>
        public static class ErrorCode
        {
            public const int DEVICE_CONNECT_FAILED = 304;        // 设备连接失败
            public const int CONNECTION_TIMEOUT = 305;           // 设备连接超时
            public const int OTA_COMMAND_SEND_FAILED = 401;      // OTA指令发送失败
            public const int FIRMWARE_FILE_NOT_FOUND = 403;      // 固件文件未找到
            public const int YMODEM_INIT_FAILED = 405;           // YModem初始化失败
            public const int YMODEM_TRANSFER_FAILED = 406;       // YModem传输失败
            public const int BLE_WRITE_FAILED = 501;             // BLE写入失败
            public const int UNKNOWN_ERROR = 901;                // 未知错误
            public const int OPERATION_CANCELLED = 902;          // 操作被取消
        }
        
        #endregion
        
        #region Fields
        
        private static IOTACallback otaCallback;
        private static string targetDeviceName;
        private static string firmwareFilePath;
        private static string otaCommand = DEFAULT_OTA_COMMAND;
        private static OtaStep currentStep = OtaStep.Initializing;
        private static bool isOtaModeEntered = false;
        private static YModem yModem;
        private static System.Collections.Generic.Queue<byte[]> writeQueue = new System.Collections.Generic.Queue<byte[]>();
        private static bool isWriting = false;
        private static int currentPacketRetryCount = 0; // 当前数据包的重试次数
        
        // 连接超时相关字段
        private static System.Threading.CancellationTokenSource connectionTimeoutCts;
        private static bool isConnectionTimedOut = false;
        
        #endregion
        
        #region Interfaces
        
        /// <summary>
        /// OTA升级回调接口
        /// </summary>
        public interface IOTACallback
        {
            /// <summary>
            /// 进度更新
            /// </summary>
            void OnProgress(int currentSent, int total);
            
            /// <summary>
            /// 状态更新
            /// </summary>
            void OnStatusUpdate(string status);
            
            /// <summary>
            /// 升级成功
            /// </summary>
            void OnSuccess();
            
            /// <summary>
            /// 升级失败
            /// </summary>
            void OnFailed(int errorCode, string errorMessage);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 开始OTA升级
        /// </summary>
        /// <param name="firmwarePath">固件文件路径</param>
        /// <param name="deviceName">蓝牙设备名称</param>
        /// <param name="callback">升级回调</param>
        /// <param name="customOtaCommand">自定义OTA指令（可选）</param>
        public static void StartOTA(string firmwarePath, string deviceName, IOTACallback callback, string customOtaCommand = null)
        {
            if (!DeviceConnector.BluetoothHardwareEnabled)
            {
                Debug.LogError($"{TAG}: 蓝牙硬件已关闭（DeviceConnector.BluetoothHardwareEnabled=false），无法 OTA。");
                callback?.OnFailed(ErrorCode.DEVICE_CONNECT_FAILED, "蓝牙功能已关闭");
                return;
            }

            Debug.Log($"{TAG}: 开始OTA升级流程");
            Debug.Log($"{TAG}: 固件文件: {firmwarePath}");
            Debug.Log($"{TAG}: 目标设备: {deviceName}");
            var gun = DeviceConnector.Instance.GetGunInfo();

            if (gun == null)
            {
                FailWithReason(ErrorCode.DEVICE_CONNECT_FAILED, "未绑定装备！");
                return;
            }
            
            // 【UniTask转换】不再需要检查协程运行器，UniTask可以独立运行
            // 停止之前的升级
            StopOTA();
            
            // 设置参数
            firmwareFilePath = firmwarePath;
            targetDeviceName = deviceName;
            otaCallback = callback;
            if (!string.IsNullOrEmpty(customOtaCommand))
            {
                otaCommand = customOtaCommand;
            }
            
            // 检查文件是否存在
            if (!System.IO.File.Exists(firmwareFilePath))
            {
                FailWithReason(ErrorCode.FIRMWARE_FILE_NOT_FOUND, "固件文件未找到: " + firmwareFilePath);
                return;
            }
            
            // 注册蓝牙事件监听
            RegisterBluetoothEvents();
            
            // 开始连接设备
            UpdateOtaStep(OtaStep.ConnectingDevice);
            UpdateStatus("YModem OTA: 正在连接设备: " + deviceName);
            
            // 【连接超时】启动连接超时计时器
            StartConnectionTimeout();
            
            // 使用现有的BlueTooth插件连接设备
            // 这里我们使用gun类型(1)作为示例，实际使用时可能需要扩展BlueTooth类支持更多设备类型
            Debug.Log("YModem OTA: 开始链接设备(ConnectDeviceAsync)... deviceName="+ deviceName+" gun.code="+gun.code);
            DeviceConnector.Instance.ConnectDeviceAsync(gun).Forget();
        }
        
        /// <summary>
        /// 停止OTA升级
        /// </summary>
        public static void StopOTA()
        {
            Debug.Log($"{TAG}: 停止OTA升级流程");
            
            // 停止YModem传输
            if (yModem != null)
            {
                yModem.Stop();
                yModem = null;
            }
            
            // 取消注册蓝牙事件监听
            UnregisterBluetoothEvents();
            
            // 【连接超时】取消连接超时计时器
            CancelConnectionTimeout();
            
            // 断开蓝牙连接
            if (DeviceConnector.BluetoothHardwareEnabled)
            {
                Plugins.BlueTooth.DisconnectAll();
            }
            
            // 重置状态
            ResetState();
            
            UpdateStatus("OTA升级已停止");
        }
        
        /// <summary>
        /// 重置状态变量
        /// </summary>
        private static void ResetState()
        {
            isOtaModeEntered = false;
            writeQueue.Clear();
            isWriting = false;
            currentStep = OtaStep.Initializing;
            currentPacketRetryCount = 0;
            isConnectionTimedOut = false;
            
            Debug.Log($"{TAG}: 所有状态变量已重置");
        }
        
        /// <summary>
        /// 处理蓝牙状态变化（集成到现有的BlueTooth事件中）
        /// 【UniTask转换】确保所有操作在主线程中执行
        /// </summary>
        public static void HandleBluetoothStatus(int status, string message)
        {
            Debug.Log("YModem HandleBluetoothStatus status="+status+" message="+message+" otaCallback==null?"+(otaCallback == null));
            if (otaCallback == null) return; // 没有在进行OTA升级
            
            // 使用UniTask异步处理，确保在主线程中执行
            HandleBluetoothStatusAsync(status, message).Forget();
        }
        
        private static async UniTaskVoid HandleBluetoothStatusAsync(int status, string message)
        {
            // 确保在主线程中执行
            await UniTask.SwitchToMainThread();
            
            Debug.Log($"{TAG}: 蓝牙状态变化 - Status: {status}, Message: {message}");
            
            var bluetoothStatus = (Plugins.BlueTooth.BlueToothStatus)status;
            
            switch (bluetoothStatus)
            {
                case Plugins.BlueTooth.BlueToothStatus.BlueToothStatusDeviceConnected:
                    // 【连接超时】设备连接成功，取消超时计时器
                    CancelConnectionTimeout();
                    
                    Debug.Log($"{TAG}: 设备连接成功，当前步骤: {currentStep}");
                    
                    // 更精确的重连检测：只有在传输数据阶段收到连接回调才认为是异常重连
                    if (currentStep == OtaStep.TransferringData && isOtaModeEntered)
                    {
                        Debug.LogWarning($"{TAG}: 数据传输过程中检测到设备重连，从头重新开始升级流程");
                        await RestartOtaProcessAsync();
                        return;
                    }
                    
                    // 正常连接流程：只有在连接设备阶段才处理连接成功
                    if (currentStep == OtaStep.ConnectingDevice)
                    {
                        Debug.Log($"{TAG}: 设备连接成功，开始发送OTA指令");
                        await SendOtaCommandAsync();
                    }
                    else
                    {
                        Debug.Log($"{TAG}: 设备连接回调 - 当前阶段: {currentStep}, 已进入OTA模式: {isOtaModeEntered}");
                    }
                    break;
                    
                case Plugins.BlueTooth.BlueToothStatus.BlueToothStatusConnectFailed:
                    // 【连接超时】连接失败，取消超时计时器
                    CancelConnectionTimeout();
                    FailWithReason(ErrorCode.DEVICE_CONNECT_FAILED, "设备连接失败: " + message);
                    break;
                    
                case Plugins.BlueTooth.BlueToothStatus.BlueToothStatusDisConnect:
                    if (currentStep != OtaStep.Completing)
                    {
                        FailWithReason(ErrorCode.DEVICE_CONNECT_FAILED, "设备连接意外断开: " + message);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 处理蓝牙消息（集成到现有的BlueTooth事件中）
        /// 【UniTask转换】确保所有数据处理在主线程中执行
        /// </summary>
        public static void HandleBluetoothMessage(int status, string message)
        {
            Debug.Log("YModem HandleBluetoothMessage status="+status+" message="+message+" otaCallback==null?"+(otaCallback == null)+" yModem==null?"+(yModem == null));
            if (otaCallback == null || yModem == null) return; // 没有在进行OTA升级或YModem未初始化
            
            // 使用UniTask异步处理，确保在主线程中执行
            HandleBluetoothMessageAsync(status, message).Forget();
        }
        
        private static async UniTaskVoid HandleBluetoothMessageAsync(int status, string message)
        {
            // 确保在主线程中执行
            await UniTask.SwitchToMainThread();
            
            Debug.Log($"{TAG}: 接收到蓝牙消息 - Status: {status}, Message: {message}");
            
            // 将十六进制字符串转换为字节数组
            byte[] data = HexStringToByteArray(message);
            
            if (data.Length > 0)
            {
                // 【iOS平台优化】检测设备错误消息
                if (data.Length > 20)
                {
                    try
                    {
                        string receivedText = System.Text.Encoding.UTF8.GetString(data);
                        if (receivedText.Contains("Failed to receive the file"))
                        {
                            Debug.LogError($"{TAG}: 设备报告文件接收失败，重新开始传输");
                            if (currentStep == OtaStep.TransferringData)
                            {
                                await RestartOtaProcessAsync();
                                return;
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        // 忽略文本转换错误，继续正常处理
                        Debug.Log($"{TAG}: 无法解析为文本消息: {e.Message}");
                    }
                }
                
                // 记录接收到的数据内容（用于调试）
                Debug.Log($"{TAG}: 收到 {data.Length} 字节数据: {string.Join(" ", System.Array.ConvertAll(data, b => $"0x{b:X2}"))}");
                
                // 简单记录数据类型（用于调试）
                if (data.Length == 1)
                {
                    switch (data[0])
                    {
                        case 0x06: // ACK
                            Debug.Log($"{TAG}: 收到ACK响应");
                            currentPacketRetryCount = 0; // 重置重试计数
                            break;
                        case 0x15: // NAK
                            Debug.LogWarning($"{TAG}: 收到NAK响应，数据包可能损坏或CRC错误");
                            currentPacketRetryCount++;
                            break;
                        case 0x43: // 'C'
                            Debug.Log($"{TAG}: 收到'C'字符，设备准备接收数据");
                            break;
                        case 0x18: // CAN
                            Debug.LogError($"{TAG}: 收到CAN，设备取消传输");
                            break;
                        default:
                            Debug.Log($"{TAG}: 收到控制字符: 0x{data[0]:X2}");
                            break;
                    }
                }
                else
                {
                    // 多字节数据，分析第一个和最后一个字节
                    Debug.Log($"{TAG}: 收到多字节数据，第一个字节: 0x{data[0]:X2}, 最后一个字节: 0x{data[data.Length-1]:X2}");
                }
                
                // 【关键修复】直接传递完整数据给YModem处理，不做拆分
                // 这与Java版本保持一致：yModem.onReceiveData(data);
                yModem.OnReceiveData(data);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private static void RegisterBluetoothEvents()
        {
            if (!DeviceConnector.BluetoothHardwareEnabled)
            {
                return;
            }

            // 确保蓝牙回调已注册
            Plugins.BlueTooth.RegisterHandler();
            
            // 注册枪的状态和消息事件（作为示例）
            Plugins.BlueTooth.GunStatusEvent += HandleBluetoothStatus;
            Plugins.BlueTooth.GunMessageEvent += HandleBluetoothMessage;
            
            Debug.Log($"{TAG}: 已注册蓝牙事件监听");
        }
        
        private static void UnregisterBluetoothEvents()
        {
            // 取消注册枪的状态和消息事件
            Plugins.BlueTooth.GunStatusEvent -= HandleBluetoothStatus;
            Plugins.BlueTooth.GunMessageEvent -= HandleBluetoothMessage;
            
            Debug.Log($"{TAG}: 取消注册蓝牙事件监听");
        }
        
        /// <summary>
        /// 启动连接超时计时器
        /// </summary>
        private static void StartConnectionTimeout()
        {
            // 取消之前的计时器
            CancelConnectionTimeout();
            
            // 创建新的超时令牌
            connectionTimeoutCts = new System.Threading.CancellationTokenSource();
            isConnectionTimedOut = false;
            
            Debug.Log($"{TAG}: 启动连接超时计时器，超时时间：{CONNECTION_TIMEOUT_SECONDS}秒");
            
            // 启动超时检查任务
            CheckConnectionTimeoutAsync().Forget();
        }
        
        /// <summary>
        /// 取消连接超时计时器
        /// </summary>
        private static void CancelConnectionTimeout()
        {
            if (connectionTimeoutCts != null)
            {
                connectionTimeoutCts.Cancel();
                connectionTimeoutCts.Dispose();
                connectionTimeoutCts = null;
                Debug.Log($"{TAG}: 连接超时计时器已取消");
            }
        }
        
        /// <summary>
        /// 【UniTask版本】异步检查连接超时
        /// </summary>
        private static async UniTaskVoid CheckConnectionTimeoutAsync()
        {
            try
            {
                // 确保在主线程中执行
                await UniTask.SwitchToMainThread();
                
                // 等待指定的超时时间
                await UniTask.Delay(CONNECTION_TIMEOUT_SECONDS * 1000, cancellationToken: connectionTimeoutCts.Token);
                
                // 如果到达这里说明超时了
                if (!isConnectionTimedOut && currentStep == OtaStep.ConnectingDevice)
                {
                    isConnectionTimedOut = true;
                    Debug.LogError($"{TAG}: 设备连接超时，超过{CONNECTION_TIMEOUT_SECONDS}秒未连接成功");
                    FailWithReason(ErrorCode.CONNECTION_TIMEOUT, $"设备连接超时，超过{CONNECTION_TIMEOUT_SECONDS}秒未连接成功");
                }
            }
            catch (System.OperationCanceledException)
            {
                // 超时被取消（正常情况，连接成功或手动停止）
                Debug.Log($"{TAG}: 连接超时计时器被正常取消");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{TAG}: 连接超时检查异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// 重新开始OTA升级流程
        /// 当升级过程中收到设备重连回调时，从头开始整个升级流程
        /// </summary>
        private static async UniTask RestartOtaProcessAsync()
        {
            // 确保在主线程中执行
            await UniTask.SwitchToMainThread();
            
            Debug.Log($"{TAG}: 重新开始OTA升级流程，停止当前进度并重置状态");
            
            // 停止当前的YModem传输
            if (yModem != null)
            {
                yModem.Stop();
                yModem = null;
            }
            
            // 清空写入队列
            writeQueue.Clear();
            isWriting = false;
            currentPacketRetryCount = 0;
            
            // 重置状态变量，但保持回调和设备信息
            isOtaModeEntered = false;
            currentStep = OtaStep.ConnectingDevice;
            
            UpdateStatus("设备重新连接，正在重新开始升级流程...");
            
            // 等待一小段时间确保设备稳定
            await UniTask.Delay(1000);
            
            Debug.Log($"{TAG}: 重新开始发送OTA指令");
            await SendOtaCommandAsync();
        }
        
        private static void SendOtaCommand()
        {
            // 同步入口，调用异步版本
            SendOtaCommandAsync().Forget();
        }
        
        /// <summary>
        /// 【UniTask版本】异步发送OTA指令，确保在主线程中执行
        /// </summary>
        private static async UniTask SendOtaCommandAsync()
        {
            // 确保在主线程中执行
            await UniTask.SwitchToMainThread();
            
            UpdateOtaStep(OtaStep.SendingOtaCommand);
            UpdateStatus("发送OTA升级指令...");
            
            Debug.Log($"{TAG}: 发送OTA指令: {otaCommand}");
            
            try
            {
                // 通过蓝牙发送OTA指令
                if (!DeviceConnector.BluetoothHardwareEnabled)
                {
                    FailWithReason(ErrorCode.OTA_COMMAND_SEND_FAILED, "蓝牙功能已关闭");
                    return;
                }

                Plugins.BlueTooth.SendMessageToDevice(1, otaCommand); // 使用gun类型发送
                
                // 【UniTask转换】等待一小段时间让设备进入OTA模式，然后启动YModem
                DelayStartYmodemAsync().Forget();
            }
            catch (System.Exception e)
            {
                FailWithReason(ErrorCode.OTA_COMMAND_SEND_FAILED, "OTA指令发送失败: " + e.Message);
            }
        }
        
        private static async UniTaskVoid DelayStartYmodemAsync()
        {
            // 确保在主线程中执行
            await UniTask.SwitchToMainThread();
            
            // 等待2秒让设备进入OTA模式
            await UniTask.Delay(2000); 
            
            StartYmodem();
        }
        
        private static void StartYmodem()
        {
            UpdateOtaStep(OtaStep.StartingYmodem);
            UpdateStatus("启动YModem传输...");
            
            isOtaModeEntered = true;
            
            // 从完整路径中提取文件名
            var fileName = System.IO.Path.GetFileName(firmwareFilePath);
            
            Debug.Log($"{TAG}: 准备开始YModem传输 - 文件: {fileName}");
            
            try
            {
                yModem = new YModem.Builder()
                    .FilePath(firmwareFilePath)
                    .FileName(fileName)
                    .CheckMd5("")
                    .SendSize(128) // 使用较小的包大小适应BLE，从1024改为128
                    .Callback(new YModemListenerImpl())
                    .Build();
                
                Debug.Log($"{TAG}: YModem实例创建完成，开始启动传输");
                yModem.Start(null);
                
                UpdateOtaStep(OtaStep.TransferringData);
                UpdateStatus("YModem传输已启动，正在传输固件...");
            }
            catch (System.Exception e)
            {
                FailWithReason(ErrorCode.YMODEM_INIT_FAILED, "YModem初始化失败: " + e.Message);
            }
        }
        
        private static void SendDataViaBle(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            
            Debug.Log($"{TAG}: 准备BLE数据发送 - 原始数据长度: {data.Length} 字节");
            
            // 【关键修复】避免在后台线程中直接操作协程
            // 将数据加入队列，由主线程处理
            if (data.Length <= MAX_PACKET_SIZE)
            {
                // 数据小于等于MTU限制，直接加入队列
                EnqueueDataForSending(data);
            }
            else
            {
                // 数据超过MTU限制，需要分包发送
                int packetCount = (data.Length + MAX_PACKET_SIZE - 1) / MAX_PACKET_SIZE;
                Debug.Log($"{TAG}: 数据需要分包发送 - 包数: {packetCount}");
                
                for (int i = 0; i < data.Length; i += MAX_PACKET_SIZE)
                {
                    int length = System.Math.Min(MAX_PACKET_SIZE, data.Length - i);
                    byte[] packet = new byte[length];
                    System.Array.Copy(data, i, packet, 0, length);
                    EnqueueDataForSending(packet);
                }
            }
            
            // 开始处理发送队列（在主线程中执行）
            ProcessWriteQueue();
        }
        
        private static void EnqueueDataForSending(byte[] data)
        {
            writeQueue.Enqueue(data);
            // Debug.Log($"{TAG}: 数据包已加入发送队列 - 长度: {data.Length} 字节，队列长度: {writeQueue.Count}");
        }
        
        private static void ProcessWriteQueue()
        {
            // 【UniTask转换】无论在什么线程，都使用UniTask异步处理
            // UniTask.SwitchToMainThread 会自动处理线程切换
            ProcessWriteQueueAsync().Forget();
        }
        
        /// <summary>
        /// 【UniTask版本】异步处理发送队列，确保在主线程中执行所有BLE操作
        /// </summary>
        private static async UniTaskVoid ProcessWriteQueueAsync()
        {
            // 确保在主线程中执行
            await UniTask.SwitchToMainThread();
            
            // Debug.Log($"{TAG}: 开始处理发送队列，队列长度: {writeQueue.Count}");
            
            while (writeQueue.Count > 0)
            {
                if (isWriting)
                {
                    await UniTask.Delay(10); // 等待当前写操作完成
                    continue;
                }
                
                isWriting = true;
                byte[] data = writeQueue.Dequeue();
                
                string hexData = BytesToHex(data);
                Debug.Log($"{TAG}: 发送BLE数据包 ({writeQueue.Count + 1}包剩余) - 长度: {data.Length} 字节");
                // Debug.Log($"{TAG}: 数据内容: {hexData}");
                
                bool sendSuccess = false;
                int retryCount = 0;
                
                while (!sendSuccess && retryCount < MAX_RETRY_TIMES)
                {
                    // 尝试发送数据
                    System.Exception sendException = null;
                    try
                    {
                        if (!DeviceConnector.BluetoothHardwareEnabled)
                        {
                            FailWithReason(ErrorCode.BLE_WRITE_FAILED, "蓝牙功能已关闭");
                            return;
                        }

                        // 【关键修复】确保数据完整性 - 直接发送完整十六进制字符串
                        Debug.Log($"{TAG}: 尝试发送 (第{retryCount + 1}次): {hexData}");
                        Plugins.BlueTooth.SendMessageToDevice(1, hexData);
                        sendSuccess = true;
                        Debug.Log($"{TAG}: 数据包发送成功 (尝试 {retryCount + 1}/{MAX_RETRY_TIMES})");
                    }
                    catch (System.Exception e)
                    {
                        sendException = e;
                        Debug.LogError($"{TAG}: 数据包发送异常: {e}");
                    }
                    
                    // 处理发送失败的情况
                    if (!sendSuccess)
                    {
                        retryCount++;
                        Debug.LogWarning($"{TAG}: 数据包发送失败 (尝试 {retryCount}/{MAX_RETRY_TIMES}): {sendException?.Message}");
                        
                        if (retryCount < MAX_RETRY_TIMES)
                        {
                            // 【性能优化】适度减少重试间隔，提升约1倍速度
                            #if UNITY_IOS
                            await UniTask.Delay(400); // iOS平台重试间隔从1000ms减少到400ms
                            #else
                            await UniTask.Delay(200); // 其他平台重试间隔从500ms减少到200ms
                            #endif
                        }
                        else
                        {
                            Debug.LogError($"{TAG}: 数据包发送彻底失败，已重试{MAX_RETRY_TIMES}次");
                            isWriting = false;
                            FailWithReason(ErrorCode.BLE_WRITE_FAILED, "BLE数据发送失败: " + sendException?.Message);
                            return;
                        }
                    }
                }
                
                if (sendSuccess)
                {
                    // 【性能优化】适度减少发送间隔，提升约1倍速度，保持稳定性
                    #if UNITY_IOS
                    await UniTask.Delay(15); // iOS平台从30ms减少到15ms
                    #else
                    await UniTask.Delay(5); // 其他平台从10ms减少到5ms
                    #endif
                    isWriting = false;
                }
            }
            
            Debug.Log($"{TAG}: 发送队列处理完成，所有分包已发送");
        }
        
        private static void UpdateOtaStep(OtaStep step)
        {
            currentStep = step;
            string stepInfo = $"【步骤 {(int)step + 1}/7】{GetStepDescription(step)}";
            Debug.Log($"{TAG}: {stepInfo}");
        }
        
        private static string GetStepDescription(OtaStep step)
        {
            switch (step)
            {
                case OtaStep.Initializing: return "初始化";
                case OtaStep.ConnectingDevice: return "连接设备";
                case OtaStep.SendingOtaCommand: return "发送OTA指令";
                case OtaStep.StartingYmodem: return "启动YModem";
                case OtaStep.TransferringData: return "传输数据";
                case OtaStep.Completing: return "完成升级";
                case OtaStep.Failed: return "升级失败";
                default: return "未知状态";
            }
        }
        
        private static void UpdateStatus(string status)
        {
            Debug.Log($"{TAG}: {status}");
            otaCallback?.OnStatusUpdate(status);
        }
        
        private static void FailWithReason(int errorCode, string errorMessage)
        {
            UpdateOtaStep(OtaStep.Failed);
            Debug.LogError($"{TAG}: OTA升级失败 [{errorCode}]: {errorMessage}");
            
            // 停止升级并断开连接
            StopOTA();
            
            // 通知失败回调
            otaCallback?.OnFailed(errorCode, errorMessage);
        }
        
        #endregion
        
        #region Utility Methods
        
        private static byte[] HexStringToByteArray(string hexString)
        {
            if (string.IsNullOrEmpty(hexString)) return new byte[0];
            
            // 移除可能的空格和特殊字符
            hexString = hexString.Replace(" ", "").Replace("-", "").Replace(":", "");
            
            int length = hexString.Length;
            byte[] data = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                data[i / 2] = System.Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return data;
        }
        
        private static string BytesToHex(byte[] bytes)
        {
            var result = new System.Text.StringBuilder();
            foreach (byte b in bytes)
            {
                result.Append(b.ToString("X2"));
            }
            return result.ToString();
        }
        
        #endregion
        
        #region YModem Listener Implementation
        
        private class YModemListenerImpl : IYModemListener
        {
            public void OnDataReady(byte[] data)
            {
                Debug.Log($"{TAG}: YModem要求发送数据: {data.Length} 字节");
                UpdateStatus($"发送数据包: {data.Length} 字节");
                
                // 【UniTask转换】直接发送数据，UniTask会处理主线程调度
                try 
                {
                    SendDataViaBle(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{TAG}: 发送数据时出错: {e.Message}");
                    UnityEngine.Debug.LogError($"{TAG}: 数据发送失败: {e}");
                }
            }
            
            public void OnProgress(int currentSent, int total)
            {
                // 修复进度计算：确保不超过100%
                int adjustedCurrentSent = System.Math.Min(currentSent, total);
                
                Debug.Log($"{TAG}: YModem传输进度: {adjustedCurrentSent}/{total} ({(adjustedCurrentSent * 100 / total)}%)");
                
                UpdateStatus($"传输进度: {adjustedCurrentSent}/{total} 字节 ({adjustedCurrentSent * 100 / total}%)");
                
                // 回调进度更新
                otaCallback?.OnProgress(adjustedCurrentSent, total);
            }
            
            public void OnSuccess()
            {
                Debug.Log($"{TAG}: YModem传输成功，固件升级完成");
                UpdateOtaStep(OtaStep.Completing);
                UpdateStatus("YModem传输成功，固件升级完成");
                
                // 传输完成后断开连接并通知成功
                StopOTA();
                otaCallback?.OnSuccess();
            }
            
            public void OnFailed(string reason)
            {
                Debug.LogError($"{TAG}: YModem传输失败: {reason}");
                FailWithReason(ErrorCode.YMODEM_TRANSFER_FAILED, "YModem传输失败: " + reason);
            }
        }
        
        #endregion
        
        #region Example Implementation
        
        /// <summary>
        /// OTA回调示例实现
        /// </summary>
        public class ExampleOTACallback : IOTACallback
        {
            public void OnProgress(int currentSent, int total)
            {
                float progress = (float)currentSent / total * 100f;
                Debug.Log($"OTA升级进度: {progress:F1}% ({currentSent}/{total})");
                
                // 这里可以更新UI进度条
                // progressBar.value = progress / 100f;
            }
            
            public void OnStatusUpdate(string status)
            {
                Debug.Log($"OTA状态更新: {status}");
                
                // 这里可以更新UI状态显示
                // statusText.text = status;
            }
            
            public void OnSuccess()
            {
                Debug.Log("🎉 OTA升级成功完成！");
                
                // 这里可以显示成功提示
                // ShowSuccessDialog("固件升级成功！");
            }
            
            public void OnFailed(int errorCode, string errorMessage)
            {
                Debug.LogError($"❌ OTA升级失败: [{errorCode}] {errorMessage}");
                
                // 这里可以显示错误提示
                // ShowErrorDialog($"固件升级失败: {errorMessage}");
            }
        }
        
        #endregion
    }
    
    /* 
    使用示例:
    
    // 1. 【UniTask版本】不再需要设置协程运行器！
    
    // 2. 创建回调实例
    var callback = new YModem.BluetoothOtaManager.ExampleOTACallback();
    
    // 3. 开始OTA升级
    YModem.BluetoothOtaManager.StartOTA(
        "/path/to/firmware.bin",     // 固件文件路径
        "PIU_Device_001",            // 蓝牙设备名称
        callback,                    // 升级回调
        "5A0007230510000000A5"       // 可选的自定义OTA指令
    );
    
    // 4. 如需停止升级
    YModem.BluetoothOtaManager.StopOTA();
    
    注意事项:
    1. 需要先确保蓝牙权限已获取
    2. 需要在现有的BlueTooth.StatusHandler和MessageHandler中集成对应的处理逻辑
    3. 固件文件需要存在于指定路径
    4. 设备需要支持对应的OTA指令格式
    */
    
    #endregion
}