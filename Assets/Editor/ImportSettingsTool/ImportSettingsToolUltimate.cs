#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ImportSettingsToolUltimate : EditorWindow
{
    [SerializeField] private string _searchKeyword = "";
    [SerializeField] private List<string> _searchResults = new List<string>();
    [SerializeField] private Vector2 _resultScroll;

    [SerializeField] private List<DefaultAsset> _selectedFolders = new List<DefaultAsset>();
    [SerializeField] private Vector2 _folderScroll;

    private readonly List<ImportSettingsPreset> _presetAssets = new List<ImportSettingsPreset>();
    private string[] _presetNames = new[] { "无预设" };
    private int _presetIndex = 0;

    private ImportSettingsPreset _working;
    private double _lastAAFix;

    // == 新增：根据搜索结果类型控制面板显示 ==
    private enum AssetType { None, Texture, Model, Audio, Mixed }
    private AssetType _currentType = AssetType.None;

    private Vector2 _settingsScroll;

    [MenuItem("Tools/Import Settings Tool (资源导入设置) &1", false, 0)]
    public static void ShowWindow()
    {
        var w = GetWindow<ImportSettingsToolUltimate>("Import Settings Tool");
        w.minSize = new Vector2(1024, 640);
        w.Show();
    }

    private void OnEnable()
    {
        GuardAntiAliasing();
        LoadPresetAssets();
        EnsureWorkingCopy();
    }

    private void OnFocus()
    {
        LoadPresetAssets();
    }

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.Space(6);
        DrawSearchArea();
        EditorGUILayout.Space(6);
        DrawResultList();
        EditorGUILayout.Space(8);

        // 刷新当前类型并绘制设置面板
        UpdateCurrentType();
        DrawSettingsInspector();

        EditorGUILayout.Space(8);
        DrawBottomButtons();
        EditorGUILayout.Space(4);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("预设", GUILayout.Width(28));

        int newIndex = EditorGUILayout.Popup(_presetIndex, _presetNames, EditorStyles.toolbarPopup, GUILayout.Width(240));
        if (newIndex != _presetIndex)
        {
            _presetIndex = newIndex;
            if (_presetIndex != 0)
            {
                var p = _presetAssets[_presetIndex - 1];
                ApplyPresetToWorking(p);
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(64)))
            RunSearch();

        if (GUILayout.Button("保存为新预设", EditorStyles.toolbarButton, GUILayout.Width(120)))
            SaveWorkingAsPreset();

        using (new EditorGUI.DisabledScope(_presetIndex == 0))
        {
            if (GUILayout.Button("删除预设", EditorStyles.toolbarButton, GUILayout.Width(84)))
                DeleteSelectedPreset();
        }

        if (GUILayout.Button("导出 JSON", EditorStyles.toolbarButton, GUILayout.Width(96)))
            ExportWorkingJson();
        if (GUILayout.Button("导入 JSON", EditorStyles.toolbarButton, GUILayout.Width(96)))
            ImportWorkingJson();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSearchArea()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("搜索设置", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("关键字", GUILayout.Width(40));
        _searchKeyword = EditorGUILayout.TextField(_searchKeyword);
        if (GUILayout.Button("搜索", GUILayout.Width(70))) RunSearch();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("目标文件夹", EditorStyles.boldLabel);

        _folderScroll = EditorGUILayout.BeginScrollView(_folderScroll, GUILayout.Height(96));
        if (_selectedFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("未选择文件夹时，默认搜索整个 Assets。", MessageType.Info);
        }
        for (int i = 0; i < _selectedFolders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _selectedFolders[i] = (DefaultAsset)EditorGUILayout.ObjectField(_selectedFolders[i], typeof(DefaultAsset), false);
            if (GUILayout.Button("-", GUILayout.Width(24)))
            {
                _selectedFolders.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ 添加文件夹", GUILayout.Width(120)))
        {
            _selectedFolders.Add(null);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawResultList()
    {
        EditorGUILayout.LabelField("搜索结果", EditorStyles.boldLabel);
        _resultScroll = EditorGUILayout.BeginScrollView(_resultScroll, GUILayout.Height(220));
        if (_searchResults.Count == 0)
        {
            EditorGUILayout.HelpBox("暂无结果。请设置关键字并点击“搜索”。", MessageType.Info);
        }
        else
        {
            foreach (var path in _searchResults)
            {
                EditorGUILayout.SelectableLabel(path, GUILayout.Height(16));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    // == 新增：根据当前搜索结果判定类型 ==
    private void UpdateCurrentType()
    {
        _currentType = AssetType.None;
        foreach (var path in _searchResults)
        {
            var importer = AssetImporter.GetAtPath(path);
            if (importer is TextureImporter)
            {
                if (_currentType == AssetType.None) _currentType = AssetType.Texture;
                else if (_currentType != AssetType.Texture) { _currentType = AssetType.Mixed; return; }
            }
            else if (importer is ModelImporter)
            {
                if (_currentType == AssetType.None) _currentType = AssetType.Model;
                else if (_currentType != AssetType.Model) { _currentType = AssetType.Mixed; return; }
            }
            else if (importer is AudioImporter)
            {
                if (_currentType == AssetType.None) _currentType = AssetType.Audio;
                else if (_currentType != AssetType.Audio) { _currentType = AssetType.Mixed; return; }
            }
        }
    }

    private void DrawSettingsInspector()
    {
        EnsureWorkingCopy();
        _settingsScroll = EditorGUILayout.BeginScrollView(_settingsScroll);

        if (_currentType == AssetType.None)
        {
            EditorGUILayout.HelpBox("请先搜索资源。搜索结果的类型将决定显示哪些设置面板。", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        if (_currentType == AssetType.Mixed)
        {
            EditorGUILayout.HelpBox("当前选择包含多种资源类型，已显示全部设置。", MessageType.Warning);
        }

        if (_currentType == AssetType.Texture || _currentType == AssetType.Mixed)
            DrawTextureSettings();

        if (_currentType == AssetType.Model || _currentType == AssetType.Mixed)
            DrawModelSettings();

        if (_currentType == AssetType.Audio || _currentType == AssetType.Mixed)
            DrawAudioSettings();

        EditorGUILayout.EndScrollView();
    }

    // == 拆分：纹理设置面板 ==
    private void DrawTextureSettings()
    {
        EditorGUILayout.LabelField("贴图设置", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            _working.setTextureType = EditorGUILayout.ToggleLeft("启用 Texture Type", _working.setTextureType);
            if (_working.setTextureType)
                _working.textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", _working.textureType);

            _working.setSRGB = EditorGUILayout.ToggleLeft("启用 sRGB", _working.setSRGB);
            if (_working.setSRGB)
                _working.sRGB = EditorGUILayout.Toggle("sRGB (Color Texture)", _working.sRGB);

            _working.setMipMap = EditorGUILayout.ToggleLeft("启用 MipMaps", _working.setMipMap);
            if (_working.setMipMap)
                _working.mipmapEnabled = EditorGUILayout.Toggle("Generate Mipmaps", _working.mipmapEnabled);

            _working.setWrapMode = EditorGUILayout.ToggleLeft("启用 Wrap Mode", _working.setWrapMode);
            if (_working.setWrapMode)
                _working.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", _working.wrapMode);

            _working.setFilterMode = EditorGUILayout.ToggleLeft("启用 Filter Mode", _working.setFilterMode);
            if (_working.setFilterMode)
                _working.filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _working.filterMode);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("平台覆盖", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _working.platformDefault.enabled = EditorGUILayout.ToggleLeft("Default 平台覆盖", _working.platformDefault.enabled);
                if (_working.platformDefault.enabled) DrawPlatformBlock(ref _working.platformDefault, "Default");

                _working.platformAndroid.enabled = EditorGUILayout.ToggleLeft("Android 平台覆盖", _working.platformAndroid.enabled);
                if (_working.platformAndroid.enabled) DrawPlatformBlock(ref _working.platformAndroid, "Android");

                _working.platformiOS.enabled = EditorGUILayout.ToggleLeft("iPhone 平台覆盖", _working.platformiOS.enabled);
                if (_working.platformiOS.enabled) DrawPlatformBlock(ref _working.platformiOS, "iPhone");
            }
        }
    }

    // == 拆分：模型设置面板 ==
    private void DrawModelSettings()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("模型设置", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            _working.setModelNormals = EditorGUILayout.ToggleLeft("启用 法线", _working.setModelNormals);
            if (_working.setModelNormals)
                _working.modelNormals = (ModelImporterNormals)EditorGUILayout.EnumPopup("Normals", _working.modelNormals);

            _working.setModelTangents = EditorGUILayout.ToggleLeft("启用 切线", _working.setModelTangents);
            if (_working.setModelTangents)
                _working.modelTangents = (ModelImporterTangents)EditorGUILayout.EnumPopup("Tangents", _working.modelTangents);

            _working.setMaterialImportMode = EditorGUILayout.ToggleLeft("启用 材质导入模式", _working.setMaterialImportMode);
            if (_working.setMaterialImportMode)
                _working.materialImportMode = (ModelImporterMaterialImportMode)EditorGUILayout.EnumPopup("Material Import Mode", _working.materialImportMode);

            _working.setReadable = EditorGUILayout.ToggleLeft("启用 读写", _working.setReadable);
            if (_working.setReadable)
                _working.modelReadable = EditorGUILayout.Toggle("Read/Write Enabled", _working.modelReadable);
        }
    }

    // == 拆分：音频设置面板（含两个按钮） ==
    private void DrawAudioSettings()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("音频设置", EditorStyles.boldLabel);

        // 仅填充 UI 的智能推荐
        if (GUILayout.Button("一键智能设置音频参数（填充到当前面板）", GUILayout.Height(22)))
        {
            ApplySmartAudioRecommendation();
        }

        // 新增：真正批量应用的按钮
        if (GUILayout.Button("一键应用智能压缩（所有音频，直接写入并重导）", GUILayout.Height(22)))
        {
            ApplySmartAudioToAll();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("一键应用智能压缩（所有音频，直接写入并重导）", GUILayout.Height(25)))
        {
            if (_searchResults.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先搜索并选择音频文件。", "确定");
            }
            else
            {
                int count = 0;
                foreach (var path in _searchResults)
                {
                    var ai = AssetImporter.GetAtPath(path) as AudioImporter;
                    if (ai == null) continue;

                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip == null) continue;

                    ApplySmartSettingsToImporter(ai, clip.length);
                    ai.SaveAndReimport();
                    count++;
                }

                EditorUtility.DisplayDialog("完成", $"已智能压缩 {count} 个音频文件。", "确定");
            }
        }

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _working.setAudioForceToMono = EditorGUILayout.ToggleLeft("启用 Force To Mono", _working.setAudioForceToMono);
            if (_working.setAudioForceToMono)
                _working.audioForceToMono = EditorGUILayout.Toggle("Force To Mono", _working.audioForceToMono);

            _working.setAudioLoadInBackground = EditorGUILayout.ToggleLeft("启用 Load In Background", _working.setAudioLoadInBackground);
            if (_working.setAudioLoadInBackground)
                _working.audioLoadInBackground = EditorGUILayout.Toggle("Load In Background", _working.audioLoadInBackground);

            _working.setAudioAmbisonic = EditorGUILayout.ToggleLeft("启用 Ambisonic", _working.setAudioAmbisonic);
            if (_working.setAudioAmbisonic)
                _working.audioAmbisonic = EditorGUILayout.Toggle("Ambisonic", _working.audioAmbisonic);

            EditorGUILayout.Space(4);
            _working.setAudioLoadType = EditorGUILayout.ToggleLeft("启用 Load Type", _working.setAudioLoadType);
            if (_working.setAudioLoadType)
                _working.audioLoadType = (AudioClipLoadType)EditorGUILayout.EnumPopup("Load Type", _working.audioLoadType);

            _working.setAudioCompressionFormat = EditorGUILayout.ToggleLeft("启用 Compression Format", _working.setAudioCompressionFormat);
            if (_working.setAudioCompressionFormat)
                _working.audioCompressionFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup("Compression Format", _working.audioCompressionFormat);

            _working.setAudioQuality = EditorGUILayout.ToggleLeft("启用 Quality", _working.setAudioQuality);
            if (_working.setAudioQuality)
                _working.audioQuality = EditorGUILayout.Slider("Quality (0~1)", _working.audioQuality, 0f, 1f);

            _working.setSampleRateSetting = EditorGUILayout.ToggleLeft("启用 Sample Rate Setting", _working.setSampleRateSetting);
            if (_working.setSampleRateSetting)
                _working.sampleRateSetting = (AudioSampleRateSetting)EditorGUILayout.EnumPopup("Sample Rate Setting", _working.sampleRateSetting);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("平台覆盖", EditorStyles.miniBoldLabel);
            _working.audioAndroid.enabled = EditorGUILayout.ToggleLeft("Android 平台覆盖", _working.audioAndroid.enabled);
            if (_working.audioAndroid.enabled)
            {
                EditorGUI.indentLevel++;
                _working.audioAndroid.format = (AudioCompressionFormat)EditorGUILayout.EnumPopup("Format", _working.audioAndroid.format);
                _working.audioAndroid.quality = EditorGUILayout.Slider("Quality", _working.audioAndroid.quality, 0f, 1f);
                EditorGUI.indentLevel--;
            }

            _working.audioiOS.enabled = EditorGUILayout.ToggleLeft("iOS 平台覆盖", _working.audioiOS.enabled);
            if (_working.audioiOS.enabled)
            {
                EditorGUI.indentLevel++;
                _working.audioiOS.format = (AudioCompressionFormat)EditorGUILayout.EnumPopup("Format", _working.audioiOS.format);
                _working.audioiOS.quality = EditorGUILayout.Slider("Quality", _working.audioiOS.quality, 0f, 1f);
                EditorGUI.indentLevel--;
            }
        }
    }

    private void DrawPlatformBlock(ref ImportSettingsPreset.TexturePlatform p, string label)
    {
        EditorGUI.indentLevel++;
        p.overridden = true;
        p.maxTextureSize = EditorGUILayout.IntPopup("Max Size", p.maxTextureSize,
            new[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" },
            new[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 });

        p.resizeAlgorithm = (TextureResizeAlgorithm)EditorGUILayout.EnumPopup("Resize Algorithm", p.resizeAlgorithm);
        p.textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Format", p.textureFormat);
        p.compressionQuality = EditorGUILayout.IntSlider("Compression Quality", p.compressionQuality, 0, 100);
        p.allowsAlphaSplitting = EditorGUILayout.Toggle("Allow Alpha Split", p.allowsAlphaSplitting);
        EditorGUI.indentLevel--;
    }

    private void DrawBottomButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("应用修改", GUILayout.Height(28)))
            ApplyToResults();
        if (GUILayout.Button("刷新检查", GUILayout.Height(28)))
            ShowDifferences();
        EditorGUILayout.EndHorizontal();
    }

    private void RunSearch()
    {
        List<string> includeFolders = new List<string>();
        foreach (var folder in _selectedFolders)
        {
            if (folder == null) continue;
            string p = AssetDatabase.GetAssetPath(folder);
            if (AssetDatabase.IsValidFolder(p))
                includeFolders.Add(p);
        }
        if (includeFolders.Count == 0) includeFolders.Add("Assets");

        string[] guids = AssetDatabase.FindAssets(_searchKeyword, includeFolders.ToArray());

        _searchResults.Clear();
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (!string.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
            {
                _searchResults.Add(path);
            }
        }

        Debug.Log($"[ImportSettingsTool] 搜索到 {_searchResults.Count} 个资源");
        Repaint();
    }

    private void EnsureWorkingCopy()
    {
        if (_working == null)
        {
            _working = ScriptableObject.CreateInstance<ImportSettingsPreset>();
        }
    }

    private void LoadPresetAssets()
    {
        _presetAssets.Clear();
        var guids = AssetDatabase.FindAssets("t:ImportSettingsPreset");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<ImportSettingsPreset>(path);
            if (asset != null) _presetAssets.Add(asset);
        }
        var names = new List<string> { "无预设" };
        names.AddRange(_presetAssets.Select(p => p.name));
        _presetNames = names.ToArray();
        if (_presetIndex >= _presetNames.Length) _presetIndex = 0;
    }

    private void ApplyPresetToWorking(ImportSettingsPreset p)
    {
        if (p == null) return;
        var json = JsonUtility.ToJson(p);
        JsonUtility.FromJsonOverwrite(json, _working);

        _selectedFolders.Clear();
        if (p.targetFolders != null)
        {
            foreach (var path in p.targetFolders)
            {
                var folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
                if (folderAsset != null) _selectedFolders.Add(folderAsset);
            }
        }
    }

    private void SaveWorkingAsPreset()
    {
        string folder = "Assets/Editor/ImportSettingsTool/ImportSettingsPresets";
        if (!AssetDatabase.IsValidFolder("Assets/Editor/ImportSettingsTool"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                AssetDatabase.CreateFolder("Assets", "Editor");
            AssetDatabase.CreateFolder("Assets/Editor", "ImportSettingsTool");
        }
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Editor/ImportSettingsTool", "ImportSettingsPresets");
        }

        string path = EditorUtility.SaveFilePanelInProject("保存预设", "NewImportPreset", "asset", "选择保存位置", folder);
        if (string.IsNullOrEmpty(path)) return;

        var preset = ScriptableObject.CreateInstance<ImportSettingsPreset>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_working), preset);

        var folderPaths = new List<string>();
        foreach (var f in _selectedFolders)
        {
            if (f == null) continue;
            string pth = AssetDatabase.GetAssetPath(f);
            if (AssetDatabase.IsValidFolder(pth)) folderPaths.Add(pth);
        }
        preset.targetFolders = folderPaths.ToArray();

        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();
        LoadPresetAssets();
        EditorUtility.DisplayDialog("成功", $"预设已保存：{path}", "OK");
    }

    private void DeleteSelectedPreset()
    {
        if (_presetIndex <= 0) return;
        var target = _presetAssets[_presetIndex - 1];
        string path = AssetDatabase.GetAssetPath(target);
        if (EditorUtility.DisplayDialog("删除预设", $"确定删除预设：{target.name} ?", "删除", "取消"))
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            LoadPresetAssets();
            _presetIndex = 0;
        }
    }

    private void ExportWorkingJson()
    {
        string path = EditorUtility.SaveFilePanel("导出 JSON", "", "ImportSettingsPreset.json", "json");
        if (string.IsNullOrEmpty(path)) return;
        File.WriteAllText(path, JsonUtility.ToJson(_working, true));
        EditorUtility.RevealInFinder(path);
    }

    private void ImportWorkingJson()
    {
        string path = EditorUtility.OpenFilePanel("导入 JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, _working);
            EditorUtility.DisplayDialog("成功", "JSON 已导入到当前面板（不会自动应用到资源）。", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("失败", "解析 JSON 出错：\n" + e.Message, "OK");
        }
    }

    private void ApplyToResults()
    {
        if (_searchResults.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有可应用的资源，请先搜索。", "OK");
            return;
        }

        int applied = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var path in _searchResults)
            {
                var importer = AssetImporter.GetAtPath(path);
                if (importer is TextureImporter ti)
                {
                    ApplyTextureSettings(ti);
                    applied++;
                }
                else if (importer is ModelImporter mi)
                {
                    ApplyModelSettings(mi);
                    applied++;
                }
                else if (importer is AudioImporter ai)
                {
                    ApplyAudioSettings(ai, _working);
                    applied++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("完成", $"已应用：{applied} 个资源导入设置。", "OK");
    }

    private void ApplyTextureSettings(TextureImporter ti)
    {
        if (_working.setTextureType) ti.textureType = _working.textureType;
        if (_working.setSRGB) ti.sRGBTexture = _working.sRGB;
        if (_working.setMipMap) ti.mipmapEnabled = _working.mipmapEnabled;
        if (_working.setWrapMode) ti.wrapMode = _working.wrapMode;
        if (_working.setFilterMode) ti.filterMode = _working.filterMode;

        ApplyPlatformSettings(ti, "Default", _working.platformDefault);
        ApplyPlatformSettings(ti, "Android", _working.platformAndroid);
        ApplyPlatformSettings(ti, "iPhone", _working.platformiOS);

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
    }

    private void ApplyPlatformSettings(TextureImporter ti, string name, ImportSettingsPreset.TexturePlatform src)
    {
        if (!src.enabled) return;

        var ps = ti.GetPlatformTextureSettings(name);
        if (ps == null) ps = new TextureImporterPlatformSettings();
        ps.name = name;
        ps.overridden = true;
        ps.maxTextureSize = Mathf.Max(32, src.maxTextureSize);
        ps.resizeAlgorithm = src.resizeAlgorithm;
        ps.textureCompression = TextureImporterCompression.Compressed;
        ps.format = src.textureFormat;
        ps.compressionQuality = Mathf.Clamp(src.compressionQuality, 0, 100);
        ps.allowsAlphaSplitting = src.allowsAlphaSplitting;
        ti.SetPlatformTextureSettings(ps);
    }

    private void ApplyModelSettings(ModelImporter mi)
    {
        if (_working.setModelNormals) mi.importNormals = _working.modelNormals;
        if (_working.setModelTangents) mi.importTangents = _working.modelTangents;
        if (_working.setMaterialImportMode) mi.materialImportMode = _working.materialImportMode;
        if (_working.setReadable) mi.isReadable = _working.modelReadable;

        EditorUtility.SetDirty(mi);
        mi.SaveAndReimport();
    }

    private void ApplyAudioSettings(AudioImporter ai, ImportSettingsPreset preset)
    {
        // Force To Mono
        if (preset.setAudioForceToMono)
            ai.forceToMono = preset.audioForceToMono;

        // Load In Background
        if (preset.setAudioLoadInBackground)
            ai.loadInBackground = preset.audioLoadInBackground;

        // Ambisonic
        if (preset.setAudioAmbisonic)
            ai.ambisonic = preset.audioAmbisonic;

        // Default Sample Settings
        if (preset.setAudioLoadType || preset.setAudioCompressionFormat || preset.setAudioQuality || preset.setSampleRateSetting)
        {
            var s = ai.defaultSampleSettings;

            if (preset.setAudioLoadType)
                s.loadType = preset.audioLoadType;

            if (preset.setAudioCompressionFormat)
                s.compressionFormat = preset.audioCompressionFormat;

            if (preset.setAudioQuality)
                s.quality = Mathf.Clamp01(preset.audioQuality);

            if (preset.setSampleRateSetting)
                s.sampleRateSetting = preset.sampleRateSetting;

            ai.defaultSampleSettings = s;
        }

        // 平台覆盖 - Android
        if (preset.audioAndroid.enabled)
        {
            var s = ai.GetOverrideSampleSettings("Android");
            s.compressionFormat = preset.audioAndroid.format;
            s.quality = Mathf.Clamp01(preset.audioAndroid.quality);
            ai.SetOverrideSampleSettings("Android", s);
        }

        // 平台覆盖 - iOS
        if (preset.audioiOS.enabled)
        {
            var s = ai.GetOverrideSampleSettings("iOS");
            s.compressionFormat = preset.audioiOS.format;
            s.quality = Mathf.Clamp01(preset.audioiOS.quality);
            ai.SetOverrideSampleSettings("iOS", s);
        }

        EditorUtility.SetDirty(ai);
        ai.SaveAndReimport();
    }

    // 旧辅助（保留兼容）
    private void ApplyAudioPlatform(AudioImporter ai, string platform, ImportSettingsPreset.AudioPlatform p)
    {
        if (!p.enabled) return;
        var s = new AudioImporterSampleSettings
        {
            compressionFormat = p.format,
            quality = Mathf.Clamp01(p.quality)
        };
        ai.SetOverrideSampleSettings(platform, s);
    }

    private void ShowDifferences()
    {
        if (_searchResults.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有可检查的资源，请先搜索。", "OK");
            return;
        }

        var lines = new List<string>();
        foreach (var path in _searchResults)
        {
            var importer = AssetImporter.GetAtPath(path);
            if (importer is TextureImporter ti)
                lines.AddRange(TextureDiff(path, ti));
            else if (importer is ModelImporter mi)
                lines.AddRange(ModelDiff(path, mi));
            else if (importer is AudioImporter ai)
                lines.AddRange(AudioDiff(path, ai));
        }

        if (lines.Count == 0) lines.Add("全部匹配，无差异。");
        string msg = string.Join("\n", lines.Take(200));
        EditorUtility.DisplayDialog("刷新检查结果", msg, "OK");
    }

    private IEnumerable<string> TextureDiff(string path, TextureImporter ti)
    {
        var l = new List<string>();
        if (_working.setTextureType && ti.textureType != _working.textureType) l.Add($"{path} : TextureType {ti.textureType} -> {_working.textureType}");
        if (_working.setSRGB && ti.sRGBTexture != _working.sRGB) l.Add($"{path} : sRGB {ti.sRGBTexture} -> {_working.sRGB}");
        if (_working.setMipMap && ti.mipmapEnabled != _working.mipmapEnabled) l.Add($"{path} : MipMap {ti.mipmapEnabled} -> {_working.mipmapEnabled}");
        if (_working.setWrapMode && ti.wrapMode != _working.wrapMode) l.Add($"{path} : Wrap {ti.wrapMode} -> {_working.wrapMode}");
        if (_working.setFilterMode && ti.filterMode != _working.filterMode) l.Add($"{path} : Filter {ti.filterMode} -> {_working.filterMode}");

        l.AddRange(PlatformDiff("Default", ti, _working.platformDefault, path));
        l.AddRange(PlatformDiff("Android", ti, _working.platformAndroid, path));
        l.AddRange(PlatformDiff("iPhone", ti, _working.platformiOS, path));
        return l;
    }

    private IEnumerable<string> PlatformDiff(string platform, TextureImporter ti, ImportSettingsPreset.TexturePlatform src, string path)
    {
        var l = new List<string>();
        if (!src.enabled) return l;

        var ps = ti.GetPlatformTextureSettings(platform);
        if (ps == null || !ps.overridden)
        {
            l.Add($"{path} : {platform} 未覆盖 -> 需要覆盖");
            return l;
        }

        if (ps.maxTextureSize != src.maxTextureSize) l.Add($"{path} : {platform}.MaxSize {ps.maxTextureSize} -> {src.maxTextureSize}");
        if (ps.resizeAlgorithm != src.resizeAlgorithm) l.Add($"{path} : {platform}.Resize {ps.resizeAlgorithm} -> {src.resizeAlgorithm}");
        if (ps.format != src.textureFormat) l.Add($"{path} : {platform}.Format {ps.format} -> {src.textureFormat}");
        if (ps.compressionQuality != src.compressionQuality) l.Add($"{path} : {platform}.Quality {ps.compressionQuality} -> {src.compressionQuality}");
        if (ps.allowsAlphaSplitting != src.allowsAlphaSplitting) l.Add($"{path} : {platform}.AlphaSplit {ps.allowsAlphaSplitting} -> {src.allowsAlphaSplitting}");

        return l;
    }

    private IEnumerable<string> ModelDiff(string path, ModelImporter mi)
    {
        var l = new List<string>();
        if (_working.setModelNormals && mi.importNormals != _working.modelNormals) l.Add($"{path} : Normals {mi.importNormals} -> {_working.modelNormals}");
        if (_working.setModelTangents && mi.importTangents != _working.modelTangents) l.Add($"{path} : Tangents {mi.importTangents} -> {_working.modelTangents}");
        if (_working.setMaterialImportMode && mi.materialImportMode != _working.materialImportMode) l.Add($"{path} : MaterialMode {mi.materialImportMode} -> {_working.materialImportMode}");
        if (_working.setReadable && mi.isReadable != _working.modelReadable) l.Add($"{path} : Read/Write {mi.isReadable} -> {_working.modelReadable}");
        return l;
    }

    private IEnumerable<string> AudioDiff(string path, AudioImporter ai)
    {
        var l = new List<string>();
        if (_working.setAudioForceToMono && ai.forceToMono != _working.audioForceToMono) l.Add($"{path} : ForceToMono {ai.forceToMono} -> {_working.audioForceToMono}");
        if (_working.setAudioLoadInBackground && ai.loadInBackground != _working.audioLoadInBackground) l.Add($"{path} : LoadInBackground {ai.loadInBackground} -> {_working.audioLoadInBackground}");
        if (_working.setAudioAmbisonic && ai.ambisonic != _working.audioAmbisonic) l.Add($"{path} : Ambisonic {ai.ambisonic} -> {_working.audioAmbisonic}");

        var s = ai.defaultSampleSettings;
        if (_working.setAudioLoadType && s.loadType != _working.audioLoadType) l.Add($"{path} : LoadType {s.loadType} -> {_working.audioLoadType}");
        if (_working.setAudioCompressionFormat && s.compressionFormat != _working.audioCompressionFormat) l.Add($"{path} : CompressionFormat {s.compressionFormat} -> {_working.audioCompressionFormat}");
        if (_working.setAudioQuality && Math.Abs(s.quality - _working.audioQuality) > 0.01f) l.Add($"{path} : Quality {s.quality} -> {_working.audioQuality}");
        if (_working.setSampleRateSetting && s.sampleRateSetting != _working.sampleRateSetting) l.Add($"{path} : SampleRateSetting {s.sampleRateSetting} -> {_working.sampleRateSetting}");

        return l;
    }

    private void GuardAntiAliasing()
    {
        if (EditorApplication.timeSinceStartup - _lastAAFix < 1.0) return;
        _lastAAFix = EditorApplication.timeSinceStartup;

        try
        {
            if (QualitySettings.antiAliasing < 1) QualitySettings.antiAliasing = 1;
        }
        catch { }

        try
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv != null && sv.camera != null && sv.camera.allowMSAA == false)
                sv.camera.allowMSAA = true;
        }
        catch { }
    }

    // == 仅填充 UI（参考第一个音频） ==
    private void ApplySmartAudioRecommendation()
    {
        if (_searchResults.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先搜索音频资源。", "OK");
            return;
        }

        foreach (var path in _searchResults)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                float length = clip.length;
                string category;
                if (length < 3f)
                {
                    category = "短音效 (<3s)";
                    _working.setAudioLoadType = true;
                    _working.audioLoadType = AudioClipLoadType.DecompressOnLoad;

                    _working.setAudioCompressionFormat = true;
                    _working.audioCompressionFormat = AudioCompressionFormat.ADPCM;

                    _working.setAudioQuality = false; // ADPCM 无质量设置

                    _working.setSampleRateSetting = true;
                    _working.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
                }
                else if (length < 30f)
                {
                    category = "语音对白 (3~30s)";
                    _working.setAudioLoadType = true;
                    _working.audioLoadType = AudioClipLoadType.CompressedInMemory;

                    _working.setAudioCompressionFormat = true;
                    _working.audioCompressionFormat = AudioCompressionFormat.Vorbis;

                    _working.setAudioQuality = true;
                    _working.audioQuality = 0.6f;

                    _working.setSampleRateSetting = true;
                    _working.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
                }
                else
                {
                    category = "背景音乐 (>30s)";
                    _working.setAudioLoadType = true;
                    _working.audioLoadType = AudioClipLoadType.Streaming;

                    _working.setAudioCompressionFormat = true;
                    _working.audioCompressionFormat = AudioCompressionFormat.Vorbis;

                    _working.setAudioQuality = true;
                    _working.audioQuality = 0.5f;

                    _working.setSampleRateSetting = true;
                    _working.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                }

                EditorUtility.DisplayDialog("智能音频参数已设置（UI）",
                    $"检测到第一个音频长度：{length:F1} 秒，应用策略：{category}\n\n已填充到当前面板，请检查后点击“应用修改”生效。",
                    "OK");
                return;
            }
        }

        EditorUtility.DisplayDialog("提示", "搜索结果中未找到音频资源。", "OK");
    }

    // == 新增：真正批量应用到所有音频 ==
    private void ApplySmartAudioToAll()
    {
        if (_searchResults.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先搜索音频资源。", "OK");
            return;
        }

        int countClips = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var path in _searchResults)
            {
                var ai = AssetImporter.GetAtPath(path) as AudioImporter;
                if (ai == null) continue;

                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                ApplySmartSettingsToImporter(ai, clip.length);
                countClips++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("完成", $"已对 {countClips} 个音频应用智能压缩并重新导入。", "OK");
    }

    /// <summary>
    /// 根据音频长度智能应用压缩策略
    /// </summary>
    private void ApplySmartSettingsToImporter(AudioImporter ai, float length)
    {
        var settings = ai.defaultSampleSettings;

        // 智能判断规则
        if (length < 3f) // 短音效（枪声、UI音效）
        {
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.quality = 1f;
            settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        }
        else if (length < 30f) // 语音对白
        {
            settings.loadType = AudioClipLoadType.CompressedInMemory;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        }
        else // 背景音乐
        {
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.5f;
            settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        }

        ai.defaultSampleSettings = settings;

        // 如果有平台覆盖，可以根据需要加上（可选）
        // 示例：Android 和 iOS 保持 Vorbis
        var androidSettings = settings;
        androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;
        androidSettings.quality = settings.quality;
        ai.SetOverrideSampleSettings("Android", androidSettings);

        var iosSettings = settings;
        iosSettings.compressionFormat = AudioCompressionFormat.Vorbis;
        iosSettings.quality = settings.quality;
        ai.SetOverrideSampleSettings("iOS", iosSettings);
    }

}

#endif
