@echo off
:: Run this script from the BimAiAssistant project root AFTER dotnet build.
:: It assembles the BIMAI ZIP from the Release output.

setlocal

set "PROJECT_ROOT=%~dp0.."
set "BUILD_OUT=%PROJECT_ROOT%\BimAiAssistant\bin\Release\net48"
set "INSTALLER_DIR=%PROJECT_ROOT%\Installer"
set "STAGING=%TEMP%\BIMAI_staging"
set "ZIP_OUT=%INSTALLER_DIR%\BIMAI_Installer.zip"

echo.
echo Building plugin...
dotnet build "%PROJECT_ROOT%\BimAiAssistant\BimAiAssistant.csproj" -c Release
if errorlevel 1 (
    echo ERROR: Build failed.
    pause & exit /b 1
)

echo.
echo Staging files...

if exist "%STAGING%" rd /S /Q "%STAGING%"
mkdir "%STAGING%\BIMAI"

copy /Y "%BUILD_OUT%\BimAiAssistant.dll"   "%STAGING%\BIMAI\" >nul
copy /Y "%BUILD_OUT%\Newtonsoft.Json.dll"  "%STAGING%\BIMAI\" >nul
copy /Y "%INSTALLER_DIR%\install.bat"      "%STAGING%\BIMAI\" >nul
copy /Y "%INSTALLER_DIR%\uninstall.bat"    "%STAGING%\BIMAI\" >nul
copy /Y "%INSTALLER_DIR%\README.txt"       "%STAGING%\BIMAI\" >nul

echo.
echo Creating ZIP...

powershell -NoProfile -Command ^
  "Compress-Archive -Path '%STAGING%\BIMAI' -DestinationPath '%ZIP_OUT%' -Force"

rd /S /Q "%STAGING%"

echo.
echo ============================================
echo   Package ready: %ZIP_OUT%
echo ============================================
echo.
pause
