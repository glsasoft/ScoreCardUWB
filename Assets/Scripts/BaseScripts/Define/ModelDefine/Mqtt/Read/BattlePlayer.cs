using System;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    public class BattlePlayer
    {
        /**
         * 用户场次短编码
         */
        private int shortId;
        /**
         * 初始血
         */
        private int initBlood;
        /**
         * 当前血
         */
        private int blood;
        /**
         * 治疗量
         */
        private long treat;
        /**
         * 击中次数
         */
        private int hitNumber;
        /**
         * 击杀数
         */
        private int killNumber;
        /**
         * 连杀数
         */
        private int continuousKillNumber;

        /**
         * 死亡数
         */
        private int deathNumber;

        /**
         * 助攻数
         */
        private int assistNumber;
        /**
         * 死亡时间
         */
        private DateTime deadTime;
        /**
         * 护甲
         */
        private long armor;
        /**
         * 烟雾弹数量
         */
        private int smokeCount;
        /**
         * 闪光弹数量
         */
        private int flashbangCount;
        /**
         * 手雷数量
         */
        private int grenadeCount;
        /**
         * 经济
         */
        private int sp;
    }
}