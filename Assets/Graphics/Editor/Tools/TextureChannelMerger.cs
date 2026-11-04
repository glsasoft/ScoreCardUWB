using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;

namespace LetsPiu
{
    
    public class TextureChannelMerger : EditorWindow
    {
        private struct TextureData
        {
            public Color[] pixels;
            public int width;
            public int height;
        }
        private TextureData GetTextureData(Texture2D tex)
        {
            return new TextureData
            {
                pixels = GetReadablePixels(tex),
                width = tex != null ? tex.width : 0,
                height = tex != null ? tex.height : 0
            };
        }
        
        private Texture2D rChannel;
        private Texture2D gChannel;
        private Texture2D bChannel;
        private Texture2D aChannel;
        
        private bool useR = true;
        private bool useG = true;
        private bool useB = true;
        private bool useA = false;
        
        private float rValue = 1f;
        private float gValue = 1f;
        private float bValue = 1f;
        private float aValue = 1f;
        
        private const int BATCH_SIZE = 512;
        private bool isProcessing = false;
        private float progress = 0;
        
        private string textureName = "New_Combined_Texture";
        private string savePath = "Assets/";

        [MenuItem("Tools/Texture Channel Merger")]
        public static void ShowWindow()
        {
            GetWindow<TextureChannelMerger>("Texture Merger");
        }

        private void OnGUI()
        {
            GUILayout.Label("Channel Settings", EditorStyles.boldLabel);
            
            // R Channel
            useR = EditorGUILayout.Toggle("Use R Channel", useR);
            rChannel = TextureField("R Channel Texture", rChannel);
            if (!useR) rValue = EditorGUILayout.Slider("R Value", rValue, 0f, 1f);

            // G Channel
            useG = EditorGUILayout.Toggle("Use G Channel", useG);
            gChannel = TextureField("G Channel Texture", gChannel);
            if (!useG) gValue = EditorGUILayout.Slider("G Value", gValue, 0f, 1f);

            // B Channel
            useB = EditorGUILayout.Toggle("Use B Channel", useB);
            bChannel = TextureField("B Channel Texture", bChannel);
            if (!useB) bValue = EditorGUILayout.Slider("B Value", bValue, 0f, 1f);

            // A Channel
            useA = EditorGUILayout.Toggle("Use A Channel", useA);
            aChannel = TextureField("A Channel Texture", aChannel);
            if (!useA) aValue = EditorGUILayout.Slider("A Value", aValue, 0f, 1f);

            EditorGUILayout.Space(20);
            textureName = EditorGUILayout.TextField("Texture Name", textureName);
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            
            GUI.enabled = !isProcessing;
            if (GUILayout.Button("Generate Combined Texture"))
            {
                if (ValidateTextures())
                {
                    isProcessing = true;
                    progress = 0;
                    GenerateAsync();
                }
            }
            GUI.enabled = true;

            if (isProcessing)
            {
                Rect rect = EditorGUILayout.GetControlRect();
                float currentProgress;
                lock (progressLock)
                {
                    currentProgress = progress;
                }
                EditorGUI.ProgressBar(rect, currentProgress, "Processing...");
                Repaint();
            }
        }
        
        private async void GenerateAsync()
        {
            try
            {
                // 主线程：获取所有必要数据
                int width = GetMaxTextureWidth();
                int height = GetMaxTextureHeight();
        
                // 主线程：准备纹理数据
                TextureData rData = useR ? GetTextureData(rChannel) : default;
                TextureData gData = useG ? GetTextureData(gChannel) : default;
                TextureData bData = useB ? GetTextureData(bChannel) : default;
                TextureData aData = useA ? GetTextureData(aChannel) : default;

                // 主线程：创建输出纹理
                Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                Color[] outputPixels = output.GetPixels();

                // 后台处理
                await Task.Run(() => 
                    ProcessPixels(
                        outputPixels, 
                        width, height,
                        rData, gData, 
                        bData, aData
                    ));

                // 主线程：应用处理结果
                output.SetPixels(outputPixels);
                output.Apply();
        
                SaveTexture(output);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理失败: {e}");
                EditorUtility.DisplayDialog("Error", $"处理失败: {e.Message}", "OK");
            }
            finally
            {
                isProcessing = false;
            }
        }
        
        private readonly object progressLock = new object();
        private void ProcessPixels(
            Color[] outputPixels,
            int targetWidth,
            int targetHeight,
            TextureData rData,
            TextureData gData,
            TextureData bData,
            TextureData aData)
        {
            Parallel.For(0, targetHeight, y =>
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int index = y * targetWidth + x;
            
                    float r = useR ? GetChannelValue(rData, x, y, targetWidth, targetHeight) : rValue;
                    float g = useG ? GetChannelValue(gData, x, y, targetWidth, targetHeight) : gValue;
                    float b = useB ? GetChannelValue(bData, x, y, targetWidth, targetHeight) : bValue;
                    float a = useA ? GetChannelValue(aData, x, y, targetWidth, targetHeight) : aValue;

                    outputPixels[index] = new Color(r, g, b, a);
                }

                // 进度更新（每处理512行更新一次）
                if (y % BATCH_SIZE == 0)
                {
                    lock (progressLock)
                    {
                        progress = Mathf.Clamp01((float)y / targetHeight);
                    }
                }
            });
        }
        
        private (Color[] r, Color[] g, Color[] b, Color[] a) GetPixelsData()
        {
            return (
                useR ? GetReadablePixels(rChannel) : null,
                useG ? GetReadablePixels(gChannel) : null,
                useB ? GetReadablePixels(bChannel) : null,
                useA ? GetReadablePixels(aChannel) : null
            );
        }

        private Texture2D TextureField(string label, Texture2D texture)
        {
            EditorGUILayout.BeginHorizontal();
            texture = (Texture2D)EditorGUILayout.ObjectField(label, texture, typeof(Texture2D), false);
            HandleDragAndDrop();
            EditorGUILayout.EndHorizontal();
            return texture;
        }

        private void HandleDragAndDrop()
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    if (DragAndDrop.objectReferences.Length > 0 && 
                        DragAndDrop.objectReferences[0] is Texture2D)
                    {
                        DragAndDrop.AcceptDrag();
                        GUI.changed = true;
                        GUI.FocusControl(null);
                    }
                }
            }
        }

        private bool ValidateTextures()
        {
            if ((useR && rChannel == null) ||
                (useG && gChannel == null) ||
                (useB && bChannel == null) ||
                (useA && aChannel == null))
            {
                EditorUtility.DisplayDialog("Error", "Please assign all enabled channel textures", "OK");
                return false;
            }
            return true;
        }
        
        private Color[] GetReadablePixels(Texture2D tex)
        {
            if (tex == null) return null;
            
            // 创建临时可读副本
            RenderTexture tmp = RenderTexture.GetTemporary(
                tex.width, 
                tex.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            UnityEngine.Graphics.Blit(tex, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            
            Texture2D readableTex = new Texture2D(tex.width, tex.height);
            readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTex.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            
            return readableTex.GetPixels();
        }

        private float GetChannelValue(TextureData data, int x, int y, int targetWidth, int targetHeight)
        {
            if (data.pixels == null || data.width == 0 || data.height == 0)
                return 0;

            // 计算UV坐标
            float u = (float)x / targetWidth * data.width;
            float v = (float)y / targetHeight * data.height;
    
            int xCoord = Mathf.Clamp((int)u, 0, data.width - 1);
            int yCoord = Mathf.Clamp((int)v, 0, data.height - 1);
    
            return data.pixels[yCoord * data.width + xCoord].grayscale;
        }

        // 修改后的GetMaxTextureWidth/Height方法
        private int GetMaxTextureWidth()
        {
            int width = 0;
            if (useR && rChannel) width = Mathf.Max(width, rChannel.width);
            if (useG && gChannel) width = Mathf.Max(width, gChannel.width);
            if (useB && bChannel) width = Mathf.Max(width, bChannel.width);
            if (useA && aChannel) width = Mathf.Max(width, aChannel.width);
            return width > 0 ? width : 512;
        }

        private int GetMaxTextureHeight()
        {
            int height = 0;
            if (useR && rChannel) height = Mathf.Max(height, rChannel.height);
            if (useG && gChannel) height = Mathf.Max(height, gChannel.width);
            if (useB && bChannel) height = Mathf.Max(height, bChannel.height);
            if (useA && aChannel) height = Mathf.Max(height, aChannel.height);
            return height > 0 ? height : 512;
        }

        private void SaveTexture(Texture2D texture)
        {
            byte[] bytes = texture.EncodeToPNG();
            string fullPath = Path.Combine(savePath, textureName + ".png");
            
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.Refresh();

            // Apply import settings
            string assetPath = fullPath.Replace(Application.dataPath, "Assets");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog("Success", $"Texture saved to: {fullPath}", "OK");
        }
    }
}
