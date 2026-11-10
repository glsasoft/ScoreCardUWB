# RemoveSubParticle.ps1
# Removes the NewSubParticleSystem MonoBehaviour from a Unity project.
# Works on stock Windows PowerShell 5.1 (no Python needed).
#
# Usage (close Unity first):
#   powershell -NoProfile -ExecutionPolicy Bypass -File RemoveNewSubParticleSystem.ps1 --dry-run
#   powershell -NoProfile -ExecutionPolicy Bypass -File RemoveNewSubParticleSystem.ps1
#   ... --root D:\SomeProject
#   ... --no-delete-script
#   ... --deep
#
# Or just double-click RemoveNewSubParticleSystem-ps.bat

$ErrorActionPreference = "Stop"
try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$Root = ""
$Guid = ""
$DryRun = $false
$NoBackup = $false
$NoDeleteScript = $false
$Deep = $false

for ($i = 0; $i -lt $args.Count; $i++) {
    $a = [string]$args[$i]
    switch ($a) {
        "--dry-run"          { $DryRun = $true }
        "--no-backup"        { $NoBackup = $true }
        "--no-delete-script" { $NoDeleteScript = $true }
        "--deep"             { $Deep = $true }
        "--root"             { $i++; $Root = [string]$args[$i] }
        "--guid"             { $i++; $Guid = [string]$args[$i] }
        "--help"             { Write-Host "args: --root <path> --guid <guid> --dry-run --no-backup --no-delete-script --deep"; exit 0 }
        default              { Write-Host "unknown arg: $a" }
    }
}

if (-not $Root) {
    $Root = Split-Path -Parent $PSScriptRoot
}
$Root = (Resolve-Path -LiteralPath $Root).Path

if (-not $Guid) {
    $meta = Join-Path $Root "Assets\ParticleSystemExpand\Mono\NewSubParticleSystem.cs.meta"
    if (Test-Path -LiteralPath $meta) {
        foreach ($line in [System.IO.File]::ReadAllLines($meta)) {
            if ($line.StartsWith("guid:")) { $Guid = $line.Substring(5).Trim(); break }
        }
    }
}
if (-not $Guid) { $Guid = "f8aa87937116feb41b82b0ab99b5f943" }

$code = @'
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class SubCleaner
{
    static readonly string[] Exts = new string[] { ".prefab", ".unity", ".asset", ".controller", ".mat", ".playable", ".anim", ".overridecontroller" };
    static readonly Regex DocRe = new Regex(@"^--- !u!\d+ &(-?\d+)");
    static readonly Regex CompRe = new Regex(@"^\s*-\s*component:\s*\{fileID:\s*(-?\d+)\s*\}\s*$");
    static readonly Regex ItemRe = new Regex(@"^(\s*)- ");
    static readonly Regex AddedRe = new Regex(@"^\s*addedObject:\s*\{fileID:\s*(-?\d+)\s*\}");

    static bool IsAsset(string path)
    {
        string e = Path.GetExtension(path).ToLowerInvariant();
        for (int i = 0; i < Exts.Length; i++) if (Exts[i] == e) return true;
        return false;
    }

    static List<string> SplitKeepEnds(string text)
    {
        List<string> list = new List<string>();
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                list.Add(text.Substring(start, i - start + 1));
                start = i + 1;
            }
        }
        if (start < text.Length) list.Add(text.Substring(start));
        return list;
    }

    static List<string> StripScriptDocs(List<string> lines, string guid, out int removed)
    {
        removed = 0;
        List<string> outList = new List<string>(lines.Count);
        int i = 0, n = lines.Count;
        while (i < n)
        {
            string ln = lines[i];
            if (ln.StartsWith("--- "))
            {
                int j = i + 1;
                while (j < n && !lines[j].StartsWith("--- ")) j++;
                StringBuilder sb = new StringBuilder();
                for (int k = i; k < j; k++) sb.Append(lines[k]);
                string body = sb.ToString();
                if (body.IndexOf(guid, StringComparison.Ordinal) >= 0 && body.IndexOf("m_Script:", StringComparison.Ordinal) >= 0)
                {
                    removed++;
                    i = j;
                    continue;
                }
                for (int k = i; k < j; k++) outList.Add(lines[k]);
                i = j;
                continue;
            }
            outList.Add(ln);
            i++;
        }
        return outList;
    }

    static HashSet<string> CollectDocIds(List<string> lines)
    {
        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < lines.Count; i++)
        {
            Match m = DocRe.Match(lines[i]);
            if (m.Success) ids.Add(m.Groups[1].Value);
        }
        return ids;
    }

    static List<string> StripComponentSlots(List<string> lines, out int dropped)
    {
        dropped = 0;
        HashSet<string> ids = CollectDocIds(lines);
        List<string> outList = new List<string>(lines.Count);
        for (int i = 0; i < lines.Count; i++)
        {
            Match m = CompRe.Match(lines[i]);
            if (m.Success && !ids.Contains(m.Groups[1].Value))
            {
                dropped++;
                continue;
            }
            outList.Add(lines[i]);
        }
        return outList;
    }

    static List<string> StripAddedComponents(List<string> lines, out int removed)
    {
        removed = 0;
        HashSet<string> ids = CollectDocIds(lines);
        List<string> outList = new List<string>(lines.Count);
        int itemStart = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            string ln = lines[i];
            Match m = ItemRe.Match(ln);
            if (m.Success)
            {
                int lead = ln.Length - ln.TrimStart().Length;
                if (lead == 4) itemStart = outList.Count;
            }
            outList.Add(ln);
            Match am = AddedRe.Match(ln);
            if (am.Success && !ids.Contains(am.Groups[1].Value))
            {
                if (itemStart >= 0 && itemStart < outList.Count)
                {
                    outList.RemoveRange(itemStart, outList.Count - itemStart);
                    removed++;
                    itemStart = -1;
                }
            }
        }
        return outList;
    }

    public static int Run(string root, string guid, bool dryRun, bool noBackup, bool noDelete, bool deep)
    {
        string assets = Path.Combine(root, "Assets");
        if (!Directory.Exists(assets))
        {
            Console.WriteLine("ERROR: not a Unity project (no Assets folder): " + root);
            return 2;
        }

        Console.WriteLine("project : " + root);
        Console.WriteLine("guid    : " + guid);
        Console.WriteLine("mode    : " + (dryRun ? "dry run" : "writing changes"));

        Encoding latin1 = Encoding.GetEncoding(28591);
        Encoding utf8 = new UTF8Encoding(false);

        string backupDir = null;
        if (!dryRun && !noBackup)
        {
            backupDir = Path.Combine(root, "_cleanup_backup", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        List<string> files = new List<string>();
        foreach (string f in Directory.GetFiles(assets, "*", SearchOption.AllDirectories))
        {
            if (IsAsset(f)) files.Add(f);
        }

        Console.WriteLine("scanning " + files.Count + " asset files ...");
        DateTime started = DateTime.Now;
        int scanned = 0;
        int touched = 0;

        for (int fi = 0; fi < files.Count; fi++)
        {
            string path = files[fi];
            scanned++;

            byte[] raw;
            try { raw = File.ReadAllBytes(path); }
            catch (Exception ex)
            {
                Console.WriteLine("skip (read error): " + path + " (" + ex.Message + ")");
                continue;
            }

            string latin = latin1.GetString(raw);
            bool hit = latin.IndexOf(guid, StringComparison.Ordinal) >= 0;

            if (!hit && !deep)
            {
                if (scanned % 500 == 0) Console.WriteLine("  ... " + scanned + " files scanned");
                continue;
            }

            bool hasBom = raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF;
            int off = hasBom ? 3 : 0;
            string text = utf8.GetString(raw, off, raw.Length - off);

            List<string> lines = SplitKeepEnds(text);
            bool changed = false;

            if (hit)
            {
                int nDocs;
                lines = StripScriptDocs(lines, guid, out nDocs);
                if (nDocs > 0) changed = true;
            }

            if (hit || deep)
            {
                int nSlots;
                lines = StripComponentSlots(lines, out nSlots);
                if (nSlots > 0) changed = true;
                int nAdded;
                lines = StripAddedComponents(lines, out nAdded);
                if (nAdded > 0) changed = true;
            }

            if (changed)
            {
                touched++;
                string rel = path.Substring(assets.Length).TrimStart('\\');
                Console.WriteLine((dryRun ? "would clean: " : "cleaned: ") + rel);

                if (!dryRun)
                {
                    if (backupDir != null)
                    {
                        string dst = Path.Combine(backupDir, rel);
                        Directory.CreateDirectory(Path.GetDirectoryName(dst));
                        File.Copy(path, dst, true);
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int k = 0; k < lines.Count; k++) sb.Append(lines[k]);
                    byte[] outBytes = utf8.GetBytes(sb.ToString());
                    if (hasBom)
                    {
                        byte[] all = new byte[outBytes.Length + 3];
                        all[0] = 0xEF; all[1] = 0xBB; all[2] = 0xBF;
                        Array.Copy(outBytes, 0, all, 3, outBytes.Length);
                        File.WriteAllBytes(path, all);
                    }
                    else
                    {
                        File.WriteAllBytes(path, outBytes);
                    }
                }
            }

            if (scanned % 500 == 0) Console.WriteLine("  ... " + scanned + " files scanned");
        }

        Console.WriteLine("scanned " + scanned + " asset files in " + (DateTime.Now - started).TotalSeconds.ToString("0.0") + "s, " + (dryRun ? "to clean " : "cleaned ") + touched);

        if (dryRun) return 0;

        if (!noDelete)
        {
            bool removedAny = false;
            string csPath = Path.Combine(root, "Assets", "ParticleSystemExpand", "Mono", "NewSubParticleSystem.cs");
            string metaPath = csPath + ".meta";
            if (File.Exists(csPath)) { File.Delete(csPath); Console.WriteLine("deleted: " + csPath); removedAny = true; }
            if (File.Exists(metaPath)) { File.Delete(metaPath); Console.WriteLine("deleted: " + metaPath); removedAny = true; }
            if (!removedAny) Console.WriteLine("script file already gone: " + csPath);
        }

        int leftover = 0;
        foreach (string f in files)
        {
            try
            {
                byte[] raw = File.ReadAllBytes(f);
                if (latin1.GetString(raw).IndexOf(guid, StringComparison.Ordinal) >= 0)
                {
                    if (leftover == 0) Console.WriteLine("WARNING: guid still present in:");
                    Console.WriteLine("  " + f.Substring(assets.Length).TrimStart('\\'));
                    leftover++;
                }
            }
            catch { }
        }
        if (leftover > 0) return 1;

        Console.WriteLine("done. open Unity and let it reimport.");
        return 0;
    }
}
'@

if (-not ("SubCleaner" -as [type])) {
    Add-Type -TypeDefinition $code -Language CSharp -ErrorAction Stop
}

$rc = [SubCleaner]::Run($Root, $Guid, [bool]$DryRun, [bool]$NoBackup, [bool]$NoDeleteScript, [bool]$Deep)
exit $rc
