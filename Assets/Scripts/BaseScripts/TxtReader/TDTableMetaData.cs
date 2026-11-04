using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public class TDTableMetaData
    {
        public delegate void DTableOnPreParse();
        public delegate void DTableOnParse(byte[] fileData, string filePath);
        public delegate void DTableOnPostParse();

        private DTableOnPreParse m_OnPreParse;
        private DTableOnParse m_OnParse;
        private DTableOnPostParse m_OnPostParse;

        public DTableOnPreParse onPreParse
        {
            get
            {
                return m_OnPreParse;
            }
        }

        public DTableOnParse onParse
        {
            get
            {
                return m_OnParse;
            }
        }

        public DTableOnPostParse onPostParse
        {
            get
            {
                return m_OnPostParse;
            }
        }

        public string tableName { get; set; }
        public TDTableMetaData(DTableOnPreParse onPreParse, DTableOnParse onParse, DTableOnPostParse onPostParse, string tName)
        {
            m_OnPreParse = onPreParse;
            m_OnParse = onParse;
            m_OnPostParse = onPostParse;
            tableName = tName;
        }
    }
}
