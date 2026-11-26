using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Character.Uwb
{
    /// <summary>
    /// 定位 Mock 模式行为注册表。UwbMockTransport 启动时按
    /// <see cref="ScoreCardBattleMockToggle.ActiveModeType"/> 选取。
    /// </summary>
    public static class UwbMockModeBehaviors
    {
        static readonly Dictionary<string, IUwbMockModeBehavior> s_Map =
            new Dictionary<string, IUwbMockModeBehavior>(8);

        static readonly IUwbMockModeBehavior s_Fallback = new Builtin("0", returnHomeOnDeath: true);

        public static IUwbMockModeBehavior Current { get; private set; } = s_Fallback;

        static UwbMockModeBehaviors()
        {
            // 积分：角点出生 + 死亡回出生点（Transport 默认实现）
            Register(new Builtin("0", returnHomeOnDeath: true));
            // 后续模式示例（未实现前不要 Register，Resolve 会打日志并回退）：
            // Register(new Builtin("1", returnHomeOnDeath: true));  // 歼灭
            // Register(new Builtin("7", returnHomeOnDeath: true));  // 超级队长
            // Register(new Builtin("9", returnHomeOnDeath: false)); // 生化等若规则不同
        }

        public static void Register(IUwbMockModeBehavior behavior)
        {
            if (behavior == null || string.IsNullOrEmpty(behavior.ModeType))
                return;
            s_Map[behavior.ModeType] = behavior;
        }

        /// <summary>按 ActiveModeType 切换当前行为；未知模式回退到积分默认。</summary>
        public static IUwbMockModeBehavior ApplyActiveMode()
        {
            string mode = ScoreCardBattleMockToggle.ActiveModeType;
            if (string.IsNullOrEmpty(mode))
                mode = "0";

            if (!s_Map.TryGetValue(mode, out var behavior))
            {
                Debug.LogWarning(
                    $"[UwbMockModeBehaviors] modeType={mode} 无定位 Mock 行为，回退积分默认");
                behavior = s_Fallback;
            }

            Current = behavior;
            return Current;
        }

        sealed class Builtin : IUwbMockModeBehavior
        {
            public string ModeType { get; }
            public bool ReturnHomeOnDeath { get; }

            public Builtin(string modeType, bool returnHomeOnDeath)
            {
                ModeType = modeType;
                ReturnHomeOnDeath = returnHomeOnDeath;
            }
        }
    }
}
