// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDPlayerLevelExp
    {
        private int m_id;
        private int m_NeedExp;
        private int m_TotaExp;

        /// <summary>
        /// 等级
        /// </summary>
        public int id {get { return m_id; } }
        /// <summary>
        /// 下一级所需经验
        /// </summary>
        public int NeedExp {get { return m_NeedExp; } }
        /// <summary>
        /// 到达当前等级的累计经验
        /// </summary>
        public int TotaExp {get { return m_TotaExp; } }

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
                        m_id = dataR.ReadInt();
                    break;
                    case 1:
                        m_NeedExp = dataR.ReadInt();
                    break;
                    case 2:
                        m_TotaExp = dataR.ReadInt();
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
            ret.Add("id",0);
            ret.Add("NeedExp",1);
            ret.Add("TotaExp",2);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_id.ToString());
            sb.Append('\t');
            sb.Append(m_NeedExp.ToString());
            sb.Append('\t');
            sb.Append(m_TotaExp.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}