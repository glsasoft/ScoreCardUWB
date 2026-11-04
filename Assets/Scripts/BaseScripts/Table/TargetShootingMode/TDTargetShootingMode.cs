// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDTargetShootingMode
    {
        private int m_id;
        private int m_ColumnCount;
        private int m_RowCount;
        private int m_InitTargetCount;
        private int m_MinSpeed;
        private int m_MaxSpeed;
        private int m_PassingScore;

        /// <summary>
        /// 模式ID
        /// </summary>
        public int id {get { return m_id; } }
        /// <summary>
        /// 列数
        /// </summary>
        public int ColumnCount {get { return m_ColumnCount; } }
        /// <summary>
        /// 行数
        /// </summary>
        public int RowCount {get { return m_RowCount; } }
        /// <summary>
        /// 初始化靶子数量
        /// </summary>
        public int InitTargetCount {get { return m_InitTargetCount; } }
        /// <summary>
        /// 最小速度
        /// </summary>
        public int MinSpeed {get { return m_MinSpeed; } }
        /// <summary>
        /// 最大速度
        /// </summary>
        public int MaxSpeed {get { return m_MaxSpeed; } }
        /// <summary>
        /// 个人及格分
        /// </summary>
        public int PassingScore {get { return m_PassingScore; } }

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
                        m_ColumnCount = dataR.ReadInt();
                    break;
                    case 2:
                        m_RowCount = dataR.ReadInt();
                    break;
                    case 3:
                        m_InitTargetCount = dataR.ReadInt();
                    break;
                    case 4:
                        m_MinSpeed = dataR.ReadInt();
                    break;
                    case 5:
                        m_MaxSpeed = dataR.ReadInt();
                    break;
                    case 6:
                        m_PassingScore = dataR.ReadInt();
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
            ret.Add("ColumnCount",1);
            ret.Add("RowCount",2);
            ret.Add("InitTargetCount",3);
            ret.Add("MinSpeed",4);
            ret.Add("MaxSpeed",5);
            ret.Add("PassingScore",6);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_id.ToString());
            sb.Append('\t');
            sb.Append(m_ColumnCount.ToString());
            sb.Append('\t');
            sb.Append(m_RowCount.ToString());
            sb.Append('\t');
            sb.Append(m_InitTargetCount.ToString());
            sb.Append('\t');
            sb.Append(m_MinSpeed.ToString());
            sb.Append('\t');
            sb.Append(m_MaxSpeed.ToString());
            sb.Append('\t');
            sb.Append(m_PassingScore.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}