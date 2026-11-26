using System;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 定位 Mock 与比分牌战斗 Mock 的统一启停桥接（Character 不能直接引用 LetsPiu.UI）。
    /// 由 UwbRuntime 在进入/离开 Mock 时调用 <see cref="Apply"/>；UI 侧 Host 按
    /// <see cref="ActiveModeType"/> 选择具体模式 Mock。
    /// </summary>
    public static class ScoreCardBattleMockToggle
    {
        /// <summary>
        /// 当前绑定的比分牌模式（modeType 字符串，与 ScoreCardGameMode 一致）。
        /// 默认积分 "0"；后续切歼灭/生化等时改此值即可，无需改 UwbRuntime。
        /// </summary>
        public static string ActiveModeType = "0";

        public static Func<bool> IsEnabled;
        public static Action<bool> SetEnabled;

        static bool? s_Pending;

        /// <summary>与定位 Mock 绑定：开定位 Mock 则开当前模式的比分牌 Mock，反之亦然。</summary>
        public static void Apply(bool enabled)
        {
            if (SetEnabled != null)
            {
                SetEnabled(enabled);
                s_Pending = null;
                return;
            }

            s_Pending = enabled;
        }

        /// <summary>UI 模块注册完回调后补上未消费的开关请求。</summary>
        public static void FlushPending()
        {
            if (!s_Pending.HasValue || SetEnabled == null)
                return;
            bool on = s_Pending.Value;
            s_Pending = null;
            SetEnabled(on);
        }
    }
}
