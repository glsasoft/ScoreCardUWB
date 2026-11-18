using System;
using System.Collections.Generic;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 玩家实时 BUFF 标识（<see cref="ScoreCardPlayer.buffList"/> 元素，MQTT 字符串）。
    /// </summary>
    public static class ScoreCardPlayerBuffType
    {
        public const string RedBuff  = "RED_BUFF";
        public const string BlueBuff = "BLUE_BUFF";
        public const string LowHpBuff = "LOW_HP_BUFF"; // 残血增幅（歼灭模式增伤）
        public const string Savior    = "SAVIOR";      // 救世者（生化模式）

        public const string Red  = "RED";
        public const string Blue = "BLUE";

        // 残血增幅、救世者没有独立图标，与红 buff 共用红色 buff 节点。
        public static bool HasRed(IList<string> buffList)
        {
            return Contains(buffList, RedBuff, Red, LowHpBuff, Savior);
        }

        public static bool HasBlue(IList<string> buffList)
        {
            return Contains(buffList, BlueBuff, Blue);
        }

        static bool Contains(IList<string> buffList, params string[] ids)
        {
            if (buffList == null || buffList.Count == 0 || ids == null || ids.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < buffList.Count; i++)
            {
                string entry = buffList[i];
                if (string.IsNullOrWhiteSpace(entry)) continue;

                for (int j = 0; j < ids.Length; j++)
                {
                    if (string.Equals(entry, ids[j], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
