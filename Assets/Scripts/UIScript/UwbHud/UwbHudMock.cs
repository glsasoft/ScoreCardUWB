using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.UI
{
    /// <summary>
    /// UWB Mock 数据源下的 HUD 演示数据：对战数据（击杀、死亡、血量、比分、倒计时）还没接入时，
    /// 用玩家编号算出稳定的假值，方便看版面。数据源不是 Mock 时不会被调用。
    /// </summary>
    static class UwbHudMock
    {
        const float MatchSeconds = 480f;

        public static void Apply(IEnumerable<UwbHudPlayer> players, float elapsed)
        {
            int redScore = 0;
            int blueScore = 0;
            int redOut = 0;
            int blueOut = 0;

            foreach (UwbHudPlayer p in players)
            {
                p.kills = (p.number * 3) % 10;
                p.deaths = (p.number * 7) % 6;
                p.maxHp = 100;
                p.hp = 20 + (p.number * 37) % 81;
                if (p.number % 9 == 0)
                {
                    p.hp = 0;
                    p.reviveSeconds = 8;
                }

                if (p.IsRed)
                {
                    redScore += p.kills * 100;
                    redOut += p.deaths;
                }
                else
                {
                    blueScore += p.kills * 100;
                    blueOut += p.deaths;
                }
            }

            UwbHudData.RedScore = redScore;
            UwbHudData.BlueScore = blueScore;
            UwbHudData.RedEliminated = redOut;
            UwbHudData.BlueEliminated = blueOut;
            UwbHudData.RemainSeconds = Mathf.Max(0f, MatchSeconds - elapsed);
        }
    }
}
