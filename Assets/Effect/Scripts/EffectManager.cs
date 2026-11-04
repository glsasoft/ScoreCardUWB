using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Effect
{
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance;

        private Stack<SortingGroup> sortingGroups = new();

        private void Awake()
        {
            Instance = this;
        }

        public GameObject InstantiateUIEffect(GameObject template, int sortingOrder, Transform parent = null)
        {
            SortingGroup sortingGroup = AllocateSortingGroup();
            Transform sortingGroupTransform = sortingGroup.transform;
            sortingGroupTransform.SetParent(parent);
            sortingGroupTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            sortingGroup.name = $"Effect Sorting Group: {sortingOrder}";
            sortingGroup.sortingOrder = sortingOrder;

            return Instantiate(template, sortingGroupTransform);
        }

        public void DestroyUIEffect(GameObject effect)
        {
            SortingGroup sortingGroup = effect.GetComponentInParent<SortingGroup>();
            ReleaseSortingGroup(sortingGroup);

            Destroy(effect);
        }

        private SortingGroup AllocateSortingGroup()
        {
            if (sortingGroups.TryPop(out SortingGroup sortingGroup))
            {
                sortingGroup.gameObject.SetActive(true);
                return sortingGroup;
            }
            else
            {
                GameObject sortingGroupObject = new("Effect Sorting Group");
                return sortingGroupObject.AddComponent<SortingGroup>();
            }
        }

        private void ReleaseSortingGroup(SortingGroup sortingGroup)
        {
            sortingGroup.gameObject.SetActive(false);
            sortingGroups.Push(sortingGroup);
        }
        
#if UNITY_EDITOR
        [CustomEditor(typeof(EffectManager))]
        private class EffectManagerEditor : Editor
        {
            private EffectManager manager;
            
            private class EditorBuffer
            {
                public bool foldDebugger;
                public GameObject template;
                public int sortingOrder;
                public Transform parent;
                public GameObject effect;
            }

            private static readonly Dictionary<EffectManager, EditorBuffer> buffers = new();
            private EditorBuffer buffer;
            
            private void OnEnable()
            {
                manager = (EffectManager)target;
                
                if (!buffers.TryGetValue(manager, out buffer))
                {
                    buffer = new EditorBuffer();
                    buffers.Add(manager, buffer);
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                buffer.foldDebugger = EditorGUILayout.Foldout(buffer.foldDebugger, "Debugger");
                if (buffer.foldDebugger)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                    
                    buffer.template =
                        (GameObject)EditorGUILayout.ObjectField("Template", buffer.template, typeof(GameObject), true);
                    buffer.sortingOrder = EditorGUILayout.IntField("Sorting Order", buffer.sortingOrder);
                    buffer.parent =
                        (Transform)EditorGUILayout.ObjectField("Parent", buffer.parent, typeof(Transform), true);
                    if (GUILayout.Button("Instantiate"))
                        buffer.effect = manager.InstantiateUIEffect(buffer.template, buffer.sortingOrder, buffer.parent);
                    
                    buffer.effect =
                        (GameObject)EditorGUILayout.ObjectField("Effect", buffer.effect, typeof(GameObject), true);
                    if (GUILayout.Button("Destroy"))
                    {
                        manager.DestroyUIEffect(buffer.effect);
                        buffer.effect = null;
                    }
                    
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
            }
        }
#endif
    }
}