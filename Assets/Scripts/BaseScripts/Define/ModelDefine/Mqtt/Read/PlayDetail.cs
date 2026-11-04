using System.Collections.Generic;
using JetBrains.Annotations;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team;
using LetsPiu.Common.Network.Model.Http.Play;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// Mqtt下发的场次变更后的消息，可能包含结算消息
    /// </summary>
    public class PlayDetail
    {
        public Play play;
        public List<Player> players;
        /// <summary>
        /// 排名好的队伍列表
        /// </summary>
        [CanBeNull] public List<Team> teams;
    }
}