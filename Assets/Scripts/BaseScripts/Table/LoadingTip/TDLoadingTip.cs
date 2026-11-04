// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDLoadingTip
    {
        private int m_ID;
        private int m_Type;
        private string m_Des;
        private string m_Condition;
        private int m_Priority;

        /// <summary>
        /// 序号
        /// </summary>
        public int ID {get { return m_ID; } }
        /// <summary>
        /// 类型
        /// </summary>
        public int Type {get { return m_Type; } }
        /// <summary>
        /// 描述
        /// </summary>
        public string Des {get { return m_Des; } }
        /// <summary>
        /// 条件
        /// </summary>
        public string Condition {get { return m_Condition; } }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority {get { return m_Priority; } }

        public void ReadRow(IDataStreamReader dataR, int[] filedIndex)
        {
            var schemeNames = dataR.GetSchemeName();
            int col = 0;
            while (true)
            {
                col = dataR.MoreFieldOnRow();
                if (col == -1)
                {
                    break;
                }
                switch (filedIndex[col])
                {
                    case 0:
                        m_ID = dataR.ReadInt();
                    break;
                    case 1:
                        m_Type = dataR.ReadInt();
                    break;
                    case 2:
                        m_Des = dataR.ReadString();
                    break;
                    case 3:
                        m_Condition = dataR.ReadString();
                    break;
                    case 4:
                        m_Priority = dataR.ReadInt();
                    break;
                    default:
                        dataR.ReadString();
                        break;
                }
            }

        }

        public static Dictionary<string, int> GetFieldHeadIndex()
        {
            Dictionary<string, int> ret = new Dictionary<string, int>(9);
            ret.Add("ID",0);
            ret.Add("Type",1);
            ret.Add("Des",2);
            ret.Add("Condition",3);
            ret.Add("Priority",4);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_ID.ToString());
            sb.Append('\t');
            sb.Append(m_Type.ToString());
            sb.Append('\t');
            sb.Append(m_Des.ToString());
            sb.Append('\t');
            sb.Append(m_Condition.ToString());
            sb.Append('\t');
            sb.Append(m_Priority.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}