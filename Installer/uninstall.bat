@echo off
setlocal enabledelayedexpansion

echo.
echo ============================================
echo   BIM AI Assistant - Uninstall
echo ============================================
echo.

set "INSTALL_DIR=%LOCALAPPDATA%\BIMAI"
set "ADDIN_NAME=BimAiAssistant.addin"

:: ── 1. Remove .addin files from all Revit versions ───────────────────────────

echo Removing Revit addin registrations...
echo.

for %%V in (2022 2023 2024 2025) do (
    set "ADDIN_FILE=%APPDATA%\Autodesk\Revit\Addins\%%V\%ADDIN_NAME%"
    if exist "!ADDIN_FILE!" (
        del /F /Q "!ADDIN_FILE!"
        if errorlevel 1 (
            echo   WARNING: Could not remove addin for Revit %%V
        ) else (
            echo   Removed addin for Revit %%V
        )
    )
)

:: ── 2. Delete install directory ──────────────────────────────────────────────

echo.
echo Removing plugin files from %INSTALL_DIR%...

if exist "%INSTALL_DIR%" (
    rd /S /Q "%INSTALL_DIR%"
    if errorlevel 1 (
        echo ERROR: Could not delete %INSTALL_DIR%
        echo        Close Revit and try again, or delete the folder manually.
        goto :error
    )
    echo Done.
) else (
    echo   Folder not found - already uninstalled.
)

echo.
echo ============================================
echo   Uninstall complete.
echo ============================================
echo.
pause
exit /b 0

:error
echo.
echo ============================================
echo   Uninstall FAILED
echo ============================================
echo.
pause
exit /b 1
