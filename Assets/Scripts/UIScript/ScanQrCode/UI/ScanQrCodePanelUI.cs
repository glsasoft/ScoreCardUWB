using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.ScanQrCode.UI
{
    public class ScanQrCodePanelUI : MonoBehaviour
    {
        /// <summary>
        /// 关闭按钮触发
        /// </summary>
        public event Action BackPressedEvent;
        /// <summary>
        /// 点击了闪光灯开关
        /// </summary>
        public event Action ToggleFlashEvent;
        public event Action SelectAlbumEvent;
        /// <summary>
        /// 手动输入了验证码
        /// </summary>
        public event Action ManualInputClickEvent; 
        
        [SerializeField] private RectTransform scanRect;
        [SerializeField] private Color maskColor;

        [SerializeField] private RectTransform topPanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;

        public TextMeshProUGUI titleText;

        private void Awake()
        {
            titleText.text = "";
        }

        public void OnButtonBackClick()
        {
            BackPressedEvent?.Invoke();
        }

        public void OnToggleFlashClick()
        {
            ToggleFlashEvent?.Invoke();
        }

        public void OnSelectAlbumClick()
        {
            SelectAlbumEvent?.Invoke();
        }

        public void OnManualInputClick()
        {
            ManualInputClickEvent?.Invoke();
        }
    }
}
