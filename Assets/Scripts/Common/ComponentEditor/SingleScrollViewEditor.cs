using UnityEngine;
using UnityEngine.UI;
using LetsPiu.Component;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(SingleScrollView), true)]
    [CanEditMultipleObjects]
    public class SingleScrollViewEditor : ScrollRectEditor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_CellPadding;
        SerializedProperty m_CellTemplate;
        SerializedProperty m_CellWidth;
        SerializedProperty m_CellHeight;
        SerializedProperty m_MaxCreateCountPerFrame;
        SerializedProperty m_DisableDragIfFits;
        SerializedProperty m_IsResizeCell;
        SerializedProperty m_NotDataObject;

        string m_InvalidWarning;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_CellPadding = serializedObject.FindProperty("m_CellPadding");
            m_CellTemplate = serializedObject.FindProperty("m_CellTemplate");
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
            EditorGUILayout.PropertyField(m_CellPadding);
            EditorGUILayout.PropertyField(m_CellTemplate);
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

                ScrollView scrollView = target as ScrollView;
                int tempValue = EditorGUILayout.IntField("SizeInEditor", scrollView.totalCount);
                if (tempValue != scrollView.totalCount)
                {
                    m_InvalidWarning = string.Empty;

                    try
                    {
                        scrollView.Refresh(tempValue);
                    }
                    catch (System.Exception e)
                    {
                        m_InvalidWarning = e.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(m_InvalidWarning))
                {
                    EditorGUILayout.HelpBox(m_InvalidWarning, MessageType.Error);
                }
            }
        }
    }
}
