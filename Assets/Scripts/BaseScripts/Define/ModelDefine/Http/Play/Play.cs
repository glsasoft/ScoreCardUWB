using System;
using System.Collections.Generic;
using LetsPiu.Define;
using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Play
{
    public class Play : BaseEntity
    {
        /// <summary>
        /// 房间号
        /// </summary>
        [JsonProperty("roomId")]
        public string roomId { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        /// <summary>
        /// 房主ID
        /// </summary>
        [JsonProperty("creatorId")]
        public string creatorId { get; set; }

        /// <summary>
        /// 战斗类型
        /// </summary>
        [JsonProperty("type")]
        public GameMode type { get; set; }

        /// <summary>
        /// 玩家总数
        /// </summary>
        [JsonProperty("playerNumber")]
        public int? playerNumber { get; set; }

        /// <summary>
        /// 队伍数
        /// </summary>
        [JsonProperty("teamNumber")]
        public int? teamNumber { get; set; }

        /// <summary>
        /// 队伍人数比
        /// </summary>
        [JsonProperty("teamPlayerRatio")]
        public string teamPlayerRatio { get; set; }

        /// <summary>
        /// 总击杀数
        /// </summary>
        [JsonProperty("totalKillNumber")]
        public int? totalKillNumber { get; set; }

        /// <summary>
        /// 总击中数
        /// </summary>
        [JsonProperty("totalHitNumber")]
        public int? totalHitNumber { get; set; }

        /// <summary>
        /// 总击倒数
        /// </summary>
        [JsonProperty("totalPoleaxeNumber")]
        public int? totalPoleaxeNumber { get; set; }

        /// <summary>
        /// 总救援数
        /// </summary>
        [JsonProperty("totalRescueNumber")]
        public int? totalRescueNumber { get; set; }

        /// <summary>
        /// 枪功率类型（1：小功率 2：大功率）
        /// </summary>
        [JsonProperty("gunShutType")]
        public GunPowerType? gunShutType { get; set; }

        /// <summary>
        /// 毒圈半径
        /// </summary>
        [JsonProperty("poisonCircleRadius")]
        public int? poisonCircleRadius { get; set; }

        /// <summary>
        /// 毒圈模式 0:关闭 1:随机生成 2:指定坐标
        /// </summary>
        [JsonProperty("poisonCircleMode")]
        public PoisonMode? poisonCircleMode { get; set; }

        /// <summary>
        /// 毒圈经度
        /// </summary>
        [JsonProperty("poisonCircleLongitude")]
        public double? poisonCircleLongitude { get; set; }

        /// <summary>
        /// 毒圈维度
        /// </summary>
        [JsonProperty("poisonCircleLatitude")]
        public double? poisonCircleLatitude { get; set; }

        /// <summary>
        /// 栅栏半径（米）
        /// </summary>
        [JsonProperty("fenceRadius")]
        public int? fenceRadius { get; set; }

        /// <summary>
        /// 栅栏经度
        /// </summary>
        [JsonProperty("fenceLongitude")]
        public double? fenceLongitude { get; set; }

        /// <summary>
        /// 栅栏维度
        /// </summary>
        [JsonProperty("fenceLatitude")]
        public double? fenceLatitude { get; set; }

        /// <summary>
        /// loading beginTime(点击开始游戏时间)
        /// </summary>
        public DateTime loadingBeginTime;

        /// <summary>
        /// 竞拍开始时间
        /// </summary>
        public DateTime? auctionBeginTime;

        /// <summary>
        /// 竞拍时长(s)
        /// </summary>
        public int? auctionDuration;

        /// <summary>
        /// 投点开始时间
        /// </summary>
        public DateTime? dropPointBeginTime { get; set; }

        /// <summary>
        /// 投点时长
        /// </summary>
        public int? dropPointDuration { get; set; }

        /// <summary>
        /// 购物开始时间
        /// </summary>
        public DateTime? preparationBeginTime;
        /// <summary>
        /// shopping阶段时长(秒)
        /// </summary>
        public int? preparationDuration;

        /// <summary>
        /// 比赛开始时间(5秒倒计时结束的时间)
        /// </summary>
        public DateTime? beginTime;

        /// <summary>
        ///  比赛时长分钟(单回合)
        /// </summary>
        public int matchDuration;

        /// <summary>
        /// 比赛结束时间(最晚)
        /// </summary>
        public DateTime? endTime;

        /// <summary>
        /// 场次状态
        /// </summary>
        [JsonProperty("playStatus")]
        public GameState playStatus { get; set; }

        /// <summary>
        /// 0：平局 1：1队胜 2：2队胜
        /// </summary>
        [JsonProperty("result")]
        public int? result { get; set; }

        /// <summary>
        /// mqtt服务器id
        /// </summary>
        [JsonProperty("mqttServerId")]
        public long? mqttServerId { get; set; }

        /// <summary>
        /// 初始默认子弹数
        /// </summary>
        [JsonProperty("initBulletSum")]
        public int? initBulletSum { get; set; }

        /// <summary>
        /// 是否舔包（0：否 1：是）
        /// </summary>
        [JsonProperty("isAddBullet")]
        public int? isAddBullet { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        [JsonProperty("province")]
        public string province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        [JsonProperty("city")]
        public string city { get; set; }

        /// <summary>
        /// 区
        /// </summary>
        [JsonProperty("district")]
        public string district { get; set; }

        /// <summary>
        /// 栅栏类型 0圆形/1多边形
        /// </summary>
        [JsonProperty("fenceType")]
        public FenceType? fenceType { get; set; }

        /// <summary>
        /// 电子栅栏开关 0关闭/1开启
        /// </summary>
        [JsonProperty("fenceEnabled")]
        public FenceEnabled fenceEnabled { get; set; }
        /// <summary>
        /// 房间人数上限
        /// </summary>
        [JsonProperty("maxPlayers")]
        public int? maxPlayers { get; set; }

        /// <summary>
        /// 目标击杀数
        /// </summary>
        [JsonProperty("winKillCount", NullValueHandling = NullValueHandling.Ignore)]
        public double? WinKillCount { get; set; }

        /// <summary>
        /// 获胜回合数
        /// </summary>
        [JsonProperty("winRoundCount")]
        public int? winRoundCount { get; set; }

        /// <summary>
        /// 备弹限制开关
        /// </summary>
        [JsonProperty("ammoLimitEnabled")]
        public int? ammoLimitEnabled { get; set; }

        /// <summary>
        /// 弹匣数量
        /// </summary>
        [JsonProperty("magazineCount")]
        public int? magazineCount { get; set; }

        /// <summary>
        /// 基础弹容倍率
        /// </summary>
        [JsonProperty("baseAmmoMultiplier")]
        public double? baseAmmoMultiplier { get; set; }

        /// <summary>
        /// 伤害倍率
        /// </summary>
        [JsonProperty("harmMultiplier")]
        public double? harmMultiplier { get; set; }

        /// <summary>
        /// 武器平衡开关 0关闭/1开启
        /// </summary>
        [JsonProperty("weaponBalanceEnabled")]
        public int? weaponBalanceEnabled { get; set; }

        /// <summary>
        /// 弹夹换弹开关 0关闭/1开启
        /// </summary>
        [JsonProperty("magazineReloadEnabled")]
        public int? magazineReloadEnabled { get; set; }

        /// <summary>
        /// 是否仅爆头有效 0关闭/1开启
        /// </summary>
        [JsonProperty("headshotOnlyEnabled")]
        public int? headshotOnlyEnabled { get; set; }

        /// <summary>
        /// 技能冷却倍率
        /// </summary>
        [JsonProperty("skillCooldownMultiplier")]
        public double? skillCooldownMultiplier { get; set; }

        /// <summary>
        /// 大招能量倍率
        /// </summary>
        [JsonProperty("ultimateEnergyMultiplier")]
        public double? ultimateEnergyMultiplier { get; set; }

        /// <summary>
        /// 开局满能量开关 0关闭/1开启
        /// </summary>
        [JsonProperty("fullEnergyEnabled")]
        public int? fullEnergyEnabled { get; set; }

        /// <summary>
        /// 充能速度倍率
        /// </summary>
        [JsonProperty("nodeChargeMultiplier")]
        public double? nodeChargeMultiplier { get; set; }

        /// <summary>
        /// 生命值倍率
        /// </summary>
        [JsonProperty("healthMultiplier")]
        public double? healthMultiplier { get; set; }

        /// <summary>
        /// 武器基础伤害值
        /// </summary>
        [JsonProperty("weaponDamage")]
        public int? weaponDamage { get; set; }

        /// <summary>
        /// 爆头伤害倍率
        /// </summary>
        [JsonProperty("headDamageMultiplier")]
        public double? headDamageMultiplier { get; set; }

        /// <summary>
        /// 躯干伤害倍率
        /// </summary>
        [JsonProperty("torsoDamageMultiplier")]
        public double? torsoDamageMultiplier { get; set; }

        /// <summary>
        /// 武器伤害倍率
        /// </summary>
        [JsonProperty("weaponDamageMultiplier")]
        public double? weaponDamageMultiplier { get; set; }

        /// <summary>
        /// mqtt地址
        /// </summary>
        public string netAddress { get; set; }

        /// <summary>
        /// 复活类型
        /// </summary>
        public RevivalType? reviveType{ get; set; }
        /// <summary>
        /// 复活时间(秒) 【只有原地复活有这个值】
        /// </summary>
        public int? reviveDuration{ get; set; }
    }
}
