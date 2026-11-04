using System;
// 提供基础数据结构
using System.Collections.Generic;
// Unity核心命名空间
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Graphics
{
        [Serializable]  //序列化
        public class PrefabLightmapData
        {
            [Serializable]
            public class RendererLightmapData
            {
                // 渲染组件
                public Renderer renderer;
                // 自定义封装的Lightmap数据
                public SerializedLightmapData lightmap;
                // Lightmap的ST坐标
                public Vector4 lightmapOffsetScale;
            }
            
            public string name;
            // 存储当前GameObject下所有渲染器的光照信息
            public List<RendererLightmapData> renderers = new();

            // 保存当前root物体（及子物体）下所有的Renderer的Lightmap数据
            public void Save(GameObject root)
            {
                renderers.Clear();
            
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.lightmapIndex == -1) continue;
                    
                    renderers.Add(new RendererLightmapData
                    {
                        renderer = renderer,
                        // LightmapSettings.lightmaps ： Unity 的全局光照贴图数组
                        // renderer.lightmapIndex ： 当前Renderer使用的贴图
                        lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex],
                        // 当前 Renderer 对Lightmap的偏移与缩放
                        lightmapOffsetScale = renderer.lightmapScaleOffset
                    });
                }
            }

            // 载入已保存的光照数据
            public void Load()
            {
                // 单例管理器
                SceneLightmapDataManager manager = SceneLightmapDataManager.Instance;
                foreach (RendererLightmapData renderer in renderers)
                {
                    renderer.renderer.gameObject.isStatic = true;
                    // 查找已有贴图，返回其在 LightmapSettings 中的索引
                    renderer.renderer.lightmapIndex = manager.AllocateLightmap(renderer.lightmap);
                    renderer.renderer.lightmapScaleOffset = renderer.lightmapOffsetScale;
                }
            }

            // 卸载资源，释放光照贴图
            public void Unload()
            {
                // 单例管理器
                SceneLightmapDataManager manager = SceneLightmapDataManager.Instance;

                // 手动释放Lightmap
                foreach (RendererLightmapData renderer in renderers)
                {
                    if (renderer.lightmap.IsEmpty()) return;
                    manager.FreeLightmap(renderer.lightmap);
                }
            }
        }
        
}