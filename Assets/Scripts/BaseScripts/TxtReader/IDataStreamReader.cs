using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    //Don't Edit it (高能预警,不要编辑)
    public enum FieldType
    {
        Unkown,
        Int,
        Float,
        String,
        Vector2,
        Vector3,
        Bool,
        Int64,
    }

    public interface IDataStreamReader
    {
        int GetRowCount();

        string[] GetSchemeName();

        FieldType[] GetFieldTypes();

        int[] GetFieldIndex(Dictionary<string, int> fieldSourceMap);

        void CheckFieldMatch(Dictionary<string, int> fieldSourceMap, string tableName);

        /// <summary>
        /// 返回-1 则表示读到行末尾
        /// </summary>
        /// <returns></returns>
        int MoreFieldOnRow();

        FieldType NextFiledType();

        void SkipField();

        void ParseDataRowHead();

        void SkipToNextDataRow();

        int GetCurRowDataOffset();

        void BeginDataRow(int cursor);

        int ReadInt();

        long ReadInt64();

        int ReadCurRowInt(string schemeName);

        bool ReadBool();

        bool ReadCurRowBool(string schemeName);

        Vector3 ReadVector3();

        Vector3 ReadCurRowVector3(string schemeName);

        Vector2 ReadVector2();

        Vector2 ReadCurRowVector2(string schemeName);

        float ReadFloat();

        float ReadCurRowFloat(string schemeName);

        string ReadString();

        string ReadCurRowString(string schemeName);

        void Dispose();
    }
}
