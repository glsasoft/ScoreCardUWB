using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LetsPiu.Base
{
    public partial class TDTargetShootingTarget
    {
        Dictionary<int, TDTargetShootingTarget> sameCodeDataMap = null;
        List<TDTargetShootingTarget> sameCodeDataList = null;
        bool sameCodeDataListSort = false;
        /// <summary>
        /// 完成数据解析 方法名和参数不可修改
        /// </summary>
        public void Reset()
        {

        }

        /// <summary>
        /// 添加一个与自己有相同Code的Target
        /// </summary>
        /// <param name="data"></param>
        public void AddSameCodeData(TDTargetShootingTarget data)
        {
            if (sameCodeDataMap == null)
            {
                sameCodeDataMap = new Dictionary<int, TDTargetShootingTarget>();
                sameCodeDataMap[this.id] = this;
            }
            sameCodeDataMap[data.id] = data;
        }

        /// <summary>
        /// 获取与自己有相同Code的Target列表（包含自己）
        /// </summary>
        /// <returns></returns>
        public List<TDTargetShootingTarget> GetSameCodeDataList()
        {
            if (sameCodeDataList == null && sameCodeDataMap == null)
                return null;

            if (sameCodeDataList == null)
            {
                sameCodeDataList = sameCodeDataMap.Values.ToList();
                sameCodeDataMap = null;
            }

            if (sameCodeDataListSort == false)
            {
                sameCodeDataListSort = true;
                sameCodeDataList.Sort((d1, d2) => d1.id.CompareTo(d2.id));
            }
            return sameCodeDataList;
        }

        ParamReader m_ParamReader = new ParamReader();
        private Vector3? m_positon;
        public Vector3 position
        {
            get
            {
                if (m_positon == null)
                {
                    m_ParamReader.SetStr(Position);
                    if (m_ParamReader.Count() != 3)
                        m_positon = Vector3.zero;
                    else
                        m_positon = new Vector3(m_ParamReader.ReadFloat(), m_ParamReader.ReadFloat(), m_ParamReader.ReadFloat());
                }
                return m_positon.Value;
            }
        }
    }
}