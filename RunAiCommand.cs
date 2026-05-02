using System;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Actions;
using BimAiAssistant.Api;
using BimAiAssistant.Models;
using BimAiAssistant.UI;

namespace BimAiAssistant
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunAiCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument    uiDoc = uiApp.ActiveUIDocument;
            Document      doc   = uiDoc.Document;

            // 1. Collect Revit context before opening the dialog
            RevitContext context = BuildContext(uiDoc, doc);

            // 2. Show dialog — returns null if user cancels
            string instruction = InputDialog.Show();
            if (instruction == null)
                return Result.Cancelled;

            // 3. Call backend (with context)
            ActionResponse response;
            try
            {
                response = BimApiClient.GenerateAction(instruction, context);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("BIM AI — Network Error", ex.Message);
                return Result.Failed;
            }

            if (response?.Actions == null || response.Actions.Count == 0)
            {
                TaskDialog.Show("BIM AI — Error",
                    "Backend returned no actions.\n\nRaw output:\n" + (response?.RawLlmOutput ?? "(empty)"));
                return Result.Failed;
            }

            // 4. Execute all actions in a single transaction — rollback everything on any failure
            int executed = 0;
            string txName = $"BIM AI — {response.Actions.Count} action(s)";

            using (var tx = new Transaction(doc, txName))
            {
                tx.Start();
                try
                {
                    executed = ActionDispatcher.ExecuteAll(doc, uiApp, response.Actions);
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.RollBack();
                    TaskDialog.Show("BIM AI — Execution Error",
                        $"Error during execution — all actions rolled back.\n\n{ex.Message}");
                    return Result.Failed;
                }
            }

            // 5. Record in history and show summary
            string summary = BuildSummary(response);
            InputDialog.RecordAction(instruction, summary);

            TaskDialog.Show("BIM AI — Done",
                $"{executed} element(s) created.\n\n{summary}\n\nInstruction: {instruction}");

            return Result.Succeeded;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static RevitContext BuildContext(UIDocument uiDoc, Document doc)
        {
            // Active view's associated level (best proxy for "current level")
            string levelName = null;
            try
            {
                var view = uiDoc.ActiveView;
                if (view?.GenLevel != null)
                    levelName = view.GenLevel.Name;

                if (levelName == null)
                {
                    // Fallback: lowest level in project
                    levelName = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .OrderBy(l => l.Elevation)
                        .FirstOrDefault()?.Name;
                }
            }
            catch { /* non-critical — context is best-effort */ }

            int selectionCount = 0;
            try { selectionCount = uiDoc.Selection.GetElementIds().Count; }
            catch { }

            return new RevitContext
            {
                SelectedLevel  = levelName,
                SelectionCount = selectionCount
            };
        }

        private static string BuildSummary(ActionResponse response)
        {
            var sb = new StringBuilder();
            var groups = response.Actions
                .GroupBy(a => a.ActionType)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                int count = group.Count();
                sb.AppendLine($"• {ActionLabel(group.Key)} × {count}");
            }

            return sb.ToString().TrimEnd();
        }

        private static string ActionLabel(string actionType)
        {
            switch (actionType)
            {
                case "create_wall":    return "Wall";
                case "create_column":  return "Column";
                case "create_beam":    return "Beam";
                case "add_window":     return "Window";
                case "add_door":       return "Door";
                default:               return actionType;
            }
        }
    }
}
