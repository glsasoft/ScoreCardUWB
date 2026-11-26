using LetsPiu.Common.Network.Model.ScoreCard;
using UnityEngine;

namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>各模式 Mock 共用的 MQTT 注入工具。</summary>
    public static class ScoreCardMockMqtt
    {
        public static string BuildPage(string subCode, string dataJson)
        {
            return "{\"msgType\":\"scoreCard\",\"msg\":{\"category\":\"" +
                   ScoreCardCommandCategory.Page + "\",\"subCode\":\"" + subCode +
                   "\",\"autoPlay\":false,\"data\":" + dataJson + "}}";
        }

        public static string BuildEvent(string subCode, string dataJson)
        {
            return "{\"msgType\":\"scoreCard\",\"msg\":{\"category\":\"" +
                   ScoreCardCommandCategory.Event + "\",\"subCode\":\"" + subCode +
                   "\",\"autoPlay\":false,\"data\":" + dataJson + "}}";
        }

        public static void Fire(string json, string logTag)
        {
            if (MqttConnector.Instance == null)
            {
                Debug.LogWarning($"[{logTag}] MqttConnector missing");
                return;
            }

            MqttConnector.Instance.SimulateReceiveScoreCardJson(json);
        }
    }
}
