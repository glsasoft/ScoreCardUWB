using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
[ExecuteAlways]
public class TextMeshProOutline : MonoBehaviour
{
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0, 1)] private float outlineThickness = 0.2f;

    private TMP_Text _tmpText;
    private Material _originalMaterial;
    private Material _runtimeMaterial;
    private bool _isMaterialCloned;
    private bool _isDirty;
    private bool _isApplicationPlaying;

    public Color OutlineColor
    {
        get => outlineColor;
        set
        {
            if (outlineColor.Equals(value)) return;
            outlineColor = value;
            MarkDirty();
        }
    }

    public float OutlineThickness
    {
        get => outlineThickness;
        set
        {
            float clamped = Mathf.Clamp01(value);
            if (Mathf.Approximately(outlineThickness, clamped)) return;
            outlineThickness = clamped;
            MarkDirty();
        }
    }

    private TMP_Text TmpText
    {
        get
        {
            if (_tmpText == null)
                _tmpText = GetComponent<TMP_Text>();
            return _tmpText;
        }
    }

    private void Awake()
    {
        _isApplicationPlaying = Application.isPlaying;
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        ApplyOutlineSettings();
    }

    private void OnDisable()
    {
        RestoreOriginalMaterial();
    }

    private void OnDestroy()
    {
        RestoreOriginalMaterial();
        CleanupRuntimeMaterial();
        _isMaterialCloned = false;
    }

    private void OnValidate()
    {
        if (isActiveAndEnabled)
        {
            MarkDirty();
        }
    }

    private void Update()
    {
        if (_isDirty)
        {
            ApplyOutlineSettings();
            _isDirty = false;
        }
    }

    private void Initialize()
    {
        if (TmpText == null)
        {
            Debug.LogError("TextMeshProOutline: TMP_Text component not found.", this);
            enabled = false;
            return;
        }

        if (!_isMaterialCloned)
        {
            _originalMaterial = TmpText.fontSharedMaterial;

            // 在编辑模式下，只有当材质确实需要修改时才克隆
            if (_isApplicationPlaying || _originalMaterial != null)
            {
                _runtimeMaterial = TmpText.fontMaterial;
                _isMaterialCloned = true;
            }
        }
    }

    private void MarkDirty()
    {
        _isDirty = true;

#if UNITY_EDITOR
        if (!_isApplicationPlaying && isActiveAndEnabled)
        {
            ApplyOutlineSettings();
            _isDirty = false;
        }
#endif
    }

    private void ApplyOutlineSettings()
    {
        if (TmpText == null || !_isMaterialCloned || _runtimeMaterial == null)
            return;

        // 检查值是否实际改变
        if (!_runtimeMaterial.GetColor(OutlineColorId).Equals(outlineColor))
            _runtimeMaterial.SetColor(OutlineColorId, outlineColor);

        if (!Mathf.Approximately(_runtimeMaterial.GetFloat(OutlineWidthId), outlineThickness))
            _runtimeMaterial.SetFloat(OutlineWidthId, outlineThickness);

        // 只有在材质不同时才设置
        if (TmpText.fontSharedMaterial != _runtimeMaterial)
            TmpText.fontSharedMaterial = _runtimeMaterial;

        TmpText.SetVerticesDirty();
    }

    private void RestoreOriginalMaterial()
    {
        if (TmpText != null && _isMaterialCloned && _originalMaterial != null)
        {
            TmpText.fontSharedMaterial = _originalMaterial;
        }
    }

    private void CleanupRuntimeMaterial()
    {
        if (_runtimeMaterial != null)
        {
            if (_isApplicationPlaying)
                Destroy(_runtimeMaterial);
            else
                DestroyImmediate(_runtimeMaterial);

            _runtimeMaterial = null;
        }
    }

    [ContextMenu("Update Outline")]
    public void UpdateOutline()
    {
        Initialize();
        ApplyOutlineSettings();
    }
}