@echo off
set "REVIT_VERSION=%~1"
if "%REVIT_VERSION%"=="" set "REVIT_VERSION=2024"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\..\scripts\package-addin.ps1" -RevitVersion "%REVIT_VERSION%"
set "RESULT=%ERRORLEVEL%"
pause
exit /b %RESULT%
