using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LetsPiu
{
    public class EffectTexturePostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/Effect") || assetPath.StartsWith("Assets/GLSAQuadView"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.mipmapEnabled = false;
            }
        }
    }
}
