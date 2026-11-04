using LetsPiu.Define;
using System.Collections.Generic;

namespace LetsPiu.Base
{
    public static partial class TDOnlineDefaultPosTable
    {
        static readonly bool m_IsReadAllData = false;

        /// <summary>
        /// key = GameMode+100+TeamType value = List<TDOnlineDefaultPos>
        /// </summary>
        static Dictionary<int, List<TDOnlineDefaultPos>> s_Mode_TeamType_DataListMap = new();

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="mode"> 游戏模式 只支持爆破模式和团竞模式 </param>
        /// <param name="teamType"> 1：红方/攻方  2：蓝色/守方 </param>
        /// <param name="orderIdx"> 位置索引 </param>
        /// <returns></returns>
        public static TDOnlineDefaultPos GetData(GameMode mode, int teamType, int orderIdx)
        {
            var complexKey = GetKey((int)mode, teamType);
            if (s_Mode_TeamType_DataListMap.TryGetValue(complexKey, out var dataList) && dataList is not null)
            {
                var cnt = dataList.Count;
                for (int i = 0; i < cnt; i++)
                {
                    var data = dataList.Get(i);
                    if (data.PosOrder == orderIdx)
                    {
                        return data;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 清理表格 方法名和参数不可修改
        /// </summary>
        static void ClearExtend()
        {

        }

        /// <summary>
        /// 完成配置解析 方法名和参数不可修改
        /// </summary>
        static void CompleteExtend()
        {
            foreach (var item in s_Mode_TeamType_DataListMap)
            {
                item.Value.Sort((x, y) =>
                {
                    return x.PosOrder.CompareTo(y.PosOrder);
                });
            }
        }

        /// <summary>
        /// 完成某行解析 方法名和参数不可修改
        /// </summary>
        static void CompleteRowHead(int key)
        {
            var data = GetData(key);
            if (data is null)
            {
                return;
            }

            var complexKey = GetKey(data.GameMode, data.TeamType);

            if (!s_Mode_TeamType_DataListMap.ContainsKey(complexKey))
            {
                s_Mode_TeamType_DataListMap[complexKey] = new List<TDOnlineDefaultPos>();
            }

            s_Mode_TeamType_DataListMap[complexKey].Add(data);
        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDOnlineDefaultPos tdData)
        {

        }

        static int GetKey(int gameMode, int teamType)
        {
            return gameMode * 100 + teamType;
        }
    }
}