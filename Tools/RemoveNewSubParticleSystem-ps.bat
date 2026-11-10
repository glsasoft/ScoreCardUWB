@echo off
chcp 65001 >nul
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0RemoveNewSubParticleSystem.ps1" %*
set "RC=%errorlevel%"
if not "%RC%"=="0" pause
exit /b %RC%
