using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.SettingScripts.UI
{
    [RequireComponent(typeof(Button))]
    public class SettingTipsBtn : MonoBehaviour
    {
        public string titleID;
        public string contentID;
        public string iconName;

        private Button selfBtn;
        private RectTransform selfTrans;

        private void Start()
        {
            selfBtn = GetComponent<Button>();
            selfTrans = GetComponent<RectTransform>();

            selfBtn.onClick.AddListener(OnSelfBtnClick);
        }

        private void OnSelfBtnClick()
        {
            if (string.IsNullOrEmpty(contentID))
                return;

            // 老 LanguageHelper.GetLanguage(...) 取出的 title/content 局部变量原本未被任何后续逻辑消费，
            // 已随老多国语言系统一并清理。如将来需要弹出 Tips 窗口，
            // 走 LocalizationManager.S.GetText(titleID/contentID)，并把字段值改为新 CSV 中分配的 9xxxxx 公共词 ID。
        }
    }
}
