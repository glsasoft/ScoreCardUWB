using Common.Network.Model.Http;
using LetsPiu.Common.Network.Model.Http.Play;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Component
{
    public class GenderAvatarDataUI : MonoBehaviour
    {
        public List<Sprite> genderSprite;
        Image genderimage;

        /// <summary>
        /// 设置玩家的性别
        /// </summary>
        /// <param name="p"></param>
        public void SetGenderIcon(Player p)
        {
            genderimage = GetComponent<Image>();
            if (p == null)
            {
                Debug.LogError($"{gameObject.name} GenderAvatarDataUI is null!");
                return;
            }
            if (genderSprite.Count == 0)
            {
                Debug.LogError($"{gameObject.name}  List<Sprite>.Count is Zero!");
                return;
            }
            if (genderimage == null)
            {
                Debug.LogError($"{gameObject.name}  GenderAvatarDataUI don't have Image!");
                return;
            }
            if (p.sex == 0)
            {
                // 女
                genderimage.sprite = genderSprite[0];
            }
            else if (p.sex == 1)
            {
                // 男
                genderimage.sprite = genderSprite[1];
            }
            genderimage.SetNativeSize();
        }
    }
}
