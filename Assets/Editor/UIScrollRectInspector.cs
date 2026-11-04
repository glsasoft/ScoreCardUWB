using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor;

namespace LetsPiu
{
    //[CustomEditor(typeof(UIScrollRect), true)]
    [CanEditMultipleObjects]
    public class UIScrollRectInspector : UnityEditor.Editor
    {
        SerializedProperty m_Content;
        protected SerializedProperty m_MovementAxis;
        SerializedProperty m_MovementType;
        SerializedProperty m_Elasticity;
        SerializedProperty m_Inertia;
        SerializedProperty m_DecelerationRate;
        SerializedProperty m_ScrollSensitivity;
        SerializedProperty m_Viewport;
        SerializedProperty m_HorizontalScrollbar;
        SerializedProperty m_HorizontalScrollbarSpacing;
        SerializedProperty m_HorizontalScrollbarVisibility;
        SerializedProperty m_VerticalScrollbar;
        SerializedProperty m_VerticalScrollbarSpacing;
        SerializedProperty m_VerticalScrollbarVisibility;
        AnimBool m_ShowElasticity;
        AnimBool m_ShowDecelerationRate;

        protected virtual void OnEnable()
        {
            m_Viewport = serializedObject.FindProperty("m_Viewport");
            m_Content = serializedObject.FindProperty("m_Content");
            m_MovementAxis = serializedObject.FindProperty("m_MovementAxis");
            m_MovementType = serializedObject.FindProperty("m_MovementType");
            m_Elasticity = serializedObject.FindProperty("m_Elasticity");
            m_Inertia = serializedObject.FindProperty("m_Inertia");
            m_DecelerationRate = serializedObject.FindProperty("m_DecelerationRate");
            m_ScrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
            m_HorizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
            m_VerticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");
            m_HorizontalScrollbarSpacing = serializedObject.FindProperty("m_HorizontalScrollbarSpacing");
            m_VerticalScrollbarSpacing = serializedObject.FindProperty("m_VerticalScrollbarSpacing");
            m_HorizontalScrollbarVisibility = serializedObject.FindProperty("m_HorizontalScrollbarVisibility");
            m_VerticalScrollbarVisibility = serializedObject.FindProperty("m_VerticalScrollbarVisibility");


            m_ShowElasticity = new AnimBool(Repaint);
            m_ShowDecelerationRate = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable()
        {
            m_ShowElasticity.valueChanged.RemoveListener(Repaint);
            m_ShowDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        void SetAnimBools(bool instant)
        {
            SetAnimBool(m_ShowElasticity, !m_MovementType.hasMultipleDifferentValues && m_MovementType.enumValueIndex == (int)ScrollRect.MovementType.Elastic, instant);
            SetAnimBool(m_ShowDecelerationRate, !m_Inertia.hasMultipleDifferentValues && m_Inertia.boolValue == true, instant);
        }

        void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

        public override void OnInspectorGUI()
        {
            SetAnimBools(false);

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Viewport);
            EditorGUILayout.PropertyField(m_Content);
            EditorGUILayout.PropertyField(m_MovementAxis);
            EditorGUILayout.PropertyField(m_MovementType);
            EditorGUILayout.PropertyField(m_HorizontalScrollbar);
            EditorGUILayout.PropertyField(m_HorizontalScrollbarSpacing);
            EditorGUILayout.PropertyField(m_HorizontalScrollbarVisibility);
            EditorGUILayout.PropertyField(m_VerticalScrollbar);
            EditorGUILayout.PropertyField(m_VerticalScrollbarSpacing);
            EditorGUILayout.PropertyField(m_VerticalScrollbarVisibility);
            if (EditorGUILayout.BeginFadeGroup(m_ShowElasticity.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Elasticity);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(m_Inertia);
            if (EditorGUILayout.BeginFadeGroup(m_ShowDecelerationRate.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DecelerationRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.PropertyField(m_ScrollSensitivity);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}