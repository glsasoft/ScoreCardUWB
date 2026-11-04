using LetsPiu.Define;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Customer
{
    public class CustomerCareer
    {
        ///<summary>
        ///总胜场数
        ///</summary>
        public int totalWinNum { get; set; }

        ///<summary>
        ///总死亡数
        ///</summary>
        public int totalDeathNum { get; set; }

        ///<summary>
        ///场均Kda
        /// </summary>
        public double kdRatio { get; set; }

        ///<summary>
        ///总击杀数
        ///</summary>
        public int totalKillNum { get; set; }

        ///<summary>
        ///场均评分
        /// </summary>
        public double avgScore { get; set; }

        ///<summary>
        ///评级
        ///</summary>
        public string grade { get; set; }

        ///<summary>
        ///总场次数
        ///</summary>
        public int totalPlayNum { get; set; }

        ///<summary>
        ///胜率
        /// </summary>
        public double winRate { get; set; }

        ///<summary>
        ///游戏模式
        /// </summary>
        public GameMode gameMode { get; set; }
    }
}