using LetsPiu.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI
{
    public class SpeedShootingPlayerLifeBar : MonoBehaviour
    {
        [SerializeField] Image lifeFg;
        [SerializeField] [Range(0f, 1f)] float percent = 1f;
        [SerializeField] [Range(-45f, 45f)] float slantAngle = 45f;

        public float Percent
        {
            get => percent;
            set => SetPercent(value);
        }

        public float SlantAngle
        {
            get => slantAngle;
            set
            {
                slantAngle = Mathf.Clamp(value, -45f, 45f);
                ApplySlant();
            }
        }

        void Awake()
        {
            EnsureFilledSetup();
            Apply();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            EnsureFilledSetup();
            Apply();
        }
#endif

        public void SetPercent(float value)
        {
            percent = Mathf.Clamp01(value);
            Apply();
        }

        public void SetLife(float current, float max)
        {
            if (max <= 0f)
            {
                SetPercent(0f);
                return;
            }

            SetPercent(current / max);
        }

        void EnsureFilledSetup()
        {
            if (lifeFg == null) return;

            lifeFg.type = Image.Type.Filled;
            lifeFg.fillMethod = Image.FillMethod.Horizontal;
            lifeFg.fillOrigin = (int)Image.OriginHorizontal.Left;
            ApplySlant();
        }

        void ApplySlant()
        {
            if (lifeFg is ISlantedFillImage slanted)
            {
                slanted.SlantAngle = slantAngle;
            }
        }

        void Apply()
        {
            if (lifeFg == null) return;
            lifeFg.fillAmount = percent;
            ApplySlant();
        }
    }
}
