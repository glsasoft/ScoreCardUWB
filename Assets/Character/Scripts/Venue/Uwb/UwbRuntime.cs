using System.IO;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// UWB 定位入口。Editor 启动默认 Mock；正式包启动 LiveMqtt（已保存 broker），连不上不回落 Mock。
    /// Mock/Replay 在正式包仅会话内手动「应用」，不作为开机默认写盘。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)]
    public class UwbRuntime : MonoBehaviour
    {
        [Header("总开关")]
        [SerializeField] bool enableUwb = true;

        [Header("数据源（开机强制 LiveMqtt，此处仅编辑器预览）")]
        [SerializeField] UwbSourceMode sourceMode = UwbSourceMode.LiveMqtt;

        [Header("MQTT（LiveMqtt）")]
        [SerializeField] string mqttBrokerUrl = "mqtt://192.168.0.15:1883";
        [SerializeField] string mqttTopic = "ips/pos";
        [SerializeField] string mqttClientId = "";

        [Header("UDP（LiveUdp）")]
        [SerializeField] int udpListenPort = 9000;
        [SerializeField] string udpLocalAddress = "";

        [Header("Mock")]
        [SerializeField] int mockTagCount = 20;
        [SerializeField] bool mockRosterCycle;
        [SerializeField] float mockAreaX = 18f;
        [SerializeField] float mockAreaY = 10f;
        [SerializeField] float mockMoveSpeed = 1.2f;
        [SerializeField] float mockIntervalSec = 0.1f;
        [SerializeField] bool mockSimulateGlitch = true;

        static readonly int[] MockRosterCycleSizes = { 10, 20, 40 };
        int _mockRosterCycleIndex;
        /// <summary>固定人数偏好（循环模式换档时不改，便于切回固定）。</summary>
        int _mockFixedTagCount = 20;

        [Header("Replay")]
        [SerializeField] string replayFilePath = "";
        [SerializeField] bool replayLoop = true;
        [SerializeField] float replaySpeed = 1f;
        [SerializeField] float replayRangeStart = -1f;
        [SerializeField] float replayRangeEnd = -1f;

        [Header("录制目录")]
        [SerializeField] string recordDirectory = "";
        [SerializeField] int recordMaxQueue = 20000;

        [Header("场馆映射")]
        [SerializeField] UwbCoordinateMapper mapper = new UwbCoordinateMapper();
        [SerializeField] UwbTagRegistry tagRegistry = new UwbTagRegistry();

        [Header("推送到 VenuePlayerSync")]
        [SerializeField] VenuePlayerSync sync;
        [SerializeField] bool usePushAll = false;
        [SerializeField] float pushAllInterval = 0.1f;

        [Header("渲染前纠偏（地图匹配，会让位置多滞后 lagFrames 帧）")]
        [SerializeField] bool enableCorrection = true;
        [SerializeField] int correctionLagFrames = 5;
        [SerializeField] float correctionGridCell = 0.25f;
        [SerializeField] bool correctionDebugDraw = true;

        [Header("与旧 MockFeed 共存")]
        [SerializeField] bool disableLegacyMockFeed = true;

        [Header("设置面板")]
        [SerializeField] bool ensureSettingsPanel = true;

        IUwbTransport _transport;
        UwbMqttTransport _mqtt;
        UwbVenueBridge _bridge;
        UwbRecorder _recorder;
        const float CorrectionAgentRadiusM = 0.15f;
        UwbPositionCorrector _corrector;
        UwbVenueWalkGrid _physGrid;
        float _baseInterpolationDelay;
        float _maxFrameDt;
        float _logTimer;
        bool _alertShown;

        public UwbSourceMode ActiveMode => sourceMode;
        public int AcceptedCount => _bridge != null ? _bridge.Accepted : 0;
        public int TrackedTags => _bridge != null ? _bridge.TrackedTags : 0;
        public string RecordPath => _recorder != null ? _recorder.FilePath : null;
        public string RecordDirectory =>
            string.IsNullOrEmpty(recordDirectory) ? UwbRecordStore.DefaultDirectory : recordDirectory;
        public bool IsMqttConnected => _mqtt != null && _mqtt.IsRunning;

        void Awake()
        {
            if (sync == null)
                sync = GetComponent<VenuePlayerSync>();
            if (string.IsNullOrEmpty(recordDirectory))
                recordDirectory = UwbRecordStore.DefaultDirectory;

            // 只加载连接/偏移等参数；运行模式不跟盘
            UwbConfigData saved = UwbConfigFile.LoadOrNull();
            if (saved != null)
                ApplyConfig(saved, restart: false, applySourceMode: false);

            // Editor 启动默认 Mock 方便调试；正式包（exe/android）启动固定连 MQTT
            sourceMode = Application.isEditor ? UwbSourceMode.Mock : UwbSourceMode.LiveMqtt;

            if (ensureSettingsPanel)
                UwbSettingsPanel.EnsureExists();
        }

        void OnEnable()
        {
            if (!enableUwb)
                return;
            if (disableLegacyMockFeed)
            {
                var legacy = GetComponent<VenuePlayerMockFeed>();
                if (legacy != null)
                    legacy.playing = false;
            }

            StartPipeline();
        }

        void OnDisable()
        {
            StopPipeline();
        }

        void Update()
        {
            if (!enableUwb)
                return;
            float dt = Time.deltaTime;
            _transport?.Tick(dt);
            _bridge?.Tick(dt);

            // 纠偏输出比原始数据晚 lagFrames 帧，回放延迟要同步加大，否则位置会卡顿
            if (_corrector != null && sync != null)
                sync.interpolationDelay = _baseInterpolationDelay + _corrector.LagSeconds;

            if (dt > _maxFrameDt)
                _maxFrameDt = dt;

            _logTimer += dt;
            if (_logTimer < 5f || _bridge == null)
                return;
            _logTimer = 0f;
            Debug.Log(
                $"[UwbFrame] maxFrameDt={_maxFrameDt * 1000f:0}ms res={Screen.width}x{Screen.height} " +
                $"delay={(sync != null ? sync.interpolationDelay : 0f):0.###}s " +
                $"fps={(1f / Mathf.Max(0.0001f, Time.smoothDeltaTime)):0} " +
                $"backSteps={(sync != null ? sync.BackSteps : 0)} maxBack={(sync != null ? sync.MaxBackStepSec : 0f):0.###}s");
            _maxFrameDt = 0f;
            if (sync != null)
            {
                float avgGap = sync.ArrivalCount > 0 ? sync.SumArrivalGapSec / sync.ArrivalCount : 0f;
                Debug.Log(
                    $"[UwbSmooth] players={sync.PlayerCount} frames={sync.PresentFrames} " +
                    $"stall={sync.StallFrames} spike={sync.SpikeFrames} maxStep={sync.MaxStepM:0.###}m " +
                    $"clamped={sync.ClampedFrames} arrivalAvg={avgGap * 1000f:0}ms arrivalMax={sync.MaxArrivalGapSec * 1000f:0}ms " +
                    $"dataJump>1m={sync.DataJumps} maxDataJump={sync.MaxDataJumpM:0.##}m");
                sync.ResetBackStats();
            }
            Debug.Log(
                $"[UwbRuntime] mode={sourceMode} accepted={_bridge.Accepted} fail={_bridge.ParseFail} " +
                $"tags={_bridge.TrackedTags} mqtt={(IsMqttConnected ? "up" : "down")}");
            if (_corrector != null && _corrector.Outputs > 0)
            {
                Debug.Log(
                    $"[UwbCorrect] out={_corrector.Outputs} viterbi={_corrector.ViterbiRuns} moved>0.3m={_corrector.Moved} " +
                    $"avgShift={(_corrector.SumDisplacement / _corrector.Outputs):0.###}m " +
                    $"maxShift={_corrector.MaxDisplacement:0.##}m breaks={_corrector.Breaks} " +
                    $"lag={_corrector.LagSeconds:0.##}s");
                _corrector.ResetStats();
            }
        }

        public UwbConfigData ExportConfig()
        {
            Vector3 offset = mapper != null ? mapper.offsetMeters : Vector3.zero;
            Vector3 scale = mapper != null ? mapper.scale : Vector3.one;
            return new UwbConfigData
            {
                enableUwb = enableUwb,
                // 导出会话态仅供面板显示；写盘时再剥离 Mock/Replay
                sourceMode = sourceMode.ToString(),
                mqttBrokerUrl = mqttBrokerUrl,
                mqttTopic = mqttTopic,
                mqttClientId = mqttClientId,
                udpListenPort = udpListenPort,
                udpLocalAddress = udpLocalAddress,
                mockTagCount = _mockFixedTagCount > 0 ? _mockFixedTagCount : mockTagCount,
                mockRosterCycle = mockRosterCycle,
                mockAreaX = mockAreaX,
                mockAreaY = mockAreaY,
                mockMoveSpeed = mockMoveSpeed,
                mockIntervalSec = mockIntervalSec,
                replayFilePath = replayFilePath,
                replayLoop = replayLoop,
                replaySpeed = replaySpeed,
                replayRangeStart = replayRangeStart,
                replayRangeEnd = replayRangeEnd,
                offsetX = offset.x,
                offsetY = offset.y,
                offsetZ = offset.z,
                scaleX = scale.x,
                scaleY = scale.y,
                scaleZ = scale.z,
                flattenHeight = mapper == null || mapper.flattenHeight
            };
        }

        /// <summary>
        /// 写盘用配置：Mock/Replay 不落盘为开机模式，固定写成 LiveMqtt。
        /// </summary>
        public UwbConfigData ExportConfigForDisk()
        {
            UwbConfigData data = ExportConfig();
            if (sourceMode == UwbSourceMode.Mock || sourceMode == UwbSourceMode.Replay)
                data.sourceMode = UwbSourceMode.LiveMqtt.ToString();
            else
                data.sourceMode = sourceMode.ToString();
            return data;
        }

        public void ApplyConfig(UwbConfigData data, bool restart)
        {
            ApplyConfig(data, restart, applySourceMode: true);
        }

        public void ApplyConfig(UwbConfigData data, bool restart, bool applySourceMode)
        {
            if (data == null)
                return;

            enableUwb = data.enableUwb;
            if (applySourceMode)
                sourceMode = UwbConfigFile.ParseMode(data.sourceMode, sourceMode);

            mqttBrokerUrl = data.mqttBrokerUrl ?? mqttBrokerUrl;
            mqttTopic = data.mqttTopic ?? mqttTopic;
            mqttClientId = data.mqttClientId ?? "";
            udpListenPort = data.udpListenPort;
            udpLocalAddress = data.udpLocalAddress ?? "";
            _mockFixedTagCount = Mathf.Max(1, data.mockTagCount);
            mockRosterCycle = data.mockRosterCycle;
            if (mockRosterCycle)
            {
                _mockRosterCycleIndex = 0;
                mockTagCount = MockRosterCycleSizes[0];
            }
            else
            {
                mockTagCount = _mockFixedTagCount;
            }

            mockAreaX = data.mockAreaX;
            mockAreaY = data.mockAreaY;
            mockMoveSpeed = data.mockMoveSpeed > 0f ? data.mockMoveSpeed : mockMoveSpeed;
            mockIntervalSec = data.mockIntervalSec > 0f ? data.mockIntervalSec : mockIntervalSec;
            replayFilePath = data.replayFilePath ?? "";
            replayLoop = data.replayLoop;
            replaySpeed = data.replaySpeed > 0f ? data.replaySpeed : 1f;
            replayRangeStart = data.replayRangeStart;
            replayRangeEnd = data.replayRangeEnd;

            if (mapper == null)
                mapper = new UwbCoordinateMapper();
            mapper.offsetMeters = new Vector3(data.offsetX, data.offsetY, data.offsetZ);
            mapper.scale = new Vector3(
                data.scaleX == 0f ? 1f : data.scaleX,
                data.scaleY == 0f ? 1f : data.scaleY,
                data.scaleZ == 0f ? 1f : data.scaleZ);
            mapper.flattenHeight = data.flattenHeight;

            if (restart && isActiveAndEnabled)
                RestartPipeline();
        }

        [ContextMenu("Restart Pipeline")]
        public void RestartPipeline()
        {
            StopPipeline();
            // 切换模式/Mock 人数前清场，避免旧玩家残留
            if (sync != null)
                sync.ClearAllPlayers();
            if (tagRegistry != null)
            {
                tagRegistry.bindings.Clear();
                tagRegistry.Rebuild();
            }

            if (enableUwb && isActiveAndEnabled)
                StartPipeline();
        }

        void StartPipeline()
        {
            _alertShown = false;
            if (sync == null)
            {
                Debug.LogError("[UwbRuntime] VenuePlayerSync missing");
                return;
            }

            // Mock/Replay 用 PushAll，名单外玩家会立刻移除
            bool rosterMode = usePushAll ||
                              sourceMode == UwbSourceMode.Mock ||
                              sourceMode == UwbSourceMode.Replay;
            _bridge = new UwbVenueBridge(sync, mapper, tagRegistry, rosterMode, pushAllInterval);
            SetupCorrection();

            if (sourceMode == UwbSourceMode.LiveMqtt)
            {
                if (string.IsNullOrWhiteSpace(mqttBrokerUrl) ||
                    !UwbMqttTransport.TryParseBrokerUrl(mqttBrokerUrl, out _, out _, out _, out _))
                {
                    ShowServerAlert("请设置服务器", "未配置有效的 MQTT 地址，请打开设置填写 Broker。");
                    Debug.LogWarning("[UwbRuntime] MQTT 地址无效，不启动 Mock");
                    return;
                }
            }

            bool live = sourceMode == UwbSourceMode.LiveMqtt || sourceMode == UwbSourceMode.LiveUdp;
            _transport = CreateTransport();
            if (_transport == null)
            {
                if (sourceMode == UwbSourceMode.LiveMqtt)
                    ShowServerAlert("请设置服务器", "无法创建 MQTT 连接，请检查服务器配置。");
                else if (sourceMode == UwbSourceMode.Replay)
                    Debug.LogWarning("[UwbRuntime] Replay 无文件，不回放");
                return;
            }

            if (live)
            {
                Directory.CreateDirectory(RecordDirectory);
                _recorder = new UwbRecorder(RecordDirectory, recordMaxQueue);
                _bridge.SetRecorder(_recorder);
            }

            _mqtt = _transport as UwbMqttTransport;
            if (_mqtt != null)
            {
                _mqtt.Connected += OnMqttConnected;
                _mqtt.ConnectFailed += OnMqttConnectFailed;
            }

            _transport.RawMessageReceived += OnRawFromTransport;
            _transport.Start();
            // 钩子必须先于比分牌 Mock 启动：它一启动就会同步读取本场人数，晚了会拿到默认的 20 人
            if (sourceMode == UwbSourceMode.Mock)
            {
                UwbMockBattleHooks.SetMockPlayerCount = ApplyMockTagCount;
                UwbMockBattleHooks.TakeMatchPlayerCount = TakeMatchPlayerCount;
            }
            else
            {
                UwbMockBattleHooks.SetMockPlayerCount = null;
                UwbMockBattleHooks.TakeMatchPlayerCount = null;
            }

            // 定位 Mock 与当前模式比分牌 Mock 绑定启停（ActiveModeType，默认积分）
            ScoreCardBattleMockToggle.Apply(sourceMode == UwbSourceMode.Mock);

            Debug.Log($"[UwbRuntime] started mode={sourceMode} record={(live ? _recorder?.FilePath : "off")}");
        }

        /// <summary>本场人数：固定=手填；循环=10→20→40 并推进。</summary>
        int TakeMatchPlayerCount()
        {
            if (!mockRosterCycle)
                return Mathf.Max(2, _mockFixedTagCount > 0 ? _mockFixedTagCount : mockTagCount);

            int n = MockRosterCycleSizes[_mockRosterCycleIndex % MockRosterCycleSizes.Length];
            _mockRosterCycleIndex = (_mockRosterCycleIndex + 1) % MockRosterCycleSizes.Length;
            return n;
        }

        /// <summary>
        /// 仅重建定位 Mock 人数；不开关比分牌 Mock（避免循环换档时把积分循环杀掉）。
        /// </summary>
        void ApplyMockTagCount(int count)
        {
            count = Mathf.Clamp(count, 2, 40);
            if ((count & 1) != 0)
                count++;
            bool same = count == mockTagCount && _transport is UwbMockTransport;
            mockTagCount = count;
            if (!mockRosterCycle)
                _mockFixedTagCount = count;
            if (!enableUwb || !isActiveAndEnabled || sourceMode != UwbSourceMode.Mock)
                return;
            if (same)
                return;
            RestartMockTransportOnly();
        }

        void RestartMockTransportOnly()
        {
            if (_transport != null)
            {
                _transport.RawMessageReceived -= OnRawFromTransport;
                _transport.Dispose();
                _transport = null;
            }

            _bridge = null;
            _corrector = null;
            _physGrid = null;
            _recorder?.Dispose();
            _recorder = null;

            if (sync != null)
                sync.ClearAllPlayers();
            if (tagRegistry != null)
            {
                tagRegistry.bindings.Clear();
                tagRegistry.Rebuild();
            }

            bool rosterMode = usePushAll ||
                              sourceMode == UwbSourceMode.Mock ||
                              sourceMode == UwbSourceMode.Replay;
            _bridge = new UwbVenueBridge(sync, mapper, tagRegistry, rosterMode, pushAllInterval);
            SetupCorrection();
            _transport = CreateTransport();
            if (_transport == null)
                return;

            _transport.RawMessageReceived += OnRawFromTransport;
            _transport.Start();
            Debug.Log($"[UwbRuntime] mock roster resized tags={mockTagCount} (score mock kept)");
        }

        /// <summary>
        /// 实体墙网格：不做人体外扩（0.22m 会把贴墙走的真实点判成不可走），半径取小值，细格。
        /// 纠偏判断「是否穿墙」和 Mock 生成「真的穿墙」的故障都用它，两边拓扑一致。
        /// </summary>
        UwbVenueWalkGrid GetPhysicalGrid(VenueSpace space)
        {
            if (_physGrid == null)
            {
                _physGrid = UwbVenueWalkGrid.BuildForMap(
                    space, mockAreaX, mockAreaY, correctionGridCell, Layer.Obstacles, CorrectionAgentRadiusM);
            }

            return _physGrid;
        }

        void SetupCorrection()
        {
            if (!enableCorrection)
                return;

            VenueSpace space = sync.space != null ? sync.space : GetComponent<VenueSpace>();
            UwbVenueWalkGrid grid = GetPhysicalGrid(space);
            if (grid == null || !grid.Ready)
            {
                Debug.LogWarning("[UwbRuntime] 纠偏网格不可用，本次不纠偏");
                return;
            }

            _corrector = new UwbPositionCorrector(grid, correctionLagFrames);
            _baseInterpolationDelay = sync.interpolationDelay;
            _bridge.SetCorrector(_corrector, correctionDebugDraw ? space : null);
            Debug.Log($"[UwbRuntime] correction on lagFrames={correctionLagFrames} cell={grid.CellSize}");
        }

        void StopPipeline()
        {
            if (_corrector != null && sync != null)
                sync.interpolationDelay = _baseInterpolationDelay;
            _corrector = null;
            _physGrid = null;

            if (_mqtt != null)
            {
                _mqtt.Connected -= OnMqttConnected;
                _mqtt.ConnectFailed -= OnMqttConnectFailed;
                _mqtt = null;
            }

            if (_transport != null)
            {
                _transport.RawMessageReceived -= OnRawFromTransport;
                _transport.Dispose();
                _transport = null;
            }

            _recorder?.Dispose();
            _recorder = null;
            _bridge = null;
            UwbMockBattleHooks.SetMockPlayerCount = null;
            UwbMockBattleHooks.TakeMatchPlayerCount = null;
            ScoreCardBattleMockToggle.Apply(false);
        }

        void OnRawFromTransport(string raw)
        {
            _bridge?.OnRawMessage(raw);
        }

        void OnMqttConnected()
        {
            Debug.Log("[UwbRuntime] MQTT connected");
        }

        void OnMqttConnectFailed(string error)
        {
            // 主线程弹窗（MQTTnet 回调可能在后台线程）
            string msg = string.IsNullOrEmpty(error) ? "无法连接 MQTT 服务器" : error;
            UnityMainThread.Post(() =>
                ShowServerAlert("服务器连接失败", "请设置服务器 / 服务器连接失败\n" + msg));
        }

        void ShowServerAlert(string title, string body)
        {
            if (_alertShown)
                return;
            _alertShown = true;
            if (UwbSettingsPanel.Current != null)
                UwbSettingsPanel.Current.ShowAlert(title, body);
            else
                Debug.LogWarning("[UwbRuntime] " + title + " " + body);
        }

        IUwbTransport CreateTransport()
        {
            switch (sourceMode)
            {
                case UwbSourceMode.LiveMqtt:
                {
                    if (!UwbMqttTransport.TryParseBrokerUrl(mqttBrokerUrl, out string host, out int port,
                            out string user, out string pass))
                        return null;
                    return new UwbMqttTransport(host, port, user, pass, mqttTopic, mqttClientId);
                }
                case UwbSourceMode.LiveUdp:
                    return new UwbUdpTransport(udpListenPort, udpLocalAddress);
                case UwbSourceMode.Mock:
                {
                    VenueSpace space = sync != null ? sync.space : null;
                    if (space == null)
                        space = GetComponent<VenueSpace>();
                    // 按整张地图包围盒建可走网格，避免中心对称裁切漏区
                    UwbVenueWalkGrid grid = UwbVenueWalkGrid.BuildForMap(space, mockAreaX, mockAreaY);
                    return new UwbMockTransport(
                        mockTagCount, mockAreaX, mockAreaY, mockMoveSpeed, mockIntervalSec, grid,
                        mockSimulateGlitch, mockSimulateGlitch ? GetPhysicalGrid(space) : null);
                }

                case UwbSourceMode.Replay:
                {
                    string path = replayFilePath;
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        return null;
                    return new UwbReplayTransport(path, replayLoop, replaySpeed, replayRangeStart, replayRangeEnd);
                }
                default:
                    return null;
            }
        }
    }

    /// <summary>把后台线程回调抛回主线程（简易队列）。</summary>
    static class UnityMainThread
    {
        static readonly System.Collections.Concurrent.ConcurrentQueue<System.Action> Queue =
            new System.Collections.Concurrent.ConcurrentQueue<System.Action>();
        static bool _hooked;

        public static void Post(System.Action action)
        {
            if (action == null)
                return;
            EnsureHook();
            Queue.Enqueue(action);
        }

        static void EnsureHook()
        {
            if (_hooked)
                return;
            _hooked = true;
            var go = new GameObject("UwbMainThreadPump");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<Pump>();
        }

        sealed class Pump : MonoBehaviour
        {
            void Update()
            {
                while (Queue.TryDequeue(out System.Action a))
                {
                    try { a(); } catch (System.Exception e) { Debug.LogWarning(e.Message); }
                }
            }
        }
    }
}
