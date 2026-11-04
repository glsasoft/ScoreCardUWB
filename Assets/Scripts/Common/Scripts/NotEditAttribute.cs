using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Main.Common.Scripts
{
    public class NotEditAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NotEditAttribute))]
    public class NotEditAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //禁用GUI，使字段不可用
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            //恢复GUI字段
            EditorGUI.EndDisabledGroup();
        }
    }
#endif
}
