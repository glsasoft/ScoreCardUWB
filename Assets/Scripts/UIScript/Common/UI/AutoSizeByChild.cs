using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI.UIScript.Common.UI
{
    /// <summary>
    /// 根据单个子节点的内容/布局首选尺寸自动调整自身的尺寸。
    /// 默认只跟随子节点的宽度变化，是否跟随高度可通过 followHeight 控制。
    /// Prefab 在编辑器首次打开时可能因尚未完成布局计算导致未刷新，已添加编辑器延迟刷新与可选强制重建。
    /// </summary>
    [ExecuteAlways]
    public class AutoSizeByChild : MonoBehaviour
    {
        // 需要跟随的子节点（不指定则自动取第一个子 RectTransform）
        [SerializeField] private RectTransform childRectTransform;
        // 额外的尺寸补偿（可用于增加内边距）
        [SerializeField] private Vector2 padding = Vector2.zero;
        // 是否在每帧检测（否则只在尺寸改变的生命周期事件中尝试更新）
        [SerializeField] private bool updateEveryFrame = true;
        // 是否优先使用 LayoutUtility 的首选尺寸
        [SerializeField] private bool useLayoutPreferredSize = true;
    // 新增：是否跟随宽度变化（默认跟随，保持原有行为）
    [SerializeField] private bool followWidth = true;
    // 新增：是否跟随高度变化（默认不跟随，仅调整宽度）
    [SerializeField] private bool followHeight; // 默认 false 保持旧行为
#if UNITY_EDITOR
        [Header("Editor Options")] 
        [Tooltip("在编辑器(未运行)读取尺寸前强制进行一次 LayoutRebuilder.ForceRebuildLayoutImmediate 以避免首帧为 0。")] 
        [SerializeField] private bool rebuildLayoutBeforeMeasureInEditMode = true;
        [Tooltip("在编辑器(未运行)增加一次 delayCall 再次刷新，确保 Text/TMP 等延迟生成的首选尺寸被捕获。")] 
        [SerializeField] private bool scheduleDelayedRefreshInEditMode = true;
        [Tooltip("额外在后续帧继续刷新次数(用于 TextMeshPro 等可能延迟多帧的情况) 0=不额外刷新")] 
        [SerializeField][Range(0,5)] private int extraEditorDelayFrames = 1;
        private bool _pendingEditorDelayedRefresh;
        private int _remainingEditorExtraFrames;
#endif

        private RectTransform _selfRectTransform; // 重命名遵循私有字段命名规范
        private float _lastAppliedWidth;
        private float _lastAppliedHeight;

        private void Awake()
        {
            _selfRectTransform = transform as RectTransform;
            AutoAssignChildIfNeeded();
            UpdateSize(true);
#if UNITY_EDITOR
            TryScheduleEditorDelayedRefresh();
#endif
        }

        private void OnEnable()
        {
            UpdateSize(true);
#if UNITY_EDITOR
            TryScheduleEditorDelayedRefresh();
#endif
        }

        private void OnTransformChildrenChanged()
        {
            // 如果子对象被替换，尝试重新获取
            if (childRectTransform == null || !childRectTransform.IsChildOf(transform))
            {
                AutoAssignChildIfNeeded();
            }
            UpdateSize(true);
        }

        private void LateUpdate()
        {
            if (!updateEveryFrame) return;
            UpdateSize(false);
        }

        private void AutoAssignChildIfNeeded()
        {
            if (childRectTransform == null && transform.childCount > 0)
            {
                childRectTransform = transform.GetChild(0) as RectTransform;
            }
        }

        /// <summary>
        /// 根据子节点尺寸更新父节点 sizeDelta。
        /// </summary>
        /// <param name="force">是否强制更新（忽略与上次一样的判断）。</param>
        private void UpdateSize(bool force)
        {
            if (_selfRectTransform == null || childRectTransform == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying && rebuildLayoutBeforeMeasureInEditMode)
            {
                // 同时重建子与自身，确保布局链条完整
                LayoutRebuilder.ForceRebuildLayoutImmediate(childRectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_selfRectTransform);
            }
#endif
            float width;
            float height;

            if (useLayoutPreferredSize)
            {
                // LayoutUtility 会在没有 ILayoutElement 实现时返回 0，需要回退。
                width = LayoutUtility.GetPreferredWidth(childRectTransform);
                height = LayoutUtility.GetPreferredHeight(childRectTransform);
                if (width <= 0f) width = childRectTransform.rect.width;
                if (height <= 0f) height = childRectTransform.rect.height;
            }
            else
            {
                width = childRectTransform.rect.width;
                height = childRectTransform.rect.height;
            }

            width += padding.x;
            height += padding.y;

            // 如果不跟随宽度，则保持当前宽度不变。
            if (!followWidth)
            {
                width = _selfRectTransform.sizeDelta.x; // 保留已有宽度
            }

            // 如果不跟随高度，则保持当前高度不变。
            if (!followHeight)
            {
                height = _selfRectTransform.sizeDelta.y; // 保留已有高度
            }

            bool widthChanged = followWidth && !Mathf.Approximately(width, _lastAppliedWidth);
            bool heightChanged = followHeight && !Mathf.Approximately(height, _lastAppliedHeight);

            if (force || widthChanged || heightChanged)
            {
                _selfRectTransform.sizeDelta = new Vector2(width, height);
                if (followWidth)
                {
                    _lastAppliedWidth = width;
                }
                if (followHeight)
                {
                    _lastAppliedHeight = height;
                }
            }
        }

#if UNITY_EDITOR
        private void TryScheduleEditorDelayedRefresh()
        {
            if (Application.isPlaying) return;
            if (!scheduleDelayedRefreshInEditMode) return;
            if (_pendingEditorDelayedRefresh) return;
            _pendingEditorDelayedRefresh = true;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                _pendingEditorDelayedRefresh = false;
                if (this == null) return; // 组件被销毁
                if (!isActiveAndEnabled) return;
                UpdateSize(true);
                // 如需要多帧刷新，开始监听 EditorApplication.update
                if (extraEditorDelayFrames > 0)
                {
                    _remainingEditorExtraFrames = extraEditorDelayFrames;
                    UnityEditor.EditorApplication.update -= EditorExtraFrameUpdate; // 防止重复添加
                    UnityEditor.EditorApplication.update += EditorExtraFrameUpdate;
                }
            };
        }

        private void EditorExtraFrameUpdate()
        {
            if (this == null || !isActiveAndEnabled || Application.isPlaying)
            {
                CancelEditorExtraUpdates();
                return;
            }
            if (_remainingEditorExtraFrames <= 0)
            {
                CancelEditorExtraUpdates();
                return;
            }
            _remainingEditorExtraFrames--;
            UpdateSize(true);
            if (_remainingEditorExtraFrames <= 0)
            {
                CancelEditorExtraUpdates();
            }
        }

        private void CancelEditorExtraUpdates()
        {
            UnityEditor.EditorApplication.update -= EditorExtraFrameUpdate;
            _remainingEditorExtraFrames = 0;
        }
#endif

        private void OnDisable()
        {
#if UNITY_EDITOR
            CancelEditorExtraUpdates();
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                _selfRectTransform = transform as RectTransform;
                AutoAssignChildIfNeeded();
                UpdateSize(true); // 立即一次
                TryScheduleEditorDelayedRefresh(); // 再安排一次延迟，捕获延迟生成的布局
            }
        }

        [ContextMenu("Refresh Auto Size (Editor)")]
        private void ContextMenuRefresh()
        {
            if (!Application.isPlaying)
            {
                UpdateSize(true);
            }
        }
#endif
    }
}
