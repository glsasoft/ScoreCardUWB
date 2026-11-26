namespace LetsPiu.UI.ScoreCardRuntime
{
    public enum ScoreCardCombatCueType
    {
        ShootHit,
        ShootKill,
        TowerKill,
        Revive,
        BuffRed,
        BuffBlue,
        MultiKill,
        Heal
    }

    /// <summary>比分牌战斗演出提示，供 3D 特效层订阅。</summary>
    public readonly struct ScoreCardCombatCue
    {
        public readonly ScoreCardCombatCueType type;
        public readonly int actorId;
        public readonly int targetId;
        public readonly int teamType;

        public ScoreCardCombatCue(ScoreCardCombatCueType type, int actorId, int targetId, int teamType)
        {
            this.type = type;
            this.actorId = actorId;
            this.targetId = targetId;
            this.teamType = teamType;
        }
    }
}
