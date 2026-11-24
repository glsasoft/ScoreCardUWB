#!/usr/bin/env bash
# 拉取 LeftPanelFpsProfiler 采样数据
# 用法: ./pull-fps-profile.sh [设备地址] [本地目录]
# 默认: 192.168.110.126:5555  -> ./fps_profile_pull/

set -euo pipefail

DEVICE="${1:-192.168.110.126:5555}"
OUT_DIR="${2:-./fps_profile_pull}"
PKG="com.glsa.mqttscorecard5s.unityuwb"

mkdir -p "${OUT_DIR}"
adb connect "${DEVICE}" >/dev/null

echo "从 /sdcard/uwb_fps_profile 拉取..."
adb -s "${DEVICE}" pull /sdcard/uwb_fps_profile "${OUT_DIR}/sdcard" 2>/dev/null || true

echo "尝试从应用私有目录拉取..."
adb -s "${DEVICE}" shell "run-as ${PKG} ls files/fps_profile" 2>/dev/null || true
adb -s "${DEVICE}" exec-out "run-as ${PKG} sh -c 'tar -c files/fps_profile 2>/dev/null'" > "${OUT_DIR}/app_fps_profile.tar" 2>/dev/null || true

# system uid 时 run-as 可能不可用，直接从 data 路径试（需 root）
adb -s "${DEVICE}" shell "ls /data/data/${PKG}/files/fps_profile 2>/dev/null" || true

echo
echo "已保存到: ${OUT_DIR}"
ls -la "${OUT_DIR}" "${OUT_DIR}/sdcard" 2>/dev/null || true
echo
echo "分析示例:"
echo "  cat ${OUT_DIR}/sdcard/*.jsonl | python3 -c \"import sys,json; rows=[json.loads(l) for l in sys.stdin if 'fps' in l]; print('avg', sum(r['fps'] for r in rows)/max(1,len(rows))); print('hints', {r.get('hint') for r in rows})\""
