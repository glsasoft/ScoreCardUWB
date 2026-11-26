using System;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 定位 Mock 与比分牌 Mock 的桥接：出生角、是否已回到复活点。
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
    }
}
