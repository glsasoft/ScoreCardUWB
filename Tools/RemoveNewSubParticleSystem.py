#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Remove the NewSubParticleSystem MonoBehaviour from a Unity project.

It rewrites serialized YAML assets (prefabs, scenes, ...) and drops
  * the MonoBehaviour document whose m_Script points at the script GUID
  * the matching entry in the owning GameObject's m_Component list
  * the matching entry in a PrefabInstance's m_AddedComponents list
Then (optionally) deletes the .cs and its .meta.

Usage (close Unity first):
    python RemoveNewSubParticleSystem.py                  # project root = parent of this script's folder
    python RemoveNewSubParticleSystem.py --root H:\\SomeProject
    python RemoveNewSubParticleSystem.py --dry-run        # report only
    python RemoveNewSubParticleSystem.py --no-delete-script
    python RemoveNewSubParticleSystem.py --deep           # also fix dangling slots in files without the GUID

Exit code 0 on success.
"""

import argparse
import codecs
import io
import os
import re
import shutil
import sys
import time

DEFAULT_GUID = "f8aa87937116feb41b82b0ab99b5f943"
SCRIPT_REL = os.path.join("Assets", "ParticleSystemExpand", "Mono", "NewSubParticleSystem.cs")
EXTS = {".prefab", ".unity", ".asset", ".controller", ".mat", ".playable", ".anim", ".overridecontroller"}

DOC_RE = re.compile(r"^--- !u!\d+ &(-?\d+)")
COMP_RE = re.compile(r"^\s*-\s*component:\s*\{fileID:\s*(-?\d+)\s*\}\s*$")
ITEM_RE = re.compile(r"^(\s*)- ")
ADDED_RE = re.compile(r"^\s*addedObject:\s*\{fileID:\s*(-?\d+)\s*\}")


def read_guid_from_meta(root):
    meta = os.path.join(root, SCRIPT_REL + ".meta")
    if not os.path.isfile(meta):
        return None
    try:
        with io.open(meta, "r", encoding="utf-8-sig", errors="replace") as f:
            for line in f:
                if line.startswith("guid:"):
                    return line.split(":", 1)[1].strip()
    except Exception:
        pass
    return None


def strip_script_docs(lines, guid):
    out = []
    removed = 0
    i, n = 0, len(lines)
    while i < n:
        ln = lines[i]
        if ln.startswith("--- "):
            j = i + 1
            while j < n and not lines[j].startswith("--- "):
                j += 1
            body = "".join(lines[i:j])
            if guid in body and "m_Script:" in body:
                removed += 1
                i = j
                continue
            out.extend(lines[i:j])
            i = j
            continue
        out.append(ln)
        i += 1
    return out, removed


def strip_component_slots(lines):
    doc_ids = set()
    for ln in lines:
        m = DOC_RE.match(ln)
        if m:
            doc_ids.add(m.group(1))
    out = []
    dropped = 0
    for ln in lines:
        m = COMP_RE.match(ln)
        if m and m.group(1) not in doc_ids:
            dropped += 1
            continue
        out.append(ln)
    return out, dropped


def strip_added_components(lines):
    doc_ids = set()
    for ln in lines:
        m = DOC_RE.match(ln)
        if m:
            doc_ids.add(m.group(1))
    out = []
    item_start = None
    removed = 0
    for ln in lines:
        m = ITEM_RE.match(ln)
        if m and len(ln) - len(ln.lstrip()) == 4:
            item_start = len(out)
        out.append(ln)
        am = ADDED_RE.match(ln)
        if am and am.group(1) not in doc_ids:
            if item_start is not None and item_start < len(out):
                del out[item_start:]
                removed += 1
    return out, removed


def iter_assets(root):
    assets = os.path.join(root, "Assets")
    for dirpath, dirnames, filenames in os.walk(assets):
        for fn in filenames:
            if os.path.splitext(fn)[1].lower() in EXTS:
                yield os.path.join(dirpath, fn)


def write_asset(path, text, bom):
    data = text.encode("utf-8")
    if bom:
        data = codecs.BOM_UTF8 + data
    tmp = path + ".tmp_clean"
    with open(tmp, "wb") as f:
        f.write(data)
    os.replace(tmp, path)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", default=None, help="Unity project root (default: folder above this script)")
    ap.add_argument("--guid", default=None, help="script GUID (default: read from .meta, else built-in)")
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--no-delete-script", action="store_true")
    ap.add_argument("--no-backup", action="store_true")
    ap.add_argument("--deep", action="store_true", help="scan every asset for dangling component slots (slow)")
    args = ap.parse_args()

    here = os.path.dirname(os.path.abspath(__file__))
    root = os.path.abspath(args.root) if args.root else os.path.dirname(here)
    if not os.path.isdir(os.path.join(root, "Assets")):
        print("ERROR: not a Unity project (no Assets folder): " + root)
        return 2

    guid = args.guid or read_guid_from_meta(root) or DEFAULT_GUID
    guid_bytes = guid.encode("ascii")
    print("project : " + root, flush=True)
    print("guid    : " + guid, flush=True)
    if not args.dry_run:
        print("mode    : writing changes", flush=True)
    else:
        print("mode    : dry run", flush=True)
    print("scanning Assets/ ...", flush=True)

    backup_dir = None
    if not args.dry_run and not args.no_backup:
        backup_dir = os.path.join(os.path.dirname(here), "_cleanup_backup", time.strftime("%Y%m%d_%H%M%S"))

    started = time.time()
    scanned = 0
    touched = []
    for path in iter_assets(root):
        scanned += 1
        try:
            with open(path, "rb") as f:
                raw = f.read()
        except Exception as exc:
            print("skip (read error): %s (%s)" % (path, exc), flush=True)
            continue

        hit = guid_bytes in raw
        if not hit and not args.deep:
            if scanned % 500 == 0:
                print("  ... %d files scanned" % scanned, flush=True)
            continue

        try:
            text = raw.decode("utf-8-sig")
        except UnicodeDecodeError:
            if scanned % 500 == 0:
                print("  ... %d files scanned" % scanned, flush=True)
            continue
        bom = raw.startswith(codecs.BOM_UTF8)
        lines = text.splitlines(keepends=True)
        changed = False

        if hit:
            lines, n_docs = strip_script_docs(lines, guid)
            if n_docs:
                changed = True

        if args.deep or hit:
            lines, n_slots = strip_component_slots(lines)
            if n_slots:
                changed = True
            lines, n_added = strip_added_components(lines)
            if n_added:
                changed = True

        if changed:
            rel = os.path.relpath(path, root)
            touched.append(rel)
            print(("would clean: " if args.dry_run else "cleaned: ") + rel, flush=True)
            if not args.dry_run:
                if backup_dir:
                    dst = os.path.join(backup_dir, rel)
                    os.makedirs(os.path.dirname(dst), exist_ok=True)
                    shutil.copy2(path, dst)
                write_asset(path, "".join(lines), bom)

        if scanned % 500 == 0:
            print("  ... %d files scanned" % scanned, flush=True)

    elapsed = time.time() - started
    print("scanned %d asset files in %.1fs, %s %d"
          % (scanned, elapsed, "to clean" if args.dry_run else "cleaned", len(touched)), flush=True)

    if args.dry_run:
        return 0

    if not args.no_delete_script:
        removed_any = False
        for rel in (SCRIPT_REL, SCRIPT_REL + ".meta"):
            path = os.path.join(root, rel)
            if os.path.isfile(path):
                os.remove(path)
                print("deleted: " + rel, flush=True)
                removed_any = True
        if not removed_any:
            print("script file already gone: " + SCRIPT_REL, flush=True)

    leftover = []
    for path in iter_assets(root):
        try:
            with open(path, "rb") as f:
                raw = f.read()
        except Exception:
            continue
        if guid_bytes in raw:
            leftover.append(os.path.relpath(path, root))
    if leftover:
        print("WARNING: guid still present in:", flush=True)
        for rel in leftover:
            print("  " + rel, flush=True)
        return 1

    print("done. open Unity and let it reimport.", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
