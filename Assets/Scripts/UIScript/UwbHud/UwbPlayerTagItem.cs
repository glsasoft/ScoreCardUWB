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
            size.x = hpLeftMargin + hpFullWidth * ratio;
            hpFill.sizeDelta = size;
        }
    }
}
