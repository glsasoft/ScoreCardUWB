using UnityEngine;

namespace LetsPiu.Base
{
    public partial class TDOnlineDefaultPos
    {
        Vector3 m_DefaultPos;
        Vector3 m_DefaultRot;

        ParamReader m_ParamReader = new ParamReader();

        public Vector3 defaultPos
        {
            get
            {
                if (m_DefaultPos == default)
                {
                    TryGetParamReader();
                    m_ParamReader.SetStr(Position);
                    if (m_ParamReader.Count() != 3)
                    {
                        return Vector3.zero;
                    }
                    m_DefaultPos = new Vector3(m_ParamReader.ReadFloat(), m_ParamReader.ReadFloat(), m_ParamReader.ReadFloat());
                }
                return m_DefaultPos;
            }
        }

        public Vector3 defaultRot
        {
            get
            {
                if (m_DefaultRot == default)
                {
                    TryGetParamReader();
                    m_ParamReader.SetStr(Rotation);
                    if (m_ParamReader.Count() != 3)
                    {
                        return Vector3.zero;
                    }
                    m_DefaultRot = new Vector3(m_ParamReader.ReadFloat(), m_ParamReader.ReadFloat(), m_ParamReader.ReadFloat());
                }
                return m_DefaultRot;
            }
        }

        /// <summary>
        /// 完成数据解析 方法名和参数不可修改
        /// </summary>
        public void Reset()
        {

        }

        void TryGetParamReader()
        {
            if (m_ParamReader == null)
            {
                m_ParamReader = new ParamReader();
            }
        }
    }
}