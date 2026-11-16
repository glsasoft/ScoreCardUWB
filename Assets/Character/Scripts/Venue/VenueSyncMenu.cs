#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LetsPiu.Character
{
    static class VenueSyncMenu
    {
        [MenuItem("LetsPiu/真人CS/创建位置同步")]
        static void Create()
        {
            var root = new GameObject("VenueSync");
            root.AddComponent<VenueSpace>();
            root.AddComponent<VenuePlayerSync>();
            root.AddComponent<VenuePlayerMockFeed>();
            Selection.activeGameObject = root;
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(root.scene);
        }
    }
}
#endif
