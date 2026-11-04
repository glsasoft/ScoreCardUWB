using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Maths
{
    using TSample = CircularRegressor.TSample;
    
    public class CircularRegressorComponent : MonoBehaviour
    {
        public CircularRegressor iterator;
        
        #if UNITY_EDITOR
        [CustomEditor(typeof(CircularRegressorComponent))]
        private class CircularRegressorEditor : Editor
        {
            private struct Domain
            {
                public float min, max;
                public float length => max - min;
                public float center => (min + max) / 2;
                public float random => Random.Range(min, max);
                public float Lerp(float t) => Mathf.Lerp(min, max, t);
                public bool Contains(float x) => min <= x && x <= max;
            }
            
            private class EditorBuffer
            {
                public bool foldDebug = true;
                public Domain domain = new() { min = -2, max = 2 };
                public readonly Vector2[] curveSamples = new Vector2[41];
                public int sampleCount = 50;
                public float noiseIntensity = 0.1f;
                public float arcAngle = Mathf.PI * 2;
            }

            private static readonly Dictionary<CircularRegressorComponent, EditorBuffer> buffers = new();
            
            private CircularRegressorComponent component;
            private CircularRegressor iterator => component.iterator;
            private EditorBuffer buffer;
            
            private void OnEnable()
            {
                component = (CircularRegressorComponent)target;
                
                if (!buffers.TryGetValue(component, out buffer))
                {
                    buffer = new EditorBuffer();
                    buffers.Add(component, buffer);
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                buffer.foldDebug = EditorGUILayout.Foldout(buffer.foldDebug, "Debug");
                if (buffer.foldDebug)
                {
                    EditorGUI.indentLevel++;

                    // 计算图表定义域和值域
                    Domain domain = buffer.domain;
                    Domain range = new() { min = iterator.originY, max = iterator.originY };

                    for (int i = 0; i < buffer.curveSamples.Length; i++)
                    {
                        float rad = (float)i / (buffer.curveSamples.Length - 1) * Mathf.PI * 2;
                        float x = iterator.originX + iterator.radius * Mathf.Cos(rad);
                        float y = iterator.originY + iterator.radius * Mathf.Sin(rad);
                        buffer.curveSamples[i] = new Vector2(x, y);
                        if (domain.min > x) domain.min = x;
                        if (domain.max < x) domain.max = x;
                        if (range.min > y) range.min = y;
                        if (range.max < y) range.max = y;
                    }

                    foreach (var sample in iterator.samples)
                    {
                        if (domain.min > sample.x) domain.min = sample.x;
                        if (domain.max < sample.x) domain.max = sample.x;
                        if (range.min > sample.y) range.min = sample.y;
                        if (range.max < sample.y) range.max = sample.y;
                    }

                    // 获取图表区域
                    Rect figureRect = EditorGUILayout.GetControlRect(false, 0.6f * EditorGUIUtility.currentViewWidth);
                    figureRect.y += EditorGUIUtility.standardVerticalSpacing / 2;
                    figureRect.height -= EditorGUIUtility.standardVerticalSpacing;
                    
                    GUI.Box(figureRect, GUIContent.none);
                
                    Vector2 dataCenter = new(domain.center, range.center), uiCenter = figureRect.center;
                    float horizontalScaleRatio = (figureRect.width - 40) / domain.length;
                    float verticalScaleRatio = (figureRect.height - 40) / range.length;
                    float MapX(float x) => (x - dataCenter.x) * horizontalScaleRatio + uiCenter.x;
                    float MapY(float y) => -(y - dataCenter.y) * verticalScaleRatio + uiCenter.y;
                    Vector2 MapV(Vector2 vec) => new(MapX(vec.x), MapY(vec.y));
                    Vector2 MapS(TSample vec) => new(MapX(vec.x), MapY(vec.y));
                
                    // 绘制图表坐标轴
                    Handles.color = Color.gray;
                    if (range.Contains(0))
                        Handles.DrawLine(new Vector3(figureRect.xMin, MapY(0)), new Vector3(figureRect.xMax, MapY(0)));
                    if (buffer.domain.Contains(0))
                        Handles.DrawLine(new Vector3(MapX(0), figureRect.yMin), new Vector3(MapX(0), figureRect.yMax));
                
                    // 绘制图表曲线
                    Handles.color = Color.green;
                    for (int i = 0; i < buffer.curveSamples.Length - 1; i++)
                        Handles.DrawLine(MapV(buffer.curveSamples[i]), MapV(buffer.curveSamples[i + 1]));
                    
                    // 绘制图表样本点
                    Handles.color = Color.red;
                    foreach (var sample in iterator.samples)
                    {
                        const float size = 5;
                        Vector3 pos = MapS(sample);
                        Handles.DrawLine(pos + size * Vector3.left, pos + size * Vector3.right);
                        Handles.DrawLine(pos + size * Vector3.down, pos + size * Vector3.up);
                    }
                    
                    // 绘制图表样本点
                    // Handles.color = Color.yellow;
                    // foreach (Vector2 sample in buffer.featuredSamples)
                    // {
                    //     const float size = 5;
                    //     Vector3 pos = Map(sample);
                    //     Handles.DrawLine(pos + size * (Vector3.left + Vector3.down).normalized,
                    //         pos + size * (Vector3.right + Vector3.up).normalized);
                    //     Handles.DrawLine(pos + size * (Vector3.left + Vector3.up).normalized,
                    //         pos + size * (Vector3.right + Vector3.down).normalized);
                    // }
                    
                    // 绘制定义域输入框
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Domain");
                    buffer.domain.min = EditorGUILayout.FloatField(buffer.domain.min);
                    EditorGUILayout.LabelField("<color=white>-</color>",
                        new GUIStyle { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(15));
                    buffer.domain.max = EditorGUILayout.FloatField(buffer.domain.max);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.LabelField("<color=white>Parameters</color>", new GUIStyle { fontStyle = FontStyle.Bold });
                    
                    // 清除参数
                    if (GUILayout.Button("Clean Parameters"))
                    {
                        Undo.RecordObject(component, "Parameter Cleaned");
                        iterator.originX = 0;
                        iterator.originY = 0;
                    }
                    
                    // 清除参数
                    if (GUILayout.Button("Generate Random Parameters"))
                    {
                        Undo.RecordObject(component, "Parameter Generated");
                        iterator.originX = Random.Range(-1f, 1f);
                        iterator.originY = Random.Range(-1f, 1f);
                        iterator.radius = Random.Range(1f, 2f);
                    }
                    
                    EditorGUILayout.LabelField("<color=white>Samples</color>", new GUIStyle { fontStyle = FontStyle.Bold });

                    // 绘制样本生成器参数输入框
                    buffer.sampleCount = EditorGUILayout.IntField("Sample Count", buffer.sampleCount);
                    buffer.noiseIntensity = EditorGUILayout.FloatField("Noise Intensity", buffer.noiseIntensity);
                    buffer.arcAngle = EditorGUILayout.Slider("Arc Angle", buffer.arcAngle, 0, Mathf.PI * 2);

                    // 样本生成器
                    if (GUILayout.Button("Generate Samples"))
                    {
                        Undo.RecordObject(component, "Samples Generated");
                        
                        while (iterator.samples.Count < buffer.sampleCount) iterator.samples.Add(new TSample());
                        while (iterator.samples.Count > buffer.sampleCount)
                            iterator.samples.RemoveAt(iterator.samples.Count - 1);

                        float arcBegin = Random.value * Mathf.PI * 2;
                        
                        for (int i = 0; i < iterator.samples.Count; i++)
                        {
                            float r = iterator.radius + (2 * Random.value - 1) * buffer.noiseIntensity;
                            float a = Random.value * buffer.arcAngle + arcBegin;
                            float x = iterator.originX + r * Mathf.Cos(a);
                            float y = iterator.originY + r * Mathf.Sin(a);
                            iterator.samples[i] = new TSample { x = x, y = y };
                        }
                    }
                    
                    EditorGUILayout.LabelField("<color=white>Calculation</color>", new GUIStyle { fontStyle = FontStyle.Bold });
                    
                    // 预测参数
                    if (GUILayout.Button("Predict Parameter"))
                    {
                        Undo.RecordObject(component, "Parameter Predicted");
                        iterator.Predict();
                        Debug.Log($"Lose: {iterator.Lose()}");
                    }
                    
                    // 迭代
                    if (GUILayout.Button("Iterate 1 Time") || GUILayout.RepeatButton("Press To Iterate"))
                    {
                        Undo.RecordObject(component, "Parameter Iterated");
                        iterator.Iterate();
                        Debug.Log($"Lose: {iterator.Lose()}");
                    }
                    
                    // 迭代
                    if (GUILayout.Button("Auto Calculate"))
                    {
                        Undo.RecordObject(component, "Parameter Calculated");
                        Debug.Log(iterator.AutoCalculate()
                            ? $"Succeeded, Lose: {iterator.Lose()}"
                            : $"Failed, Lose: {iterator.Lose()}");
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endif
    }
}