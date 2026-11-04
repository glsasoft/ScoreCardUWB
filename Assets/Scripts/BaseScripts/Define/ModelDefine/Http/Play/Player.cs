using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LetsPiu.Base.BaseScripts.Define.ModelDefine;
using LetsPiu.Define;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LetsPiu.Common.Network.Model.Http.Play
{
    public class Player : BaseEntity
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string customerId;

        /// <summary>
        /// 小组id
        /// </summary>
        public string groupId;

        /// <summary>
        /// 房间号
        /// </summary>
        public string roomId;

        /// <summary>
        /// 场次id
        /// </summary>
        public string playId;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime gmtCreate;

        /// <summary>
        /// 游戏模式
        /// </summary>
        public GameMode? gamemode;


        /// <summary>
        /// 回合id
        /// </summary>
        public string roundId;

        /// <summary>
        /// 用户场次短编码
        /// </summary>
        public int shortId;

        /// <summary>
        /// 队伍编号
        /// </summary>
        public int team;

        /// <summary>
        /// 队伍序列
        /// 队伍内部排序
        /// </summary>
        public int teamIndex;

        /// <summary>
        /// 角色
        /// </summary>
        public string professionId;
        
        /// <summary>
        /// 皮肤id
        /// </summary>
        public string skinId;

        /// <summary>
        /// 裤子
        /// </summary>
        public string pantsCode;

        /// <summary>
        /// 盔甲
        /// </summary>
        public string jacketCode;

        /// <summary>
        /// 枪
        /// </summary>
        public string gunCode;

        /// <summary>
        /// 头盔
        /// </summary>
        public string headCode;

        // /// <summary>
        // /// 击杀人id
        // /// </summary>
        // public string killerId;
        //
        // /// <summary>
        // /// 被击杀方式，0：普通击杀 1：第一滴血 2：终结 3：装备掉线 4:毒圈
        // /// </summary>
        // public AttackType killType;

        /// <summary>
        /// 射速
        /// </summary>
        public int shootSpeed;
        
        /// <summary>
        /// 技能时的射速
        /// </summary>
        public int? skillShootSpeed;

        /// <summary>
        /// 爆头伤害
        /// </summary>
        public int headHarm;

        /// <summary>
        /// 伤害值(固定值)
        /// </summary>
        public int harm;

        /// <summary>
        /// 初始血
        /// </summary>
        public int initBlood;

        /// <summary>
        /// 当前血
        /// </summary>
        public int blood;

        /// <summary>
        /// 初始子弹
        /// </summary>
        public int? initBullet;
        
        /// <summary>
        /// 当前子弹(弹夹内)
        /// </summary>
        public int bullet;

        /// <summary>
        /// 弹夹容量
        /// </summary>
        public int bulletVolume;

        /// <summary>
        /// 背包内剩余子弹
        /// </summary>
        public int bulletBackpack;

        /// <summary>
        /// 舔包子弹数
        /// </summary>
        public int addBullet;

        /// <summary>
        /// 状态 0：离线 1：等待席 2：未准备 3：已准备 4：强制准备 5：匹配中 6：loading 7：经济模式 8：对战中
        /// </summary>
        public PlayerState status;

        /// <summary>
        /// 观战状态 0：未观战 1：观战
        /// </summary>
        public WatchFlag? watchFlg;

        /// <summary>
        /// 经验值
        /// </summary>
        public string exp;

        /// <summary>
        /// 场次评分
        /// </summary>
        public int playScore;

        /// <summary>
        /// 评分等级
        /// </summary>
        public PlayScoreLevel playScoreLevel;

        /// <summary>
        /// 胜利积分
        /// </summary>
        public string winScore;

        /// <summary>
        /// KDA积分(击杀、死亡、助攻 计算出来的值)
        /// </summary>
        public int kdaScore;

        /// <summary>
        /// 治疗量
        /// </summary>
        public int treat;

        /// <summary>
        /// 鸡毛数
        /// </summary>
        public string chickenFeather;

        /// <summary>
        /// 战斗力
        /// </summary>
        public string fightScore;

        /// <summary>
        /// 总伤害
        /// </summary>
        public int strikeScore;

        /// <summary>
        /// 抗伤
        /// </summary>
        public int strikenScore;

        /// <summary>
        /// 救援量
        /// </summary>
        public int rescueScore;

        /// <summary>
        /// 击中次数
        /// </summary>
        public int hitNumber;

        /// <summary>
        /// 击杀数
        /// </summary>
        public int killNumber;

        /// <summary>
        /// 连杀数
        /// </summary>
        public int continuousKillNumber;

        /// <summary>
        /// 命中率
        /// </summary>
        public double shootingPercent;

        /// <summary>
        /// 死亡数
        /// </summary>
        public int deathNumber;

        /// <summary>
        /// 助攻数
        /// </summary>
        public int assistNumber;

        /// <summary>
        /// 击倒数
        /// </summary>
        public int poleaxeNumber;

        /// <summary>
        /// 被击倒数
        /// </summary>
        public int bePoleaxeNumber;

        /// <summary>
        /// 救援数
        /// </summary>
        public int rescueNumber;

        /// <summary>
        /// 被救援数
        /// </summary>
        public int beRescueNumber;

        /// <summary>
        /// 死亡时间
        /// </summary>
        public DateTime? deadTime;

        /// <summary>
        /// mvp状态（0：无 1：mvp 2：svp）
        /// </summary>
        public int mvpStatus;

        /// <summary>
        /// 场次排名
        /// </summary>
        public int rank;

        /// <summary>
        /// 0：平局 1：胜 2：败
        /// </summary>
        public GameResult? result;

        /// <summary>
        /// 离线时间
        /// </summary>
        public DateTime? outlineTime;

        /// <summary>
        /// 备注
        /// </summary>
        public string remarks;

        public string back1;

        public string back2;

        public string back3;

        public string back4;

        /// <summary>
        /// 躯干伤害
        /// </summary>
        public int torsoHarm;

        /// <summary>
        /// 子弹分布 (90%)
        /// </summary>
        public double bulletSpread;

        /// <summary>
        /// 举镜子弹分布
        /// </summary>
        public double adsBulletSpread;

        /// <summary>
        /// 瞄具放大倍率
        /// </summary>
        public double sightMagnification;

        /// <summary>
        /// 换弹速度
        /// </summary>
        public double reloadSpeedMultiplier;

        /// <summary>
        /// 护甲类型
        /// </summary>
        public TacticalType armor;

        /// <summary>
        /// 烟雾弹数量
        /// </summary>
        public int smokeCount;

        /// <summary>
        /// 闪光弹数量
        /// </summary>
        public int flashbangCount;

        /// <summary>
        /// 手雷数量
        /// </summary>
        public int grenadeCount;

        /// <summary>
        /// 连胜/连败回合数
        /// </summary>
        public int streakRoundCount;

        /// <summary>
        /// 技能命中数
        /// </summary>
        public int skillHits;

        /// <summary>
        /// 是否仅爆头有效（0未开启，1开启）
        /// </summary>
        public int isHeadshotOnly;

        /// <summary>
        /// 技能冷却时间倍率(100%)
        /// </summary>
        public double skillCooldownMultiplier;

        /// <summary>
        /// 终极技能能量槽倍率(100%)
        /// </summary>
        public double ultimateEnergyMultiplier;

        /// <summary>
        /// 连胜/连败标识 （0：默认开局 1：连胜 2：连败）
        /// </summary>
        public int? streakRoundFlag;

        /// <summary>
        /// 重生/开局满量能是否开启 （0：关闭  1：开启）
        /// </summary>
        public int fullEnergyFlag;

        /// <summary>
        /// 节点充能速度倍率
        /// </summary>
        public float nodeChargeMultiplier;

        /// <summary>
        /// 经济
        /// </summary>
        public int? sp;

        /// <summary>
        /// 总经济
        /// </summary>
        public int? totalSp;

        /// <summary>
        /// 生命值倍率
        /// </summary>
        public double healthMultiplier;

        /// <summary>
        /// 武器基础伤害值
        /// </summary>
        public int weaponDamage;

        /// <summary>
        /// 爆头伤害倍率
        /// </summary>
        public double headDamageMultiplier;

        /// <summary>
        /// 躯干伤害倍率
        /// </summary>
        public double torsoDamageMultiplier;

        /// <summary>
        /// 武器伤害倍率
        /// </summary>
        public double weaponDamageMultiplier;

        public SubDeviceType? gunVersion;
        public SubDeviceType? headVersion;
        
        public int sex;
        public string name;
        public string imageUrl;
        public int? level;

        /// <summary>
        /// 玩家当前护甲值
        /// </summary>
        public int? playerArmorValue;
        ///<summary>
        /// 玩家当前护盾值
        /// </summary>
        public int? playerShieldsValue;
        /// <summary>
        /// 玩家护盾基础值
        /// </summary>
        public int? shieldsValue;
        /// <summary>
        /// 玩家护甲基础值
        /// </summary>
        public int? armorValue;
        /// <summary>
        /// 终极技能能量条点数(用于游戏下方面板中显示当前能量条)
        /// </summary>
        public float? ultimateEnergy;
        /// <summary>
        /// 对战中状态 0：死亡，1：存活 2：
        /// </summary>
        public PlayerBattleStatus? battleStatus;
        /// <summary>
        /// 护甲减伤倍率
        /// </summary>
        public double? armorReduceHarm;
        /// <summary>
        /// 爆炸伤害倍率
        /// </summary>
        public double? blowUpHarm;
        /// <summary>
        /// 治疗倍率
        /// </summary>
        public float? treatmentMultiplier;
        /// <summary>
        /// 秒数(用于接收)
        /// </summary>
        public int? duration;
        /// <summary>
        /// 投掷范围增加的米数(被动投手类卡牌生效后增加)
        /// </summary>
        public float? throwingRange;
        /// <summary>
        /// 范围
        /// </summary>
        public int? profitRange;
        /// <summary>
        /// deBuff倍率
        /// </summary>
        public float? deBuffMultiplier;

        /// <summary>
        /// 复活时间
        /// </summary>
        public DateTime? reviveTime;
        
        /// <summary>
        /// 无敌结束时间
        /// </summary>
        private DateTime? invincibleEndTime;
        
        /// <summary>
        /// 维度
        /// </summary>
        public double? latitude;
        /// <summary>
        /// 经度
        /// </summary>
        public double? longitude;
        /// <summary>
        /// 开镜时间
        /// </summary>
        public float? openSightMultiplier;
        /// <summary>
        /// 是否在栅栏里 0：false  1：true
        /// </summary>
        public FenceStatus? outFence;

        /// <summary>
        /// 小技能冷却结束时间
        /// </summary>
        public DateTime? coolDownEndTime;

        /// <summary>
        /// 玩家旋转角度(从服务器同步)
        /// </summary>
        [JsonIgnore]
        public float? degree = null;

        /// <summary>
        /// 所属玩家Id
        /// 非空标识为机器人
        /// </summary>
        public string ownerId { get; set; }

        /// <summary>
        /// 小技能持续结束时间
        /// </summary>
        public DateTime? skillEndTime;
        /// <summary>
        ///  终极技能持续结束时间
        /// </summary>
        public DateTime? ultimateSkillEndTime;

        /// <summary>
        /// 预选角色
        /// </summary>
        [JsonIgnore] public string beforehandProfessionId;

        /// <summary>
        /// 经验值奖励
        /// </summary>
        public int? expGain;
        
        /// <summary>
        /// 金币奖励
        /// </summary>
        public int? goldCoin;

        /// <summary>
        /// 初始奖杯数(玩家玩游戏之前的奖杯经验值)
        /// </summary>
        public int? cupNum;
        
        /// <summary>
        /// 初始奖杯level(玩家玩游戏之前的等级)
        /// </summary>
        public int? cupLevel;
        
        /// <summary>
        /// 奖杯数奖励(玩家本局游戏过后奖励的奖杯经验值)
        /// </summary>
        public int? cupGain;

        /// <summary>
        /// 初始职业熟练度
        /// </summary>
        public int? professionProficiency;
        
        /// <summary>
        /// 初始职业熟练度level
        /// </summary>
        public int? professionProficiencyLevel;
        
        /// <summary>
        /// 职业熟练度奖励
        /// </summary>
        public int? professionProficiencyGain;

        /// <summary>
        /// 初始武器熟练度
        /// </summary>
        public int? weaponProficiency;
        
        /// <summary>
        /// 初始武器熟练度level
        /// </summary>
        public int? weaponProficiencyLevel;
        
        /// <summary>
        /// 武器熟练度奖励
        /// </summary>
        public int? weaponProficiencyGain;

        /// <summary>
        /// 枪的颜色
        /// </summary>
        public DeviceColor gunColor;
        
        public static Player ParsePlayer(JToken jToken)
        {
            Player player = JsonConvert.DeserializeObject<Player>(jToken.ToString());
            return player;
        }
        
        public bool IsInRoom()
        {
            return roomId != null && !roomId.Equals("0");
        }

        public bool IsInGroup()
        {
            return groupId != null && !groupId.Equals("0");
        }
    }
}
