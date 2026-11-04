using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu
{
    public class UITools
    {
        static List<GraphicRaycaster> s_RaycasterList = new List<GraphicRaycaster>();
        static List<Transform> s_TransformList = new List<Transform>();
        static List<Canvas> s_CanvasList = new List<Canvas>();

        public static void SetGraphicRaycasterState(GameObject root, bool state)
        {
            root.GetComponentsInChildren(true, s_RaycasterList);
            for (int i = 0; i < s_RaycasterList.Count; ++i)
            {
                s_RaycasterList[i].enabled = state;
            }
        }

        public static void SetGameObjectLayer(GameObject root, int layer, bool onlyUI = false)
        {
            if (onlyUI)
            {
                root.GetComponentsInChildren(true, s_CanvasList);
                for (int i = 0; i < s_CanvasList.Count; ++i)
                {
                    s_CanvasList[i].gameObject.layer = layer;
                }
                return;
            }
            root.layer = layer;
            root.GetComponentsInChildren(true, s_TransformList);
            for (int i = 0; i < s_TransformList.Count; ++i)
            {
                s_TransformList[i].gameObject.layer = layer;
            }
        }
    }
}
