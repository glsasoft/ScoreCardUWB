using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Presets;
#endif
namespace ExpandParticleSystem
{
    /// <summary>
    /// 预设里一起记录的材质，和 presets 一一对应
    /// </summary>
    [System.Serializable]
    public class PresetMaterialSet
    {
        public Material[] materials;
    }

    /// <summary>
    /// 粒子拓展预设集合
    /// 预设本身只在编辑器里用，打包 Player 时相关字段和方法会被条件编译剔除
    /// </summary>
    public class ParticlePresetClass : ScriptableObject
    {
#if UNITY_EDITOR
        public List<Preset> presets ;
#endif
        [Tooltip("与 presets 一一对应，预设应用时一起切换的材质")]
        public List<PresetMaterialSet> presetMaterials;

        public ParticlePresetClass()
        {
#if UNITY_EDITOR
            if(presets == null)
            {
                presets = new List<Preset>();
            }
            presets.Add(null);
#endif
            if (presetMaterials == null)
            {
                presetMaterials = new List<PresetMaterialSet>();
            }
            presetMaterials.Add(null);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 把当前粒子渲染器的材质记录进指定预设
        /// </summary>
        /// <param name="index">预设下标</param>
        /// <param name="particle">当前粒子系统</param>
        public void CaptureMaterials(int index, ParticleSystem particle)
        {
            EnsureMaterialList();
            if (index < 0 || index >= presetMaterials.Count)
            {
                return;
            }
            ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                presetMaterials[index] = null;
                return;
            }
            PresetMaterialSet set = presetMaterials[index];
            if (set == null)
            {
                set = new PresetMaterialSet();
                presetMaterials[index] = set;
            }
            set.materials = renderer.sharedMaterials;
        }

        /// <summary>
        /// 把预设里记录的材质应用到粒子渲染器
        /// </summary>
        /// <param name="index">预设下标</param>
        /// <param name="particle">当前粒子系统</param>
        public void ApplyMaterials(int index, ParticleSystem particle)
        {
            EnsureMaterialList();
            if (index < 0 || index >= presetMaterials.Count || presetMaterials[index] == null)
            {
                return;
            }
            Material[] materials = presetMaterials[index].materials;
            if (materials == null)
            {
                return;
            }
            ParticleSystemRenderer renderer = particle.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                return;
            }
            renderer.sharedMaterials = materials;
        }

        /// <summary>
        /// 补齐材质列表长度，保证和 presets 对齐
        /// </summary>
        public void EnsureMaterialList()
        {
            if (presetMaterials == null)
            {
                presetMaterials = new List<PresetMaterialSet>();
            }
            while (presetMaterials.Count < presets.Count)
            {
                presetMaterials.Add(null);
            }
        }
#endif
    }
}
