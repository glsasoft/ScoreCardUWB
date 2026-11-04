using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Define
{
    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// 备战中
        /// </summary>
        PREPARING = 0,
        /// <summary>
        /// 加载中
        /// </summary>
        LOADING = 1,
        /// <summary>
        /// 竞拍中
        /// </summary>
        BINDDING = 2,
        /// <summary>
        /// 投点中
        /// </summary>
        DROPPING = 3,
        /// <summary>
        /// 购物中
        /// </summary>
        SHOPPING = 4,
        /// <summary>
        /// 战斗中
        /// </summary>
        FIGHTING = 5,
        /// <summary>
        /// 休息中
        /// </summary>
        INTERMISSION = 6,
        /// <summary>
        /// 结束
        /// </summary>
        END = 7,
        /// <summary>
        /// 房间锁定(开始设置最终安全区 和 复活点列表)
        /// </summary>
        LOCK = 9
    }

    /// <summary>
    /// 栅栏状态 状态 1：离开栅栏范围（开启死亡倒计时） 0：进入栅栏范围（结束倒计时）
    /// </summary>
    public enum FenceStatus
    {
        /// <summary>
        /// 进入栅栏范围（结束倒计时）
        /// </summary>
        Inside = 0,
        /// <summary>
        /// 离开栅栏范围（开启死亡倒计时）
        /// </summary>
        Outside = 1,
    }

    /// <summary>
    /// 玩家的游戏中状态
    /// </summary>
    public enum PlayerBattleStatus
    {
        /// <summary>
        /// 死亡
        /// </summary>
        Dead = 0,
        /// <summary>
        /// 存活
        /// </summary>
        Alive = 1,
    }

    /// <summary>
    /// 玩家阶段性状态
    /// </summary>
    public enum PlayerState
    {
        /// <summary>
        /// 离线
        /// </summary>
        OFFLINE = 0,
        /// <summary>
        /// 等待席
        /// </summary>
        WAITING_SEAT = 1,
        /// <summary>
        /// 未准备
        /// </summary>
        NOT_READY = 2,
        /// <summary>
        /// 已准备
        /// </summary>
        READY = 3,
        /// <summary>
        /// 强制准备
        /// </summary>
        FORCE_READY = 4,
        /// <summary>
        /// 匹配中
        /// </summary>
        MATCHING = 5,
        /// <summary>
        /// loading
        /// </summary>
        LOADING = 6,
        /// <summary>
        /// 竞拍中
        /// </summary>
        BIDDING = 7,
        /// <summary>
        /// 投点中
        /// </summary>
        DROPPING = 8,
        /// <summary>
        /// 经济模式
        /// </summary>
        ECONOMY_MODE = 9,
        /// <summary>
        /// 对战中
        /// </summary>
        IN_BATTLE = 10,
        /// <summary>
        /// 休息中
        /// </summary>
        INTERMISSION = 11,
        /// <summary>
        /// 结束
        /// </summary>
        END = 12,
    }

    /// <summary>
    /// 特效状态
    /// </summary>
    public enum EffectStatus
    {
        /// <summary>
        /// 停止
        /// </summary>
        Stop = 0,
        /// <summary>
        /// 播放
        /// </summary>
        Play = 1,
    }

    /// <summary>
    /// 游戏模式
    /// </summary>
    public enum GameMode
    {
        DEFAULT = 0,            // 默认模式，通常用于UI界面显示
        /// <summary>
        /// 新手模式
        /// </summary>
        BEGINNER_MODE = 1,
        /// <summary>
        /// 死斗模式
        /// </summary>
        DEAD_FIGHT_MODE = 2,
        /// <summary>
        /// 极速模式(快速模式) [红蓝队]
        /// </summary>
        SPEED_MODE = 3,
        /// <summary>
        /// 大逃杀模式(单人)
        /// </summary>
        ESCAPE_KILL_MODE_ONE = 4,
        /// <summary>
        /// 大逃杀模式(双人)
        /// </summary>
        ESCAPE_KILL_MODE_TWO = 5,
        /// <summary>
        /// 大逃杀模式(4人)
        /// </summary>
        ESCAPE_KILL_MODE_FOUR = 6,
        /// <summary>
        /// 竞技模式 [红蓝队]
        /// </summary>
        ARENA_MODE = 7,
        /// <summary>
        /// 硬核模式 [红蓝队]
        /// </summary>
        HARD_CORE_MODE = 8,
        //onLine
        /// <summary>
        /// 爆破模式 [红蓝队]
        /// </summary>
        BLAST_MODE = 9,
        /// <summary>
        /// online
        /// 团队竞技
        /// </summary>
        TEAM_SPORTS_MODE = 10,
        /// <summary>
        /// 单人打靶
        /// </summary>
        SINGLE_TARGET_SHOOTING = 11,
    }

    ///<summary>
    ///毒圈模式分为三种：随机模式、当前模式、投放模式
    /// </summary>
    public enum PoisonMode
    {
        Closed = 0,
        /// <summary>
        /// 随机模式
        /// </summary>
        Random = 1,
        /// <summary>
        /// 当前模式
        /// </summary>
        Current = 2,
        /// <summary>
        /// 投放模式
        /// </summary>
        Drop = 3,
    }


    /// <summary>
    /// 场景定义
    /// </summary>
    public enum SceneType
    {
        None = 0,           // 起始值
        Update = 1,         // 更新页
        Hall = 2,           // 大厅页
        Room = 3,           // 房间页
        Fight_Offline = 4,  // 线下战斗场景（保留枚举值兼容性）
    }

    /// <summary>
    /// 道具作用类型 (Tactical Function Type)
    /// </summary>
    public enum TacticalFunctionType
    {
        /// <summary>
        /// 攻击 (Attack) - 对目标造成伤害
        /// </summary>
        Attack = 1,

        /// <summary>
        /// 控制 (Control) - 限制目标行动（如眩晕、冰冻等）
        /// </summary>
        Control = 2,

        /// <summary>
        /// 辅助 (Support) - 提供增益效果（如加速、护盾等）
        /// </summary>
        Support = 3,

        /// <summary>
        /// 破碎 (Break) - 破坏场景物体或护甲
        /// </summary>
        Break = 4,

        /// <summary>
        /// 回复 (Recovery) - 恢复生命值或能量
        /// </summary>
        Recovery = 5,

        /// <summary>
        /// 特殊 (Special) - 独特效果（如 再生甲）
        /// </summary>
        Special = 6
    }

    public enum CardUseType
    {
        /// <summary>
        /// 被动
        /// </summary>
        Passive = 1,
        /// <summary>
        /// 主动
        /// </summary>
        Active = 2,
        /// <summary>
        /// 共鸣
        /// </summary>
        Resonance = 3,
    }

    /// <summary>
    /// mqtt语音类型
    /// </summary>
    public enum MqttVoiceType
    {
        /// <summary>
        /// 小组内语音
        /// </summary>
        Group = 1,
        /// <summary>
        /// 房间内语音
        /// </summary>
        Room = 2,
        /// <summary>
        /// 对战内语音
        /// </summary>
        Play = 3,
    }

    /// <summary>
    /// 技能类型 1：被动 2：小技能 3：终极技能 4：打靶技能
    /// </summary>
    public enum SkillType
    {
        /// <summary>
        /// 被动
        /// </summary>
        Passive = 1,
        /// <summary>
        /// 小技能
        /// </summary>
        SmallSkill = 2,
        /// <summary>
        /// 终极技能
        /// </summary>
        UltimateSkill = 3,
        /// <summary>
        /// 打靶技能
        /// </summary>
        TargetShootingSkill = 4,
    }

    /// <summary>
    /// 技能状态 0：释放失败 1：释放成功 2：冷却时间变更 3：技能生效中，无法重复释放
    /// </summary>
    public enum SkillStatus
    {
        /// <summary>
        /// 释放失败
        /// </summary>
        Failed = 0,
        /// <summary>
        /// 释放成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 冷却时间变更
        /// </summary>
        CooldownChanged = 2,
        /// <summary>
        /// 技能生效中，无法重复释放
        /// </summary>
        Active = 3,
        /// <summary>
        /// 技能结束
        /// </summary>
        End = 100,
        /// <summary>
        /// 技能冷却中
        /// </summary>
        ColdDown = 101,
    }

    /// <summary>
    /// 击中部位的枚举
    /// </summary>
    public enum HitLocation
    {
        /// <summary>
        /// 击中了枪（身体）
        /// </summary>
        Gun = 1,
        /// <summary>
        /// 击中了头盔（头部）
        /// </summary>
        Helmet = 2,
        /// <summary>
        /// 击中了背部
        /// </summary>
        Back = 3
    }

    public enum AttackSubType
    {
        Unknown = 0,
        /// <summary>
        /// 普通击杀
        /// </summary>
        Normal = 1,
        /// <summary>
        /// 首杀
        /// </summary>
        FirstKill = 2,


    }

    /// <summary>
    /// 伤害类型的枚举
    /// </summary>
    public enum AttackType
    {
        /// <summary>
        /// 射击
        /// </summary>
        ShootAttack = 1,
        /// <summary>
        /// 角色小技能
        /// </summary>
        SmallSkill = 2,
        /// <summary>
        /// 角色终极技能
        /// </summary>
        UltimateSkill = 3,
        /// <summary>
        /// 道具
        /// </summary>
        ItemAttack = 4,
        /// <summary>
        /// 卡片
        /// </summary>
        CardAttack = 5,
        /// <summary>
        /// 毒圈
        /// </summary>
        PoisonAttack = 6,
        /// <summary>
        /// 栅栏
        /// </summary>
        FenceAttack = 7,
        /// <summary>
        /// 安全区（己方安全区，长时间不出去会死）
        /// </summary>
        SafeArea = 8,
    }

    /// <summary>
    /// 击杀类型的枚举
    /// </summary>
    public enum KillType
    {
        /// <summary>
        /// 射击击杀
        /// </summary>
        ShootKill = 1,
        /// <summary>
        /// 技能效果击杀
        /// </summary>
        SkillKill = 2,
        /// <summary>
        /// 道具击杀
        /// </summary>
        ItemKill = 3,
        /// <summary>
        /// 卡片击杀
        /// </summary>
        CardKill = 4,
        /// <summary>
        /// 毒圈击杀
        /// </summary>
        PoisonKill = 5,
        /// <summary>
        /// 栅栏击杀
        /// </summary>
        FenceKill = 6
    }

    /// <summary>
    /// 电子栅栏类型
    /// </summary>
    public enum FenceType
    {
        /// <summary>
        /// 圆形
        /// </summary>
        Circle = 0,
        /// <summary>
        /// 多边形
        /// </summary>
        Polygon = 1,
        /// <summary>
        /// 三角形
        /// </summary>
        Triangle = 2,
        /// <summary>
        /// 正方形
        /// </summary>
        Square = 3,
    }

    /// <summary>
    /// 电子栅栏是否启用
    /// </summary>
    public enum FenceEnabled
    {
        /// <summary>
        /// 关闭
        /// </summary>
        Closed = 0,
        /// <summary>
        /// 开启
        /// </summary>
        Opened = 1,
    }

    public enum LoadType
    {
        /// <summary>
        /// 默认值
        /// </summary>
        Default = 0,
        /// <summary>
        /// 完成
        /// </summary>
        OK = 1,
        /// <summary>
        /// 订阅Topic失败
        /// </summary>
        SwitchTopicFail = 2,
        /// <summary>
        /// 加载数据失败
        /// </summary>
        LoadSeverDataFail = 3,
    }

    /// <summary>
    /// 角色类型
    /// </summary>
    public enum ProfessionType
    {
        None = 0,
        /// <summary>
        /// 工程-养蜂人 - Beekpr
        /// </summary>
        Engineer = 1,

        /// <summary>
        /// 侦察-凌镜师 - Prism
        /// </summary>
        Recon = 2,

        /// <summary>
        /// 治疗-弥合者 - Servant
        /// </summary>
        Medic = 3,

        /// <summary>
        /// 突击-维序者 - Warden
        /// </summary>
        Assault = 4
    }

    /// <summary>
    /// 1无限方舟 2深浅农场 3引业同行
    /// </summary>
    public enum FactionType
    {
        None = 0,
        /// <summary>
        /// 无限方舟
        /// </summary>
        Ark = 1,
        /// <summary>
        /// 深浅农场
        /// </summary>
        Farm = 2,
        /// <summary>
        /// 引业同行
        /// </summary>
        YYTH = 3,
    }

    /// <summary>
    /// 区分实体线上/线下类型
    /// </summary>
    public enum LineType
    {
        /// <summary>
        /// 线上线下通用
        /// </summary>
        Default = 0,
        /// <summary>
        /// 线下
        /// </summary>
        Offline = 1,
        /// <summary>
        /// 线上
        /// </summary>
        Online = 2,
    }

    /// <summary>
    /// 设备类型 1：枪 2：头盔 3：胸甲 4：裤
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// 枪
        /// </summary>
        Gun = 1,
        /// <summary>
        /// 头盔
        /// </summary>
        Head = 2,
        /// <summary>
        /// 胸甲
        /// </summary>
        ChestArmor = 3,
        /// <summary>
        /// 裤子
        /// </summary>
        Pants = 4,
    }

    /// <summary>
    /// 设备子类型(根据这个subType从服务器获取到的列表中查找武器属性)
    /// </summary>
    public enum SubDeviceType
    {
        None = 0,
        FirstGen = 1,
        SecondGen = 2,
    }

    ///<summary>
    ///枪的类型
    /// </summary>
    public enum GunTypeEnum
    {
        /// <summary>
        /// 微冲
        /// </summary>
        SubmachinePistol = 1,
        /// <summary>
        /// 手枪
        /// </summary>
        Pistol = 2,
        /// <summary>
        /// 步枪
        /// </summary>
        Rifle = 3,
        /// <summary>
        /// 狙击步枪
        /// </summary>
        SniperRifle = 4,
        /// <summary>
        /// 霰弹枪
        /// </summary>
        Shotgun = 5,
        /// <summary>
        /// 机枪
        /// </summary>
        MachineGun = 6,
    }

    /// <summary>
    /// 设备颜色枚举类型
    /// </summary>
    public enum DeviceColor
    {
        None = 0,
        /// <summary>
        /// 曜石黑
        /// </summary>
        ObsidianBlack = 1,
        /// <summary>
        /// 皓月白
        /// </summary>
        MoonWhite = 2,
        /// <summary>
        /// 粉晶色
        /// </summary>
        RoseQuartz = 3,
        /// <summary>
        /// 蒂芙尼蓝
        /// </summary>
        TiffanyBlue = 4,
        /// <summary>
        /// 中国红
        /// </summary>
        ChinaRed = 5,
        /// <summary>
        /// 曜钻黑
        /// </summary>
        HeiBai = 6,
        /// <summary>
        /// 润珠白
        /// </summary>
        PearlWhite = 7,
        /// <summary>
        /// 幻电紫
        /// </summary>
        PhantomPurple = 8,
        /// <summary>
        /// 月陨白
        /// </summary>
        MeteoriteWhite = 9,
    }

    /// <summary>
    /// 道具类型 1:烟雾弹 2:闪光弹 3:手雷 4：燃烧弹 5：肾上腺素 6：半甲 7：迅捷甲 8：重甲 9：再生甲
    /// </summary>
    public enum TacticalType
    {
        /// <summary>
        /// 烟雾弹
        /// </summary>
        SmokeGrenade = 1,
        /// <summary>
        /// 闪光弹
        /// </summary>
        FlashGrenade = 2,
        /// <summary>
        /// 高爆手雷
        /// </summary>
        FragGrenade = 3,
        /// <summary>
        /// 燃烧弹
        /// </summary>
        IncendiaryGrenade = 4,
        /// <summary>
        /// 肾上腺素
        /// </summary>
        Adrenaline = 5,
        /// <summary>
        /// 半甲
        /// </summary>
        HalfArmor = 6,
        /// <summary>
        /// 迅捷甲
        /// </summary>
        SwiftArmor = 7,
        /// <summary>
        /// 重甲
        /// </summary>
        HeavyArmor = 8,
        /// <summary>
        /// 再生甲
        /// </summary>
        RegenerativeArmor = 9
    }

    /// <summary>
    /// 道具父类型 1：投掷道具 2：护甲道具
    /// </summary>
    public enum TacticalParentType
    {
        /// <summary>
        /// 投掷道具
        /// </summary>
        ThrowingProps = 1,
        /// <summary>
        /// 护甲道具
        /// </summary>
        ArmorProps = 2,
        /// <summary>
        /// 肾上腺素
        /// </summary>
        Adrenaline = 3,
    }

    /// <summary>
    /// 道具状态:0未使用，1已使用
    /// </summary>
    public enum TacticalStatus
    {
        /// <summary>
        /// 未使用
        /// </summary>
        Unused = 0,
        /// <summary>
        /// 已使用
        /// </summary>
        Used = 1
    }

    /// <summary>
    /// 卡的状态: 0未使用, 1生效中, 2已使用, -1购买未成功
    /// </summary>
    public enum CardStatus
    {
        /// <summary>
        /// 未使用
        /// </summary>
        Unused = 0,
        /// <summary>
        /// 生效中
        /// </summary>
        Active = 1,
        /// <summary>
        /// 已使用
        /// </summary>
        Used = 2,
        /// <summary>
        /// 购买未成功
        /// </summary>
        PurchaseFailed = -1
    }

    /// <summary>
    /// 复活类型（0:否 1:无条件复活 2:扫码复活  3:定点复活）
    /// </summary>
    public enum RevivalType
    {
        /// <summary>
        /// 无法复活
        /// </summary>
        NoRevival = 0,
        /// <summary>
        /// 无条件复活(自动复活)
        /// </summary>
        UnconditionalRevival = 1,
        /// <summary>
        /// 扫码复活
        /// </summary>
        QrCodeRevival = 2,
        /// <summary>
        /// 定点复活(走到某个经纬度复活)
        /// </summary>
        FixedPointRevival = 3
    }

    /// <summary>
    /// 扫码类型
    /// </summary>
    public enum ScanCodeType
    {
        /// <summary>
        /// 扫码复活
        /// </summary>
        Revive,
        /// <summary>
        /// 扫码加子弹
        /// </summary>
        AddBullet,
        /// <summary>
        /// 扫码加血
        /// </summary>
        AddBlood,
        /// <summary>
        /// 扫描装备
        /// </summary>
        ScanDevice,
    }

    ///<summary>
    ///游戏胜负（0:平局 1:胜利 2:失败）
    /// </summary>
    public enum GameResult
    {
        /// <summary>
        /// 平局
        /// </summary>
        Draw = 0,
        /// <summary>
        /// 胜利
        /// </summary>
        Victory = 1,
        /// <summary>
        /// 失败
        /// </summary>
        Defeat = 2
    }

    /// <summary>
    /// 主理人观战状态
    /// 观战状态 0：未观战(加入游戏) 1：观战(不加入游戏)
    /// </summary>
    public enum WatchFlag
    {
        /// <summary>
        /// 未观战，加入游戏
        /// </summary>
        NotWatching = 0,

        /// <summary>
        /// 观战，不加入游戏
        /// </summary>
        Watching = 1
    }

    /// <summary>
    /// xx请求类型
    /// </summary>
    public enum RequestRecordType
    {
        /// <summary>
        /// 好友请求
        /// </summary>
        FriendApply = 1
    }

    /// <summary>
    /// 检查设备是否安装AR apk
    /// </summary>
    public enum CheckResult
    {
        /// <summary>
        /// 不是安卓
        /// </summary>
        NotAndroid=0,
        /// <summary>
        /// 华为手机安装了AR apk
        /// </summary>
        HuaweiPackageInstalled=1,
        /// <summary>
        /// 其他手机安装了AR apk
        /// </summary>
        GooglePackageInstalled=2,
        /// <summary>
        /// 华为手机没有安装AR apk
        /// </summary>
        HuaweiPackageMissing=3,
        /// <summary>
        /// 其他手机没有安装AR apk
        /// </summary>
        GooglePackageMissing=4
    }


    /// <summary>
    /// 某某请求的处理状态
    /// 0.待处理 1.已接受 2.已拒绝 3.已过期
    /// </summary>
    public enum RequestRecordStatus
    {
        /// <summary>
        /// 请求记录等待处理。
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 请求记录已被接受。
        /// </summary>
        Accepted = 1,

        /// <summary>
        /// 请求记录已被拒绝。
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// 请求记录已过期。
        /// </summary>
        Expired = 3
    }

    /// <summary>
    /// 接收方是否已读（0: 未读, 1: 已读）
    /// </summary>
    public enum RequestRecordReadStatus
    {
        /// <summary>
        /// 未读。
        /// </summary>
        Unread = 0,

        /// <summary>
        /// 已读。
        /// </summary>
        Read = 1
    }

    /// <summary>
    /// 枪功率类型（1：小功率 2：大功率）
    /// </summary>
    public enum GunPowerType
    {
        /// <summary>
        /// 未知或未定义
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 小功率
        /// </summary>
        LowPower = 1,

        /// <summary>
        /// 大功率
        /// </summary>
        HighPower = 2
    }

    /// <summary>
    /// 爆破模式攻守方定义
    /// </summary>
    public enum BlastCampType
    {
        Default = 0,
        /// <summary>
        /// 进攻方
        /// </summary>
        Attacker = 1,
        /// <summary>
        /// 防守方
        /// </summary>
        Defender = 2,
    }

    /// <summary>
    /// 队伍颜色类型
    /// </summary>
    public enum TeamType
    {
        Default = 0,
        /// <summary>
        /// 红色方
        /// </summary>
        Red = 1,
        /// <summary>
        /// 蓝色方
        /// </summary>
        Blue = 2,
    }

    /// <summary>
    /// 是否同意： 1:同意/接受/成功 2:拒绝/失败
    /// </summary>
    public enum CommonResult
    {
        /// <summary>
        /// 同意/接受/成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 拒绝/失败
        /// </summary>
        Fail = 0

    }

    ///<summary>
    ///是否接受匹配对局:1:接受 2:拒绝 -1:取消
    /// </summary>
    public enum MatchAcceptStatus
    {
        /// <summary>
        /// 接受匹配对局
        /// </summary>
        Accept = 1,
        /// <summary>
        /// 拒绝匹配对局
        /// </summary>
        Reject = 2,
        /// <summary>
        /// 取消匹配对局
        /// </summary>
        Cancel = -1
    }

    /// <summary>
    /// 小组匹配状态
    /// </summary>
    public enum MatchStatus
    {
        /// <summary>
        /// 备战中
        /// </summary>
        BeforeMatching = 0,
        /// <summary>
        /// 匹配中
        /// </summary>
        Matching = 1,
        /// <summary>
        /// 匹配成功
        /// </summary>
        Matched = 2,
        /// <summary>
        /// 等待开局
        /// </summary>
        Preparing = 3,
        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 4,
    }

    /// <summary>
    /// 傀儡投掷物状态
    /// </summary>
    public enum PuppetMissileState
    {
        Default = 0,
        /// <summary>
        /// 道具飞行中
        /// </summary>
        Fly = 1,
        /// <summary>
        /// 道具爆炸
        /// </summary>
        Explosion = 2,
    }

    /// <summary>
    /// 场次评级 6 —1     S+ — D
    /// </summary>
    public enum PlayScoreLevel
    {
        /// <summary>
        /// S+ 级 - 最高评级
        /// </summary>
        SPlus = 6,

        /// <summary>
        /// S 级
        /// </summary>
        S = 5,

        /// <summary>
        /// A 级
        /// </summary>
        A = 4,

        /// <summary>
        /// B 级
        /// </summary>
        B = 3,

        /// <summary>
        /// C 级
        /// </summary>
        C = 2,

        /// <summary>
        /// D 级 - 最低评级
        /// </summary>
        D = 1
    }

    /// <summary>
    /// 射击状态
    /// </summary>
    public enum ShootingMode
    {
        /// <summary>
        /// 单发模式
        /// </summary>
        Single,
        /// <summary>
        /// 连发模式
        /// </summary>
        Burst
    }

    /// <summary>
    /// 回合的获胜方式
    /// </summary>
    public enum RoundWinCondition
    {
        /// <summary>
        /// 平局
        /// </summary>
        Draw = 0,

        /// <summary>
        /// 全歼敌方
        /// </summary>
        Annihilation = 1,

        /// <summary>
        /// 击杀总人数
        /// </summary>
        TotalKills = 2,

        /// <summary>
        /// 总伤害量
        /// </summary>
        TotalDamage = 3
    }

    /// <summary>
    /// 特效类型
    /// </summary>
    public enum EffecResourceType
    {
        None = 0,
        /// <summary>
        /// 在ui层级上
        /// </summary>
        OnTheUI = 1,
        /// <summary>
        /// 在ui层级下
        /// </summary>
        UnderTheUI = 2,
        /// <summary>
        /// 全屏特效在UI上
        /// </summary>
        FullScreenOnTheUI = 3,
        /// <summary>
        /// 全屏特效在UI下
        /// </summary>
        FullScreenUnderTheUI = 4,
        /// <summary>
        /// 战斗特效
        /// </summary>
        Fight = 5,
    }

    /// <summary>
    /// 地图下发的道具类型(用于地图显示)
    /// 1：玩家 2：道具 3：技能 4：卡牌
    /// </summary>
    public enum DownMapType
    {
        /// <summary>
        /// 玩家
        /// </summary>
        Player = 1,
        /// <summary>
        /// 道具
        /// </summary>
        Item = 2,
        /// <summary>
        /// 技能
        /// </summary>
        Skill = 3,
        /// <summary>
        /// 卡牌
        /// </summary>
        Card = 4,
    }

    /// <summary>
    /// 子类型 1：持续性范围伤害 2：持续性范围治疗 3：范围瞬伤 4：范围瞬疗 5：范围视野遮挡型 6：移动型范围类 7：领域类 8：资源包（绿） 9：资源包（蓝） 10：资源包（紫） 11：资源包（金）
    /// </summary>
    public enum DownMapSubType
    {
        /// <summary>
        /// 未知
        /// </summary>
        None = 0,
        /// <summary>
        /// 持续性范围伤害
        /// </summary>
        ContinuousDamage = 1,
        /// <summary>
        /// 持续性范围治疗
        /// </summary>
        ContinuousHeal = 2,
        /// <summary>
        /// 范围瞬伤
        /// </summary>
        InstantDamage = 3,
        /// <summary>
        /// 范围瞬疗
        /// </summary>
        InstantHeal = 4,
        /// <summary>
        /// 范围视野遮挡型
        /// </summary>
        VisionBlock = 5,
        /// <summary>
        /// 移动型范围类
        /// </summary>
        MovingArea = 6,
        /// <summary>
        /// 领域类
        /// </summary>
        Domain = 7,
        /// <summary>
        /// 资源包（绿）
        /// </summary>
        GreenMaterialBox = 8,
        /// <summary>
        /// 资源包（蓝）
        /// </summary>
        BlueMaterialBox = 9,
        /// <summary>
        /// 资源包（紫）
        /// </summary>
        PurpleMaterialBox = 10,
        /// <summary>
        /// 资源包（金）
        /// </summary>
        GoldMaterialBox = 11,
    }

    /// <summary>
    /// 回合状态
    /// </summary>
    public enum RoundStatus
    {
        None = 0,
        /// <summary>
        /// 游戏中
        /// </summary>
        Fighting = 1,
        /// <summary>
        /// 结束
        /// </summary>
        End = 2,
    }

    /// <summary>
    /// 物资箱类型： 类型 1：物资 2：空投
    /// </summary>
    public enum MaterialBoxType
    {
        /// <summary>
        /// 物资箱
        /// </summary>
        Material = 1,
        /// <summary>
        /// 空投箱
        /// </summary>
        AirDrop = 2,
    }

    /// <summary>
    /// 物资箱子类型: 1：资源包（绿） 2：资源包（蓝） 3：资源包（紫） 4：资源包（金）
    /// </summary>
    public enum MaterialBoxSubType
    {
        /// <summary>
        /// 资源包（绿）
        /// </summary>
        Green = 1,
        /// <summary>
        /// 资源包（蓝）
        /// </summary>
        Blue = 2,
        /// <summary>
        /// 资源包（紫）
        /// </summary>
        Purple = 3,
        /// <summary>
        /// 资源包（金）
        /// </summary>
        Gold = 4,
    }

    /// <summary>
    /// 枪的部件类型：1:枪管 2:瞄准镜 3:弹匣 4:枪托
    /// </summary>
    public enum WeaponPart
    {
        DEFAULT = 0,
        /// <summary>
        /// 枪管
        /// </summary>
        Barrel = 1,
        /// <summary>
        /// 瞄准
        /// </summary>
        Aim = 2,
        /// <summary>
        /// 弹匣
        /// </summary>
        Magazine = 3,
        /// <summary>
        /// 枪托
        /// </summary>
        Buttstock = 4,
    }

    /// <summary>
    /// 角色通用技能与功能性按键
    /// </summary>
    public enum PlayerSkillID
    {
        /// <summary>
        /// 角色通用技能
        /// </summary>
        Primary = 0,
        /// <summary>
        /// 角色小技能
        /// </summary>
        Secondary = 1,
        /// <summary>
        /// 角色大招
        /// </summary>
        Ultimate = 2,
        /// <summary>
        /// 下包
        /// </summary>
        PackageInstall = 4,
        /// <summary>
        /// 拆包
        /// </summary>
        PackageDisarm = 5,
        /// <summary>
        /// 治疗扎针
        /// </summary>
        PropCure = 6,
        /// <summary>
        /// 投掷烟雾弹
        /// </summary>
        PropSmoke = 7,
        /// <summary>
        /// 投掷闪光弹
        /// </summary>
        PropFlash = 8,
        /// <summary>
        /// 投掷炸弹
        /// </summary>
        PropBomb = 9,
        /// <summary>
        /// 投掷燃烧瓶
        /// </summary>
        PropFire = 10,
        /// <summary>
        /// 死亡
        /// </summary>
        CharDie = 11,
        /// <summary>
        /// 复活
        /// </summary>
        CharRelive = 12,
        /// <summary>
        /// 装弹
        /// </summary>
        Reload = 13
    }

    public enum PlayerMode
    {
        /// <summary>
        /// 线上模式
        /// </summary>
        Online,
        /// <summary>
        /// 线下模式
        /// </summary>
        Offline
    }

    /// <summary>
    /// 是否是主理人
    /// </summary>
    public enum ManagePeoType
    {
        /// <summary>
        /// 非主理人
        /// </summary>
        None = 0,
        /// <summary>
        /// 主理人
        /// </summary>
        Owner = 1,
    }

    public enum HitBodyType
    {
        /// <summary>
        /// 无盾无甲
        /// </summary>
        None = 0,
        /// <summary>
        /// 只有甲
        /// </summary>
        OnlyArmor = 1,
        /// <summary>
        /// 只有盾
        /// </summary>
        OnlyShield = 2,
        /// <summary>
        /// 有盾有甲
        /// </summary>
        ShieldAndArmor = 3,
    }
    /// <summary>
    /// 是否是mvp
    /// </summary>
    public enum MvpState
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 胜者方Mvp，全场最佳
        /// </summary>
        Mvp = 1,
        /// <summary>
        /// 败者方Svp
        /// </summary>
        Svp = 2,
    }

    public enum TrophyLevel
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 一级
        /// </summary>
        Level1 = 1,
        /// <summary>
        /// 二级
        /// </summary>
        Level2 = 2,
        /// <summary>
        /// 三级
        /// </summary>
        Level3 = 3,
        /// <summary>
        /// 四级
        /// </summary>
        Level4 = 4,
        /// <summary>
        /// 五级
        /// </summary>
        Level5 = 5,
    }

    public enum AddFriendStatus
    {
        /// <summary>
        /// 可申请
        /// </summary>
        canApply = 0,
        /// <summary>
        /// 已申请
        /// </summary>
        alreadyApploed = 1,
        /// <summary>
        /// 已添加
        /// </summary>
        added = 2,
    }

    /// <summary>
    /// 打靶模式
    /// </summary>
    public enum TargetShootingMode
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 网格射击
        /// </summary>
        Grid = 1,

        /// <summary>
        /// 闪烁靶鸡射击
        /// </summary>
        Flicker = 2,

        /// <summary>
        /// 割草扫射射击
        /// </summary>
        Mow = 3,
    }

    /// <summary>
    /// 靶类型：1普通计分靶 2异色计分靶 3惩罚靶 4boss靶 5精英靶 6异色精英靶 7爆破靶 8钻石靶 9找茬靶
    /// </summary>
    public enum TargetType
    {
        None = 0,

        /// <summary>
        /// 普通计分靶
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 异色计分靶
        /// </summary>
        Heterochromatic = 2,

        /// <summary>
        /// 惩罚靶
        /// </summary>
        Penalty = 3,

        /// <summary>
        /// boss靶
        /// </summary>
        Boss = 4,

        /// <summary>
        /// 精英靶
        /// </summary>
        Elite = 5,

        /// <summary>
        /// 异色精英靶
        /// </summary>
        HeterochromaticElite = 6,

        /// <summary>
        /// 爆破靶
        /// </summary>
        Blast = 7,

        /// <summary>
        /// 钻石靶
        /// </summary>
        Diamond = 8,

        /// <summary>
        /// 找茬靶
        /// </summary>
        Difference = 9,
    }

    /// <summary>
    /// 靶移动类型：0不移动 1普通移动 2折返运动
    /// </summary>
    public enum TargetMoveType
    {
        /// <summary>
        /// 不移动
        /// </summary>
        None = 0,

        /// <summary>
        /// 普通移动
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 折返运动
        /// </summary>
        PingPong = 2,
    }
    
    /// <summary>
    /// 5S设备类型
    /// </summary>
    public enum DeviceType5S
    {
        /// <summary>
        /// 音乐盒子
        /// </summary>
        MusicBox = 3,
        /// <summary>
        /// 比分牌
        /// </summary>
        ScoreCard = 4,
        /// <summary>
        /// Unity比分牌+音乐盒子
        /// </summary>
        UnityScoreCard = 2
    }
}