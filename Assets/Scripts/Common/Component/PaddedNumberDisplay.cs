using TMPro;
using UnityEngine;

namespace LetsPiu.Common.Component
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class PaddedNumberDisplay : MonoBehaviour
    {
        [SerializeField] private int displayLength = 6; // 默认显示6位数字
        [SerializeField] private Color paddingColor = new Color(0.89f, 0.91f, 0.93f, 1f); // #E3E8ED
        [SerializeField] private Color numberColor = Color.white;
    
        private TextMeshProUGUI _textComponent;
        private string _currentValue = "";
    
        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
            UpdateDisplay("0"); // 初始化显示
        }
    
        // 设置要显示的数字
        public void SetNumber(int number)
        {
            SetNumber(number.ToString());
        }
    
        public void SetNumber(string numberStr)
        {
            _currentValue = numberStr;
            UpdateDisplay(numberStr);
        }
    
        private void UpdateDisplay(string numberStr)
        {
            // 确保输入只包含数字
            for (var index = 0; index < numberStr.Length; index++)
            {
                var c = numberStr[index];
                if (!char.IsDigit(c))
                {
                    Debug.LogWarning("PaddedNumberDisplay: Input contains non-digit characters");
                    return;
                }
            }

            // 创建富文本字符串
            var sb = new System.Text.StringBuilder();
        
            // 计算需要补充的0的数量
            var paddingCount = displayLength - numberStr.Length;
        
            // 添加灰色0
            for (var i = 0; i < paddingCount; i++)
            {
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(paddingColor)}>0</color>");
            }
        
            // 添加白色数字
            sb.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(numberColor)}>{numberStr}</color>");
        
            _textComponent.text = sb.ToString();
        }
    
        // 在Inspector中修改显示长度时更新显示
        private void OnValidate()
        {
            if (_textComponent == null)
                _textComponent = GetComponent<TextMeshProUGUI>();
            
            if (!string.IsNullOrEmpty(_currentValue))
                UpdateDisplay(_currentValue);
        }
    }
}