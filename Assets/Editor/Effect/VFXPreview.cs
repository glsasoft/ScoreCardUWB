using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FFTools
{
    namespace FXTools
    {
        public class VFXPreview : EditorWindow
        {
            // 选中物体
            GameObject m_Obj;

            // 播放速度
            float m_Speed = 1f;

            // 是否在预览播放状态
            bool m_Playing;

            // 是否暂停
            bool m_Pausing;

            // 粒子 及其属性
            ParticleSystem[] m_Pss = new ParticleSystem[0];
            Dictionary<ParticleSystem, float> activeTime = new Dictionary<ParticleSystem, float>();
            List<ParticleSystem> randomPs = new List<ParticleSystem>();


            // 所有动画
            Animator[] m_Anims;

            // 当前运行时间
            float m_RunningTime;

            // 上一次系统时间
            double m_PreviousTime;

            // 滑动杆的当前时间
            private float m_CurTime;

            // 是否已经烘焙
            private bool m_HasBake;

            //是否锁定选择
            private bool m_Lock;

            //帧率
            private float frameRate = 30f;

            //是否开启当前特效预览
            private bool m_PreviewCurrent;

            //是否开启资源预览
            private bool m_PreviewAsset;

            //当前预览资源
            private GameObject m_PreAsset;

            // 滑动杆总长度
            private float kDuration = 30f;

            //当前被实例化资源数量
            private int assetNum;

            //各个粒子系统的粒子数
            private List<int> psCounts = new List<int>();

            //各个mesh的三角数
            private List<int> meshTrians = new List<int>();

            //最大粒子数
            private int psCount = 0;

            // 最大模型面数
            private int meshTrian = 0;

            //------------------------------------------------
            //复制材质
            //复制材质的保存路径
            private string savePath = "Assets/";

            //复制保存的材质
            private Material newMat;
            void OnEnable()
            {
                m_PreviousTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += inspectorUpdate;
            }

            void OnDisable()
            {
                EditorApplication.update -= inspectorUpdate;
                Reset();
            }

            private void OnDestroy()
            {
                Reset();
                OnSelectionChange();
            }

            [MenuItem("Tools/FFTools/VFX/VFXPreview", false, -10000)]
            static void PreviewWindow()
            {
                GetWindow<VFXPreview>("特效预览");
            }

            private void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(10);
                m_PreviewAsset = GUILayout.Toggle(m_PreviewAsset, "开启资源预览", EditorStyles.toolbarButton, GUILayout.Height(20));
                OnSelectionChange();

                if (m_PreviewAsset && EditorGUI.EndChangeCheck())
                {
                    psCount = 0;
                    meshTrian = 0;
                    if (!m_PreviewAsset)
                    {
                        if (m_Obj != null && assetNum > 0)
                        {
                            DestroyImmediate(m_Obj, false);
                            m_HasBake = false;
                        }
                    }
                    OnSelectionChange();
                }

                if (m_PreviewAsset) return;

                //第一行
                GUILayout.BeginHorizontal();
                GUILayout.Label("当前选择:", GUILayout.Width(60));
                m_Obj = EditorGUILayout.ObjectField(m_Obj, typeof(GameObject), true) as GameObject;
                m_Lock = GUILayout.Toggle(m_Lock, "锁定选择", EditorStyles.toolbarButton);


                if (!m_Lock && !m_PreviewAsset)
                {
                    if (m_Obj != Selection.activeObject as GameObject)
                    {
                        m_Obj = Selection.activeObject as GameObject;

                        if (m_Obj)
                        {
                            GetPsAndAnim();
                        }
                        else
                        {
                            Array.Resize(ref m_Pss, 0);
                            Array.Resize(ref m_Anims, 0);
                        }
                    }
                }

                GUILayout.EndHorizontal();

                if (!m_Obj) return;

                if (!Application.isPlaying && m_Pss.Length + m_Anims.Length != 0)
                {
                    //第三行
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("进度条总长度(s)：", GUILayout.Width(100));
                    kDuration = EditorGUILayout.FloatField(/*"进度条总长度(s) ： ", */kDuration);
                    GUILayout.Label("帧率：", GUILayout.Width(40));
                    frameRate = EditorGUILayout.FloatField(frameRate);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);


                    if (!m_Playing)
                    {
                        //第二行
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("进度条：", GUILayout.Width(60));
                        m_CurTime = EditorGUILayout.Slider(/*"进度条 : ", */m_CurTime, 0f, kDuration);
                        GUILayout.EndHorizontal();
                    }


                    //第四行
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("播放速度：", GUILayout.Width(60));
                    m_Speed = EditorGUILayout.Slider(/*"播放速度 : ", */m_Speed, 0f, 10f);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("预加载", GUILayout.Height(60)))
                    {
                        m_HasBake = false;

                        Bake();
                    }

                    if (m_Playing && !m_Pausing)
                    {
                        if (GUILayout.Button("Pause", GUILayout.Height(60)))
                        {
                            Pause();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Play", GUILayout.Height(60)))
                        {
                            Play();
                        }
                    }

                    if (GUILayout.Button("Stop/手动预览", GUILayout.Height(60)))
                    {
                        Stop();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField("当前时间：" + (m_CurTime).ToString());
                    EditorGUILayout.LabelField("当前帧数：" + ((int)(m_CurTime * frameRate)).ToString());
                    EditorGUILayout.LabelField("当前特效的最大粒子数： " + psCount.ToString());
                    EditorGUILayout.LabelField("当前特效大概的三角面数： " + ((meshTrian / 3) + (psCount * 2)).ToString());
                    EditorGUILayout.EndVertical();

                    if ((m_HasBake && m_PreviewCurrent) || m_PreviewAsset)
                    {
                        manualUpdate();
                    }
                }

                //复制材质
                GUILayout.BeginVertical("box");

                GUILayout.Space(4);
                EditorGUIUtility.labelWidth = 60;
                GUILayout.BeginHorizontal();
                GUILayout.Label("快速复制材质");

                if (m_Obj == null) GUI.enabled = false;
                if (GUILayout.Button("Duplicate", GUILayout.Width(80)))
                {
                    DuplicateMat();
                }
                if (GUILayout.Button("SaveAs", GUILayout.Width(80)))
                {
                    SaveAsNewMaterial();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (newMat != null)
                {
                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(new GUIContent("SavedMat"), newMat, typeof(GameObject), true, GUILayout.Width(160));
                    EditorGUILayout.TextField(AssetDatabase.GetAssetPath(newMat));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
                GUILayout.Space(4);
                GUILayout.EndVertical();

            }

            void OnSelectionChange()
            {
                if (m_PreviewAsset)
                {
                    if (m_PreAsset != Selection.activeGameObject)
                    {
                        if (m_Obj && (m_Obj.name.Contains("Clone")))
                        {
                            DestroyImmediate(m_Obj, false);
                            m_PreAsset = null;
                        }

                        if (Selection.activeGameObject)
                        {
                            m_PreAsset = Selection.activeGameObject;
                            InstantiateAsset();
                        }
                    }
                }
            }

            void InstantiateAsset()
            {
                if (m_PreAsset != null && AssetDatabase.Contains(m_PreAsset) && m_PreAsset.transform.root == m_PreAsset.transform)
                {
                    var go = Instantiate(m_PreAsset, new Vector3(0, 0, 0), m_PreAsset.transform.localRotation);
                    m_Obj = go as GameObject;
                    assetNum += 1;
                    if (m_Obj)
                    {
                        GetPsAndAnim();
                    }
                    else
                    {
                        Array.Resize(ref m_Pss, 0);
                        Array.Resize(ref m_Anims, 0);
                    }
                    Bake();
                    Play();
                }
            }

            void Play()
            {
                m_PreviewCurrent = false;
                GetPsAndAnim();
                if (m_Pausing)
                {
                    m_Pausing = false;
                    m_PreviousTime = EditorApplication.timeSinceStartup;
                }
                else
                {
                    m_RunningTime = 0f;
                    m_Playing = true;
                }
            }

            void Pause()
            {
                if (m_Playing)
                {
                    m_Pausing = true;
                }
            }

            void Stop()
            {
                Reset();
                m_Playing = false;
                m_CurTime = 0f;
                m_RunningTime = 0f;
                m_PreviewCurrent = true;
            }

            void Bake()
            {
                m_PreviewCurrent = false;

                foreach (var anim in m_Anims)
                {
                    if (!anim.gameObject.activeInHierarchy)
                    {
                        anim.gameObject.SetActive(true);
                        RecordAnimator(anim);
                        anim.gameObject.SetActive(false);
                    }
                    else
                    {
                        RecordAnimator(anim);
                    }
                }

                foreach (var ps in m_Obj.GetComponentsInChildren<ParticleSystem>(true))
                {
                    if (ps.useAutoRandomSeed)
                    {
                        randomPs.Add(ps);
                        ps.useAutoRandomSeed = false;
                    }
                }

                m_HasBake = true;
                Stop();
            }

            void RecordAnimator(Animator anim)
            {
                int frameCount = (int)((kDuration * frameRate) + 2);
                anim.Rebind();
                anim.StopPlayback();
                anim.recorderStartTime = 0;

                // 开始记录指定的帧数
                anim.StartRecording(frameCount);

                for (var i = 0; i < frameCount - 1; i++)
                {
                    // 记录每一帧

                    anim.Update(1.0f / frameRate);
                }
                // 完成记录
                anim.StopRecording();

                // 开启回放模式
                anim.StartPlayback();
            }

            void update(float delta)
            {
                GetPsAndAnim();
                CalculateEffectData();

                if (m_RunningTime >= kDuration)
                {
                    m_Playing = false;
                    return;
                }

                foreach (var ps in this.m_Pss)
                {
                    if (m_RunningTime <= 0)
                    {
                        ps.Simulate(delta * m_Speed, true, true);
                    }
                    else
                    {
                        ps.Simulate(delta * m_Speed, true, false);
                    }

                    //ps.Simulate(/*m_RunningTime - activeTime[ps]*/delta, true,true);
                }

                foreach (var anim in m_Anims)
                {
                    if (anim.gameObject.activeInHierarchy)
                    {
                        anim.playbackTime = m_RunningTime;
                        anim.Update(0);
                    }
                }
                //SceneView.RepaintAll();
                Repaint();

                m_CurTime = m_RunningTime;
            }

            void inspectorUpdate()
            {
                if (m_Pausing)
                {
                    return;
                }
                var delta = EditorApplication.timeSinceStartup - m_PreviousTime;
                m_PreviousTime = EditorApplication.timeSinceStartup;

                if (!Application.isPlaying && m_Playing && m_Obj)
                {
                    m_RunningTime = Mathf.Clamp(m_RunningTime + (float)delta * m_Speed, 0f, kDuration);
                    update((float)delta);
                }
            }

            void manualUpdate()
            {
                GetPsAndAnim();
                if (!m_Playing && m_HasBake)
                {
                    float time;
                    foreach (var ps in m_Pss)
                    {
                        if (activeTime.ContainsKey(ps))
                        {
                            time = activeTime[ps];
                        }
                        else
                        {
                            time = 0;
                        }
                        ps.Simulate(m_CurTime - time, false);
                    }

                    foreach (var anim in m_Anims)
                    {
                        anim.playbackTime = m_CurTime;
                        anim.Update(0);
                    }
                    SceneView.RepaintAll();
                }
            }

            void CalculateEffectData()
            {
                meshTrians.Clear();
                psCounts.Clear();
                var meshs = m_Obj.GetComponentsInChildren<MeshFilter>();
                foreach (var mesh in meshs)
                {
                    if (mesh.sharedMesh != null)
                    {
                        meshTrians.Add(mesh.sharedMesh.triangles.Length);
                    }
                }
                foreach (var ps in m_Pss)
                {
                    var psCount = ps.GetParticles(new ParticleSystem.Particle[ps.main.maxParticles]);
                    if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Mesh && ps.GetComponent<ParticleSystemRenderer>().mesh != null)
                    {
                        meshTrians.Add(ps.GetComponent<ParticleSystemRenderer>().mesh.triangles.Length * psCount);
                    }
                    var psNum = ps.GetParticles(new ParticleSystem.Particle[ps.main.maxParticles]);
                    psCounts.Add(psNum);
                }
                var skinMeshs = m_Obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var mesh in skinMeshs)
                {
                    if (mesh.sharedMesh != null)
                    {
                        meshTrians.Add(mesh.sharedMesh.triangles.Length);
                    }
                }

                if (meshTrians.Count > 0)
                {
                    var psNum = 0;
                    foreach (var item in meshTrians)
                    {
                        psNum = psNum + item;
                    }
                    if (meshTrian < psNum)
                    {
                        meshTrian = psNum;
                    }
                }


                if (psCounts.Count > 0)
                {
                    var psNum = 0;
                    foreach (var item in psCounts)
                    {
                        psNum = psNum + item;
                    }
                    if (psCount < psNum)
                    {
                        psCount = psNum;
                    }
                }
            }

            void GetPsAndAnim()
            {
                ParticleSystem[] pssTemp = m_Obj.GetComponentsInChildren<ParticleSystem>();
                ParticleSystem[] pssAdd = pssTemp.Except(m_Pss).ToArray();
                foreach (var ps in pssAdd)
                {
                    if (activeTime.ContainsKey(ps))
                    {
                        activeTime[ps] = m_RunningTime;
                    }
                    else
                    {
                        activeTime.Add(ps, m_RunningTime);
                    }
                }
                m_Pss = m_Obj.GetComponentsInChildren<ParticleSystem>();
                m_Anims = m_Obj.GetComponentsInChildren<Animator>(true);
            }

            void Reset()
            {
                foreach (var ps in randomPs)
                {
                    ps.useAutoRandomSeed = true;
                }
                randomPs.Clear();
            }

            //复制材质
            void SaveAsNewMaterial()
            {
                GameObject go = Selection.activeGameObject;
                if (go == null) return;
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer == null) return;
                List<Material> mat = new List<Material>();
                renderer.GetSharedMaterials(mat);
                if (mat[0] == null || mat[0].Equals(null)) return;
                string matPath = AssetDatabase.GetAssetPath(mat[0]);

                savePath = EditorUtility.SaveFilePanelInProject("Save as", mat[0].name, "mat", string.Empty, savePath);
                if (!string.IsNullOrEmpty(savePath))
                {
                    newMat = new Material(mat[0]);
                    AssetDatabase.CreateAsset(newMat, savePath);
                    ParticleSystemRenderer pRenderer = renderer.GetComponent<ParticleSystemRenderer>();
                    if (pRenderer != null)
                    {
                        pRenderer.trailMaterial = null;
                    }
                    renderer.material = newMat;
                }
            }
            void DuplicateMat()
            {
                GameObject go = Selection.activeGameObject;
                if (go == null) return;
                Renderer renderer = go.GetComponent<Renderer>();
                if (renderer == null) return;
                List<Material> mat = new List<Material>();
                renderer.GetSharedMaterials(mat);
                if (mat[0] == null || mat[0].Equals(null)) return;
                if (mat[0].name == "Default-ParticleSystem" || mat[0].name == "ParticlesUnlit") return;
                string matPath = AssetDatabase.GetAssetPath(mat[0]);
                string parentPath = matPath.Substring(0, matPath.LastIndexOf(mat[0].name));

                bool isEndWithNum = true;
                int spaceIndex = matPath.LastIndexOf("_");
                string matPathWithNoNum;
                if (spaceIndex < 0)
                {
                    isEndWithNum = false;
                }
                else
                {
                    string numString = matPath.Substring(matPath.LastIndexOf("_")+1, matPath.LastIndexOf(".") - matPath.LastIndexOf("_")-1);
                    Debug.Log(numString);
                    try
                    {
                        int num = int.Parse(numString);
                    }
                    catch
                    {
                        isEndWithNum = false;
                    }
                }

                if (isEndWithNum)
                {
                    matPathWithNoNum = matPath.Substring(0, matPath.LastIndexOf("_"));
                }
                else
                {
                    matPathWithNoNum = parentPath + mat[0].name;
                }


                string newSavePath = "null";
                for (int i = 1; i < 111; i++)
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(matPathWithNoNum + "_" + i.ToString().PadLeft(3, '0') + ".mat", typeof(Material));
                    if (obj != null)
                    {
                        continue;
                    }
                    else
                    {
                        newSavePath = matPathWithNoNum + "_" + i.ToString().PadLeft(3,'0');
                        break;
                    }
                }

                if (newSavePath != "null")
                {
                    newMat = new Material(mat[0]);
                    //FFTools.CommonTools.MaterialPropertyCleaner.RemoveUnusedProperties(newMat);
                    AssetDatabase.CreateAsset(newMat, newSavePath + ".mat");
                    ParticleSystemRenderer pRenderer = renderer.GetComponent<ParticleSystemRenderer>();
                    if (pRenderer != null)
                    {
                        pRenderer.trailMaterial = null;
                    }
                    renderer.material = newMat;
                }
            }
        }
    }
}