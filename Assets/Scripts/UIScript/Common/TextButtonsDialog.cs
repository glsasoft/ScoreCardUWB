using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Common.UI
{
    /// <summary>
    /// 对话框窗口
    /// </summary>
    public class TextButtonsDialog : MonoBehaviour
    {
        /// <summary>
        /// 对话框选项取消按钮的模板
        /// </summary>
        [SerializeField] private TextButton cancelButtonTemplate;

        /// <summary>
        /// 对话框选项确认按钮的模板
        /// </summary>
        [SerializeField] private TextButton confirmButtonTemplate;

        /// <summary>
        /// 标题文本框
        /// </summary>
        [SerializeField] private TMP_Text titleText;

        /// <summary>
        /// 标题为空时需要隐藏的对象
        /// </summary>
        [SerializeField] private GameObject[] titleObjects;

        /// <summary>
        /// 右上角关闭按钮
        /// </summary>
        [SerializeField] private Button closeButton;

        /// <summary>
        /// 背景按钮
        /// </summary>
        [SerializeField] private Button backgroundButton;

        /// <summary>
        /// 对话框内容区域
        /// </summary>
        public RectTransform contents;

        public string title
        {
            get => titleText ? titleText.text : null;
            set
            {
                if (!titleText) return;
                titleText.text = value;
                bool isNotNullOrWhiteSpace = !string.IsNullOrWhiteSpace(value);
                foreach (GameObject titleObject in titleObjects)
                    titleObject.SetActive(isNotNullOrWhiteSpace);
            }
        }

        /// <summary>
        /// 是否在点击背景时关闭对话框
        /// </summary>
        public bool closeDialogOnBackgroundClicked;

        /// <summary>
        /// 是否显示关闭对话框按钮
        /// </summary>
        public bool showCloseButton
        {
            get => closeButton && closeButton.gameObject.activeSelf;
            set { if (closeButton) closeButton.gameObject.SetActive(value); }
        }

        /// <summary>
        /// 对话框按钮被按下，或对话框被关闭事件<br/>
        /// 参数为点按按钮的索引，当点击背景关闭或代码关闭时参数为<c>-1</c>
        /// </summary>
        public event Action<int> OnButtonClickedEvent;

        public List<TextButton> buttons = new();

        private void Awake()
        {
            if (backgroundButton) backgroundButton.onClick.AddListener(() =>
            {
                if (closeDialogOnBackgroundClicked) ClosePanel();
            });
            if (closeButton) closeButton.onClick.AddListener(() => { ClosePanel(); });
        }

        // 在TextButtonsDialog类中添加如下内容

        // 新增字段：每个按钮的最大点击次数
        private Dictionary<TextButton, int> buttonClickLimits = new();
        // 新增字段：每个按钮当前点击次数
        private Dictionary<TextButton, int> buttonClickCounts = new();

        public void SetButtonTexts(params string[] texts)
        {
            foreach (TextButton button in buttons) Destroy(button);
            buttons.Clear();

            Transform root = confirmButtonTemplate.transform.parent;

            if (texts.Length == 0)
            {
                root.gameObject.SetActive(false);
                return;
            }

            root.gameObject.SetActive(true);
            buttons.Capacity = texts.Length;

            for (int i = 0; i < texts.Length; i++)
            {
                int j = i;
                TextButton button =
                    Instantiate(i == 0 && texts.Length != 1 ? cancelButtonTemplate : confirmButtonTemplate, root);
                button.name = texts[i];
                button.gameObject.SetActive(true);
                button.button.onClick.AddListener(() => { Close(j); });
                button.text.text = texts[i];
                buttons.Add(button);
            }
        }

        /// <summary>
        /// 设置按钮文本和每个按钮的最大点击次数
        /// </summary>
        /// <param name="clickLimits">每个按钮的最大点击次数（可选），不传时所有按钮最大点击次数为1</param>
        /// <param name="texts">按钮文本</param>
        public void SetButtonTextsWithClickLimits(int[] clickLimits = null, params string[] texts)
        {
            foreach (TextButton button in buttons) Destroy(button.gameObject);
            buttons.Clear();
            buttonClickLimits.Clear();
            buttonClickCounts.Clear();

            Transform root = confirmButtonTemplate.transform.parent;

            if (texts.Length == 0)
            {
                root.gameObject.SetActive(false);
                return;
            }

            root.gameObject.SetActive(true);
            buttons.Capacity = texts.Length;

            for (int i = 0; i < texts.Length; i++)
            {
                int j = i;
                TextButton button =
                    Instantiate(i == 0 && texts.Length != 1 ? cancelButtonTemplate : confirmButtonTemplate, root);
                button.name = texts[i];
                button.gameObject.SetActive(true);
                button.button.onClick.AddListener(() =>
                {
                    Debug.Log("点击按钮次数: " + buttonClickLimits[button]);
                    // 增加点击次数
                    if (!buttonClickCounts.ContainsKey(button))
                        buttonClickCounts[button] = 0;
                    buttonClickCounts[button]++;

                    if (buttonClickLimits[button] > 1)
                    {
                        button.text2.gameObject.SetActive(true);
                        button.text2.text = "连点" + (buttonClickLimits[button] - buttonClickCounts[button]).ToString() + "次";
                    }
                    

                    // 检查是否达到最大点击次数
                    if (buttonClickCounts[button] >= buttonClickLimits[button])
                    {
                        Close(j);
                    }
                });
                button.text.text = texts[i];
                buttons.Add(button);
                // 设置点击次数限制
                int limit = (clickLimits != null && i < clickLimits.Length) ? clickLimits[i] : 1;
                Debug.Log("设置按钮最大点击次数: " + limit + "i" + i);
                buttonClickLimits[button] = limit;
                buttonClickCounts[button] = 0;

                if (buttonClickLimits[button] > 1)
                {
                    button.text2.gameObject.SetActive(true);
                    button.text2.text = "连点" + (buttonClickLimits[button] - buttonClickCounts[button]).ToString() + "次";
                }
            }
        }

        public void Close() => ClosePanel();

        private void Close(int index)
        {
            OnButtonClickedEvent?.Invoke(index);
            ClosePanel();
        }

        private void ClosePanel()
        {
            UIModule.S.ClosePanel(UIDefine.UIID.DialogManagerPanel);
        }
    }
}