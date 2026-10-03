@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1"
set "RESULT=%ERRORLEVEL%"
pause
exit /b %RESULT%
