using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 玩家头像：头像框在最下，其上是默认头像，真实头像（正方形图）用 CircleRawImage 材质裁成圆形盖在最上。
    /// 结构与 ScoreCardUnity 的 PlayerItem 一致：HeadFrame / HeadIconDefault / HeadIcon。
    /// </summary>
    public class UwbAvatarView : MonoBehaviour
    {
        [SerializeField] Image headIcon;
        [SerializeField] GameObject headIconDefault;

        Sprite m_Sprite;

        public void SetSprite(Sprite sprite)
        {
            if (m_Sprite == sprite && headIcon.enabled == (sprite != null))
                return;

            m_Sprite = sprite;
            headIcon.sprite = sprite;
            headIcon.enabled = sprite != null;
            headIconDefault.SetActive(sprite == null);
        }
    }
}
