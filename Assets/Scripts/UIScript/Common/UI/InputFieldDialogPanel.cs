using System;
using TMPro;
using UnityEngine;

namespace LetsPiu.UI.UIScript.Common.UI
{
    /// <summary>
    /// 输入内容对话框
    /// </summary>
    public class InputFieldDialogPanel: AbstractPanel
    {
        /// <summary>
        /// 对话框参数
        /// </summary>
        public class InputFieldDialogPanelArgs
        {
            public enum InputMode
            {
                Normal,
                Numeric
            }

            public InputMode Mode = InputMode.Normal;

            public string TitleText = "请输入";
            public string DefaultInputText = "";
            public string PlaceholderText = "请输入内容";
            public Action<InputFieldDialogPanel, string> OnConfirm;
            public Action<InputFieldDialogPanel> OnCancel;
        }
        
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text placeholderText;
        
        private InputFieldDialogPanelArgs inputFieldDialogPanelArgs;

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            if (args.Length > 0)
            {
                try
                {
                    inputFieldDialogPanelArgs = args[0] as InputFieldDialogPanelArgs;
                }
                catch (Exception ex)
                {
                    Debug.LogError("InputFieldDialogPanel OnOpen args error: " + ex);
                    inputFieldDialogPanelArgs = null;
                }
            }
            else
            {
                Debug.LogWarning("InputFieldDialogPanel OnOpen args is null");
                inputFieldDialogPanelArgs = null;
            }
            
            if(inputFieldDialogPanelArgs != null)
            {
                titleText.text = inputFieldDialogPanelArgs.TitleText;
                inputField.text = inputFieldDialogPanelArgs.DefaultInputText;
                placeholderText.text = inputFieldDialogPanelArgs.PlaceholderText;

                // 根据输入模式设置输入类型（用于 Android TV 数字/全键盘弹出）
                try
                {
                    if (inputField != null)
                    {
                        switch (inputFieldDialogPanelArgs.Mode)
                        {
                            case InputFieldDialogPanelArgs.InputMode.Numeric:
                                inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                                inputField.keyboardType = TouchScreenKeyboardType.NumberPad;
                                break;
                            default:
                                inputField.contentType = TMP_InputField.ContentType.Standard;
                                inputField.keyboardType = TouchScreenKeyboardType.Default;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to apply input mode: " + ex);
                }
            }
        }

        public void OnConfirmClick()
        {
            if (string.IsNullOrEmpty(inputField.text))
            {
                ToastPanel.Show("请输入内容");
                return;
            }
            inputFieldDialogPanelArgs.OnConfirm?.Invoke(this, inputField.text);
        }

        public void OnCancelClick()
        {
            inputFieldDialogPanelArgs.OnCancel?.Invoke(this);
        }
    }
}