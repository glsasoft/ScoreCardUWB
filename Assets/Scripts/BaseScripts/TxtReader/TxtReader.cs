using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    class TxtReader
    {
        struct LineData
        {
            public string[] col;
        }

        string m_FileData = "";
        int m_ColCount;
        string[] m_ColName;

        List<LineData> _lines = new List<LineData>();

        List<string> lineList = new List<string>(512);

        public bool ParseData(string fileData, int start_line = 3, int colNameLine = 0)
        {
            m_FileData = fileData;

            if (null == m_FileData)
            {
                return false;
            }

            int line_num = 0;
            int filed_cur = 0;
            string field;
            int fieldLen;
            for (int i = 0; i < m_FileData.Length; ++i)
            {
                switch (m_FileData[i])
                {
                    case '\t':
                        field = m_FileData.Substring(filed_cur, i - filed_cur);
                        lineList.Add(field);
                        filed_cur = i + 1;
                        continue;
                    case '\r':
                        {
                            // 忽略
                            continue;
                        }
                    case '\n':
                        {
                            ++line_num;

                            if ((start_line > 0) && ((int)line_num < start_line))
                            {
                                continue;
                            }

                            if (m_FileData[i - 1] == '\r')
                            {
                                fieldLen = i - filed_cur - 1;
                            }
                            else
                            {
                                fieldLen = i - filed_cur;
                            }
                            field = m_FileData.Substring(filed_cur, fieldLen);
                            lineList.Add(field);
                            LineData ld;
                            ld.col = lineList.ToArray();
                            _lines.Add(ld);
                            filed_cur = i + 1;
                            if (0 == m_ColCount)
                            {
                                m_ColCount = lineList.Count;
                            }
                            if (line_num - 1 == colNameLine)
                            {
                                m_ColName = ld.col;
                            }
                            lineList.Clear();
                        }
                        break;

                    default:
                        break;
                }
            }

            m_FileData = null;
            lineList.Clear();

            return true;
        }

        public string[] GetColName()
        {
            return m_ColName;
        }

        public string GetField(int row, int col)
        {
            if (row < 0 || row >= _lines.Count)
            {
                return null;
            }

            var columns = _lines[row].col;
            if (columns == null)
            {
                return null;
            }

            if (col < 0 || col >= columns.Length)
            {
                return null;
            }

            return columns[col];
        }

        public string[] GetARow(int row)
        {
            return _lines[row].col;
        }

        public int GetCols()
        {
            return m_ColCount;
        }

        public int GetRows()
        {
            return _lines.Count;
        }

        public int ReadInt(int row, string colName)
        {
            string data = GetData(row, colName);

            if (null == data)
            {
                return 0;
            }
            else
            {
                return data.ToInt();
            }
        }

        public bool ReadBool(int row, string colName)
        {
            string data = GetData(row, colName);

            if (null == data)
            {
                return false;
            }
            else
            {
                return data.ToBool();
            }
        }

        public long ReadInt64(int row, string colName)
        {
            string data = GetData(row, colName);

            if (null == data)
            {
                return 0;
            }
            else
            {
                return data.ToInt64();
            }
        }

        public string ReadString(int row, string colName)
        {
            string data = GetData(row, colName);

            if (null == data)
            {
                return "";
            }
            else
            {
                return data;
            }
        }

        public float ReadFloat(int row, string colName)
        {
            string data = GetData(row, colName);

            if (null == data)
            {
                return 0f;
            }
            else
            {
                return data.ToFloat();
            }
        }

        public int[] ReadIntArray(int row, string colName, char split)
        {
            string data = GetData(row, colName);

            if (!Helper.IsConfigStringValid(data))
            {
                return new int[0];
            }

            string[] datas = data.Split(new char[] { split });

            int[] rets = new int[datas.Length];

            for (int i = 0; i < rets.Length; ++i)
            {
                rets[i] = datas[i].ToInt();
            }

            return rets;
        }

        public long[] ReadInt64Array(int row, string colName, char split)
        {
            string data = GetData(row, colName);

            if (!Helper.IsConfigStringValid(data))
            {
                return new long[0];
            }

            string[] datas = data.Split(new char[] { split });

            long[] rets = new long[datas.Length];

            for (int i = 0; i < rets.Length; ++i)
            {
                rets[i] = datas[i].ToInt64();
            }

            return rets;
        }

        public string[] ReadStringArray(int row, string colName, char split)
        {
            string data = GetData(row, colName);

            if (!Helper.IsConfigStringValid(data))
            {
                return new string[0];
            }

            string[] datas = data.Split(new char[] { split });

            return datas;
        }

        string GetData(int row, string colName)
        {
            int col = GetColIndex(colName);
            return GetField(row, col);
        }

        int GetColIndex(string colName)
        {
            if (null == colName)
            {
                return -1;
            }

            for (int i = 0; i < m_ColName.Length; ++i)
            {
                if (m_ColName[i] == colName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
