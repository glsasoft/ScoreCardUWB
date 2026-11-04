using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LetsPiu.UI
{
    public static partial class UIHelper
    {
        /// <summary>
        /// 在节点点显示cell
        /// </summary>
        public static void RefreshCell(Transform cellParentRoot, Transform cellTemplate, int dataCount, Action<Transform, int> onCellUpdate)
        {
            if (cellTemplate == null || cellParentRoot == null)
            {
                return;
            }

            // 先生成子对象
            for (int i = 0; i < dataCount; ++i)
            {
                if (i >= cellParentRoot.childCount)
                {
                    Transform cell = GameObject.Instantiate(cellTemplate, cellParentRoot);
                    //cell.SetParent(cellParentRoot, false);
                }
            }

            // 再设置数据
            for (int i = 0; i < dataCount; ++i)
            {
                Transform cell = null;

                if (i >= cellParentRoot.childCount)
                {
                    cell = GameObject.Instantiate(cellTemplate, cellParentRoot);
                    //cell.SetParent(cellParentRoot, false);
                }
                else
                {
                    cell = cellParentRoot.GetChild(i);
                }

                cell.gameObject.SetActive(true);

                onCellUpdate?.Invoke(cell, i);
            }

            // 隐藏未使用
            for (int i = dataCount; i < cellParentRoot.childCount; ++i)
            {
                cellParentRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 在节点点显示cell
        /// </summary>
        public static void RefreshCell<T>(Transform cellParentRoot, Transform cellTemplate, List<T> dataList, Action<Transform, int, List<T>> onCellUpdate)
        {
            if (cellTemplate == null || cellParentRoot == null)
            {
                return;
            }

            int dataCount = dataList == null ? 0 : dataList.Count;

            for (int i = 0; i < dataCount; ++i)
            {
                Transform cell = null;

                if (i >= cellParentRoot.childCount)
                {
                    cell = GameObject.Instantiate(cellTemplate);
                    cell.SetParent(cellParentRoot, false);
                }
                else
                {
                    cell = cellParentRoot.GetChild(i);
                }

                cell.gameObject.SetActive(true);

                onCellUpdate?.Invoke(cell, i, dataList);
            }

            // 隐藏未使用
            for (int i = dataCount; i < cellParentRoot.childCount; ++i)
            {
                cellParentRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static T Find<T>(this GameObject parent, string name) where T : UnityEngine.Component
        {
            Transform target = parent.transform.Find(name);
            if (target == null)
            {
                return null;
            }

            return target.gameObject.GetComponent<T>();
        }

        public static T Find<T>(this Transform parent, string name) where T : UnityEngine.Component
        {
            Transform target = parent.Find(name);
            if (target == null)
            {
                return null;
            }

            return target.gameObject.GetComponent<T>();
        }

        public static async UniTask<bool> SetGray(this Transform parent, bool isGray)
        {
            var uiGray = parent.GetComponent<UIGrayEffect>();
            if (uiGray == null)
            {
                uiGray = parent.gameObject.AddComponent<UIGrayEffect>();
            }
            return await uiGray.SetUIGray(isGray);
        }
    }
}
