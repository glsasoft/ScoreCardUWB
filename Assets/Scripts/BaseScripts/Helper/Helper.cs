using LetsPiu.Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static partial class Helper
    {
        static List<Transform> s_TransformList = new List<Transform>();
        static ParamReader m_ParamReader;

        public static ParamReader paramReader
        {
            get
            {
                if (m_ParamReader == null)
                {
                    m_ParamReader = new ParamReader();
                }
                return m_ParamReader;
            }
        }

        public static void SetParent(GameObject go, GameObject parent)
        {
            if (null == go || null == parent)
            {
                return;
            }

            SetParent(go.transform, parent.transform);
        }

        public static void SetParent(GameObject go, Transform parent)
        {
            if (null == go)
            {
                return;
            }

            SetParent(go.transform, parent);
        }

        public static void SetParent(Transform objTrans, Transform parent, bool resetPosition = true)
        {
            if (null == objTrans)
            {
                return;
            }

            objTrans.SetParent(parent);

            if (resetPosition)
            {
                ResetTransform(objTrans);
            }
        }

        public static void ResetTransform(Transform objTrans)
        {
            objTrans.localScale = Vector3.one;
            objTrans.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="layerOriginObj"></param>
        /// <param name="target"></param>
        /// <param name="ignoreLayerMask">如果原层为ignoreLayerMask，不设置为新层</param>
        /// 用以保护特殊层(如用于特效碰撞的ParticleBlock层，此层肯定不可见，不影响总体隐藏)
        public static void PassLayer(GameObject layerOriginObj, GameObject target, int ignoreLayerMask = 0)
        {
            if (null == layerOriginObj || null == target)
            {
                return;
            }

            SetLayerRecursively(target.transform, layerOriginObj.layer, ignoreLayerMask);
        }

        /// <summary>
        /// 递归设置层
        /// </summary>
        /// <param name="parent">节点</param>
        /// <param name="layer">新设置的层</param>
        /// <param name="ignoreLayer">如果原层在ignoreLayerMask以内，不设置为新层</param>
        public static void SetLayerRecursively(Transform parent, int layer, int ignoreLayerMask = 0)
        {
            if (null == parent)
            {
                return;
            }
            
            //Debug.Log($"[Helper] [初始化] 设置{parent.name}的layer为{LayerMask.LayerToName(layer)}");
            
            parent.GetComponentsInChildren<Transform>(true, s_TransformList);

            for (int i = 0; i < s_TransformList.Count; ++i)
            {
                if (((1 << s_TransformList[i].gameObject.layer) & ignoreLayerMask) == 0)
                {
                    s_TransformList[i].gameObject.layer = layer;
                }
            }
        }

        public static Transform GetEffectParent(int id)
        {
            var tdData = TDEffectResourceTable.GetData(id);
            if (tdData == null)
            {
                return null;
            }

            switch ((EffecResourceType)tdData.EffectType)
            {
                case EffecResourceType.OnTheUI:
                    return UIRoot.S.topEffectRoot;
                case EffecResourceType.UnderTheUI:
                    return UIRoot.S.bottomEffectRoot;
                case EffecResourceType.FullScreenOnTheUI:
                case EffecResourceType.FullScreenUnderTheUI:
                    return CameraMgr.S.effectRoot;
                default:
                    return null;
            }
        }
    }
}

