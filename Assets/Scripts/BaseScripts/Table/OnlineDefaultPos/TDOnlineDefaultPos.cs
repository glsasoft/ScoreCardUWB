// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDOnlineDefaultPos
    {
        private int m_id;
        private int m_GameMode;
        private int m_PosOrder;
        private int m_TeamType;
        private string m_Position;
        private string m_Rotation;

        /// <summary>
        /// 序号
        /// </summary>
        public int id {get { return m_id; } }
        /// <summary>
        /// 游戏模式
        /// </summary>
        public int GameMode {get { return m_GameMode; } }
        /// <summary>
        /// 位置排序
        /// </summary>
        public int PosOrder {get { return m_PosOrder; } }
        /// <summary>
        /// 队伍类型
        /// </summary>
        public int TeamType {get { return m_TeamType; } }
        /// <summary>
        /// 位置
        /// </summary>
        public string Position {get { return m_Position; } }
        /// <summary>
        /// 旋转
        /// </summary>
        public string Rotation {get { return m_Rotation; } }

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
                        m_GameMode = dataR.ReadInt();
                    break;
                    case 2:
                        m_PosOrder = dataR.ReadInt();
                    break;
                    case 3:
                        m_TeamType = dataR.ReadInt();
                    break;
                    case 4:
                        m_Position = dataR.ReadString();
                    break;
                    case 5:
                        m_Rotation = dataR.ReadString();
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
            ret.Add("GameMode",1);
            ret.Add("PosOrder",2);
            ret.Add("TeamType",3);
            ret.Add("Position",4);
            ret.Add("Rotation",5);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_id.ToString());
            sb.Append('\t');
            sb.Append(m_GameMode.ToString());
            sb.Append('\t');
            sb.Append(m_PosOrder.ToString());
            sb.Append('\t');
            sb.Append(m_TeamType.ToString());
            sb.Append('\t');
            sb.Append(m_Position.ToString());
            sb.Append('\t');
            sb.Append(m_Rotation.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}