using System;
using LetsPiu.Common.Network.Model.ScoreCard;
using UnityEngine;

namespace LetsPiu.UI.ScoreCardRuntime
{
    /// <summary>
    /// 各模式 Mock 共用的 MQTT 注入工具。信封结构与 ScoreCardUnity 的 ScoreCardSubCodeTester.BuildScoreCardJson 一致：
    /// msgType / msg{category, subCode, autoPlay, [serverTimestamp], data}。
    /// </summary>
    public static class ScoreCardMockMqtt
    {
        /// <param name="withServerTimestamp">与 ScoreCardUnity 一致，只有 S4 这类整场快照页带 serverTimestamp。</param>
        public static string BuildPage(string subCode, string dataJson, bool withServerTimestamp = false)
        {
            return Build(ScoreCardCommandCategory.Page, subCode, dataJson, withServerTimestamp);
        }

        public static string BuildEvent(string subCode, string dataJson)
        {
            return Build(ScoreCardCommandCategory.Event, subCode, dataJson, false);
        }

        /// <summary>NOTICE 是服务端给语音播报用的那一份，和同一事件的 EVENT 成对下发（NOTICE 在前）。</summary>
        public static string BuildNotice(string subCode, string dataJson)
        {
            return Build(ScoreCardCommandCategory.Notice, subCode, dataJson, false);
        }

        static string Build(string category, string subCode, string dataJson, bool withServerTimestamp)
        {
            string timestamp = withServerTimestamp
                ? "\"serverTimestamp\":" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ","
                : "";
            return "{\"msgType\":\"scoreCard\",\"msg\":{\"category\":\"" + category +
                   "\",\"subCode\":\"" + subCode + "\",\"autoPlay\":false," + timestamp +
                   "\"data\":" + dataJson + "}}";
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
