using System;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read
{
    /// <summary>
    /// 毒圈缩小消息
    /// </summary>
    public class PoisonCircle
    {
        /**
     * 场次id
     */
        public string playId;
        /**
         * 序列
         */
        public int circleIndex;
        /**
         * 经度
         */
        public double longitude;
        /**
         * 维度
         */
        public double latitude;
        /**
         * 半径（米）
         */
        public int radius;
        
        /// <summary>
        /// 下次缩圈时间
        /// </summary>
        public DateTime? nextShrinkTime;
    }
}