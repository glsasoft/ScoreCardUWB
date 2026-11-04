using UnityEngine;

namespace LetsPiu.Component
{
    /// <summary>
    /// 位置、旋转、缩放随机抖动：隔一段时间抽一个目标，再用 SmoothDamp 追过去。
    /// 动画直接 K <see cref="weight"/> 和三个幅度。位移动画放父节点，本组件挂子节点。
    /// </summary>
    public class RandomJitter : MonoBehaviour
    {
        [Tooltip("启用时开始循环抖动")]
        [SerializeField] bool playOnEnable = true;

        [Tooltip("动画可直接 K。0 回到初始姿态，1 为满幅度")]
        [Range(0f, 1f)] public float weight = 1f;

        [Header("幅度")]
        [Tooltip("本地坐标，某轴为 0 则该轴不动")]
        public Vector3 positionAmplitude = new Vector3(0.1f, 0.1f, 0f);

        [Tooltip("欧拉角，单位度")]
        public Vector3 rotationAmplitude = new Vector3(0f, 0f, 5f);

        [Tooltip("相对初始缩放的比例，0.1 表示 ±10%")]
        public Vector3 scaleAmplitude = new Vector3(0.05f, 0.05f, 0f);

        [Header("节奏")]
        [Tooltip("两次抽目标的间隔（秒）")]
        [SerializeField] float interval = 0.12f;

        [Tooltip("追向目标的时间，越小越利")]
        [SerializeField] float smoothTime = 0.06f;

        [Tooltip("PlayOnce() 的默认时长（秒）")]
        [SerializeField] float onceDuration = 0.35f;

        enum Mode
        {
            Idle,
            Loop,
            Once
        }

        Mode mode;
        bool resumeLoop;
        bool captured;
        float retargetIn;
        float onceElapsed;
        float onceLength;

        Vector3 restPosition;
        Quaternion restRotation;
        Vector3 restScale;

        Vector3 positionUnit;
        Vector3 rotationUnit;
        Vector3 scaleUnit;

        Vector3 positionOffset;
        Vector3 rotationOffset;
        Vector3 scaleOffset;
        Vector3 positionVelocity;
        Vector3 rotationVelocity;
        Vector3 scaleVelocity;

        public void PlayLoop()
        {
            if (!captured) CaptureRest();
            mode = Mode.Loop;
            resumeLoop = false;
            retargetIn = 0f;
        }

        public void PlayOnce()
        {
            PlayOnce(onceDuration);
        }

        public void PlayOnce(float duration)
        {
            if (!captured) CaptureRest();
            if (mode != Mode.Once)
                resumeLoop = mode == Mode.Loop;
            mode = Mode.Once;
            onceElapsed = 0f;
            onceLength = duration > 0.0001f ? duration : 0.0001f;
            retargetIn = 0f;
        }

        public void Stop()
        {
            mode = Mode.Idle;
            resumeLoop = false;
            ClearMotion();
            if (captured) Restore();
        }

        void OnEnable()
        {
            SelfCheck();
            if (!Application.isPlaying) return;
            CaptureRest();
            ClearMotion();
            if (playOnEnable) PlayLoop();
        }

        void OnDisable()
        {
            if (!Application.isPlaying) return;
            Stop();
        }

        void LateUpdate()
        {
            if (!Application.isPlaying || mode == Mode.Idle) return;

            float amp = Mathf.Clamp01(weight);
            if (mode == Mode.Once)
            {
                onceElapsed += Time.deltaTime;
                if (onceElapsed >= onceLength)
                {
                    if (resumeLoop) PlayLoop();
                    else Stop();
                    if (mode == Mode.Idle) return;
                    amp = Mathf.Clamp01(weight);
                }
                else
                {
                    amp *= Envelope(onceElapsed / onceLength);
                }
            }

            if (amp <= 0.0001f)
            {
                positionUnit = rotationUnit = scaleUnit = Vector3.zero;
                retargetIn = 0f;
            }
            else
            {
                retargetIn -= Time.deltaTime;
                if (retargetIn <= 0f)
                {
                    positionUnit = RandomUnit();
                    rotationUnit = RandomUnit();
                    scaleUnit = RandomUnit();
                    retargetIn = interval > 0f ? interval : 0f;
                }
            }

            float dt = Time.deltaTime;
            float damp = smoothTime > 0.0001f ? smoothTime : 0.0001f;
            Vector3 positionTarget = Vector3.Scale(positionAmplitude, positionUnit) * amp;
            Vector3 rotationTarget = Vector3.Scale(rotationAmplitude, rotationUnit) * amp;
            Vector3 scaleTarget = Vector3.Scale(scaleAmplitude, scaleUnit) * amp;
            positionOffset = Vector3.SmoothDamp(positionOffset, positionTarget, ref positionVelocity, damp, Mathf.Infinity, dt);
            rotationOffset = Vector3.SmoothDamp(rotationOffset, rotationTarget, ref rotationVelocity, damp, Mathf.Infinity, dt);
            scaleOffset = Vector3.SmoothDamp(scaleOffset, scaleTarget, ref scaleVelocity, damp, Mathf.Infinity, dt);
            Apply();
        }

        // ponytail: 初始姿态只在 OnEnable 记一次。抖动期间改本节点 Transform 会被盖掉；要挪整体或播位移动画，改父节点。
        void CaptureRest()
        {
            restPosition = transform.localPosition;
            restRotation = transform.localRotation;
            restScale = transform.localScale;
            captured = true;
        }

        void Apply()
        {
            transform.localPosition = restPosition + positionOffset;
            transform.localRotation = restRotation * Quaternion.Euler(rotationOffset);
            transform.localScale = new Vector3(
                ScaleAxis(restScale.x, scaleOffset.x),
                ScaleAxis(restScale.y, scaleOffset.y),
                ScaleAxis(restScale.z, scaleOffset.z));
        }

        void Restore()
        {
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            transform.localScale = restScale;
        }

        void ClearMotion()
        {
            positionUnit = rotationUnit = scaleUnit = Vector3.zero;
            positionOffset = rotationOffset = scaleOffset = Vector3.zero;
            positionVelocity = rotationVelocity = scaleVelocity = Vector3.zero;
            retargetIn = 0f;
        }

        static Vector3 RandomUnit()
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }

        internal static float Envelope(float t)
        {
            if (t <= 0f || t >= 1f) return 0f;
            return Mathf.Sin(Mathf.PI * t);
        }

        internal static float ScaleAxis(float rest, float offset)
        {
            float multiplier = 1f + offset;
            if (multiplier < 0.01f) multiplier = 0.01f;
            return rest * multiplier;
        }

#if UNITY_EDITOR
        static bool selfChecked;

        static void SelfCheck()
        {
            if (selfChecked) return;
            selfChecked = true;
            const float tolerance = 0.001f;
            bool envelope = Envelope(0f) <= tolerance
                && Envelope(1f) <= tolerance
                && Mathf.Abs(Envelope(0.5f) - 1f) <= tolerance;
            bool scale = ScaleAxis(-2f, -5f) < 0f && ScaleAxis(2f, 0.1f) > 2.19f;
            if (!envelope || !scale)
                Debug.LogError("RandomJitter self-check failed");
        }
#else
        static void SelfCheck() { }
#endif
    }
}
