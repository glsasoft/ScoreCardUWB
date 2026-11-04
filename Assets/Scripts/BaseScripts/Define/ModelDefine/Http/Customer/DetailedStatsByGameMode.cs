using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Customer
{
    public class DetailedStatsByGameMode
    {

        ///<summary>
        ///场均评分
        /// </summary>
        public double avgScore { get; set; }

        ///<summary>
        ///评级
        ///</summary>
        public string grade { get; set; }

        ///<summary>
        ///MVP次数
        /// </summary>
        public int totalMvpNum { get; set; }

        /// <summary>
        /// 总伤害
        /// </summary>
        public int totalStrikeNum { get; set; }

        /// <summary>
        /// 平均死亡数
        /// </summary>
        public float avgDeath { get; set; }

        /// <summary>
        /// 总死亡数
        /// </summary>
        public int totalDeathNum { get; set; }

        /// <summary>
        /// 总击杀数
        /// </summary>
        public int totalKillNum { get; set; }

        /// <summary>
        /// 总助攻数
        /// </summary>
        public int totalAssistNum { get; set; }

        /// <summary>
        /// 总参与场次
        /// </summary>
        public int totalPlayNum { get; set; }

        /// <summary>
        /// 平均助攻数
        /// </summary>
        public float avgAssist { get; set; }

        /// <summary>
        /// 命中率（现改为命中次数）
        /// </summary>
        public int hitRate { get; set; }

        /// <summary>
        /// 总能量点数
        /// </summary>
        public int totalEnergyPointNum { get; set; }

        /// <summary>
        /// 总胜场数
        /// </summary>
        public int totalWinNum { get; set; }

        /// <summary>
        /// 总承伤
        /// </summary>
        public int totalStrikenNum { get; set; }

        /// <summary>
        /// 总终极技能释放数
        /// </summary>
        public int totalUltimateSkillNum { get; set; }

        /// <summary>
        /// 总技能使用数
        /// </summary>
        public int totalUseSkillNum { get; set; }

        /// <summary>
        /// 百分位排名
        /// </summary>
        public float percentileRank { get; set; }

        /// <summary>
        /// 总射击子弹数
        /// </summary>
        public int totalShootBulletNum { get; set; }

        /// <summary>
        /// 平均击杀数
        /// </summary>
        public float avgKill { get; set; }

        /// <summary>
        /// 角色统计列表
        /// </summary>
        public List<RoleStats> roleStats { get; set; }

        /// <summary>
        /// 总治疗数
        /// </summary>
        public int totalTreatNum { get; set; }

        /// <summary>
        /// 平均胜率
        /// </summary>
        public float avgWinRate { get; set; }

        /// <summary>
        /// 总购买卡牌数
        /// </summary>
        public int totalCardPurchasedNum { get; set; }

        /// <summary>
        /// 总命中数
        /// </summary>
        public int totalHitNum { get; set; }
    }

  
}
