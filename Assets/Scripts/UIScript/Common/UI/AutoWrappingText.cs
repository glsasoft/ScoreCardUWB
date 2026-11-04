/*
一个自动换行脚本，挂在 TMP_Text 组件所在的同一对象上。

功能:
1. 根据当前 RectTransform 宽度对源文本进行逐字符(含中文/无空格文本)宽度测量，插入 \n 手动换行，保证不水平溢出。
2. 支持富文本标签(<b><color><size> 等)——会整体保留，不在标签内部拆分。
3. 可配置: 额外行间距、是否运行时持续监听、是否启用脚本。
4. 在编辑器模式 (Edit Mode) 即时预览 (ExecuteAlways)。RectTransform 尺寸 / Inspector 参数 / 文本变化都会触发刷新。

使用说明:
 - 源文本优先取 m_SourceText；若为空则取绑定的 TMP_Text 当前文本。
 - 调用 Refresh() 可手动强制刷新；修改源文本或宽度后若未勾选“自动更新”也可手动刷新。
 - 如果本身开启 TMP 的自动换行，会与此脚本逻辑重复。脚本会强制设置 enableWordWrapping=false。
 - 刷新时会自动移除源文本中的手动换行 (\n / \r)，按当前宽度重新计算换行。

局限:
 - 逐字符测量会比内置换行稍慢，不建议对每帧都在变化的长段文本高频调用；已做缓存避免无变化重复计算。
 - 极端长的单一富文本标签(非常长属性)仍需要一次性测量，通常影响可忽略。
*/

using System.Text;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class AutoWrappingText : MonoBehaviour
{
	[Header("源文本 (留空则使用 TMP_Text 当前文本)")] 
	[TextArea(2, 8)]
	[SerializeField] private string m_SourceText = string.Empty;

	[Header("行为控制")]
	[SerializeField] private bool m_Enabled1 = true;               // 是否启用脚本逻辑
	[SerializeField] private bool m_AutoUpdate = true;             // 播放模式下是否自动检测尺寸/文本变化
	[SerializeField] private bool m_LogOverflowWarning = false;    // 可选: 宽度为0时打印警告

	[Header("显示参数")]
	[Min(0f)]
	[SerializeField] private float m_AdditionalLineSpacing = 0f;   // 额外行间距 (叠加 TMP 自身行距)

	[Header("对齐策略")]
	[SerializeField] private bool m_PreferLongerSecondLine = false; // “正三角”两行策略: 若文本可在两行内放下，则使用最早可行的换行点，让第一行更短、第二行更长（且两行都不溢出）

	[Header("可用宽度计算")]
	[SerializeField] private bool m_UseParentWidth = false; // 改用父节点宽度作为限制 (适配 Stretch + Layout 场景)
	[SerializeField] private float m_PaddingLeft = 0f;      // 计算可用宽度时左内边距
	[SerializeField] private float m_PaddingRight = 0f;     // 计算可用宽度时右内边距

	[Header("调试/监控状态 (只读)")]
	[SerializeField] private float m_LastAppliedWidth;             // 上次运算使用的宽度 (只读展示)
	[SerializeField] private string m_CachedOriginal;              // 缓存的源原始文本 (只读展示)

	private TMP_Text _tmp;
	private RectTransform _rt;
	private string _lastWrapped;          // 上一次生成后的文本
	private string _lastOriginal;         // 上一次的源文本
	private float _lastWidth;             // 上一次的宽度
	private float _lastFontSize;          // 监测字体尺寸变化 (字号变化需要重新测)
	private float _lastLineSpacing;
	private bool _lastPreferLongerSecondLine;
	private bool _isUpdatingInternally;
	private bool _canvasHooked; // 是否已注册 Canvas 回调

	#region Unity Lifecycle
	private void Awake()
	{
		EnsureComponents();
	}

	private void OnEnable()
	{
		EnsureComponents();
		Refresh(true);
		HookCanvasEvent();
	}

	private void OnDisable()
	{
		UnhookCanvasEvent();
	}

	private void OnValidate()
	{
		EnsureComponents();
		Refresh(true);
	}

	private void Update()
	{
		if (!m_Enabled1) return;
		if (!Application.isPlaying) return; // 编辑器下尺寸变化由 OnRectTransformDimensionsChange 处理
		if (!m_AutoUpdate) return;

		if (ShouldRebuild(false))
		{
			Refresh();
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		if (!m_Enabled1) return;
		if (_rt == null) return;
		if (!gameObject.activeInHierarchy) return;

		// 编辑器下或运行时都可触发尺寸监听
		if (ShouldRebuild(true))
		{
			Refresh();
		}
	}
	#endregion

	/// <summary>
	/// 外部可调用强制刷新。force=true 忽略缓存判断。
	/// </summary>
	public void Refresh(bool force = false)
	{
		if (!_tmp || !_rt)
		{
			EnsureComponents();
			if (!_tmp || !_rt) return;
		}
		if (!m_Enabled1)
		{
			// 还原为源文本
			string original = GetOriginalText();
			if (_tmp.text != original)
			{
				_tmp.text = original;
			}
			return;
		}

		if (!force && !ShouldRebuild(false))
		{
			return; // 无需重建
		}

		// 保护递归
		if (_isUpdatingInternally) return;
		_isUpdatingInternally = true;

		// 获取未换行的源文本并移除用户手动换行符 (如果当前 text 是上次包裹后的结果，则回退使用 _lastOriginal)
		string originalText = GetOriginalText();
		originalText = SanitizeOriginal(originalText); // 清除用户手动换行，统一由脚本再分行
		_lastOriginal = originalText; // 保存真实未换行源文本
		m_CachedOriginal = originalText;

		float width = GetAvailableWidth();
		if (width <= 0.01f)
		{
			if (m_LogOverflowWarning)
				Debug.LogWarning($"[AutoWrappingText] 宽度无效({width})，跳过。", this);
			_isUpdatingInternally = false;
			return;
		}

		// 设置显示属性
		ApplyDisplaySettings();

		string wrapped = BuildWrappedText(originalText, width);
		if (_tmp.text != wrapped)
		{
			_tmp.enableWordWrapping = false; // 我们手动插入换行
			_tmp.text = wrapped;
		}

		_lastWrapped = wrapped;
		_lastWidth = width;
		_lastFontSize = _tmp.fontSize;
		_lastLineSpacing = m_AdditionalLineSpacing;
		_lastPreferLongerSecondLine = m_PreferLongerSecondLine;
		m_LastAppliedWidth = width;

		_isUpdatingInternally = false;
	}

	/// <summary>
	/// 判断是否需要重新生成换行。
	/// </summary>
	private bool ShouldRebuild(bool dimensionChangedEvent)
	{
		if (!_tmp || !_rt) return true; // 组件获取失败时尝试刷新

		string original = GetOriginalText();
		if (original != _lastOriginal) return true;
		float width = GetAvailableWidth();
		if (!Mathf.Approximately(width, _lastWidth)) return true;
		if (!Mathf.Approximately(_tmp.fontSize, _lastFontSize)) return true;
		if (!Mathf.Approximately(m_AdditionalLineSpacing, _lastLineSpacing)) return true;
		if (m_PreferLongerSecondLine != _lastPreferLongerSecondLine) return true;

		// 尺寸事件触发但宽度没变且其它参数没变 -> 不需要
		return false;
	}

	/// <summary>
	/// 获取源文本: 优先 Inspector 的 m_SourceText，其次 TMP 的原始 text (没有我们加的换行前)。
	/// </summary>
	private string GetOriginalText()
	{
		// 优先使用用户显式设定的源文本
		if (!string.IsNullOrEmpty(m_SourceText)) return m_SourceText;

		if (_tmp == null) return string.Empty;

		// 如果当前显示的文本正是上一次的包裹结果，则返回未换行的 _lastOriginal，
		// 这样在宽度增大时可以重新测量并还原为单行。之后会再次移除源中的手动换行符。
		if (!string.IsNullOrEmpty(_lastWrapped) && _tmp.text == _lastWrapped && !string.IsNullOrEmpty(_lastOriginal))
		{
			return _lastOriginal;
		}

		// 否则认为用户主动修改了 TMP_Text.text，直接使用当前内容作为新源文本。
		return _tmp.text;
	}

	/// <summary>
	/// 移除手动硬换行符，统一交由本脚本重新分行。
	/// </summary>
	private string SanitizeOriginal(string raw)
	{
		if (string.IsNullOrEmpty(raw)) return string.Empty;
		// 去除 \r 和 \n；不替换为空格，保持字符紧邻，中文/英文均再由算法重新排版。
		return raw.Replace("\r", string.Empty).Replace("\n", string.Empty);
	}

	/// <summary>
	/// 应用对齐、行间距等设置。
	/// </summary>
	private void ApplyDisplaySettings()
	{
		// 只调节额外行距，沿用 TMP_Text 自身对齐设置
		_tmp.lineSpacingAdjustment = m_AdditionalLineSpacing;
	}

	/// <summary>
	/// 计算当前可用宽度。可选择使用父节点宽度 + 内边距扣除。
	/// </summary>
	private float GetAvailableWidth()
	{
		if (_rt == null)
			return 0f;

		float width = _rt.rect.width;
		if (m_UseParentWidth && _rt.parent is RectTransform parentRt)
		{
			float parentWidth = parentRt.rect.width;
			// 以父宽度减去内边距为准。用于 Stretch 或 ContentSizeFitter 后希望使用父的最终宽度。
			width = parentWidth - m_PaddingLeft - m_PaddingRight;
		}

		if (width < 0f) width = 0f; // 防御
		return width;
	}

	/// <summary>
	/// 构建换行后的文本。
	/// </summary>
	private string BuildWrappedText(string original, float maxWidth)
	{
		if (string.IsNullOrEmpty(original)) return string.Empty;

		// 快速路径: 先测整段宽度，如果不超就直接返回 (避免多次逐字符调用开销)
		Vector2 totalSize = _tmp.GetPreferredValues(original, Mathf.Infinity, Mathf.Infinity);
		if (totalSize.x <= maxWidth)
		{
			return original; // 不需要换行
		}

		// 逐 token（字符或富文本标签）处理；由于已移除手动换行，硬换行只可能来自上游保留逻辑（此版本中不会出现原始\n）
		var builder = new StringBuilder(original.Length + 8);
		var lineBuilder = new StringBuilder();

		int len = original.Length;
		for (int i = 0; i < len; )
		{
			char c = original[i];

			// 理论上已清除用户手动换行，这里不再处理原始 \n 作为硬换行

			string token;
			if (c == '<')
			{
				int close = original.IndexOf('>', i);
				if (close != -1)
				{
					token = original.Substring(i, close - i + 1);
					i = close + 1;
				}
				else
				{
					// 非法标签，按普通字符处理
					token = c.ToString();
					i++;
				}
			}
			else
			{
				token = c.ToString();
				i++;
			}

			// 试探加上 token 后的宽度
			string testLine = lineBuilder + token;
			Vector2 size = _tmp.GetPreferredValues(testLine, Mathf.Infinity, Mathf.Infinity);
			if (size.x > maxWidth && lineBuilder.Length > 0)
			{
				// 换行，再将 token 放入新行
				builder.Append(lineBuilder);
				builder.Append('\n');
				lineBuilder.Clear();
				lineBuilder.Append(token);
			}
			else
			{
				lineBuilder.Append(token);
			}
		}

		// 余下最后一行
		if (lineBuilder.Length > 0)
		{
			builder.Append(lineBuilder);
		}

		string result = builder.ToString();

		// 如果启用“第二行更长”策略，并且最终只有两行，则尝试调整分割点。
		if (m_PreferLongerSecondLine)
		{
			int newlineIdx = result.IndexOf('\n');
			if (newlineIdx > -1 && result.IndexOf('\n', newlineIdx + 1) == -1)
			{
				string line1 = result.Substring(0, newlineIdx);
				string line2 = result.Substring(newlineIdx + 1);
				string combined = line1 + line2;

				// “正三角”策略：若当前第一行不比第二行短，则尝试寻找一个更早的换行点，使第一行更短且宽度合法。
				if (line1.Length >= line2.Length)
				{
					int totalLen = combined.Length;
					int originalSplit = line1.Length;
					int bestSplit = -1;
					// 约束：两行宽度都不能超过 maxWidth；避免出现第一行很短但第二行过宽溢出。
					for (int s = 1; s < originalSplit; s++)
					{
						int secondLen = totalLen - s;
						if (secondLen <= s) continue; // 第二行必须更长
						string test1 = combined.Substring(0, s);
						string test2 = combined.Substring(s);
						Vector2 size1 = _tmp.GetPreferredValues(test1, Mathf.Infinity, Mathf.Infinity);
						Vector2 size2 = _tmp.GetPreferredValues(test2, Mathf.Infinity, Mathf.Infinity);
						if (size1.x <= maxWidth && size2.x <= maxWidth)
						{
							bestSplit = s;
							break;
						}
					}
					if (bestSplit > 0 && bestSplit != originalSplit)
					{
						string newLine1 = combined.Substring(0, bestSplit);
						string newLine2 = combined.Substring(bestSplit);
						result = newLine1 + '\n' + newLine2;
					}
				}
			}
		}

		return result;
	}

	private void EnsureComponents()
	{
		if (_tmp == null)
		{
			_tmp = GetComponent<TMP_Text>();
		}
		if (_rt == null && _tmp != null)
		{
			_rt = _tmp.rectTransform;
		}
	}

	private void HookCanvasEvent()
	{
		if (_canvasHooked) return;
		Canvas.willRenderCanvases += OnCanvasRebuild;
		_canvasHooked = true;
	}

	private void UnhookCanvasEvent()
	{
		if (!_canvasHooked) return;
		Canvas.willRenderCanvases -= OnCanvasRebuild;
		_canvasHooked = false;
	}

	/// <summary>
	/// 在 Canvas 重建阶段执行一次检测，处理 LayoutGroup / ContentSizeFitter 导致的延迟宽度更新。
	/// </summary>
	private void OnCanvasRebuild()
	{
		if (!m_Enabled1) return;
		if (!m_AutoUpdate && Application.isPlaying) return; // 运行时关闭自动更新则不在回调里处理
		if (ShouldRebuild(false))
		{
			Refresh();
		}
	}

	#region Public API
	/// <summary>
	/// 设置源文本并刷新。
	/// </summary>
	public void SetSourceText(string text, bool forceRefresh = true)
	{
		m_SourceText = text;
		if (forceRefresh) Refresh(true);
	}

	/// <summary>
	/// 切换启用/禁用并刷新。
	/// </summary>
	public void SetEnabled(bool enabledAndRefresh)
	{
		m_Enabled1 = enabledAndRefresh;
		Refresh(true);
	}
	#endregion

#if UNITY_EDITOR
	[CustomEditor(typeof(AutoWrappingText))]
	private class AutoWrappingTextEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			var aw = (AutoWrappingText)target;
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("手动刷新 (Refresh)", GUILayout.Height(24)))
			{
				aw.Refresh(true);
			}
			if (GUILayout.Button("使用当前显示文本作为源", GUILayout.Height(24)))
			{
				if (aw._tmp != null)
				{
					aw.SetSourceText(aw._tmp.text, true);
				}
			}
			GUILayout.EndHorizontal();
		}
	}
#endif
}