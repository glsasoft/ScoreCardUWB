using LWGUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.DanbaidongGUI
{
    // LWGUI already owns [Ramp]. This stays inside an LWGUI group and opens GradientsRampEditorWindow.
    public class GradientRampDrawer : LWGUI.SubDrawer
    {
        public GradientRampDrawer() { }
        public GradientRampDrawer(string group) : base(group) { }

        protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight; }
        protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Texture; }

        public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            DrawerHelper.InitGUIStyle();
            DrawerHelper.DrawRamp(position, label, ref prop, editor);
        }
    }
}
