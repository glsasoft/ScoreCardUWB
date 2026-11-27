using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// UWB 设置面板。连点 2 次弹出。
    /// 顶栏 Tab：0~3 为数据源模式（点「应用」才生效），4 为系统设置（预注册/清注册/帧率显示/退出）。
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(200)]
    public class UwbSettingsPanel : MonoBehaviour
    {
        const string HostName = "UwbSettingsPanel_Host";
        const int TapNeed = 2;
        const float TapWindowSec = 1.2f;
        const float TitleHeight = 48f;
        const float CloseWidth = 52f;
        const float Pad = 14f;
        const float TabHeight = 46f;
        const int SystemTabIndex = 4;
        const float WindowMaxWidth = 1320f;
        const string ShowFpsPrefKey = "UwbSettings_ShowFps";
        const float FpsRefreshSec = 0.5f;

        public static UwbSettingsPanel Current { get; private set; }

        [SerializeField] bool dontDestroyOnLoad = true;
        [SerializeField] int fontSize = 20;

        /// <summary>
        /// 确保全局设置面板存在（Login/注册页也需要，否则无法双击退出）。
        /// UwbRuntime 未就绪时仍可改连接配置并退出程序。
        /// </summary>
        public static UwbSettingsPanel EnsureExists()
        {
            if (Current != null)
                return Current;

            var existing = Object.FindObjectOfType<UwbSettingsPanel>();
            if (existing != null)
            {
                Current = existing;
                return existing;
            }

            var host = new GameObject(HostName);
            return host.AddComponent<UwbSettingsPanel>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootstrapAfterSceneLoad()
        {
            EnsureExists();
        }

        bool _showFps;
        float _fpsValue;
        float _fpsElapsed;
        int _fpsFrames;
        GUIStyle _fpsStyle;
        GUIStyle _fpsShadowStyle;

        bool _visible;
        int _tapCount;
        float _lastTapTime = -999f;
        Vector2 _scroll;
        string _status = "";
        Rect _windowRect;
        bool _hasWindowPos;
        bool _dragging;
        Vector2 _dragGrab;

        /// <summary>当前顶栏 Tab（0~3 模式编辑，4 系统设置）</summary>
        int _editTab;

        /// <summary>已生效模式索引 0~3</summary>
        int _appliedTab;

        string _mqttUrl = "";
        string _mqttTopic = "";
        string _mqttClientId = "";
        string _udpPort = "9000";
        string _udpLocal = "";
        string _mockCount = "20";
        /// <summary>0=固定人数，1=10/20/40 循环。</summary>
        int _mockRosterMode;
        readonly string[] _mockRosterModeLabels = { "固定人数", "循环 10/20/40" };
        string _mockAreaX = "18";
        string _mockAreaY = "10";
        string _replayPath = "";
        bool _replayLoop = true;
        string _rangeStart = "";
        string _rangeEnd = "";
        string _offsetX = "0";
        string _offsetY = "0";
        string _offsetZ = "0";
        readonly string[] _modeNames = { "LiveMqtt", "LiveUdp", "Mock", "Replay" };
        readonly string[] _modeLabels = { "真机MQTT", "真机UDP", "Mock", "Replay" };
        readonly string[] _tabLabels = { "真机MQTT", "真机UDP", "Mock", "Replay", "系统设置" };

        bool IsSystemTab => _editTab == SystemTabIndex;

        List<string> _dates = new List<string>();
        int _dateIndex;
        List<UwbRecordFileInfo> _dayFiles = new List<UwbRecordFileInfo>();
        int _fileIndex = -1;
        string _probeInfo = "";
        Vector2 _logScroll;
        bool _showLogBrowser;

        bool _alertVisible;
        string _alertTitle = "";
        string _alertBody = "";
        bool _confirmClearRegister;

        GUIStyle _box;
        GUIStyle _titleBar;
        GUIStyle _contentBox;
        GUIStyle _label;
        GUIStyle _field;
        GUIStyle _btn;
        GUIStyle _title;
        GUIStyle _closeBtn;
        GUIStyle _tabIdle;
        GUIStyle _tabEdit;
        GUIStyle _tabApplied;
        GUIStyle _selectedBtn;
        bool _stylesReady;
        readonly List<Texture2D> _ownedTextures = new List<Texture2D>();

        void Awake()
        {
            if (dontDestroyOnLoad)
            {
                if (Current != null && Current != this)
                {
                    Destroy(gameObject);
                    return;
                }

                if (gameObject.name != HostName)
                {
                    var host = new GameObject(HostName);
                    var moved = host.AddComponent<UwbSettingsPanel>();
                    moved.dontDestroyOnLoad = true;
                    moved.fontSize = fontSize;
                    DontDestroyOnLoad(host);
                    Destroy(this);
                    return;
                }

                DontDestroyOnLoad(gameObject);
            }

            Current = this;
            _showFps = PlayerPrefs.GetInt(ShowFpsPrefKey, 0) == 1;
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;
            for (int i = 0; i < _ownedTextures.Count; i++)
            {
                if (_ownedTextures[i] != null)
                    Destroy(_ownedTextures[i]);
            }

            _ownedTextures.Clear();
            _stylesReady = false;
        }

        public void ShowAlert(string title, string body)
        {
            _alertTitle = title ?? "";
            _alertBody = body ?? "";
            _alertVisible = true;
        }

        void Update()
        {
            // 用 Update 收点击，避免注册页 uGUI 吃掉 OnGUI 的 MouseDown
            HandleTapFromInput();
            SampleFps();
        }

        void SampleFps()
        {
            if (!_showFps)
                return;
            _fpsFrames++;
            _fpsElapsed += Time.unscaledDeltaTime;
            if (_fpsElapsed < FpsRefreshSec)
                return;
            _fpsValue = _fpsFrames / _fpsElapsed;
            _fpsFrames = 0;
            _fpsElapsed = 0f;
        }

        void DrawFps()
        {
            if (!_showFps)
                return;
            if (_fpsStyle == null)
            {
                _fpsStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                _fpsStyle.normal.textColor = Color.green;
                _fpsShadowStyle = new GUIStyle(_fpsStyle);
                _fpsShadowStyle.normal.textColor = Color.black;
            }

            string text = "FPS: " + _fpsValue.ToString("0");
            GUI.Label(new Rect(13f, 9f, 260f, 44f), text, _fpsShadowStyle);
            GUI.Label(new Rect(12f, 8f, 260f, 44f), text, _fpsStyle);
        }

        void SetShowFps(bool value)
        {
            _showFps = value;
            _fpsValue = 0f;
            _fpsFrames = 0;
            _fpsElapsed = 0f;
            PlayerPrefs.SetInt(ShowFpsPrefKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        void OnGUI()
        {
            DrawFps();
            DrawAlertIfNeeded();
            if (!_visible)
                return;

            EnsureStyles();
            EnsureWindowRect();
            HandleWindowDragAndClose();

            // Windows Player 上 GUIStyle.background 的运行时贴图常画不出来，改用 whiteTexture 铺底。
            FillRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.5f));
            FillRect(_windowRect, new Color(0.08f, 0.08f, 0.1f, 0.96f));

            Rect titleRect = new Rect(_windowRect.x, _windowRect.y, _windowRect.width - CloseWidth, TitleHeight);
            FillRect(new Rect(_windowRect.x, _windowRect.y, _windowRect.width, TitleHeight),
                new Color(0.14f, 0.14f, 0.16f, 1f));
            GUI.Label(
                new Rect(titleRect.x + 14f, titleRect.y, titleRect.width - 20f, TitleHeight),
                "UWB 设置",
                _title);

            Rect closeRect = new Rect(_windowRect.xMax - CloseWidth, _windowRect.y, CloseWidth, TitleHeight);
            if (GUI.Button(closeRect, "×", _closeBtn))
            {
                _visible = false;
                _dragging = false;
                return;
            }

            float y = _windowRect.y + TitleHeight + 6f;
            float innerW = _windowRect.width - Pad * 2f;
            float x0 = _windowRect.x + Pad;

            DrawTopTabs(new Rect(x0, y, innerW, TabHeight));
            y += TabHeight + 8f;

            if (IsSystemTab)
            {
                GUI.Label(new Rect(x0, y, innerW, 28f),
                    "当前生效数据源: " + _modeLabels[Mathf.Clamp(_appliedTab, 0, 3)] + "　　本页为系统设置（无需点应用）",
                    _label);
            }
            else
            {
                GUI.Label(
                    new Rect(x0, y, innerW, 28f),
                    "当前生效: " + _modeLabels[Mathf.Clamp(_appliedTab, 0, 3)] +
                    "　　编辑中: " + _modeLabels[Mathf.Clamp(_editTab, 0, 3)] +
                    "（点「应用」后切换）",
                    _label);
            }

            y += 30f;

            if (!string.IsNullOrEmpty(_status))
            {
                GUI.Label(new Rect(x0, y, innerW, 26f), _status, _label);
                y += 26f;
            }

            float contentBottomPad = IsSystemTab ? 8f : 70f;
            Rect contentRect = new Rect(x0, y, innerW, _windowRect.yMax - y - contentBottomPad - Pad);
            FillRect(contentRect, new Color(0.12f, 0.12f, 0.14f, 1f));
            GUILayout.BeginArea(new Rect(contentRect.x + 12f, contentRect.y + 10f,
                contentRect.width - 24f, contentRect.height - 16f));
            _scroll = GUILayout.BeginScrollView(_scroll);

            if (IsSystemTab)
            {
                DrawSystemSettingsContent();
            }
            else
            {
                switch (_editTab)
                {
                    case 0:
                        GUILayout.Label("真机 MQTT", _title);
                        GUILayout.Label("应用后将持续使用此模式；自动按日录制。", _label);
                        _mqttUrl = LabeledField("Broker URL", _mqttUrl);
                        _mqttTopic = LabeledField("Topic", _mqttTopic);
                        _mqttClientId = LabeledField("ClientId(可空)", _mqttClientId);
                        GUILayout.Label("日志: " + GetRecordDir() + "/uwb_yyyyMMdd.jsonl", _label);
                        break;
                    case 1:
                        GUILayout.Label("真机 UDP", _title);
                        GUILayout.Label("应用后将持续使用此模式；自动按日录制。", _label);
                        _udpPort = LabeledField("Listen Port", _udpPort);
                        _udpLocal = LabeledField("Local Address(可空)", _udpLocal);
                        GUILayout.Label("日志: " + GetRecordDir() + "/uwb_yyyyMMdd.jsonl", _label);
                        break;
                    case 2:
                        GUILayout.Label("Mock", _title);
                        GUILayout.Label("应用后将持续使用此模式；不录制。", _label);
                        GUILayout.Label("人数模式", _label);
                        _mockRosterMode = GUILayout.SelectionGrid(
                            _mockRosterMode, _mockRosterModeLabels, 2, _btn, GUILayout.Height(40));
                        if (_mockRosterMode == 0)
                            _mockCount = LabeledField("固定人数（手动输入）", _mockCount);
                        else
                            GUILayout.Label("每场依次：10 → 20 → 40 → 10 …（应用后按此启动）", _label);
                        _mockAreaX = LabeledField("Area X(米)", _mockAreaX);
                        _mockAreaY = LabeledField("Area Y(米)", _mockAreaY);
                        GUILayout.Space(8);
                        GUILayout.Label("比分牌战斗 Mock（按模式）", _title);
                        GUILayout.Label("与定位 Mock 绑定：应用本模式即自动开，离开 Mock 即关。", _label);
                        string modeLabel = string.IsNullOrEmpty(ScoreCardBattleMockToggle.ActiveModeType)
                            ? "0(积分)"
                            : ScoreCardBattleMockToggle.ActiveModeType;
                        GUILayout.Label("当前模式 modeType=" + modeLabel + "（默认积分；其他模式实现后可切换）", _label);
                        bool scoreMockOn = ScoreCardBattleMockToggle.IsEnabled != null && ScoreCardBattleMockToggle.IsEnabled();
                        GUILayout.Label(scoreMockOn ? "状态：已随定位 Mock 启动" : "状态：未启动（应用 Mock 后生效）", _label);
                        break;
                    case 3:
                        DrawReplayContent();
                        break;
                }

                GUILayout.Space(12);
                GUILayout.Label("坐标偏移(米)", _title);
                _offsetX = LabeledField("Offset X", _offsetX);
                _offsetY = LabeledField("Offset Y", _offsetY);
                _offsetZ = LabeledField("Offset Z", _offsetZ);

                UwbRuntime runtime = FindRuntime();
                if (runtime != null)
                {
                    GUILayout.Space(8);
                    GUILayout.Label(
                        $"运行状态: mode={runtime.ActiveMode} accepted={runtime.AcceptedCount} tags={runtime.TrackedTags}",
                        _label);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (!IsSystemTab)
            {
                float btnH = 52f;
                float yBtn = _windowRect.yMax - Pad - btnH;
                Rect applyRect = new Rect(x0, yBtn, innerW, btnH);
                if (GUI.Button(applyRect,
                        "应用（切换到「" + _modeLabels[Mathf.Clamp(_editTab, 0, 3)] + "」并保存）", _btn))
                    ApplyAndSave();
            }
        }

        static void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void DrawSystemSettingsContent()
        {
            GUILayout.Label("系统设置", _title);
            GUILayout.Label("预注册、清除注册、帧率显示、退出程序。与 UWB 数据源「应用」无关。", _label);

            GUILayout.Space(14);
            GUILayout.Label("1~4号预注册比分牌", _title);
            GUILayout.Label(
                "跳过「开始注册」接口，直接写入本地设备缓存并进入大厅（Editor / Release 均可）。",
                _label);
            GUILayout.Space(6);

            for (int i = 0; i < ScoreCardTestDevicePresets.Count; i++)
            {
                int index = i;
                if (GUILayout.Button(ScoreCardTestDevicePresets.GetButtonLabel(index), _btn, GUILayout.Height(48)))
                    ApplyPresetRegister(index);
            }

            GUILayout.Space(18);
            GUILayout.Label("设备注册", _title);
            GUILayout.Label("清除本地注册后返回登录/注册页，可重新注册或选用上方预设。", _label);
            GUILayout.Space(6);

            if (_confirmClearRegister)
            {
                GUILayout.Label("确定要清除设备注册数据吗？清除后将返回注册页。", _label);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("确定清除", _btn, GUILayout.Height(48)))
                    ConfirmClearRegister();
                if (GUILayout.Button("取消", _btn, GUILayout.Height(48)))
                    _confirmClearRegister = false;
                GUILayout.EndHorizontal();
            }
            else if (GUILayout.Button("清除设备注册数据（返回注册页）", _btn, GUILayout.Height(48)))
            {
                _confirmClearRegister = true;
            }

            GUILayout.Space(18);
            GUILayout.Label("帧率显示", _title);
            GUILayout.Label("在屏幕左上角显示当前帧率。", _label);
            GUILayout.Space(6);
            if (GUILayout.Button(_showFps ? "帧率显示：开（点击关闭）" : "帧率显示：关（点击开启）", _btn, GUILayout.Height(48)))
                SetShowFps(!_showFps);

            GUILayout.Space(18);
            GUILayout.Label("程序", _title);
            GUILayout.Space(6);
            if (GUILayout.Button("退出程序", _btn, GUILayout.Height(52)))
                QuitApp();
        }

        void ApplyPresetRegister(int index)
        {
            if (!ScoreCardTestDevicePresets.TryApply(index, out string error))
            {
                _status = error ?? "预注册失败";
                ShowAlert("预注册失败", _status);
                return;
            }

            _status = $"已应用{index + 1}号预注册，正在进入大厅…";
            _confirmClearRegister = false;
            Close();
            ScoreCardTestDevicePresets.RequestEnterHall();
        }

        void ConfirmClearRegister()
        {
            _confirmClearRegister = false;
            _status = "已请求清除注册数据…";
            Close();
            ScoreCardTestDevicePresets.RequestClearRegisterAndReturnToLogin();
        }

        void DrawTopTabs(Rect area)
        {
            int tabCount = _tabLabels.Length;
            float gap = 6f;
            float w = (area.width - gap * (tabCount - 1)) / tabCount;
            for (int i = 0; i < tabCount; i++)
            {
                Rect r = new Rect(area.x + i * (w + gap), area.y, w, area.height);
                GUIStyle style = _tabIdle;
                if (i < 4 && i == _appliedTab)
                    style = _tabApplied;
                else if (i == _editTab)
                    style = _tabEdit;

                string text = _tabLabels[i];
                if (i < 4 && i == _appliedTab)
                    text = "● " + text;

                if (GUI.Button(r, text, style))
                {
                    if (_editTab == i)
                        continue;

                    _editTab = i;
                    _scroll = Vector2.zero;
                    _showLogBrowser = false;
                    _confirmClearRegister = false;
                    if (i == SystemTabIndex)
                        _status = "";
                    else
                        _status = "编辑: " + _modeLabels[i] + "（尚未应用）";
                }
            }
        }

        void DrawReplayContent()
        {
            GUILayout.Label("Replay", _title);
            GUILayout.Label("应用后将持续使用此模式；不录制。无选中文件则不回放。", _label);

            if (string.IsNullOrEmpty(_replayPath) || !File.Exists(_replayPath))
                GUILayout.Label("未选择日志文件。", _label);
            else
                GUILayout.Label("文件: " + _replayPath, _label);

            if (!string.IsNullOrEmpty(_probeInfo))
                GUILayout.Label(_probeInfo, _label);

            DrawToggle("循环回放", ref _replayLoop);
            GUILayout.Label("循环时段（文件内 t 秒；空=整段）", _label);
            _rangeStart = LabeledField("起点秒", _rangeStart);
            _rangeEnd = LabeledField("终点秒", _rangeEnd);

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_showLogBrowser ? "收起日志列表" : "浏览日志文件", _btn, GUILayout.Height(40)))
            {
                _showLogBrowser = !_showLogBrowser;
                if (_showLogBrowser)
                    RefreshLogList();
            }

            if (!string.IsNullOrEmpty(_replayPath) && File.Exists(_replayPath))
            {
                if (GUILayout.Button("读取时段", _btn, GUILayout.Height(40)))
                    ProbeSelectedFile();
            }

            GUILayout.EndHorizontal();

            if (!_showLogBrowser)
                return;

            GUILayout.Space(8);
            GUILayout.Label("日志目录: " + GetRecordDir(), _label);
            if (GUILayout.Button("刷新列表", _btn, GUILayout.Height(36)))
                RefreshLogList();

            if (_dates.Count == 0)
            {
                GUILayout.Label("暂无录制文件（连真机后才会生成）", _label);
                return;
            }

            GUILayout.Label("日期", _label);
            string[] dateLabels = _dates.ToArray();
            int newDate = GUILayout.SelectionGrid(_dateIndex, dateLabels, Mathf.Min(3, dateLabels.Length), _btn);
            if (newDate != _dateIndex)
            {
                _dateIndex = newDate;
                LoadFilesForSelectedDate();
            }

            GUILayout.Label("文件（点击选中）", _label);
            _logScroll = GUILayout.BeginScrollView(_logScroll, GUILayout.Height(180));
            for (int i = 0; i < _dayFiles.Count; i++)
            {
                UwbRecordFileInfo f = _dayFiles[i];
                string label = $"{f.fileName}  ({f.sizeBytes / 1024f:0.#} KB)  {f.lastWrite:HH:mm:ss}";
                GUIStyle style = i == _fileIndex ? _selectedBtn : _btn;
                if (GUILayout.Button(label, style, GUILayout.Height(38)))
                {
                    _fileIndex = i;
                    _replayPath = f.path;
                    ProbeSelectedFile();
                    _status = "已选中回放文件（点应用后生效 Replay）";
                }
            }

            GUILayout.EndScrollView();
        }

        void RefreshLogList()
        {
            _dates = UwbRecordStore.ListDates(GetRecordDir());
            if (_dateIndex >= _dates.Count)
                _dateIndex = 0;
            LoadFilesForSelectedDate();
        }

        void LoadFilesForSelectedDate()
        {
            _dayFiles.Clear();
            _fileIndex = -1;
            if (_dates.Count == 0)
                return;
            string day = _dates[Mathf.Clamp(_dateIndex, 0, _dates.Count - 1)];
            _dayFiles = UwbRecordStore.ListFilesForDate(GetRecordDir(), day);
            for (int i = 0; i < _dayFiles.Count; i++)
            {
                if (!string.IsNullOrEmpty(_replayPath) &&
                    string.Equals(_dayFiles[i].path, _replayPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    _fileIndex = i;
                    ProbeSelectedFile();
                    break;
                }
            }
        }

        void ProbeSelectedFile()
        {
            _probeInfo = "";
            if (string.IsNullOrEmpty(_replayPath) || !File.Exists(_replayPath))
            {
                _probeInfo = "文件不存在";
                return;
            }

            if (!UwbRecordStore.TryProbeTimeRange(_replayPath, out double minT, out double maxT, out int lines))
            {
                _probeInfo = "无法解析时段（文件可能为空）";
                return;
            }

            _probeInfo =
                $"帧数={lines}  t=[{minT:0.##} ~ {maxT:0.##}]  约 {UwbRecordStore.FormatDuration(maxT - minT)}";
            if (string.IsNullOrWhiteSpace(_rangeStart))
                _rangeStart = minT.ToString("0.###", CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(_rangeEnd))
                _rangeEnd = maxT.ToString("0.###", CultureInfo.InvariantCulture);
        }

        string GetRecordDir()
        {
            UwbRuntime rt = FindRuntime();
            return rt != null ? rt.RecordDirectory : UwbRecordStore.DefaultDirectory;
        }

        void EnsureWindowRect()
        {
            float w = Mathf.Min(WindowMaxWidth, Screen.width - 32f);
            float h = Mathf.Min(960f, Screen.height - 32f);
            if (!_hasWindowPos)
            {
                _windowRect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
                _hasWindowPos = true;
                return;
            }

            _windowRect.width = w;
            _windowRect.height = h;
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0f, Mathf.Max(0f, Screen.width - w));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0f, Mathf.Max(0f, Screen.height - h));
        }

        void HandleWindowDragAndClose()
        {
            Event e = Event.current;
            if (e == null)
                return;

            Rect dragRect = new Rect(_windowRect.x, _windowRect.y, _windowRect.width - CloseWidth, TitleHeight);
            if (e.type == EventType.MouseDown && e.button == 0 && dragRect.Contains(e.mousePosition))
            {
                _dragging = true;
                _dragGrab = e.mousePosition - new Vector2(_windowRect.x, _windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _dragging && e.button == 0)
            {
                _windowRect.x = e.mousePosition.x - _dragGrab.x;
                _windowRect.y = e.mousePosition.y - _dragGrab.y;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _dragging = false;
            }
        }

        void HandleTapFromInput()
        {
            if (_visible || _alertVisible)
                return;
            if (!Input.GetMouseButtonDown(0))
                return;

            float now = Time.unscaledTime;
            if (now - _lastTapTime > TapWindowSec)
                _tapCount = 0;
            _lastTapTime = now;
            _tapCount++;
            if (_tapCount < TapNeed)
                return;
            _tapCount = 0;
            Open();
        }

        public void Open()
        {
            PullFromRuntimeOrFile();
            // 打开时定位到当前生效模式
            _editTab = _appliedTab;
            _visible = true;
            _scroll = Vector2.zero;
            _dragging = false;
            _hasWindowPos = false;
            _showLogBrowser = false;
            _confirmClearRegister = false;
            _status = FindRuntime() == null
                ? "当前在启动/注册页：可改连接配置并退出程序；Mock/Replay 需进入场馆画面后应用"
                : "";
        }

        public void Close()
        {
            _visible = false;
            _dragging = false;
        }

        void PullFromRuntimeOrFile()
        {
            UwbConfigData data = null;
            UwbRuntime runtime = FindRuntime();
            if (runtime != null)
                data = runtime.ExportConfig();
            if (data == null)
                data = UwbConfigFile.LoadOrNull() ?? new UwbConfigData();
            ApplyDataToUi(data);
        }

        void ApplyDataToUi(UwbConfigData d)
        {
            UwbSourceMode mode = UwbConfigFile.ParseMode(d.sourceMode, UwbSourceMode.Mock);
            _appliedTab = ModeToIndex(mode);
            _editTab = _appliedTab;
            _mqttUrl = d.mqttBrokerUrl ?? "";
            _mqttTopic = d.mqttTopic ?? "";
            _mqttClientId = d.mqttClientId ?? "";
            _udpPort = d.udpListenPort.ToString();
            _udpLocal = d.udpLocalAddress ?? "";
            _mockCount = d.mockTagCount.ToString();
            _mockRosterMode = d.mockRosterCycle ? 1 : 0;
            _mockAreaX = d.mockAreaX.ToString("0.##");
            _mockAreaY = d.mockAreaY.ToString("0.##");
            _replayPath = d.replayFilePath ?? "";
            _replayLoop = d.replayLoop;
            _rangeStart = d.replayRangeStart < 0 ? "" : d.replayRangeStart.ToString("0.###", CultureInfo.InvariantCulture);
            _rangeEnd = d.replayRangeEnd < 0 ? "" : d.replayRangeEnd.ToString("0.###", CultureInfo.InvariantCulture);
            _offsetX = d.offsetX.ToString("0.###");
            _offsetY = d.offsetY.ToString("0.###");
            _offsetZ = d.offsetZ.ToString("0.###");
            if (!string.IsNullOrEmpty(_replayPath) && File.Exists(_replayPath))
                ProbeSelectedFile();
        }

        void ApplyAndSave()
        {
            if (IsSystemTab)
                return;

            if (_editTab == 3 && (string.IsNullOrEmpty(_replayPath) || !File.Exists(_replayPath)))
            {
                _status = "Replay 需要先选中日志文件";
                return;
            }

            var data = BuildDataFromUi();
            data.sourceMode = _modeNames[Mathf.Clamp(_editTab, 0, 3)];

            UwbRuntime runtime = FindRuntime();
            if (runtime == null)
            {
                // 无 Runtime 时只落盘连接参数；模式不写成 Mock/Replay
                var disk = CloneForDisk(data);
                UwbConfigFile.Save(disk);
                _appliedTab = _editTab;
                _status = "已保存连接配置；下次启动仍自动连 MQTT";
                return;
            }

            // 会话内切换到所选模式（含 Mock/Replay）
            runtime.ApplyConfig(data, restart: true, applySourceMode: true);
            _appliedTab = _editTab;

            // 写盘：Mock/Replay 不作为开机默认
            UwbConfigFile.Save(runtime.ExportConfigForDisk());
            if (_editTab == 2 || _editTab == 3)
                _status = "已切换到 " + _modeLabels[_appliedTab] + "（仅本次运行；重启后仍自动连 MQTT）";
            else
                _status = "已切换并保存: " + _modeLabels[_appliedTab];
        }

        static UwbConfigData CloneForDisk(UwbConfigData data)
        {
            if (data == null)
                return new UwbConfigData();
            string mode = data.sourceMode;
            if (string.Equals(mode, "Mock", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mode, "Replay", System.StringComparison.OrdinalIgnoreCase))
                mode = "LiveMqtt";
            data.sourceMode = mode;
            return data;
        }

        void DrawAlertIfNeeded()
        {
            if (!_alertVisible)
                return;
            EnsureStyles();
            float w = Mathf.Min(560f, Screen.width - 40f);
            float h = 240f;
            Rect box = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            FillRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.55f));
            FillRect(box, new Color(0.1f, 0.1f, 0.12f, 0.98f));
            GUILayout.BeginArea(new Rect(box.x + 20f, box.y + 16f, box.width - 40f, box.height - 32f));
            GUILayout.Label(_alertTitle, _title);
            GUILayout.Space(8);
            GUILayout.Label(_alertBody, _label);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("打开设置", _btn, GUILayout.Height(48)))
            {
                _alertVisible = false;
                Open();
                _editTab = 0;
            }

            if (GUILayout.Button("关闭", _btn, GUILayout.Height(48)))
                _alertVisible = false;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        UwbConfigData BuildDataFromUi()
        {
            return new UwbConfigData
            {
                enableUwb = true,
                sourceMode = _modeNames[Mathf.Clamp(_editTab, 0, 3)],
                mqttBrokerUrl = (_mqttUrl ?? "").Trim(),
                mqttTopic = (_mqttTopic ?? "").Trim(),
                mqttClientId = (_mqttClientId ?? "").Trim(),
                udpListenPort = ParseInt(_udpPort, 9000),
                udpLocalAddress = (_udpLocal ?? "").Trim(),
                mockTagCount = ParseInt(_mockCount, 20),
                mockRosterCycle = _mockRosterMode == 1,
                mockAreaX = ParseFloat(_mockAreaX, 18f),
                mockAreaY = ParseFloat(_mockAreaY, 10f),
                mockMoveSpeed = 1.2f,
                mockIntervalSec = 0.1f,
                replayFilePath = (_replayPath ?? "").Trim(),
                replayLoop = _replayLoop,
                replaySpeed = 1f,
                replayRangeStart = ParseFloatOrNeg(_rangeStart),
                replayRangeEnd = ParseFloatOrNeg(_rangeEnd),
                offsetX = ParseFloat(_offsetX, 0f),
                offsetY = ParseFloat(_offsetY, 0f),
                offsetZ = ParseFloat(_offsetZ, 0f),
                scaleX = 1f,
                scaleY = 1f,
                scaleZ = 1f,
                flattenHeight = true
            };
        }

        static UwbRuntime FindRuntime()
        {
            return Object.FindObjectOfType<UwbRuntime>();
        }

        static int ModeToIndex(UwbSourceMode mode)
        {
            switch (mode)
            {
                case UwbSourceMode.LiveMqtt: return 0;
                case UwbSourceMode.LiveUdp: return 1;
                case UwbSourceMode.Mock: return 2;
                case UwbSourceMode.Replay: return 3;
                default: return 2;
            }
        }

        static int ParseInt(string s, int fallback)
        {
            return int.TryParse(s, out int v) ? v : fallback;
        }

        static float ParseFloat(string s, float fallback)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ||
                   float.TryParse(s, out v)
                ? v
                : fallback;
        }

        static float ParseFloatOrNeg(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return -1f;
            return ParseFloat(s, -1f);
        }

        string LabeledField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _label, GUILayout.Width(150));
            string next = GUILayout.TextField(value ?? "", _field);
            GUILayout.EndHorizontal();
            return next;
        }

        void DrawToggle(string label, ref bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _label, GUILayout.Width(150));
            value = GUILayout.Toggle(value, value ? "开" : "关", _btn);
            GUILayout.EndHorizontal();
        }

        void EnsureStyles()
        {
            if (_stylesReady)
                return;
            int fs = Mathf.Max(16, fontSize);

            _box = new GUIStyle(GUI.skin.box);
            _titleBar = new GUIStyle(GUI.skin.box);
            _contentBox = new GUIStyle(GUI.skin.box);
            _contentBox.padding = new RectOffset(8, 8, 8, 8);

            _label = new GUIStyle(GUI.skin.label)
            {
                fontSize = fs,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = fs + 3,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };
            _field = new GUIStyle(GUI.skin.textField) { fontSize = fs };
            ApplySolidBackground(_field, new Color(0.18f, 0.18f, 0.2f, 1f));
            _field.normal.textColor = Color.white;
            _field.focused.textColor = Color.white;

            _btn = new GUIStyle(GUI.skin.button) { fontSize = fs };
            ApplySolidBackground(_btn, new Color(0.25f, 0.25f, 0.28f, 1f));
            _btn.hover.background = MakeTex(8, 8, new Color(0.32f, 0.32f, 0.36f, 1f));
            _btn.active.background = MakeTex(8, 8, new Color(0.38f, 0.38f, 0.42f, 1f));
            _btn.normal.textColor = Color.white;

            _tabIdle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fs,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            ApplySolidBackground(_tabIdle, new Color(0.22f, 0.22f, 0.25f, 1f));
            _tabIdle.hover.background = MakeTex(8, 8, new Color(0.28f, 0.28f, 0.32f, 1f));
            _tabIdle.active.background = MakeTex(8, 8, new Color(0.32f, 0.32f, 0.36f, 1f));
            _tabIdle.normal.textColor = Color.white;

            _tabEdit = new GUIStyle(_tabIdle) { fontStyle = FontStyle.Bold };
            ApplySolidBackground(_tabEdit, new Color(0.18f, 0.36f, 0.55f, 1f));
            _tabEdit.hover.background = MakeTex(8, 8, new Color(0.22f, 0.42f, 0.62f, 1f));
            _tabEdit.normal.textColor = Color.white;

            _tabApplied = new GUIStyle(_tabIdle) { fontStyle = FontStyle.Bold };
            ApplySolidBackground(_tabApplied, new Color(0.12f, 0.48f, 0.28f, 1f));
            _tabApplied.hover.background = MakeTex(8, 8, new Color(0.16f, 0.55f, 0.32f, 1f));
            _tabApplied.normal.textColor = Color.white;

            _selectedBtn = new GUIStyle(_btn) { fontSize = fs, fontStyle = FontStyle.Bold };
            _selectedBtn.normal.textColor = new Color(0.4f, 1f, 0.6f);

            _closeBtn = new GUIStyle(_btn)
            {
                fontSize = fs + 8,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _stylesReady = true;
        }

        void ApplySolidBackground(GUIStyle style, Color col)
        {
            Texture2D tex = MakeTex(8, 8, col);
            style.normal.background = tex;
            style.onNormal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            style.focused.background = tex;
        }

        static void FillRect(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply(false, false);
            _ownedTextures.Add(tex);
            return tex;
        }
    }
}
