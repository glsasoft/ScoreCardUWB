#if  UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ArtTools
{
    public class ResetSubGameObject
    {
        [MenuItem("Tools/ResetSelectObjPos")]
        public static void ResetSelectObjPos()
        {
            var objs = Selection.gameObjects;
            Vector3 rootPos = Vector3.zero;
            foreach (var obj in objs)
            {
                // rootPos = obj.transform.localPosition;
                CheckSubGameObject(obj);
            }
        }

        public static void CheckSubGameObject(GameObject root)
        {
            var subObjs = root.GetComponentsInChildren<Transform>();
            bool isSubPosAllSame = true;
            int compareIndex = 0;
            Vector3 comparePos = Vector3.zero;
            foreach (var subObj in subObjs)
            {
                if (subObj == root.transform)
                {
                    continue;
                }

                if (compareIndex == 0)
                {
                    comparePos = subObj.position;
                }

                compareIndex++;
                if (!Mathf.Approximately(subObj.position.x, comparePos.x) ||
                    !Mathf.Approximately(subObj.position.y, comparePos.y) ||
                    !Mathf.Approximately(subObj.position.z, comparePos.z))
                {
                    isSubPosAllSame = false;
                    break;
                }
            }

            if (isSubPosAllSame)
            {
                foreach (var subObj in subObjs)
                {
                    if (subObj == root.transform)
                    {
                        continue;
                    }

                    root.transform.localScale = new Vector3(
                        root.transform.localScale.x * subObj.transform.localScale.x,
                        root.transform.localScale.y * subObj.transform.localScale.y,
                        root.transform.localScale.z * subObj.transform.localScale.z);
                    root.transform.localPosition = subObj.localPosition + root.transform.localPosition;
                    subObj.localPosition = Vector3.zero;
                    subObj.localScale = Vector3.one;
                }
            }
        }
    }
}
#endif