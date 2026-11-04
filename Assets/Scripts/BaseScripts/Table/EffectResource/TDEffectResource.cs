// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;

namespace LetsPiu.Base
{
    public partial class TDEffectResource
    {
        private int m_ID;
        private string m_ResName;
        private int m_Duration;
        private int m_DelayDestroyTime;
        private int m_EffectType;
        private string m_BindingPointName;

        /// <summary>
        /// 序号
        /// </summary>
        public int ID {get { return m_ID; } }
        /// <summary>
        /// 特效名称
        /// </summary>
        public string ResName {get { return m_ResName; } }
        /// <summary>
        /// 持续时间(毫秒)
        /// </summary>
        public int Duration {get { return m_Duration; } }
        /// <summary>
        /// 延时销毁时间(毫秒)
        /// </summary>
        public int DelayDestroyTime {get { return m_DelayDestroyTime; } }
        /// <summary>
        /// 特效类型
        /// </summary>
        public int EffectType {get { return m_EffectType; } }
        /// <summary>
        /// 挂点
        /// </summary>
        public string BindingPointName {get { return m_BindingPointName; } }

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
                        m_ResName = dataR.ReadString();
                    break;
                    case 2:
                        m_Duration = dataR.ReadInt();
                    break;
                    case 3:
                        m_DelayDestroyTime = dataR.ReadInt();
                    break;
                    case 4:
                        m_EffectType = dataR.ReadInt();
                    break;
                    case 5:
                        m_BindingPointName = dataR.ReadString();
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
            ret.Add("ResName",1);
            ret.Add("Duration",2);
            ret.Add("DelayDestroyTime",3);
            ret.Add("EffectType",4);
            ret.Add("BindingPointName",5);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_ID.ToString());
            sb.Append('\t');
            sb.Append(m_ResName.ToString());
            sb.Append('\t');
            sb.Append(m_Duration.ToString());
            sb.Append('\t');
            sb.Append(m_DelayDestroyTime.ToString());
            sb.Append('\t');
            sb.Append(m_EffectType.ToString());
            sb.Append('\t');
            sb.Append(m_BindingPointName.ToString());
            sb.Append('\t');

            return sb.ToString();
        }
    }
}