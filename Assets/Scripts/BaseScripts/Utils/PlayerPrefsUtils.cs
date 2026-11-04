using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Utils
{
    public class PlayerPrefsUtils
    {
        public static T GetObject<T>(string key, T defaultValue = default)
        {
            var str = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            try
            {
                var o = JsonConvert.DeserializeObject<T>(str);
                return o;
            }
            catch (Exception e)
            {
                Debug.LogError("PlayerPrefsUtils GetObject:" + key + " " + e.Message);
                return defaultValue;
            }
        }

        public static bool SetObject<T>(string key, T o)
        {
            try
            {
                if (o == null)
                {
                    PlayerPrefs.DeleteKey(key);
                    return true;
                }
                var str = JsonConvert.SerializeObject(o);
                PlayerPrefs.SetString(key, str);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("PlayerPrefsUtils SetObject:" + key + " " + e.Message);
                return false;
            }
        }
    }
}