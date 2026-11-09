using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GLSA.FX
{
    /// <summary>
    /// 太阳光驱动器，替代原来那个空的 Animator（Sun.controller：层权重 0、无 motion、无参数）。
    ///
    /// 管两件事：
    ///   1. 按固定角速度缓慢旋转方向光（最普通的「每秒多少度」）
    ///   2. 漂移 URP 的 Light Cookie，做出云影扫过地面的效果
    ///
    /// 前置条件：URP Asset 的 Lighting → Light Cookies 必须勾上，
    /// 否则 URP 连 _LIGHT_COOKIES keyword 都不会设置，cookie 一律不生效。
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

        void OnEnable()
        {
            Cache();
            _lastTime = Time.realtimeSinceStartup;
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

        /// <summary>编辑器里手动推进一步，看清云影和光照往哪边走。推完就停，不持续触发 GI。</summary>
        [ContextMenu("Preview: Step")]
        void PreviewStep()
        {
            Step(Mathf.Max(0.01f, previewStepSeconds));
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
            rotationPerSecond = new Vector3(0f, 1f, 0f);
            cookieDriftPerSecond = new Vector2(0.006f, 0.0025f);
            cookieSize = 80f;
            previewInEditMode = false;
        }
    }
}
