using System;
using System.Collections.Generic;
using LetsPiu.Base;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu
{
    
    public class HideInvisibleChildren : MonoBehaviour
    {
        [Serializable]
        public struct ChildData
        {
            public MeshRenderer renderer;
            public Bounds bounds;
        }
        
        public ChildData[] children;
        
        private new Camera camera;
        private Pose lastPose;

        private float disLimit = 0.5f * 0.5f;
        private float angleLimit = 2;

        private void CollectChildren()
        {
            List<ChildData> list = new();
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            // 获取渲染器和包围盒
            foreach (MeshRenderer r in renderers) list.Add(new ChildData { renderer = r, bounds = r.bounds });
            children = list.ToArray();  // 将list转化为数组，赋给children
        }

        private void Awake()
        {
            //Unity不会渲染视椎体以外的对象
            this.enabled = false;
        }

        private void Start()
        {
            camera = CameraMgr.S? CameraMgr.S.mainCamera : Camera.main;
            lastPose = Pose.identity;
        }
        private void LateUpdate()
        {
            if ((camera.transform.position - lastPose.position).sqrMagnitude < disLimit && Quaternion.Angle(camera.transform.rotation, lastPose.rotation) < angleLimit)
                return;

            lastPose.position = camera.transform.position;
            lastPose.rotation = camera.transform.rotation;

            // 获取摄像机视锥体平面
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            
            foreach (ChildData child in children)
                child.renderer.enabled = GeometryUtility.TestPlanesAABB(planes, child.bounds); // 判断相交
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(HideInvisibleChildren))]
        private class HideInvisibleChildrenEditor : Editor
        {
            private HideInvisibleChildren _target;

            private void OnEnable()
            {
                _target = (HideInvisibleChildren)target;    // target：Editor 基类中的内置属性，指当前 Inspector 正在编辑的对象
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Collect Children")) _target.CollectChildren();
            }
        }
#endif
    }
}
