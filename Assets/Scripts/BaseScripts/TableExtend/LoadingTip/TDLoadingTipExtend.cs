using System.Collections.Generic;

namespace LetsPiu.Base
{
    public partial class TDLoadingTip
    {

        List<int> m_ConditionList;

        /// <summary>
        /// 条件Id
        /// </summary>
        public int conditionId
        {
            get
            {
                if (m_ConditionList == null)
                {
                    m_ConditionList = new List<int>();
                    Condition.ToListInt(m_ConditionList);
                }
                return m_ConditionList[0];
            }
        }

        /// <summary>
        /// 数量Id
        /// </summary>
        public int conditionParam
        {
            get
            {
                if (m_ConditionList == null)
                {
                    m_ConditionList = new List<int>();
                    Condition.ToListInt(m_ConditionList);
                }
                return m_ConditionList[0];
            }
        }

        /// <summary>
        /// 完成数据解析 方法名和参数不可修改
        /// </summary>
        public void Reset()
        {

        }
    }
}