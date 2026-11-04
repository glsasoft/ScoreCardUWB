using UnityEngine;
using YooAsset;

namespace LetsPiu
{
    public static class AssetNodeExtension
    {
        public static void AddAssetHandle(this GameObject obj, HandleBase handle)
        {
            AssetNode.AddHandle(handle, obj);
        }

        public static void AttachToGameObject(this HandleBase handle, GameObject attachedObj)
        {
            AssetNode.AddHandle(handle, attachedObj);
        }

        public static void DisposeAssetHandle(this GameObject obj)
        {
            AssetNode.DisposeHandle(obj);
        }
    }
}
