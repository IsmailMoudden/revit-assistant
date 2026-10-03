Revit Assistant - Windows add-in
===============================

Separate packages target Revit 2024, 2025 and 2026 on Windows.
Use the package matching your installed release. Runtime validation is ongoing.

INSTALL
1. Extract the complete matching ZIP and close Revit.
2. Run install.bat. Keep revit-version.txt beside the scripts.
3. Start your backend on http://127.0.0.1:8000.
4. Open Revit and select BIM AI > Run AI.

Each build installs into %LOCALAPPDATA%\BIMAI\<version> and registers only
that version. Multiple versions can coexist without replacing each other.

CONFIGURATION
Set BIM_BACKEND_URL to choose another backend. Set BIM_BACKEND_API_KEY if
it requires a token. Restart Revit after changing environment variables.
Model selection and provider credentials belong in the backend .env file.
See README.md and docs/configuration.md in the repository for setup.

Use a disposable model first. Dimensions use meters. Section suggestions
are heuristics and do not validate structural adequacy.

UNINSTALL
Close Revit and run uninstall.bat from this package. Other versions stay installed.
