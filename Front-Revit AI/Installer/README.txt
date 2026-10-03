Revit Assistant - Windows add-in
===============================

Experimental build for Autodesk Revit 2024 and Windows 10/11.
Other Revit releases are not supported by this package.

INSTALL
1. Place BimAiAssistant.dll and Newtonsoft.Json.dll beside install.bat.
2. Close Revit and run install.bat.
3. Start your backend on http://127.0.0.1:8000.
4. Open Revit, select BIM AI > Run AI and enter an instruction.

CONFIGURATION
Set BIM_BACKEND_URL as a Windows user environment variable to use another backend.
Set BIM_BACKEND_API_KEY if the backend requires a bearer token.
Restart Revit after changing environment variables.
Model selection and provider credentials belong in the backend's .env file.
See the repository README and docs/configuration.md for setup instructions.

Use a disposable model first. All action dimensions use meters.
Section suggestions are heuristics and do not validate structural adequacy.

UNINSTALL
Close Revit and run uninstall.bat.
