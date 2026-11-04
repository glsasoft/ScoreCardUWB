using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static partial class TDLoadingTipTable
    {
        // 条件类型定义
        public static int conditionKey = 100;

        static readonly bool m_IsReadAllData = false;

        static Dictionary<int, List<int>> m_dicType2IDLIst = new();
        static List<int> m_TempList;

        public static void GetData(int type, List<int> tempList, bool isClear)
        {
            if (tempList == null)
            {
                return;
            }

            if (isClear)
            {
                tempList.Clear();
            }

            if (m_dicType2IDLIst.TryGetValue(type, out List<int> list) && list != null)
            {
                tempList.AddRange(list);
            }
        }

        /// <summary>
        /// 清理表格 方法名和参数不可修改
        /// </summary>
        static void ClearExtend()
        {
            m_dicType2IDLIst.Clear();
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
            var dateType = m_DataR.ReadCurRowInt("Type");

            if (!m_dicType2IDLIst.TryGetValue(dateType, out m_TempList) || m_TempList == null)
            {
                m_TempList = new List<int>();
                m_dicType2IDLIst[dateType] = m_TempList;
            }

            m_TempList.Add(key);
        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDLoadingTip tdData)
        {
          
        }
    }
}