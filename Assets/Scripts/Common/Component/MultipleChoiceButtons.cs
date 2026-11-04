using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;
using LetsPiu.Base;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Common.UI
{
    /// <summary>
    /// 单选按钮组控件
    /// </summary>
    [RequireComponent(typeof(ToggleGroup))]
    public class MultipleChoiceButtons : MonoBehaviour
    {
        /// <summary>
        /// 单选按钮模板
        /// </summary>
        public Toggle choiceTemplate;

        /// <summary>
        /// 选项数
        /// </summary>
        public int choiceCount
        {
            get => _choiceCount;
            set => UpdateChoiceCount(value);
        }

        [SerializeField] private int _choiceCount;

        public IReadOnlyList<Toggle> choices => _choices;

        [SerializeField] private List<Toggle> _choices = new();

        /// <summary>
        /// 目前选中的选项的索引
        /// </summary>
        public int choiceIndex
        {
            get => _choiceIndex;
            set => UpdateChoiceIndex(value);
        }
        public TMP_Text[] choiceTexts => _choiceTexts;
        [SerializeField] private TMP_Text[] _choiceTexts;

        public Image[] choiceImages => _choiceImages;
        [SerializeField] private Image[] _choiceImages;

        [SerializeField] private int _choiceIndex = 0;

        private ToggleGroup toggleGroup;

        /// <summary>
        /// 选中选项被点击或发生变更事件，参数为新选中的选项的索引
        /// </summary>
        public event Action<int> OnChoiceChangedEvent;

        /// <summary>
        /// 设置一个默认的切换音效（菜单音效）
        /// </summary>
        public AudioDefine audioDefine=AudioDefine.UI_MenuSwitch;

        private void Awake()
        {
            toggleGroup = GetComponent<ToggleGroup>();

            if (_choices.Count >= 1)
            {
                // 如果是第一个选项，设置为选中状态
                _choices[0].isOn = true;
            }
            for (int i = 0; i < _choiceCount; i++)
            {
                var index = i;
                choices[i].onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnChoiceChangedEvent?.Invoke(index);_choiceIndex = index; AudioPlayModule.S.PlayUIAudio(audioDefine);

                });
            }

            UpdateChoiceTexts();
        }

        private void Start()
        {
            choices[choiceIndex].isOn = true;
        }

        private void UpdateChoiceCount(int count)
        {
            if (count < 1) count = 1;
            if (_choiceCount == count) return;
            if (!choiceTemplate) return;

            _choiceCount = count;

            while (_choiceCount > _choices.Count)
            {
                Toggle choice = Instantiate(choiceTemplate, transform);
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(choice.gameObject, "Create Choice");
#endif
                _choices.Add(choice);
                choice.name = $"Choice {_choices.Count}";
                choice.gameObject.SetActive(true);
                choice.group = toggleGroup;
                // Add by Jia Ye: 补充change事件
                choice.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnChoiceChangedEvent?.Invoke(_choices.Count-1);
                });
            }

            while (_choiceCount < _choices.Count)
            {
                Toggle button = _choices[^1];
                _choices.RemoveAt(_choices.Count - 1);
                if (button)
                {
                    if (Application.isPlaying) Destroy(button.gameObject);
                    else
                    {
#if UNITY_EDITOR
                        Undo.DestroyObjectImmediate(button.gameObject);
#else
                                DestroyImmediate(button.gameObject);
#endif
                    }
                }
            }

            UpdateChoiceIndex(_choiceIndex);
            UpdateChoiceTexts();
        }

        private void UpdateChoiceIndex(int index)
        {
            if (index < 0 || index >= _choiceCount) index = 0;
            if (_choiceIndex == index) return;
            _choiceIndex = index;

            if (Application.isPlaying) _choices[_choiceIndex].isOn = true;
            else
            {
                for (int i = 0; i < _choiceCount; i++)
                    choices[i].isOn = i == _choiceIndex;
            }
        }

        private void UpdateChoiceTexts()
        {
            _choiceTexts = new TMP_Text[_choices.Count];
            for (int i = 0; i < _choices.Count; i++)
            {
                _choiceTexts[i] = _choices[i].GetComponentInChildren<TMP_Text>();
            }
            _choiceImages = new Image[_choices.Count];
            for (int i = 0; i < _choices.Count; i++)
            {
                _choiceImages[i] = _choices[i].transform.GetChild(0).GetComponent<Image>();
            }
        }

        public void SetTexts(string[] text)
        {
            if (text == null || text.Length != _choiceCount)
            {
                Debug.LogError("Text array length must match choice count.");
                return;
            }
            for (int i = 0; i < _choiceTexts.Length; i++)
            {
                if (_choiceTexts[i] != null)
                {
                    _choiceTexts[i].text = text[i];
                }
                else
                {
                    Debug.LogWarning($"Choice {i} does not have a text component.");
                }
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(MultipleChoiceButtons))]
        private class MultipleChoiceButtonsEditor : Editor
        {
            private MultipleChoiceButtons buttons;

            private void OnEnable()
            {
                buttons = (MultipleChoiceButtons)target;
                buttons.toggleGroup = buttons.GetComponent<ToggleGroup>();
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(choiceTemplate)));

                serializedObject.ApplyModifiedProperties();

                EditorGUI.BeginDisabledGroup(!buttons.choiceTemplate);
                Rect position = EditorGUILayout.GetControlRect();
                GUIContent label = EditorGUI.BeginProperty(position, new GUIContent("Choice Count"),
                    serializedObject.FindProperty(nameof(_choiceCount)));
                int count = EditorGUI.IntField(position, label, buttons.choiceCount);
                if (count < 1) count = 1;
                if (buttons.choiceCount != count && buttons.choiceTemplate)
                {
                    Undo.RegisterCompleteObjectUndo(buttons, "Updated Choice Count");
                    buttons.UpdateChoiceCount(count);
                }
                EditorGUI.EndProperty();
                EditorGUI.EndDisabledGroup();

                serializedObject.Update();

                // 添加代码来显示枚举字段
                position = EditorGUILayout.GetControlRect();
                label = EditorGUI.BeginProperty(position, new GUIContent("音效片段"),
                    serializedObject.FindProperty(nameof(audioDefine)));
                AudioDefine selectedType = (AudioDefine)EditorGUI.EnumPopup(position, label, buttons.audioDefine);
                if (buttons.audioDefine != selectedType)
                {
                    Undo.RegisterCompleteObjectUndo(buttons, "Updated Audio Define");
                    buttons.audioDefine = selectedType;
                }
                EditorGUI.EndProperty();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_choices)));
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();

                GUIContent[] names = new GUIContent[buttons.choiceCount];
                for (int i = 0; i < buttons.choiceCount; i++) names[i] = new GUIContent(buttons.choices[i].name);
                EditorGUI.BeginDisabledGroup(buttons.choiceCount == 0);
                position = EditorGUILayout.GetControlRect();
                label = EditorGUI.BeginProperty(position, new GUIContent("Choice"),
                    serializedObject.FindProperty(nameof(_choiceIndex)));
                int index = EditorGUI.Popup(position, label, buttons.choiceIndex, names);
                EditorGUI.EndProperty();
                EditorGUI.EndDisabledGroup();
                if (buttons.choiceIndex != index)
                {
                    Undo.RegisterCompleteObjectUndo(new Object[]
                    {
                buttons,
                buttons.choices[buttons.choiceIndex],
                buttons.choices[index]
                    }, "Updated Choice Index");
                    buttons.choiceIndex = index;
                }
            }
        }
#endif
    }

}