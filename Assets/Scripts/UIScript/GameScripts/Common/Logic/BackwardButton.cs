using UnityEngine;
using UnityEngine.Events;

namespace LetsPiu.UI.UIScript.Common.Logic
{
    public class BackwardButton : MonoBehaviour
    {
        [SerializeField] private AbstractPage page;
        [SerializeField] public UnityEvent onPageCloseEvent;

        public void OnClick()
        {
            page?.CloseSelf();
            onPageCloseEvent?.Invoke();
        }
    }
}
