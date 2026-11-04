using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LetsPiu.Base
{
    public static partial class Helper
    {
        static ParamReader m_BaseParamReader = new ParamReader();
        public static string SafeFormat(this string format, params object[] args)
        {
            string ret = format;

            try
            {
                ret = string.Format(format, args);
            }
            catch (System.Exception ex)
            {
                Debug.LogFormat("[HelperExtend:SafeFormat] format: {0}, msg: {1}, stacktrace: {2}", format, ex.Message, ex.StackTrace);
            }

            return ret;
        }

        public static int ToInt(this string value)
        {
            int.TryParse(value, out int ret);
            return ret;
        }

        public static bool ToBool(this string value)
        {
            bool ret = false;
            if (ToInt(value) > 0)
            {
                return true;
            }
            else
            {
                bool.TryParse(value, out ret);
            }
            return ret;
        }

        public static long ToInt64(this string value)
        {
            Int64.TryParse(value, out long ret);

            return ret;
        }

        public static float ToFloat(this string value)
        {
            float.TryParse(value, out float ret);
            return ret;
        }

        public static bool IsConfigStringValid(string data)
        {
            return !string.IsNullOrEmpty(data) && data != "null";
        }

        public static Vector3 ToVector3(this string pos, char split = ',')
        {
            if (string.IsNullOrEmpty(pos))
            {
                return Vector3.zero;
            }

            m_BaseParamReader.SetStr(pos, split);
            int count = m_BaseParamReader.Count();

            float x = count > 0 ? m_BaseParamReader.ReadFloat() : 0f;
            float y = count > 1 ? m_BaseParamReader.ReadFloat() : 0f;
            float z = count > 2 ? m_BaseParamReader.ReadFloat() : 0f;

            Vector3 ret = new Vector3(x, y, z);

            return ret;
        }

        public static Vector2 ToVector2(this string pos, char splitChar = ',')
        {
            if (string.IsNullOrEmpty(pos))
            {
                return Vector2.zero;
            }

            m_BaseParamReader.SetStr(pos, splitChar);
            int count = m_BaseParamReader.Count();

            float x = count > 0 ? m_BaseParamReader.ReadFloat() : 0f;
            float y = count > 1 ? m_BaseParamReader.ReadFloat() : 0f;

            Vector2 ret = new Vector2(x, y);

            return ret;
        }

        public static void ToListInt(this string res, List<int> list, char splitChar = ',')
        {
            if (string.IsNullOrEmpty(res))
            {
                return;
            }

            if (list == null)
            {
                return;
            }

            list.Clear();

            m_BaseParamReader.SetStr(res, splitChar);
            int count = m_BaseParamReader.Count();

            for (int i = 0; i < count; i++)
            {
                list.Add(m_BaseParamReader.ReadInt());
            }
        }
    }
}
