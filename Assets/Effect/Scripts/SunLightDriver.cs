using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GLSA.FX
{
    /// <summary>
    /// 太阳光驱动器，替代原来那个空的 Animator（Sun.controller：层权重 0、无 motion、无参数）。
    ///
    /// 管三件事：
    ///   1. 按固定角速度缓慢旋转方向光（最普通的「每秒多少度」）
    ///   2. 按关键点跑一个加速的昼夜循环：色温（K）和强度随「几点」变化
    ///   3. 漂移 URP 的 Light Cookie，做出云影扫过地面的效果
    ///
    /// 前置条件：URP Asset 的 Lighting → Light Cookies 必须勾上，
    /// 否则 URP 连 _LIGHT_COOKIES keyword 都不会设置，cookie 一律不生效。
    ///
    /// 关于昼夜循环：
    ///   不是读系统时间，是拿 realtime 自己累加「小时」，所以一天是几秒过完由
    ///   cycleLengthSeconds 决定。写场景时想定在傍晚，把 startHour 设在 17.5 就行。
    ///
    /// 关于 GI：
    ///   编辑模式下只要每帧修改 Transform / 光组件，场景就会一直「已修改」，
    ///   Unity 随之反复跑 Global Illumination Preprocessing，烘焙也会被不断作废。
    ///   所以默认编辑模式完全静止，只在 Play 模式驱动；要在编辑器里看效果，
    ///   用组件右键菜单 "Preview: Step" 手动推一步，推完就停，不会持续触发 GI。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    [AddComponentMenu("FX/Sun Light Driver")]
    public sealed class SunLightDriver : MonoBehaviour
    {
        /// <summary>昼夜曲线上的一个关键点：第几点、多少开尔文、多亮。</summary>
        [Serializable]
        public struct DayNightKey
        {
            [Range(0f, 24f)] public float hour;

            [Tooltip("黑体色温（K）：1800 左右是橙红落日，5500-6500 是正午白，8000+ 偏青蓝的夜晚。")]
            [Range(1000f, 20000f)] public float kelvin;

            [Tooltip("方向光强度（Intensity）。夜里别给 0，留一点才有月光的方向感。")]
            [Min(0f)] public float intensity;

            public DayNightKey(float hour, float kelvin, float intensity)
            {
                this.hour = hour;
                this.kelvin = kelvin;
                this.intensity = intensity;
            }

            /// <summary>一条默认的昼夜曲线：夜蓝 → 日出橙 → 正午白 → 黄昏暖 → 夜里压暗。</summary>
            public static DayNightKey[] Defaults()
            {
                return new[]
                {
                    new DayNightKey(0f, 9000f, 0.05f),    // 零点，月光青蓝
                    new DayNightKey(4.5f, 7500f, 0.08f),  // 破晓前
                    new DayNightKey(6.5f, 2600f, 0.85f),  // 日出，橙红
                    new DayNightKey(9f, 5200f, 1.6f),     // 上午，微暖
                    new DayNightKey(12f, 6300f, 2f),      // 正午，偏白
                    new DayNightKey(15.5f, 5600f, 1.7f),  // 午后
                    new DayNightKey(18f, 2400f, 1f),      // 黄昏，落日橙
                    new DayNightKey(19.5f, 3800f, 0.25f), // 暮光
                    new DayNightKey(21.5f, 8000f, 0.07f), // 入夜
                };
            }
        }

        [Header("昼夜色温循环")]
        [Tooltip("按下面的关键点驱动方向光的色温与强度，跑一个加速的昼夜循环。")]
        public bool driveDayNightCycle = true;

        [Tooltip("一整天过完要几秒。240 = 四分钟走完 24 小时。")]
        [Min(0.01f)] public float cycleLengthSeconds = 240f;

        [Tooltip("起始时刻（小时，0-24）。想开在傍晚就填 17.5。")]
        [Range(0f, 24f)] public float startHour = 8f;

        [Tooltip("勾上则连 Intensity 一起驱动；关掉就只改色温，亮度保持一个固定氛围。")]
        public bool driveIntensity = true;

        [Tooltip("按 hour 升序排的关键点，首尾会自动绕回 24 点接着插值。")]
        public DayNightKey[] dayNightKeys = DayNightKey.Defaults();

        [Header("太阳旋转")]
        [Tooltip("每秒旋转的欧拉角（度/秒）：X 俯仰、Y 绕行、Z 滚转。")]
        public Vector3 rotationPerSecond = new Vector3(0f, 1f, 0f);

        [Tooltip("Self = 绕自身轴转；World = 绕世界轴转（更像真实的日间环绕）。")]
        public Space rotationSpace = Space.Self;

        [Header("云影漂移")]
        [Tooltip("每秒走多少张图（与 cookie size 无关，0.006 ≈ 166 秒走完一张）。")]
        public Vector2 cookieDriftPerSecond = new Vector2(0.006f, 0.0025f);

        [Tooltip("勾上后由脚本接管 Light Cookie Size，否则用手调的 Inspector 值。")]
        public bool driveCookieSize = false;

        [Tooltip("云影在世界里的尺度（URP 的 Light Cookie Size）。越大云团越大。")]
        public float cookieSize = 80f;

        [Header("编辑器")]
        [Tooltip("非播放模式是否也持续动。开着会让场景一直 dirty、持续触发 GI 预处理，默认关闭。")]
        public bool previewInEditMode = false;

        [Tooltip("右键菜单 \"Preview: Step\" 每次推进的秒数。")]
        public float previewStepSeconds = 2f;

        Light _light;
        UniversalAdditionalLightData _cookieData;
        Vector2 _cookieOffset;
        float _lastTime;

        /// <summary>循环当前跑到第几个小时（0-24），别的表现脚本可以读它做联动。</summary>
        public float CurrentHour => _hour;

        float _hour;

        void OnEnable()
        {
            Cache();
            _lastTime = Time.realtimeSinceStartup;
            _hour = Mathf.Repeat(startHour, 24f);
        }

        /// <summary>老场景里这个组件早就存在，新加的关键点数组不会自动补，这里兜一层默认曲线。</summary>
        void OnValidate()
        {
            if (dayNightKeys == null || dayNightKeys.Length == 0)
                dayNightKeys = DayNightKey.Defaults();
        }

        /// <summary>只取引用，首次取到时光组件时同步一次 cookie offset，之后不再覆盖。</summary>
        void Cache()
        {
            if (_light == null)
                _light = GetComponent<Light>();

            if (_cookieData == null)
            {
                _cookieData = GetComponent<UniversalAdditionalLightData>();
                if (_cookieData != null)
                    _cookieOffset = _cookieData.lightCookieOffset;
            }
        }

        void Update()
        {
            if (!Application.isPlaying && !previewInEditMode)
                return;

            float now = Time.realtimeSinceStartup;
            float dt = now - _lastTime;
            _lastTime = now;
            if (dt > 0f)
                Step(dt);
        }

        void Step(float dt)
        {
            Cache();

            if (_light == null || _light.type != LightType.Directional)
                return;

            if (rotationPerSecond != Vector3.zero)
                transform.Rotate(rotationPerSecond * dt, rotationSpace);

            // 色温放在 cookie 的 return 之前：没配 cookie 也要能跑昼夜。
            StepDayNight(dt);

            if (_cookieData == null || _light.cookie == null)
                return;

            // URP 的换算（LightCookieManager.GetLightUVScaleOffset）：
            //   uvScale  = 1 / lightCookieSize
            //   uv       = (projPos - lightCookieOffset) * uvScale
            // 也就是说 offset 走满 size，贴图才正好平移一整张。
            // 所以循环周期是 size 而不是 1 —— 写死 1 的话，size=2 时走半张就绕回去了。
            Vector2 size = _cookieData.lightCookieSize;
            if (size.x <= 0.0001f) size.x = 1f;
            if (size.y <= 0.0001f) size.y = 1f;

            if (cookieDriftPerSecond != Vector2.zero)
            {
                // cookieDriftPerSecond 按「每秒走多少张图」解释，与 size 解耦：
                // 乘上 size 换回 offset 单位，视觉速度在任何 size 下都一样。
                _cookieOffset.x += cookieDriftPerSecond.x * size.x * dt;
                _cookieOffset.y += cookieDriftPerSecond.y * size.y * dt;
                _cookieOffset.x = Mathf.Repeat(_cookieOffset.x, size.x);
                _cookieOffset.y = Mathf.Repeat(_cookieOffset.y, size.y);
                _cookieData.lightCookieOffset = _cookieOffset;
            }

            if (driveCookieSize && cookieSize > 0f)
            {
                _cookieData.lightCookieSize = new Vector2(cookieSize, cookieSize);
                _cookieOffset.x = Mathf.Repeat(_cookieOffset.x, cookieSize);
                _cookieOffset.y = Mathf.Repeat(_cookieOffset.y, cookieSize);
            }
        }

        /// <summary>推进循环时间，把色温/强度写到光组件上。</summary>
        void StepDayNight(float dt)
        {
            if (!driveDayNightCycle)
                return;

            if (cycleLengthSeconds > 0f)
            {
                _hour += dt / cycleLengthSeconds * 24f;
                _hour = Mathf.Repeat(_hour, 24f);
            }

            SampleCycle(_hour, out float kelvin, out float intensity);

            // useColorTemperature 关着的时候 colorTemperature 完全不参与计算，必须一起打开。
            _light.useColorTemperature = true;
            _light.colorTemperature = kelvin;

            if (driveIntensity)
                _light.intensity = intensity;
        }

        /// <summary>按小时在关键点之间插值，跨过 24 点会绕回 0 点继续。</summary>
        void SampleCycle(float hour, out float kelvin, out float intensity)
        {
            kelvin = _light.colorTemperature;
            intensity = _light.intensity;

            var keys = dayNightKeys;
            if (keys == null || keys.Length == 0)
                keys = DayNightKey.Defaults();

            if (keys.Length == 1)
            {
                kelvin = keys[0].kelvin;
                intensity = keys[0].intensity;
                return;
            }

            // 找到 hour 落在哪两个点之间。比第一个点还早、比最后一个点还晚，都走首尾回绕那一段。
            int upper = 0;
            while (upper < keys.Length && keys[upper].hour <= hour)
                upper++;

            int a, b;
            float ha, hb, hh;
            if (upper == 0 || upper >= keys.Length)
            {
                a = keys.Length - 1;
                b = 0;
                ha = keys[a].hour;
                hb = keys[b].hour + 24f;
                hh = upper == 0 ? hour + 24f : hour;
            }
            else
            {
                a = upper - 1;
                b = upper;
                ha = keys[a].hour;
                hb = keys[b].hour;
                hh = hour;
            }

            float t = hb > ha ? Mathf.Clamp01((hh - ha) / (hb - ha)) : 0f;
            kelvin = Mathf.Lerp(keys[a].kelvin, keys[b].kelvin, t);
            intensity = Mathf.Lerp(keys[a].intensity, keys[b].intensity, t);
        }

        /// <summary>编辑器里手动推进一步，看清云影和光照往哪边走。推完就停，不持续触发 GI。</summary>
        [ContextMenu("Preview: Step")]
        void PreviewStep()
        {
            Step(Mathf.Max(0.01f, previewStepSeconds));
        }

        /// <summary>把循环时刻拉回 startHour 并立刻套用一次，方便反复看同一个时刻。</summary>
        [ContextMenu("Preview: Reset Time")]
        void ResetTime()
        {
            _hour = Mathf.Repeat(startHour, 24f);
            if (_light == null)
                _light = GetComponent<Light>();
            if (_light != null && _light.type == LightType.Directional)
                StepDayNight(0f);
        }

        [ContextMenu("Preview: Reset Cookie Offset")]
        void ResetCookieOffset()
        {
            Cache();
            if (_cookieData == null)
                return;
            _cookieOffset = Vector2.zero;
            _cookieData.lightCookieOffset = Vector2.zero;
        }

        void Reset()
        {
            driveDayNightCycle = true;
            cycleLengthSeconds = 240f;
            startHour = 8f;
            driveIntensity = true;
            dayNightKeys = DayNightKey.Defaults();
            rotationPerSecond = new Vector3(0f, 1f, 0f);
            cookieDriftPerSecond = new Vector2(0.006f, 0.0025f);
            cookieSize = 80f;
            previewInEditMode = false;
        }
    }
}
