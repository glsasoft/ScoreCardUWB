using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LetsPiu
{
    public static class HelperExtend
    {
        public static string SafeFormat(this string format, params object[] args)
        {
            string ret = format;

            try
            {
                ret = string.Format(format, args);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[HelperExtend:SafeFormat] format: {0}, msg: {1}, stacktrace: {2}".SafeFormat(format, ex.Message, ex.StackTrace));
            }

            return ret;
        }

        public static void SetListener(this UnityEvent onEvent, UnityAction action)
        {
            if (onEvent == null)
            {
                return;
            }

            onEvent.RemoveAllListeners();
            onEvent.AddListener(action);
        }

        public static void SafeSetActive(this Component component, bool isShow)
        {
            if (component != null && component.gameObject.activeSelf != isShow)
            {
                component.gameObject.SetActive(isShow);
            }
        }

        public static void SafeSetActive(this GameObject go, bool isShow)
        {
            if (go != null && go.activeSelf != isShow)
            {
                go.SetActive(isShow);
            }
        }
    }
}
