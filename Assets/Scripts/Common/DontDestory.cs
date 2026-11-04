using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class DontDestory : MonoBehaviour
    {
        void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
/*
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        #region FPS
        private float deltaTime = 0.0f;
        private List<float> fpsRecords = new List<float>();
        private float tenSecondTimer = 0.0f;
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            float currentFPS = 1.0f / deltaTime;

            fpsRecords.Add(currentFPS);

            tenSecondTimer += Time.unscaledDeltaTime;

            while (tenSecondTimer > 10.0f && fpsRecords.Count > 0)
            {
                tenSecondTimer -= 10.0f / fpsRecords.Count;
                fpsRecords.RemoveAt(0);
            }
        }

        GUIStyle style;
        void OnGUI()
        {
            int width = Screen.width, height = Screen.height;
            if (style == null)
            {
                style = new GUIStyle();
                style.alignment = TextAnchor.UpperLeft;
                style.clipping = TextClipping.Overflow;
                //style.fontSize = height * 2 / 80;
                style.normal.textColor = Color.green;
            }
            style.fontSize = height * 2 / 80;

            Rect rect = new Rect(width / 2 - width / 4, 10, width / 2, height * 2 / 100);
            //Rect rect = new Rect(10, 10, width/2, height * 2 / 100);

            float fps = 1.0f / deltaTime;
            float averageFPS = 0f;
            if (fpsRecords.Count > 0)
            {
                foreach (float record in fpsRecords)
                {
                    averageFPS += record;
                }
                averageFPS /= fpsRecords.Count;
            }
            string text = string.Format("FPS: {0:0.} | 10s Avg: {1:0.}", fps, averageFPS);
            GUI.Label(rect, text, style);
        }
        #endregion
#endif
*/
    }
}
