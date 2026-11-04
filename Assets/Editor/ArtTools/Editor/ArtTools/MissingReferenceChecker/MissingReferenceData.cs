using System.Collections.Generic;
using UnityEngine;

namespace ArtTools
{
    /// <summary>
    /// 丢失引用的数据结构
    /// </summary>
    public class MissingReference
    {
        // 资源路径
        public string AssetPath { get; set; }
        
        // 属性名称
        public string PropertyName { get; set; }
        
        // 丢失类型
        public string MissingType { get; set; }
        
        // 节点路径 - 用于详细定位问题位置
        public string NodePath { get; set; }
        
        // 详细错误描述
        public string ErrorDescription { get; set; }
        
        // 组件类型名称（如果适用）
        public string ComponentTypeName { get; set; }
        
        // 实例ID（用于唯一标识）
        public int InstanceID { get; set; }
        
        public MissingReference(string assetPath, string propertyName, string missingType)
        {
            AssetPath = assetPath;
            PropertyName = propertyName;
            MissingType = missingType;
            NodePath = string.Empty;
            ErrorDescription = string.Empty;
            ComponentTypeName = string.Empty;
            InstanceID = 0;
        }
        
        public MissingReference(string assetPath, string propertyName, string missingType, string nodePath, string errorDescription, string componentTypeName = "", int instanceID = 0)
        {
            AssetPath = assetPath;
            PropertyName = propertyName;
            MissingType = missingType;
            NodePath = nodePath;
            ErrorDescription = errorDescription;
            ComponentTypeName = componentTypeName;
            InstanceID = instanceID;
        }
    }
    
    /// <summary>
    /// 检查配置
    /// </summary>
    public class CheckerConfig
    {
        public List<string> DirectoriesToCheck { get; set; } = new List<string>();
        public bool CheckMaterials { get; set; } = true;
        public bool CheckPrefabs { get; set; } = true;
        public bool CheckScenes { get; set; } = true;
        public bool CheckScriptableObjects { get; set; } = true;
    }
    
    /// <summary>
    /// 检查结果
    /// </summary>
    public class CheckResult
    {
        // 按类别分组的丢失引用
        public Dictionary<string, List<MissingReference>> MissingReferences { get; private set; }
        
        // 检查是否完成
        public bool IsCheckComplete { get; set; }
        
        // 总问题数
        public int TotalIssueCount
        {
            get
            {
                int count = 0;
                foreach (var list in MissingReferences.Values)
                {
                    count += list.Count;
                }
                return count;
            }
        }
        
        public CheckResult()
        {
            MissingReferences = new Dictionary<string, List<MissingReference>>();
            IsCheckComplete = false;
        }
        
        public void Clear()
        {
            MissingReferences.Clear();
            IsCheckComplete = false;
        }
        
        public void AddMissingReference(string category, MissingReference reference)
        {
            if (!MissingReferences.ContainsKey(category))
            {
                MissingReferences[category] = new List<MissingReference>();
            }
            
            MissingReferences[category].Add(reference);
        }
    }
}