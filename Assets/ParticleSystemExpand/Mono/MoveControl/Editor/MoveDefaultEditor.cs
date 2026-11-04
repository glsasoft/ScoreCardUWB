using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MoveDefault_SO))]
public class MoveDefaultEditor : Editor
{
    private SerializedObject MoveDefault;
    private SerializedProperty t;
    private SerializedProperty displacement;
    private SerializedProperty speedStart;
    private SerializedProperty finalVelocity;
    private SerializedProperty accleration;
    private SerializedProperty lift;
    private SerializedProperty moveType;

    private SerializedProperty currentVelocity;
    private SerializedProperty currentTime;
    public bool showHide = true;
    private void OnEnable()
    {
        MoveDefault = new SerializedObject(target);
        Get(ref t, "t");
        Get(ref displacement, "displacement");
        Get(ref speedStart, "speedStart");
        Get(ref finalVelocity, "finalVelocity");
        Get(ref accleration, "accleration");
        Get(ref lift, "lift");
        Get(ref moveType, "moveType");
        Get(ref currentVelocity, "currentVelocity");
        Get(ref currentTime, "currentTime");

       
    }
    private void Get(ref SerializedProperty serializedProperty, string name)
    {
        serializedProperty = MoveDefault.FindProperty(name);
    }
    public override void OnInspectorGUI()
    {
        MoveDefault.Update();//更新面板信息
        EditorGUILayout.PropertyField(moveType);
        EditorGUILayout.PropertyField(lift);
        switch (moveType.enumValueIndex)
        {
            case (int)MoveDefault_SO.MoveType.Time:
                {
                    EditorGUILayout.PropertyField(t);
                    break;
                }
            case (int)MoveDefault_SO.MoveType.Distance:
                {
                    EditorGUILayout.PropertyField(displacement);
                    break;
                }
        }

        EditorGUILayout.PropertyField(speedStart);
        EditorGUILayout.PropertyField(finalVelocity);
        EditorGUILayout.PropertyField(accleration);
        if(GUILayout.Button(showHide?"隐藏变量": "显示隐藏变量"))
        {
            showHide = !showHide;
        }
        if(showHide)
        {
            EditorGUILayout.PropertyField(currentVelocity);
            EditorGUILayout.PropertyField(currentTime);
        }
        MoveDefault.ApplyModifiedProperties();
    }
}
