@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

rem 禁用 UWB，启用 ScoreCardUnity，并拉起 Unity
rem 用法: switch-to-unity.bat [设备地址]
rem 默认: 192.168.110.126:5555

set "DEVICE=%~1"
if "%DEVICE%"=="" set "DEVICE=192.168.110.126:5555"

set "PKG_UNITY=com.glsa.mqttscorecard5s.unity"
set "PKG_UWB=com.glsa.mqttscorecard5s.unityuwb"

echo ============================================
echo 切换到 ScoreCardUnity 比分牌
echo 设备: %DEVICE%
echo ============================================

where adb >nul 2>&1
if errorlevel 1 (
    echo [错误] 未找到 adb，请先配置 PATH
    goto :fail
)

echo [1/5] 连接设备...
adb connect %DEVICE% >nul
if errorlevel 1 (
    echo [错误] adb connect 失败
    goto :fail
)

echo [2/5] 强制停止 UWB...
adb -s %DEVICE% shell am force-stop %PKG_UWB%

echo [3/5] 禁用 UWB...
adb -s %DEVICE% shell pm disable-user --user 0 %PKG_UWB%
if errorlevel 1 (
    echo [警告] 禁用 UWB 失败，请确认设备上已安装 %PKG_UWB%
)

echo [4/5] 启用 ScoreCardUnity...
adb -s %DEVICE% shell pm enable %PKG_UNITY%
if errorlevel 1 (
    echo [错误] 启用 Unity 失败，请确认设备上已安装 %PKG_UNITY%
    goto :fail
)

echo [5/5] 启动 ScoreCardUnity...
call :launch_pkg %PKG_UNITY% ScoreCardUnity
if errorlevel 1 goto :fail

echo.
echo 完成: 已禁用 UWB，已启用并启动 ScoreCardUnity
echo.
goto :end

:launch_pkg
set "LPKG=%~1"
set "LLABEL=%~2"
echo 启动 %LLABEL%...
timeout /t 1 /nobreak >nul

set "COMPONENT="
for /f "usebackq delims=" %%A in (`adb -s %DEVICE% shell cmd package resolve-activity --brief -c android.intent.category.LAUNCHER %LPKG% 2^>nul`) do set "COMPONENT=%%A"
set "COMPONENT=!COMPONENT: =!"

if not "!COMPONENT!"=="" if not "!COMPONENT:/=!"=="!COMPONENT!" (
    echo   launcher: !COMPONENT!
    adb -s %DEVICE% shell am start -n !COMPONENT!
    if not errorlevel 1 goto :launch_ok
)

echo   am start 失败，改用 monkey...
adb -s %DEVICE% shell monkey -p %LPKG% -c android.intent.category.LAUNCHER 1 >nul 2>&1

:launch_ok
timeout /t 1 /nobreak >nul
set "LPID="
for /f "usebackq delims=" %%P in (`adb -s %DEVICE% shell pidof %LPKG% 2^>nul`) do set "LPID=%%P"
if "%LPID%"=="" (
    echo [错误] 未能确认 %LLABEL% 进程已运行
    exit /b 1
)
echo   已拉起: %LPKG% (pid=%LPID%)
exit /b 0

:fail
echo.
echo 切换失败
echo.
pause
exit /b 1

:end
pause
exit /b 0
