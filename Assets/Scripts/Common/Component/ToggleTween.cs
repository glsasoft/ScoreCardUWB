using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Component.Common.Component
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleTween : MonoBehaviour
    {
        public Image handle;
        public Transform onPos;
        public Transform offPos;
        [SerializeField] private Sprite onIcon;
        [SerializeField] private Sprite offIcon;
        public float duration = 0.2f;
        public Ease ease = Ease.OutQuad;

        Toggle m_Toggle;
        Tweener m_Tweener;

        private void Awake()
        {
            m_Toggle = GetComponent<Toggle>(); 
        }

        private void OnEnable()
        {
            m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
            handle.sprite = m_Toggle.isOn ? onIcon : offIcon;
            if (m_Toggle.isOn)
            {
                if (handle && onPos)
                {
                    handle.transform.localPosition = onPos.localPosition;
                }
            }
            else
            {
                if (handle && offPos)
                {
                    handle.transform.localPosition = offPos.localPosition;
                }
            }
        }

        private void OnDisable()
        {
            m_Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        void OnToggleValueChanged(bool v)
        {
            if (m_Tweener != null)
            {
                m_Tweener.Kill();
            }

            handle.sprite = v ? onIcon : offIcon;
            if (v)
            {
                if (handle && onPos)
                {
                    if (duration > 0)
                        m_Tweener = handle.transform.DOLocalMove(onPos.localPosition, duration).SetEase(ease);
                    else
                        handle.transform.localPosition = onPos.localPosition;
                }
            }
            else
            {
                if (handle && offPos)
                {
                    if (duration > 0)
                        m_Tweener = handle.transform.DOLocalMove(offPos.localPosition, duration).SetEase(ease);
                    else
                        handle.transform.localPosition = offPos.localPosition;
                }
            }
        }
    }
}
