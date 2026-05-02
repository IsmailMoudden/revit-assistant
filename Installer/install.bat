@echo off
setlocal enabledelayedexpansion

echo.
echo ============================================
echo   BIM AI Assistant - Installation
echo ============================================
echo.

:: ── 1. Define paths ──────────────────────────────────────────────────────────

set "INSTALL_DIR=%LOCALAPPDATA%\BIMAI"
set "SCRIPT_DIR=%~dp0"
set "ADDIN_NAME=BimAiAssistant.addin"
set "FOUND_REVIT=0"

:: ── 2. Create install directory ──────────────────────────────────────────────

echo Installing to: %INSTALL_DIR%
echo.

if not exist "%INSTALL_DIR%" (
    mkdir "%INSTALL_DIR%"
    if errorlevel 1 (
        echo ERROR: Could not create %INSTALL_DIR%
        echo        Try running as Administrator.
        goto :error
    )
)

:: ── 3. Copy DLLs ─────────────────────────────────────────────────────────────

echo Copying plugin files...

copy /Y "%SCRIPT_DIR%BimAiAssistant.dll" "%INSTALL_DIR%\" >nul
if errorlevel 1 (
    echo ERROR: Could not copy BimAiAssistant.dll
    goto :error
)

copy /Y "%SCRIPT_DIR%Newtonsoft.Json.dll" "%INSTALL_DIR%\" >nul
if errorlevel 1 (
    echo ERROR: Could not copy Newtonsoft.Json.dll
    goto :error
)

echo Files copied successfully.
echo.

:: ── 4. Write .addin content to a temp file ───────────────────────────────────

set "ADDIN_CONTENT=%TEMP%\bimai_addin_tmp.xml"

(
echo ^<?xml version="1.0" encoding="utf-8"?^>
echo ^<RevitAddIns^>
echo   ^<AddIn Type="Application"^>
echo     ^<Name^>BIM AI^</Name^>
echo     ^<Assembly^>%INSTALL_DIR%\BimAiAssistant.dll^</Assembly^>
echo     ^<AddInId^>8D83C886-B739-4ACD-A9DB-5B6F0F2E1234^</AddInId^>
echo     ^<FullClassName^>BimAiAssistant.App^</FullClassName^>
echo     ^<VendorId^>BIMAI^</VendorId^>
echo     ^<VendorDescription^>BIM AI Assistant^</VendorDescription^>
echo   ^</AddIn^>
echo ^</RevitAddIns^>
) > "%ADDIN_CONTENT%"

:: ── 5. Detect Revit versions and deploy .addin ───────────────────────────────

echo Detecting installed Revit versions...
echo.

for %%V in (2022 2023 2024 2025) do (
    set "ADDIN_DIR=%APPDATA%\Autodesk\Revit\Addins\%%V"
    if exist "!ADDIN_DIR!" (
        echo   Revit %%V detected
        copy /Y "%ADDIN_CONTENT%" "!ADDIN_DIR!\%ADDIN_NAME%" >nul
        if errorlevel 1 (
            echo   WARNING: Could not write addin file for Revit %%V
            echo            Check that you have write permission to !ADDIN_DIR!
        ) else (
            echo   Addin registered for Revit %%V
            set "FOUND_REVIT=1"
        )
    )
)

del "%ADDIN_CONTENT%" >nul 2>&1

echo.

if "%FOUND_REVIT%"=="0" (
    echo WARNING: No Revit installation detected.
    echo          The plugin files have been copied to %INSTALL_DIR%
    echo          but no .addin file was registered.
    echo.
    echo          If Revit is installed, manually create the folder:
    echo          %%APPDATA%%\Autodesk\Revit\Addins\^<version^>
    echo          and re-run this installer.
    echo.
)

:: ── 6. Done ──────────────────────────────────────────────────────────────────

echo ============================================
echo   Installation complete!
echo ============================================
echo.
echo   Next steps:
echo   1. Open Autodesk Revit
echo   2. Look for the "BIM AI" tab in the ribbon
echo   3. Click "Run AI" and enter your instruction
echo.
pause
exit /b 0

:error
echo.
echo ============================================
echo   Installation FAILED
echo ============================================
echo.
pause
exit /b 1
