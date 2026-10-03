$ErrorActionPreference = 'Stop'
$version = (Get-Content (Join-Path $PSScriptRoot 'revit-version.txt') -Raw).Trim()
if ($version -notin @('2024', '2025', '2026')) { throw 'Invalid package Revit version.' }
foreach ($name in @('BimAiAssistant.dll', 'Newtonsoft.Json.dll')) {
    if (!(Test-Path (Join-Path $PSScriptRoot $name))) { throw "Incomplete package: $name" }
}

$install = Join-Path $env:LOCALAPPDATA "BIMAI/$version"
$registration = Join-Path $env:APPDATA "Autodesk/Revit/Addins/$version"
New-Item -ItemType Directory -Path $install, $registration -Force | Out-Null
Copy-Item (Join-Path $PSScriptRoot '*.dll') $install -Force
Get-ChildItem $PSScriptRoot -Filter '*.deps.json' | Copy-Item -Destination $install -Force

$document = New-Object System.Xml.XmlDocument
$declaration = $document.CreateXmlDeclaration('1.0', 'utf-8', $null)
$document.AppendChild($declaration) | Out-Null
$root = $document.CreateElement('RevitAddIns')
$document.AppendChild($root) | Out-Null
$addin = $document.CreateElement('AddIn')
$addin.SetAttribute('Type', 'Application')
$root.AppendChild($addin) | Out-Null
$fields = [ordered]@{
    Name = 'BIM AI Assistant'
    Assembly = (Join-Path $install 'BimAiAssistant.dll')
    AddInId = '8D83C886-B739-4ACD-A9DB-5B6F0F2E1234'
    FullClassName = 'BimAiAssistant.App'
    VendorId = 'BIMAI'
    VendorDescription = 'Revit Assistant'
}
foreach ($field in $fields.GetEnumerator()) {
    $node = $document.CreateElement($field.Key)
    $node.InnerText = $field.Value
    $addin.AppendChild($node) | Out-Null
}
$document.Save((Join-Path $registration 'BimAiAssistant.addin'))
Write-Host "Installed for Revit $version in $install. Restart Revit to load the add-in."
