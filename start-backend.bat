@echo off
cd /d "%~dp0"
where py >nul 2>&1
if errorlevel 1 (
    where python >nul 2>&1
    if errorlevel 1 (
        echo Install Python 3.11 or newer from python.org, then try again.
        pause
        exit /b 1
    )
    python scripts\start-backend.py %*
) else (
    py -3 scripts\start-backend.py %*
)
set "RESULT=%ERRORLEVEL%"
if not "%RESULT%"=="0" pause
exit /b %RESULT%
