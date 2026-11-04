using LetsPiu.Base;
using LetsPiu.GameLogic;
using LetsPiu.UI.UIScript.SettingScripts.Logic;
using LetsPiu.UIDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.SettingScripts.UI
{
    [RequireComponent(typeof(Toggle))]
    public class SettingToggle : MonoBehaviour
    {
        public SettingID settingID = SettingID.None;
        public int optionValue = 0;

        public event Action<SettingID, int> OnSetSettingValue;

        Toggle selfToggle;
        private void Awake()
        {
            selfToggle = GetComponent<Toggle>();
            selfToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void OnToggleValueChanged(bool v)
        {
            if (settingID == SettingID.None)
                return;

            if (v)
            {
                if (optionValue > 0)
                {
                    SettingMgr.S.SetSettingValue(settingID, optionValue);
                    OnSetSettingValue?.Invoke(settingID, optionValue);
                }
                else
                {
                    SettingMgr.S.SetSettingValue(settingID, 1);
                    OnSetSettingValue?.Invoke(settingID, 1);
                }
            }
            else
            {
                if (optionValue <= 0)
                {
                    SettingMgr.S.SetSettingValue(settingID, 0);
                    OnSetSettingValue?.Invoke(settingID, 0);
                }
            }
        }

        public void ClearEvent()
        {
            OnSetSettingValue = null;
        }
    }
}
