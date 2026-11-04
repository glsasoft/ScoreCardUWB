using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Common.UI
{
    public class DiscreteValueIndicator : MonoBehaviour
    {
        public Transform indicatorRoot;
        
        public int value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateValue();
            }
        }
        
        [SerializeField, HideInInspector] private int _value;

        private void UpdateValue()
        {
            if (!indicatorRoot) return;
            int childCount = indicatorRoot.childCount;
            
            if (_value < 0) _value = 0;
            if (_value > childCount) _value = childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = indicatorRoot.GetChild(i);
                child.gameObject.SetActive(_value > i);
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(DiscreteValueIndicator))]
        private class DiscreteValueIndicatorEditor : Editor
        {
            private DiscreteValueIndicator indicator;

            private void OnEnable()
            {
                indicator = (DiscreteValueIndicator)target;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                
                if (!indicator.indicatorRoot) return;
                
                serializedObject.Update();
                SerializedProperty valueProperty = serializedObject.FindProperty(nameof(_value));
                valueProperty.intValue = EditorGUILayout.IntSlider("Value",
                    valueProperty.intValue, 0, indicator.indicatorRoot.childCount);
                if (serializedObject.ApplyModifiedProperties())
                {
                    Undo.RegisterFullObjectHierarchyUndo(indicator.indicatorRoot.gameObject,
                        "Set Indicators' Active State");
                    indicator.UpdateValue();
                }
            }
        }
#endif
    }
}
