param(
    [ValidateSet('2024', '2025', '2026')]
    [string]$RevitVersion = '2024',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '../dist')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'Front-Revit AI/BimAiAssistant'
$framework = if ($RevitVersion -eq '2024') { 'net48' } else { 'net8.0-windows' }

& dotnet build (Join-Path $project 'BimAiAssistant.csproj') -c Release "-p:RevitVersion=$RevitVersion"
if ($LASTEXITCODE -ne 0) { throw "Revit $RevitVersion build failed." }

$build = Join-Path $project "bin/$RevitVersion/Release/$framework"
foreach ($name in @('BimAiAssistant.dll', 'Newtonsoft.Json.dll')) {
    if (!(Test-Path (Join-Path $build $name))) { throw "Missing build output: $name" }
}
foreach ($name in @('RevitAPI.dll', 'RevitAPIUI.dll')) {
    if (Test-Path (Join-Path $build $name)) { throw "Revit runtime assembly must not be distributed: $name" }
}

$temporary = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString())
$stage = Join-Path $temporary "RevitAssistant-Revit$RevitVersion"
try {
    New-Item -ItemType Directory -Path $stage -Force | Out-Null
    Copy-Item (Join-Path $build '*.dll') $stage
    Get-ChildItem $build -Filter '*.deps.json' | Copy-Item -Destination $stage
    foreach ($name in @('install.bat', 'uninstall.bat', 'install.ps1', 'uninstall.ps1', 'README.txt')) {
        Copy-Item (Join-Path $project "Installer/$name") $stage
    }
    Set-Content (Join-Path $stage 'revit-version.txt') -Value $RevitVersion -Encoding ASCII
    Copy-Item (Join-Path $root 'LICENSE') $stage
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    $archive = Join-Path $OutputDirectory "RevitAssistant-Revit$RevitVersion.zip"
    Compress-Archive -Path $stage -DestinationPath $archive -Force
    Write-Host "Package ready: $archive"
} finally {
    if (Test-Path $temporary) { Remove-Item $temporary -Recurse -Force }
}
