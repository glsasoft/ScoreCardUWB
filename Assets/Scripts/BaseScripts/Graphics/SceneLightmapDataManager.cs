using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Graphics
{
    [ExecuteAlways]
    public class SceneLightmapDataManager : MonoBehaviour
    {
        public static SceneLightmapDataManager Instance { get; private set; }

        private Dictionary<int, int> lightmapIndex; // lightmap索引
        private List<int> lightmapReferenceCount; // 索引位引用计数，跟踪多少对象使用该lightmap

        private void Awake()
        {
            Instance = this;

            // 初始化
            LightmapData[] lightmaps = LightmapSettings.lightmaps;
            lightmapIndex = new Dictionary<int, int>(lightmaps.Length);
            lightmapReferenceCount = new List<int>(lightmaps.Length);
            // 注册全部 lightmap，初始引用记为 1
            for (int i = 0; i < lightmaps.Length; i++)
            {
                var hashCode = ((SerializedLightmapData)lightmaps[i]).GetHashCode();
                if (lightmapIndex.ContainsKey(hashCode)) continue;
                lightmapIndex.Add(hashCode, i);
                lightmapReferenceCount.Add(1);
            }
        }

        // 分配 lightmap
        public int AllocateLightmap(SerializedLightmapData lightmap)
        {
            if (lightmap.IsEmpty())
            {
                Debug.LogWarning("[SceneLightmapDataManager] 尝试注册空贴图，忽略。");
                return -1;
            }
            int hashCode = lightmap.GetHashCode();
            int index = -1;
            // hash 相同就复用 ； hash不同，就添加
            if (!lightmapIndex.TryGetValue(hashCode, out index))
            {
                List<LightmapData> lightmaps = new List<LightmapData>(LightmapSettings.lightmaps);
                index = lightmaps.Count;
                lightmaps.Add(lightmap);
                LightmapSettings.lightmaps = lightmaps.ToArray();

                lightmapIndex.Add(hashCode, index);
                lightmapReferenceCount.Add(0);

            }
            // 确保 index 是有效的
            if (index >= 0 && index < lightmapReferenceCount.Count)
            {
                lightmapReferenceCount[index]++;
            }
            

            return index;
        }

        // 释放光照贴图

        public void FreeLightmap(SerializedLightmapData lightmap)
        {
            if (lightmap.IsEmpty())
            {
                Debug.LogWarning("[SceneLightmapDataManager] 尝试注册空贴图，忽略。");
                return;
            }
 

            int index = -1;
            var mapIdx = lightmap.GetHashCode();
            if (lightmapIndex.ContainsKey(mapIdx))
            {
                index = lightmapIndex[mapIdx];
            }

            if (index >= 0 && index < lightmapReferenceCount.Count)
            {
                lightmapReferenceCount[index]--;
            }
            // ^1 表示最后一个元素（C# 8.0 的语法）
            if (lightmapReferenceCount[^1] != 0) return;


            List<LightmapData> lightmaps = new(LightmapSettings.lightmaps);
            // 循环删除 末尾连续未使用的贴图，确保中间index不变（避免破坏已经绑定的renderer）
            while (lightmapReferenceCount.Count > 0 && lightmapReferenceCount[^1] == 0)
            {
                lightmapIndex.Remove(((SerializedLightmapData)lightmaps[^1]).GetHashCode());
                lightmaps.RemoveAt(lightmaps.Count - 1);
                lightmapReferenceCount.RemoveAt(lightmapReferenceCount.Count - 1);
            }
            LightmapSettings.lightmaps = lightmaps.ToArray();
        }
    }
}