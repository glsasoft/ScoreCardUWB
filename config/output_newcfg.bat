@echo off
cd /d "%~dp0"

echo 正在运行 publish\ReadExcel.exe...
start "" /wait "publish\ReadExcel.exe"
echo 编译完成 正在复制文件

rd /S /Q "..\Assets\TableConfigRes"
xcopy "Config" "..\Assets\TableConfigRes" /Y /I /E
xcopy "TableCSharp" "..\Assets\Scripts\BaseScripts\Table" /Y /I /E

for /d %%i in ("TableExtend\*") do (
    if	not exist "..\Assets\Scripts\BaseScripts\%%i" (
	xcopy "%%i" "..\Assets\Scripts\BaseScripts\%%i" /Y /I /E
)
)
pause