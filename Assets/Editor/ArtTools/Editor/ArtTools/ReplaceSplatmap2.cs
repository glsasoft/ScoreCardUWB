/* 
This is a VERY quick and dirty, but useful script for replacing the terrain splatmap with an imported one from World Machine.

This script is released into the public domain; you may do whatever you want with it!

To use:

1) Import the new Splat map as an asset
2) Change the type of it to Advanced, Readable/Writeable, ARGB32 and Nearest power of two.
3) Invoke the script from the Terrain menu

Two things of note:

 * Note that you will need to first create an original splatmap before being able to replace it. To do this, save your world and setup your textures, etc first. 
 * Note that World Machine by default considers the texture origin to be the bottom-left, not the top-left.

 Updated for C# on 2/12/2019 by Stephen Schmitt

*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtTools
{
    public class WizardReplaceSplatmap : ScriptableWizard
    {
        public Texture2D splatmap;
        public Texture2D newsplat;
        public Texture2D splatmap1;
        public Texture2D newsplat1;
        public Texture2D splatmap2;

        public Texture2D newsplat2;

        //public RenderTexture heightMap;
        public Texture2D newheightMap;



        //public Texture2D[] newsplats;
        public bool flipVertical;


        WizardReplaceSplatmap()
        {
            flipVertical = false;
            newsplat = null;
            splatmap = null;
            splatmap1 = null;
            newsplat1 = null;
            splatmap2 = null;
            newsplat2 = null;
            newheightMap = null;
        }

        [MenuItem("工具/Terrain/Replace Splatmap (from WM)", false, 20)]
        static void CreateWizard()
        {
            //Texture2D[] texs = new[] {newsplat, newsplat1};
            ScriptableWizard.DisplayWizard<WizardReplaceSplatmap>("Replace Splatmap", "Replace");
        }


        [MenuItem("工具/Terrain/Export Splatmap (from WM)", false, 21)]
        static void Apply()
        {
            Texture2D texture = Selection.activeObject as Texture2D;
            if (texture == null)
            {
                EditorUtility.DisplayDialog("Select Texture", "You Must Select a Texture first!", "Ok");
                return;
            }

            //var bytes = texture.EncodeToPNG();
            var bytes = texture.EncodeToTGA();



            Debug.LogError("����Ŀ¼��" + Application.dataPath + " Name:" + "exported_texture.png");
            File.WriteAllBytes(Application.dataPath + "/exported_texture.tga", bytes);
        }

        private void Awake() //awake在新窗口打开时调用
        {
            //设置贴图默认值
            newsplat = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Artist/hetu/terraindata_jgzd_001_pingyuan_SplatAlpha_0.tga");
            newsplat1 = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Artist/hetu/terraindata_jgzd_001_pingyuan_SplatAlpha_1.tga");
            newsplat2 = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Artist/hetu/terraindata_jgzd_001_pingyuan_SplatAlpha_2.tga");
            newheightMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Artist/hetu/heightMap.exr");

            //获取地形data的地1，2，3张图
            var terrainData = GameObject.Find("terrain_plain").GetComponent<Terrain>().terrainData;
            var b = terrainData.alphamapTextures[1];
            var c = terrainData.alphamapTextures[2];
            var height1 = terrainData.heightmapTexture; //获取高度图
            //在插件窗口填写预设值
            splatmap = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Artist/Scene/SceneResource/diwuziyuan/terrain/terraindata/terraindata_jgzd_001_pingyuan.asset");
            splatmap1 = b;
            splatmap2 = c;

        }

        void OnWizardCreate()
        {
            // if (newsplat.format != TextureFormat.RGBA32 && newsplat.format != TextureFormat.ARGB32 && newsplat.format != TextureFormat.RGB24)
            // {
            //     EditorUtility.DisplayDialog("Wrong format", "Your new splatmap must be set to the RGBA 32 bit format in the Texture Inspector.\nMake sure the type is Advanced and set the format!", "Cancel");
            //     return;
            // }
            // int w = newsplat.width;
            //
            // if (Mathf.ClosestPowerOfTwo(w) != w)
            // {
            //     EditorUtility.DisplayDialog("Wrong size", "Splatmap width and height must be a power of two!", "Cancel");
            //     return;
            // }
            //
            // try
            // {
            //     var pixels = newsplat.GetPixels();
            //     if (flipVertical)
            //     {
            //         var h = w; // always square in unity
            //         for (var y = 0; y < h / 2; y++)
            //         {
            //             var otherY = h - y - 1;
            //             for (var x = 0; x < w; x++)
            //             {
            //                 var swapval = pixels[y * w + x];
            //                 pixels[y * w + x] = pixels[otherY * w + x];
            //                 pixels[otherY * w + x] = swapval;
            //             }
            //         }
            //     }
            //     splatmap.Reinitialize(newsplat.width, newsplat.height, newsplat.format, true);
            //     splatmap.SetPixels(pixels);
            //     splatmap.Apply();
            // }
            // catch (UnityException)
            // {
            //     EditorUtility.DisplayDialog("Not readable", "The 'New' splatmap must be readable. Make sure the type is Advanced and enable read/write and try again!", "Cancel");
            //     return;
            // }

            /*
            var terrainData =   GameObject.Find("terrain_plain").GetComponent<Terrain>().terrainData;
            var terrainData1 =   GameObject.Find("output0_0").GetComponent<Terrain>().terrainData;
            var height = terrainData.heightmapTexture;      //获取高度图
            var height1 = terrainData1.heightmapTexture;      //获取高度图
            float[,] a = terrainData1.GetHeights(0,0,terrainData1.heightmapTexture.width,terrainData1.heightmapTexture.height);
            terrainData.SetHeights(0,0,a);
            */

            yu(newsplat, splatmap);
            yu(newsplat1, splatmap1);
            yu(newsplat2, splatmap2);

            var terrainData = GameObject.Find("terrain_plain").GetComponent<Terrain>().terrainData;

            float[,] newHeights = new float[newheightMap.width, newheightMap.height];

            for (int y = 0; y < (newheightMap.height); y++)
            {
                for (int x = 0; x < (newheightMap.width); x++)
                {
                    Color color = newheightMap.GetPixel(x, y - 1);
                    newHeights[y, x] = color.grayscale;
                }
            }

            terrainData.SetHeights(0, 0, newHeights);

        }

        void OnWizardUpdate()
        {
            helpString =
                "Replace the existing splatmap of your terrain with a new one.\n1) Drag the embedded splatmap texture of your terrain to the 'Splatmap box'.\n2) Then drag the replacement splatmap texture to the 'New' box\n3) Then hit 'Replace'.";
            isValid = (splatmap != null) && (newsplat != null);
        }


        void yu(Texture2D newsplat, Texture2D splatmap)
        {
            if (newsplat.format != TextureFormat.RGBA32 && newsplat.format != TextureFormat.ARGB32 &&
                newsplat.format != TextureFormat.RGB24)
            {
                EditorUtility.DisplayDialog("Wrong format",
                    "Your new splatmap must be set to the RGBA 32 bit format in the Texture Inspector.\nMake sure the type is Advanced and set the format!",
                    "Cancel");
                return;
            }

            int w = newsplat.width;

            if (Mathf.ClosestPowerOfTwo(w) != w)
            {
                EditorUtility.DisplayDialog("Wrong size", "Splatmap width and height must be a power of two!",
                    "Cancel");
                return;
            }

            try
            {
                var pixels = newsplat.GetPixels();
                if (flipVertical)
                {
                    var h = w; // always square in unity
                    for (var y = 0; y < h / 2; y++)
                    {
                        var otherY = h - y - 1;
                        for (var x = 0; x < w; x++)
                        {
                            var swapval = pixels[y * w + x];
                            pixels[y * w + x] = pixels[otherY * w + x];
                            pixels[otherY * w + x] = swapval;
                        }
                    }
                }

                splatmap.Reinitialize(newsplat.width, newsplat.height, newsplat.format, true);
                splatmap.SetPixels(pixels);
                splatmap.Apply();
            }
            catch (UnityException)
            {
                EditorUtility.DisplayDialog("Not readable",
                    "The 'New' splatmap must be readable. Make sure the type is Advanced and enable read/write and try again!",
                    "Cancel");
                return;
            }

        }

    }
}