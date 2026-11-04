using LetsPiu.Common.Network.Model.Http.Play;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace LetsPiu.Common.UI
{
    public class DialogManager : AbstractPanel
    {
        public static DialogManager Instance;
        public static Action OnInstanceStart;

        public TextButtonsDialog commonDialogTemplate;
        public TMP_Text commonDialogTextTemplate;
        public CommonDialogScrollRect commonDialogScrollRectTemplate;
        public RectTransform dialogRoot;

        public TextButtonsDialog currentTextButtonsDialog;



        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void Start()
        {
            OnInstanceStart?.Invoke();
            OnInstanceStart = null;
        }

        public static void OpenPlainTextDialog(string title, string message, Action<TextButtonsDialog> callback = null)
        {
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, panel =>
            {
                TextButtonsDialog dialog = ((DialogManager)panel).OpenPlainTextDialog_Instance(title, message);
                callback?.Invoke(dialog);
            });

        }

        TextButtonsDialog OpenPlainTextDialog_Instance(string title, string message)
        {
            TextButtonsDialog dialog = Instance.OpenCommonDialogWithTextMessage(title, message);
            dialog.closeDialogOnBackgroundClicked = true;
            dialog.showCloseButton = true;
            Instance.currentTextButtonsDialog = dialog;
            return dialog;
        }

        public static void OpenConfirmTextDialog(string title, string message, string confirm,
            Action callback = null)
        {
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, (r) =>
            {
                var dialogManager = r as DialogManager;
                dialogManager.OpenConfirmTextDialog_Instance(title, message, confirm, callback);
            });
        }
        
        /// <summary>
        /// 不带关闭按钮的确认对话框
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="confirm"></param>
        /// <param name="callback"></param>
        public static void OpenAlertDialog(string title, string message, string confirm,
            Action callback = null)
        {
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, (r) =>
            {
                var dialogManager = r as DialogManager;
                dialogManager.OpenConfirmTextDialog_Instance(title, message, confirm, false, callback);
            });
        }

        TextButtonsDialog OpenConfirmTextDialog_Instance(string title, string message, string confirm,
            Action callback = null)
        {
            TextButtonsDialog dialog = Instance.OpenCommonDialogWithTextMessage(title, message);
            dialog.SetButtonTexts(confirm);
            if (callback is not null) dialog.OnButtonClickedEvent += _ => callback();
            Instance.currentTextButtonsDialog = dialog;
            return dialog;
        }
        
        TextButtonsDialog OpenConfirmTextDialog_Instance(string title, string message, string confirm, bool showCloseButton,
            Action callback = null)
        {
            TextButtonsDialog dialog = Instance.OpenCommonDialogWithTextMessage(title, message);
            dialog.SetButtonTexts(confirm);
            dialog.showCloseButton = showCloseButton;
            if (callback is not null) dialog.OnButtonClickedEvent += _ => callback();
            Instance.currentTextButtonsDialog = dialog;
            return dialog;
        }

        public static void OpenBinaryChoiceTextDialog(string title, string message, string confirm, string cancel,
            Action<bool> callback = null)
        {
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, (r) =>
            {
                var dialogManager = r as DialogManager;
                dialogManager.OpenBinaryChoiceTextDialog_Instance(title, message, confirm, cancel, callback);
            });
        }

        TextButtonsDialog OpenBinaryChoiceTextDialog_Instance(string title, string message, string confirm, string cancel,
            Action<bool> callback = null)
        {
            TextButtonsDialog dialog = OpenCommonDialogWithTextMessage(title, message);
            dialog.SetButtonTexts(cancel, confirm);
            dialog.showCloseButton = false;
            if (callback is not null) dialog.OnButtonClickedEvent += index => callback(index == 1);
            currentTextButtonsDialog = dialog;
            return dialog;
        }

        public static void OpenBinaryChoiceTextDialog(string title, string message, string confirm, string cancel, string okBtnTxt,
            Action<int> callback = null)
        {
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, (r) =>
            {
                var dialogManager = r as DialogManager;
                dialogManager.OpenBinaryThreeBtnTextDialog_Instance(title, message, confirm, okBtnTxt, cancel, callback);
            });
        }

        TextButtonsDialog OpenBinaryThreeBtnTextDialog_Instance(string title, string message, string confirm, string okBtn, string cancel,
          Action<int> callback = null)
        {
            TextButtonsDialog dialog = OpenCommonDialogWithTextMessage(title, message);
            dialog.SetButtonTexts(cancel, okBtn, confirm);
            if (callback is not null) dialog.OnButtonClickedEvent += index => callback(index);
            currentTextButtonsDialog = dialog;
            return dialog;
        }

        public static void OpenBinaryChoiceScrollviewDialog(string title, List<Player> players, string confirm, string cancel,
            Action<bool> callback = null)
        {
            if (players == null) return;
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, (r) =>
            {
                var dialogManager = r as DialogManager;
                var dialog = dialogManager.OpenBinaryChoiceScrollviewDialog_Instance(title, players, confirm, cancel, callback);

            });
        }

        private TextButtonsDialog OpenBinaryChoiceScrollviewDialog_Instance(string title, List<Player> players, string confirm, string cancel, Action<bool> callback = null
            )
        {
            TextButtonsDialog dialog = OpenScrollview(title, players);
            dialog.SetButtonTexts(cancel, confirm);
            if (callback is not null) dialog.OnButtonClickedEvent += index => callback(index == 1);
            currentTextButtonsDialog = dialog;
            return dialog;
        }

        private TextButtonsDialog OpenCommonDialogWithTextMessage(string title, string message)
        {
            TextButtonsDialog dialog = OpenCommonDialog(title);
            TMP_Text text = Instantiate(commonDialogTextTemplate, dialog.contents);
            text.text = message;
            currentTextButtonsDialog = dialog;
            return dialog;
        }




        private TextButtonsDialog OpenScrollview(string title, List<Player> players)
        {
            TextButtonsDialog dialog = OpenCommonDialog(title);
            CommonDialogScrollRect scrollRect = Instantiate(commonDialogScrollRectTemplate, dialog.contents);
            scrollRect.UpdataData(players);
            return dialog;
        }

        private TextButtonsDialog OpenCommonDialog(string title)
        {
            TextButtonsDialog dialog = Instantiate(commonDialogTemplate, dialogRoot);
            dialog.title = title;
            return dialog;
        }

        public static void OpenCountdownDialog(string title, string message, int countdownSeconds, string confirm = "", string cancel = "", Action<int> callback = null, bool countdownBeforeTitle = true, int[] clickCount = null)
        {
            UIModule.S.OpenPanel(UIDefine.UIID.DialogManagerPanel, (r) =>
            {
                var dialogManager = r as DialogManager;
                dialogManager.OpenCountdownDialog_Instance(title, message, countdownSeconds, confirm, cancel, callback, countdownBeforeTitle, clickCount);
            });


        }

        private TextButtonsDialog OpenCountdownDialog_Instance(string title, string message, int countdownSeconds, string confirm = "", string cancel = "", Action<int> callback = null, bool countdownBeforeTitle = true, int[] clickCount = null)
        {
            string displayMessage;
            if (countdownBeforeTitle)
                displayMessage = $"{countdownSeconds}s{title}";
            else
                displayMessage = $"{title}{countdownSeconds}s";

            TextButtonsDialog dialog = OpenCommonDialogWithTextMessage(displayMessage, message);
            dialog.closeDialogOnBackgroundClicked = false;
            dialog.showCloseButton = false;
            if (confirm != "" && cancel != "")
                dialog.SetButtonTextsWithClickLimits(clickCount, cancel, confirm);
            else if (confirm != "")
                dialog.SetButtonTextsWithClickLimits(clickCount, confirm);
            else if (cancel != "")
                dialog.SetButtonTextsWithClickLimits(clickCount, cancel);
            if (callback is not null) dialog.OnButtonClickedEvent += index => callback(index);
            currentTextButtonsDialog = dialog;
            StartCoroutine(CountdownCoroutine(dialog, title, countdownSeconds, callback, countdownBeforeTitle));
            return dialog;
        }

        private IEnumerator CountdownCoroutine(TextButtonsDialog dialog, string title, int countdownSeconds, Action<int> callback = null, bool countdownBeforeTitle = true)
        {

            while (countdownSeconds > 0)
            {
                yield return new WaitForSeconds(1);
                countdownSeconds--;
                if (dialog.title != null)
                {
                    if (countdownBeforeTitle)
                        dialog.title = $"{countdownSeconds}s{title}";
                    else
                        dialog.title = $"{title}{countdownSeconds}s";
                }
            }
            //callback如果为空，则直接关闭对话框，否则执行回调
            if (callback is not null)
            {
                callback(-1);
            }
            dialog.Close();
        }

    }
}