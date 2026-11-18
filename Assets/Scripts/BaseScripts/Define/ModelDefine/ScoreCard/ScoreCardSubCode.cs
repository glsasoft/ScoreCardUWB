using System;

namespace LetsPiu.Common.Network.Model.ScoreCard
{
    /// <summary>
    /// 比分牌 MQTT <c>msg.category</c> 取值。
    /// </summary>
    public static class ScoreCardCommandCategory
    {
        public const string Page = "PAGE";
        public const string Event = "EVENT";
        /// <summary>服务端语音/横幅类通知（仅播报，EVENT 负责数据刷新）。</summary>
        public const string Notice = "NOTICE";
    }

    /// <summary>
    /// 比分牌 MQTT <c>msg.subCode</c> 常量集合。
    /// 与 <c>Docs/比分牌MQTT对接改造计划.md §6.1</c> 注册表一一对应。
    /// 
    /// 协议侧约定（2026-05-29）：
    /// - 多子页阶段（S2/S6/S7/S8）：只发带 <c>-n</c> 后缀的 subCode；
    /// - EVENT 使用大写蛇形：PLAYER_JOIN / PLAYER_LEAVE / BEGIN_PLAY / END_PLAY；
    /// - 兜底归一化由 <see cref="ScoreCardScreenRegistry.NormalizeSubCode"/> 处理。
    /// </summary>
    public static class ScoreCardSubCode
    {
        // ===== 阶段切屏（PAGE / EVENT）=====
        public const string S0 = "S0";              // 待机广告（与 standby 等价，服务端正式 subCode）
        public const string S1 = "S1";              // 开场动画，单页
        public const string S1_3 = "S1-3";          // 独立播 Loop02 一次后客户端自动切 S2-1（不切 S1 屏）
        public const string S2_1 = "S2-1";          // 房间分队前
        public const string S2_2 = "S2-2";          // 房间分队后
        public const string S2_3 = "S2-3";          // 分队完成教官开始讲解
        public const string S2_4 = "S2-4";          // 安全注意事项（中央玩家列表同 S2-3，两侧按语音逐条刷出）
        public const string S2_5 = "S2-5";          // 安全注意事项快速播放（不播语音，标题和五项同时刷出）
        public const string S2_6 = "S2-6";          // 装备介绍
        public const string BeginPlay = "BEGIN_PLAY";   // S3 倒计时（开始比赛 EVENT）
        public const string S3 = "S3";              // S3 倒计时（开始比赛 PAGE 投屏）
        public const string S4 = "S4";              // 实时比分
        public const string EndPlay = "END_PLAY";   // 比赛结束：S4 上叠加关门+Loop（不切屏）
        public const string S5_1 = "S5-1";          // 联调等价 END_PLAY：S4 上叠加关门+Loop
        public const string S5_2 = "S5-2";          // 结算完成：关 S4 进 S6-1（EVENT）
        public const string S6_1 = "S6-1";          // 回顾：战斗 + 击杀曲线
        public const string S6_2 = "S6-2";          // 回顾：积分曲线 + 合并
        public const string S6_3 = "S6-3";          // 回顾：结果
        public const string S7_1 = "S7-1";          // MVP 小奖
        public const string S7_2 = "S7-2";          // MVP / SVP 大奖
        public const string S8_1 = "S8-1";          // 结算：伤害
        public const string S8_2 = "S8-2";          // 结算：积分
        public const string S8_3 = "S8-3";          // 结算：竞技分 / 段位 / 成就

        /// <summary>
        /// S8-3 成就页开关。成就系统未完成，保持 false：
        /// 投屏消息不切到 S8-3，结算自动连播和 BLE 翻页停在 S8-2。
        /// 成就做好后改为 true，导航列表里的 S8-3 会自动加回来。
        /// </summary>
        public const bool S8_3PageEnabled = false;

        /// <summary>
        /// <see cref="S8_3PageEnabled"/> 为 false 时，从页面序列里去掉 S8-3。
        /// 为 true 时原样返回，方便以后打开成就页。
        /// </summary>
        public static string[] ApplyS83PageGate(string[] pages)
        {
            if (S8_3PageEnabled || pages == null) return pages;

            int count = 0;
            for (int i = 0; i < pages.Length; i++)
            {
                if (!string.Equals(pages[i], S8_3, StringComparison.OrdinalIgnoreCase))
                    count++;
            }

            if (count == pages.Length) return pages;

            var result = new string[count];
            int w = 0;
            for (int i = 0; i < pages.Length; i++)
            {
                if (string.Equals(pages[i], S8_3, StringComparison.OrdinalIgnoreCase)) continue;
                result[w++] = pages[i];
            }

            return result;
        }

        /// <summary>
        /// 竞速靶场 S8 结算屏开关。服务器返回的 S8 数据不全，保持 false：
        /// 竞速靶场下 S8 投屏消息被忽略，结算自动连播和 BLE 翻页停在 S7-1。
        /// 服务器数据完整后改为 true 即可恢复。
        /// </summary>
        public const bool SpeedShootingS8PageEnabled = false;

        public static bool IsS8SubCode(string subCode)
        {
            return string.Equals(subCode, S8_1, StringComparison.OrdinalIgnoreCase)
                || string.Equals(subCode, S8_2, StringComparison.OrdinalIgnoreCase)
                || string.Equals(subCode, S8_3, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// <see cref="SpeedShootingS8PageEnabled"/> 为 false 时，从竞速靶场的页面序列里去掉 S8 各页。
        /// </summary>
        public static string[] ApplySpeedShootingS8PageGate(string[] pages)
        {
            if (SpeedShootingS8PageEnabled || pages == null) return pages;
            return Array.FindAll(pages, p => !IsS8SubCode(p));
        }

        public const string Standby = "standby";    // 待机广告（名称待与后端最终对齐）

        // ===== 非切屏事件 =====
        public const string AutoPlay = "autoPlay";          // 通知当前 Panel 切换自动播放
        public const string PlayerJoin = "PLAYER_JOIN";     // 玩家加入房间，刷数据，不切屏
        public const string PlayerLeave = "PLAYER_LEAVE";   // 玩家离开房间，刷数据，不切屏
        public const string RefreshData = "REFRESH_DATA";   // 全量刷新 playerList，不切屏；S2-1/S2-2 与 S4 客户端处理
        public const string RangeUpdate = "RANGE_UPDATE";   // 竞速靶场回合内数据刷新，不切屏，不播动画
        public const string TurnBegin = "TURN_BEGIN";       // 竞速靶场玩家开始：NOTICE 播开始语音，EVENT 播换人动画
        public const string TurnEnd = "TURN_END";           // 竞速靶场玩家结束：NOTICE 按 endReason 播结果语音，EVENT 播结束动画
        public const string Shoot = "SHOOT";                // 击中/击杀事件：局部更新 shooter/victim 玩家字段
        public const string PlayerRevive = "PLAYER_REVIVE"; // 玩家复活事件：data.player 局部更新玩家字段 + 播放复活消息条
        public const string TowerKill = "TOWER_KILL";       // 玩家被防御靶灯击杀：data.victim 局部更新玩家字段 + 播放防御灯击杀消息条
        public const string MultiKill = "MULTI_KILL";       // 连杀事件：局部更新 shooter + 播放连杀条
        public const string FirstBlood = "FIRST_BLOOD";     // 首杀事件：FirstKill 横幅
        /// <summary>倒计时提醒（原 LAST_30S / LAST_10S 合并）。data.seconds：30=常规最后30秒，10=竞速靶场回合剩10秒。</summary>
        public const string LastSeconds = "LAST_SECONDS";
        /// <summary>兼容旧协议；服务端已改为 <see cref="LastSeconds"/>。</summary>
        public const string Last30s = "LAST_30S";
        public const string RedBuffRefresh  = "RED_BUFF_REFRESH";   // 红 BUFF 刷新（NOTICE 语音）
        public const string BlueBuffRefresh = "BLUE_BUFF_REFRESH";  // 蓝 BUFF 刷新（NOTICE 语音）
        public const string RedBuffCaptured  = "RED_BUFF_CAPTURED";   // 红 BUFF 被夺取（NOTICE 语音）
        public const string BlueBuffCaptured = "BLUE_BUFF_CAPTURED";  // 蓝 BUFF 被夺取（NOTICE 语音）
        public const string LeadChange = "LEAD_CHANGE";         // 领先方变化（NOTICE 语音，teamType 0=红 / 1=蓝；生化模式复用：0=人类 1=丧尸；超级队长模式复用：0/1=队长击杀领先）
        public const string CaptainDown = "CAPTAIN_DOWN";       // 超级队长阵亡（NOTICE 语音，data.teamType 0=红队长/1=蓝队长）
        public const string TeamSurvival = "TEAM_SURVIVAL";     // 歼灭模式残局提示（NOTICE 语音，data.teamType 0=红/1=蓝，data.aliveCount 当前存活数，1/3 才有对应语音）

        // ===== 竞速靶场 NOTICE（纯 wav，无 TTS）=====
        public const string ComboHit = "COMBO_HIT";             // 连击高光：data.comboCount，从 2 连起仅刷新最高时下发
        public const string TargetRemain = "TARGET_REMAIN";     // 剩余靶数：data.remain，仅 7/3/1
        public const string PenaltyPlus = "PENALTY_PLUS";       // 击中罚时靶
        public const string LowHp = "LOW_HP";                   // 生命告急（每回合最多一次）
        public const string LowAmmo = "LOW_AMMO";               // 弹药告急（每回合最多一次）
        public const string RangeShoot = "RANGE_SHOOT";         // 竞速靶场开枪：data.remainBullet
        public const string RangeHit = "RANGE_HIT";             // 竞速靶场命中靶灯：data.hitType（GREEN/RED/PENALTY）
        public const string RangeBeHit = "RANGE_BE_HIT";        // 竞速靶场被靶灯击中：data.blood（死亡为 0）
        public const string ButtonStart = "BUTTON_START";       // 竞速靶场玩家拍下开始按钮：NOTICE 在耳机口播开始音效，EVENT 在比分牌(HDMI)播
        public const string ClearSuccess = "CLEAR_SUCCESS";     // 旧结果码，结果语音已改走 TURN_END
        public const string ChallengeFail = "CHALLENGE_FAIL";   // 旧结果码，结果语音已改走 TURN_END

        public const string WinRate = "WIN_RATE";           // S4 AI 实时胜率预测推送，data.winRateLine 为完整历史数组
        // 生化模式救世者播报（人类剩余≤2 时）为客户端本地判定触发，无服务器事件，详见 ScoreCardFightPanel。
        // S6 结算回顾的数据复用 S6-1 / S6-2 / S6-3 三条切屏 subCode 携带（msg.data 为完整历史数组），无单独 settlement subCode。

        // ===== killxxx 事件占位（业务完整枚举待后端给出）=====
        public const string PlayerDead = "playerDead";      // 占位示例：玩家死亡
    }

    /// <summary>
    /// 阶段聚合键，用于 <c>ScoreCardScreenRegistry.ScreenDescriptor.stageKey</c> 与
    /// <c>CloseCurrentStageAsync</c> 判断「跨阶段离开」与「同阶段换子页」。
    /// </summary>
    public static class ScoreCardStageKey
    {
        public const string S1 = "S1";
        public const string S2 = "S2";
        public const string S3 = "S3";
        public const string S4 = "S4";
        public const string S5 = "S5";
        public const string S6 = "S6";
        public const string S7 = "S7";
        public const string S8 = "S8";
        public const string Ad = "Ad"; // standby
    }
}
