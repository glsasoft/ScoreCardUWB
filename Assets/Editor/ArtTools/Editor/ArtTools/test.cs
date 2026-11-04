using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtTools
{
   
    public class Test : EditorWindow
    {

        public  string m_path ;
        
        public  string m_Newpath ;
        
        
       // [MenuItem("美术工具/Test")]
        static void Init()
        {
            var window = GetWindow(typeof(Test));

        }


        private void OnGUI()
        {
            EditorGUILayout.LabelField("查找文件夹路径(不要有子文件夹)");

            GUILayout.BeginHorizontal();
            m_path = GUILayout.TextField(m_path);
            if (GUILayout.Button(". . .", GUILayout.Width(30)))
                m_path = EditorUtility.OpenFolderPanel("选择路径", "", "").Replace(Application.dataPath, "Assets");
            GUILayout.EndHorizontal();
            
            
            EditorGUILayout.LabelField("新文件夹路径");

            GUILayout.BeginHorizontal();
            m_Newpath = GUILayout.TextField(m_Newpath);
            if (GUILayout.Button(". . .", GUILayout.Width(30)))
                m_Newpath = EditorUtility.OpenFolderPanel("选择路径", "", "").Replace(Application.dataPath, "Assets");
            GUILayout.EndHorizontal();



            if (GUILayout.Button("确认"))
                test();

        }


       
        
      
        public  void test()
        {
            List<string> obj = new List<string>();

          //  string path2 = m_path;
            //  Object o = ;
            foreach (var VARIABLE in Selection.assetGUIDs)
            {
                string path =AssetDatabase.GUIDToAssetPath(VARIABLE) ;

                if (path.Contains(m_path))
                {
                    Object a = AssetDatabase.LoadAssetAtPath<Object>(path);
                    string newPath =path.Replace(m_path, m_Newpath);
                    AssetDatabase.MoveAsset(path,newPath);
                        
                  Debug.LogError(path);
                   
                }
            }



          



        }
    }
    
}