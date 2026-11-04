using System.Collections.Generic;
using Common.Bluetooth.Model.Read;
using Common.Network.Model.Http;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel;
using LetsPiu.Base.BaseScripts.Define.BlueToothModel.Read;
using LetsPiu.Base.BaseScripts.Define.ModelDefine;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Event;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Read;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.Mqtt.Send;
using LetsPiu.Base.BaseScripts.Define.ModelDefine.VoiceManager;
using LetsPiu.Common.Network.Model.Http.Play;
using LetsPiu.Common.Network.Model.Mqtt.Read;
using LetsPiu.Define;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;

namespace LetsPiu
{
    /// <summary>
    /// 消息Id定义
    /// </summary>
    public enum EventMsg
    {
        None = 0, // 起始消息头

        #region 蓝牙设备相关消息

        /// <summary>
        /// 设备管理器->初始化成功
        /// </summary>
        DevConnectorInitSuccessEvent,
        /// <summary>
        /// 设备管理器->初始化失败
        /// <para>回调函数参数: <see cref = "string" />错误原因</para>
        /// </summary>
        DevConnectorInitFailedEvent,
        /// <summary>
        /// 设备管理器->枪开始连接
        /// </summary>
        GunConnectingEvent,
        /// <summary>
        /// 设备管理器->枪连接成功
        /// </summary>
        GunConnectedEvent,
        /// <summary>
        /// 设备管理器->抢链接失败
        /// </summary>
        GunConnectFailEvent,
        /// <summary>
        /// 设备管理器->枪连接失败消息内容
        /// <para>回调函数参数: <see cref = "string" />错误原因</para>
        /// </summary>
        GunConnectFailEventMessage,
        /// <summary>
        /// 设备管理器->抢链接断开
        /// </summary>
        GunDisconnectedEvent,
        /// <summary>
        /// 设备管理器->枪固件更新状态
        /// <para>回调函数参数: <see cref = "float" />百分比</para>
        /// </summary>
        GunFirmwareUpdateProgress,
        /// <summary>
        /// 设备管理器->枪固件更新成功
        /// </summary>
        GunFirmwareUpdateSuccess,
        /// <summary>
        /// 设备管理器->枪固件更新失败
        /// </summary>
        GunFirmwareUpdateFailed,
        /// <summary>
        /// 设备管理器->枪绑定成功
        /// <para>回调函数参数: <see cref = "BindDeviceResult" /></para >
        /// </summary>
        GunBoundEvent,
        /// <summary>
        /// 设备管理器->枪解绑成功
        /// <para>回调函数参数: <see cref="BindDeviceResult"/></para>
        /// </summary>
        GunUnboundEvent,
        /// <summary>
        /// 设备管理器->头固件更新状态
        /// <para>回调函数参数: <see cref = "float" />百分比</para>
        /// </summary>
        HeadFirmwareUpdateProgress,
        /// <summary>
        /// 设备管理器->头枪固件更新成功
        /// </summary>
        HeadFirmwareUpdateSuccess,
        /// <summary>
        /// 设备管理器->头固件更新失败
        /// </summary>
        HeadFirmwareUpdateFailed,
        /// <summary>
        /// 设备管理器->头绑定成功
        /// <para>回调函数参数: <see cref="BindDeviceResult"/></para>
        /// </summary>
        HeadBoundEvent,
        /// <summary>
        /// 头解绑成功
        /// <para>回调函数参数: <see cref="BindDeviceResult"/></para>
        /// </summary>
        HeadUnboundEvent,
        /// <summary>
        /// 枪的Ping消息事件
        /// <para>回调函数参数: <see cref="BlueGunPingMessage"/></para>
        /// </summary>
        GunPingEvent,
        /// <summary>
        /// 枪被击中事件
        /// <para>回调函数参数: <see cref="BlueGunBeHitMessage"/></para>
        /// </summary>
        GunBeHitEvent,
        /// <summary>
        /// 读取到了枪的MAC地址
        /// <para>回调函数参数: <see cref="BlueGunMacMessage"/></para>
        /// </summary>
        GunReadMacEvent,
        /// <summary>
        /// 读取到了手机支架的状态
        /// <para>回调函数参数: <see cref="BlueGunClampMessage"/></para>
        /// </summary>
        GunReadClampStatusEvent,
        /// <summary>
        /// [重要说明]
        /// <para>输入层的蓝牙开枪事件，不必要的情况不要直接订阅该事件。</para>
        /// <para>开枪事件</para>
        /// </summary>
        GunShootEvent,
        /// <summary>
        /// 子弹发射事件(一般由枪械类发送)
        /// </summary>
        GunShootBulletEvent,
        /// <summary>
        /// 开枪结束
        /// <para>回调函数参数: <see cref="int"/></para>
        /// </summary>
        GunShootFinishEvent,
        /// <summary>
        /// 开枪事件(单发)
        /// </summary>
        GunSingeShootEvent,
        /// <summary>
        /// 单发结束事件，非必要不使用
        /// </summary>
        GunSingeShootEndEvent,
        /// <summary>
        /// 开枪事件(连发)
        /// </summary>
        GunBurstShootEvent,
        /// <summary>
        /// 连发开始事件
        /// </summary>
        GunBurstStartEvent,
        /// <summary>
        /// 连发结束事件
        /// </summary>
        GunBurstEndEvent,
        /// <summary>
        /// [重要说明]
        /// <para>输入层的蓝牙装弹事件，不必要的情况不要直接订阅该事件。</para>
        /// <para>装弹事件</para>
        /// </summary>
        GunReloadEvent,
        /// <summary>
        /// 手机夹子打开
        /// </summary>
        PhoneClampOpenedEvent,
        /// <summary>
        /// 手机夹子关闭(高手模式)
        /// </summary>
        PhoneClampClosedEvent,
        /// <summary>
        /// 手机夹子事件
        /// <para>回调函数参数: <see cref="KeyStatus"/></para>
        /// </summary>
        PhoneClampEvent,
        /// <summary>
        /// F键事件
        /// <para>回调函数参数: <see cref="KeyEvent"/></para>
        /// </summary>
        FKeyEvent,
        /// <summary>
        /// F 键按下事件
        /// </summary>
        FKeyDownEvent,
        /// <summary>
        /// F 键弹起事件
        /// </summary>
        FKeyUpEvent,
        /// <summary>
        /// F 键短按事件
        /// </summary>
        FKeyPressEvent,
        /// <summary>
        /// F 键长按事件
        /// </summary>
        FKeyLongPressEvent,
        /// <summary>
        /// Q键事件
        /// <para>回调函数参数: <see cref="KeyEvent"/></para>
        /// </summary>
        QKeyEvent,
        /// <summary>
        /// Q 键按下事件
        /// </summary>
        QKeyDownEvent,
        /// <summary>
        /// Q 键弹起事件
        /// </summary>
        QKeyUpEvent,
        /// <summary>
        /// Q 键短按事件
        /// </summary>
        QKeyPressEvent,
        /// <summary>
        /// Q 键长按事件
        /// </summary>
        QKeyLongPressEvent,
        /// <summary>
        /// W键事件
        /// <para>回调函数参数: <see cref="KeyEvent"/></para>
        /// </summary>
        WKeyEvent,
        /// <summary>
        /// W 键按下事件
        /// </summary>
        WKeyDownEvent,
        /// <summary>
        /// W 键弹起事件
        /// </summary>
        WKeyUpEvent,

        /// <summary>
        /// W 键短按事件
        /// </summary>
        WKeyPressEvent,
        /// <summary>
        /// W 键长按事件
        /// </summary>
        WKeyLongPressEvent,

        /// <summary>
        /// R键事件
        /// <para>回调函数参数: <see cref="KeyEvent"/></para>
        /// </summary>
        RKeyEvent,
        /// <summary>
        /// R 键按下事件
        /// </summary>
        RKeyDownEvent,
        /// <summary>
        /// R 键弹起事件
        /// </summary>
        RKeyUpEvent,
        /// <summary>
        /// R 键短按事件
        /// </summary>
        RKeyPressEvent,
        /// <summary>
        /// R 键长按事件
        /// </summary>
        RKeyLongPressEvent,

        /// <summary>
        /// E键事件
        /// <para>回调函数参数: <see cref="KeyEvent"/></para>
        /// </summary>
        EKeyEvent,
        /// <summary>
        /// E 键按下事件
        /// </summary>
        EKeyDownEvent,
        /// <summary>
        /// E 键弹起事件
        /// </summary>
        EKeyUpEvent,

        /// <summary>
        /// E 键短按事件
        /// </summary>
        EKeyPressEvent,
        /// <summary>
        /// Q 键长按事件
        /// </summary>
        EKeyLongPressEvent,

        #region 电视遥控（Remote Key）广播事件
        /// <summary>
        /// 电视遥控按键事件（枚举）
        /// <para>回调函数参数: <see cref="LetsPiu.Main.Common.Scripts.RemoteKey"/></para>
        /// </summary>
        RemoteKeyEvent,
        /// <summary>
        /// 电视遥控按键原始 keyCode 事件（整型）
        /// <para>回调函数参数: <see cref="int"/></para>
        /// </summary>
        RemoteKeyCodeEvent,
        #endregion

        /// <summary>
        /// 头开始连接
        /// </summary>
        HeadConnectingEvent,
        /// <summary>
        /// 头连接成功
        /// </summary>
        HeadConnectedEvent,
        /// <summary>
        /// 头连接失败
        /// </summary>
        HeadConnectFailEvent,
        /// <summary>
        /// 头断开连接
        /// </summary>
        HeadDisconnectedEvent,
        /// <summary>
        /// 头被击中事件
        /// <para>回调函数参数: <see cref="BlueHeadBeHitMessage"/></para>
        /// </summary>
        HeadBeHitEvent,
        /// <summary>
        /// 头的Ping消息事件
        /// <para>回调函数参数: <see cref="BlueHeadPingMessage"/></para>
        /// </summary>
        HeadPingEvent,
        /// <summary>
        /// 通知枪不能发射
        /// <para>回调函数参数: <see cref="float"/>可以发射的延迟时间</para>
        /// </summary>
        NoticeGunNotFire,
        /// <summary>
        /// 通知枪发射
        /// </summary>
        NoticeGunCanFire,
        #endregion

        #region Mqtt相关消息

        /// <summary>
        /// MQTT连接失败
        /// <para>回调函数参数: <see cref="MqttClientDisconnectReason"/></para>
        /// </summary>
        MqttDisconnectEvent,
        /// <summary>
        /// MQTT连接成功
        /// </summary>
        MqttConnectedEvent,
        /// <summary>
        /// Mqtt连接中
        /// </summary>
        MqttConnectingEvent,
        /// <summary>
        /// MQTT消息解析失败
        /// <para>回调函数参数: <see cref="System.Exception"/></para>
        /// </summary>
        MqttParseErrorEvent,
        /// <summary>
        /// MQTT ping消息
        /// <para>回调函数参数: <see cref="PingMessage"/></para>
        /// </summary>
        MqttPingMessageEvent,
        ///<summary>
        ///创建队伍消息(新手模式)
        /// </summary>
        MqttCreateTeamMessageEvent,
        /// <summary>
        ///加入队伍消息
        ///</summary>
        MqttJoinTeamMessageEvent,
        /// <summary>
        /// 顶号
        /// <para>回调函数参数: <see cref="ReplaceLogin"/></para>
        /// </summary>
        MqttReplaceLoginMessageEvent,
        /// <summary>
        /// 场次详情
        /// </summary>
        MqttPlayDetailMessageEvent,
        /// <summary>
        /// 游戏状态发生变化
        /// <para>回调函数参数: <see cref="GameState"/></para>
        /// </summary>
        MqttGameStateChanged,
        /// <summary>
        /// 对战玩家数据同步(5秒钟更新一次)
        /// <para>回调函数参数: <see cref="List{Player}"/></para>
        /// </summary>
        MqttBattlePlayerMessageEvent,
        /// <summary>
        /// 游戏开始消息
        /// <summary>
        MqttGameStart,
        /// <summary>
        /// 语音
        /// <para>回调函数参数: <see cref="VoiceReadMessage"/></para>
        /// </summary>
        MqttVoiceMessageEvent,
        /// <summary>
        /// 玩家角色变更消息
        /// <para>回调函数参数: <see cref="PlayerProfession"/></para>
        /// </summary>
        MqttPlayerProfessionEvent,
        /// <summary>
        /// 选择角色发生变化事件
        /// <para>回调函数参数: <see cref="PlayerProfession"/></para>
        /// </summary>
        MqttPlayerChoosseProfessionEvent,
        /// <summary>
        /// 玩家自己的技能释放事件
        /// <para>回调函数参数: <see cref="MqttSkillMessage"/></para>
        /// </summary>
        MqttSkillMessageEvent,
        /// <summary>
        /// 击中消息
        /// <para>回调函数参数: <see cref="HitMessage"/></para>
        /// </summary>
        MqttHitMessageEvent,
        /// <summary>
        /// 攻击记录
        /// <para>回调函数参数: <see cref="AttackRecord"/></para>
        /// </summary>
        MqttAttackRecordMessageEvent,
        /// <summary>
        /// 被击中记录
        /// <para>回调函数参数: <see cref="SufferRecord"/></para>
        /// </summary>
        MqttSufferRecordMessageEvent,
        /// <summary>
        /// 技能特效生效
        /// <para>回调函数参数: <see cref="VisualEffect"/></para>
        /// </summary>
        MqttVisualEffectMessageEvent,
        /// <summary>
        /// 击杀
        /// <para>回调函数参数: <see cref="KillMessage"/></para>
        /// </summary>
        MqttKillMessageEvent,
        /// <summary>
        /// 玩家护甲变更
        /// </summary>
        MqttArmorMessageEvent,
        /// <summary>
        /// Mqtt下发护盾更新消息
        /// <para>回调函数参数: <see cref="MqttShieldMessage"/></para>
        /// </summary>
        MqttShieldMessageEvent,
        /// <summary>
        /// 玩家复活的mqtt消息
        /// <para>回调函数参数: <see cref="Player"/></para>
        /// </summary>
        MqttReviveMessageEvent,
        /// <summary>
        /// 电子围栏
        /// <para>回调函数参数: <see cref="Fence"/></para>
        /// </summary>
        MqttFenceMessageEvent,
        /// <summary>
        /// 毒圈缩小
        /// <para>回调函数参数: <see cref="PoisonCircle"/></para>
        /// </summary>
        MqttPoisonCircleMessageEvent,
        /// <summary>
        /// 下发的玩家坐标
        /// <para>回调函数参数: <see cref="List{DownMap}"/></para>
        /// </summary>
        MqttDownMapMessageEvent,
        /// <summary>
        /// Mqtt订阅成功
        /// <para>回调函数参数: <see cref="string"/></para>
        /// </summary>
        MqttSubscribeSuccessEvent,
        /// <summary>
        /// Mqtt订阅失败
        /// <para>回调函数参数: (<see cref="string"/>, <see cref="System.Exception"/>) </para>
        /// </summary>
        MqttSubscribeFailEvent,

        #region 比分牌投屏相关消息

        /// <summary>
        /// 比分牌 MQTT 切屏 / 事件统一入口（<c>msgType=scoreCard</c>）。
        /// 见 <see cref="LetsPiu.Common.Network.Model.ScoreCard.ScoreCardCommand"/>。
        /// 由 <c>HomePageLogic</c> 订阅并按 <c>category + subCode</c> 路由到具体 Panel / Page。
        /// </summary>
        ScoreCardCommandEvent,

        /// <summary>
        /// 比分牌 MQTT 语音/背景音乐控制（<c>msgType=voice</c>，含 <c>type=play/stop/vol</c> 等，载荷见 <see cref="LetsPiu.Common.Network.Model.ScoreCard.ScoreCardVoiceCommandPayload"/>）
        /// </summary>
        ScoreCardVoiceMusicCommandEvent,

        /// <summary>
        /// 比分牌当前场次数据刷新（载荷 <see cref="LetsPiu.Common.Network.Model.ScoreCard.ScoreCardMatchData"/>）。
        /// 由 <c>ScoreCardFightPanel</c> 经 <c>ScoreCardMatchDataHub.Publish</c> 广播；
        /// 各模式顶部 Top widget 自行订阅接收，页面不再转发。
        /// </summary>
        ScoreCardMatchDataEvent,

        /// <summary>
        /// 比分牌顶部对比条「直投」数据（载荷 <see cref="LetsPiu.UI.ScoreCardCommon.ScoreCardTopScore"/>）。
        /// 用于没有完整 <see cref="ScoreCardMatchDataEvent"/> 流的场景（如 S6 回放按采样帧喂顶部比分/击杀），
        /// 由数据源直接广播红蓝顶部值，积分模式 Top widget 订阅刷新（含特效）。
        /// </summary>
        ScoreCardTopScoreEvent,

        /// <summary>
        /// S6 回顾页顶部汇总（红蓝积分/存活 + KDA + 比赛时间，载荷 <see cref="LetsPiu.UI.ScoreCardReviewTopSummary"/>）。
        /// 由 Combine/Result 页广播，积分/歼灭两版顶部条各自订阅刷新，页面不再直接填充顶部。
        /// </summary>
        ScoreCardReviewTopSummaryEvent,

        #endregion

        #endregion

        #region 登录相关消息

        /// <summary>
        /// 登录成功事件
        /// </summary>
        LoginSuccess,
        /// <summary>
        /// 请求退出登录事件
        /// </summary>
        RequestLoginOut,
        /// <summary>
        /// 登录token超时
        /// </summary>
        LoginTokenTimeout,
        /// <summary>
        /// App版本太低，需要更新
        /// <para>回调函数参数: <see cref="string"/></para>
        /// </summary>
        NeedUpdateApp,
        /// <summary>
        /// 已退出登录
        /// </summary>
        LoggedOut,

        #endregion

        #region 百度定位相关消息

        /// <summary>
        /// 启动百度定位
        /// </summary>
        StartBdLocation,
        /// <summary>
        /// 关闭百度定位
        /// </summary>
        StopBdLocation,
        /// <summary>
        /// 百度定位启动成功
        /// </summary>
        BdLocationStarted,
        /// <summary>
        /// 百度定位启动失败
        /// </summary>
        BdLocationStartFailed,
        /// <summary>
        /// 百度定位已结束
        /// </summary>
        BdLocationStopped,
        /// <summary>
        /// 百度定位位置改变
        /// <para>回调函数参数: <see cref="LatLng"/></para>
        /// </summary>
        BdLocationChanged,
        /// <summary>
        /// 玩家百度定位位置更新
        /// <para>回调函数参数: <see cref="LatLng"/></para>
        /// </summary>
        PlayerBdLocationChanged,
        ZoomInMap,
        ZoomOutMap,

        #endregion

        #region 资源更新相关消息

        //***************************资源更新相关信息***************************

        /// <summary>
        /// 用户尝试再次初始化资源包
        /// </summary>
        UserTryInitialize,
        /// <summary>
        /// 用户开始下载网络文件
        /// </summary>
        UserBeginDownloadWebFiles,
        /// <summary>
        /// 用户尝试再次请求资源版本
        /// </summary>
        UserTryRequestPackageVersion,
        /// <summary>
        /// 用户尝试再次更新补丁清单
        /// </summary>
        UserTryUpdatePackageManifest,
        /// <summary>
        /// 用户尝试再次下载网络文件
        /// </summary>
        UserTryDownloadWebFiles,

        /// <summary>
        /// 补丁包初始化失败
        /// </summary>
        InitializeFailed,
        /// <summary>
        /// 补丁流程步骤改变
        /// <para>回调函数参数: 
        /// <see cref="string"/>
        /// </para>
        /// </summary>
        PatchStepsChange,
        /// <summary>
        /// 发现更新文件
        /// <para>回调函数参数: 
        /// 下载总数量<see cref="int"/>
        /// 下载总字节数<see cref="long"/>
        /// </para>
        /// </summary>
        FoundUpdateFiles,
        /// <summary>
        /// 下载进度更新
        /// <para>回调函数参数: 
        /// <see cref="YooAsset.DownloadUpdateData"/>
        /// </para>
        /// </summary>
        DownloadUpdate,
        /// <summary>
        /// 资源版本请求失败
        /// </summary>
        PackageVersionRequestFailed,
        /// <summary>
        /// 资源清单更新失败
        /// </summary>
        PackageManifestUpdateFailed,
        /// <summary>
        /// 网络文件下载失败
        /// </summary>
        WebFileDownloadFailed,
        /// <summary>
        /// 设备足够内存
        /// </summary>
        DeviceNoEnoughSpace,
        #endregion

        #region UI相关消息

        //***************************UI相关消息***************************
        /// <summary>
        /// 界面打开
        /// <para>回调函数参数: 
        /// 界面ID（<see cref="int"/>）
        /// 实例ID（<see cref="int"/>）
        /// </para>
        /// </summary>
        OnPanelOpen,
        /// <summary>
        /// 界面关闭
        /// <para>回调函数参数: 
        /// 界面ID（<see cref="int"/>）
        /// 实例ID（<see cref="int"/>）
        /// </para>
        /// </summary>
        OnPanelClose,
        /// <summary>
        /// 页面打开
        /// <para>回调函数参数: 
        /// 界面ID（<see cref="int"/>）
        /// 实例ID（<see cref="int"/>）
        /// </para>
        /// </summary>
        OnOpenPage,
        /// <summary>
        /// 页面顺序发生变化
        /// <para>回调函数参数: 
        /// 实例ID（<see cref="int"/>）
        /// </para>
        /// </summary>
        OnPanelObjectChanged,
        /// <summary>
        /// 经济面板关闭
        /// </summary>
        FightPurchasePanelCloseEvent,
        /// <summary>
        /// 经济面板打开
        /// </summary>
        FightPurchasePanelOpenEvent,
        /// <summary>
        /// 页面Canvas的sortingOrder变化时
        /// </summary>
        OnPageCanvasOrderChanged,

        #endregion

        #region 录音相关消息

        /// <summary>
        /// 录音启动结果
        /// <para>回调函数参数: <see cref="VoiceManagerStartResult"/></para>
        /// </summary>
        VoiceManagerRecordingStartResult,
        /// <summary>
        /// 录音结束
        /// <para>回调函数参数: <see cref="RecordingResult"/></para>
        /// </summary>
        VoiceManagerRecordingEnd,
        /// <summary>
        /// 录音上传结果
        /// <para>回调函数参数: <see cref="FileUploadResult"/></para>
        /// </summary>
        VoiceManagerUploadResult,

        #endregion

        #region 场景跳转相关
        /// <summary>
        /// 场景跳转成功
        /// <para>回调函数参数：场景枚举（<see cref="Define.SceneType"/>）</para>
        /// </summary>
        SceneSwitchSuccess,

        #endregion

        #region 其他设备相关消息

        /// <summary>
        /// 指南针朝向角度变化/持枪状态的指南针角度(正北0度，范围 0~360)
        /// <para>回调函数参数: <see cref="double"/></para>
        /// </summary>
        CompassHeadingChangeEvent,

        #endregion

        #region 游戏内消息

        /// <summary>
        /// Tab键区域按下
        /// </summary>
        OnTabKeyDown,
        /// <summary>
        /// Tab键区域弹起
        /// </summary>
        OnTabKeyUp,
        /// <summary>
        /// Tab键区域向上滚动
        /// <para>回调函数参数dy: <see cref="float"/> </para>
        /// </summary>
        OnTabScroll,
        /// <summary>
        /// 点击了游戏界面中的设置按钮
        /// </summary>
        GameSettingButtonClicked,
        /// <summary>
        /// 高手模式已打开
        /// </summary>
        MasterModeOpened,
        /// <summary>
        /// 高手模式已关闭
        /// </summary>
        MasterModeClosed,
        /// <summary>
        /// 玩家自己死亡
        /// <para>回调函数参数: <see cref="PlayerChangeMessage"/> </para>
        /// </summary>
        MainPlayerDead,
        /// <summary>
        /// 其他玩家死亡
        /// <para>回调函数参数: <see cref="PlayerChangeMessage"/> </para>
        /// </summary>
        OtherPlayerDead,
        /// <summary>
        /// 玩家自己复活
        /// <para>回调函数参数: <see cref="PlayerChangeMessage"/> </para>
        /// </summary>
        MainPlayerRevive,
        /// <summary>
        /// 主玩家子弹变更(需从当前Player中读取)
        /// </summary>
        MainPlayerBulletChanged,
        /// <summary>
        /// 主玩家子弹为空
        /// </summary>
        MainPlayerEmptyMagazine,
        /// <summary>
        /// 主玩家子弹用光了
        /// </summary>
        MainPlayerEmptyBulletPacket,
        /// <summary>
        /// 主玩家状态改变
        /// </summary>
        MainPlayerStatusChangedEvent,
        /// <summary>
        /// 刷新主玩家子弹数据（舔包）
        /// </summary>
        RefreshMainPlayerBulletData,
        /// <summary>
        /// 更新设备射击状态
        /// </summary>
        UpdateDeviceShootState,
        /// <summary>
        /// 设置玩家列表
        /// </summary>
        SetPlayerList,
        /// <summary>
        /// 其他玩家复活
        /// <para>回调函数参数: <see cref="PlayerChangeMessage"/> </para>
        /// </summary>
        OtherPlayerRevive,
        /// <summary>
        /// 其他玩家子弹变更(需从当前Player控制的AI角色中读取)
        /// </summary>
        OtherPlayerBulletChanged,
        /// <summary>
        /// 其他玩家子弹为空
        /// </summary>
        OtherPlayerEmptyMagazine,
        /// <summary>
        /// 其他玩家子弹用光了
        /// </summary>
        OtherPlayerEmptyBulletPacket,
        /// <summary>
        /// 其他玩家状态改变
        /// </summary>
        OtherPlayerStatusChangedEvent,
        /// <summary>
        /// 玩家创建成功
        /// <para>回调函数参数: <see cref="int"/> </para>
        /// </summary>
        OnPlayerCreateSuccess,
        /// <summary>
        /// 主角进入安全区
        /// </summary>
        OnMianPlayerEnterSafeArea,
        /// <summary>
        /// 主角离开安全区
        /// </summary>
        OnMainPlayerExitSafeArea,
        /// <summary>
        /// 准星颜色变化
        /// </summary>
        OnGunAimColorChanged,
        /// <summary>
        /// 更新F键按钮的可见性
        /// </summary>
        UpdateFKeyButtonVisible,
        /// <summary>
        /// 更新场次中玩家的卡牌数量
        /// </summary>
        PlayersCardCountChanged,
        /// <summary>
        /// 5秒倒计时事件
        /// <para>回调函数参数: <see cref="int"/>游戏剩余开始的秒数</para>
        /// </summary>
        FiveSecCountdownEvent,
        /// <summary>
        /// 单人打靶开始游戏
        /// </summary>
        SingleTargetShootingStart,
        /// <summary>
        /// 单人打靶更新积分
        /// </summary>
        SingleTargetShootingUpdateScore,
        #endregion

        #region 道具类相关消息

        /// <summary>
        /// 卡牌数量发生变化
        /// </summary>
        CardInfoChanged,

        /// <summary>
        /// 道具数据发生变化
        /// </summary>
        TacticalInfoChanged,

        /// <summary>
        /// 词条数量发生变化
        /// </summary>
        WeaponAccessoriesChanged,
        #endregion

        #region 大厅相关
        HallModeChoiceData,
        #endregion
    }
}
