using UnityEngine;
using UnityEditor;

namespace LetsPiu.Graphics
{
    public class LODEditorController : EditorWindow
    {
        [MenuItem("Tools/LOD Controller/强制所有LOD为第0层")]
        public static void ForceAllLODsToZero()
        {
            int count = 0;
            foreach (LODGroup lodGroup in FindObjectsOfType<LODGroup>())
            {
                lodGroup.ForceLOD(0);
                count++;
            }

            Debug.Log($"已强制 {count} 个 LODGroup 使用 LOD0。");
        }

        [MenuItem("Tools/LOD Controller/恢复所有LOD自动控制")]
        public static void ResetAllLODsToAuto()
        {
            int count = 0;
            foreach (LODGroup lodGroup in FindObjectsOfType<LODGroup>())
            {
                lodGroup.ForceLOD(-1); // 恢复自动
                count++;
            }

            Debug.Log($"已将 {count} 个 LODGroup 恢复为自动LOD。");
        }
    }
}