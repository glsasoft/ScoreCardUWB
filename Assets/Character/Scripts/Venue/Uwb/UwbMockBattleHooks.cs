using System;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 定位 Mock 与比分牌 Mock 的桥接：出生角、回出生点、开局重置、人数切换。
    /// 仅 Mock 使用；真机 UWB 不注册这些回调。
    /// </summary>
    public static class UwbMockBattleHooks
    {
        /// <summary>team：0=蓝（右上），1=红（左下）。返回场馆 XZ（Vector2.x/y）。</summary>
        public static Func<int, Vector2> GetSpawnVenueXz;

        /// <summary>大炮挂点：红炮左下 team=1，蓝炮右上 team=0。</summary>
        public static Func<int, Vector2> GetCannonVenueXz;

        /// <summary>玩家当前场馆水平坐标（Mock 真值）。</summary>
        public static Func<int, Vector2?> GetPlayerVenueXz;

        /// <summary>是否已走到己方复活点附近。</summary>
        public static Func<int, float, bool> IsNearHome;

        /// <summary>全员立刻传回各自出生点附近（新一场 Mock 开局）。</summary>
        public static Action ResetAllToTeamSpawns;

        /// <summary>切换定位 Mock 人数；不关闭比分牌 Mock。</summary>
        public static Action<int> SetMockPlayerCount;

        /// <summary>
        /// 取本场应用的人数：固定模式返回手填值；循环模式取 10→20→40 并前进一档。
        /// </summary>
        public static Func<int> TakeMatchPlayerCount;
    }
}
