using UnityEngine;
using UnityEngine.UI;
using LetsPiu.Component;
using UnityEditor;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(CarouselScrollView), true)]
    [CanEditMultipleObjects]
    public class CarouselScrollViewEditor : ScrollRectEditor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_CellTemplate;
        SerializedProperty m_MaxWidth;
        SerializedProperty m_CellWidth;
        SerializedProperty m_CellHeight;
        SerializedProperty m_MaxCreateCountPerFrame;
        SerializedProperty m_DisableDragIfFits;
        SerializedProperty m_IsResizeCell;
        SerializedProperty m_NotDataObject;

        string m_StrInvalidWarning;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_CellTemplate = serializedObject.FindProperty("m_CellTemplate");
            m_MaxWidth = serializedObject.FindProperty("m_MaxWidth");
            m_CellWidth = serializedObject.FindProperty("m_CellWidth");
            m_CellHeight = serializedObject.FindProperty("m_CellHeight");
            m_MaxCreateCountPerFrame = serializedObject.FindProperty("m_MaxCreateCountPerFrame");
            m_DisableDragIfFits = serializedObject.FindProperty("m_DisableDragIfFits");
            m_IsResizeCell = serializedObject.FindProperty("m_IsResizeCell");
            m_NotDataObject = serializedObject.FindProperty("m_NotDataObject");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ScrollViewPropertyGUI();
            ScrollViewShowGUI();
            serializedObject.ApplyModifiedProperties();
        }

        void ScrollViewPropertyGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(m_Padding, true);
            EditorGUILayout.PropertyField(m_CellTemplate);
            EditorGUILayout.PropertyField(m_MaxWidth);
            EditorGUILayout.PropertyField(m_CellWidth);
            EditorGUILayout.PropertyField(m_CellHeight);
            EditorGUILayout.PropertyField(m_MaxCreateCountPerFrame);
            EditorGUILayout.PropertyField(m_DisableDragIfFits);
            EditorGUILayout.PropertyField(m_IsResizeCell);
            EditorGUILayout.PropertyField(m_NotDataObject);
            EditorGUILayout.EndVertical();
        }

        void ScrollViewShowGUI()
        {
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("-----Editor mode-----");

                CarouselScrollView scrollView = target as CarouselScrollView;
                int tempValue = EditorGUILayout.IntField("SizeInEditor", scrollView.totalCount);
                if (tempValue != scrollView.totalCount)
                {
                    m_StrInvalidWarning = string.Empty;

                    try
                    {
                        scrollView.Refresh(tempValue);
                    }
                    catch (System.Exception e)
                    {
                        m_StrInvalidWarning = e.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(m_StrInvalidWarning))
                {
                    EditorGUILayout.HelpBox(m_StrInvalidWarning, MessageType.Error);
                }
            }
        }
    }
}
