namespace LetsPiu.UI
{
    /// <summary>
    /// 战斗底栏人数档位（策划案 P10–12）：开局按参赛人数锁定，本局不换档。
    /// </summary>
    public enum UwbHudRosterMode
    {
        /// <summary>≤10 人：每队 5 槽。</summary>
        Ten = 0,
        /// <summary>11–20 人：每队 10 槽，压缩卡片。</summary>
        Twenty = 1,
        /// <summary>21–40 人：每队 20 槽（2×10），更紧凑。</summary>
        Forty = 2
    }

    public static class UwbHudRosterModeUtil
    {
        public static UwbHudRosterMode FromCount(int rosterCount)
        {
            if (rosterCount <= 10)
                return UwbHudRosterMode.Ten;
            if (rosterCount <= 20)
                return UwbHudRosterMode.Twenty;
            return UwbHudRosterMode.Forty;
        }

        public static int SlotsPerTeam(UwbHudRosterMode mode)
        {
            switch (mode)
            {
                case UwbHudRosterMode.Twenty: return 10;
                case UwbHudRosterMode.Forty: return 20;
                default: return 5;
            }
        }
    }
}
