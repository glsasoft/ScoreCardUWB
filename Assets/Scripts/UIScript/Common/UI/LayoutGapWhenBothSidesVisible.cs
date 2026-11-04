using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class LayoutGapWhenBothSidesVisible : MonoBehaviour
    {
        [SerializeField] RectTransform leftSide;
        [SerializeField] RectTransform rightSide;
        [SerializeField] GameObject gapObject;
        [SerializeField] bool updateEveryFrame = true;

        bool m_LastActive = true;

        void OnEnable()
        {
            Refresh(true);
        }

        void OnTransformChildrenChanged()
        {
            Refresh(true);
        }

        void LateUpdate()
        {
            if (!updateEveryFrame) return;
            Refresh(false);
        }

        void Refresh(bool force)
        {
            if (gapObject == null) return;

            bool shouldShow = IsLeftVisible() && HasVisibleRightChild();
            if (!force && shouldShow == m_LastActive) return;

            if (gapObject.activeSelf != shouldShow)
            {
                gapObject.SetActive(shouldShow);
            }

            m_LastActive = shouldShow;

            var self = transform as RectTransform;
            if (self != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(self);
            }
        }

        bool IsLeftVisible()
        {
            return leftSide != null && leftSide.gameObject.activeInHierarchy;
        }

        bool HasVisibleRightChild()
        {
            if (rightSide == null || !rightSide.gameObject.activeInHierarchy)
            {
                return false;
            }

            for (int i = 0; i < rightSide.childCount; i++)
            {
                var child = rightSide.GetChild(i);
                if (child != null && child.gameObject.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
