param([Parameter(Mandatory = $true)][string]$PackageDirectory)

$ErrorActionPreference = 'Stop'
$temporary = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString())
$originalAppData = $env:APPDATA
$originalLocalAppData = $env:LOCALAPPDATA
try {
    $env:APPDATA = Join-Path $temporary 'profile'
    $env:LOCALAPPDATA = Join-Path $temporary 'local'
    foreach ($version in @('2024', '2025', '2026')) {
        $archive = Join-Path $PackageDirectory "RevitAssistant-Revit$version.zip"
        $extracted = Join-Path $temporary "package-$version"
        Expand-Archive $archive -DestinationPath $extracted
        $package = Join-Path $extracted "RevitAssistant-Revit$version"
        & (Join-Path $package 'install.ps1')
        $manifest = Join-Path $env:APPDATA "Autodesk/Revit/Addins/$version/BimAiAssistant.addin"
        [xml]$registration = Get-Content $manifest -Raw
        $expected = Join-Path $env:LOCALAPPDATA "BIMAI/$version/BimAiAssistant.dll"
        if ($registration.RevitAddIns.AddIn.Assembly -ne $expected -or !(Test-Path $expected)) {
            throw "Incorrect assembly registration for Revit $version."
        }
        if (!(Test-Path (Join-Path $env:LOCALAPPDATA "BIMAI/$version/Newtonsoft.Json.dll"))) {
            throw "Missing runtime dependency for Revit $version."
        }
    }

    & (Join-Path $temporary 'package-2025/RevitAssistant-Revit2025/uninstall.ps1')
    if ((Test-Path (Join-Path $env:LOCALAPPDATA 'BIMAI/2025')) -or
        (Test-Path (Join-Path $env:APPDATA 'Autodesk/Revit/Addins/2025/BimAiAssistant.addin'))) {
        throw 'Uninstall did not remove the selected version.'
    }
    foreach ($version in @('2024', '2026')) {
        if (!(Test-Path (Join-Path $env:LOCALAPPDATA "BIMAI/$version/BimAiAssistant.dll")) -or
            !(Test-Path (Join-Path $env:APPDATA "Autodesk/Revit/Addins/$version/BimAiAssistant.addin"))) {
            throw "Uninstall affected another Revit version: $version."
        }
    }
    Write-Host 'All three installer packages coexist; uninstall is isolated by version.'
} finally {
    $env:APPDATA = $originalAppData
    $env:LOCALAPPDATA = $originalLocalAppData
    if (Test-Path $temporary) { Remove-Item $temporary -Recurse -Force }
}
