using System;

namespace LetsPiu.UIDefine
{
    public enum PanelSortLayer : byte
    {
        Auto,           // 自动叠加
    }

    public enum PanelHideMask : byte
    {
        None = 0,                   //不遮挡
        UnInteractive = 1 << 0,   //隐藏交互
        Hide = 1 << 1,   //隐藏下层
    }

    public enum UIID
    {
        NONE = 0,//无效值
        UIDevPanel,
        LoginPanel,
        // 通用输入对话框面板
        InputFieldDialogPanel,
        AppUpdatePanel,
        /// <summary>
        /// 注册5S设备面板
        /// </summary>
        RegisterDevicePanel,
        #region 游戏大厅面板
        /// <summary>
        /// 首页面板
        /// </summary>
        HomePagePanel,
        /// <summary>
        /// 线下模式Page
        /// </summary>
        OfflinePage,
        #endregion
        #region 设置面板

        /// <summary>
        /// 设置面板
        /// </summary>
        SettingPanel,
        /// <summary>
        /// 设置面板->基础设置页
        /// </summary>
        SettingBasicPage,

        #endregion
        
        #region 加载面板

        /// <summary>
        /// 默认加载面板
        /// </summary>
        DefaultLoadingPanel,

        #endregion

        #region 通用面板

        /// <summary>
        /// Toast面板
        /// </summary>
        ToastPanel,

        /// <summary>
        /// 扫码面板
        /// </summary>
        ScanQrCodePanel,
        
        #endregion

        /// <summary>
        /// 公共弹窗面板
        /// </summary>
        DialogManagerPanel,

        #region 比分牌投屏面板（S1~S8 + 待机）

        /// <summary>S1：开场动画</summary>
        ScoreCardStartScreenPanel,
        /// <summary>S2：房间（分队前/分队后/视频）共用同 Panel，由 Page 区分</summary>
        ScoreCardRoomPanel,
        /// <summary>S3：倒计时（Panel 待建）</summary>
        ScoreCardCountdownPanel,
        ScoreCardCountdownSpeedShootingPanel,
        /// <summary>S4：实时比分面板</summary>
        ScoreCardFightPanel,
        /// <summary>已废弃（原 S5 WaitScreen），保留枚举槽位避免序列化错位</summary>
        ScoreCardWaitScreenPanel,
        /// <summary>S6：回顾面板</summary>
        ScoreCardReviewPanel,
        /// <summary>S7：MVP 颁奖面板</summary>
        ScoreCardMvpPanel,
        /// <summary>S8：结算面板</summary>
        ScoreCardSettlementPanel,
        /// <summary>待机广告面板（Panel 待建；subCode 名称暂定 standby）</summary>
        ScoreCardStandbyPanel,

        // S4 子页（按 modeType 选择中央 NvN 页，左右各挂一张）
        ScoreCardFightPage1v1,
        ScoreCardFightPage5v5,
        ScoreCardFightPage10v10,
        ScoreCardFightPage20v20,
        ScoreCardWebCamPage,
        ScoreCardWinRatePage,
        ScoreCardKillPage,
        ScoreCardKillPageZombieMode,
        ScoreCardFightPageZombieMode5v5,
        ScoreCardFightPageZombieMode10v10,
        ScoreCardFightPageZombieMode20v20,

        // S6 子页
        ScoreCardReviewFightPage,
        ScoreCardReviewKillCurvePage,
        ScoreCardReviewScoreCurvePage,
        ScoreCardReviewSurvivalTimelinePage,
        ScoreCardReviewCombinePage,
        ScoreCardReviewResultPage,
        ScoreCardReviewResultZombiePage,

        // S7 子页
        ScoreCardMvpSmallPrizePage,
        ScoreCardMvpBigPrizePage,
        ScoreCardMvpBigPrizeAnnihilationPage,
        ScoreCardMvpBigPrizeCaptainPage,
        ScoreCardMvpBigPrizeZombiePage,

        // S8 子页
        ScoreCardSettlementDamagePage,
        ScoreCardSettlementScorePage,
        ScoreCardSettlementRankPage,

        // S2 子页（每条 subCode 单 Page，挂到 ScoreCardRoomPanel.contentRect）
        ScoreCardRoomEntryPage,
        ScoreCardRoomTeamPage,
        SuperCaptainRoomTeamPage,
        ZombieRoomTeamPage,

        // ===== 歼灭模式(_Annihilation) 副本页（枚举名 == 预制体名；由 ScoreCardModeRegistry 按 modeType 选择）=====
        // 复用积分模式脚本，仅预制体不同（Top/特效/列布局后续替换）；追加在末尾避免序列化错位。
        /// <summary>已废弃：S4 中央战斗页已并入同一预制体（Top 内嵌），不再用副本页。保留枚举槽位避免序列化错位。</summary>
        ScoreCardFightPage1v1_Annihilation,
        /// <summary>已废弃：同上。</summary>
        ScoreCardFightPage5v5_Annihilation,
        /// <summary>已废弃：同上。</summary>
        ScoreCardFightPage10v10_Annihilation,
        /// <summary>已废弃：同上。</summary>
        ScoreCardWebCamPage_Annihilation,
        /// <summary>已废弃：S6-1 战斗回顾页已并入同一预制体（内部按 modeType 判断），且从未配套过对应 prefab。保留枚举槽位避免序列化错位。</summary>
        ScoreCardReviewFightPage_Annihilation,
        ScoreCardReviewScoreCurvePage_Annihilation,
        ScoreCardReviewCombinePage_Annihilation,
        ScoreCardReviewResultPage_Annihilation,
        ScoreCardSettlementDamagePage_Annihilation,
        ScoreCardSettlementScorePage_Annihilation,
        ScoreCardSettlementRankPage_Annihilation,
        ScoreCardReviewResultSuperCaptainPage,
        ScoreCardReviewCaptainKillTimelinePage,

        // 竞速靶场模式：S2 全员列表/战役讲解共用页；追加在末尾避免序列化错位。
        SpeedShootingTeamPage,
        // 竞速靶场模式：S4 战斗主页；追加在末尾避免序列化错位。
        ScoreCardFightPageSpeedShooting,
        // 竞速靶场模式：S4 摄像头页（白边框版，复用 ScoreCardWebCamPage 脚本）；追加在末尾避免序列化错位。
        ScoreCardWebCamPageWhiteEdge,
        // 竞速靶场模式：S4 胜率预测页（独立布局，不走 WIN_RATE 曲线协议）；追加在末尾避免序列化错位。
        ScoreCardWinRatePageSpeedShooting,
        // 竞速靶场模式：S6 回顾面板。S6-3 显示内置雷达页；追加在末尾避免序列化错位。
        ScoreCardReviewPanelSpeedShooting,
        // 竞速靶场模式：S6-1 战绩结果页；追加在末尾避免序列化错位。
        ScoreCardReviewResultSpeedShooting,
        // 竞速靶场模式：S7-1/S7-2 共用同一页（MVP/SVP + 两个小奖 + 全员滚动列表）；追加在末尾避免序列化错位。
        ScoreCardMvpSpeedShootingPage,
        // 竞速靶场模式：S8-1/S8-2/S8-3 共用同一结算页；追加在末尾避免序列化错位。
        ScoreCardSettlementSpeedShootingPage,
        // S2-6 装备介绍页；追加在末尾避免序列化错位。
        ScoreCardDeviceIntroPage,
        // S2-4 安全须知页；追加在末尾避免序列化错位。
        ScoreCardSafetyInstructionsPage,

        /// <summary>已废弃：20v20 改为 PlayerItemNoScore20v20 变体，不再用整页副本。保留枚举槽位避免序列化错位。</summary>
        ScoreCardFightPageNoScore20v20,

        #endregion

        // 内置面板起始值
        InnerPageBegin = 3072,
    }

    public static class UISortIndex
    {
        public const int LOWER_BOTTOM = 0;  // 低于bottom
        public const int LOADING_PANEL = 10000000;
        public const int SYSTEM_MSGBOX = 20000000;
        public const int TOP_PANEL = 30000000;
    }

    [Serializable]
    public struct UIPrepareResData
    {
        public string address;
        public ResType type;
    }
}
