using System.Collections.Generic;
using JetBrains.Annotations;
using LetsPiu.Common.Network.Model.Http.Play;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play
{
    /// <summary>
    /// 获取场次返回数据
    /// </summary>
    public class QueryPlayResponse
    {
        public Play play;
        public List<Player> players;
        /// <summary>
        /// 排名好的队伍列表
        /// </summary>
        [CanBeNull] public List<Team.Team> teams;
    }
}