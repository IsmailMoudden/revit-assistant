$ErrorActionPreference = 'Stop'
$version = (Get-Content (Join-Path $PSScriptRoot 'revit-version.txt') -Raw).Trim()
if ($version -notin @('2024', '2025', '2026')) { throw 'Invalid package Revit version.' }
$registration = Join-Path $env:APPDATA "Autodesk/Revit/Addins/$version/BimAiAssistant.addin"
$install = Join-Path $env:LOCALAPPDATA "BIMAI/$version"
if (Test-Path $registration) { Remove-Item $registration -Force }
if (Test-Path $install) { Remove-Item $install -Recurse -Force }
Write-Host "Uninstalled Revit $version add-in."
