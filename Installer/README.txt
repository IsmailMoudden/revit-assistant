============================================
  BIM AI Assistant — Revit Plugin
  Version 1.0
============================================

REQUIREMENTS
------------
- Autodesk Revit 2022, 2023, 2024, or 2025
- Windows 10 / 11
- Internet connection (the plugin calls a cloud API)


INSTALLATION
------------
1. Extract this ZIP to any folder (e.g. Desktop)
2. Double-click  install.bat
3. If Windows asks "Do you want to allow this app to make changes?", click Yes
4. The script will detect your Revit version and register the plugin automatically
5. Open Autodesk Revit
6. Look for the "BIM AI" tab in the ribbon at the top
7. Click "Run AI"
8. Type a natural language instruction, for example:
     "Create a 5 meter wall from (0,0) to (5,0)"
     "Add a window at position (2, 0, 1)"
     "Add 2 doors spaced 2 meters apart at position (1, 0, 0)"
9. The plugin sends your instruction to the AI backend and executes the result in Revit


USAGE NOTES
-----------
- All dimensions are in METERS
- The plugin requires an active internet connection to reach the AI backend
- If the "BIM AI" tab does not appear, close Revit and re-run install.bat
- If Revit asks to load an unverified plugin on first launch, click "Always Load"


UNINSTALL
---------
Double-click  uninstall.bat
This removes all plugin files and addin registrations from every Revit version.


SUPPORT
-------
Backend API: https://autodesk-revit-backend.up.railway.app
GitHub:      https://github.com/IsmailMoudden/revit-ai-plugin


============================================
