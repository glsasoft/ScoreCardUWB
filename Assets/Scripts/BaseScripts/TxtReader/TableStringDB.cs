using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static class TableStringDB
    {
        // 1111 1111 1111 1111 1111 1111 1111 1111
        // (31-26) (25-0)
        // index  subHash

        const int SUB_HASH_MASK = 0x3FFFFFF;
        const int SUB_INDEX_SHFIT_COUNT = 26;
        const int SUB_INDEX_COUNT = 64;

        static Dictionary<int, List<string>> m_Cache = new Dictionary<int, List<string>>();
        static Dictionary<int, string> m_TempCache = new Dictionary<int, string>();

        public static void CleanTemp()
        {
            m_TempCache = new Dictionary<int, string>();
        }

        public static void CleanAll()
        {
            m_Cache = new Dictionary<int, List<string>>();
            m_TempCache = new Dictionary<int, string>();
        }

        // 不会返回0
        public static int GenHashCode(string str)
        {
            int subHash = str.GetHashCode() & SUB_HASH_MASK;

            if (m_TempCache.ContainsKey(subHash))
            {
                List<string> strList = null;

                if (m_Cache.TryGetValue(subHash, out strList))
                {
                    if (strList.Count >= SUB_INDEX_COUNT)
                    {
                        Debug.LogError("TableStringDB GenHashCode error " + str);
                        return subHash;
                    }
                }
                else
                {
                    strList = new List<string>();
                    strList.Add(m_TempCache[subHash]);
                    m_Cache[subHash] = strList;

                    if (subHash == 0)
                    {
                        strList.Add(null);
                    }
                }

                int subIndex = strList.IndexOf(str, subHash == 0 ? 1 : 0);
                if (subIndex < 0)
                {
                    subIndex = strList.Count;
                    strList.Add(str);
                }

                return ((subIndex << SUB_INDEX_SHFIT_COUNT) | subHash);
            }
            else
            {
                m_TempCache[subHash] = str;
                return subHash;
            }
        }

        public static int GetHashCode(string str)
        {
            int subHash = str.GetHashCode() & SUB_HASH_MASK;
            List<string> strList = null;

            if (m_Cache.TryGetValue(subHash, out strList))
            {
                int subIndex = strList.IndexOf(str);
                if (subIndex == -1)
                {
                    return 0;
                }

                return ((subIndex << SUB_INDEX_SHFIT_COUNT) | subHash);
            }
            else
            {
                return subHash;
            }
        }

        public static string GetStr(int stringHashCode)
        {
            if (stringHashCode == 0)
            {
                Debug.LogError("TableStringDB GetStr error " + stringHashCode);
                return string.Empty;
            }

            int subHash = stringHashCode & SUB_HASH_MASK;
            int subIndex = stringHashCode >> SUB_INDEX_SHFIT_COUNT;
            string str = string.Empty;
            List<string> strList = null;

            if (m_Cache.TryGetValue(subHash, out strList))
            {
                if (strList == null || subIndex >= strList.Count || subIndex < 0)
                {
                    Debug.LogError("TableStringDB GetStr error " + stringHashCode);
                }
                else
                {
                    return strList[subIndex];
                }
            }
            else if (m_TempCache.TryGetValue(subHash, out str))
            {
                return str;
            }
            else
            {
                Debug.LogError("TableStringDB GetStr error " + stringHashCode);
            }

            return str;
        }
    }
}
