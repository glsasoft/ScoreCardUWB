using System;
using LetsPiu.UI.UIScript.WebViewScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.LoginScripts.UI
{
    public class PrivacyAgreementUI : MonoBehaviour
    {
        /// <summary>
        /// 用户拒绝了协议
        /// </summary>
        public event Action UserRejectedEvent;
        /// <summary>
        /// 用户同意了协议
        /// </summary>
        public event Action UserAcceptedEvent;

        public event Action ClickAppleLoginEvent;
        public event Action ClickWxLoginEvent;
        public event Action ClickPhoneLoginEvent;
        public event Action ClickCloseLoginEvent;
        
        public event Action OpenAnnouncementEvent;
        public event Action OpenCustomerServiceEvent;
        public event Action OpenSwitchAccountEvent;

        public Transform announcement;
        public Transform customerService;
        public Transform switchAccount;

        /// <summary>
        /// 点击了任意区域
        /// </summary>
        public event Action ClickAnywhereEvent;
        
        /// <summary>
        /// 确认对话框
        /// </summary>
        public Transform confirmDialog;
        /// <summary>
        /// 是否同意的状态显示
        /// </summary>
        public Toggle agreementCheck;

        /// <summary>
        /// 登录账号信息
        /// </summary>
        public TMP_Text accountText;

        /// <summary>
        /// 选择登录方式对话框
        /// </summary>
        public Transform selectLoginMethodDialog;

        public Transform startGame;

        public TitleWebView titleWebView;

        private void Awake()
        {
            ResetUI();
        }

        public void ResetUI()
        {
            confirmDialog.gameObject.SetActive(false);
            titleWebView.gameObject.SetActive(false);
            //SetUserRejected();
        }

        /// <summary>
        /// 切换账号
        /// </summary>
        public void OnClickSwitchAccount()
        {
            OpenSwitchAccountEvent?.Invoke();
        }

        public void OnClickAnnouncement()
        {
            OpenAnnouncementEvent?.Invoke();
        }

        public void OnClickCustomerService()
        {
            OpenCustomerServiceEvent?.Invoke();   
        }

        public void OnClickAnywhere()
        {
            ClickAnywhereEvent?.Invoke();
        }
        
        /// <summary>
        /// 显示同意协议对话框
        /// </summary>
        public void ShowConfirmDialog()
        {
            confirmDialog.gameObject.SetActive(true);
        }

        public void OnClickConfirm()
        {
            Debug.Log("PrivacyAgreementUI 用户点击了同意");
            UserAcceptedEvent?.Invoke();
        }

        public void OnClickReject()
        {
            Debug.Log("PrivacyAgreementUI 用户点击了不同意");
            UserRejectedEvent?.Invoke();
        }
        
        /// <summary>
        /// 显示隐私政策
        /// </summary>
        public void OnClickShowPrivacy()
        {
            //titleWebView.LoadUrl("隐私政策", "https://letspiu.com/privacyPhone.html");
            titleWebView.LoadUrl("隐私政策", "https://letspiu.com/privacy.html");
        }
        
        /// <summary>
        /// 显示用户协议
        /// </summary>
        public void OnClickShowAgreement()
        {
            //titleWebView.LoadUrl("用户协议", "https://letspiu.com/userAgrPhone.html");
            titleWebView.LoadUrl("用户协议", "https://letspiu.com/userAgr.html");
        }

        public void OnClickShowThirdInformation()
        {
            //titleWebView.LoadUrl("第三方信息共享清单", "https://letspiu.com/informationPhone.html");
            titleWebView.LoadUrl("第三方信息共享清单", "https://letspiu.com/information.html");
        }

        public void OnClickShowChildrenGuide()
        {
            //titleWebView.LoadUrl("儿童隐私保护指引", "https://letspiu.com/childrenPrPhone.html");
            titleWebView.LoadUrl("儿童隐私保护指引", "https://letspiu.com/childrenPr.html");
        }
        
        /// <summary>
        /// 显示用户同意协议的状态
        /// </summary>
        public void SetUserAgreed()
        {
            Debug.Log("PrivacyAgreementUI 设置UI状态为已同意");
            agreementCheck.isOn = true;
        }
        
        /// <summary>
        /// 显示用户不同意协议的状态
        /// </summary>
        public void SetUserRejected()
        {
            Debug.Log("PrivacyAgreementUI 设置UI状态为未同意");
            agreementCheck.isOn = false;
        }

        public bool IsAgreeChecked()
        {
            return agreementCheck.isOn;
        }

        public void OnClickAppleLogin()
        {
            ClickAppleLoginEvent?.Invoke();   
        }
        
        public void OnClickWxLogin()
        {
            ClickWxLoginEvent?.Invoke();
        }

        public void OnClickPhoneLogin()
        {
            ClickPhoneLoginEvent?.Invoke();
        }

        public void OnClickCloseLogin()
        {
            ClickCloseLoginEvent?.Invoke();
        }
    }
}
