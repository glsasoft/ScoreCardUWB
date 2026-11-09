using UnityEngine;

/// <summary>
/// 剧情式天气过渡。挂一个在场景里，持有若干预设，由上层代码调用 TransitionTo 平滑切换。
/// 同一个场景里只保留一个，多开会互相覆盖。
/// </summary>
[DisallowMultipleComponent]
public class AerialFogController : MonoBehaviour
{
    [Tooltip("天气预设，顺序即 TransitionTo 的索引。")]
    public AerialFogPreset[] presets;

    [Tooltip("启动时使用第几个预设。")]
    public int startIndex;

    [Tooltip("TransitionTo 传 -1 时使用的过渡时长（秒）。")]
    public float defaultTransitionDuration = 3f;

    [Header("方向光")]
    [Tooltip("方向光对雾色的影响程度，0 = 只用雾色，1 = 雾色完全乘上主光颜色。这个值随时可调，不受下面的手动覆盖影响。")]
    [Range(0f, 1f)] public float sunInfluence = 0.5f;

    [Header("手动覆盖")]
    [Tooltip("勾上后，下面几个值会盖掉当前预设的对应字段。切预设不会重置它们，方便手动调。")]
    public bool manualOverride;

    [Tooltip("雾开始出现的视距（米）。场景近端的深度。")]
    public float overrideFogStart = 15.5f;

    [Tooltip("雾达到标称密度的视距（米）。场景最远端的深度。必须大于 fogStart。")]
    public float overrideFogEnd = 26f;

    [Tooltip("雾起点的颜色。近处一般带地面反射的暖灰。")]
    public Color overrideColorNear = new Color(0.58f, 0.56f, 0.55f, 1f);

    [Tooltip("雾终点的颜色。远处向天光的冷蓝靠。")]
    public Color overrideColorFar = new Color(0.62f, 0.70f, 0.82f, 1f);

    /// <summary>
    /// 当前生效的控制器。场景里没有时为 null，此时 Feature 用自己的 Inspector 参数。
    /// 编辑器里 static 会随 domain reload 丢失，取不到时惰性查找一次（每帧最多一次）。
    /// </summary>
    public static AerialFogController Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            if (_lastSearchFrame == Time.frameCount)
                return null;

            _lastSearchFrame = Time.frameCount;
            _instance = FindObjectOfType<AerialFogController>();

            // 编辑器里 Awake 不一定跑过，找到后补一次初始化，否则 CurrentParams 全是 0（无雾）。
            if (_instance != null && !_instance._initialized)
                _instance.SnapTo(_instance.startIndex);

            return _instance;
        }
        private set { _instance = value; }
    }

    static AerialFogController _instance;
    static int _lastSearchFrame = -1;

    /// <summary>
    /// 当前生效的参数，Feature 每帧读它。
    /// 勾了 manualOverride 时，这里的 fogStart / fogEnd / 两个颜色会被 Inspector 上的覆盖值替换。
    /// </summary>
    public AerialFogParams CurrentParams
    {
        get
        {
            var p = _current;

            // 方向光影响始终由 Controller 上的滑块决定，不受手动覆盖开关影响，随时可调。
            p.sunInfluence = sunInfluence;

            if (manualOverride)
            {
                p.fogStart = overrideFogStart;
                p.fogEnd = Mathf.Max(overrideFogEnd, overrideFogStart + 0.01f);
                p.colorNear = overrideColorNear;
                p.colorFar = overrideColorFar;
            }

            return p;
        }
    }

    AerialFogParams _current;
    AerialFogParams _from;
    AerialFogParams _to;
    float _elapsed;
    float _duration;
    bool _transitioning;
    bool _initialized;

    void Awake()
    {
        SnapTo(startIndex);
    }

    void OnEnable()
    {
        if (Instance != null && Instance != this)
            Debug.LogWarning("[AerialFog] 场景里有多个 AerialFogController，只有最后启用的那个生效。", this);

        Instance = this;

        // 编辑器里 Awake 不一定跑过，这里补一次，否则 _current 全是 0，画面没雾。
        if (!_initialized)
            SnapTo(startIndex);
    }

    void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

#if UNITY_EDITOR
    // 编辑器里 static 会随 domain reload 丢失，重新加载后把场景里的控制器接回来，
    // 这样非 Play 模式下点预设按钮也能立刻看到画面变化。
    [UnityEditor.InitializeOnLoadMethod]
    static void RestoreInstanceInEditor()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (Instance != null)
                return;

            var found = Object.FindObjectOfType<AerialFogController>();
            if (found == null)
                return;

            Instance = found;
            found.SnapTo(found.startIndex);
        };
    }
#endif

    /// <summary>立刻切到某个预设，不走过渡。</summary>
    public void SnapTo(int index)
    {
        if (!TryGetPreset(index, out var preset))
        {
            _initialized = true;   // 预设没配好也别反复尝试
            return;
        }

        _from = preset.parameters;
        _to = preset.parameters;
        _elapsed = 0f;
        _duration = 0f;
        _transitioning = false;
        _initialized = true;
        _current = preset.parameters;
    }

    /// <summary>在 duration 秒内平滑过渡到某个预设。duration 传 -1 用 defaultTransitionDuration，传 0 等价于 SnapTo。</summary>
    public void TransitionTo(int index, float duration = -1f)
    {
        if (!TryGetPreset(index, out var preset))
            return;

        float d = duration < 0f ? defaultTransitionDuration : duration;
        if (d <= 0f)
        {
            SnapTo(index);
            return;
        }

        // 以「当前插值结果」为新起点：中途改主意不会跳变，连续切换也不会累积漂移。
        _from = CurrentParams;
        _to = preset.parameters;
        _elapsed = 0f;
        _duration = d;
        _transitioning = true;
    }

    void Update()
    {
        if (!_transitioning)
            return;

        _elapsed += Time.deltaTime;

        if (_elapsed >= _duration)
        {
            _current = _to;
            _transitioning = false;
            return;
        }

        float raw = Mathf.Clamp01(_elapsed / _duration);
        float s = raw * raw * (3f - 2f * raw); // smoothstep，两端导数为 0，不会有折角
        _current = AerialFogParams.Lerp(_from, _to, s);
    }

    bool TryGetPreset(int index, out AerialFogPreset preset)
    {
        preset = null;

        if (presets == null || presets.Length == 0)
        {
            Debug.LogWarning("[AerialFog] AerialFogController 还没有配置任何预设。", this);
            return false;
        }

        if (index < 0 || index >= presets.Length)
        {
            Debug.LogWarning($"[AerialFog] 预设索引 {index} 越界，当前共 {presets.Length} 个。", this);
            return false;
        }

        preset = presets[index];
        if (preset == null)
        {
            Debug.LogWarning($"[AerialFog] 预设索引 {index} 是空引用。", this);
            return false;
        }

        return true;
    }
}
