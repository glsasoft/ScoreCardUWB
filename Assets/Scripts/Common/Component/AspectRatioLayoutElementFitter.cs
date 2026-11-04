using System;
using UnityEngine;
using UnityEngine.UI;
using AspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode;

namespace LetsPiu.Common.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(LayoutElement))]
    public class AspectRatioLayoutElementFitter : MonoBehaviour
    {
        public AspectMode aspectMode;
        public float aspectRation;

        private LayoutElement layoutElement;
        private new RectTransform transform;
        private RectTransform parent;

        private void Awake()
        {
            layoutElement = GetComponent<LayoutElement>();
            transform = GetComponent<RectTransform>();
            parent = (RectTransform)transform.parent;
        }

        private void Update()
        {
            switch (aspectMode)
            {
                case AspectMode.None:
                    break;
                case AspectMode.WidthControlsHeight:
                    layoutElement.preferredHeight = transform.rect.width / aspectRation;
                    break;
                case AspectMode.HeightControlsWidth:
                    layoutElement.preferredWidth = transform.rect.height * aspectRation;
                    break;
                case AspectMode.FitInParent:
                    Rect rect = parent.rect;
                    float width = Mathf.Min(rect.width, rect.height * aspectRation);
                    layoutElement.preferredWidth = width;
                    layoutElement.preferredHeight = width / aspectRation;
                    break;
                case AspectMode.EnvelopeParent:
                    rect = parent.rect;
                    width = Mathf.Max(rect.width, rect.height * aspectRation);
                    layoutElement.preferredWidth = width;
                    layoutElement.preferredHeight = width / aspectRation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Reset()
        {
            transform = GetComponent<RectTransform>();
            Rect rect = transform.rect;
            aspectRation = rect.height / rect.width;
        }
    }
}
