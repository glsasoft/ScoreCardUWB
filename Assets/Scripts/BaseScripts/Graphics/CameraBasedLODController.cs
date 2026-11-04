using UnityEngine;
using System.Collections.Generic;
using LetsPiu.Base;

namespace LetsPiu.Graphics
{
    [RequireComponent(typeof(Camera))]
    public class CameraBasedLODController : MonoBehaviour
    {
        [Tooltip("设置屏幕像素大小对应的LOD层级阈值")]
        public List<float> screenSizeThresholds = new List<float> { 50f, 15f, 1f }; // LOD0 >50px, LOD1 >15px, etc.
        
        private Camera mainCamera;
        public List<Transform> players = new List<Transform>();
        private int playerLayer;
        private int enemyPlayerLayer;

        void Awake()
        {
            mainCamera = CameraMgr.S ? CameraMgr.S.mainCamera : Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("CameraBasedLODController: 主相机未找到！");
                return;
            }
            // 获取层级
            playerLayer = LayerMask.NameToLayer("Player");
            enemyPlayerLayer = LayerMask.NameToLayer("EnermyPlayer");
            // 获取场景中的Player
            RefreshPlayersFromScene();
        }

        void RefreshPlayersFromScene()
        {
            players.Clear();
            LODGroup[] allLODGroups = GameObject.FindObjectsOfType<LODGroup>();
            foreach (LODGroup lodGroup in allLODGroups)
            {
                GameObject obj = lodGroup.gameObject;
                int objLayer = obj.layer;
                
                if (objLayer == playerLayer || objLayer == enemyPlayerLayer)
                {
                    players.Add(obj.transform);
                }
            }
        }

        void LateUpdate()
        {
            if (mainCamera == null) return;
            foreach (Transform player in players)
            {
                if (player == null) continue;
                
                LODGroup lodGroup = player.GetComponent<LODGroup>();
                if (lodGroup == null) continue;

                float screenSize = CalculateObjectScreenSize(player);
                int lodLevel = GetLODLevelFromScreenSize(screenSize);
                lodGroup.ForceLOD(lodLevel);
            }
        }

        float CalculateObjectScreenSize(Transform target)
        {
            // 背面剔除
            Vector3 toTarget = (target.position - mainCamera.transform.position).normalized;
            if (Vector3.Dot(mainCamera.transform.forward, toTarget) <= 0f)
                return 0f; // 物体在摄像机背面
            
            Renderer rend = target.GetComponentInChildren<Renderer>();
            if (rend == null) return 0f;
            
            Bounds bounds = rend.bounds;            // 模型的世界空间包围盒
            Vector3 boundsCenter = bounds.center;   // 包围盒中心位置
            float radius = bounds.extents.magnitude;    // 包围盒对角线的一半

            // 世界坐标转换到屏幕坐标
            Vector3 screenPosCenter = mainCamera.WorldToScreenPoint(boundsCenter);
            Vector3 screenPosEdge = mainCamera.WorldToScreenPoint(boundsCenter + mainCamera.transform.right * radius);

            float screenSize = Vector3.Distance(screenPosCenter, screenPosEdge) * 2f; // 粗略估算直径
            return screenSize;
        }

        int GetLODLevelFromScreenSize(float screenSize)
        {
            for (int i = 0; i < screenSizeThresholds.Count; i++)
            {
                if (screenSize >= screenSizeThresholds[i])
                    return i;
            }
            return screenSizeThresholds.Count; // 最小LOD层
        }
    }
}
