@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1"
set "RESULT=%ERRORLEVEL%"
pause
exit /b %RESULT%
