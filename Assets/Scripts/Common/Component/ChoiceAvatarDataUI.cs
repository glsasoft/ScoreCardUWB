using LetsPiu.Define;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Component
{
    public class ChoiceAvatarDataUI : MonoBehaviour
    {
        public List<Sprite> targetSprite;
        Image image;
        /// <summary>
        /// 设置目标Sprite
        /// </summary>
        /// <param name="index">
        /// 目标Sprite在List中的索引
        /// </param>

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        bool IsEmpty()
        {
            if (targetSprite.Count == 0)
            {
                Debug.LogError($"{gameObject.name}  List<Sprite>.Count is Zero!");
                return true;
            }
            if (image == null)
            {
                Debug.LogError($"{gameObject.name}  ChoiceAvatarDataUI don't have Image!");
                return true;
            }
            return false;
        }
        public void SetTargetSprite(MvpState vpstate)
        {
            if (IsEmpty()) return;
            image.sprite = targetSprite[(int)vpstate];
            image.SetNativeSize();
            //image.sprite = targetSprite[0];
            //image.SetNativeSize();
        }
        public void SetTargetSprite(TrophyLevel trophyLevel)
        {
            if (IsEmpty()) return;
            if (trophyLevel == TrophyLevel.None)
            {
                Debug.LogError($"{gameObject.name}  ChoiceAvatarDataUI invalid TrophyLevel!");
                return;
            }
            image.sprite = targetSprite[(int)trophyLevel];
        }
        
        /// <summary>
        /// 卡牌边框资源顺序: 绿、蓝、紫、金
        /// </summary>
        /// <param name="grade"></param>
        public void SetTargetSprite(MaterialBoxSubType grade)
        {
            if (IsEmpty()) return;
            if (grade == MaterialBoxSubType.Green)
            {
                image.sprite = targetSprite[0]; 
            }else if (grade == MaterialBoxSubType.Blue)
            {
                image.sprite = targetSprite[1];
            }else if(grade == MaterialBoxSubType.Purple)
            {
                image.sprite = targetSprite[2];
            }else if(grade == MaterialBoxSubType.Gold)
            {
                image.sprite = targetSprite[3];
            }
            else
            {
                image.sprite = targetSprite[0]; 
            }
        }

        /// <summary>
        /// 宝石背景顺序：蓝、紫、黄
        /// </summary>
        /// <param name="grade"></param>
        public void SetDiamondBackgroundSprite(MaterialBoxSubType grade)
        {
            if (IsEmpty()) return;
            gameObject.SetActive(true);
            if (grade == MaterialBoxSubType.Blue)
            {
                image.sprite = targetSprite[0];
            }else if (grade == MaterialBoxSubType.Purple)
            {
                image.sprite = targetSprite[1];
            }else if(grade == MaterialBoxSubType.Gold)
            {
                image.sprite = targetSprite[2];
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 宝石图标顺序：蓝、紫、黄
        /// </summary>
        public void SetDiamondSprite(MaterialBoxSubType grade)
        {
            if (IsEmpty()) return;
            gameObject.SetActive(true);
            if (grade == MaterialBoxSubType.Blue)
            {
                image.sprite = targetSprite[0];   
            }else if (grade == MaterialBoxSubType.Purple)
            {
                image.sprite = targetSprite[1];
            }else if(grade == MaterialBoxSubType.Gold)
            {
                image.sprite = targetSprite[2];
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

    }
}
