using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// Nooploop IPS 推送消息（见官方 API）。运行时只关心 name=Pos。
    /// </summary>
    [Serializable]
    public class UwbPosMessage
    {
        public string name;
        public string uid;
        public string deviceName;
        public UwbPosData data;
    }

    [Serializable]
    public class UwbPosData
    {
        public long cnt;
        public int mapId;
        public float noiseRadius;
        public float[] pos;
        public float[] vel;
        public float[] posVar;
        public float[] velVar;
        public float z;
        public long time;
        public string rxAnchorSn;
        public float rxRssi;
        public List<UwbFenceRef> fences;
        public List<float[]> optPoses;
        public List<float[]> optLngLats;
    }

    [Serializable]
    public class UwbFenceRef
    {
        public int id;
        public string name;
    }

    /// <summary>
    /// 解析后的统一位置事件（米，UWB 原始轴，尚未映射到场馆）。
    /// </summary>
    public struct UwbPosEvent
    {
        public string uid;
        public string deviceName;
        public float rawX;
        public float rawY;
        public float rawZ;
        public float velX;
        public float velY;
        public float velZ;
        public long deviceTimeMs;
        public double recvTimeSec;
        public string rawJson;
    }

    public enum UwbSourceMode
    {
        LiveMqtt = 0,
        LiveUdp = 1,
        Mock = 2,
        Replay = 3
    }

    public static class UwbPosParser
    {
        /// <summary>
        /// 支持官方裸 JSON，以及 piupiu-map 旧桥接外壳 type=14 + data 字符串。
        /// </summary>
        public static bool TryParse(string json, double recvTimeSec, out UwbPosEvent evt)
        {
            evt = default;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                JToken root = JToken.Parse(json);
                if (root is JObject obj)
                {
                    // 旧桥：{"type":14,"data":"{...}"}
                    if (obj.TryGetValue("type", out JToken typeTok) && typeTok.Type == JTokenType.Integer &&
                        typeTok.Value<int>() == 14 && obj.TryGetValue("data", out JToken dataTok))
                    {
                        string inner = dataTok.Type == JTokenType.String
                            ? dataTok.Value<string>()
                            : dataTok.ToString(Formatting.None);
                        return TryParseOfficial(inner, recvTimeSec, out evt);
                    }
                }

                return TryParseOfficial(json, recvTimeSec, out evt);
            }
            catch
            {
                return false;
            }
        }

        static bool TryParseOfficial(string json, double recvTimeSec, out UwbPosEvent evt)
        {
            evt = default;
            UwbPosMessage msg = JsonConvert.DeserializeObject<UwbPosMessage>(json);
            if (msg == null)
                return false;

            // 非 Pos 消息忽略（Call 等）
            if (!string.IsNullOrEmpty(msg.name) &&
                !string.Equals(msg.name, "Pos", StringComparison.OrdinalIgnoreCase))
                return false;

            float[] pos = msg.data != null ? msg.data.pos : null;
            if (pos == null || pos.Length < 2)
            {
                // 旧桥仅 sn+pos，可能没有 name 字段
                JObject obj = JObject.Parse(json);
                JToken data = obj["data"];
                if (data == null)
                    return false;
                string sn = data.Value<string>("sn");
                JArray p = data["pos"] as JArray;
                if (p == null || p.Count < 2)
                    return false;
                evt = new UwbPosEvent
                {
                    uid = sn ?? obj.Value<string>("uid") ?? string.Empty,
                    deviceName = obj.Value<string>("deviceName") ?? sn ?? string.Empty,
                    rawX = p[0].Value<float>(),
                    rawY = p[1].Value<float>(),
                    rawZ = p.Count > 2 ? p[2].Value<float>() : 0f,
                    deviceTimeMs = 0,
                    recvTimeSec = recvTimeSec,
                    rawJson = json
                };
                return !string.IsNullOrEmpty(evt.uid);
            }

            float[] vel = msg.data.vel;
            evt = new UwbPosEvent
            {
                uid = msg.uid ?? string.Empty,
                deviceName = string.IsNullOrEmpty(msg.deviceName) ? msg.uid : msg.deviceName,
                rawX = pos[0],
                rawY = pos[1],
                rawZ = pos.Length > 2 ? pos[2] : msg.data.z,
                velX = vel != null && vel.Length > 0 ? vel[0] : 0f,
                velY = vel != null && vel.Length > 1 ? vel[1] : 0f,
                velZ = vel != null && vel.Length > 2 ? vel[2] : 0f,
                deviceTimeMs = msg.data.time,
                recvTimeSec = recvTimeSec,
                rawJson = json
            };
            return !string.IsNullOrEmpty(evt.uid);
        }
    }
}
