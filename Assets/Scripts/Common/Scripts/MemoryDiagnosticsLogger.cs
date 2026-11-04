using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// 长时间挂机内存诊断采样器：每隔固定时间记录一次 Native Heap / Mono Heap 总量、
/// 进程级 PSS 分类（等价 adb dumpsys meminfo，用 Debug.getMemoryInfo 在进程内直接读取，
/// 不需要电脑连着跑 adb），以及几个已知高嫌疑缓存（头像纹理、语音克隆 PCM、特效对象池）
/// 的条目数，追加写入 <see cref="Application.persistentDataPath"/> 下的 CSV 文件。
///
/// 背景：Unity 自己的 Profiler/Native Heap 读数看不到 dumpsys 里的 Private Other
/// （多数机型上是显存/图形驱动分配），本脚本把两边的数据采到同一份 CSV 里，
/// 事后用 Tools/analyze_memory_diagnostics.py 对比增长曲线，
/// 哪个计数和 Native Heap/PSS 同斜率增长，哪个就是泄漏点。
///
/// 通过 <see cref="RuntimeInitializeOnLoadMethodAttribute"/> 自动挂载，不需要拖进任何场景，
/// App 启动后全程静默运行，不产生任何 UI，只写文件。
///
/// 日志位置（Android）：/sdcard/Android/data/&lt;包名&gt;/files/MemoryLogs/mem_yyyyMMdd_HHmmss.csv
/// 取回方式：adb pull /sdcard/Android/data/&lt;包名&gt;/files/MemoryLogs ./MemoryLogs
/// </summary>
public class MemoryDiagnosticsLogger : MonoBehaviour
{
    /// <summary>采样间隔（秒）。长时间挂机诊断不需要太密，5 分钟足够看出线性趋势，又不会把文件写太大。</summary>
    const float SampleIntervalSeconds = 300f;

    /// <summary>
    /// 是否顺带用 Resources.FindObjectsOfTypeAll 扫描 Texture2D/Sprite/AudioClip 等对象计数。
    /// 这一步有一次全量遍历开销，在 S4 实时演出画面偶尔可能引起单帧卡顿；
    /// 如果发现明显掉帧，把这里改成 false，退化为只看 Native/Mono 总量与几个缓存计数。
    /// </summary>
    const bool ScanUnityObjectCounts = true;

    /// <summary>每隔多少次采样导出一次 Material/Mesh/Texture2D 按名字分组的 Top 明细（12*5分钟=1小时一次）。
    /// materialCount/meshCount 只能看到总数在涨，看不到具体是哪个资源，这里补上明细方便定位。</summary>
    const int TopNamesDumpEverySamples = 12;

    string m_LogFilePath;
    float m_StartRealtime;
    int m_SampleCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (FindObjectOfType<MemoryDiagnosticsLogger>() != null)
        {
            return;
        }

        var go = new GameObject(nameof(MemoryDiagnosticsLogger));
        DontDestroyOnLoad(go);
        go.AddComponent<MemoryDiagnosticsLogger>();
    }

    void Awake()
    {
        m_StartRealtime = Time.realtimeSinceStartup;

        try
        {
            string dir = Path.Combine(Application.persistentDataPath, "MemoryLogs");
            Directory.CreateDirectory(dir);
            m_LogFilePath = Path.Combine(dir, $"mem_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            File.WriteAllText(m_LogFilePath, BuildHeader() + "\n", Encoding.UTF8);
            Debug.Log($"[MemoryDiagnosticsLogger] 开始记录内存诊断日志: {m_LogFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 创建日志文件失败，诊断已停用: {e.Message}");
            enabled = false;
            return;
        }

        StartCoroutine(SampleLoop());
    }

    static string BuildHeader() =>
        "timestamp,elapsedHours,stage," +
        "nativeHeapAllocMB,nativeHeapSizeMB," +
        "monoUsedMB,monoHeapMB,totalReservedMB,totalAllocMB,gfxDriverMB," +
        "pssTotalMB,pssPrivateOtherMB,pssGraphicsMB,pssSystemMB," +
        "tex2DCount,tex2DMemMB,spriteCount,audioClipCount,audioClipMemMB," +
        "materialCount,meshCount,renderTextureCount," +
        "webImgCache,webSquareImgCache,webSquareSpriteCache," +
        "voiceCloneCacheCount,voiceCloneCacheMB," +
        "effectResCount,effectCacheInstances,gcGen0Count";

    IEnumerator SampleLoop()
    {
        // 启动即采一条基线，之后按固定间隔采样，方便跟后面的样本算增量。
        WriteSample();

        var wait = new WaitForSecondsRealtime(SampleIntervalSeconds);
        while (true)
        {
            yield return wait;
            WriteSample();
        }
    }

    void WriteSample()
    {
        try
        {
            File.AppendAllText(m_LogFilePath, BuildSampleLine() + "\n", Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 写入采样失败: {e.Message}");
        }

        ++m_SampleCount;
        // 第一次采样也顺带导出一次：一是刚装上就能验证这个文件会不会生成，不用等一小时；
        // 二是后面每隔 TopNamesDumpEverySamples 次刷新一次看长期趋势。
        if (ScanUnityObjectCounts && (m_SampleCount == 1 || m_SampleCount % TopNamesDumpEverySamples == 0))
        {
            DumpTopObjectNames();
        }
    }

    /// <summary>按名字对 Material/Mesh/Texture2D 分组计数，取涨得最多的几个写入单独文件（每次覆盖，只留最新一份）。</summary>
    void DumpTopObjectNames()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"采样时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AppendTopNames(sb, "Material", Resources.FindObjectsOfTypeAll<Material>());
            AppendTopNames(sb, "Mesh", Resources.FindObjectsOfTypeAll<Mesh>());
            AppendTopNames(sb, "Texture2D", Resources.FindObjectsOfTypeAll<Texture2D>());

            string path = Path.Combine(Path.GetDirectoryName(m_LogFilePath), "top_object_names_latest.txt");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 导出对象名称明细失败: {e.Message}");
        }
    }

    static void AppendTopNames<T>(StringBuilder sb, string label, T[] objects) where T : UnityEngine.Object
    {
        var counts = new Dictionary<string, int>();
        foreach (var obj in objects)
        {
            string name = string.IsNullOrEmpty(obj.name) ? "(未命名)" : obj.name;
            counts.TryGetValue(name, out int c);
            counts[name] = c + 1;
        }

        var top = new List<KeyValuePair<string, int>>(counts);
        top.Sort((a, b) => b.Value.CompareTo(a.Value));
        if (top.Count > 15) top.RemoveRange(15, top.Count - 15);

        sb.AppendLine($"--- {label} 总数 {objects.Length}，按名字分组 Top {top.Count} ---");
        foreach (var kv in top)
        {
            sb.AppendLine($"{kv.Value,6}  {kv.Key}");
        }
        sb.AppendLine();
    }

    string BuildSampleLine()
    {
        GetNativeHeap(out float nativeAllocMB, out float nativeSizeMB);

        float monoUsedMB = Profiler.GetMonoUsedSizeLong() / 1048576f;
        float monoHeapMB = Profiler.GetMonoHeapSizeLong() / 1048576f;
        float totalReservedMB = Profiler.GetTotalReservedMemoryLong() / 1048576f;
        float totalAllocMB = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
        float gfxDriverMB = Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576f;

        GetProcessPssStat(out float pssTotalMB, out float pssPrivateOtherMB, out float pssGraphicsMB, out float pssSystemMB);

        int tex2DCount = 0, spriteCount = 0, audioClipCount = 0;
        int materialCount = 0, meshCount = 0, renderTextureCount = 0;
        float tex2DMemMB = 0f, audioClipMemMB = 0f;
        if (ScanUnityObjectCounts)
        {
            ScanUnityObjects(
                out tex2DCount, out tex2DMemMB,
                out spriteCount,
                out audioClipCount, out audioClipMemMB,
                out materialCount, out meshCount, out renderTextureCount);
        }

        GetWebImageCacheCounts(out int webImg, out int webSquareImg, out int webSquareSprite);
        GetVoiceCloneCacheStats(out int voiceCloneCount, out float voiceCloneMB);
        GetEffectPoolCounts(out int effectResCount, out int effectCacheInstances);

        float elapsedHours = (Time.realtimeSinceStartup - m_StartRealtime) / 3600f;

        return string.Join(",",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            elapsedHours.ToString("F3", CultureInfo.InvariantCulture),
            SafeGetStage(),
            nativeAllocMB.ToString("F1", CultureInfo.InvariantCulture),
            nativeSizeMB.ToString("F1", CultureInfo.InvariantCulture),
            monoUsedMB.ToString("F1", CultureInfo.InvariantCulture),
            monoHeapMB.ToString("F1", CultureInfo.InvariantCulture),
            totalReservedMB.ToString("F1", CultureInfo.InvariantCulture),
            totalAllocMB.ToString("F1", CultureInfo.InvariantCulture),
            gfxDriverMB.ToString("F1", CultureInfo.InvariantCulture),
            pssTotalMB.ToString("F1", CultureInfo.InvariantCulture),
            pssPrivateOtherMB.ToString("F1", CultureInfo.InvariantCulture),
            pssGraphicsMB.ToString("F1", CultureInfo.InvariantCulture),
            pssSystemMB.ToString("F1", CultureInfo.InvariantCulture),
            tex2DCount.ToString(CultureInfo.InvariantCulture),
            tex2DMemMB.ToString("F1", CultureInfo.InvariantCulture),
            spriteCount.ToString(CultureInfo.InvariantCulture),
            audioClipCount.ToString(CultureInfo.InvariantCulture),
            audioClipMemMB.ToString("F1", CultureInfo.InvariantCulture),
            materialCount.ToString(CultureInfo.InvariantCulture),
            meshCount.ToString(CultureInfo.InvariantCulture),
            renderTextureCount.ToString(CultureInfo.InvariantCulture),
            webImg.ToString(CultureInfo.InvariantCulture),
            webSquareImg.ToString(CultureInfo.InvariantCulture),
            webSquareSprite.ToString(CultureInfo.InvariantCulture),
            voiceCloneCount.ToString(CultureInfo.InvariantCulture),
            voiceCloneMB.ToString("F1", CultureInfo.InvariantCulture),
            effectResCount.ToString(CultureInfo.InvariantCulture),
            effectCacheInstances.ToString(CultureInfo.InvariantCulture),
            GC.CollectionCount(0).ToString(CultureInfo.InvariantCulture));
    }

    static void GetNativeHeap(out float allocMB, out float sizeMB)
    {
        allocMB = 0f;
        sizeMB = 0f;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var debugClass = new AndroidJavaClass("android.os.Debug");
            allocMB = debugClass.CallStatic<long>("getNativeHeapAllocatedSize") / 1048576f;
            sizeMB = debugClass.CallStatic<long>("getNativeHeapSize") / 1048576f;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 读取 Native Heap 失败: {e.Message}");
        }
#endif
    }

    /// <summary>
    /// 读取跟 adb shell dumpsys meminfo 的 App Summary 一样的 PSS 分类（Android Debug.getMemoryInfo，
    /// 进程查自己无需任何权限）。之前发现 App 内 Native Heap/Unity Profiler 已经走平，
    /// 但外部 dumpsys 里的 Private Other（多数机型上是显存/图形驱动分配）还在缓慢涨，
    /// 且看不到是哪个环节导致，所以直接在进程内采集，不用再靠电脑常驻跑 adb 轮询。
    /// </summary>
    static void GetProcessPssStat(out float totalMB, out float privateOtherMB, out float graphicsMB, out float systemMB)
    {
        totalMB = 0f; privateOtherMB = 0f; graphicsMB = 0f; systemMB = 0f;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var debugClass = new AndroidJavaClass("android.os.Debug");
            using var memInfo = new AndroidJavaObject("android.os.Debug$MemoryInfo");
            debugClass.CallStatic("getMemoryInfo", memInfo);

            totalMB = ReadMemoryStatKB(memInfo, "summary.total-pss") / 1024f;
            privateOtherMB = ReadMemoryStatKB(memInfo, "summary.private-other") / 1024f;
            graphicsMB = ReadMemoryStatKB(memInfo, "summary.graphics") / 1024f;
            systemMB = ReadMemoryStatKB(memInfo, "summary.system") / 1024f;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 读取进程 PSS 分类失败: {e.Message}");
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static long ReadMemoryStatKB(AndroidJavaObject memInfo, string statName)
    {
        string value = memInfo.Call<string>("getMemoryStat", statName);
        return long.TryParse(value, out long kb) ? kb : 0L;
    }
#endif

    static void ScanUnityObjects(
        out int tex2DCount, out float tex2DMemMB,
        out int spriteCount,
        out int audioClipCount, out float audioClipMemMB,
        out int materialCount, out int meshCount, out int renderTextureCount)
    {
        tex2DCount = 0; tex2DMemMB = 0f;
        spriteCount = 0;
        audioClipCount = 0; audioClipMemMB = 0f;
        materialCount = 0; meshCount = 0; renderTextureCount = 0;

        try
        {
            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            tex2DCount = textures.Length;
            long texBytes = 0;
            foreach (var t in textures) texBytes += Profiler.GetRuntimeMemorySizeLong(t);
            tex2DMemMB = texBytes / 1048576f;

            spriteCount = Resources.FindObjectsOfTypeAll<Sprite>().Length;

            var clips = Resources.FindObjectsOfTypeAll<AudioClip>();
            audioClipCount = clips.Length;
            long clipBytes = 0;
            foreach (var c in clips) clipBytes += Profiler.GetRuntimeMemorySizeLong(c);
            audioClipMemMB = clipBytes / 1048576f;

            materialCount = Resources.FindObjectsOfTypeAll<Material>().Length;
            meshCount = Resources.FindObjectsOfTypeAll<Mesh>().Length;
            renderTextureCount = Resources.FindObjectsOfTypeAll<RenderTexture>().Length;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 扫描 Unity 对象计数失败: {e.Message}");
        }
    }

    static void GetWebImageCacheCounts(out int imageCache, out int squareImageCache, out int squareSpriteCache)
    {
        imageCache = 0; squareImageCache = 0; squareSpriteCache = 0;
        try
        {
            if (!LetsPiu.Main.Common.Scripts.WebImageLoader.HasInstance) return;
            var counts = LetsPiu.Main.Common.Scripts.WebImageLoader.Instance.GetCacheDebugCounts();
            imageCache = counts.imageCache;
            squareImageCache = counts.squareImageCache;
            squareSpriteCache = counts.squareSpriteCache;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 读取头像缓存计数失败: {e.Message}");
        }
    }

    static void GetVoiceCloneCacheStats(out int count, out float mb)
    {
        count = 0; mb = 0f;
        try
        {
            if (!XFVoiceCloneService.HasInstance) return;
            var stats = XFVoiceCloneService.Instance.GetCacheDebugStats();
            count = stats.count;
            mb = stats.bytes / 1048576f;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 读取语音克隆缓存计数失败: {e.Message}");
        }
    }

    // EffectPoolMgr（Assets/Effect/Scripts，无 asmdef，落在默认 Assembly-CSharp）和
    // ScoreCardScreenHelper（LetsPiu.UI 程序集）都不在本脚本所在的 LetsPiu.Main 程序集的
    // 引用范围内，且 LetsPiu.UI 已经反向引用了 LetsPiu.Main，不能再加一条引用形成循环依赖，
    // 所以这两处用反射跨程序集读取，只在采样时用一次，找不到就跳过，不影响主流程。

    static void GetEffectPoolCounts(out int resCount, out int cacheInstances)
    {
        resCount = 0; cacheInstances = 0;
        try
        {
            Type type = FindType("LetsPiu.Base.EffectPoolMgr");
            // S 属性定义在基类 SingletonBase<T> 上，反射查找继承的静态成员必须加 FlattenHierarchy，否则一直返回 null。
            object instance = type?.GetProperty("S", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
            object counts = instance?.GetType()
                .GetMethod("GetDebugCounts", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(instance, null);
            if (counts == null) return;

            Type tupleType = counts.GetType();
            resCount = (int)tupleType.GetField("Item1").GetValue(counts);
            cacheInstances = (int)tupleType.GetField("Item2").GetValue(counts);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MemoryDiagnosticsLogger] 读取特效池计数失败: {e.Message}");
        }
    }

    static string SafeGetStage()
    {
        try
        {
            Type type = FindType("LetsPiu.UI.ScoreCardCommon.ScoreCardScreenHelper");
            object value = type?.GetProperty("LastKnownStageKey", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
            return value as string ?? "";
        }
        catch
        {
            return "";
        }
    }

    static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = asm.GetType(fullName);
            if (type != null) return type;
        }

        return null;
    }
}
