using JetBrains.Annotations;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team
{
    public class Team
    {
        public string id;
        public int teamId;
        public string roomId;
        public string playId;
        public string name;
        public int teamLeaderId;
        public string teamIndex;
        /// <summary>
        /// 总杀人数
        /// </summary>
        public int? totalKills;
        /// <summary>
        /// 队伍头像
        /// </summary>
        [CanBeNull] public string icon;
    }
}