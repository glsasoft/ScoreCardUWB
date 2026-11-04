using LetsPiu.GameLogic;
using LetsPiu.Main.Common.Scripts;
using LetsPiu.UI.UIScript.LoginScripts.Logic;
using LetsPiu.UI.UIScript.WebViewScripts;
using LetsPiu.UIDefine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.SettingScripts.UI
{
    public class SettingBasicPageUI : MonoBehaviour
    {
        public TMP_Text textSmooth;
        public TMP_Text textOverload;
        public ToggleGroup tgFrameRate;
        public ToggleGroup tgResolutionRatio;
        public ToggleGroup tgGraphicsQuality;
        public CanvasGroup cgCustomQuality;
        public Toggle toggleHideUI;
        public Toggle toggleRenderOptimize;
        public ToggleGroup tgAutoDownloadRes;
        public TitleWebView titleWebView;
        
        /// <summary>
        /// 设备注册信息
        /// </summary>
        [SerializeField] private TMP_Text txtRegisterInfo;

        private List<Toggle> customQualityToggles = new List<Toggle>();

        private void Start()
        {
            InitSettingToggleGroup(tgFrameRate, SettingID.FrameRate);
            InitSettingToggleGroup(tgResolutionRatio, SettingID.ResolutionRatio);
            InitSettingToggleGroup(tgGraphicsQuality, SettingID.GraphicsQuality);
            InitCustomGraphicsQuality();
            InitSettingToggle(toggleHideUI, SettingID.HideUiEnable);
            InitSettingToggle(toggleRenderOptimize, SettingID.RenderOptimizeEnable);
            InitSettingToggleGroup(tgAutoDownloadRes, SettingID.AutoDownloadRes);

            RefreshRegisterInfo();
        }

        /// <summary>
        /// 刷新设备注册信息显示：设备名称、门店 Key、友盟标识、服务地址
        /// </summary>
        private void RefreshRegisterInfo()
        {
            if (txtRegisterInfo == null) return;

            var deviceInfo = AppStateManager.Instance.DeviceInfo5S;
            if (deviceInfo == null || !deviceInfo.IsValid())
            {
                txtRegisterInfo.text = "未注册";
                return;
            }

            txtRegisterInfo.text =
                $"设备名称：{deviceInfo.GetDeviceName()}\n" +
                $"门店Key：{deviceInfo.GetProductKey()}\n" +
                $"友盟标识：{deviceInfo.GetUmengFlag()}\n" +
                $"服务地址：{deviceInfo.GetUri()}";
        }

        public void DeleteRegisterInfo()
        {
            LetsPiu.Common.UI.DialogManager.OpenBinaryChoiceTextDialog(
                "确认删除", "确定要删除设备信息重新注册？", "确定", "取消", isConfirm =>
                {
                    if (isConfirm) LoginPanel.ClearRegisterInfoAndReturnToLogin();
                });
        }

        private void InitSettingToggleGroup(ToggleGroup group, SettingID settingID)
        {
            if (!group)
                return;

            int[] optionValues = SettingMgr.S.GetSettingOptons(settingID);
            if (optionValues != null && optionValues.Length > 0)
            {
                int index = 0;
                Toggle toggle;
                foreach (Transform child in group.transform)
                {
                    toggle = child.GetComponent<Toggle>();
                    if (toggle)
                    {
                        if (index >= optionValues.Length)
                            break;
                        InitSettingToggle(toggle, settingID, optionValues[index]);
                        index++;
                    }
                }
            }
        }

        private void InitSettingToggle(Toggle toggle, SettingID settingID, int optionValue = 0)
        {
            if (!toggle)
                return;

            SettingToggle settingToggle = toggle.GetComponent<SettingToggle>();
            if (!settingToggle)
                settingToggle = toggle.gameObject.AddComponent<SettingToggle>();

            settingToggle.settingID = SettingID.None;

            int value = SettingMgr.S.GetSettingValue(settingID);
            if (optionValue > 0)
                toggle.isOn = value == optionValue;
            else
                toggle.isOn = value == 1;

            settingToggle.settingID = settingID;
            settingToggle.optionValue = optionValue;
            settingToggle.ClearEvent();
            settingToggle.OnSetSettingValue += OnSetSettingValue;
        }

        private void InitCustomGraphicsQuality()
        {
            if (!cgCustomQuality)
                return;

            int graphicsQualityValue = SettingMgr.S.GetSettingValue(SettingID.GraphicsQuality);
            cgCustomQuality.enabled = graphicsQualityValue != (int)SettingOption.Custom;

            SettingID[] ids = SettingMgr.S.GetPresetValueSettingIDs(SettingID.GraphicsQuality);
            int index = 0;
            Toggle toggle;
            foreach (Transform child in cgCustomQuality.transform)
            {
                foreach (Transform child2 in child)
                {
                    toggle = child2.GetComponent<Toggle>();
                    if (toggle)
                    {
                        if (index >= ids.Length)
                            return;
                        customQualityToggles.Add(toggle);
                        InitSettingToggle(toggle, ids[index]);
                        index++;
                        break;
                    }
                }
            }
        }

        private void OnSetSettingValue(SettingID settingID, int value)
        {
            if (settingID == SettingID.GraphicsQuality)
            {
                if (value == (int)SettingOption.Custom)
                {
                    cgCustomQuality.enabled = false;
                }
                else
                {
                    cgCustomQuality.enabled = true;
                    SettingID[] ids = SettingMgr.S.GetPresetValueSettingIDs(SettingID.GraphicsQuality);
                    for (int i = 0; i < customQualityToggles.Count; i++)
                    {
                        customQualityToggles[i].isOn = SettingMgr.S.GetSettingPresetValue(settingID, (SettingOption)value, ids[i]) == 1;
                    }
                }
            }
        }

        public void OnBtnDownloadDefaultClick()
        {
            if (!tgAutoDownloadRes)
                return;

            int defaultValue = SettingMgr.S.GetSettingDefaultValue(SettingID.AutoDownloadRes);
            Toggle toggle;
            SettingToggle settingToggle;
            foreach (Transform child in tgAutoDownloadRes.transform)
            {
                toggle = child.GetComponent<Toggle>();
                if (toggle)
                {
                    settingToggle = child.GetComponent<SettingToggle>();
                    if (defaultValue == settingToggle.optionValue)
                    {
                        toggle.isOn = true;
                        return;
                    }
                }
            }
        }
    }
}
