using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 玩家头顶标牌：头像、编号、姓名、血条作为一个整体，跟着人物走。
    /// 预制体：UwbGameHudPanel/TagLayer/TagTemplate，运行时按玩家克隆。
    /// </summary>
    public class UwbPlayerTagItem : MonoBehaviour
    {
        [SerializeField] UwbAvatarView avatarView;
        [SerializeField] RectTransform hpFill;
        [SerializeField] Image hpFillImage;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text numberText;
        [SerializeField] CanvasGroup group;

        [Tooltip("血条满格时填充条的宽度（画布单位）")]
        [SerializeField] float hpFullWidth = 110.5f;
        [Tooltip("填充条贴图左侧的投影留白（画布单位）")]
        [SerializeField] float hpLeftMargin = 3f;
        [SerializeField] float deadAlpha = 0.55f;

        public RectTransform Rect => (RectTransform)transform;
        public int Number { get; private set; } = int.MinValue;

        struct Snap
        {
            public RectTransform rt;
            public Vector2 pos;
            public Vector2 size;
            public TMP_Text text;
            public TextAlignmentOptions align;
        }

        const float CompactW = 112f;
        const float CompactH = 34f;

        Snap[] m_Base;
        Vector2 m_RootSize;
        float m_FullWidth;
        float m_LeftMargin;

        int m_Hp = int.MinValue;
        int m_MaxHp;
        string m_Name;
        Sprite m_Avatar;
        bool m_Dead;

        public void Apply(UwbHudPlayer player)
        {
            if (Number != player.number)
            {
                Number = player.number;
                numberText.text = player.number.ToString("D2");
            }

            if (m_Name != player.name)
            {
                m_Name = player.name;
                nameText.text = player.name;
            }

            if (m_Avatar != player.avatar)
            {
                m_Avatar = player.avatar;
                avatarView.SetSprite(player.avatar);
            }

            if (m_Hp != player.hp || m_MaxHp != player.maxHp)
            {
                m_Hp = player.hp;
                m_MaxHp = player.maxHp;
                ApplyHp(player.hp, player.maxHp);
            }

            bool dead = player.IsDead;
            if (m_Dead != dead || group.alpha == 0f)
            {
                m_Dead = dead;
                group.alpha = dead ? deadAlpha : 1f;
            }
        }

        void ApplyHp(int hp, int maxHp)
        {
            float ratio = maxHp > 0 ? Mathf.Clamp01(hp / (float)maxHp) : 0f;
            bool show = ratio > 0.001f;
            if (hpFillImage.enabled != show)
                hpFillImage.enabled = show;
            if (!show)
                return;

            Vector2 size = hpFill.sizeDelta;
            size.x = (m_Base == null ? hpLeftMargin : m_LeftMargin) + (m_Base == null ? hpFullWidth : m_FullWidth) * ratio;
            hpFill.sizeDelta = size;
        }

        static readonly string[] ChildNames = { "Bg", "Border", "Name", "HpTrack", "HpFill", "Avatar", "Badge", "Number" };

        /// <summary>10/20 人档用完整标牌（头像、编号压头像下缘、姓名、血条）；40 人档只留姓名与血条。</summary>
        public void ApplyRosterDensity(UwbHudRosterMode mode)
        {
            if (m_Base == null)
            {
                m_Base = new Snap[ChildNames.Length];
                for (int i = 0; i < ChildNames.Length; i++)
                {
                    Transform t = transform.Find(ChildNames[i]);
                    if (t == null)
                        continue;
                    var rt = (RectTransform)t;
                    m_Base[i] = new Snap
                    {
                        rt = rt,
                        pos = rt.anchoredPosition,
                        size = rt.sizeDelta,
                        text = t.GetComponent<TMP_Text>(),
                    };
                    if (m_Base[i].text != null)
                        m_Base[i].align = m_Base[i].text.alignment;
                }

                m_RootSize = Rect.sizeDelta;
                m_FullWidth = hpFullWidth;
                m_LeftMargin = hpLeftMargin;
            }

            bool compact = mode == UwbHudRosterMode.Forty;
            for (int i = 0; i < m_Base.Length; i++)
            {
                Snap s = m_Base[i];
                if (s.rt == null)
                    continue;
                s.rt.anchoredPosition = s.pos;
                s.rt.sizeDelta = s.size;
                if (s.text != null)
                    s.text.alignment = s.align;
            }

            Rect.sizeDelta = m_RootSize;
            m_FullWidth = hpFullWidth;
            SetShown(5, !compact);
            SetShown(6, !compact);
            SetShown(7, !compact);

            if (compact)
            {
                Rect.sizeDelta = new Vector2(CompactW, CompactH);
                Place(0, 0f, 0f, CompactW, CompactH);
                Place(1, 0f, 0f, CompactW, CompactH);
                Place(2, 6f, 0f, CompactW - 12f, 22f);
                if (m_Base[2].text != null)
                    m_Base[2].text.alignment = TextAlignmentOptions.Center;
                float trackW = CompactW - 16f;
                Place(3, 8f, 22f, trackW, m_Base[3].size.y);
                Place(4, 8f - hpLeftMargin, 22f, m_Base[4].size.x, m_Base[4].size.y);
                m_FullWidth = trackW + (hpFullWidth - m_Base[3].size.x);
            }

            m_Hp = int.MinValue;
        }

        void SetShown(int i, bool shown)
        {
            if (m_Base[i].rt != null)
                m_Base[i].rt.gameObject.SetActive(shown);
        }

        // 局部坐标：左上角为原点，y 向下为正
        void Place(int i, float x, float y, float w, float h)
        {
            if (m_Base[i].rt == null)
                return;
            m_Base[i].rt.anchoredPosition = new Vector2(x, -y);
            m_Base[i].rt.sizeDelta = new Vector2(w, h);
        }
    }
}
