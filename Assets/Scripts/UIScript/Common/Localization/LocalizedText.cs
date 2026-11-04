using LetsPiu.Base.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.Localization
{
    /// <summary>
    /// 把 CSV 中某条文案绑定到所在 GameObject 的 TMP_Text / Text 上。
    ///
    /// 设计要点（与方案文档 §4.3 一致）：
    /// - 不带 [ExecuteAlways]：Edit 模式不动 TMP，预制体里的中文 m_text 即所见即所得。
    /// - OnEnable 订阅、OnDisable 取消订阅，切语言事件全屏自动刷新。
    /// - 业务侧禁止直接给 m_Key 字段赋值，应走 <see cref="SetKey"/>，保证 args 被缓存。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("LetsPiu/UI/Localized Text")]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] string m_Key;

        TMP_Text m_TMP;
        Text m_Txt;
        object[] m_Args;
        bool m_Awaked;

        public string Key => m_Key;

        void Awake()
        {
            m_TMP = GetComponent<TMP_Text>();
            m_Txt = GetComponent<Text>();
            m_Awaked = true;
            if (m_TMP == null && m_Txt == null)
            {
                Debug.LogWarning($"[LocalizedText] {name} 未挂 TMP_Text / Text，组件将空跑", this);
            }
        }

        void OnEnable()
        {
            Apply();
            LocalizationManager.S.OnLanguageChanged += Apply;
        }

        void OnDisable()
        {
            LocalizationManager.S.OnLanguageChanged -= Apply;
        }

        /// <summary>
        /// 业务运行时改 key（带可选参数）；切语言事件触发时会用最近一次的 args 重新格式化。
        /// </summary>
        public void SetKey(string key, params object[] args)
        {
            m_Key = key;
            m_Args = args != null && args.Length > 0 ? args : null;
            if (m_Awaked) Apply();
        }

        /// <summary>立即从 CSV 取译文写入 TMP/Text。</summary>
        public void Apply()
        {
            if (string.IsNullOrEmpty(m_Key)) return;

            string txt = m_Args != null
                ? LocalizationManager.S.GetText(m_Key, m_Args)
                : LocalizationManager.S.GetText(m_Key);

            if (m_TMP != null) m_TMP.text = txt;
            if (m_Txt != null) m_Txt.text = txt;
        }
    }
}
