// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDAudioResource
    {
        private int m_ID;
        private int m_GroupId;
        private string m_ResName;
        private int m_IsLoop;
        private int m_Type;
        private int m_Kind;

        /// <summary>
        /// 序号
        /// </summary>
        public int ID {get { return m_ID; } }
        /// <summary>
        /// 随机组
        /// </summary>
        public int GroupId {get { return m_GroupId; } }
        /// <summary>
        /// 音效资源名称
        /// </summary>
        public string ResName {get { return m_ResName; } }
        /// <summary>
        /// 是否循环播放(0否1是)
        /// </summary>
        public int IsLoop {get { return m_IsLoop; } }
        /// <summary>
        /// 是否3D播放(0否1是)
        /// </summary>
        public int Type {get { return m_Type; } }
        /// <summary>
        /// 音效种类
        /// </summary>
        public int Kind {get { return m_Kind; } }

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
                        m_GroupId = dataR.ReadInt();
                    break;
                    case 2:
                        m_ResName = dataR.ReadString();
                    break;
                    case 3:
                        m_IsLoop = dataR.ReadInt();
                    break;
                    case 4:
                        m_Type = dataR.ReadInt();
                    break;
                    case 5:
                        m_Kind = dataR.ReadInt();
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
            ret.Add("GroupId",1);
            ret.Add("ResName",2);
            ret.Add("IsLoop",3);
            ret.Add("Type",4);
            ret.Add("Kind",5);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_ID.ToString());
            sb.Append('\t');
            sb.Append(m_GroupId.ToString());
            sb.Append('\t');
            sb.Append(m_ResName.ToString());
            sb.Append('\t');
            sb.Append(m_IsLoop.ToString());
            sb.Append('\t');
            sb.Append(m_Type.ToString());
            sb.Append('\t');
            sb.Append(m_Kind.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}