using LetsPiu.Main.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Common.UI
{
    public class EffectDepth : MonoBehaviour
    {
        [SerializeField][NotEdit]
        private int baseOrder = 0;
        public int order = 0;
        public bool executeOnStart = false;

        private Renderer[] m_renderers;

        private void Awake()
        {
            m_renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Start()
        {
            if (executeOnStart)
                Execute();
        }

        void GetCanvasOrder()
        {
            Canvas canvas = null;
            Transform parent = transform.parent;
            while (parent)
            {
                canvas = parent.GetComponent<Canvas>();
                if (canvas && canvas.GetComponent<AbstractPage>())
                {
                    break;
                }
                else
                {
                    canvas = null;
                    parent = parent.parent;
                }
            }
            if (canvas)
                baseOrder = canvas.sortingOrder;
        }

        [ContextMenu("Execute")]
        public void Execute(bool getLatestRenderer = false)
        {
            if (getLatestRenderer)
                m_renderers = GetComponentsInChildren<Renderer>(true);

            GetCanvasOrder();

            int finalOrder = baseOrder + order;

            foreach (Renderer r in m_renderers)
            {
                if (r != null)
                    r.sortingOrder = finalOrder;
            } 
        }
    }
}
