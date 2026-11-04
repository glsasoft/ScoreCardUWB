using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static partial class TDAudioResourceTable
    {
        static readonly bool m_IsReadAllData = false;

        static Dictionary<int, List<TDAudioResource>> m_GroupId_IDListMap = new Dictionary<int, List<TDAudioResource>>();

        public static TDAudioResource GetData(AudioDefine audioId)
        {
            if (m_GroupId_IDListMap.TryGetValue((int)audioId, out var dataList) && dataList != null && dataList.Count > 0)
            {
                if (dataList.Count == 1)
                {
                    return dataList[0];
                }
                var randomIdx = Random.Range(0, dataList.Count);
                return dataList[randomIdx];
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

        }

        /// <summary>
        /// 完成某行解析 方法名和参数不可修改
        /// </summary>
        static void CompleteRowHead(int key)
        {
            var data = GetData(key);
            if (!m_GroupId_IDListMap.TryGetValue(data.GroupId, out var dataList) || dataList == null)
            {
                dataList = new List<TDAudioResource>();
                m_GroupId_IDListMap[data.GroupId] = dataList;
            }

            dataList.Add(data);
        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDAudioResource tdData)
        {

        }
    }
}