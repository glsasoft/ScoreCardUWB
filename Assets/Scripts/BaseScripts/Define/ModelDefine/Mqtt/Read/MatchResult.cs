using System;
using System.Data;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 匹配结果
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// matchId
        /// </summary>
        public string matchId;

        /// <summary>
        /// 匹配成功后开始时间
        /// </summary>
        public DateTime matchedBeginTime;

        /// <summary>
        /// 匹配成功后持续时间
        /// </summary>
        public int matchedDuration;

        /// <summary>
        /// 匹配成功后总人数
        /// </summary>
        public int matchPeoNum;

        /// <summary>
        /// 当前游戏模式
        /// </summary>
        public GameMode type;

        /// <summary>
        /// 结果
        /// </summary>
        public MatchAcceptStatus result;

        public MatchStatus status;
    }
}