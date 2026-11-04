// 不可编辑，此文件代码自动生成
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LetsPiu.Base
{
    public static partial class TDLoadingTipTable
    {
        private static TDTableMetaData m_MetaData = new TDTableMetaData(TDLoadingTipTable.PreParse,
            TDLoadingTipTable.Parse, TDLoadingTipTable.PostParse, "LoadingTip.txt");
        public static TDTableMetaData metaData
        {
            get { return m_MetaData; }
        }

        private static Dictionary<int, TDLoadingTip> m_DataCache = new Dictionary<int, TDLoadingTip>();
        private static List<TDLoadingTip> m_DataList = new List<TDLoadingTip>();

        private static Dictionary<int, int> m_IndexCache = new Dictionary<int, int>();

        private static IDataStreamReader m_DataR;
        private static int[] m_FieldIndex;
        private static int m_RowCount;
        private static int m_CachedKey = int.MinValue;
        private static TDLoadingTip m_CachedData;
        private static bool m_IsParsing;
        private static List<TDLoadingTip> m_NullDataList = new List<TDLoadingTip>();

        public static void PreParse()
        {
            ClearExtend();

            if (m_DataR != null)
            {
                m_DataR.Dispose();
                m_DataR = null;
            }
        }

        public static void Parse(byte[] fileData, string filePath)
        {
            m_DataCache.Clear();
            m_DataList.Clear();
            m_IndexCache.Clear();

            if (m_DataR != null)
            {
                m_DataR.Dispose();
                m_DataR = null;
            }

            m_DataR = new DataStreamReaderTxt(fileData);
            m_FieldIndex = m_DataR.GetFieldIndex(TDLoadingTip.GetFieldHeadIndex());
            m_RowCount = m_DataR.GetRowCount();
            m_CachedKey = int.MinValue;
            m_CachedData = null;
            m_IsParsing = true;

#if (UNITY_STANDALONE_WIN) || UNITY_EDITOR || UNITY_STANDALONE_OSX
            m_DataR.CheckFieldMatch(TDLoadingTip.GetFieldHeadIndex(), "TDLoadingTipTable");
#endif

            for (int i = 0; i < m_RowCount; ++i)
            {
                m_DataR.ParseDataRowHead();
                int key = m_DataR.ReadInt();
                int offset = m_DataR.GetCurRowDataOffset();

                m_IndexCache[key] = offset;

                m_DataR.BeginDataRow(offset);
                m_DataR.SkipToNextDataRow();
            }

            if (m_IsReadAllData)
            {
                ReadAllData();
            }

            m_IsParsing = false;
            Debug.Log("Parse Success TDLoadingTipTable");
        }

        public static void PostParse()
        {
            ExcuteReadCallBack();
        }

        public static int count
        {
            get
            {
                if (m_IsParsing)
                {
                    Debug.Assert(false, "Can not read table while parsing.");
                    return 0;
                }

                if (!m_IsReadAllData)
                {
                    Debug.Assert(false, "Can not access count when not read all data");
                    return 0;
                }

                return m_DataList.Count;
            }
        }

        public static List<TDLoadingTip> dataList
        {
            get
            {
                if (m_IsParsing)
                {
                    Debug.Assert(false, "Can not read table while parsing.");
                    return m_NullDataList;
                }

                if (!m_IsReadAllData)
                {
                    Debug.Assert(false, "Can not access dataList when not read all data");
                    return m_NullDataList;
                }

                return m_DataList;
            }
        }

        public static TDLoadingTip GetData(int key)
        {
            return GetData(key, false);
        }

        public static TDLoadingTip GetData(int key, bool showErrorTips)
        {
            if (m_IsParsing)
            {
                Debug.Assert(false, "Can not read table while parsing.");
                return null;
            }

            if (m_CachedKey == key)
            {
                return m_CachedData;
            }

            TDLoadingTip data = null;

            if (m_DataCache.TryGetValue(key, out data))
            {
                m_CachedKey = key;
                m_CachedData = data;
                return data;
            }

            if (m_IndexCache.ContainsKey(key))
            {
                if (m_DataR == null)
                {
                    return null;
                }

                m_DataR.BeginDataRow(m_IndexCache[key]);

                data = new TDLoadingTip();
                data.ReadRow(m_DataR, m_FieldIndex);
                OnAddRow(data);

                data.Reset();
                CompleteRowAdd(data);

                m_CachedKey = data.ID;
                m_CachedData = data;

                return (data.ID == key) ? data : null;
            }
            else
            {
                if (showErrorTips)
                {
                    Debug.LogWarningFormat("Can't find key {0} in TDAchievementLevel", key);
                }

                return null;
            }
        }

        public static string Dump()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_DataList.Count; ++i)
            {
                sb.Append(m_DataList[i].ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        static void ExcuteReadCallBack()
        {
            if (m_DataR != null)
            {

                foreach (KeyValuePair<int, int> kv in m_IndexCache)
                {
                    m_DataR.BeginDataRow(kv.Value);
                    CompleteRowHead(kv.Key);
                }

            }

            for (int i = 0; i < m_DataList.Count; ++i)
            {
                TDLoadingTip data = m_DataList[i];
                data.Reset();
                CompleteRowAdd(data);
            }

            CompleteExtend();
        }

        static void OnAddRow(TDLoadingTip memberInstance)
        {
            int key = memberInstance.ID;
            if (m_DataCache.ContainsKey(key))
            {
                Debug.LogFormat("Invaild,  TDAchievementLevelTable Id already exists {0}", key);
            }
            else
            {
                m_DataCache.Add(key, memberInstance);
                m_DataList.Add(memberInstance);
            }
        }

        static void ReadAllData()
        {
            if (m_DataR == null)
            {
                return;
            }

            foreach (KeyValuePair<int, int> kv in m_IndexCache)
            {
                m_DataR.BeginDataRow(kv.Value);
                TDLoadingTip data = new TDLoadingTip();
                data.ReadRow(m_DataR, m_FieldIndex);
                OnAddRow(data);
            }

            m_DataR.Dispose();
            m_DataR = null;
        }
    }
}