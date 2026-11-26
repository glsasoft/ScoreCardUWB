using System.Collections.Generic;
using LetsPiu.Character.Uwb;
using LetsPiu.Common.Network.Model.ScoreCard;
using UnityEngine;

namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>
    /// 比分牌多模式 Mock 主机：与定位 Mock 绑定启停，按
    /// <see cref="ScoreCardBattleMockToggle.ActiveModeType"/> 选择具体实现。
    /// 目前仅注册积分；歼灭/队长/生化等实现 <see cref="IScoreCardModeMock"/> 后在此 Register。
    /// </summary>
    public class ScoreCardModeMockHost : MonoBehaviour
    {
        const string HostName = "ScoreCardModeMockHost";

        public static ScoreCardModeMockHost Current { get; private set; }

        readonly Dictionary<string, IScoreCardModeMock> _mocks =
            new Dictionary<string, IScoreCardModeMock>(8);

        IScoreCardModeMock _running;

        public static bool IsRunning =>
            Current != null && Current._running != null && Current._running.IsRunning;

        public static string ActiveModeType =>
            string.IsNullOrEmpty(ScoreCardBattleMockToggle.ActiveModeType)
                ? ScoreCardGameMode.Score
                : ScoreCardBattleMockToggle.ActiveModeType;

        public static string ActiveDisplayName
        {
            get
            {
                if (Current != null &&
                    Current._mocks.TryGetValue(ActiveModeType, out var m) &&
                    m != null)
                    return m.DisplayName;
                return ActiveModeType;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            EnsureHost();
        }

        static void EnsureHost()
        {
            if (Current != null)
                return;
            var host = new GameObject(HostName);
            DontDestroyOnLoad(host);
            host.AddComponent<ScoreCardModeMockHost>();
        }

        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
            RegisterBuiltins();
            ScoreCardBattleMockToggle.IsEnabled = () => IsRunning;
            ScoreCardBattleMockToggle.SetEnabled = SetBoundEnabled;
            ScoreCardBattleMockToggle.FlushPending();
        }

        void OnDestroy()
        {
            StopActive();
            if (Current == this)
            {
                ScoreCardBattleMockToggle.IsEnabled = null;
                ScoreCardBattleMockToggle.SetEnabled = null;
                Current = null;
            }
        }

        void RegisterBuiltins()
        {
            var score = GetComponent<ScoreCardScoreModeMock>() ??
                        gameObject.AddComponent<ScoreCardScoreModeMock>();
            Register(score);

            // 后续模式在此挂载，例如：
            // Register(GetComponent<ScoreCardAnnihilationModeMock>() ??
            //          gameObject.AddComponent<ScoreCardAnnihilationModeMock>());
            // Register(... SuperCaptain / Zombie / Fire / Defense / SpeedShooting ...);
        }

        public void Register(IScoreCardModeMock mock)
        {
            if (mock == null || string.IsNullOrEmpty(mock.ModeType))
                return;
            _mocks[mock.ModeType] = mock;
        }

        /// <summary>运行时切换要比分牌 Mock 的模式（下次随定位 Mock 启动时生效；若已在跑则立即重开）。</summary>
        public static void SetActiveModeType(string modeType)
        {
            if (string.IsNullOrEmpty(modeType))
                modeType = ScoreCardGameMode.Score;
            bool wasOn = IsRunning;
            if (wasOn)
                Current?.StopActive();
            ScoreCardBattleMockToggle.ActiveModeType = modeType;
            UwbMockModeBehaviors.ApplyActiveMode();
            if (wasOn)
                Current?.StartActive();
        }

        void SetBoundEnabled(bool enabled)
        {
            if (enabled)
                StartActive();
            else
                StopActive();
        }

        void StartActive()
        {
            StopActive();
            UwbMockModeBehaviors.ApplyActiveMode();

            string mode = ActiveModeType;
            if (!_mocks.TryGetValue(mode, out var mock) || mock == null)
            {
                Debug.LogWarning(
                    $"[ScoreCardModeMockHost] modeType={mode} 无比分牌 Mock 实现，跳过启动");
                return;
            }

            _running = mock;
            mock.StartMock();
            Debug.Log(
                $"[ScoreCardModeMockHost] started mode={mode} ({mock.DisplayName}), bound to UWB Mock");
        }

        void StopActive()
        {
            if (_running == null)
                return;
            _running.StopMock();
            _running = null;
        }
    }
}
