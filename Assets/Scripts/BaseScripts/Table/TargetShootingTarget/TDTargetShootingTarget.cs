// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDTargetShootingTarget
    {
        private int m_id;
        private int m_Mode;
        private int m_Group;
        private int m_Code;
        private string m_Position;
        private int m_Angle;
        private int m_Speed;

        /// <summary>
        /// ID
        /// </summary>
        public int id {get { return m_id; } }
        /// <summary>
        /// 模式
        /// </summary>
        public int Mode {get { return m_Mode; } }
        /// <summary>
        /// 分组
        /// </summary>
        public int Group {get { return m_Group; } }
        /// <summary>
        /// 编号
        /// </summary>
        public int Code {get { return m_Code; } }
        /// <summary>
        /// 位置
        /// </summary>
        public string Position {get { return m_Position; } }
        /// <summary>
        /// 角度
        /// </summary>
        public int Angle {get { return m_Angle; } }
        /// <summary>
        /// 速度
        /// </summary>
        public int Speed {get { return m_Speed; } }

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
                        m_Mode = dataR.ReadInt();
                    break;
                    case 2:
                        m_Group = dataR.ReadInt();
                    break;
                    case 3:
                        m_Code = dataR.ReadInt();
                    break;
                    case 4:
                        m_Position = dataR.ReadString();
                    break;
                    case 5:
                        m_Angle = dataR.ReadInt();
                    break;
                    case 6:
                        m_Speed = dataR.ReadInt();
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
            ret.Add("Mode",1);
            ret.Add("Group",2);
            ret.Add("Code",3);
            ret.Add("Position",4);
            ret.Add("Angle",5);
            ret.Add("Speed",6);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_id.ToString());
            sb.Append('\t');
            sb.Append(m_Mode.ToString());
            sb.Append('\t');
            sb.Append(m_Group.ToString());
            sb.Append('\t');
            sb.Append(m_Code.ToString());
            sb.Append('\t');
            sb.Append(m_Position.ToString());
            sb.Append('\t');
            sb.Append(m_Angle.ToString());
            sb.Append('\t');
            sb.Append(m_Speed.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}