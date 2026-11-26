namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>
    /// 比分牌某一 modeType 的战斗 Mock。由 <see cref="ScoreCardModeMockHost"/> 按
    /// ActiveModeType 启停；新增模式时实现本接口并 Register 即可。
    /// </summary>
    public interface IScoreCardModeMock
    {
        /// <summary>对应 ScoreCardGameMode / modeType。</summary>
        string ModeType { get; }

        string DisplayName { get; }

        bool IsRunning { get; }

        void StartMock();

        void StopMock();
    }
}
