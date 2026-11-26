namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 定位 Mock 按比赛模式可挂载的行为。积分已实现默认角点出生 + 死亡回出生点；
    /// 其他模式实现后注册到 <see cref="UwbMockModeBehaviors"/> 即可。
    /// </summary>
    public interface IUwbMockModeBehavior
    {
        /// <summary>对应 ScoreCardGameMode / modeType。</summary>
        string ModeType { get; }

        /// <summary>死亡后是否导航回己方出生点（等比分牌发 REVIVE）。</summary>
        bool ReturnHomeOnDeath { get; }
    }
}
