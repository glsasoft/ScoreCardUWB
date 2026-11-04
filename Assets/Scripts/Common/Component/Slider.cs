using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UESlider = UnityEngine.UI.Slider;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Common.UI
{
    /// <summary>
    /// 带增减按钮的滑条控件
    /// </summary>
    public class Slider : MonoBehaviour
    {
        public UESlider slider;
        public TMP_Text text;
        public Button decreaseButton;
        public Button increaseButton;

        /// <summary>
        /// 滑条划到最小时的值
        /// </summary>
        public float minValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                UpdateConfig();
            }
        }

        /// <summary>
        /// 滑条划到最大时的值
        /// </summary>
        public float maxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                UpdateConfig();
            }
        }

        /// <summary>
        /// 滑条的段数<br/>
        /// 例如将 0 - 100 的滑条分 20 段的话，那么滑条只能停留在 0, 5, 10, ... 的位置上
        /// </summary>
        public int stepCount
        {
            get => _stepCount;
            set
            {
                _stepCount = value;
                UpdateConfig();
            }
        }
        
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue = 100;
        [SerializeField] private int _stepCount = 10;

        public enum SliderBarDisplayMode
        {
            [InspectorName("均匀分布")] EvenDistribution,
            [InspectorName("实际数值")] ActualValue
        }

        /// <summary>
        /// 滑条长度的显示模式
        /// </summary>
        public SliderBarDisplayMode sliderBarDisplayMode
        {
            get => _sliderBarDisplayMode;
            set
            {
                _sliderBarDisplayMode = value;
                UpdateConfig();
            }
        }
        
        [SerializeField] private SliderBarDisplayMode _sliderBarDisplayMode = SliderBarDisplayMode.EvenDistribution;
        
        /// <summary>
        /// 滑条上文本的格式
        /// </summary>
        public string textFormat
        {
            get => _textFormat;
            set
            {
                _textFormat = value;
                UpdateConfig();
            }
        }
        
        /// <summary>
        /// 滑条划到最左侧时是否有数字外的额外档位
        /// </summary>
        public bool hasBottom
        {
            get => _hasBottom;
            set
            {
                _hasBottom = value;
                UpdateConfig();
            }
        }
        
        /// <summary>
        /// 滑条划到最左侧额外档位时显示的文本
        /// </summary>
        public string bottomText
        {
            get => _bottomText;
            set
            {
                _bottomText = value;
                UpdateConfig();
            }
        }
        
        /// <summary>
        /// 滑条划到最右侧时是否有数字外的额外档位
        /// </summary>
        public bool hasTop
        {
            get => _hasTop;
            set
            {
                _hasTop = value;
                UpdateConfig();
            }
        }
        
        /// <summary>
        /// 滑条划到最右侧额外档位时显示的文本
        /// </summary>
        public string topText
        {
            get => _topText;
            set
            {
                _topText = value;
                UpdateConfig();
            }
        }
        
        private float stepLength => (_maxValue - _minValue) / _stepCount;
        
        [SerializeField] private string _textFormat = "{0}";
        [SerializeField, HideInInspector] private bool _hasBottom;
        [SerializeField, HideInInspector] private string _bottomText = "Off";
        [SerializeField, HideInInspector] private bool _hasTop;
        [SerializeField, HideInInspector] private string _topText = "Infinity";

        /// <summary>
        /// 滑条目前选中的值
        /// </summary>
        public float value
        {
            get => slider.value;
            set
            {
                slider.value = value;
                UpdateValue();
            }
        }

        private float originValue;

        /// <summary>
        /// 滑条是否位于最左侧的额外档位
        /// </summary>
        public bool isBottom => slider.value < _minValue - stepLength * 0.5f;
        
        /// <summary>
        /// 滑条是否位于最右侧的额外档位
        /// </summary>
        public bool isTop => slider.value > _maxValue + stepLength * 0.5f;
        
        /// <summary>
        /// 滑条被点击或发生变更事件，参数为滑条新选中的值
        /// </summary>
        public event Action<float> OnValueChangedEvent;

        private void Awake()
        {
            if (slider) slider.onValueChanged.AddListener(_ => UpdateValue());
            if (decreaseButton) decreaseButton.onClick.AddListener(Decrease);
            if (increaseButton) increaseButton.onClick.AddListener(Increase);
            originValue = value;
            UpdateConfig();
        }

        /// <summary>
        /// 将滑条设置到最左侧的额外档位
        /// </summary>
        public void SetValueToBottom()
        {
            switch (sliderBarDisplayMode)
            {
                case SliderBarDisplayMode.EvenDistribution:
                    slider.value = _minValue - stepLength;
                    break;
                case SliderBarDisplayMode.ActualValue:
                    slider.value = 0;
                    break;
            }
            UpdateValue();
        }

        /// <summary>
        /// 将滑条设置到最右侧的额外档位
        /// </summary>
        public void SetValueToTop()
        {
            slider.value = _maxValue + stepLength;
            UpdateValue();
        }

        private void Decrease()
        {
            switch (sliderBarDisplayMode)
            {
                case SliderBarDisplayMode.EvenDistribution:
                    slider.value -= stepLength;
                    break;
                case SliderBarDisplayMode.ActualValue:
                    if (_hasBottom && slider.value < _minValue + stepLength * 0.5f) slider.value = 0;
                    else slider.value -= stepLength;
                    break;
            }
            UpdateValue();
        }
        
        private void Increase()
        {
            switch (sliderBarDisplayMode)
            {
                case SliderBarDisplayMode.EvenDistribution:
                    slider.value += stepLength;
                    break;
                case SliderBarDisplayMode.ActualValue:
                    if (_hasBottom && slider.value < _minValue / 2) slider.value = _minValue;
                    else slider.value += stepLength;
                    break;
            }
            UpdateValue();
        }

        private void UpdateConfig()
        {
            if (!slider) return;

            switch (sliderBarDisplayMode)
            {
                case SliderBarDisplayMode.EvenDistribution:
                    slider.minValue = _hasBottom ? _minValue - stepLength : _minValue;
                    slider.maxValue = _hasTop ? _maxValue + stepLength : _maxValue;
                    break;
                case SliderBarDisplayMode.ActualValue:
                    if (_minValue < 0) _minValue = 0;
                    if (_maxValue < 0) _maxValue = 0;
                    slider.minValue = 0;
                    slider.maxValue = _hasTop ? _maxValue + stepLength : _maxValue;
                    break;
            }
            
            slider.wholeNumbers = false;

            UpdateValue();
        }

        private void UpdateValue()
        {
            float _stepLength = stepLength;

            switch (sliderBarDisplayMode)
            {
                case SliderBarDisplayMode.EvenDistribution:
                    if (!Mathf.Approximately(_minValue, _maxValue))
                        slider.value = Mathf.Round((slider.value - _minValue) / _stepLength) * _stepLength + _minValue;
                    break;
                case SliderBarDisplayMode.ActualValue:
                    if (!Mathf.Approximately(_minValue, _maxValue))
                        slider.value = Mathf.Round((slider.value - _minValue) / _stepLength) * _stepLength + _minValue;
                    if (slider.value < _minValue)
                        slider.value = slider.value < _minValue / 2 && _hasBottom ? 0 : _minValue;
                    break;
            }
            
            if (text)
            {
                if (isBottom) text.text = _bottomText;
                else if (isTop) text.text = _topText;
                else text.text = string.Format(_textFormat, value);
            }

            if (decreaseButton)
                decreaseButton.interactable =
                    slider.value > (_hasBottom ? _minValue / 2 : _minValue + _stepLength * 0.5f);
            if (increaseButton)
                increaseButton.interactable =
                    slider.value < _maxValue + (_hasTop ? _stepLength * 0.5f : _stepLength * -0.5f);
            
            if (!Mathf.Approximately(value, originValue))
            {
                OnValueChangedEvent?.Invoke(value);
                originValue = value;
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Slider))]
        private class SliderEditor : Editor
        {
            private Slider slider;
            
            private void OnEnable()
            {
                slider = (Slider)target;
            }

            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();
                
                base.OnInspectorGUI();
                
                serializedObject.Update();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Bottom");
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_hasBottom)), GUIContent.none,
                    GUILayout.Width(EditorGUIUtility.singleLineHeight));
                EditorGUI.BeginDisabledGroup(!slider.hasBottom);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_bottomText)), GUIContent.none);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Top");
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_hasTop)), GUIContent.none,
                    GUILayout.Width(EditorGUIUtility.singleLineHeight));
                EditorGUI.BeginDisabledGroup(!slider.hasTop);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_topText)), GUIContent.none);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                serializedObject.ApplyModifiedProperties();
                
                if (EditorGUI.EndChangeCheck())
                {
                    if (slider.slider) Undo.RegisterCompleteObjectUndo(slider.slider, "Slider Value Changed");
                    if (slider.text) Undo.RegisterCompleteObjectUndo(slider.text, "Slider Text Changed");
                    slider.UpdateConfig();
                }
                
                if (slider.slider)
                {
                    EditorGUI.BeginChangeCheck();
                    Rect position = EditorGUILayout.GetControlRect();
                    SerializedObject serializedSlider = new(slider.slider);
                    GUIContent label = EditorGUI.BeginProperty(position, new GUIContent("Value"),
                        serializedSlider.FindProperty("m_Value"));
                    float value = EditorGUI.Slider(position, label, slider.slider.value,
                        slider.slider.minValue, slider.slider.maxValue);
                    EditorGUI.EndProperty();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(slider.slider, "Slider Value Changed");
                        if (slider.text) Undo.RegisterCompleteObjectUndo(slider.text, "Slider Text Changed");
                        slider.slider.value = value;
                        slider.UpdateConfig();
                        Repaint();
                    }
                }
            }
        }
#endif
    }
}
