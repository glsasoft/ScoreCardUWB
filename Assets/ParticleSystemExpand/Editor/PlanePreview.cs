using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ExpandParticleSystem
{
    /// <summary>
    /// 用平面渲染材质缩略图，替代 Unity 默认的球体预览
    /// </summary>
    public static class PlanePreview
    {
        private const int Size = 128;
        private static readonly Dictionary<Material, Texture2D> cache = new Dictionary<Material, Texture2D>();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            //域重载前清掉缓存，避免纹理泄漏
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
        }

        /// <summary>
        /// 取材质的平面预览图，第一次调用时渲染并缓存
        /// </summary>
        /// <param name="material">要预览的材质</param>
        /// <returns>预览纹理，材质为空时返回 null</returns>
        public static Texture2D Get(Material material)
        {
            if (material == null)
            {
                return null;
            }
            Texture2D cached;
            if (cache.TryGetValue(material, out cached) && cached != null)
            {
                return cached;
            }
            Texture2D result = Render(material);
            cache[material] = result;
            return result;
        }

        /// <summary>
        /// 清掉缓存，材质改过之后重新生成
        /// </summary>
        public static void Clear()
        {
            foreach (Texture2D tex in cache.Values)
            {
                if (tex != null)
                {
                    Object.DestroyImmediate(tex);
                }
            }
            cache.Clear();
        }

        /// <summary>
        /// 把材质贴到一个平面上渲染成缩略图
        /// </summary>
        /// <param name="material">要渲染的材质</param>
        /// <returns>渲染出的预览纹理</returns>
        private static Texture2D Render(Material material)
        {
            PreviewRenderUtility utility = new PreviewRenderUtility();
            Texture2D result = null;
            try
            {
                utility.BeginPreview(new Rect(0f, 0f, Size, Size), GUIStyle.none);

                //一个 1x1 的平面，正交相机正好铺满预览区
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.hideFlags = HideFlags.HideAndDontSave;
                MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                utility.AddSingleGO(quad);

                Camera camera = utility.camera;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.orthographicSize = 0.5f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 10f;
                camera.transform.position = new Vector3(0f, 0f, -2f);
                camera.transform.rotation = Quaternion.identity;
                camera.Render();

                RenderTexture target = utility.EndPreview() as RenderTexture;
                if (target != null)
                {
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = target;
                    result = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                    result.ReadPixels(new Rect(0f, 0f, Size, Size), 0, 0);
                    result.Apply();
                    RenderTexture.active = previous;
                }
            }
            finally
            {
                utility.Cleanup();
            }
            return result;
        }
    }
}
