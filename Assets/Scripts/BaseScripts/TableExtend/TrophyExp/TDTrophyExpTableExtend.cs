using System;

namespace LetsPiu.Base
{
    public static partial class TDTrophyExpTable
    {
        static readonly bool m_IsReadAllData = false;

        static int m_MaxLevel;
        public static int MaxLevel => m_MaxLevel;
        
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
            if (data is null)
            {
                return;
            }
            m_MaxLevel = Math.Max(m_MaxLevel, data.id);
        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDTrophyExp tdData)
        {

        }
    }
}