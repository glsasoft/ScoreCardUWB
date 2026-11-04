using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Component.Common.Component
{
    public class SettingButton : MonoBehaviour
    {
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color unselectedColor = new Color(1f, 1f, 1f, 0.2f);

        [SerializeField] private TMP_Text text;
        [SerializeField] private Image selectedImage;
        
        [SerializeField] private bool selected = false;

        private void Start()
        {
            SetSelected(selected);
        }
    
        /// <summary>
        /// 设置按钮的选中状态
        /// </summary>
        /// <param name="isSelected"></param>
        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            
            selectedImage.gameObject.SetActive(isSelected);
            text.color = isSelected ? selectedColor : unselectedColor;
        }
        
        public bool IsSelected()
        {
            return selected;
        }
    }
}
