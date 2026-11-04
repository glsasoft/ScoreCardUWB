using System.Collections;
using System.Collections.Generic;
using LetsPiu.Base.BaseScripts.Define;
using UnityEngine;
using YooAsset;

namespace LetsPiu.Main
{
    [CreateAssetMenu]
    //[PreferBinarySerialization]
    public class GameDataConfig : ScriptableObject
    {
        private static AssetHandle _assetHandle;
        private static GameDataConfig _instance;
        public static GameDataConfig Instance
        {
            get
            {
                if (_instance == null && YooAssets.Initialized)
                {
                    ResourcePackage package = YooAssets.GetPackage(Constants.DEFAULT_PACKAGE);
                    if (package != null)
                    {
                        AssetHandle handle = package.LoadAssetSync<GameDataConfig>("GameDataConfig");
                        _instance = handle.AssetObject as GameDataConfig;
                        _assetHandle = handle;
                    }
                }
                return _instance;
            }
        }

        private void OnDestroy()
        {
            _assetHandle?.Dispose();
        }

        /// <summary>
        /// 溶解曲线
        /// </summary>
        [SerializeField]
        private AnimationCurve _characterDissolveCurve;
        /// <summary>
        /// 溶解曲线
        /// </summary>
        public AnimationCurve characterDissolveCurve => _characterDissolveCurve;

        /// <summary>
        /// 溶解贴图
        /// </summary>
        [SerializeField]
        private Texture2D _dissolveThresholdMap;
        /// <summary>
        /// 溶解贴图
        /// </summary>
        public Texture2D dissolveThresholdMap => _dissolveThresholdMap;
    }
}
