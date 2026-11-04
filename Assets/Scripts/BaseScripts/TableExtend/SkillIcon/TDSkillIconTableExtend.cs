using LetsPiu.Define;
using System.Collections.Generic;

namespace LetsPiu.Base
{
    public static partial class TDSkillIconTable
    {
        static readonly bool m_IsReadAllData = false;

        // key = PrefessionId*10000+SkillType value = ID
        static Dictionary<int,int> m_IDMap = new Dictionary<int, int>();

        public static TDSkillIcon GetData(ProfessionType professionId, SkillType skillType)
        {
            var complexKey = GetKey((int)professionId, (int)skillType);
            if (m_IDMap.TryGetValue(complexKey, out int id))
            {
                return GetData(id);
            }
            return null;
        }

        /// <summary>
        /// 清理表格 方法名和参数不可修改
        /// </summary>
        static void ClearExtend()
        {
            m_IDMap.Clear();
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
            if (data == null)
            {
                return;
            }
            var complexKey = GetKey(data.ProfessionId, data.SkillType);
            if (m_IDMap.ContainsKey(complexKey))
            {
                return;
            }
            m_IDMap.Add(complexKey, data.ID);

        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDSkillIcon tdData)
        {

        }

        static int GetKey(int professionId,int skillType)
        {
            return professionId * 10000 + skillType;
        }
    }
}