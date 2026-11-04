using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public class DataStreamReaderTxt : IDataStreamReader
    {
        private const int FILETYPE_ROW = 1;
        private const int FIELDNAME_ROW = 2;
        private const int SCHEME_ROW_LEN = 3;

        private TxtReader m_TxtReader;
        private string[] m_SchemeNames;
        private FieldType[] m_FieldTypes;
        private Dictionary<string, int> m_SchemeNameColMap;
        private int m_AllLen;

        private int m_Col = 0;
        private int m_CurRow = 0;
        private int m_CurCol = 0;

        int m_BakCol;
        int m_BakCurRow;
        int m_BakCurCol;

        public DataStreamReaderTxt(byte[] data)
        {
            m_TxtReader = new TxtReader();
            m_TxtReader.ParseData(System.Text.UTF8Encoding.UTF8.GetString(data), 0, FIELDNAME_ROW);
            m_SchemeNames = m_TxtReader.GetColName();
            m_Col = m_TxtReader.GetCols();
            PreReadFieldType();
            SkipSchemeName();
            BuildSchemeNameColMap();
        }

        public int GetRowCount()
        {
            return m_TxtReader.GetRows() - SCHEME_ROW_LEN;
        }

        public string[] GetSchemeName()
        {
            return m_SchemeNames;
        }

        public FieldType NextFiledType()
        {
            return m_FieldTypes[m_CurCol];
        }

        public FieldType[] GetFieldTypes()
        {
            return m_FieldTypes;
        }

        public int[] GetFieldIndex(Dictionary<string, int> fieldSourceMap)
        {
            int[] ret = new int[m_Col];
            for (int i = 0; i < m_Col; ++i)
            {
                string key = m_SchemeNames[i];
                if (fieldSourceMap.ContainsKey(key))
                {
                    ret[i] = fieldSourceMap[key];
                }
                else
                {
                    ret[i] = -1;
                }
            }
            return ret;
        }

        public void CheckFieldMatch(Dictionary<string, int> fieldSourceMap, string tableName)
        {
            // Check New Field Add
            for (int i = 0; i < m_Col; ++i)
            {
                string key = m_SchemeNames[i];
                if (!fieldSourceMap.ContainsKey(key))
                {
                    Debug.LogWarning(string.Format("Found a new Field :{0} In {1}",
                        key, tableName));
                }
            }

            //Check Filed Remove
            foreach (string key in fieldSourceMap.Keys)
            {
                bool findKey = false;
                for (int i = 0; i < m_SchemeNames.Length; ++i)
                {
                    if (m_SchemeNames[i].Equals(key))
                    {
                        findKey = true;
                        break;
                    }
                }
                if (!findKey)
                {
                    Debug.LogError(string.Format("Field :{0}  removed In {1}",
                        key, tableName));
                }
            }
        }

        /// <summary>
        /// 返回-1 则表示读到行末尾
        /// </summary>
        /// <returns></returns>
        public int MoreFieldOnRow()
        {
            if (m_CurCol >= m_Col)
            {
                m_CurCol = 0;
                m_CurRow += 1;
                return -1;
            }
            else
            {
                return m_CurCol;
            }
        }

        public void SkipField()
        {
            ReadString();
        }

        public void ParseDataRowHead()
        {
            m_CurCol = 0;
        }

        public void SkipToNextDataRow()
        {
            ++m_CurRow;
            m_CurCol = 0;
        }

        public int GetCurRowDataOffset()
        {
            return m_CurRow;
        }

        public void BeginDataRow(int curRow)
        {
            m_CurRow = curRow;
            m_CurCol = 0;
        }

        public int ReadInt()
        {
            int ret = m_TxtReader.ReadInt(m_CurRow, m_SchemeNames[m_CurCol]);
            m_CurCol += 1;
            return ret;
        }

        public long ReadInt64()
        {
            long ret = m_TxtReader.ReadInt64(m_CurRow, m_SchemeNames[m_CurCol]);
            m_CurCol += 1;
            return ret;
        }

        public int ReadCurRowInt(string schemeName)
        {
            CacheState();

            if (!SkipToCurRowSchemeNameCol(schemeName))
            {
                return 0;
            }

            int val = ReadInt();
            RestoreState();
            return val;
        }

        public bool ReadBool()
        {
            bool ret = m_TxtReader.ReadBool(m_CurRow, m_SchemeNames[m_CurCol]);
            m_CurCol += 1;
            return ret;

        }

        public bool ReadCurRowBool(string schemeName)
        {
            CacheState();

            if (!SkipToCurRowSchemeNameCol(schemeName))
            {
                return false;
            }

            bool val = ReadBool();
            RestoreState();
            return val;
        }

        public float ReadFloat()
        {
            float ret = m_TxtReader.ReadFloat(m_CurRow, m_SchemeNames[m_CurCol]);
            m_CurCol += 1;
            return ret;
        }

        public float ReadCurRowFloat(string schemeName)
        {
            CacheState();

            if (!SkipToCurRowSchemeNameCol(schemeName))
            {
                return 0;
            }

            float val = ReadFloat();
            RestoreState();
            return val;
        }

        public string ReadString()
        {
            string ret = m_TxtReader.ReadString(m_CurRow, m_SchemeNames[m_CurCol]);
            m_CurCol += 1;
            return ret;
        }

        public string ReadCurRowString(string schemeName)
        {
            CacheState();

            if (!SkipToCurRowSchemeNameCol(schemeName))
            {
                return string.Empty;
            }

            string val = ReadString();
            RestoreState();
            return val;
        }

        public Vector3 ReadVector3()
        {
            string str = ReadString();
            return str.ToVector3();
        }

        public Vector3 ReadCurRowVector3(string schemeName)
        {
            CacheState();

            if (!SkipToCurRowSchemeNameCol(schemeName))
            {
                return Vector3.zero;
            }

            Vector3 val = ReadVector3();
            RestoreState();
            return val;
        }

        public Vector2 ReadVector2()
        {
            string str = ReadString();
            return str.ToVector2();
        }

        public Vector2 ReadCurRowVector2(string schemeName)
        {
            CacheState();

            if (!SkipToCurRowSchemeNameCol(schemeName))
            {
                return Vector2.zero;
            }

            Vector2 val = ReadVector2();
            RestoreState();
            return val;
        }

        public void Dispose()
        {

        }

        void SkipSchemeName()
        {
            m_CurRow += SCHEME_ROW_LEN;
        }

        // 获取type类型
        private void PreReadFieldType()
        {
            m_FieldTypes = new FieldType[m_Col];
            for (int i = 0; i < m_FieldTypes.Length; ++i)
            {
                string ret = m_TxtReader.ReadString(FILETYPE_ROW, m_SchemeNames[i]);
                switch (ret.ToLower())
                {
                    case "bool":
                        m_FieldTypes[i] = FieldType.Bool;
                        break;
                    case "byte":
                    case "int":
                        m_FieldTypes[i] = FieldType.Int;
                        break;
                    case "float":
                        m_FieldTypes[i] = FieldType.Float;
                        break;
                    case "string":
                        m_FieldTypes[i] = FieldType.String;
                        break;
                    case "vector2":
                        m_FieldTypes[i] = FieldType.Vector2;
                        break;
                    case "vector3":
                        m_FieldTypes[i] = FieldType.Vector3;
                        break;
                    case "int64":
                        m_FieldTypes[i] = FieldType.Int64;
                        break;
                    default:
                        m_FieldTypes[i] = FieldType.Unkown;
                        break;
                }
            }
        }

        void BuildSchemeNameColMap()
        {
            if (m_SchemeNames == null)
            {
                return;
            }

            m_SchemeNameColMap = new Dictionary<string, int>();
            for (int i = 0; i < m_SchemeNames.Length; ++i)
            {
                m_SchemeNameColMap[m_SchemeNames[i]] = i;
            }
        }

        int GetSchemeNameCol(string name)
        {
            if (m_SchemeNameColMap == null)
            {
                return -1;
            }

            if (m_SchemeNameColMap.ContainsKey(name))
            {
                return m_SchemeNameColMap[name];
            }

            return -1;
        }

        bool SkipToCurRowSchemeNameCol(string name)
        {
            int col = GetSchemeNameCol(name);
            if (col < 0)
            {
                return false;
            }

            if (col >= m_Col)
            {
                return false;
            }

            for (int i = 0; i < col; ++i)
            {
                SkipField();
            }

            return true;
        }

        void CacheState()
        {
            m_BakCol = m_Col;
            m_BakCurRow = m_CurRow;
            m_BakCurCol = m_CurCol;
        }

        void RestoreState()
        {
            m_Col = m_BakCol;
            m_CurRow = m_BakCurRow;
            m_CurCol = m_BakCurCol;
        }
    }
}
