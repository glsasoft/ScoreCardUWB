using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 底部队员卡片：编号角标、头像、姓名、击杀/死亡。每队固定 5 个槽位，没人时整张隐藏。
    /// 死亡时整卡变灰，战绩行换成「N s可复活 / 待复活」。
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

        Sprite m_Avatar;
        int m_Number = int.MinValue;
        string m_Name;
        int m_Kills = int.MinValue;
        int m_Deaths = int.MinValue;
        int m_Revive = int.MinValue;
        bool m_Dead;
        bool m_HasState;

        public void SetEmpty()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            m_HasState = false;
        }

        public void Apply(UwbHudPlayer player)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (m_Number != player.number)
            {
                m_Number = player.number;
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

            if (m_Kills != player.kills)
            {
                m_Kills = player.kills;
                killsText.text = player.kills.ToString("D2");
            }

            if (m_Deaths != player.deaths)
            {
                m_Deaths = player.deaths;
                deathsText.text = player.deaths.ToString("D2");
            }

            bool dead = player.IsDead;
            if (!m_HasState || m_Dead != dead || (dead && m_Revive != player.reviveSeconds))
            {
                m_HasState = true;
                m_Dead = dead;
                m_Revive = player.reviveSeconds;
                contentGroup.alpha = dead ? deadAlpha : 1f;
                statsRow.SetActive(!dead);
                deadTipText.gameObject.SetActive(dead);
                if (dead)
                    deadTipText.text = player.reviveSeconds > 0 ? player.reviveSeconds + "s可复活" : "待复活";
            }
        }
    }
}
