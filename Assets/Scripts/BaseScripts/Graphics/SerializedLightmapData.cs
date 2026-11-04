using System;
using UnityEngine;

namespace LetsPiu.Graphics
{
    // Unity的LightmapData不是Serializable
    // 手动写一个，将 LightmapData 转成可以序列化的形式
    [Serializable]
    public struct SerializedLightmapData
    {
        
        public Texture2D lightmapColor; // 主光照贴图
        public Texture2D lightmapDir; // 方向光照贴图
        public Texture2D shadowMask; // 阴影遮罩

        public bool IsEmpty()
        {
            return lightmapColor == null && lightmapDir == null && shadowMask == null;
        }
        
        // 隐式类型转换
        // LightmapData -> SerializedLightmapData
        public static implicit operator SerializedLightmapData(LightmapData data)
        {
            return new SerializedLightmapData
            {
                lightmapColor = data.lightmapColor,
                lightmapDir = data.lightmapDir,
                shadowMask = data.shadowMask
            };
        }
            
        // SerializedLightmapData -> LightmapData;
        public static implicit operator LightmapData(SerializedLightmapData data)
        {
            return new LightmapData
            {
                lightmapColor = data.lightmapColor,
                lightmapDir = data.lightmapDir,
                shadowMask = data.shadowMask
            };
        }

        // Hash值：用于比较 光照贴图是否相同 ； 作为字典键
        public override int GetHashCode()
        {
            // 把三张贴图的 ID 混合成一个唯一的哈希值
            return HashCode.Combine(SafeGetID(lightmapColor), SafeGetID(lightmapDir), SafeGetID(shadowMask));
        }

        
        // 安全获取贴图 ID
        private int SafeGetID(Texture2D t) => t ? t.GetInstanceID() : 0;
    }
}