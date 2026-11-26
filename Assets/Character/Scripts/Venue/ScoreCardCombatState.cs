using System.Collections.Generic;

namespace LetsPiu.Character
{
    /// <summary>
    /// 比分牌战斗态覆盖：UWB 定位只提供位置，血量/阵亡由 MQTT 比赛事件写入这里，
    /// VenuePlayerSync 呈现时优先读覆盖值。
    /// </summary>
    public static class ScoreCardCombatState
    {
        public struct Entry
        {
            public int hp;
            public int maxHp;
            public bool hasCombat;
        }

        static readonly Dictionary<int, Entry> s_ByPlayer = new Dictionary<int, Entry>(64);

        public static void Set(int playerId, int hp, int maxHp)
        {
            if (playerId <= 0)
                return;
            if (maxHp <= 0)
                maxHp = 100;
            s_ByPlayer[playerId] = new Entry
            {
                hp = hp,
                maxHp = maxHp,
                hasCombat = true
            };
        }

        public static bool TryGet(int playerId, out Entry entry)
        {
            return s_ByPlayer.TryGetValue(playerId, out entry) && entry.hasCombat;
        }

        public static void Clear()
        {
            s_ByPlayer.Clear();
        }
    }
}
