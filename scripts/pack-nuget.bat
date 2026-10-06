@echo off
setlocal EnableExtensions
REM Build and pack AOSharp.Common + SmokeLounge.AOtomation
REM
REM   pack-nuget.bat
REM   pack-nuget.bat 1.0.1
REM   pack-nuget.bat 1.0.1 Release
REM   pack-nuget.bat 1.0.1 Release -Push -ApiKey YOUR_KEY
REM
REM Prefer calling the ps1 directly when pushing:
REM   powershell -File .\pack-nuget.ps1 -Version 1.0.1 -Push -ApiKey YOUR_KEY

set "SCRIPT=%~dp0pack-nuget.ps1"

if "%~1"=="" (
  powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%"
  exit /b %ERRORLEVEL%
)

REM Named PowerShell params: forward unchanged
echo(%~1| findstr /r "^-" >nul
if %ERRORLEVEL%==0 (
  powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*
  exit /b %ERRORLEVEL%
)

REM Positional: version [configuration] [extra switches...]
set "VERSION=%~1"
set "CONFIG=Release"

if /I "%~2"=="Debug" goto with_config
if /I "%~2"=="Release" goto with_config

REM No configuration arg — extras start at %2
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" -Version "%VERSION%" -Configuration "%CONFIG%" %2 %3 %4 %5 %6 %7 %8 %9
exit /b %ERRORLEVEL%

:with_config
set "CONFIG=%~2"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" -Version "%VERSION%" -Configuration "%CONFIG%" %3 %4 %5 %6 %7 %8 %9
exit /b %ERRORLEVEL%
