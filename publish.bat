@echo off
setlocal
chcp 65001 > nul

echo =================================
echo  CFDDNS 项目发布脚本
echo =================================

REM 1. 框架依赖版发布 (需要用户安装 .NET 6 Desktop Runtime)
echo [1/2] 正在发布框架依赖版...
set OUTDIR_FD=publish\framework_dependent
rd /s /q %OUTDIR_FD% 2>nul
mkdir %OUTDIR_FD%
dotnet publish CFDDNS.csproj -c Release -o %OUTDIR_FD% --self-contained false

echo 删除不必要的文件...
cd %OUTDIR_FD%
del /q *.pdb *.xml *.runtimeconfig.dev.json 2>nul
cd /d %~dp0

echo 框架依赖版发布完成!


REM 2. 自包含版发布（单文件.exe, 无需.NET环境）
echo [2/2] 正在发布自包含版 (win-x64)...
set OUTDIR_SC=publish\self_contained
rd /s /q %OUTDIR_SC% 2>nul
mkdir %OUTDIR_SC%
dotnet publish CFDDNS.csproj -c Release -o %OUTDIR_SC% --self-contained true -r win-x64 /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true

echo 清理多余文件，只保留 .exe...
cd %OUTDIR_SC%
for %%f in (*) do (
  if /I not "%%~xf"==".exe" del /q "%%f"
)
cd /d %~dp0
echo 自包含版发布完成!


echo =================================
echo  发布全部完成!
echo =================================
echo 可以在 publish 文件夹中查看两种不同版本的程序。
pause 