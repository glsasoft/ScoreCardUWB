using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.UI
{
    /// <summary>
    /// 一个玩家在 HUD 上要显示的数据。位置、血量来自场上的 VenuePlayerView，击杀死亡来自 UwbHudData。
    /// </summary>
    public class UwbHudPlayer
    {
        public int number;
        public int team;
        public string name;
        public int hp;
        public int maxHp = 100;
        public int kills;
        public int deaths;
        public int reviveSeconds = -1;
        public Sprite avatar;
        public Vector3 headWorld;

        /// <summary>team 为 0 是蓝队，其余是红队，与 VenuePlayerView 保持一致。</summary>
        public bool IsRed => team != 0;
        public bool IsDead => hp <= 0;
    }

    /// <summary>
    /// 单屏比分牌游戏中 HUD 的数据入口。对战数据接入后（MQTT 击杀/比分/倒计时）往这里写即可，HUD 自己读。
    /// 没写过的字段保持默认值显示。
    /// </summary>
    public static class UwbHudData
    {
        public struct PlayerStats
        {
            public int kills;
            public int deaths;
            public int reviveSeconds;
        }

        public static string TaskTitle = "任务说明";
        public static string TaskBody = "积分战斗得分，为团队扩大积分优势";

        public static int RedScore;
        public static int BlueScore;
        public static int RedEliminated;
        public static int BlueEliminated;

        /// <summary>剩余比赛秒数，小于 0 显示 --:--。</summary>
        public static float RemainSeconds = -1f;

        /// <summary>true 时由 HUD 面板生成演示数据（只在 UWB Mock 数据源下打开）。</summary>
        public static bool UseMock;

        /// <summary>Mock 新一场随机人数时自增，底栏据此解锁并按新峰值重新锁档。</summary>
        public static int RosterLayoutEpoch;

        /// <summary>新一场已知的人数；大于 0 时底栏立刻按它选档，不等玩家真正出现。</summary>
        public static int ExpectedRoster;

        public static void NotifyNewMatchRoster(int expectedRoster = 0)
        {
            ExpectedRoster = expectedRoster;
            RosterLayoutEpoch++;
        }

        static readonly Dictionary<int, PlayerStats> s_Stats = new Dictionary<int, PlayerStats>();
        static readonly Dictionary<int, Sprite> s_Avatars = new Dictionary<int, Sprite>();

        public static void SetStats(int number, int kills, int deaths, int reviveSeconds = -1)
        {
            s_Stats[number] = new PlayerStats { kills = kills, deaths = deaths, reviveSeconds = reviveSeconds };
        }

        public static bool TryGetStats(int number, out PlayerStats stats)
        {
            return s_Stats.TryGetValue(number, out stats);
        }

        public static void SetAvatar(int number, Sprite sprite)
        {
            if (sprite == null)
                s_Avatars.Remove(number);
            else
                s_Avatars[number] = sprite;
        }

        public static Sprite GetAvatar(int number)
        {
            s_Avatars.TryGetValue(number, out Sprite sprite);
            return sprite;
        }

        public static void Clear()
        {
            RedScore = BlueScore = RedEliminated = BlueEliminated = 0;
            RemainSeconds = -1f;
            RosterLayoutEpoch = 0;
            ExpectedRoster = 0;
            s_Stats.Clear();
            s_Avatars.Clear();
        }
    }
}
