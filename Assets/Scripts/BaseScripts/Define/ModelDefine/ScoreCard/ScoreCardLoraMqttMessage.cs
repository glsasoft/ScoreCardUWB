using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌 MQTT LORA 下行协议外壳（服务器→设备）。
    /// 示例：<c>{"msgType":"lora","msg":{"value":"5A3D0804006400000000A5"}}</c>
    /// </summary>
    public class ScoreCardLoraMqttEnvelope
    {
        [JsonProperty("msgType")]
        public string msgType;

        [JsonProperty("msg")]
        public ScoreCardLoraCommandPayload msg;
    }

    /// <summary>
    /// LORA 下行透传载荷：HEX 字符串（如 5A3D0804006400000000A5，长度 11~14 字节可变）。
    /// </summary>
    public class ScoreCardLoraCommandPayload
    {
        [JsonProperty("value")]
        public string value;
    }

    /// <summary>
    /// 比分牌 MQTT LORA 上行协议消息体（设备→服务器）。
    /// 示例：<c>{"type":"lora","value":"5A0282000864500101A5"}</c>
    /// </summary>
    public class ScoreCardLoraUplinkMessage
    {
        [JsonProperty("type")]
        public string type = "lora";

        [JsonProperty("value")]
        public string value;

        public ScoreCardLoraUplinkMessage(string value)
        {
            this.value = value;
        }
    }
}
