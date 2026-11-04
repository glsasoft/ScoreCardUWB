using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace LetsPiu.UI.UIScript.LoginScripts.Logic
{
    public class ChineseEnglishNumberInput : MonoBehaviour
    {
        public TMP_InputField inputField;
        void Start()
        {
            inputField.contentType = TMP_InputField.ContentType.Custom;
            inputField.onValidateInput = OnValidateInput;
        }

        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            // 使用正则表达式检查字符是否为中文、英文或数字
            if (Regex.IsMatch(addedChar.ToString(), @"^[\u4e00-\u9fa5a-zA-Z0-9]+$"))
            {
                return addedChar;
            }
            return '\0';
        }
    }
}
