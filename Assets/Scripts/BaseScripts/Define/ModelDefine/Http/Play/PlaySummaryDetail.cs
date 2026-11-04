using System.Collections.Generic;
using JetBrains.Annotations;
using LetsPiu.Common.Network.Model.Http.Play;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play
{
    
    public class PlayRoundDetail
    {
        public List<Player> players;
    }
    
    /// <summary>
    /// HTTP获取的结算数据
    /// </summary>
    public class PlaySummaryDetail
    {
        public Play play;
        
        /// <summary>
        /// 全部玩家列表的结算数据(整个场次)
        /// </summary>
        public List<Player> players;
        
        /// <summary>
        /// 排名好的队伍列表
        /// </summary>
        public List<Team.Team> teams;
        
        /// <summary>
        /// 每个轮次的结算数据(只有回合模式才有)
        /// </summary>
        [CanBeNull] public List<PlayRoundDetail> roundDetails;
    }
}