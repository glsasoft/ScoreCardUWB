using LetsPiu.Common.Network.Model.ScoreCard;

namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>
    /// 当前场次快照枢纽（对齐 ScoreCardUnity 的同名能力，但不依赖 FightPanel）。
    /// </summary>
    public static class ScoreCardMatchDataHub
    {
        public static ScoreCardMatchData Current { get; private set; }

        public static void Publish(ScoreCardMatchData matchData)
        {
            if (matchData == null)
                return;

            Current = matchData;
            EventManager.Instance.Broadcast(EventMsg.ScoreCardMatchDataEvent, matchData);
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
