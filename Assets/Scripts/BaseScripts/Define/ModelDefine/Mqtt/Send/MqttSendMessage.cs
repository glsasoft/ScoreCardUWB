using LetsPiu.Common.Network.Model.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Send
{
    public  class MqttSendMessage<T>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MqttMessageType type;
        public T payload;
        
        public MqttSendMessage(MqttMessageType type, T payload)
        {
            this.type = type;
            this.payload = payload;
        }
    }
}
