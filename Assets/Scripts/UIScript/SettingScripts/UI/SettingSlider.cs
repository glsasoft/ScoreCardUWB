using LetsPiu.Base;
using LetsPiu.GameLogic;
using LetsPiu.UI.UIScript.SettingScripts.Logic;
using LetsPiu.UIDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.SettingScripts.UI
{
    [RequireComponent(typeof(Slider))]
    public class SettingSlider : MonoBehaviour
    {
        public SettingID settingID = SettingID.None;
        public TMP_Text textValue;

        public event Action<SettingID, int> OnSetSettingValue;

        Slider selfSlider;
        private void Awake()
        {
            selfSlider = GetComponent<Slider>();
            selfSlider.minValue = 0;
            selfSlider.maxValue = 100;
            selfSlider.wholeNumbers = true;
            selfSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(float v)
        {
            if (textValue)
                textValue.text = v.ToString();

            if (settingID == SettingID.None)
                return;

            SettingMgr.S.SetSettingValue(settingID, (int)v);
            OnSetSettingValue?.Invoke(settingID, (int)v);
        }

        public void Step(bool add)
        {
            if (add)
                selfSlider.value += 1;
            else
                selfSlider.value -= 1;
        }

        public void ClearEvent()
        {
            OnSetSettingValue = null;
        }
    }
}
