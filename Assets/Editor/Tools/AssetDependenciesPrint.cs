using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LetsPiu
{
    public class AssetDependenciesPrint
    {
        [MenuItem("Assets/Print Dependencies", false, 30)]
        static void PrintDependencies()
        {
            var obj = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(obj);
            string[] dependencies = AssetDatabase.GetDependencies(path);
            List<string> filterDependencies = new List<string>();
            for (int i = 0; i < dependencies.Length; i++)
            {
                if(Path.GetExtension(dependencies[i]) != ".cs")
                {
                    filterDependencies.Add(dependencies[i]);
                }
            }
            Debug.Log(obj.name + " 依赖的资源有：\n" + string.Join("\n", filterDependencies));
        }

        [MenuItem("Assets/Print Dependencies", true, 30)]
        static bool ValidatePrintDependencies()
        {
            var obj = Selection.activeObject;
            if (obj != null && AssetDatabase.Contains(obj))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                return !AssetDatabase.IsValidFolder(path);
            }
            return false;
        }
    }
}
