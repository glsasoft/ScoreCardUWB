using Newtonsoft.Json;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 与 Java <c>AndroidMsgType</c> 的 <c>code</c> 一致，用于比分牌 / APK 侧 MQTT 外层 <c>msgType</c> 判别。
    /// </summary>
    public static class ScoreCardAndroidMqttMsgType
    {
        public const string Lora = "lora";
        public const string Voice = "voice";
        public const string ScoreCard = "scoreCard";
        /// <summary>下行：自动更新通知，<c>{"msg":{},"msgType":"updateVersion"}</c></summary>
        public const string UpdateVersion = "updateVersion";
        /// <summary>下行：查询版本号，<c>{"msgType":"queryVersion","msg":{}}</c></summary>
        public const string QueryVersion = "queryVersion";
        /// <summary>上行：上报版本号，<c>{"msgType":"version","msg":{"version":"1.0.0.24"}}</c></summary>
        public const string Version = "version";
    }

    /// <summary>
    /// <c>type=vol</c> 时 <c>voiceType</c>：
    /// 1 比分牌音量（Unity/HDMI，即安卓系统媒体音量）；
    /// 2 战场音乐音量（耳机口独立声卡，代码自行控制，独立调整当前播放中的战场背景音乐）；
    /// 3 战场播报音量（耳机口独立声卡，代码自行控制 NOTICE 播报音量，不影响 Unity 端播报音量）。
    /// </summary>
    public enum ScoreCardVoiceVolChannel
    {
        ScoreBoardAndroidHdmi = 1,
        BattlefieldBackgroundMp3 = 2,
        BattlefieldVoiceBroadcast = 3,
    }

    /// <summary>
    /// 与 Java 端一致：<c>isLoop</c> 在 <c>type=play</c> 时表示播放形态：0 暂停当前、1 循环、2 单次。
    /// </summary>
    public enum ScoreCardVoiceLoopMode
    {
        Pause = 0,
        Loop = 1,
        PlayOnce = 2,
    }

    /// <summary>
    /// 比分牌 MQTT 语音/背景音乐下行协议外壳。
    /// 示例：<c>{"msg":{"id":"1","isLoop":2,"type":"play","url":"https://...mp3"},"msgType":"voice"}</c>
    /// </summary>
    public class ScoreCardVoiceMqttEnvelope
    {
        [JsonProperty("msgType")]
        public string msgType;

        [JsonProperty("msg")]
        public ScoreCardVoiceCommandPayload msg;
    }

    /// <summary>
    /// <c>msg</c> 载荷：<c>type</c> 为 play / stop / vol 等；<c>isLoop</c> 仅 <c>play</c> 需要；<c>value</c>、<c>voiceType</c> 仅 <c>vol</c> 需要。
    /// </summary>
    public class ScoreCardVoiceCommandPayload
    {
        public string id;

        /// <summary>
        /// 与 Java 一致：0 暂停当前播放，1 循环播放，2 不循环（单次）。<c>type=vol</c> 时字段缺省，须为 <c>null</c>。
        /// </summary>
        [JsonProperty("isLoop")]
        public int? isLoop;

        /// <summary>
        /// 指令类型，如 play、stop、pause、resume、vol。
        /// </summary>
        [JsonProperty("type")]
        public string commandType;

        public string url;

        /// <summary><c>type=vol</c> 时音量档位；常见为 0~10，亦兼容 0~100。</summary>
        [JsonProperty("value")]
        public int? value;

        /// <summary><c>type=vol</c> 时声道/业务类型，见 <see cref="ScoreCardVoiceVolChannel"/>。</summary>
        [JsonProperty("voiceType")]
        public int? voiceType;

        /// <summary>
        /// 仅当 <see cref="isLoop"/> 为 0、1、2 时有值；缺省或其它值表示对 <c>play</c> 不可用。
        /// </summary>
        public ScoreCardVoiceLoopMode? LoopMode =>
            isLoop switch
            {
                null => null,
                0 => ScoreCardVoiceLoopMode.Pause,
                1 => ScoreCardVoiceLoopMode.Loop,
                2 => ScoreCardVoiceLoopMode.PlayOnce,
                _ => null
            };
    }
}
