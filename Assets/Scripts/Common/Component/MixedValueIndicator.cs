using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Common.UI
{
    public class MixedValueIndicator : MonoBehaviour
    {
        public ContinuousValueIndicator unitTemplate;
        
        public int unitCount
        {
            get => _unitCount;
            set => UpdateUnitCount(value);
        }
        
        [SerializeField] private int _unitCount;
        
        public IReadOnlyList<ContinuousValueIndicator> units => _units;
        
        [SerializeField] private List<ContinuousValueIndicator> _units = new();
        
        public float offset
        {
            get => _offset;
            set
            {
                _offset = value;
                UpdateUnitValue();
            }
        }
        
        [SerializeField] private float _offset;
        
        public float value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateUnitValue();
            }
        }
        
        [SerializeField] private float _value;

        private void UpdateUnitCount(int count)
        {
            if (count < 1) count = 1;
            if (_unitCount == count) return;
            if (!unitTemplate) return;

            _unitCount = count;
            
            while (_unitCount > _units.Count)
            {
                ContinuousValueIndicator unit = Instantiate(unitTemplate, transform);
#if  UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(unit.gameObject, "Create Choice");
#endif
                _units.Add(unit);
                unit.name = $"Unit {_units.Count}";
                unit.gameObject.SetActive(true);
            }
            
            while (_unitCount < _units.Count)
            {
                ContinuousValueIndicator unit = _units[^1];
                _units.RemoveAt(_units.Count - 1);
                if (unit)
                {
                    if (Application.isPlaying) Destroy(unit.gameObject);
                    else
                    {
#if  UNITY_EDITOR
                        Undo.DestroyObjectImmediate(unit.gameObject);
#else
                        DestroyImmediate(unit.gameObject);
#endif
                    }
                }
            }

            UpdateUnitValue();
        }

        private void UpdateUnitValue()
        {
            if (_offset < 0) _offset = 0;
            if (_offset > _unitCount) _offset = _unitCount;
            
            if (_value < 0) _value = 0;
            if (_value + _offset > _unitCount) _value = _unitCount - _offset;

            for (int i = 0; i < _unitCount; i++)
            {
                _units[i].offset = Mathf.Clamp01(_offset - i);
                _units[i].value = Mathf.Clamp01(_value + _offset - i) - _units[i].offset;
            }
        }
        
#if UNITY_EDITOR
        [CustomEditor(typeof(MixedValueIndicator))]
        private class ComplexValueIndicatorEditor : Editor
        {
            private MixedValueIndicator indicator;
            
            private void OnEnable()
            {
                indicator = (MixedValueIndicator)target;
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(unitTemplate)));
                serializedObject.ApplyModifiedProperties();
                
                EditorGUI.BeginDisabledGroup(!indicator.unitTemplate);
                Rect position = EditorGUILayout.GetControlRect();
                GUIContent label = EditorGUI.BeginProperty(position, new GUIContent("Unit Count"),
                    serializedObject.FindProperty(nameof(_unitCount)));
                int count = EditorGUI.IntField(position, label, indicator.unitCount);
                if (count < 1) count = 1;
                if (indicator.unitCount != count && indicator.unitTemplate)
                {
                    Undo.RegisterCompleteObjectUndo(indicator, "Updated Unit Count");
                    indicator.UpdateUnitCount(count);
                }
                EditorGUI.EndProperty();
                EditorGUI.EndDisabledGroup();
                
                serializedObject.Update();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_units)));
                EditorGUI.EndDisabledGroup();
                serializedObject.ApplyModifiedProperties();

                EditorGUI.BeginChangeCheck();
                float offset = EditorGUILayout.Slider("Offset", indicator.offset, 0, indicator.unitCount);
                float value = EditorGUILayout.Slider("Value", indicator.value, 0, indicator.unitCount);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterFullObjectHierarchyUndo(indicator.gameObject, "Update Unit Values");
                    indicator.offset = offset;
                    indicator.value = value;
                }
            }
        }
#endif
    }
}
