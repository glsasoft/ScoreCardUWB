using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace LetsPiu.Common
{
    public class UniversalClickCooldown : MonoBehaviour, IPointerClickHandler
    {
        public float cooldown = 1f;
        private Selectable selectable;
        private bool isCoolingDown = false;

        void Awake()
        {
            selectable = GetComponent<Selectable>(); 
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isCoolingDown)
            {
                eventData.Use(); // 阻止多次点击
                return;
            }
            StartCoroutine("CooldownCoroutine");
            // 这里可以写你自己的点击逻辑，或让外部监听
        }

        private IEnumerator CooldownCoroutine()
        {
            isCoolingDown = true;
            if (selectable != null)
            {
                selectable.interactable = false;
            }

            yield return new WaitForSeconds(cooldown);

            if (selectable != null)
            {
                selectable.interactable = true;
            }
            isCoolingDown = false;
        }
        private void OnDisable()
        {
            StopCoroutine("CooldownCoroutine");
        }
    }
}
