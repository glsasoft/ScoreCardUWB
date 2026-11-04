// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDSkillIcon
    {
        private int m_ID;
        private int m_ProfessionId;
        private int m_SkillType;
        private string m_ResName;

        /// <summary>
        /// 序号
        /// </summary>
        public int ID {get { return m_ID; } }
        /// <summary>
        /// 职业ID
        /// </summary>
        public int ProfessionId {get { return m_ProfessionId; } }
        /// <summary>
        /// 技能类型
        /// </summary>
        public int SkillType {get { return m_SkillType; } }
        /// <summary>
        /// 技能icon
        /// </summary>
        public string ResName {get { return m_ResName; } }

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
                        m_ProfessionId = dataR.ReadInt();
                    break;
                    case 2:
                        m_SkillType = dataR.ReadInt();
                    break;
                    case 3:
                        m_ResName = dataR.ReadString();
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
            ret.Add("ProfessionId",1);
            ret.Add("SkillType",2);
            ret.Add("ResName",3);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_ID.ToString());
            sb.Append('\t');
            sb.Append(m_ProfessionId.ToString());
            sb.Append('\t');
            sb.Append(m_SkillType.ToString());
            sb.Append('\t');
            sb.Append(m_ResName.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}