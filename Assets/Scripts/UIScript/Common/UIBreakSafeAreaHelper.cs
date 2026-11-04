using System;
using System.Reflection;
using UnityEngine;

namespace LetsPiu.UI.UIScript.Common
{
    public class UIBreakSafeAreaHelper : MonoBehaviour
    {
        public const int MIN_SCREEN_OFFSET = 24; // 最小安全区偏移，防止圆角遮挡
        // 缓存反射得到的属性访问委托，避免每次调用都走反射开销
        private Func<int> _getOffsetX;
        private Func<int> _getOffsetY;
        private Func<ScreenOrientation> _getOrientation;
        private Func<float> _getScaleFactor; // 读取 UIScreenAdapter.m_RootCanvas.scaleFactor
        private bool _initialized;

        [Header("SafeArea 检测频率")]
        [Tooltip("每秒检测次数 (N)。设置为 0 或以下表示不在 Update 中自动检测，需要手动调用。建议范围：1~10。")]
        [SerializeField] private int checksPerSecond = 4;
        [Tooltip("使用不受 Time.timeScale 影响的时间（例如暂停界面也继续检测）")]
        [SerializeField] private bool useUnscaledTime = true;
        private float _nextCheckTime;

        // 上一次记录的值，用于变更检测
        private int _lastOffsetX;
        private int _lastOffsetY;
        private ScreenOrientation _lastOrientation;
        private bool _hasSnapshot; // 避免首次就触发
        private float _lastScaleFactor; // 上一次的 Canvas scaleFactor
        private bool _initialFired; // 是否已执行过初次事件派发

        [Header("生命周期事件控制")]
        [Tooltip("重新启用(OnEnable)时是否检测一次变化并可能触发事件（已完成初始触发后才生效）")]
        [SerializeField] private bool fireEventOnEnable = true;

        /// <summary>
        /// 当检测到 UIScreenAdapter 真正刷新（即遍历 panelRoot 设置 offset）所对应的状态变化时触发。
        /// 参数：offsetX, offsetY, orientation
        /// 非侵入式：不修改 UIScreenAdapter 脚本，通过轮询静态属性变化判断。
        /// </summary>
        public static event Action<int, int, ScreenOrientation> OnSafeAreaRefreshed;

        private void Start()
        {
            InitUIScreenAdapter();
            FireInitialIfNeeded();
        }

        private void OnEnable()
        {
            if (!fireEventOnEnable) return;
            if (!_initialFired) return; // 还没执行初次事件，等待 Start
            if (!_initialized) InitUIScreenAdapter();
            if (!_initialized) return;

            // 已有基线 -> 检测是否有变化
            if (_hasSnapshot)
            {
                CheckAndDispatchIfChanged();
            }
            else
            {
                // 理论上不应出现（Start 已建立），做防御
                FireInitialIfNeeded();
            }
        }

        private void FireInitialIfNeeded()
        {
            if (!_initialized || _initialFired) return;
            int curX = GetOffsetX();
            int curY = GetOffsetY();
            ScreenOrientation curOrientation = GetOrientation();
            _lastOffsetX = curX;
            _lastOffsetY = curY;
            _lastOrientation = curOrientation;
            _hasSnapshot = true;
            _initialFired = true;
            try
            {
                OnSafeAreaRefreshed?.Invoke(curX, curY, curOrientation);
            }
            catch (Exception e)
            {
                Debug.LogWarning("UIBreakSafeAreaHelper 初始触发 OnSafeAreaRefreshed 异常: " + e);
            }
        }

        public void InitUIScreenAdapter()
        {
            //通过反射获取UIScreenAdapter类的offsetX 和 offsetY 方法
            if (_initialized) return;

            try
            {
                // UIScreenAdapter处于命名空间 LetsPiu，根据源码定义进行获取
                var assembly = typeof(UIBreakSafeAreaHelper).Assembly; // 当前程序集（在Unity中通常可以直接通过Type获取）
                var screenAdapterType = assembly.GetType("LetsPiu.UIScreenAdapter");
                if (screenAdapterType == null)
                {
                    // 备用方案：遍历所有已加载程序集查找（防止类不在同一程序集）
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        screenAdapterType = asm.GetType("LetsPiu.UIScreenAdapter");
                        if (screenAdapterType != null) break;
                    }
                }

                if (screenAdapterType == null)
                {
                    Debug.LogWarning("UIBreakSafeAreaHelper 未找到类型 LetsPiu.UIScreenAdapter，无法获取安全区偏移");
                    return;
                }

                // 获取静态属性 offsetX / offsetY / orientation
                var propOffsetX = screenAdapterType.GetProperty("offsetX", BindingFlags.Public | BindingFlags.Static);
                var propOffsetY = screenAdapterType.GetProperty("offsetY", BindingFlags.Public | BindingFlags.Static);
                var propOrientation = screenAdapterType.GetProperty("orientation", BindingFlags.Public | BindingFlags.Static);
                // 非静态实例字段 m_RootCanvas 需要获取实例
                var fieldRootCanvas = screenAdapterType.GetField("m_RootCanvas", BindingFlags.NonPublic | BindingFlags.Instance);

                if (propOffsetX == null || propOffsetY == null)
                {
                    Debug.LogWarning("UIBreakSafeAreaHelper 未找到 UIScreenAdapter.offsetX / offsetY 属性");
                    return;
                }

                if (propOrientation == null)
                {
                    Debug.LogWarning("UIBreakSafeAreaHelper 未找到 UIScreenAdapter.orientation 属性，将仅监听偏移变化");
                }

                // 创建委托 (int) -> 直接调用GetValue
                _getOffsetX = () =>
                {
                    object v = propOffsetX.GetValue(null, null);
                    return v is int iv ? iv : 0;
                };
                _getOffsetY = () =>
                {
                    object v = propOffsetY.GetValue(null, null);
                    return v is int iv ? iv : 0;
                };
                if (propOrientation != null)
                {
                    _getOrientation = () =>
                    {
                        object v = propOrientation.GetValue(null, null);
                        return v is ScreenOrientation so ? so : Screen.orientation; // 兜底当前屏幕方向
                    };
                }

                // 寻找 UIScreenAdapter 实例以便获取 m_RootCanvas.scaleFactor
                if (fieldRootCanvas != null)
                {
                    try
                    {
                        object instance = null;
                        var go = GameObject.Find("UIScreenAdapter");
                        if (go != null)
                        {
                            var comp = go.GetComponent(screenAdapterType);
                            if (comp != null) instance = comp;
                        }
                        if (instance == null)
                        {
                            // 兜底查找任意实例
                            var comps = GameObject.FindObjectsOfType(typeof(MonoBehaviour));
                            foreach (var c in comps)
                            {
                                if (c != null && c.GetType() == screenAdapterType)
                                {
                                    instance = c;
                                    break;
                                }
                            }
                        }

                        if (instance != null)
                        {
                            _getScaleFactor = () =>
                            {
                                try
                                {
                                    var canvas = fieldRootCanvas.GetValue(instance) as Canvas;
                                    return canvas != null ? canvas.scaleFactor : 1f;
                                }
                                catch
                                {
                                    return 1f;
                                }
                            };
                        }
                        else
                        {
                            Debug.LogWarning("UIBreakSafeAreaHelper 未找到 UIScreenAdapter 实例，无法读取 m_RootCanvas.scaleFactor");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("UIBreakSafeAreaHelper 反射获取 m_RootCanvas.scaleFactor 失败: " + e);
                    }
                }

                _initialized = true;
                Debug.Log("UIBreakSafeAreaHelper 初始化成功 (反射) offsetX/offsetY");
            }
            catch (Exception e)
            {
                Debug.LogWarning("UIBreakSafeAreaHelper 反射获取 UIScreenAdapter offsetX/offsetY 失败: " + e);
            }
        }

        /// <summary>
        /// 获取当前屏幕安全区 X 方向偏移
        /// </summary>
        public int GetOffsetX()
        {
            if (!_initialized)
            {
                InitUIScreenAdapter();
            }
            return _getOffsetX != null ? _getOffsetX() : 0;
        }

        /// <summary>
        /// 获取当前屏幕安全区 Y 方向偏移
        /// </summary>
        public int GetOffsetY()
        {
            if (!_initialized)
            {
                InitUIScreenAdapter();
            }
            return _getOffsetY != null ? _getOffsetY() : 0;
        }

        /// <summary>
        /// 获取当前屏幕方向（使用 UIScreenAdapter.orientation 反射结果；若未获取到则回退 Screen.orientation）
        /// </summary>
        public ScreenOrientation GetOrientation()
        {
            if (!_initialized)
            {
                InitUIScreenAdapter();
            }
            return _getOrientation != null ? _getOrientation() : Screen.orientation;
        }

        /// <summary>
        /// 强制建立当前快照（不触发事件）。
        /// 用途：初始化或外部重置基准线。
        /// </summary>
        public void ForceSnapshot()
        {
            if (!_initialized) InitUIScreenAdapter();
            int curX = GetOffsetX();
            int curY = GetOffsetY();
            ScreenOrientation curOrientation = _getOrientation != null ? _getOrientation() : Screen.orientation;
            float curScale = _getScaleFactor != null ? _getScaleFactor() : 1f;
            _lastOffsetX = curX;
            _lastOffsetY = curY;
            _lastOrientation = curOrientation;
            _lastScaleFactor = curScale;
            _hasSnapshot = true;
        }

        /// <summary>
        /// 手动检测是否发生变化；如果发生变化则触发 OnSafeAreaRefreshed 事件。
        /// 返回值：true = 有变化并已触发事件；false = 无变化或尚未初始化。
        /// 使用场景：在布局重建、场景切换、屏幕旋转回调或用户主动触发的时机调用，避免每帧轮询。
        /// </summary>
        public bool CheckAndDispatchIfChanged()
        {
            if (!_initialized)
            {
                InitUIScreenAdapter();
                if (!_initialized)
                    return false; // 仍未成功初始化
            }

            int curX = _getOffsetX != null ? _getOffsetX() : 0;
            int curY = _getOffsetY != null ? _getOffsetY() : 0;
            ScreenOrientation curOrientation = _getOrientation != null ? _getOrientation() : Screen.orientation;
            float curScale = _getScaleFactor != null ? _getScaleFactor() : 1f;

            if (!_hasSnapshot)
            {
                // 第一次建立基线，不触发
                _lastOffsetX = curX;
                _lastOffsetY = curY;
                _lastOrientation = curOrientation;
                _lastScaleFactor = curScale;
                _hasSnapshot = true;
                return false;
            }

            // 增加 Canvas scaleFactor 判断，与 UIScreenAdapter 内部刷新条件保持一致
            bool scaleChanged = Math.Abs(curScale - _lastScaleFactor) > 0.001f;
            if (curX != _lastOffsetX || curY != _lastOffsetY || curOrientation != _lastOrientation || scaleChanged)
            {
                _lastOffsetX = curX;
                _lastOffsetY = curY;
                _lastOrientation = curOrientation;
                _lastScaleFactor = curScale;
                try
                {
                    Debug.Log($"UIBreakSafeAreaHelper (CompactWebview) 检测到安全区变化，触发 OnSafeAreaRefreshed 事件 offsetX:{curX} offsetY:{curY} orientation:{curOrientation} scaleFactor:{curScale}");
                    OnSafeAreaRefreshed?.Invoke(curX, curY, curOrientation);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("UIBreakSafeAreaHelper OnSafeAreaRefreshed 调用监听异常: " + e);
                }
                return true;
            }
            return false;
        }

        public bool IsInitialized()
        {
            return _initialized;
        }

        private void Update()
        {
            if (checksPerSecond <= 0) return; // 不自动检测
            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            if (now < _nextCheckTime) return;

            // 计算下一次检测时间（避免累计误差 用 while 保证补偿落后帧）
            float interval = 1f / Mathf.Max(1, checksPerSecond);
            while (now >= _nextCheckTime)
            {
                _nextCheckTime = (_nextCheckTime == 0f ? now : _nextCheckTime) + interval;
            }

            CheckAndDispatchIfChanged();
        }
        
        public float GetScaleFactor()
        {
            if (!_initialized)
            {
                InitUIScreenAdapter();
            }
            return _getScaleFactor != null ? _getScaleFactor() : 1f;
        }
    }
}