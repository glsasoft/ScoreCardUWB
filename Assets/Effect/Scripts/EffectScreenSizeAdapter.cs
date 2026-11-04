using System;
using System.Reflection;
using LetsPiu.Base;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Effect
{
    [ExecuteAlways]
    public class EffectScreenSizeAdapter : MonoBehaviour
    {
        [SerializeField, HideInInspector] private bool initialized;
        public Vector2 referenceScreenSize;
        public Vector3 referencePosition;
        public Vector3 referenceScale;
        public float referenceFOV = 40;

        [Range(0, 1)] public float matchWidthOrHeight = 1;
        public Vector3 pivot;

        private Transform effectTransform;

        #region Screen Size
            
        #if UNITY_EDITOR
        private static int sizeFrame = -1;
        private static MethodInfo GetSizeOfMainGameView;
        private static Vector2 gameSize = Vector2.zero;

        public static Vector2 screenSize
        {
            get
            {
                int frame = Time.frameCount;
        
                if (sizeFrame != frame || !Application.isPlaying)
                {
                    sizeFrame = frame;
        
                    if (GetSizeOfMainGameView is null)
                    {
                        Type type = Type.GetType("UnityEditor.GameView,UnityEditor");
                        GetSizeOfMainGameView = type?.GetMethod("GetSizeOfMainGameView",
                            BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    }
                    gameSize = (Vector2)(GetSizeOfMainGameView?.Invoke(null, null) ?? Vector2.zero);
                }
                return gameSize;
            }
        }
        #else
        public static Vector2 screenSize => new(Screen.width, Screen.height);
        #endif

        #endregion

        private void Awake()
        {
            Resize();
        }

#if UNITY_EDITOR
        private void Update()
        {
            Resize();
        }
#endif

        private void Resize()
        {
            Vector2 actualScreenSize = screenSize;
            Vector2 targetScreenSizeByWidth = actualScreenSize / actualScreenSize.x * referenceScreenSize.x;
            Vector2 targetScreenSizeByHeight = actualScreenSize / actualScreenSize.y * referenceScreenSize.y;
            Vector2 targetScreenSize =
                Vector2.Lerp(targetScreenSizeByWidth, targetScreenSizeByHeight, matchWidthOrHeight);
            Vector2 ratio = targetScreenSize / referenceScreenSize;

            Vector3 position = referencePosition;
            position.x += (1 - ratio.x) * pivot.x;
            position.y += (1 - ratio.y) * pivot.y;

            Vector3 scale = referenceScale;
            scale.x *= ratio.x;
            scale.y *= ratio.y;

            //Camera cam = CameraMgr.S?.mainCamera ?? Camera.main ?? FindObjectOfType<Camera>(); //组件不支持null条件运算符(?.)与null合并运算符(??)
            Camera cam = Camera.main;
            if (CameraMgr.S && CameraMgr.S.mainCamera)
                cam = CameraMgr.S.mainCamera;
            if (cam == null)
                cam = FindObjectOfType<Camera>();
            if (cam)
                scale *= Mathf.Tan(cam.fieldOfView / 2 * Mathf.Deg2Rad) / Mathf.Tan(referenceFOV / 2 * Mathf.Deg2Rad);

            Transform controlTransform = transform;
            if (Application.isPlaying)
            {
                InitEffectTransform();
                controlTransform = effectTransform;
            }
            controlTransform.localPosition = position;
            controlTransform.localScale = scale;
        }

        void InitEffectTransform()
        {
            if (effectTransform == null)
            {
                Transform[] childs = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                    childs[i] = transform.GetChild(i);

                string effectName = this.name.Replace("(Clone)", "").Trim();
                effectTransform = new GameObject(effectName).transform;
                effectTransform.SetParent(transform, false);
                effectTransform.localPosition = transform.localPosition;
                effectTransform.localScale = transform.localScale;
                effectTransform.gameObject.layer = gameObject.layer;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;

                foreach (Transform child in childs)
                    child.SetParent(effectTransform, false);
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(EffectScreenSizeAdapter))]
        private class EffectScreenSizeAdapterEditor : Editor
        {
            
            private EffectScreenSizeAdapter adapter;
            
            private void OnEnable()
            {
                adapter = (EffectScreenSizeAdapter)target;

                if (!adapter.initialized) SetReferences();
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                
                if (GUILayout.Button("重置引用")) SetReferences();
            }

            private void OnSceneGUI()
            {
                serializedObject.Update();
                SerializedProperty pivotProperty = serializedObject.FindProperty(nameof(pivot));
                Transform trans = adapter.transform;
                Vector3 p = trans.TransformPoint(pivotProperty.vector3Value);
                Vector3 np = Handles.PositionHandle(p, trans.rotation);
                if (np != p) pivotProperty.vector3Value = trans.InverseTransformPoint(np);
                serializedObject.ApplyModifiedProperties();
            }

            private void SetReferences()
            {
                adapter.initialized = true;
                adapter.referenceScreenSize = screenSize;
                adapter.referencePosition = adapter.transform.localPosition;
                adapter.referenceScale = adapter.transform.localScale;
            }
        }
#endif
    }
}
