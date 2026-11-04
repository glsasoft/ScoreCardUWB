using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Common.UI
{
    public class ContinuousValueIndicator : MonoBehaviour
    {
        public RectTransform indicator;

        public enum IndicateMode
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom
        }

        public IndicateMode indicateMode = IndicateMode.LeftToRight;
        
        public float offset
        {
            get => _offset;
            set
            {
                _offset = value;
                UpdateValue();
            }
        }
        
        [SerializeField, Range(0, 1)] private float _offset;
        
        public float value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateValue();
            }
        }
        
        [SerializeField, Range(0, 1)] private float _value;

        private void UpdateValue()
        {
            if (!indicator) return;

            if (_offset < 0) _offset = 0;
            if (_offset > 1) _offset = 1;
            
            if (_value < 0) _value = 0;
            if (_offset + _value > 1) _value = 1 - _offset;

            switch (indicateMode)
            {
                case IndicateMode.LeftToRight:
                    indicator.anchorMin = new Vector2(_offset, 0);
                    indicator.anchorMax = new Vector2(_offset + _value, 1);
                    break;
                case IndicateMode.RightToLeft:
                    indicator.anchorMin = new Vector2(1 - _offset - _value, 0);
                    indicator.anchorMax = new Vector2(1 - _offset, 1);
                    break;
                case IndicateMode.BottomToTop:
                    indicator.anchorMin = new Vector2(0, _offset);
                    indicator.anchorMax = new Vector2(1, _offset + _value);
                    break;
                case IndicateMode.TopToBottom:
                    indicator.anchorMin = new Vector2(0, 1 - _offset - _value);
                    indicator.anchorMax = new Vector2(1, 1 - _offset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(ContinuousValueIndicator))]
        private class ContinuousValueIndicatorEditor : Editor
        {
            private ContinuousValueIndicator indicator;

            private void OnEnable()
            {
                indicator = (ContinuousValueIndicator)target;
            }

            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();
                
                base.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck() && indicator.indicator)
                {
                    Undo.RecordObject(indicator.indicator, "Update Indicator");
                    indicator.UpdateValue();
                }
            }
        }
#endif
    }
}
