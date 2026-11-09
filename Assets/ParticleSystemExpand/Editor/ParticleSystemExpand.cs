using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ExpandParticleSystem
{

    [CustomEditor(typeof(ParticleSystem))]

    public class ParticleSystemExpand : DecoratorEditor
    {

                private string ButtonStyleName
        #if UNITY_2018
                 = "ScriptText";
        #else
                  = "AnimClipToolbarPopup";
        #endif
        private ParticleSystem thisParticleSystem;//自身的粒子系统

        public bool isOpen_ParticleTransform = true;//是否打开Transform（默认展开，其余面板默认折叠）


        public bool isOpen_ParticlePreset = false;//是否打开粒子预设
        public ParticlePreset particlePreset;
        public Vector2 presetScrollPosition = Vector2.zero;//预设列表滚动位置

        public bool isOpen_ParticleRot = false;//是否打开粒子旋转
        public ParticleRot particleRot;

        public bool isOpen_ParticleMove = false;//是否打开粒子移动
        public ParticleMove particleMove;

        public bool isOpen_ParticleShape = false;//是否打开粒子
        public ParticleShape particleShape;

        public bool isOpen_Optimize = false;//是否打开优化
        public int maxParticlesValue = 1000;//要限制的最大粒子数
        public float maxParticleSizeValue = 44f;//渲染器上的最大粒子尺寸 Max Particle Size
        public string namePrefix = "";//给子级粒子物体加的前缀
        public string nameSuffix = "";//给子级粒子物体加的后缀
        public int nameIndexStart = 1;//序号起始值
        public int nameIndexDigits = 2;//序号补零位数
        private const int DefaultMaxParticles = 1000;//Unity 默认的最大粒子数，只改这个值的
        private const float DefaultMaxParticleSize = 0.5f;//Unity 默认的最大粒子尺寸，只改这个值的
        public ParticleSystemExpand() : base("ParticleSystemInspector") { }
        private void OnEnable()
        {
            thisParticleSystem = target as ParticleSystem;

            //总开关关掉时把扩展组件停掉，别让它们继续改粒子参数（比如锁住 shape 的 radius）
            if (!ExpandParticleSettings.PanelEnabled)
            {
                SetExtendComponentsEnabled(false);
                return;
            }

            //粒子预设
            Enable(out particlePreset);
            //粒子旋转
            Enable(out particleRot);
            //粒子移动
            Enable(out particleMove);
            //粒子移动
            Enable(out particleShape);

            maxParticlesValue = thisParticleSystem.main.maxParticles;
        }
        /// <summary>
        /// 统一开关挂在这个粒子上的扩展组件
        /// </summary>
        /// <param name="enabled">要启用还是禁用</param>
        private void SetExtendComponentsEnabled(bool enabled)
        {
            ParticleRot rot = thisParticleSystem.GetComponent<ParticleRot>();
            if (rot != null)
            {
                rot.enabled = enabled;
            }
            ParticleMove move = thisParticleSystem.GetComponent<ParticleMove>();
            if (move != null)
            {
                move.enabled = enabled;
            }
            ParticleShape shape = thisParticleSystem.GetComponent<ParticleShape>();
            if (shape != null)
            {
                shape.enabled = enabled;
            }
            ParticlePreset preset = thisParticleSystem.GetComponent<ParticlePreset>();
            if (preset != null)
            {
                preset.enabled = enabled;
            }
        }
        public override void OnInspectorGUI()
        {
            //总开关关掉时只显示 Unity 原生的粒子面板
            if (!ExpandParticleSettings.PanelEnabled)
            {
                base.OnInspectorGUI();
                return;
            }
            ///重置Transform
            GUIUpdate_ParticleTransform();
            ///粒子预设
            GUIUpdate_ParticlePreset();
            ///绕轴旋转
            GUIUpdate_ParticleRot();
            ///移动配置
            GUIUpdate_ParticleMove();
            ///粒子形状
            GUIUpdate_ParticleShape();
            ///优化
            GUIUpdate_Optimize();
            base.OnInspectorGUI();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <typeparam name="T">初始化类型</typeparam>
        /// <param name="t">类</param>
        private void Enable<T>(out T t) where T: MonoBehaviour
        {
            t = thisParticleSystem.gameObject.GetComponent<T>();
            if (t == null)
            {
                t = thisParticleSystem.gameObject.AddComponent<T>();
            }
            //不管是新建的还是场景里已有的，都强制隐藏，避免面板上多出一堆组件
            if (t.hideFlags != HideFlags.HideInInspector)
            {
                t.hideFlags = HideFlags.HideInInspector;
            }
        }
#region 粒子预设
        //private void Enable_ParticlePreset()
        //{
        //    particlePreset = thisParticleSystem.gameObject.GetComponent<ParticlePreset>();
        //    if (particlePreset == null)
        //    {
        //        particlePreset = thisParticleSystem.gameObject.AddComponent<ParticlePreset>();
        //        //particlePreset.hideFlags = HideFlags.HideInInspector;
        //    }
        //}
        private void GUIUpdate_ParticlePreset()
        {
       
            using (new GUILayout.HorizontalScope("box"))
            {
                EditorGUILayout.LabelField("", GUILayout.Width(20));
                if (GUILayout.Button(new GUIContent("粒子预设", "从预设集合里挑一个，一键把参数套用到当前粒子系统"), new GUIStyle(ButtonStyleName)))
                {
                    isOpen_ParticlePreset = !isOpen_ParticlePreset;
                }
            }
            if (isOpen_ParticlePreset)
            {
                if (GUILayout.Button(new GUIContent("空粒子", "关闭发射和形状、取消循环，并把 Transform 归零")))
                {
                    particlePreset.NullParticle();
                }
                if (GUILayout.Button(new GUIContent("单个粒子", "只发射一颗粒子，速度归零，用来单独看一颗粒子的表现")))
                {
                    particlePreset.OneParticle();
                }
                if(particlePreset.presetClass == null)
                {
                    particlePreset.Load();
                    return;
                }
                particlePreset.presetClass.EnsureMaterialList();
                //预设多了以后限制高度，超出部分用滚动条
                presetScrollPosition = GUILayout.BeginScrollView(presetScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.MaxHeight(260));
                for(int i=0;i< particlePreset.presetClass.presets.Count;i++)
                {
                    if(particlePreset.presetClass.presets.Count == 1 && particlePreset.presetClass.presets[0] == null)
                    {
                        break;
                    }
                    if(i != 0 && i!= particlePreset.presetClass.presets.Count - 1 && particlePreset.presetClass.presets[i] == null)
                    {
                        particlePreset.presetClass.presets.RemoveAt(i);
                        if (i < particlePreset.presetClass.presetMaterials.Count)
                        {
                            particlePreset.presetClass.presetMaterials.RemoveAt(i);
                        }
                        continue;
                    }
                    if (particlePreset.presetClass.presets[i] != null)
                    {
                        //左边材质缩略图，右边名字和材质名
                        using (new GUILayout.HorizontalScope())
                        {
                            Texture preview = GetPresetPreview(i);
                            if (preview != null)
                            {
                                GUILayout.Label(preview, GUILayout.Width(40), GUILayout.Height(40));
                            }
                            else
                            {
                                GUILayout.Space(40);
                            }
                            using (new GUILayout.VerticalScope())
                            {
                                if (GUILayout.Button(new GUIContent(particlePreset.presetClass.presets[i].name, "点击把该预设的参数和材质一起套用到当前粒子系统")))
                                {
                                    particlePreset.presetClass.presets[i].ApplyTo(thisParticleSystem);
                                    ParticleSystemRenderer renderer = thisParticleSystem.GetComponent<ParticleSystemRenderer>();
                                    if (renderer != null)
                                    {
                                        Undo.RecordObject(renderer, "应用预设材质");
                                    }
                                    particlePreset.presetClass.ApplyMaterials(i, thisParticleSystem);
                                }
                                GUILayout.Label(new GUIContent(GetPresetMaterialName(i), "这个预设记录的材质，在「ArtTools/VFX/粒子预设」窗口里记"));
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
        }
        /// <summary>
        /// 预设记录的材质名，用于面板上显示
        /// </summary>
        /// <param name="index">预设下标</param>
        /// <returns>材质名，没有记录时返回「无材质」</returns>
        private string GetPresetMaterialName(int index)
        {
            if (particlePreset.presetClass == null || particlePreset.presetClass.presetMaterials == null)
            {
                return "无材质";
            }
            if (index >= particlePreset.presetClass.presetMaterials.Count)
            {
                return "无材质";
            }
            PresetMaterialSet set = particlePreset.presetClass.presetMaterials[index];
            if (set == null || set.materials == null || set.materials.Length == 0 || set.materials[0] == null)
            {
                return "无材质";
            }
            if (set.materials.Length > 1)
            {
                return set.materials[0].name + " 等" + set.materials.Length + "个";
            }
            return set.materials[0].name;
        }
        /// <summary>
        /// 预设的缩略图，取它记录的第一个材质的预览图
        /// </summary>
        /// <param name="index">预设下标</param>
        /// <returns>材质预览图，没有材质时返回 null</returns>
        private Texture GetPresetPreview(int index)
        {
            if (particlePreset.presetClass == null || particlePreset.presetClass.presetMaterials == null)
            {
                return null;
            }
            if (index >= particlePreset.presetClass.presetMaterials.Count)
            {
                return null;
            }
            PresetMaterialSet set = particlePreset.presetClass.presetMaterials[index];
            if (set == null || set.materials == null || set.materials.Length == 0 || set.materials[0] == null)
            {
                return null;
            }
            Material material = set.materials[0];
            return PlanePreview.Get(material);
        }
#endregion
#region 粒子围绕轴旋转
        private void GUIUpdate_ParticleRot()
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                particleRot.useRot = EditorGUILayout.Toggle(new GUIContent("", "开启后粒子围绕指定的轴心旋转"), particleRot.useRot, GUILayout.Width(20));
                particleRot.enabled = particleRot.useRot;
                if (GUILayout.Button(new GUIContent("粒子绕轴转", "让粒子绕一个轴心做圆周运动，漩涡、环绕类特效常用"), new GUIStyle(ButtonStyleName)))
                {
                    isOpen_ParticleRot = !isOpen_ParticleRot;
                }
            }
            if (isOpen_ParticleRot)
            {
                particleRot.rotObject = EditorGUILayout.ObjectField(new GUIContent("旋转轴", "作为旋转中心的 Transform，粒子会绕着它转"), particleRot.rotObject, typeof(Transform), true) as Transform;
                //particleRot.rotObject = EditorGUILayout.Vector3Field("旋转轴", particleRot.rotObject);
                particleRot.direction = (ParticleRot.Direction)EditorGUILayout.EnumPopup(new GUIContent("轴向", "绕哪个轴转，X / Y / Z"), particleRot.direction);
                particleRot.speed = EditorGUILayout.FloatField(new GUIContent("旋转速度", "每秒旋转的角度，正负决定转向"), particleRot.speed);
            }
        }
#endregion
#region 粒子移动
        private void GUIUpdate_ParticleMove()
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                particleMove.useMove = EditorGUILayout.Toggle(new GUIContent("", "开启后粒子会按移动配置发生位移"), particleMove.useMove, GUILayout.Width(20));
                particleMove.enabled = particleMove.useMove;
                if (GUILayout.Button(new GUIContent("粒子移动", "让粒子沿指定方向位移，可按时间或按距离移动"), new GUIStyle(ButtonStyleName)))
                {
                    isOpen_ParticleMove = !isOpen_ParticleMove;
                }
            }
            if (isOpen_ParticleMove)
            {
                //预览控制：暂停/继续、重播、循环
                using (new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button(new GUIContent(particleMove.Paused ? "播放" : "暂停", "暂停或继续位移、旋转的预览")))
                    {
                        if (particleMove.Paused)
                        {
                            particleMove.Play();
                        }
                        else
                        {
                            particleMove.Pause();
                        }
                        particleRot.paused = particleMove.Paused;
                    }
                    if (GUILayout.Button(new GUIContent("重播", "把位移送回起点，从头播放")))
                    {
                        if (particleMove.moveObj != null)
                        {
                            Undo.RecordObject(particleMove.moveObj.transform, "重播位移");
                        }
                        particleMove.Restart();
                    }
                    particleMove.loop = GUILayout.Toggle(particleMove.loop, new GUIContent("循环", "位移播完后自动回到起点重来"));
                }
                using (new GUILayout.HorizontalScope("box"))
                {

                    particleMove.moveObj = EditorGUILayout.ObjectField(new GUIContent("移动的物体", "实际被移动的对象，一般就是粒子系统自己或它的父物体"), particleMove.moveObj, typeof(GameObject), true) as GameObject;
                    if (GUILayout.Button(new GUIContent("自身", "一键填入当前粒子系统所在的物体")))
                    {
                        particleMove.moveObj = thisParticleSystem.gameObject;
                    }
                }
                particleMove.moveInformation = EditorGUILayout.ObjectField(new GUIContent("移动配置", "位移规则资产（MoveBase），决定按时间还是按距离移动"), particleMove.moveInformation, typeof(MoveBase), true) as MoveBase;
            }
        }
#endregion
#region 发射器形状更改
        private void GUIUpdate_ParticleShape()
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                particleShape.useShape = EditorGUILayout.Toggle(new GUIContent("", "开启后用曲线控制发射器形状随时间变化"), particleShape.useShape, GUILayout.Width(20));
                particleShape.enabled = particleShape.useShape;
                if (GUILayout.Button(new GUIContent("发射器形状更改", "用曲线在整个生命周期里改变发射器的半径或 XYZ 缩放"), new GUIStyle(ButtonStyleName)))
                {
                    isOpen_ParticleShape = !isOpen_ParticleShape;
                }
            }
            if (isOpen_ParticleShape)
            {
                EditorGUI.BeginChangeCheck();
                particleShape.scaleType = (ParticleShape.ScaleType)EditorGUILayout.EnumPopup(new GUIContent("缩放选项", "按半径整体缩放，还是按 X / Y / Z 三轴分别缩放"), particleShape.scaleType);
                if(EditorGUI.EndChangeCheck())
                {
                    particleShape.ResetShape();
                }
                switch (particleShape.scaleType)
                {
                    case ParticleShape.ScaleType.Radius:
                        {
                            particleShape.Radius_size = EditorGUILayout.CurveField(new GUIContent("曲线", "横轴为粒子生命周期 0-1，纵轴为发射器半径"), particleShape.Radius_size);
                            break;
                        }
                    case ParticleShape.ScaleType.Scale:
                        {
                            particleShape.X_size = EditorGUILayout.CurveField(new GUIContent("X", "横轴为粒子生命周期 0-1，纵轴为 X 轴缩放"), particleShape.X_size);
                            particleShape.Y_size = EditorGUILayout.CurveField(new GUIContent("Y", "横轴为粒子生命周期 0-1，纵轴为 Y 轴缩放"), particleShape.Y_size);
                            particleShape.Z_size = EditorGUILayout.CurveField(new GUIContent("Z", "横轴为粒子生命周期 0-1，纵轴为 Z 轴缩放"), particleShape.Z_size);
                            break;
                        }
                }
            }
        }
#endregion
#region Transform
        private void GUIUpdate_ParticleTransform()
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                EditorGUILayout.LabelField("", GUILayout.Width(20));
                if (GUILayout.Button("Transform", new GUIStyle(ButtonStyleName)))
                {
                    isOpen_ParticleTransform = !isOpen_ParticleTransform;
                }
            }
            if (isOpen_ParticleTransform)
            {
                if (GUILayout.Button("重置"))
                {
                    ModifyTransforms("重置 Transform", tr =>
                    {
                        tr.localPosition = Vector3.zero;
                        tr.localEulerAngles = Vector3.zero;
                        tr.localScale = Vector3.one;
                    });
                }
                if (GUILayout.Button("重置位置"))
                {
                    ModifyTransforms("重置位置", tr => tr.localPosition = Vector3.zero);
                }
                if (GUILayout.Button(new GUIContent("重置旋转", "把本地旋转归零")))
                {
                    ModifyTransforms("重置旋转", tr => tr.localEulerAngles = Vector3.zero);
                }
                //绕 XYZ 各加减 90 度，两行三列
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("X +90°", "绕 X 轴再转 90 度")))
                    {
                        RotateByAxis("X", 0, 90f);
                    }
                    if (GUILayout.Button(new GUIContent("Y +90°", "绕 Y 轴再转 90 度")))
                    {
                        RotateByAxis("Y", 1, 90f);
                    }
                    if (GUILayout.Button(new GUIContent("Z +90°", "绕 Z 轴再转 90 度")))
                    {
                        RotateByAxis("Z", 2, 90f);
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("X -90°", "绕 X 轴反着转 90 度")))
                    {
                        RotateByAxis("X", 0, -90f);
                    }
                    if (GUILayout.Button(new GUIContent("Y -90°", "绕 Y 轴反着转 90 度")))
                    {
                        RotateByAxis("Y", 1, -90f);
                    }
                    if (GUILayout.Button(new GUIContent("Z -90°", "绕 Z 轴反着转 90 度")))
                    {
                        RotateByAxis("Z", 2, -90f);
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("重置大小", "把三个轴的缩放都归为 1")))
                    {
                        ModifyTransforms("重置大小", tr => tr.localScale = Vector3.one);
                    }
                    if (GUILayout.Button(new GUIContent("大小+100", "三个轴的缩放各加 100")))
                    {
                        ModifyTransforms("大小+100", tr => tr.localScale += new Vector3(100f, 100f, 100f));
                    }
                }
                //渲染排序：本体和子级的 Order in Layer 一起加减
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("Order in Layer +1", "本体和所有子级渲染器的排序层 +1")))
                    {
                        ChangeSortingOrder(1);
                    }
                    if (GUILayout.Button(new GUIContent("Order in Layer -1", "本体和所有子级渲染器的排序层 -1")))
                    {
                        ChangeSortingOrder(-1);
                    }
                }

            }
        }
        /// <summary>
        /// 对所有选中对象的 Transform 执行同一修改，并注册 Undo（支持 Ctrl+Z 撤销）
        /// </summary>
        /// <param name="undoName">撤销列表里显示的名字</param>
        /// <param name="modify">对单个 Transform 要做的修改</param>
        private void ModifyTransforms(string undoName, System.Action<Transform> modify)
        {
            List<Transform> transforms = new List<Transform>();
            foreach (var t in targets)
            {
                ParticleSystem ps = t as ParticleSystem;
                if (ps != null && !transforms.Contains(ps.transform))
                {
                    transforms.Add(ps.transform);
                }
            }
            if (transforms.Count == 0)
            {
                return;
            }
            //先记录修改前的状态，撤销时才能还原
            Undo.RecordObjects(transforms.ToArray(), undoName);
            for (int i = 0; i < transforms.Count; i++)
            {
                modify(transforms[i]);
                //预制体实例要标记修改，撤销时才会写回实例的覆盖值
                if (PrefabUtility.IsPartOfPrefabInstance(transforms[i]))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(transforms[i]);
                }
                EditorUtility.SetDirty(transforms[i]);
            }
        }
        /// <summary>
        /// 绕指定轴叠加旋转 90 度，支持 Undo
        /// </summary>
        /// <param name="axisName">轴名，X / Y / Z</param>
        /// <param name="axisIndex">轴下标：0 = X，1 = Y，2 = Z</param>
        /// <param name="angle">旋转角度，正负决定方向</param>
        private void RotateByAxis(string axisName, int axisIndex, float angle)
        {
            Vector3 axis = Vector3.right;
            if (axisIndex == 1)
            {
                axis = Vector3.up;
            }
            else if (axisIndex == 2)
            {
                axis = Vector3.forward;
            }
            //用四元数直接累加，避免反复读写欧拉角产生的精度误差
            Quaternion delta = Quaternion.AngleAxis(angle, axis);
            string undoName = "绕 " + axisName + " 轴 " + (angle > 0 ? "+" : "") + angle + "°";
            ModifyTransforms(undoName, tr =>
            {
                tr.localRotation = tr.localRotation * delta;
            });
        }
#endregion
#region 优化
        private void GUIUpdate_Optimize()
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                EditorGUILayout.LabelField("", GUILayout.Width(20));
                if (GUILayout.Button(new GUIContent("优化", "批量调整性能相关参数"), new GUIStyle(ButtonStyleName)))
                {
                    isOpen_Optimize = !isOpen_Optimize;
                }
            }
            if (isOpen_Optimize)
            {
                using (new GUILayout.HorizontalScope("box"))
                {
                    maxParticlesValue = EditorGUILayout.IntField(new GUIContent("最大粒子数", "只改原本是 1000 的粒子系统，手动调过的保持不变"), maxParticlesValue);
                    if (GUILayout.Button(new GUIContent("应用", "把上面的数值写进本体和所有原本为 1000 的子发射器"), GUILayout.Width(56)))
                    {
                        ApplyMaxParticles(maxParticlesValue);
                    }
                }
                //第二项：最大粒子尺寸 Max Particle Size
                using (new GUILayout.HorizontalScope("box"))
                {
                    maxParticleSizeValue = EditorGUILayout.FloatField(new GUIContent("最大尺寸", "只改原本是 0.5 的渲染器，手动调过的保持不变"), maxParticleSizeValue);
                    if (GUILayout.Button(new GUIContent("应用", "写进本体和所有原本为 0.5 的子级渲染器"), GUILayout.Width(56)))
                    {
                        ApplyMaxParticleSize(maxParticleSizeValue);
                    }
                }
                //第三项：加前缀 / 删前缀
                using (new GUILayout.HorizontalScope("box"))
                {
                    namePrefix = EditorGUILayout.TextField(new GUIContent("前缀", "子级粒子物体名用的前缀，加和删都看这个输入框"), namePrefix);
                    if (GUILayout.Button(new GUIContent("加前缀", "只给子级粒子物体加前缀，本体不动"), GUILayout.Width(56)))
                    {
                        ApplyNameAffix(namePrefix, "");
                    }
                    if (GUILayout.Button(new GUIContent("删前缀", "去掉子级粒子物体名开头的这个前缀"), GUILayout.Width(56)))
                    {
                        RemoveNameAffix(namePrefix, "");
                    }
                }
                //第四项：加后缀 / 删后缀
                using (new GUILayout.HorizontalScope("box"))
                {
                    nameSuffix = EditorGUILayout.TextField(new GUIContent("后缀", "子级粒子物体名用的后缀，加和删都看这个输入框"), nameSuffix);
                    if (GUILayout.Button(new GUIContent("加后缀", "只给子级粒子物体加后缀，本体不动"), GUILayout.Width(56)))
                    {
                        ApplyNameAffix("", nameSuffix);
                    }
                    if (GUILayout.Button(new GUIContent("删后缀", "去掉子级粒子物体名结尾的这个后缀"), GUILayout.Width(56)))
                    {
                        RemoveNameAffix("", nameSuffix);
                    }
                }
                //第四项：在后面依次加序号
                using (new GUILayout.HorizontalScope("box"))
                {
                    GUILayout.Label(new GUIContent("起始", "从几开始编号"), GUILayout.Width(32));
                    nameIndexStart = EditorGUILayout.IntField(nameIndexStart, GUILayout.Width(40));
                    GUILayout.Label(new GUIContent("位数", "补零位数，2 就是 01、02"), GUILayout.Width(32));
                    nameIndexDigits = EditorGUILayout.IntField(nameIndexDigits, GUILayout.Width(40));
                    if (GUILayout.Button(new GUIContent("加序号", "按层级顺序给子级粒子物体名后面加 _01、_02…")))
                    {
                        ApplyNameIndex(nameIndexStart, Mathf.Max(1, nameIndexDigits));
                    }
                }
            }
        }
        /// <summary>
        /// 给所有子级粒子物体改名，加上前缀和/或后缀，本体不动
        /// </summary>
        /// <param name="prefix">要加的前缀，传空则不加</param>
        /// <param name="suffix">要加的后缀，传空则不加</param>
        private void ApplyNameAffix(string prefix, string suffix)
        {
            if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
            {
                return;
            }
            List<GameObject> objects = CollectChildParticleObjects();
            if (objects.Count == 0)
            {
                return;
            }
            Undo.RecordObjects(objects.ToArray(), "添加前后缀");
            for (int i = 0; i < objects.Count; i++)
            {
                string name = objects[i].name;
                if (!string.IsNullOrEmpty(prefix) && !name.StartsWith(prefix))
                {
                    name = prefix + name;
                }
                if (!string.IsNullOrEmpty(suffix) && !name.EndsWith(suffix))
                {
                    name = name + suffix;
                }
                if (name != objects[i].name)
                {
                    objects[i].name = name;
                }
            }
        }
        /// <summary>
        /// 去掉所有子级粒子物体名的前缀和/或后缀，本体不动
        /// </summary>
        /// <param name="prefix">要去掉的前缀，传空则不动</param>
        /// <param name="suffix">要去掉的后缀，传空则不动</param>
        private void RemoveNameAffix(string prefix, string suffix)
        {
            if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
            {
                return;
            }
            List<GameObject> objects = CollectChildParticleObjects();
            if (objects.Count == 0)
            {
                return;
            }
            Undo.RecordObjects(objects.ToArray(), "删除前后缀");
            for (int i = 0; i < objects.Count; i++)
            {
                string name = objects[i].name;
                if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix))
                {
                    name = name.Substring(prefix.Length);
                }
                if (!string.IsNullOrEmpty(suffix) && name.EndsWith(suffix) && name.Length >= suffix.Length)
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                }
                //删完变空名或者没变化就跳过
                if (string.IsNullOrEmpty(name) || name == objects[i].name)
                {
                    continue;
                }
                objects[i].name = name;
            }
        }
        /// <summary>
        /// 按层级顺序给子级粒子物体名后面加 _01、_02 这样的序号
        /// </summary>
        /// <param name="start">起始序号</param>
        /// <param name="digits">补零位数</param>
        private void ApplyNameIndex(int start, int digits)
        {
            List<GameObject> objects = CollectChildParticleObjects();
            if (objects.Count == 0)
            {
                return;
            }
            Undo.RecordObjects(objects.ToArray(), "添加序号");
            int index = start;
            for (int i = 0; i < objects.Count; i++)
            {
                //先去掉已有的 _数字 结尾，重复点不会越叠越长
                string baseName = RemoveIndexSuffix(objects[i].name);
                string newName = baseName + "_" + index.ToString("D" + digits);
                if (newName != objects[i].name)
                {
                    objects[i].name = newName;
                }
                index++;
            }
        }
        /// <summary>
        /// 去掉名字结尾的 _数字
        /// </summary>
        /// <param name="name">原名字</param>
        /// <returns>去掉序号后的名字</returns>
        private string RemoveIndexSuffix(string name)
        {
            int i = name.Length - 1;
            while (i >= 0 && char.IsDigit(name[i]))
            {
                i--;
            }
            if (i >= 0 && i < name.Length - 1 && name[i] == '_')
            {
                return name.Substring(0, i);
            }
            return name;
        }
        /// <summary>
        /// 收集所有子级粒子物体（不含本体）
        /// </summary>
        /// <returns>子级粒子的 GameObject 列表</returns>
        private List<GameObject> CollectChildParticleObjects()
        {
            List<GameObject> objects = new List<GameObject>();
            //层级里的子发射器，排除本体
            ParticleSystem[] inChildren = thisParticleSystem.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < inChildren.Length; i++)
            {
                if (inChildren[i] == thisParticleSystem)
                {
                    continue;
                }
                if (!objects.Contains(inChildren[i].gameObject))
                {
                    objects.Add(inChildren[i].gameObject);
                }
            }
            return objects;
        }
        /// <summary>
        /// 把最大粒子数写进本体和所有子发射器，只改原本是默认值 1000 的
        /// </summary>
        /// <param name="maxParticles">要限制的最大粒子数</param>
        private void ApplyMaxParticles(int maxParticles)
        {
            if (maxParticles <= 0)
            {
                return;
            }
            List<ParticleSystem> systems = new List<ParticleSystem>();
            //本体和层级里的所有子发射器
            ParticleSystem[] inChildren = thisParticleSystem.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < inChildren.Length; i++)
            {
                if (!systems.Contains(inChildren[i]))
                {
                    systems.Add(inChildren[i]);
                }
            }
            //只改还是 Unity 默认值 1000 的，已经手动调过的不动
            List<ParticleSystem> toChange = new List<ParticleSystem>();
            for (int i = 0; i < systems.Count; i++)
            {
                if (systems[i].main.maxParticles == DefaultMaxParticles)
                {
                    toChange.Add(systems[i]);
                }
            }
            if (toChange.Count == 0)
            {
                return;
            }
            Undo.RecordObjects(toChange.ToArray(), "限制最大粒子数");
            for (int i = 0; i < toChange.Count; i++)
            {
                ParticleSystem.MainModule main = toChange[i].main;
                main.maxParticles = maxParticles;
                EditorUtility.SetDirty(toChange[i]);
            }
        }
        /// <summary>
        /// 把最大粒子尺寸写进本体和所有子级粒子的渲染器
        /// </summary>
        /// <param name="size">Max Particle Size 的值</param>
        private void ApplyMaxParticleSize(float size)
        {
            if (size <= 0f)
            {
                return;
            }
            List<ParticleSystemRenderer> renderers = CollectParticleRenderers();
            if (renderers.Count == 0)
            {
                return;
            }
            //只改还是 Unity 默认值 0.5 的，已经手动调过的不动
            List<ParticleSystemRenderer> toChange = new List<ParticleSystemRenderer>();
            for (int i = 0; i < renderers.Count; i++)
            {
                if (Mathf.Approximately(renderers[i].maxParticleSize, DefaultMaxParticleSize))
                {
                    toChange.Add(renderers[i]);
                }
            }
            if (toChange.Count == 0)
            {
                return;
            }
            Undo.RecordObjects(toChange.ToArray(), "设置最大粒子尺寸");
            for (int i = 0; i < toChange.Count; i++)
            {
                toChange[i].maxParticleSize = size;
                EditorUtility.SetDirty(toChange[i]);
            }
        }
        /// <summary>
        /// 收集本体和所有子级粒子的渲染器
        /// </summary>
        /// <returns>ParticleSystemRenderer 列表</returns>
        private List<ParticleSystemRenderer> CollectParticleRenderers()
        {
            List<GameObject> objects = CollectChildParticleObjects();
            if (!objects.Contains(thisParticleSystem.gameObject))
            {
                objects.Add(thisParticleSystem.gameObject);
            }
            List<ParticleSystemRenderer> renderers = new List<ParticleSystemRenderer>();
            for (int i = 0; i < objects.Count; i++)
            {
                ParticleSystemRenderer renderer = objects[i].GetComponent<ParticleSystemRenderer>();
                if (renderer != null && !renderers.Contains(renderer))
                {
                    renderers.Add(renderer);
                }
            }
            return renderers;
        }
        /// <summary>
        /// 调整本体和所有子级渲染器的 Order in Layer
        /// </summary>
        /// <param name="delta">+1 或 -1</param>
        private void ChangeSortingOrder(int delta)
        {
            List<ParticleSystemRenderer> renderers = CollectParticleRenderers();
            if (renderers.Count == 0)
            {
                return;
            }
            Undo.RecordObjects(renderers.ToArray(), delta > 0 ? "Order in Layer +1" : "Order in Layer -1");
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].sortingOrder += delta;
                EditorUtility.SetDirty(renderers[i]);
            }
        }
#endregion

    }
}

