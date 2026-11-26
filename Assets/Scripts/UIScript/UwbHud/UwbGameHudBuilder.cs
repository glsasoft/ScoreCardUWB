#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 按 PSD 导出的布局 JSON 和切图，生成「游戏中」HUD 预制体 UwbGameHudPanel。
    /// 切图和 JSON 由 Docs/tools/psd_hud_export.py 生成。生成后的预制体可以直接在编辑器里手调，
    /// 重新执行菜单会覆盖手调结果。
    /// 坐标换算：PSD 画布 3840x2160 对应 UI 参考分辨率 1920x1080，切图 PPU=2，所以 UI 尺寸 = PSD 像素 / 2。
    /// </summary>
    public static class UwbGameHudBuilder
    {
        const string SpriteDir = "Assets/Package_Offline/PanleUI/UwbHud/Sprites";
        const string PrefabDir = "Assets/Package_Offline/PanleUI/UwbHud/Prefabs";
        const string PrefabPath = PrefabDir + "/UwbGameHudPanel.prefab";
        const string LayoutPath = "Assets/Scripts/UIScript/UwbHud/Layout/uwb_hud_layout.json";
        const string FontDir = "Assets/TextMesh Pro/Resources/Fonts & Materials/Fonts";
        const string PuHuiTiTtf = FontDir + "/AlibabaPuHuiTi-3-75-SemiBold.ttf";
        const string PuHuiTiAsset = FontDir + "/AlibabaPuHuiTi-3-75-SemiBold SDF.asset";
        const string ImpactAsset = FontDir + "/Impact SDF.asset";
        const string TagNameMatPath = PrefabDir + "/UwbHud_TagName_Outline.mat";
        const string CircleMatPath = "Assets/Art/Materials/UI/CircleRawImage_M.mat";
        const string ScoreBarSourcePath = "Assets/Art/UwbHud/ScoreBar/GLSAQuadView/4_FightingScreen/Prefabs/PanelUI/Widgets/ScoreModeNvNFightTopbar.prefab";
        const string ScoreBarPath = PrefabDir + "/UwbScoreBar.prefab";
        const string AvatarFrameFile = "frame2.png";
        const string AvatarDefaultFile = "head2.png";

        const float CanvasW = 3840f;
        const float CanvasH = 2160f;
        const float Half = 0.5f;
        const float ImpactScale = 0.94f;

        [Serializable] class LSprite { public string name; public string file; public int x, y, w, h; public int[] slice; }
        [Serializable] class LText { public string key; public string text; public string color; public string font; public string align; public float fontSize; public int tracking; public int[] bbox; }
        [Serializable] class LRect { public int x, y, w, h; }
        [Serializable] class LCard { public int[] red_origin; public int[] blue_origin; public int step; public int count; public LRect avatar; }
        [Serializable] class LTag { public int[] size; public LSprite[] sprites; public LText[] texts; public LRect avatar; }
        [Serializable] class Layout { public int[] canvas; public LRect score_bar; public LSprite[] sprites; public LText[] texts; public LTag tag; public LCard card; }

        enum Space { TopCenter, BottomCenter, Local }

        static Layout s_Layout;
        static Dictionary<string, LSprite> s_Sprites;
        static Dictionary<string, LText> s_Texts;
        static Dictionary<string, LSprite> s_TagSprites;
        static Dictionary<string, LText> s_TagTexts;
        static TMP_FontAsset s_Chinese;
        static TMP_FontAsset s_Impact;
        static Material s_TagNameMat;

        [MenuItem("LetsPiu/UWB/生成游戏中HUD预制体")]
        public static void Build()
        {
            LoadLayout();
            ImportSprites();
            LoadFonts();

            Directory.CreateDirectory(PrefabDir);
            BuildScoreBarPrefab();
            // 根节点自带 Canvas，保存为预制体根节点时 Unity 会接管它的 RectTransform（缩放被写成 0），
            // 保存后直接改 yaml，把根节点还原成铺满父节点。
            var holder = new GameObject("Holder", typeof(RectTransform), typeof(Canvas));
            GameObject root = BuildRoot();
            try
            {
                root.transform.SetParent(holder.transform, false);
                Stretch((RectTransform)root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                FixRootRectInYaml();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UwbGameHudBuilder] 已生成 " + PrefabPath);
        }

        static void FixRootRectInYaml()
        {
            string text = File.ReadAllText(PrefabPath);
            int father = text.IndexOf("m_Father: {fileID: 0}", StringComparison.Ordinal);
            int start = text.LastIndexOf("--- !u!224 &", father, StringComparison.Ordinal);
            int end = text.IndexOf("--- !u!", father, StringComparison.Ordinal);
            if (father < 0 || start < 0 || end < 0)
                throw new InvalidDataException("找不到预制体根节点 RectTransform");

            string block = text.Substring(start, end - start);
            block = System.Text.RegularExpressions.Regex.Replace(block, @"m_LocalScale: \{[^}]*\}", "m_LocalScale: {x: 1, y: 1, z: 1}");
            block = System.Text.RegularExpressions.Regex.Replace(block, @"m_AnchorMin: \{[^}]*\}", "m_AnchorMin: {x: 0, y: 0}");
            block = System.Text.RegularExpressions.Regex.Replace(block, @"m_AnchorMax: \{[^}]*\}", "m_AnchorMax: {x: 1, y: 1}");
            block = System.Text.RegularExpressions.Regex.Replace(block, @"m_Pivot: \{[^}]*\}", "m_Pivot: {x: 0.5, y: 0.5}");
            File.WriteAllText(PrefabPath, text.Substring(0, start) + block + text.Substring(end));
            AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate);
        }

        #region 资源准备

        static void LoadLayout()
        {
            s_Layout = JsonUtility.FromJson<Layout>(File.ReadAllText(LayoutPath));
            s_Sprites = new Dictionary<string, LSprite>();
            foreach (LSprite s in s_Layout.sprites)
                s_Sprites[s.name] = s;
            s_TagSprites = new Dictionary<string, LSprite>();
            foreach (LSprite s in s_Layout.tag.sprites)
                s_TagSprites[s.name] = s;
            s_Texts = new Dictionary<string, LText>();
            foreach (LText t in s_Layout.texts)
                s_Texts[t.key] = t;
            s_TagTexts = new Dictionary<string, LText>();
            foreach (LText t in s_Layout.tag.texts)
                s_TagTexts[t.key] = t;
        }

        static void ImportSprites()
        {
            var all = new List<LSprite>(s_Layout.sprites);
            all.AddRange(s_Layout.tag.sprites);
            foreach (LSprite s in all)
            {
                string path = SpriteDir + "/" + s.file;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    throw new FileNotFoundException("缺少切图 " + path);

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 2f;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 4096;

                int[] sl = s.slice;
                importer.spriteBorder = sl != null && sl.Length == 4
                    ? new Vector4(sl[0], sl[3], sl[1], sl[2])
                    : Vector4.zero;

                importer.SaveAndReimport();
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                bool small = tex != null && Mathf.Max(tex.width, tex.height) <= 512;
                importer.textureCompression = small ? TextureImporterCompression.Uncompressed : TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        static void LoadFonts()
        {
            s_Impact = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ImpactAsset);
            if (s_Impact == null)
                throw new FileNotFoundException("缺少 " + ImpactAsset + "，先执行 Tools/字体相关/(TMP)Create Impact SDF Font");

            s_Chinese = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PuHuiTiAsset);
            if (s_Chinese == null)
                s_Chinese = CreatePuHuiTiAsset();

            var chars = new HashSet<char>();
            foreach (LText t in s_Layout.texts)
                foreach (char c in t.text) chars.Add(c);
            foreach (LText t in s_Layout.tag.texts)
                foreach (char c in t.text) chars.Add(c);
            foreach (char c in "0123456789:%+-/ sS可复活待击杀死亡") chars.Add(c);
            var sb = new System.Text.StringBuilder();
            foreach (char c in chars) sb.Append(c);
            s_Chinese.TryAddCharacters(sb.ToString());
            EditorUtility.SetDirty(s_Chinese);

            s_TagNameMat = AssetDatabase.LoadAssetAtPath<Material>(TagNameMatPath);
            if (s_TagNameMat == null)
            {
                Directory.CreateDirectory(PrefabDir);
                s_TagNameMat = new Material(s_Chinese.material) { name = "UwbHud_TagName_Outline" };
                AssetDatabase.CreateAsset(s_TagNameMat, TagNameMatPath);
            }

            s_TagNameMat.EnableKeyword("OUTLINE_ON");
            s_TagNameMat.SetFloat("_OutlineWidth", 0.22f);
            s_TagNameMat.SetColor("_OutlineColor", Color.black);
            EditorUtility.SetDirty(s_TagNameMat);
        }

        static TMP_FontAsset CreatePuHuiTiAsset()
        {
            var source = AssetDatabase.LoadAssetAtPath<Font>(PuHuiTiTtf);
            if (source == null)
                throw new FileNotFoundException("缺少字体 " + PuHuiTiTtf);

            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
            asset.name = "AlibabaPuHuiTi-3-75-SemiBold SDF";
            asset.TryAddCharacters(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
            AssetDatabase.CreateAsset(asset, PuHuiTiAsset);
            if (asset.material != null)
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            if (asset.atlasTextures != null)
            {
                foreach (Texture2D tex in asset.atlasTextures)
                    if (tex != null) AssetDatabase.AddObjectToAsset(tex, asset);
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        #endregion

        #region 构建

        static GameObject BuildRoot()
        {
            var root = new GameObject("UwbGameHudPanel", typeof(RectTransform));
            root.layer = LayerMask.NameToLayer("UI");
            var panel = root.AddComponent<UwbGameHudPanel>();
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var rootRt = root.GetComponent<RectTransform>();
            Stretch(rootRt);
            rootRt.localScale = Vector3.one;

            // 绘制顺序（自下而上）：头顶标牌 → 顶部边框 → 比分条 → 任务说明 → 底部队员栏
            RectTransform tagLayer = NewRect("TagLayer", rootRt);
            Stretch(tagLayer);
            UwbPlayerTagItem tagTemplate = BuildTagTemplate(tagLayer);

            RectTransform frame = NewRect("Frame", rootRt);
            Stretch(frame);
            BuildFrame(frame);

            UwbScoreBar scoreBar = BuildScoreBar(rootRt);

            RectTransform topBarRt = NewRect("TopBar", rootRt);
            Stretch(topBarRt);
            UwbHudTopBar topBar = BuildTopBar(topBarRt, scoreBar);

            RectTransform bottom = NewRect("BottomBar", rootRt);
            Stretch(bottom);
            UwbPlayerCardItem[] red;
            UwbPlayerCardItem[] blue;
            BuildBottomBar(bottom, out red, out blue);

            Stretch(rootRt);
            rootRt.localScale = Vector3.one;

            var so = new SerializedObject(panel);
            so.FindProperty("topBar").objectReferenceValue = topBar;
            so.FindProperty("tagLayer").objectReferenceValue = tagLayer;
            so.FindProperty("tagTemplate").objectReferenceValue = tagTemplate;
            SetArray(so.FindProperty("redCards"), red);
            SetArray(so.FindProperty("blueCards"), blue);
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        static void BuildFrame(RectTransform parent)
        {
            LSprite band = s_Sprites["hud_top_band"];
            Image bandImg = AddSprite(parent, band, Space.TopCenter, "TopBand");
            var bandRt = bandImg.rectTransform;
            bandRt.anchorMin = new Vector2(0f, 1f);
            bandRt.anchorMax = new Vector2(1f, 1f);
            bandRt.pivot = new Vector2(0.5f, 1f);
            bandRt.anchoredPosition = Vector2.zero;
            bandRt.sizeDelta = new Vector2(0f, band.h * Half);

            LSprite bezel = s_Sprites["hud_bezel"];
            Image bezelImg = AddSprite(parent, bezel, Space.TopCenter, "Bezel");
            bezelImg.fillCenter = false;
            var bezelRt = bezelImg.rectTransform;
            bezelRt.anchorMin = Vector2.zero;
            bezelRt.anchorMax = Vector2.one;
            bezelRt.pivot = new Vector2(0.5f, 0.5f);
            bezelRt.offsetMin = Vector2.zero;
            bezelRt.offsetMax = new Vector2(0f, -bezel.y * Half);
        }

        // 比分条预制体：取 ScoreCardUnity 的 ScoreModeNvNFightTopbar（已连同特效整套拷到 Assets/Art/UwbHud/ScoreBar），
        // 只留 FillBg（进度条背景、遮罩、填充特效）和红蓝常态/高亮特效，去掉原来的顶部计时牌、旗帜、积分文字（这些用 PSD 切图）。
        static void BuildScoreBarPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(ScoreBarSourcePath);
            try
            {
                foreach (string n in new[] { "TopCanvas", "RedInfo", "BlueInfo" })
                    UnityEngine.Object.DestroyImmediate(root.transform.Find(n).gameObject);

                foreach (string n in new[] { "FillBg/RedFill", "FillBg/BlueFill" })
                {
                    Transform fill = root.transform.Find(n);
                    for (int i = fill.childCount - 1; i >= 0; i--)
                        UnityEngine.Object.DestroyImmediate(fill.GetChild(i).gameObject);
                }

                foreach (MonoBehaviour mb in root.GetComponents<MonoBehaviour>())
                {
                    if (mb == null || mb.GetType().Name == "ScoreModeNvNFightTopbar")
                        UnityEngine.Object.DestroyImmediate(mb);
                }

                root.name = "UwbScoreBar";
                root.AddComponent<UwbScoreBar>();
                PrefabUtility.SaveAsPrefabAsset(root, ScoreBarPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // 原预制体里比分条宽 4390、旗帜在 ±2362；PSD 里比分条宽 3203（UI 单位 1601.5），按宽度等比缩放。
        // 比分条中心取 PSD 里红条的中心 (1931.5, 104.5)；预制体根节点比 FillBg 高 55 个单位。
        static UwbScoreBar BuildScoreBar(RectTransform parent)
        {
            const float SourceBarWidth = 4390f;
            const float SourceBarOffsetY = -55f;
            LRect bar = s_Layout.score_bar;
            float scale = bar.w * Half / SourceBarWidth;
            float cx = (bar.x + bar.w * Half - CanvasW * Half) * Half;
            float cy = -(bar.y + bar.h * Half) * Half;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ScoreBarPath);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(cx, cy - SourceBarOffsetY * scale);
            rt.localScale = new Vector3(scale, scale, scale);
            return go.GetComponent<UwbScoreBar>();
        }

        static UwbHudTopBar BuildTopBar(RectTransform parent, UwbScoreBar scoreBar)
        {
            AddSprite(parent, s_Sprites["hud_center"], Space.TopCenter, "Center");
            AddSprite(parent, s_Sprites["hud_flag_red"], Space.TopCenter, "FlagRed");
            AddSprite(parent, s_Sprites["hud_flag_blue"], Space.TopCenter, "FlagBlue");

            AddText(parent, "score_label", Space.TopCenter, 0, 0, "ScoreLabel");
            TMP_Text timer = AddText(parent, "timer", Space.TopCenter, 0, 0, "Timer");
            AddText(parent, "mode_label", Space.TopCenter, 0, 0, "ModeLabel");
            AddText(parent, "red_score_label", Space.TopCenter, 0, 0, "RedScoreLabel");
            TMP_Text redScore = AddText(parent, "red_score", Space.TopCenter, 0, 0, "RedScore", "left");
            AddText(parent, "blue_score_label", Space.TopCenter, 0, 0, "BlueScoreLabel");
            TMP_Text blueScore = AddText(parent, "blue_score", Space.TopCenter, 0, 0, "BlueScore", "left");
            TMP_Text redOut = AddText(parent, "red_flag_number", Space.TopCenter, 0, 0, "RedEliminated");
            AddText(parent, "red_flag_label", Space.TopCenter, 0, 0, "RedFlagLabel");
            TMP_Text blueOut = AddText(parent, "blue_flag_number", Space.TopCenter, 0, 0, "BlueEliminated");
            AddText(parent, "blue_flag_label", Space.TopCenter, 0, 0, "BlueFlagLabel");

            // 任务说明条
            RectTransform task = NewRect("TaskBar", parent);
            Stretch(task);
            AddSprite(task, s_Sprites["hud_taskbar"], Space.TopCenter, "Bg");
            AddSprite(task, s_Sprites["hud_task_icon"], Space.TopCenter, "IconBg");
            AddText(task, "task_icon", Space.TopCenter, 0, 0, "Icon");
            TMP_Text title = AddText(task, "task_title", Space.TopCenter, 0, 0, "Title");
            TMP_Text body = AddText(task, "task_body", Space.TopCenter, 0, 0, "Body");

            // 计时牌、旗帜、文字要盖在比分条（自带 Canvas 和特效）上方，所以单独成层，层级由 UwbScoreBar 统一排布
            var topCanvas = parent.gameObject.AddComponent<Canvas>();
            topCanvas.overrideSorting = true;
            var barSo = new SerializedObject(scoreBar);
            barSo.FindProperty("topCanvas").objectReferenceValue = topCanvas;
            barSo.ApplyModifiedPropertiesWithoutUndo();

            var view = parent.gameObject.AddComponent<UwbHudTopBar>();
            var so = new SerializedObject(view);
            so.FindProperty("scoreBar").objectReferenceValue = scoreBar;
            so.FindProperty("timerText").objectReferenceValue = timer;
            so.FindProperty("redScoreText").objectReferenceValue = redScore;
            so.FindProperty("blueScoreText").objectReferenceValue = blueScore;
            so.FindProperty("redEliminatedText").objectReferenceValue = redOut;
            so.FindProperty("blueEliminatedText").objectReferenceValue = blueOut;
            so.FindProperty("taskTitleText").objectReferenceValue = title;
            so.FindProperty("taskBodyText").objectReferenceValue = body;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static void BuildBottomBar(RectTransform parent, out UwbPlayerCardItem[] red, out UwbPlayerCardItem[] blue)
        {
            AddSprite(parent, s_Sprites["hud_bottom_bg"], Space.BottomCenter, "Bg");

            LCard card = s_Layout.card;
            LSprite redBg = s_Sprites["hud_card_bg_red"];
            LSprite blueBg = s_Sprites["hud_card_bg_blue"];
            red = new UwbPlayerCardItem[card.count];
            blue = new UwbPlayerCardItem[card.count];
            for (int i = 0; i < card.count; i++)
                red[i] = BuildCard(parent, "RedCard_" + (i + 1), true, redBg.x + card.step * i, redBg.y);
            for (int i = 0; i < card.count; i++)
                blue[i] = BuildCard(parent, "BlueCard_" + (i + 1), false, blueBg.x + card.step * i, blueBg.y);

            AddSprite(parent, s_Sprites["hud_bottom_frame"], Space.BottomCenter, "Frame");
        }

        // slotX/slotY：本卡片左上角在画布上的 PSD 坐标。切图相对本队首张卡片取位置，文字统一取红队首张卡片的相对位置。
        static UwbPlayerCardItem BuildCard(RectTransform parent, string name, bool isRed, int slotX, int slotY)
        {
            string side = isRed ? "red" : "blue";
            LSprite bg = s_Sprites["hud_card_bg_" + side];
            LSprite border = s_Sprites["hud_card_border_" + side];
            LSprite badge = s_Sprites["hud_card_badge_" + side];
            LSprite redBg = s_Sprites["hud_card_bg_red"];
            int tx = redBg.x;
            int ty = redBg.y;
            int sx = bg.x;
            int sy = bg.y;

            RectTransform root = NewRect(name, parent);
            Place(root, Space.BottomCenter, slotX, slotY, bg.w, bg.h);
            RectTransform content = NewRect("Content", root);
            Stretch(content);
            var group = content.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            AddSprite(content, bg, Space.Local, "Bg", sx, sy);
            AddSprite(content, border, Space.Local, "Border", sx, sy);
            AddSprite(content, badge, Space.Local, "Badge", sx, sy);

            RectTransform statsRow = NewRect("StatsRow", content);
            Stretch(statsRow);
            AddText(statsRow, "card_kill_label", Space.Local, tx, ty, "KillLabel");
            TMP_Text nameText = AddText(content, "card_name", Space.Local, tx, ty, "Name");
            TMP_Text number = AddTextInRect(content, "card_number", badge, sx, sy, "Number");
            TMP_Text kills = AddText(statsRow, "card_kills", Space.Local, tx, ty, "Kills");
            AddText(statsRow, "card_death_label", Space.Local, tx, ty, "DeathLabel");
            TMP_Text deaths = AddText(statsRow, "card_deaths", Space.Local, tx, ty, "Deaths");

            LText kl = s_Texts["card_kill_label"];
            LText dl = s_Texts["card_death_label"];
            TMP_Text tip = AddTextRaw(content, "DeadTip", kl, Space.Local,
                kl.bbox[0] - tx, kl.bbox[1] - 14 - ty, dl.bbox[2] - kl.bbox[0] + 40, kl.bbox[3] - kl.bbox[1] + 28, "left", s_Chinese,
                kl.fontSize * 1.1f, ParseColor(kl.color));
            tip.gameObject.SetActive(false);

            LRect av = s_Layout.card.avatar;
            UwbAvatarView avatar = BuildAvatar(content, av, tx, ty);

            var item = root.gameObject.AddComponent<UwbPlayerCardItem>();
            var so = new SerializedObject(item);
            so.FindProperty("avatarView").objectReferenceValue = avatar;
            so.FindProperty("numberText").objectReferenceValue = number;
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("killsText").objectReferenceValue = kills;
            so.FindProperty("deathsText").objectReferenceValue = deaths;
            so.FindProperty("statsRow").objectReferenceValue = statsRow.gameObject;
            so.FindProperty("deadTipText").objectReferenceValue = tip;
            so.FindProperty("contentGroup").objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        // 头像框 + 默认头像 + 圆形头像，素材和比例取自 ScoreCardUnity：
        // 头像框 144，圆形头像 111，默认头像 92x77（偏下 3.5）。PSD 里头像环外径 81 对应头像框里金环外径 128/144。
        static UwbAvatarView BuildAvatar(RectTransform parent, LRect av, float ox, float oy)
        {
            const float FrameSize = 144f;
            const float RingSize = 128f;
            float frame = av.w * FrameSize / RingSize;
            float cx = av.x - ox + av.w * Half;
            float cy = av.y - oy + av.h * Half;

            RectTransform root = NewRect("Avatar", parent);
            Place(root, Space.Local, cx - frame * Half, cy - frame * Half, frame, frame);
            float k = frame / FrameSize * Half;

            Image frameImg = AddAvatarImage(root, "HeadFrame", AvatarFrameFile, FrameSize * k, FrameSize * k, 0f);
            Image defaultImg = AddAvatarImage(root, "HeadIconDefault", AvatarDefaultFile, 92f * k, 77f * k, -3.5f * k);
            Image head = AddAvatarImage(root, "HeadIcon", null, 111f * k, 111f * k, 0f);
            head.material = AssetDatabase.LoadAssetAtPath<Material>(CircleMatPath);
            head.enabled = false;

            var view = root.gameObject.AddComponent<UwbAvatarView>();
            var so = new SerializedObject(view);
            so.FindProperty("headIcon").objectReferenceValue = head;
            so.FindProperty("headIconDefault").objectReferenceValue = defaultImg.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static Image AddAvatarImage(RectTransform parent, string name, string file, float w, float h, float y)
        {
            RectTransform rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(w, h);
            var img = rt.gameObject.AddComponent<Image>();
            if (file != null)
                img.sprite = LoadSprite(file);
            img.raycastTarget = false;
            return img;
        }

        static UwbPlayerTagItem BuildTagTemplate(RectTransform parent)
        {
            LSprite border = s_TagSprites["hud_tag_border"];
            int ox = border.x;
            int oy = border.y;

            RectTransform root = NewRect("TagTemplate", parent);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0f);
            root.sizeDelta = new Vector2(border.w * Half, border.h * Half);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            AddSprite(root, s_TagSprites["hud_tag_bg"], Space.Local, "Bg", ox, oy);
            AddSprite(root, border, Space.Local, "Border", ox, oy);
            TMP_Text name = AddTagText(root, "name", "Name", ox, oy, "left", true);
            AddSprite(root, s_TagSprites["hud_tag_hp_track"], Space.Local, "HpTrack", ox, oy);
            Image fill = AddSprite(root, s_TagSprites["hud_tag_hp_fill"], Space.Local, "HpFill", ox, oy);
            LRect av = s_Layout.tag.avatar;
            UwbAvatarView avatar = BuildAvatar(root, av, ox, oy);
            LSprite badge = s_TagSprites["hud_tag_badge"];
            AddSprite(root, badge, Space.Local, "Badge", ox, oy);
            TMP_Text number = AddTagTextInRect(root, "number", badge, ox, oy, "Number");

            var item = root.gameObject.AddComponent<UwbPlayerTagItem>();
            var so = new SerializedObject(item);
            so.FindProperty("avatarView").objectReferenceValue = avatar;
            so.FindProperty("hpFill").objectReferenceValue = fill.rectTransform;
            so.FindProperty("hpFillImage").objectReferenceValue = fill;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("numberText").objectReferenceValue = number;
            so.FindProperty("group").objectReferenceValue = group;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.gameObject.SetActive(false);
            return item;
        }

        #endregion

        #region 元素创建

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // x,y,w,h 是 PSD 像素。TopCenter/BottomCenter 的 x,y 是整张画布上的坐标；Local 是相对父节点左上角。
        static void Place(RectTransform rt, Space space, float x, float y, float w, float h)
        {
            switch (space)
            {
                case Space.TopCenter:
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2((x - CanvasW * Half) * Half, -y * Half);
                    break;
                case Space.BottomCenter:
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                    rt.anchoredPosition = new Vector2((x - CanvasW * Half) * Half, (CanvasH - (y + h)) * Half);
                    break;
                default:
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    rt.anchoredPosition = new Vector2(x * Half, -y * Half);
                    break;
            }

            rt.sizeDelta = new Vector2(w * Half, h * Half);
        }

        static Sprite LoadSprite(string file)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + "/" + file);
            if (sprite == null)
                throw new FileNotFoundException("切图未导入为 Sprite: " + file);
            return sprite;
        }

        static Image AddSprite(Transform parent, LSprite s, Space space, string name, float ox = 0f, float oy = 0f)
        {
            Image img = AddRaw(parent, name, s.file, space, s.x - ox, s.y - oy, s.w, s.h);
            bool sliced = s.slice != null && s.slice.Length == 4 && (s.slice[0] + s.slice[1] + s.slice[2] + s.slice[3]) > 0;
            img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            // 切图 PPU=2，Canvas 参考 PPU=100，九宫格边距要乘 100 才是「PSD 像素 / 2」
            img.pixelsPerUnitMultiplier = 100f;
            return img;
        }

        static Image AddRaw(Transform parent, string name, string file, Space space, float x, float y, float w, float h)
        {
            RectTransform rt = NewRect(name, parent);
            Place(rt, space, x, y, w, h);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = LoadSprite(file);
            img.raycastTarget = false;
            img.preserveAspect = false;
            return img;
        }

        static TMP_Text AddText(Transform parent, string key, Space space, float ox, float oy, string name, string alignOverride = null)
        {
            return AddTextCore(parent, s_Texts[key], space, ox, oy, name, alignOverride, false);
        }

        static TMP_Text AddTagText(Transform parent, string key, string name, float ox, float oy, string alignOverride, bool outline)
        {
            return AddTextCore(parent, s_TagTexts[key], Space.Local, ox, oy, name, alignOverride, outline);
        }

        // 文字放在指定图形的矩形里居中（编号、血量这类要随位数变化的角标）
        static TMP_Text AddTextInRect(Transform parent, string key, LSprite host, float ox, float oy, string name)
        {
            LText t = s_Texts[key];
            return AddTextRaw(parent, name, t, Space.Local, host.x - ox, host.y - oy, host.w, host.h, "center", FontFor(t), t.fontSize, ParseColor(t.color), false);
        }

        static TMP_Text AddTagTextInRect(Transform parent, string key, LSprite host, float ox, float oy, string name)
        {
            LText t = s_TagTexts[key];
            return AddTextRaw(parent, name, t, Space.Local, host.x - ox, host.y - oy, host.w, host.h, "center", FontFor(t), t.fontSize, ParseColor(t.color), false);
        }

        static TMP_Text AddTextCore(Transform parent, LText t, Space space, float ox, float oy, string name, string alignOverride, bool outline)
        {
            const float PadX = 24f;
            const float PadY = 14f;
            string align = alignOverride ?? t.align;
            float bx = t.bbox[0], by = t.bbox[1], bw = t.bbox[2] - t.bbox[0], bh = t.bbox[3] - t.bbox[1];
            float rw = bw + PadX * 2f;
            float rx = align == "left" ? bx : align == "right" ? bx + bw - rw : bx - PadX;

            return AddTextRaw(parent, name, t, space, rx - ox, by - PadY - oy, rw, bh + PadY * 2f, align, FontFor(t), t.fontSize, ParseColor(t.color), outline);
        }

        static TMP_Text AddTextRaw(Transform parent, string name, LText style, Space space, float x, float y, float w, float h,
            string align, TMP_FontAsset font, float fontSizePsd, Color color, bool outline = false)
        {
            RectTransform rt = NewRect(name, parent);
            Place(rt, space, x, y, w, h);
            var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            // Impact 的数字比 PSD 里渲染出来的略宽，整体收一点让宽高都接近设计稿
            text.fontSize = fontSizePsd * Half * (font == s_Impact ? ImpactScale : 1f);
            text.color = color;
            text.raycastTarget = false;
            text.enableWordWrapping = false;
            text.overflowMode = outline ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
            text.richText = false;
            text.characterSpacing = style != null ? style.tracking / 10f : 0f;
            text.fontStyle = StyleFor(style);
            text.alignment = align == "center" ? TextAlignmentOptions.Center
                : align == "right" ? TextAlignmentOptions.Right
                : TextAlignmentOptions.Left;
            if (outline)
                text.fontSharedMaterial = s_TagNameMat;
            text.text = style != null ? style.text : "";
            return text;
        }

        static TMP_FontAsset FontFor(LText t)
        {
            return t.font != null && t.font.IndexOf("Impact", StringComparison.OrdinalIgnoreCase) >= 0 ? s_Impact : s_Chinese;
        }

        // 项目里只有 PuHuiTi 75 SemiBold，PSD 的 Black / Heavy 用加粗近似
        static FontStyles StyleFor(LText t)
        {
            if (t == null || t.font == null)
                return FontStyles.Normal;
            return t.font.Contains("Black") || t.font.Contains("Heavy") ? FontStyles.Bold : FontStyles.Normal;
        }

        static Color ParseColor(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.white;
        }

        static void SetArray(SerializedProperty prop, UnityEngine.Object[] items)
        {
            prop.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        #endregion
    }
}
#endif
