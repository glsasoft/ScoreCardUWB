using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 底部队员卡片：编号角标、头像、姓名、击杀/死亡。
    /// 10 人档沿用 PSD 横版卡片；20 人档改竖版（头像在上、编号压头像下缘、姓名居中、击杀/死亡在下）；
    /// 40 人档只留姓名与战绩，头像作低对比度底纹，第三行显示复活倒计时。
    /// </summary>
    public class UwbPlayerCardItem : MonoBehaviour
    {
        [SerializeField] UwbAvatarView avatarView;
        [SerializeField] TMP_Text numberText;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text killsText;
        [SerializeField] TMP_Text deathsText;
        [SerializeField] GameObject statsRow;
        [SerializeField] TMP_Text deadTipText;
        [SerializeField] CanvasGroup contentGroup;
        [SerializeField] float deadAlpha = 0.5f;

        const int Bg = 0;
        const int Border = 1;
        const int Badge = 2;
        const int Number = 3;
        const int Name = 4;
        const int Avatar = 5;
        const int KillLabel = 6;
        const int Kills = 7;
        const int DeathLabel = 8;
        const int Deaths = 9;
        const int DeadTip = 10;

        static readonly string[] ChildPaths =
        {
            "Content/Bg",
            "Content/Border",
            "Content/Badge",
            "Content/Number",
            "Content/Name",
            "Content/Avatar",
            "Content/StatsRow/KillLabel",
            "Content/StatsRow/Kills",
            "Content/StatsRow/DeathLabel",
            "Content/StatsRow/Deaths",
            "Content/DeadTip",
        };

        struct Snap
        {
            public Vector2 pos;
            public Vector2 size;
            public Vector3 scale;
            public float font;
            public TextAlignmentOptions align;
            public TextOverflowModes overflow;
            public bool wrap;
            public string str;
            public int sibling;
        }

        // 所有卡片共用 10 人档的原始布局；克隆体可能来自已改过的模板，所以只在第一次（未改动时）记录
        static Snap[] s_Base;

        readonly RectTransform[] m_Rts = new RectTransform[ChildPaths.Length];
        readonly TMP_Text[] m_Txts = new TMP_Text[ChildPaths.Length];
        bool m_Bound;

        Sprite m_Avatar;
        int m_Number = int.MinValue;
        string m_Name;
        int m_Kills = int.MinValue;
        int m_Deaths = int.MinValue;
        int m_Revive = int.MinValue;
        bool m_Dead;
        bool m_HasState;
        UwbHudRosterMode m_Density = UwbHudRosterMode.Ten;
        bool m_ShowEmptySlot;

        public RectTransform Rect => (RectTransform)transform;

        public void SetEmpty()
        {
            // 10 人档空槽保留占位；20/40 多余槽隐藏
            if (m_ShowEmptySlot)
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                if (contentGroup != null)
                    contentGroup.alpha = 0.15f;
                if (statsRow != null)
                    statsRow.SetActive(false);
                if (deadTipText != null)
                    deadTipText.gameObject.SetActive(false);
                if (nameText != null)
                    nameText.text = string.Empty;
                if (numberText != null)
                    numberText.text = string.Empty;
                if (avatarView != null)
                    avatarView.SetSprite(null);
            }
            else if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            m_HasState = false;
            // 文字已清空，缓存也要作废，否则同一个人再回来时不会重写文字
            m_Number = int.MinValue;
            m_Name = null;
            m_Avatar = null;
            m_Kills = int.MinValue;
            m_Deaths = int.MinValue;
        }

        public void Apply(UwbHudPlayer player)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (m_Number != player.number)
            {
                m_Number = player.number;
                if (numberText != null)
                    numberText.text = player.number.ToString("D2");
            }

            if (m_Name != player.name)
            {
                m_Name = player.name;
                if (nameText != null)
                    nameText.text = player.name;
            }

            if (m_Avatar != player.avatar)
            {
                m_Avatar = player.avatar;
                if (avatarView != null)
                    avatarView.SetSprite(player.avatar);
            }

            if (m_Kills != player.kills)
            {
                m_Kills = player.kills;
                if (killsText != null)
                    killsText.text = player.kills.ToString("D2");
            }

            if (m_Deaths != player.deaths)
            {
                m_Deaths = player.deaths;
                if (deathsText != null)
                    deathsText.text = player.deaths.ToString("D2");
            }

            bool dead = player.IsDead;
            if (!m_HasState || m_Dead != dead || (dead && m_Revive != player.reviveSeconds))
            {
                m_HasState = true;
                m_Dead = dead;
                m_Revive = player.reviveSeconds;
                if (contentGroup != null)
                    contentGroup.alpha = dead ? deadAlpha : 1f;
                // 10 人档死亡后战绩行换成倒计时；20/40 人档姓名与战绩常驻，倒计时另占一行
                if (statsRow != null)
                    statsRow.SetActive(!dead || m_Density != UwbHudRosterMode.Ten);
                if (deadTipText != null)
                {
                    deadTipText.gameObject.SetActive(dead);
                    if (dead)
                        deadTipText.text = player.reviveSeconds > 0
                            ? player.reviveSeconds + "s可复活"
                            : "待复活";
                }
            }
        }

        /// <summary>按 10/20/40 档切换卡片版式。w/h 是该档单张卡片尺寸（画布单位）。</summary>
        public void ApplyRosterDensity(UwbHudRosterMode mode, bool keepEmptySlotVisible, float w, float h)
        {
            m_Density = mode;
            m_ShowEmptySlot = keepEmptySlotVisible;
            m_HasState = false;

            BindChildren();
            CaptureBaseline();
            RestoreBaseline();

            bool forty = mode == UwbHudRosterMode.Forty;
            SetActive(Badge, !forty);
            SetActive(Number, !forty);

            if (avatarView != null)
            {
                avatarView.gameObject.SetActive(true);
                var cg = avatarView.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = avatarView.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = forty ? 0.15f : 1f;
                cg.blocksRaycasts = false;
            }

            if (mode == UwbHudRosterMode.Twenty)
                LayoutTwenty(w, h);
            else if (forty)
                LayoutForty(w, h);
        }

        void BindChildren()
        {
            if (m_Bound)
                return;
            m_Bound = true;
            for (int i = 0; i < ChildPaths.Length; i++)
            {
                Transform t = transform.Find(ChildPaths[i]);
                if (t == null)
                    continue;
                m_Rts[i] = (RectTransform)t;
                m_Txts[i] = t.GetComponent<TMP_Text>();
            }
        }

        void CaptureBaseline()
        {
            if (s_Base != null)
                return;
            s_Base = new Snap[ChildPaths.Length];
            for (int i = 0; i < ChildPaths.Length; i++)
            {
                RectTransform rt = m_Rts[i];
                if (rt == null)
                    continue;
                var s = new Snap
                {
                    pos = rt.anchoredPosition,
                    size = rt.sizeDelta,
                    scale = rt.localScale,
                    sibling = rt.GetSiblingIndex(),
                };
                TMP_Text t = m_Txts[i];
                if (t != null)
                {
                    s.font = t.fontSize;
                    s.align = t.alignment;
                    s.overflow = t.overflowMode;
                    s.wrap = t.enableWordWrapping;
                    s.str = t.text;
                }

                s_Base[i] = s;
            }
        }

        void RestoreBaseline()
        {
            for (int i = 0; i < ChildPaths.Length; i++)
            {
                RectTransform rt = m_Rts[i];
                if (rt == null)
                    continue;
                Snap s = s_Base[i];
                rt.anchoredPosition = s.pos;
                rt.sizeDelta = s.size;
                rt.localScale = s.scale;
                rt.SetSiblingIndex(s.sibling);
                TMP_Text t = m_Txts[i];
                if (t == null)
                    continue;
                t.fontSize = s.font;
                t.alignment = s.align;
                t.overflowMode = s.overflow;
                t.enableWordWrapping = s.wrap;
                if (i == KillLabel || i == DeathLabel)
                    t.text = s.str;
            }
        }

        void SetActive(int key, bool active)
        {
            if (m_Rts[key] != null)
                m_Rts[key].gameObject.SetActive(active);
        }

        // 局部坐标：左上角为原点，y 向下为正
        void Place(int key, float x, float y, float w, float h)
        {
            RectTransform rt = m_Rts[key];
            if (rt == null)
                return;
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }

        void Style(int key, float font, TextAlignmentOptions align)
        {
            TMP_Text t = m_Txts[key];
            if (t == null)
                return;
            t.fontSize = font;
            t.alignment = align;
            t.overflowMode = TextOverflowModes.Overflow;
        }

        // 单行显示，超宽时按字宽截断并加省略号；矩形要高于一行字高，否则 TMP 一个字都不显示，所以配合顶部对齐
        void StyleSingleLineEllipsis(int key, float font)
        {
            TMP_Text t = m_Txts[key];
            if (t == null)
                return;
            t.fontSize = font;
            t.alignment = TextAlignmentOptions.Top;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
        }

        void SetText(int key, string s)
        {
            if (m_Txts[key] != null)
                m_Txts[key].text = s;
        }

        // 20 人档：头像在上，编号压头像下缘，姓名居中，击杀/死亡在下，倒计时再下一行
        void LayoutTwenty(float w, float h)
        {
            Place(Bg, 0f, 0f, w, h);
            Place(Border, 0f, 0f, w, h);

            const float AvatarScale = 0.85f;
            float avatarSize = m_Rts[Avatar] != null ? s_Base[Avatar].size.x * AvatarScale : 0f;
            const float AvatarTop = 4f;
            if (m_Rts[Avatar] != null)
            {
                m_Rts[Avatar].localScale = Vector3.one * AvatarScale;
                m_Rts[Avatar].anchoredPosition = new Vector2((w - avatarSize) * 0.5f, -AvatarTop);
            }

            float avatarBottom = AvatarTop + avatarSize;
            const float BadgeW = 34f;
            const float BadgeH = 19f;
            float badgeY = avatarBottom - BadgeH * 0.5f - 2f;
            Place(Badge, (w - BadgeW) * 0.5f, badgeY, BadgeW, BadgeH);
            Place(Number, (w - BadgeW) * 0.5f, badgeY, BadgeW, BadgeH);
            Style(Number, 11f, TextAlignmentOptions.Center);

            float nameY = badgeY + BadgeH + 2f;
            Place(Name, 2f, nameY, w - 4f, 20f);
            StyleSingleLineEllipsis(Name, 13f);

            LayoutStatsRow(w, nameY + 18f, 20f, 10f, 14f);
            Place(DeadTip, 2f, nameY + 36f, w - 4f, 15f);
            Style(DeadTip, 11f, TextAlignmentOptions.Center);
        }

        // 40 人档：姓名、击杀/死亡、倒计时三行；头像放大成低对比度底纹压在文字下面
        void LayoutForty(float w, float h)
        {
            Place(Bg, 0f, 0f, w, h);
            Place(Border, 0f, 0f, w, h);

            if (m_Rts[Avatar] != null)
            {
                float frame = s_Base[Avatar].size.x;
                float scale = (h - 4f) / frame;
                float size = frame * scale;
                m_Rts[Avatar].localScale = Vector3.one * scale;
                m_Rts[Avatar].anchoredPosition = new Vector2((w - size) * 0.5f, -2f);
                m_Rts[Avatar].SetSiblingIndex(Border + 1);
            }

            Place(Name, 2f, 2f, w - 4f, 20f);
            StyleSingleLineEllipsis(Name, 12f);

            LayoutStatsRow(w, 17f, 15f, 9f, 12f);
            Place(DeadTip, 2f, 33f, w - 4f, 14f);
            Style(DeadTip, 10f, TextAlignmentOptions.Center);
        }

        // 一行「击 00 死 00」居中排在卡片宽度内
        void LayoutStatsRow(float w, float y, float rowH, float labelFont, float valueFont)
        {
            SetText(KillLabel, "击");
            SetText(DeathLabel, "死");

            const float LabelW = 12f;
            const float ValueW = 21f;
            const float Gap = 6f;
            float groupW = LabelW + ValueW;
            float total = groupW * 2f + Gap;
            float x0 = (w - total) * 0.5f;

            Place(KillLabel, x0, y, LabelW, rowH);
            Place(Kills, x0 + LabelW, y, ValueW, rowH);
            Place(DeathLabel, x0 + groupW + Gap, y, LabelW, rowH);
            Place(Deaths, x0 + groupW + Gap + LabelW, y, ValueW, rowH);

            Style(KillLabel, labelFont, TextAlignmentOptions.Left);
            Style(DeathLabel, labelFont, TextAlignmentOptions.Left);
            Style(Kills, valueFont, TextAlignmentOptions.Left);
            Style(Deaths, valueFont, TextAlignmentOptions.Left);
        }
    }
}
