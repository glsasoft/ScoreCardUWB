using System;
using System.Collections.Generic;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Team;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{

    public class PlayerSp
    {
        public int shortId;
        public int sp;
    }
    
    /// <summary>
    /// [服务器下发]击杀消息
    /// Topic: /glsa/player/{房间 ID}/{玩家短编号}
    /// </summary>
    public class KillMessage
    {
        public KillRecord killRecord;
        /// <summary>
        /// 击杀排名列表项
        /// </summary>
        public List<Team> teamKills;
        /// <summary>
        /// 玩家sp更新数据
        /// </summary>
        public List<PlayerSp> playerSps;
    }
}