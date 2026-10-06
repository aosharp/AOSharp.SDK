@echo off
setlocal EnableExtensions
REM Pack and push AOSharp.Common + SmokeLounge.AOtomation to nuget.org, then bump the patch version.
REM
REM   nuget_deploy.cmd --ApiKey YOUR_KEY   (first run: pushes and saves the key to the NUGET_API_KEY user env var)
REM   nuget_deploy.cmd                     (later runs: uses the saved key)
REM
REM <Version> in the csprojs is the next version to publish. After a successful push
REM both csprojs are bumped to the following patch version.

set "SCRIPT_DIR=%~dp0"
set "SDK_DIR=%SCRIPT_DIR%.."
set "COMMON=%SDK_DIR%\AOSharp.Common\AOSharp.Common.csproj"
set "SMOKE=%SDK_DIR%\SmokeLounge.AOtomation\SmokeLounge.AOtomation.csproj"
set "APIKEY="
set "SAVEKEY="

:parse
if "%~1"=="" goto parsed
if /I "%~1"=="--ApiKey" (
  set "APIKEY=%~2"
  set "SAVEKEY=1"
  shift
  shift
  goto parse
)
echo Unknown argument: %~1
echo Usage: %~nx0 [--ApiKey YOUR_KEY]
exit /b 1
:parsed

if "%APIKEY%"=="" set "APIKEY=%NUGET_API_KEY%"
if "%APIKEY%"=="" (
  for /f "tokens=2,*" %%a in ('reg query HKCU\Environment /v NUGET_API_KEY 2^>nul ^| find "NUGET_API_KEY"') do set "APIKEY=%%b"
)
if "%APIKEY%"=="" (
  echo No API key. Run once with --ApiKey YOUR_KEY.
  exit /b 1
)

if defined SAVEKEY (
  setx NUGET_API_KEY "%APIKEY%" >nul
  echo Saved API key to NUGET_API_KEY user environment variable.
)

set "VER="
set "NEXTVER="
for /f "usebackq tokens=1,2" %%a in (`powershell -NoProfile -Command "$m=[regex]::Match((Get-Content -Raw '%COMMON%'),'<Version>(\d+)\.(\d+)\.(\d+)</Version>'); if(-not $m.Success){exit 1}; $m.Groups[1].Value+'.'+$m.Groups[2].Value+'.'+$m.Groups[3].Value+' '+$m.Groups[1].Value+'.'+$m.Groups[2].Value+'.'+([int]$m.Groups[3].Value+1)"`) do (
  set "VER=%%a"
  set "NEXTVER=%%b"
)

if "%VER%"=="" (
  echo Could not read ^<Version^>X.Y.Z^</Version^> from %COMMON%
  exit /b 1
)

echo Publishing %VER% to nuget.org...

powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%pack-nuget.ps1" -Version %VER% -Push -ApiKey "%APIKEY%"
if errorlevel 1 (
  echo Push failed. csproj versions left at %VER%.
  exit /b 1
)

powershell -NoProfile -Command "foreach($p in '%COMMON%','%SMOKE%'){ $c=[IO.File]::ReadAllText($p); $c=([regex]'<Version>[^<]*</Version>').Replace($c,'<Version>%NEXTVER%</Version>',1); [IO.File]::WriteAllText($p,$c) }"
if errorlevel 1 (
  echo Pushed %VER% but failed to bump csproj versions. Set ^<Version^>%NEXTVER%^</Version^> manually.
  exit /b 1
)

echo Published %VER%. Next version will be %NEXTVER%.
exit /b 0
