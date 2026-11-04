using LetsPiu.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class ConstantContainer : MonoBehaviour
    {
        public static Transform avatarRoot { get; private set; }
        public static Transform avatarCacheRoot { get; private set; }
        public static Transform effectCacheRoot { get; private set; }
        public static Transform audioCacheRoot { get;private set; }

        void Awake()
        {
            if (avatarRoot == null)
            {
                avatarRoot = CreateRoot("avatarRoot");
            }

            if (avatarCacheRoot == null)
            {
                avatarCacheRoot = CreateRoot("avatarCacheRoot");
            }

            if (effectCacheRoot == null)
            {
                effectCacheRoot = CreateRoot("effectCacheRoot");
            }

            if (audioCacheRoot == null)
            {
                audioCacheRoot = CreateRoot("audioCacheRoot");
            }
        }

        Transform CreateRoot(string name)
        {
            GameObject root = new GameObject(name);
            Helper.SetParent(root.transform, transform, true);

            return root.transform;
        }
    }
}
