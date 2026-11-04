using LetsPiu.Base;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Common.UI
{
    public class TextButton : MonoBehaviour
    {
        public Button button;
        public TMP_Text text;
        public TMP_Text text2;
        public bool m_IsConfim = false;

        private void Start()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if(m_IsConfim)
            {
                //播放确认按钮音效、特效
                AudioPlayModule.S.PlayUIAudio(AudioDefine.UI_Ok);
                //EffectResourceModule.S.AddEffect(1209, Helper.GetEffectParent(1209), (id) =>
                //{
                //    var effect = EffectResourceModule.S.GetEffectGameObject(id);
                //    if (effect)
                //    {
                //        effect.SetActive(true);
                //        //UI坐标等于鼠标点击得位置
                //        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 4f);
                //        var uiPos = UIRoot.S.uiCamera.ScreenToWorldPoint(mousePos);
                //        effect.transform.position = uiPos;

                //    }
                //});
            }
        }

        //点击后button进入不可点击状态，text文本显示倒计时，时间传参，时间一到恢复可点击状态
        public void StartCountdown(float time)
        {
            button.interactable = false;
            text.text = "倒计时：" + time.ToString("F1");
            Invoke("ResetButton", time);
        }

        //恢复button可点击状态
        public void ResetButton()
        {
            button.interactable = true;
            text.text = "再次发送";
        }
    }
}