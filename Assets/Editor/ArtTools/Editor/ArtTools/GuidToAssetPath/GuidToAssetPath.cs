using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtTools
{
    public class GuidToAssetPath : ScriptableWizard
    {
        public string guid;
        
        [MenuItem("Tools/gui查找物体")]
        static void  CreateWizard() 
        {
            ScriptableWizard.DisplayWizard<GuidToAssetPath>("gui查找物体", "Replace");
        }

        private void OnWizardCreate()
        {

            string path =AssetDatabase.GUIDToAssetPath(guid) ;
            var a = AssetDatabase.LoadAssetAtPath<Object>(path);
            Selection.activeObject = a;
           Debug.LogError(path);
        }
    }
}