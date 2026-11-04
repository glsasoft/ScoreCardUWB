using System;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 收到的语音消息
    /// </summary>
    public class VoiceReadMessage
    {
        public string groupId;
        public string playId;
        public string roomId;
        public int seconds;
        public int shortId;
        public int teamId;
        public int type;
        public string voiceUrl;
        public DateTime receiveTime = DateTime.Now;
    }
}