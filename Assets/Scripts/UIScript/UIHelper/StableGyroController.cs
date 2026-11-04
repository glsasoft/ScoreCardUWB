using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.UI
{
    public class StableGyroController : MonoBehaviour 
    {
        /// <summary>
        /// 陀螺仪灵敏度，控制旋转速度
        /// </summary>
        [Header("灵敏度设置")]
        [Range(0.1f, 5.0f)]
        public float sensitivity = 1.5f;
        /// <summary>
        /// 最大旋转速度（度/秒）
        /// </summary>
        [Range(10f, 360f)]
        public float maxRotationSpeed = 120f;
    
        /// <summary>
        /// 旋转平滑度，值越小响应越快，值越大越平滑
        /// </summary>
        [Header("平滑设置")]
        [Range(0.01f, 1.0f)]
        public float rotationSmoothness = 0.15f;
        /// <summary>
        /// 输入平滑度，控制陀螺仪输入的平滑程度
        /// </summary>
        [Range(0.01f, 0.5f)]
        public float inputSmoothness = 0.1f;
        /// <summary>
        /// 平均帧数，用于移动平均滤波
        /// </summary>
        [Range(1, 10)]
        public int averageFrames = 5;
    
        /// <summary>
        /// 死区阈值，小于此值的输入将被忽略
        /// </summary>
        [Header("稳定性设置")]
        [Range(0.01f, 1.0f)]
        public float deadZone = 0.15f;
        /// <summary>
        /// 跳变阈值，超过此值的突变将被过滤
        /// </summary>
        [Range(1.0f, 10.0f)]
        public float jumpThreshold = 3.0f;
        /// <summary>
        /// 静止检测阈值，小于此值时认为设备静止
        /// </summary>
        [Range(0.001f, 0.1f)]
        public float staticThreshold = 0.03f;
        /// <summary>
        /// 静止检测时间，设备需要保持静止的时间
        /// </summary>
        [Range(0.1f, 5f)]
        public float staticTime = 0.5f;
        /// <summary>
        /// 是否启用漂移补偿
        /// </summary>
        public bool enableDriftCompensation = true;
        /// <summary>
        /// 漂移补偿速度，控制漂移补偿的应用速度
        /// </summary>
        [Range(0.01f, 0.5f)]
        public float driftCompensationSpeed = 0.05f;
    
        /// <summary>
        /// 初始化等待时间，超过此时间将强制开始校准
        /// </summary>
        [Header("初始化设置")]
        [Range(1f, 10f)]
        public float initializationWaitTime = 2.0f;
        /// <summary>
        /// 稳定性检测时间，设备需要保持稳定的时间
        /// </summary>
        [Range(0.5f, 5f)]
        public float initStabilizationTime = 0.5f;
        /// <summary>
        /// 初始化稳定性阈值，小于此值时认为设备稳定
        /// </summary>
        [Range(0.001f, 0.1f)]
        public float initStabilityThreshold = 0.08f;
        /// <summary>
        /// 校准样本数量，收集的样本越多校准越精确但时间越长
        /// </summary>
        [Range(5, 100)]
        public int calibrationSamples = 10;
    
        /// <summary>
        /// 是否启用X轴旋转
        /// </summary>
        [Header("轴控制")]
        public bool enableX = true;
        /// <summary>
        /// 是否启用Y轴旋转
        /// </summary>
        public bool enableY = true;
        /// <summary>
        /// 是否启用Z轴旋转
        /// </summary>
        public bool enableZ = true;
    
        /// <summary>
        /// 是否启用自动校准
        /// </summary>
        [Header("校准设置")]
        public bool autoCalibrate = true;
        /// <summary>
        /// 自动校准间隔时间（秒）
        /// </summary>
        [Range(5f, 30f)]
        public float calibrationInterval = 15f;
        /// <summary>
        /// 重置旋转的按键
        /// </summary>
        public KeyCode resetKey = KeyCode.R;
        /// <summary>
        /// 手动校准的按键
        /// </summary>
        public KeyCode calibrateKey = KeyCode.C;
    
        /// <summary>
        /// 是否在控制台显示调试信息
        /// </summary>
        [Header("调试信息")]
        public bool showDebugInfo = true;
        /// <summary>
        /// 是否在屏幕上显示调试信息
        /// </summary>
        public bool showOnScreenDebug = true;
    
        /// <summary>
        /// 陀螺仪控制模式枚举
        /// </summary>
        public enum ControlMode 
        {
            /// <summary>
            /// 跟随设备模式：物体旋转跟随设备旋转
            /// </summary>
            FollowDevice,
            /// <summary>
            /// 相对旋转模式：物体根据设备旋转增量旋转
            /// </summary>
            RelativeRotation,
            /// <summary>
            /// 固定轴旋转模式：物体围绕固定轴旋转
            /// </summary>
            FixedAxis
        }
    
        /// <summary>
        /// 初始化状态枚举
        /// </summary>
        private enum InitializationState
        {
            /// <summary>
            /// 未开始初始化
            /// </summary>
            NotStarted,
            /// <summary>
            /// 等待设备稳定
            /// </summary>
            WaitingForStability,
            /// <summary>
            /// 正在校准中
            /// </summary>
            Calibrating,
            /// <summary>
            /// 初始化完成
            /// </summary>
            Completed
        }
    
        // 私有变量
        /// <summary>
        /// 目标旋转四元数，用于平滑插值
        /// </summary>
        private Quaternion targetRotation;
    
        /// <summary>
        /// 初始旋转四元数，用于重置旋转
        /// </summary>
        private Quaternion initialRotation;
    
        /// <summary>
        /// 上一帧的陀螺仪输入，用于检测突变
        /// </summary>
        private Vector3 lastGyroInput;
    
        /// <summary>
        /// 经过平滑处理的陀螺仪输入
        /// </summary>
        private Vector3 smoothedGyroInput;
    
        /// <summary>
        /// 陀螺仪偏移值，用于校准
        /// </summary>
        private Vector3 gyroOffset = Vector3.zero;
    
        /// <summary>
        /// 陀螺仪历史数据队列，用于移动平均滤波
        /// </summary>
        private Queue<Vector3> gyroHistory = new Queue<Vector3>();
    
        /// <summary>
        /// 上次校准时间，用于自动校准
        /// </summary>
        private float lastCalibrationTime;
    
        /// <summary>
        /// 陀螺仪是否已初始化
        /// </summary>
        private bool gyroInitialized = false;
    
        // 初始化相关
        /// <summary>
        /// 当前初始化状态
        /// </summary>
        private InitializationState initState = InitializationState.NotStarted;
    
        /// <summary>
        /// 初始化开始时间
        /// </summary>
        private float initStartTime;
    
        /// <summary>
        /// 稳定性检测开始时间
        /// </summary>
        private float stabilityCheckStartTime;
    
        /// <summary>
        /// 校准样本队列，用于收集校准数据
        /// </summary>
        private Queue<Vector3> calibrationSampleQueue = new Queue<Vector3>();
    
        /// <summary>
        /// 是否正在等待设备稳定
        /// </summary>
        private bool isWaitingForStability = false;
    
        // 静止检测相关
        /// <summary>
        /// 静止计时器，记录设备保持静止的时间
        /// </summary>
        private float staticTimer = 0f;
    
        /// <summary>
        /// 设备是否处于静止状态
        /// </summary>
        private bool isDeviceStatic = false;
    
        /// <summary>
        /// 上次静止检测的陀螺仪值
        /// </summary>
        private Vector3 lastStaticCheck = Vector3.zero;
    
        // 漂流补偿相关
        /// <summary>
        /// 漂流补偿值，用于修正陀螺仪漂流
        /// </summary>
        private Vector3 driftCompensation = Vector3.zero;
    
        /// <summary>
        /// 静止状态下的陀螺仪读数队列，用于计算漂流
        /// </summary>
        private Queue<Vector3> staticReadings = new Queue<Vector3>();
    
        /// <summary>
        /// 静止读数队列的最大容量
        /// </summary>
        private int staticReadingsCount = 30;
    
        // UI相关
        /// <summary>
        /// 调试信息显示区域
        /// </summary>
        private Rect debugRect = new Rect(10, 10, 350, 250);
    
        /// <summary>
        /// 在组件启动时初始化
        /// </summary>
        void Start() 
        {
            InitializeGyroscope();
            initialRotation = transform.rotation;
        }
    
        /// <summary>
        /// 初始化陀螺仪设备
        /// </summary>
        void InitializeGyroscope() 
        {
            if (SystemInfo.supportsGyroscope) 
            {
                Input.gyro.enabled = true;
                targetRotation = transform.rotation;
                lastCalibrationTime = Time.time;
            
                // 开始初始化流程
                StartInitializationProcess();
                Debug.Log("StableGyroController 开始陀螺仪初始化流程...");
            }
            else 
            {
                Debug.LogWarning("StableGyroController 设备不支持陀螺仪，将禁用陀螺仪控制");
                enabled = false;
            }
        }
    
        // 添加校准完成事件
        /// <summary>
        /// 陀螺仪校准完成事件的委托
        /// </summary>
        /// <param name="gyroOffset">校准后的陀螺仪偏移值</param>
        public delegate void CalibrationCompletedHandler(Vector3 gyroOffset);
    
        /// <summary>
        /// 陀螺仪校准完成时触发的事件
        /// </summary>
        public event CalibrationCompletedHandler OnCalibrationCompleted;
    
        /// <summary>
        /// 控制模式设置
        /// </summary>
        [Header("控制模式")]
        public ControlMode controlMode = ControlMode.FollowDevice;
        /// <summary>
        /// 是否反转X轴旋转
        /// </summary>
        public bool invertX = false;
        /// <summary>
        /// 是否反转Y轴旋转
        /// </summary>
        public bool invertY = false;
        /// <summary>
        /// 是否反转Z轴旋转
        /// </summary>
        public bool invertZ = false;
    
        /// <summary>
        /// 开始陀螺仪初始化流程
        /// </summary>
        void StartInitializationProcess()
        {
            initState = InitializationState.WaitingForStability;
            initStartTime = Time.time;
            stabilityCheckStartTime = Time.time;
            isWaitingForStability = true;
            calibrationSampleQueue.Clear();
        
            Debug.Log($"StableGyroController 等待设备稳定 ({initStabilizationTime}秒)...");
        }
    
        /// <summary>
        /// 更新陀螺仪初始化状态
        /// </summary>
        void UpdateInitialization()
        {
            if (initState == InitializationState.Completed) return;
        
            Vector3 currentGyro = Input.gyro.rotationRate;
        
            switch (initState)
            {
                case InitializationState.WaitingForStability:
                    // 检查设备是否稳定
                    float timeSinceStabilityCheck = Time.time - stabilityCheckStartTime;
                    bool isCurrentlyStable = currentGyro.magnitude < initStabilityThreshold;
                
                    if (isCurrentlyStable)
                    {
                        // 设备稳定，继续等待
                        if (timeSinceStabilityCheck >= initStabilizationTime)
                        {
                            // 稳定时间足够，开始校准
                            initState = InitializationState.Calibrating;
                            Debug.Log("StableGyroController 设备已稳定，开始校准...");
                        }
                    }
                    else
                    {
                        // 设备不稳定，重新开始稳定性检测
                        // 轻微晃动不重置稳定性检测，只有大幅晃动才重置
                        if (currentGyro.magnitude > initStabilityThreshold * 2)
                        {
                            stabilityCheckStartTime = Time.time;
                            Debug.Log("StableGyroController 检测到设备大幅晃动，重新等待稳定...");
                        }
                    }
                
                    // 超时保护
                    if (Time.time - initStartTime > initializationWaitTime)
                    {
                        Debug.LogWarning("StableGyroController 初始化超时，强制开始校准");
                        initState = InitializationState.Calibrating;
                    }
                    break;
                
                case InitializationState.Calibrating:
                    // 收集校准样本（一次收集多个样本以加快速度）
                    for (int i = 0; i < 5 && calibrationSampleQueue.Count < calibrationSamples; i++)
                    {
                        calibrationSampleQueue.Enqueue(currentGyro);
                    }
                
                    if (calibrationSampleQueue.Count >= calibrationSamples)
                    {
                        // 计算平均值作为偏移
                        Vector3 averageOffset = Vector3.zero;
                        foreach (Vector3 sample in calibrationSampleQueue)
                        {
                            averageOffset += sample;
                        }
                        gyroOffset = averageOffset / calibrationSampleQueue.Count;
                    
                        // 校准完成
                        initState = InitializationState.Completed;
                        gyroInitialized = true;
                        isWaitingForStability = false;
                    
                        Debug.Log($"StableGyroController 陀螺仪校准完成！偏移值: {gyroOffset}");
                        Debug.Log("StableGyroController 你现在可以旋转设备来控制立方体了");
                    
                        // 触发校准完成事件
                        OnCalibrationCompleted?.Invoke(gyroOffset);
                    }
                    break;
            }
        }
    
        void Update() 
        {
            if (!gyroInitialized) 
            {
                UpdateInitialization();
                // 即使在校准过程中也允许基本的旋转响应
                if (initState == InitializationState.Calibrating)
                {
                    HandleInput();
                    // 使用临时的简化旋转逻辑
                    Vector3 gyroInput = Input.gyro.rotationRate;
                    if (gyroInput.magnitude > deadZone)
                    {
                        Vector3 rotationDelta = gyroInput * (sensitivity * 0.5f) * Time.deltaTime * Mathf.Rad2Deg;
                        transform.Rotate(-rotationDelta.x, rotationDelta.y, -rotationDelta.z, Space.World);
                    }
                }
                return;
            }
        
            HandleInput();
            UpdateRotation();
            HandleCalibration();
        }
    
        void HandleInput() 
        {
            // 重置旋转
            if (Input.GetKeyDown(resetKey)) 
            {
                ResetRotation();
            }
        
            // 手动校准
            if (Input.GetKeyDown(calibrateKey)) 
            {
                RecalibrateGyro();
            }
        
            // 移除点击屏幕功能
            // if (Input.GetMouseButtonDown(0)) 
            // {
            //     ResetRotation();  // 先重置角度
            //     RecalibrateGyro(); // 再重新校准陀螺仪
            // }
        }
    
        void UpdateRotation() 
        {
            Vector3 gyroInput = GetStabilizedGyroInput();
        
            // 检测设备是否静止
            CheckDeviceStatic(gyroInput);
        
            // 如果设备静止且启用漂移补偿，则停止旋转
            if (isDeviceStatic && enableDriftCompensation) 
            {
                // 设备静止时，快速停止旋转
                gyroInput = Vector3.Lerp(gyroInput, Vector3.zero, 0.98f); // 增加停止速度
            
                // 收集静态读数用于漂流补偿
                UpdateDriftCompensation(Input.gyro.rotationRate);
            }
        
            // 应用轴控制
            if (!enableX) gyroInput.x = 0;
            if (!enableY) gyroInput.y = 0;
            if (!enableZ) gyroInput.z = 0;
        
            // 应用反向控制
            if (invertX) gyroInput.x = -gyroInput.x;
            if (invertY) gyroInput.y = -gyroInput.y;
            if (invertZ) gyroInput.z = -gyroInput.z;
        
            // 限制旋转速度
            gyroInput = Vector3.ClampMagnitude(gyroInput, maxRotationSpeed * Mathf.Deg2Rad);
        
            // 根据控制模式应用旋转
            switch (controlMode) 
            {
                case ControlMode.FollowDevice:
                    ApplyDeviceFollowRotation(gyroInput);
                    break;
                case ControlMode.RelativeRotation:
                    ApplyRelativeRotation(gyroInput);
                    break;
                case ControlMode.FixedAxis:
                    ApplyFixedAxisRotation(gyroInput);
                    break;
            }
        
            // 平滑插值到目标旋转
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness);
        }
    
        void ApplyDeviceFollowRotation(Vector3 gyroInput) 
        {
            Vector3 rotationDelta = gyroInput * sensitivity * Time.deltaTime * Mathf.Rad2Deg;
            targetRotation *= Quaternion.Euler(-rotationDelta.x, rotationDelta.y, -rotationDelta.z);
        }
    
        void ApplyRelativeRotation(Vector3 gyroInput) 
        {
            Vector3 rotationDelta = gyroInput * sensitivity * Time.deltaTime * Mathf.Rad2Deg;
            Quaternion deltaRotation = Quaternion.Euler(rotationDelta.x, rotationDelta.y, rotationDelta.z);
            targetRotation = targetRotation * deltaRotation;
        }
    
        void ApplyFixedAxisRotation(Vector3 gyroInput) 
        {
            Vector3 rotationDelta = gyroInput * sensitivity * Time.deltaTime * Mathf.Rad2Deg;
            if (enableX) targetRotation = Quaternion.AngleAxis(rotationDelta.x, Vector3.right) * targetRotation;
            if (enableY) targetRotation = Quaternion.AngleAxis(rotationDelta.y, Vector3.up) * targetRotation;
            if (enableZ) targetRotation = Quaternion.AngleAxis(rotationDelta.z, Vector3.forward) * targetRotation;
        }
    
        Vector3 GetStabilizedGyroInput() 
        {
            Vector3 rawGyroInput = Input.gyro.rotationRate;
        
            // 应用漂流补偿
            if (enableDriftCompensation) 
            {
                rawGyroInput -= driftCompensation;
            }
        
            // 应用基础偏移
            rawGyroInput -= gyroOffset;
        
            // 应用死区
            rawGyroInput = FilterDeadZone(rawGyroInput);
        
            // 检测并过滤突变
            if (Vector3.Distance(rawGyroInput, lastGyroInput) > jumpThreshold) 
            {
                rawGyroInput = lastGyroInput;
                if (showDebugInfo) 
                {
                    Debug.Log("StableGyroController 检测到陀螺仪数据突变，已过滤");
                }
            }
        
            // 移动平均滤波
            gyroHistory.Enqueue(rawGyroInput);
            if (gyroHistory.Count > averageFrames) 
            {
                gyroHistory.Dequeue();
            }
        
            Vector3 averageGyro = Vector3.zero;
            foreach (Vector3 gyro in gyroHistory) 
            {
                averageGyro += gyro;
            }
            averageGyro /= gyroHistory.Count;
        
            // 低通滤波
            smoothedGyroInput = Vector3.Lerp(smoothedGyroInput, averageGyro, inputSmoothness);
        
            lastGyroInput = rawGyroInput;
            return smoothedGyroInput;
        }
    
        Vector3 FilterDeadZone(Vector3 input) 
        {
            Vector3 result = input;
            if (Mathf.Abs(input.x) < deadZone) result.x = 0;
            if (Mathf.Abs(input.y) < deadZone) result.y = 0;
            if (Mathf.Abs(input.z) < deadZone) result.z = 0;
            return result;
        }
    
        void CheckDeviceStatic(Vector3 currentInput) 
        {
            bool isCurrentlyStatic = currentInput.magnitude < staticThreshold;
        
            if (isCurrentlyStatic) 
            {
                staticTimer += Time.deltaTime;
                if (staticTimer >= staticTime) 
                {
                    isDeviceStatic = true;
                }
            }
            else 
            {
                staticTimer = 0f;
                isDeviceStatic = false;
            }
        }
    
        void UpdateDriftCompensation(Vector3 rawGyroReading) 
        {
            if (isDeviceStatic) 
            {
                staticReadings.Enqueue(rawGyroReading);
                if (staticReadings.Count > staticReadingsCount) 
                {
                    staticReadings.Dequeue();
                }
            
                if (staticReadings.Count >= staticReadingsCount) 
                {
                    Vector3 averageDrift = Vector3.zero;
                    foreach (Vector3 reading in staticReadings) 
                    {
                        averageDrift += reading;
                    }
                    averageDrift /= staticReadings.Count;
                
                    // 使用可调节的漂流补偿速度
                    driftCompensation = Vector3.Lerp(driftCompensation, averageDrift, driftCompensationSpeed);
                
                    // 立即应用一部分漂流补偿到当前旋转
                    if (gyroInitialized && isDeviceStatic)
                    {
                        // 逐渐减小目标旋转的变化率
                        targetRotation = Quaternion.Slerp(targetRotation, transform.rotation, 0.1f);
                    }
                }
            }
        }
    
        void HandleCalibration() 
        {
            if (autoCalibrate && Time.time - lastCalibrationTime > calibrationInterval) 
            {
                AutoCalibrate();
                lastCalibrationTime = Time.time;
            }
        }
    
        void AutoCalibrate() 
        {
            Vector3 currentGyro = Input.gyro.rotationRate;
            if (currentGyro.magnitude < deadZone * 2) 
            {
                gyroOffset = Vector3.Lerp(gyroOffset, currentGyro, 0.1f);
            
                if (showDebugInfo) 
                {
                    Debug.Log($"StableGyroController 自动校准完成，偏移值: {gyroOffset}");
                }
            }
        }
    
        public void ResetRotation() 
        {
            targetRotation = initialRotation;
            gyroHistory.Clear();
            smoothedGyroInput = Vector3.zero;
        
            staticTimer = 0f;
            isDeviceStatic = false;
            staticReadings.Clear();
        
            Debug.Log("StableGyroController 立方体旋转已重置到初始状态");
        }
    
        public void RecalibrateGyro() 
        {
            // 保存当前旋转状态
            Quaternion currentRotation = transform.rotation;
        
            // 强制重新开始初始化流程
            gyroInitialized = false;
            StartInitializationProcess();
        
            // 使用更少的样本进行快速校准
            calibrationSamples = Mathf.Min(calibrationSamples, 5); // 减少到5个样本
        
            // 减少稳定性检测时间
            initStabilizationTime = 0.3f; // 减少到0.3秒
        
            // 增加稳定性阈值，更容易判定为稳定
            initStabilityThreshold = 0.1f;
        
            Debug.Log("StableGyroController 开始快速重新校准陀螺仪...");
        }
    
        // 提供手动完成初始化的方法（在已知设备稳定时使用）
        public void ForceCompleteInitialization()
        {
            if (!gyroInitialized)
            {
                gyroOffset = Input.gyro.rotationRate;
                initState = InitializationState.Completed;
                gyroInitialized = true;
                isWaitingForStability = false;
                Debug.Log($"StableGyroController 强制完成初始化，偏移值: {gyroOffset}");
            }
        }
    
        public void SetSensitivity(float newSensitivity)
        {
            sensitivity = Mathf.Clamp(newSensitivity, 0.1f, 5.0f);
        }
    
        public void SetControlMode(ControlMode newMode)
        {
            controlMode = newMode;
            Debug.Log($"StableGyroController 控制模式已切换为: {controlMode}");
        }
    
        // 添加新的公共方法：重置角度并校准
        public void ResetAndCalibrate()
        {
            ResetRotation();  // 先重置角度
            RecalibrateGyro(); // 再重新校准陀螺仪
        }
    
        // 添加新的公共方法：快速校准（不等待稳定性检测）
        public void QuickCalibrate()
        {
            // 保存当前旋转状态
            Quaternion currentRotation = transform.rotation;
        
            // 强制重新开始初始化流程
            gyroInitialized = false;
        
            // 直接进入校准阶段，跳过稳定性检测
            initState = InitializationState.Calibrating;
            calibrationSampleQueue.Clear();
        
            // 使用更少的样本进行快速校准
            calibrationSamples = Mathf.Min(calibrationSamples, 3); // 减少到3个样本
        
            Debug.Log("StableGyroController 开始极速校准陀螺仪...");
        }
#if UNITY_EDITOR
        void OnGUI()
        {
            if (showOnScreenDebug)
            {
                GUI.Box(debugRect, "");
                GUILayout.BeginArea(debugRect);
                GUILayout.Label("陀螺仪调试信息", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
                if (!gyroInitialized)
                {
                    GUILayout.Label($"初始化状态: {initState}");
                    if (initState == InitializationState.WaitingForStability)
                    {
                        float remainingTime = initStabilizationTime - (Time.time - stabilityCheckStartTime);
                        GUILayout.Label($"等待稳定: {Mathf.Max(0, remainingTime):F1}s");
                        GUILayout.Label($"当前晃动: {Input.gyro.rotationRate.magnitude:F3}");
                        GUILayout.Label($"稳定阈值: {initStabilityThreshold:F3}");
                    }
                    else if (initState == InitializationState.Calibrating)
                    {
                        GUILayout.Label($"校准进度: {calibrationSampleQueue.Count}/{calibrationSamples}");
                    }
                
                    if (GUILayout.Button("强制完成初始化", GUILayout.Width(150)))
                    {
                        ForceCompleteInitialization();
                    }
                }
                else
                {
                    GUILayout.Label($"原始数据: {Input.gyro.rotationRate.ToString("F3")}");
                    GUILayout.Label($"平滑数据: {smoothedGyroInput.ToString("F3")}");
                    GUILayout.Label($"偏移量: {gyroOffset.ToString("F3")}");
                    GUILayout.Label($"漂流补偿: {driftCompensation.ToString("F3")}");
                    GUILayout.Label($"当前旋转: {transform.rotation.eulerAngles.ToString("F1")}");
                    GUILayout.Label($"控制模式: {controlMode}");
                    GUILayout.Label($"设备静止: {(isDeviceStatic ? "是" : "否")} ({staticTimer:F1}s)");
                
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("重置", GUILayout.Width(60))) 
                    {
                        ResetRotation();
                    }
                    if (GUILayout.Button("校准", GUILayout.Width(60))) 
                    {
                        RecalibrateGyro();
                    }
                    GUILayout.EndHorizontal();
                }
            
                // 更新提示文本，移除点击屏幕提示
                // GUILayout.Label("点击屏幕重新校准");
                GUILayout.Label($"按{resetKey}键重置，按{calibrateKey}键校准");
            
                GUILayout.EndArea();
            }
        }
#endif
        /// <summary>
        /// 检查陀螺仪是否正在校准中
        /// </summary>
        /// <returns>如果陀螺仪正在校准中返回true，否则返回false</returns>
        public bool IsCalibrating()
        {
            return initState != InitializationState.Completed || !gyroInitialized;
        }
    }
}