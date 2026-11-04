using System.Collections.Generic;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play
{
    public class KillRecordResponse
    {
        public List<KillRecord> killRecords;
        /// <summary>
        /// 击杀排名列表项
        /// </summary>
        public List<Team.Team> teamKills;
    }
}