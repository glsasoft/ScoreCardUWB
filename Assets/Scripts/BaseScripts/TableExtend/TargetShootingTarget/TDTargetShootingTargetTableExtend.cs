using LetsPiu.Define;
using System.Collections.Generic;

namespace LetsPiu.Base
{
    public static partial class TDTargetShootingTargetTable
    {
        static readonly bool m_IsReadAllData = false;

        static Dictionary<int, List<TDTargetShootingTarget>> m_ModeIdListMap = new Dictionary<int, List<TDTargetShootingTarget>>();
        static Dictionary<int, List<TDTargetShootingTarget>> m_ModeGroupIdListMap = new Dictionary<int, List<TDTargetShootingTarget>>();
        static Dictionary<int, TDTargetShootingTarget> m_ModeGroupCodeIdListMap = new Dictionary<int, TDTargetShootingTarget>();

        static Dictionary<int, List<int>> m_GroupIds = new Dictionary<int, List<int>>();

        public static List<TDTargetShootingTarget> GetDataListByMode(TargetShootingMode mode)
        {
            if (m_ModeIdListMap.TryGetValue((int)mode, out var dataList))
                return dataList;
            return null;
        }

        public static List<TDTargetShootingTarget> GetDataListByModeAndGroup(TargetShootingMode mode, int group)
        {
            int modeGroupId = (int)mode * 100 + group;
            if (m_ModeGroupIdListMap.TryGetValue(modeGroupId, out var dataList))
                return dataList;
            return null;
        }

        public static TDTargetShootingTarget GetDataByModeAndGroupAndCode(TargetShootingMode mode, int group, int code)
        {
            int modeGroupCodeId = (int)mode * 100000 + group * 1000 + code;
            if (m_ModeGroupCodeIdListMap.TryGetValue(modeGroupCodeId, out var data))
                return data;
            return null;
        }

        public static List<int> GetGroupIds(TargetShootingMode mode)
        {
            if (!m_GroupIds.TryGetValue((int)mode, out var groupIds) || groupIds == null)
            {
                groupIds = new List<int>();
                m_GroupIds[(int)mode] = groupIds;

                foreach (int key in m_ModeGroupIdListMap.Keys)
                {
                    if (key / 100 == (int)mode)
                    {
                        groupIds.Add(key % 100);
                    }
                }
                groupIds.Sort();
            }
            return groupIds;
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

        }

        /// <summary>
        /// 完成某行解析 方法名和参数不可修改
        /// </summary>
        static void CompleteRowHead(int key)
        {
            TDTargetShootingTarget data = GetData(key);

            bool sameCode = false;

            int modeGroupCodeId = data.Mode * 100000 + data.Group * 1000 + data.Code;
            if (m_ModeGroupCodeIdListMap.TryGetValue(modeGroupCodeId, out var data_1))
            {
                data_1.AddSameCodeData(data);
                sameCode = true;
            }
            else
            {
                m_ModeGroupCodeIdListMap.Add(modeGroupCodeId, data);
            }

            int modeGroupId = data.Mode * 100 + data.Group;
            if (!m_ModeGroupIdListMap.TryGetValue(modeGroupId, out var dataList_1) || dataList_1 == null)
            {
                dataList_1 = new List<TDTargetShootingTarget>();
                m_ModeGroupIdListMap[modeGroupId] = dataList_1;
            }
            if (!sameCode)
            {
                dataList_1.Add(data);
            }

            if (!m_ModeIdListMap.TryGetValue(data.Mode, out var dataList) || dataList == null)
            {
                dataList = new List<TDTargetShootingTarget>();
                m_ModeIdListMap[data.Mode] = dataList;
            }
            if(!sameCode)
            {
                dataList.Add(data); 
            }
        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDTargetShootingTarget tdData)
        {

        }
    }
}