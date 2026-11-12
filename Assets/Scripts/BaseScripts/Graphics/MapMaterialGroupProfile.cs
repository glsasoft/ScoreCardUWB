using System;
using System.Collections.Generic;
using UnityEngine;


namespace LetsPiu.Graphics
{
    public class MapMaterialGroupProfile : MonoBehaviour
    {
        [Header("指定包含材质的文件夹（Assets/开头）")]
        public String materialHighFolder = "Assets/Materials";
        public String materialMediumFolder = "Assets/Materials";
        public String materialLowFolder = "Assets/Materials";
        
        [Header("低中高三档画质材质列表")]
        public List<Material> lowQualityMaterials = new();
        public List<Material> mediumQualityMaterials = new();
        public List<Material> highQualityMaterials = new();

        public List<Material> GetMaterialList(QualityLevel type)
        {
            return type switch
            {
                QualityLevel.Low => lowQualityMaterials,
                QualityLevel.Medium => mediumQualityMaterials,
                QualityLevel.High => highQualityMaterials,
                _ => null
            };
        }
    }
}
