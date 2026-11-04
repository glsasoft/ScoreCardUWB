using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtTools
{
    public class TextureTools : UnityEditor.Editor
    {
        [MenuItem("Assets/XGame/关闭贴图mipmap")]
        public static void DisableMipmaps()
        {
            Object[] texs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            int a = 0;
            int b = 0;
            foreach (var VARIABLE in texs)
            {
                var path = AssetDatabase.GetAssetPath(VARIABLE);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;

                        importer.SaveAndReimport();
                        a++;
                        Debug.Log(path + "   修改mipmap");
                    }


                    int maxTextureSize;
                    TextureImporterFormat textureFormat;
                    int compressionQuality;
                    bool etc1AlphaSplitEnabled;

                    importer.GetPlatformTextureSettings("Android", out maxTextureSize, out textureFormat,
                        out compressionQuality, out etc1AlphaSplitEnabled);
                    if (maxTextureSize != 2048)
                    {
                        importer.SetPlatformTextureSettings("Android", 2048, textureFormat,
                            compressionQuality, etc1AlphaSplitEnabled);
                        
                        
                        b++;
                        Debug.Log(path + "   修改maxTextureSize");
                    }
                }
            }

            Debug.LogError("共修改了" + a + "张贴图mipmap    " + b + "张贴图maxTextureSize");
            
        }


        [MenuItem("Assets/XGame/UI图片裁切透明区域")]
        public static void TrimSprites()
        {
            // 获取所有选中的对象
            Object[] objects = Selection.objects;
            List<Texture2D> texture2Ds = new List<Texture2D>();
            foreach (Object obj in objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                // 检查是否为文件夹
                if (Directory.Exists(path))
                {
                    // 获取目录下所有文件
                    Object[] texs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets) as Texture2D[];
                    foreach (var VARIABLE in texs)
                    {
                        texture2Ds.Add(VARIABLE as Texture2D);
                    }
                }
                else
                {
                    if (obj is Texture2D)
                        texture2Ds.Add(obj as Texture2D);
                }
            }


            foreach (var VARIABLE in texture2Ds)
            {
                TrimSprite(VARIABLE);
            }
        }

        private static void TrimSprite(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.isReadable = true;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();

            var oldSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(oldSettings);
            Vector2 oldPivot = importer.spritePivot * new Vector2(texture.width, texture.height);

            Texture2D newTexture = new Texture2D(texture.width, texture.height);
            newTexture.SetPixels(texture.GetPixels());
            newTexture.Apply();

            // Find non-transparent bounds
            int xMin = int.MaxValue, xMax = int.MinValue, yMin = int.MaxValue, yMax = int.MinValue;
            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    if (newTexture.GetPixel(x, y).a > 0)
                    {
                        if (x < xMin) xMin = x;
                        if (x > xMax) xMax = x;
                        if (y < yMin) yMin = y;
                        if (y > yMax) yMax = y;
                    }
                }
            }

            if (xMin == int.MaxValue) return;

            int width = xMax - xMin + 1;
            int height = yMax - yMin + 1;
            Texture2D croppedTexture = new Texture2D(width, height);
            Color[] pixels = newTexture.GetPixels(xMin, yMin, width, height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            // Calculate new pivot

            Vector2 relativePivot = new Vector2(oldPivot.x - xMax, oldPivot.y - yMax);
            Vector2 newPivot = new Vector2(oldPivot.x - xMin, oldPivot.y - yMin) / new Vector2(width, height);

            // Save cropped texture
            string newPath = "Assets/test/Cropped_" + texture.name + ".png";
            System.IO.File.WriteAllBytes(path, croppedTexture.EncodeToPNG());
            AssetDatabase.Refresh();

            // Apply old settings and new pivot to new texture
            //   TextureImporter newImporter = AssetImporter.GetAtPath(newPath) as TextureImporter;
            //   newImporter.SetTextureSettings(oldSettings);
            //   newImporter.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = newPivot;
            importer.SaveAndReimport();
        }
    }
}