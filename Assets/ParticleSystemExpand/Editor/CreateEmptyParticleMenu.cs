using UnityEditor;
using UnityEngine;

namespace ExpandParticleSystem
{
    /// <summary>
    /// Hierarchy 右键菜单 GameObject/Effects/VFX：一键建一个空粒子
    /// </summary>
    public static class CreateEmptyParticleMenu
    {
        [MenuItem("GameObject/Effects/VFX", false, 10)]
        private static void CreateEmptyVFX(MenuCommand command)
        {
            GameObject go = new GameObject("VFX");
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "创建空粒子 VFX");

            ParticleSystem particle = go.AddComponent<ParticleSystem>();
            //和预设面板里「空粒子」的效果保持一致
            ParticlePreset.ApplyEmptyParticle(particle);

            Selection.activeGameObject = go;
        }
    }
}
