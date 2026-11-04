# Import Settings Tool (Ultimate • Folder Fix)

这是一套**最终整合版**：
- ✅ 预设管理（含“无预设”）
- ✅ JSON 导入/导出（跨项目共享，不会自动应用）
- ✅ **使用选中文件夹（已修复：实时更新路径）**
- ✅ 关键字搜索 + 结果列表
- ✅ 批量应用（Texture / Model / Audio）
- ✅ 刷新检查（弹窗列出差异）
- ✅ 平台覆盖（Default/Android/iPhone）
- ✅ Inspector 风格 UI + 间距修复（路径/关键字标签更贴近输入）
- ✅ AntiAliasing 防御（避免 SRP 报错刷屏）

## 放置路径
将整个文件夹放到：`Assets/Editor/ImportSettingsTool/`  
菜单打开：**Tools → Import Settings Tool (Ultimate)**

## 使用步骤
1. 勾选 **使用选中文件夹**，然后在 Project 窗口选中要处理的**文件夹**（上方“路径”会实时显示）。  
2. 输入 **关键字**（支持 Unity `FindAssets` 语法：`t:Texture` / `t:Model` / `t:AudioClip`），点 **搜索**。  
3. 在下面的 **设置面板** 勾选需要修改的字段并设置具体值。  
4. 点击 **应用修改** 进行批量设置，或点 **刷新检查** 查看差异。  
5. 通过 **保存为新预设**、**导出/导入 JSON** 做团队与跨项目复用。

## 预设路径
默认保存到：`Assets/Editor/ImportSettingsTool/ImportSettingsPresets/`

## 自定义/扩展
如需更多字段（Sprite 导入、读写开关、法线贴图类型、模型动画细化等），在 `ImportSettingsPreset.cs` 添加字段，并在 `ImportSettingsToolUltimate.cs` 中同步：
- `DrawSettingsInspector`（绘制 UI）
- `Apply*Settings`（写回 Importer）
- `*DiffLines`（刷新检查）

---
Made for smooth pipelines.