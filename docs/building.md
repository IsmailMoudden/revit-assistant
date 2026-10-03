# Revit Builds

| Revit | Framework | API package version |
| --- | --- | --- |
| 2024 | `net48` | `2024.0.0` |
| 2025 | `net8.0-windows` | `2025.0.2` |
| 2026 | `net8.0-windows` | `2026.0.4` |

The API packages are pinned to baseline releases. Revit provides its own runtime
assemblies; the packages are compile-time references and are excluded from installers.
Autodesk documents the .NET 8 requirement for
[Revit 2025](https://help.autodesk.com/cloudhelp/2025/CHS/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_NET8_Update_html.html)
and [Revit 2026](https://help.autodesk.com/cloudhelp/2026/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Welcome_to_the_Revit_Platform_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Welcome_to_the_Revit_Platform_API_Development_Requirements_html.html).

## Compile

Install the .NET 8 SDK. Build each version independently:

```powershell
dotnet build "Front-Revit AI/BimAiAssistant/BimAiAssistant.csproj" -c Release -p:RevitVersion=2024
dotnet build "Front-Revit AI/BimAiAssistant/BimAiAssistant.csproj" -c Release -p:RevitVersion=2025
dotnet build "Front-Revit AI/BimAiAssistant/BimAiAssistant.csproj" -c Release -p:RevitVersion=2026
```

The default is 2024. Outputs are stored under
`Front-Revit AI/BimAiAssistant/bin/<version>/Release/<framework>/`, with
intermediate files under `obj/<version>/`. Do not reuse a DLL for another release.

## Package And Install

On Windows, run `./scripts/package-addin.ps1 -RevitVersion 2025` (or 2024/2026).
The script builds and creates `dist/RevitAssistant-Revit2025.zip`. The ZIP includes
dependencies, the MIT license, installation scripts and a `revit-version.txt` marker.

Extract the matching package and run `install.bat` with Revit closed. Registration
is written to `%APPDATA%/Autodesk/Revit/Addins/<version>/`. Binaries are installed
to `%LOCALAPPDATA%/BIMAI/<version>/`. `uninstall.bat` removes only that version.
Legacy packages used `%LOCALAPPDATA%/BIMAI/` directly; old DLLs can be removed
after migration, with all Revit instances closed.

CI produces three independent installer artifacts. Compilation does not prove
runtime compatibility: check startup, clarification dialogs, each action type and
rollback in each Revit version before publishing a tested release.

Run `./scripts/test-installers.ps1 -PackageDirectory ./dist` to verify that all
three packages install side by side and that uninstalling one leaves the others
intact. This check uses temporary profile directories and does not launch Revit.
The same check runs on Windows in CI after all three packages are built.
