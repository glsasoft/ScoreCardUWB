using System;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 开始匹配了
    /// </summary>
    public class StartMatch
    {
        /// <summary>
        /// 匹配时长秒
        /// </summary>
        public int matchDuration;
        /// <summary>
        /// 开始匹配的时间点
        /// </summary>
        public DateTime startMatchTime;
        
        /// <summary>
        /// 状态
        /// </summary>
        public MatchStatus status;
    }
}