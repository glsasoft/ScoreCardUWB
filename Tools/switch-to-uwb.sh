#!/usr/bin/env bash
# 禁用 ScoreCardUnity，启用 UWB，并拉起 UWB
# 用法: ./switch-to-uwb.sh [设备地址]
# 默认: 192.168.110.126:5555

set -euo pipefail

DEVICE="${1:-192.168.110.126:5555}"
PKG_UNITY="com.glsa.mqttscorecard5s.unity"
PKG_UWB="com.glsa.mqttscorecard5s.unityuwb"

adb_s() {
  adb -s "${DEVICE}" "$@"
}

resolve_launcher() {
  local pkg="$1"
  adb_s shell cmd package resolve-activity --brief -c android.intent.category.LAUNCHER "${pkg}" 2>/dev/null \
    | tr -d '\r' \
    | awk 'NF{line=$0} END{print line}'
}

is_pkg_up() {
  local pkg="$1"
  local pid
  pid="$(adb_s shell pidof "${pkg}" 2>/dev/null | tr -d '\r' | awk '{print $1}')"
  [[ -n "${pid}" ]]
}

launch_pkg() {
  local pkg="$1"
  local label="$2"
  local component
  local i
  local start_out=""

  echo "启动 ${label}..."
  # pm enable 后组件偶发未立刻可解析，稍等再启
  sleep 1

  for i in 1 2 3; do
    component="$(resolve_launcher "${pkg}")"
    if [[ "${component}" == */* ]]; then
      echo "  launcher: ${component}"
      start_out="$(adb_s shell am start -n "${component}" 2>&1 | tr -d '\r' || true)"
      echo "  ${start_out}"
      # 已在前台也会打印 Warning，视为成功
      if echo "${start_out}" | grep -Eq 'Starting:|delivered to currently running'; then
        break
      fi
    fi
    echo "  第 ${i} 次 am start 未成功，改用 monkey..."
    adb_s shell monkey -p "${pkg}" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1 || true
    sleep 1
  done

  sleep 1
  if is_pkg_up "${pkg}"; then
    echo "  已拉起: ${pkg} (pid=$(adb_s shell pidof "${pkg}" | tr -d '\r'))"
    return 0
  fi

  echo "[错误] 未能确认 ${label} 进程已运行"
  return 1
}

echo "============================================"
echo "切换到 UWB 比分牌"
echo "设备: ${DEVICE}"
echo "============================================"

if ! command -v adb >/dev/null 2>&1; then
  echo "[错误] 未找到 adb，请先配置 PATH"
  exit 1
fi

echo "[1/5] 连接设备..."
adb connect "${DEVICE}" >/dev/null

echo "[2/5] 强制停止 ScoreCardUnity..."
adb_s shell am force-stop "${PKG_UNITY}" || true

echo "[3/5] 禁用 ScoreCardUnity..."
if ! adb_s shell pm disable-user --user 0 "${PKG_UNITY}"; then
  echo "[警告] 禁用 Unity 失败，请确认设备上已安装 ${PKG_UNITY}"
fi

echo "[4/5] 启用 UWB..."
adb_s shell pm enable "${PKG_UWB}"

echo "[5/5] 启动 UWB..."
launch_pkg "${PKG_UWB}" "UWB"

echo
echo "完成: 已禁用 Unity，已启用并启动 UWB"
