using LetsPiu.Common.Network.Model.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    public class MqttReadMessage<T>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MqttMessageType type;
        public T data;

        public MqttReadMessage(MqttMessageType type, T data)
        {
            this.type = type;
            this.data = data;
        }
    }
}