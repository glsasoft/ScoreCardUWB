using UnityEditor;
using UnityEngine;

namespace ExpandParticleSystem
{
    /// <summary>
    /// 粒子扩展面板的全局开关，存在 EditorPrefs 里，重启编辑器也会保留
    /// </summary>
    public static class ExpandParticleSettings
    {
        private const string EnabledKey = "ExpandParticleSystem.PanelEnabled";

        /// <summary>
        /// 是否在粒子系统组件上显示扩展面板，默认关闭
        /// </summary>
        public static bool PanelEnabled
        {
            get { return EditorPrefs.GetBool(EnabledKey, false); }
            set
            {
                EditorPrefs.SetBool(EnabledKey, value);
                //开关变了，让所有 Inspector 立刻按新状态重画
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}
