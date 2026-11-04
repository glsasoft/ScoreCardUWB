using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ExpandParticleSystem
{
	/// <summary>
	/// 装饰 Unity 内置编辑器类型时使用的编辑器基类
	/// </summary>
	public abstract class DecoratorEditor : Editor
	{
		// 用反射调用方法时的空参数数组
		private static readonly object[] EMPTY_ARRAY = new object[0];

		#region 编辑器字段

		/// <summary>
		/// 内部实际使用的（被装饰的）编辑器的类型
		/// </summary>
		private System.Type decoratedEditorType;

		/// <summary>
		/// 被这个编辑器编辑的对象的类型
		/// </summary>
		private System.Type editedObjectType;

		private Editor editorInstance;

		#endregion

		private static Dictionary<string, MethodInfo> decoratedMethods = new Dictionary<string, MethodInfo>();

		private static Assembly editorAssembly = Assembly.GetAssembly(typeof(Editor));

		protected Editor EditorInstance
		{
			get
			{
				if (editorInstance == null && targets != null && targets.Length > 0)
				{
					editorInstance = Editor.CreateEditor(targets, decoratedEditorType);
				}

				if (editorInstance == null)
				{
					Debug.LogError("无法创建编辑器！");
				}

				return editorInstance;
			}
		}

		public DecoratorEditor(string editorTypeName)
		{
			this.decoratedEditorType = editorAssembly.GetTypes().Where(t => t.Name == editorTypeName).FirstOrDefault();

			Init();

			// 检查 CustomEditor 标注的类型是否匹配
			var originalEditedType = GetCustomEditorType(decoratedEditorType);

			if (originalEditedType != editedObjectType)
			{
				throw new System.ArgumentException(
					string.Format("类型 {0} 与编辑器 {1} 的类型 {2} 不匹配",
							  editedObjectType, editorTypeName, originalEditedType));
			}
		}

		private System.Type GetCustomEditorType(System.Type type)
		{
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			var attributes = type.GetCustomAttributes(typeof(CustomEditor), true) as CustomEditor[];
			var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", flags)).First();

			return field.GetValue(attributes[0]) as System.Type;
		}

		private void Init()
		{
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			var attributes = this.GetType().GetCustomAttributes(typeof(CustomEditor), true) as CustomEditor[];
			var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", flags)).First();

			editedObjectType = field.GetValue(attributes[0]) as System.Type;
		}

		void OnDisable()
		{
			//不在 OnDisable 里主动销毁内置编辑器实例：Unity 2022 的 ParticleSystemInspector
			//销毁时会抛 ArgumentOutOfRangeException（它自己的内部列表已经空了），
			//try/catch 拦不住（Unity 在内部先记了日志）。交给域重载统一回收。
			editorInstance = null;
		}

		/// <summary>
		/// 把指定名字的方法调用转发给被装饰的编辑器实例
		/// </summary>
		protected void CallInspectorMethod(string methodName)
		{
			MethodInfo method = null;

			// 把 MethodInfo 加入缓存
			if (!decoratedMethods.ContainsKey(methodName))
			{
				var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

				method = decoratedEditorType.GetMethod(methodName, flags);

				if (method != null)
				{
					decoratedMethods[methodName] = method;
				}
				else
				{
					//Debug.LogError(string.Format("Could not find method {0}", method));
					//Debug.Log(method);
				}
			}
			else
			{
				method = decoratedMethods[methodName];
			}

			if (method != null)
			{
				method.Invoke(EditorInstance, EMPTY_ARRAY);
			}
		}

		public void OnSceneGUI()
		{
			CallInspectorMethod("OnSceneGUI");
		}

		protected override void OnHeaderGUI()
		{
			CallInspectorMethod("OnHeaderGUI");
		}

		public override void OnInspectorGUI()
		{
			EditorInstance.OnInspectorGUI();
		}

		public override void DrawPreview(Rect previewArea)
		{
			EditorInstance.DrawPreview(previewArea);
		}

		public override string GetInfoString()
		{
			return EditorInstance.GetInfoString();
		}

		public override GUIContent GetPreviewTitle()
		{
			return EditorInstance.GetPreviewTitle();
		}

		public override bool HasPreviewGUI()
		{
			return EditorInstance.HasPreviewGUI();
		}

		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
		{
			EditorInstance.OnInteractivePreviewGUI(r, background);
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			EditorInstance.OnPreviewGUI(r, background);
		}

		public override void OnPreviewSettings()
		{
			EditorInstance.OnPreviewSettings();
		}

		public override void ReloadPreviewInstances()
		{
			EditorInstance.ReloadPreviewInstances();
		}

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			return EditorInstance.RenderStaticPreview(assetPath, subAssets, width, height);
		}

		public override bool RequiresConstantRepaint()
		{
			return EditorInstance.RequiresConstantRepaint();
		}

		public override bool UseDefaultMargins()
		{
			return EditorInstance.UseDefaultMargins();
		}
	}
}

