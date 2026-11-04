using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public class ParamReader
    {
        enum Symbol
        {
            None,
            Positive,
            Nagetive
        }

        string m_Str;
        int m_Pos;
        int m_Count = -1;

        char[] m_SplitChars;

        char[] DEFAULT_SPLIT = new char[]
        {
            ',',
            ';',
            '|',
            ':',
        };

        char[] CUSTOM_SPLIT = new char[]
        {
            ',',
        };

        public int remainCount
        {
            get
            {
                if (string.IsNullOrEmpty(m_Str))
                {
                    return 0;
                }
                else
                {
                    int count = 1;

                    for (int i = m_Pos; i < m_Str.Length; ++i)
                    {
                        char c = m_Str[i];

                        if (IsSplitChar(c))
                        {
                            ++count;
                        }
                    }

                    return count;
                }
            }
        }

        // 123,34234,0.23498,abc;123,34234,0.23498,abc;
        public void SetStr(string str, char splitChar = '\0')
        {
            m_Str = str;
            m_Pos = 0;
            m_Count = -1;

            if (splitChar != '\0')
            {
                m_SplitChars = CUSTOM_SPLIT;
                CUSTOM_SPLIT[0] = splitChar;
            }
            else
            {
                m_SplitChars = DEFAULT_SPLIT;
            }
        }

        public void SetStr(string str, char[] splitChars)
        {
            m_Str = str;
            m_Pos = 0;
            m_Count = -1;

            if (splitChars == null)
            {
                m_SplitChars = DEFAULT_SPLIT;
            }
            else
            {
                m_SplitChars = splitChars;
            }
        }

        public void Clear()
        {
            m_Str = string.Empty;
            m_Pos = 0;
            m_Count = -1;
            m_SplitChars = DEFAULT_SPLIT;
        }

        public int ReadInt()
        {
            if (m_Str == null || m_Pos >= m_Str.Length)
            {
                return 0;
            }

            int nextPos = GetNextSpliterPos(m_Pos);
            int val = GetInt(m_Str, m_Pos, nextPos);

            m_Pos = nextPos + 1;

            return val;
        }

        public float ReadFloat()
        {
            if (m_Str == null || m_Pos >= m_Str.Length)
            {
                return 0;
            }

            int nextPos = GetNextSpliterPos(m_Pos);
            float val = GetFloat(m_Str, m_Pos, nextPos);

            m_Pos = nextPos + 1;

            return val;
        }

        public long ReadInt64()
        {
            if (m_Str == null || m_Pos >= m_Str.Length)
            {
                return 0;
            }

            int nextPos = GetNextSpliterPos(m_Pos);
            long val = GetInt64(m_Str, m_Pos, nextPos);

            m_Pos = nextPos + 1;

            return val;
        }

        public string ReadString()
        {
            if (m_Str == null || m_Pos >= m_Str.Length)
            {
                return string.Empty;
            }

            int nextPos = GetNextSpliterPos(m_Pos);
            string val = GetStr(m_Str, m_Pos, nextPos);

            m_Pos = nextPos + 1;

            return val;
        }

        public bool HasData()
        {
            if (string.IsNullOrEmpty(m_Str))
            {
                return false;
            }

            if (m_Pos >= m_Str.Length)
            {
                return false;
            }

            return true;
        }

        public int Count()
        {
            if (m_Count >= 0)
            {
                return m_Count;
            }

            if (string.IsNullOrEmpty(m_Str))
            {
                m_Count = 0;
                return 0;
            }

            int count = 1;

            for (int i = 0; i < m_Str.Length; ++i)
            {
                char c = m_Str[i];

                if (IsSplitChar(c))
                {
                    ++count;
                }
            }

            m_Count = count;
            return count;
        }

        public bool SkipTo(int index)
        {
            if (m_Str == null || index < 0)
            {
                return false;
            }

            m_Pos = 0;

            for (int i = 0; i < index; ++i)
            {
                int nextPos = GetNextSpliterPos(m_Pos);

                if (nextPos != m_Pos && nextPos < m_Str.Length)
                {
                    m_Pos = GetNextSpliterPos(m_Pos) + 1;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        int GetNextSpliterPos(int startPos)
        {
            if (m_Str == null || startPos < 0)
            {
                return 0;
            }

            for (int i = startPos; i < m_Str.Length; ++i)
            {
                char c = m_Str[i];

                if (IsSplitChar(c))
                {
                    return i;
                }
            }

            return m_Str.Length;
        }

        int GetInt(string str, int startPos, int endPos)
        {
            int result = 0;

            if (str == null || startPos < 0)
            {
                return result;
            }

            startPos = MoveToNoSpacePos(str, startPos);
            if (startPos < 0)
            {
                return result;
            }

            var symbol = GetSymbol(str, startPos);
            if (symbol != Symbol.None)
            {
                ++startPos;
            }

            for (int i = startPos; i < endPos && i < str.Length; ++i)
            {
                if (str[i] == ' ')
                {
                    break;
                }
                else
                {
                    result = result * 10 + (str[i] - '0');
                }
            }

            if (symbol == Symbol.Nagetive)
            {
                result = -result;
            }
            return result;
        }

        float GetFloat(string str, int startPos, int endPos)
        {
            float result = 0;
            bool hasDot = false;
            float factor = 1;

            if (str == null || startPos < 0)
            {
                return result;
            }

            startPos = MoveToNoSpacePos(str, startPos);
            if (startPos < 0)
            {
                return result;
            }

            var symbol = GetSymbol(str, startPos);
            if (symbol != Symbol.None)
            {
                ++startPos;
            }

            for (int i = startPos; i < endPos && i < str.Length; ++i)
            {
                if (str[i] == ' ')
                {
                    break;
                }
                else if (str[i] == '.')
                {
                    hasDot = true;
                }
                else if (!hasDot)
                {
                    result = result * 10 + (str[i] - '0');
                }
                else
                {
                    factor = factor * 0.1f;
                    result += (str[i] - '0') * factor;
                }
            }

            if (symbol == Symbol.Nagetive)
            {
                result = -result;
            }
            return result;
        }

        long GetInt64(string str, int startPos, int endPos)
        {
            long result = 0;

            if (str == null || startPos < 0)
            {
                return result;
            }

            startPos = MoveToNoSpacePos(str, startPos);
            if (startPos < 0)
            {
                return result;
            }

            var symbol = GetSymbol(str, startPos);
            if (symbol != Symbol.None)
            {
                ++startPos;
            }

            for (int i = startPos; i < endPos && i < str.Length; ++i)
            {
                if (str[i] == ' ')
                {
                    break;
                }
                else
                {
                    result = result * 10 + (str[i] - '0');
                }
            }

            if (symbol == Symbol.Nagetive)
            {
                result = -result;
            }
            return result;
        }

        string GetStr(string str, int startPos, int endPos)
        {
            string result = string.Empty;

            if (str == null || startPos < 0)
            {
                return result;
            }

            if (startPos >= str.Length)
            {
                return result;
            }

            if (endPos >= str.Length)
            {
                return str.Substring(startPos);
            }

            return str.Substring(startPos, endPos - startPos);
        }

        int MoveToNoSpacePos(string str, int pos)
        {
            for (int i = pos; i < str.Length; i++)
            {
                char c = str[i];
                if (c != ' ')
                {
                    return i;
                }
            }

            return -1;
        }

        Symbol GetSymbol(string str, int pos)
        {
            if (str != null && pos < str.Length)
            {
                return GetSymbol(str[pos]);
            }

            return Symbol.None;
        }

        Symbol GetSymbol(char c)
        {
            if (c == '+')
            {
                return Symbol.Positive;
            }
            else if (c == '-')
            {
                return Symbol.Nagetive;
            }

            return Symbol.None;
        }

        bool IsSplitChar(char c)
        {
            if (m_SplitChars == null)
            {
                return false;
            }

            for (int i = 0; i < m_SplitChars.Length; ++i)
            {
                if (c == m_SplitChars[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
