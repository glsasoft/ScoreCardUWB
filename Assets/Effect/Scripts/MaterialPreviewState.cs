using UnityEngine;

namespace LetsPiu.Graphics
{
    /// <summary>
    /// 角色材质能不能"动"的总闸。
    ///
    /// Play 模式永远允许；编辑模式只有编辑器预览开关打开时才允许。
    /// 预览开着的时候材质副本全部是 HideAndDontSave，保存场景前由编辑器脚本统一还原，
    /// 所以拖 Timeline 能看见效果，磁盘上的 .mat 和场景里的材质引用都不会被改。
    /// </summary>
    public static class MaterialPreviewState
    {
        /// <summary>
        /// 由编辑器脚本（Tools/GLSA/角色材质/编辑器预览）设置。
        /// 静态字段，不进场景、不进构建；域重载后由编辑器脚本从 EditorPrefs 恢复。
        /// </summary>
        public static bool EditorPreviewEnabled;

        /// <summary>当前是否允许碰材质（Play 或编辑器预览）。</summary>
        public static bool Active
        {
            get { return Application.isPlaying || EditorPreviewEnabled; }
        }

        /// <summary>是否处于纯编辑器的预览状态。用来决定材质副本要不要 HideAndDontSave。</summary>
        public static bool EditorOnly
        {
            get { return !Application.isPlaying && EditorPreviewEnabled; }
        }

        /// <summary>编辑器预览被强制关掉（比如正在保存场景）。</summary>
        public static void Suspend()
        {
            EditorPreviewEnabled = false;
        }
    }
}
