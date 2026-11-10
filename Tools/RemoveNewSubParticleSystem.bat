@echo off
chcp 65001 >nul
setlocal
set "SCRIPT=%~dp0RemoveNewSubParticleSystem.py"
set "PY="
where py >nul 2>nul && set "PY=py -3"
if not defined PY (
  where python >nul 2>nul && set "PY=python"
)
if not defined PY (
  echo Python 3 not found. Install it, or run RemoveNewSubParticleSystem.py with any Python 3.
  pause
  exit /b 1
)
echo Running with: %PY%
%PY% "%SCRIPT%" %*
set "RC=%errorlevel%"
if not "%RC%"=="0" pause
exit /b %RC%
