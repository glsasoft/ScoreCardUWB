using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 打包后可改的运行时配置，存在 persistentDataPath/uwb_config.json。
    /// </summary>
    [Serializable]
    public class UwbConfigData
    {
        public bool enableUwb = true;
        public string sourceMode = "LiveMqtt";
        public string mqttBrokerUrl = "mqtt://192.168.0.15:1883";
        public string mqttTopic = "ips/pos";
        public string mqttClientId = "";
        public int udpListenPort = 9000;
        public string udpLocalAddress = "";
        public int mockTagCount = 20;
        public float mockAreaX = 18f;
        public float mockAreaY = 10f;
        public float mockMoveSpeed = 1.2f;
        public float mockIntervalSec = 0.1f;
        public string replayFilePath = "";
        public bool replayLoop = true;
        public float replaySpeed = 1f;
        /// <summary>回放时段起点（文件内 t 秒），&lt;0 表示文件头</summary>
        public float replayRangeStart = -1f;
        /// <summary>回放时段终点（文件内 t 秒），&lt;0 表示文件尾</summary>
        public float replayRangeEnd = -1f;
        public float offsetX;
        public float offsetY;
        public float offsetZ;
        public float scaleX = 1f;
        public float scaleY = 1f;
        public float scaleZ = 1f;
        public bool flattenHeight = true;
    }

    public static class UwbConfigFile
    {
        public const string FileName = "uwb_config.json";

        public static string Path =>
            System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static UwbConfigData LoadOrNull()
        {
            try
            {
                string path = Path;
                if (!File.Exists(path))
                    return null;
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return null;
                return JsonConvert.DeserializeObject<UwbConfigData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[UwbConfig] load fail: " + e.Message);
                return null;
            }
        }

        public static bool Save(UwbConfigData data)
        {
            if (data == null)
                return false;
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(Path, json);
                Debug.Log("[UwbConfig] saved " + Path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[UwbConfig] save fail: " + e.Message);
                return false;
            }
        }

        public static UwbSourceMode ParseMode(string name, UwbSourceMode fallback)
        {
            if (string.IsNullOrEmpty(name))
                return fallback;
            if (Enum.TryParse(name, true, out UwbSourceMode mode))
                return mode;
            return fallback;
        }
    }
}
