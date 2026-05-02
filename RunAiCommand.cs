using System;
using System.Collections.Generic;
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
        private const int MaxClarificationRounds = 5; // safety cap — prevents infinite loops

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument    uiDoc = uiApp.ActiveUIDocument;
            Document      doc   = uiDoc.Document;

            // 1. Collect Revit context
            RevitContext context = BuildContext(uiDoc, doc);

            // 2. Get instruction from user
            string instruction = InputDialog.Show();
            if (instruction == null)
                return Result.Cancelled;

            // 3. Clarification loop
            ActionResponse response = RunClarificationLoop(instruction, context, out bool cancelled);

            if (cancelled)
                return Result.Cancelled;

            if (response == null)
                return Result.Failed;

            // 4. Execute all actions in one transaction
            int executed = 0;
            using (var tx = new Transaction(doc, $"BIM AI — {response.Actions.Count} action(s)"))
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
                        $"All actions rolled back.\n\n{ex.Message}");
                    return Result.Failed;
                }
            }

            // 5. Summary
            string summary = BuildSummary(response);
            InputDialog.RecordAction(instruction, summary);

            TaskDialog.Show("BIM AI — Done",
                $"{executed} element(s) created.\n\n{summary}\n\nInstruction: {instruction}");

            return Result.Succeeded;
        }

        // ── Clarification loop ────────────────────────────────────────────────

        /// <summary>
        /// Sends the instruction, handles needs_clarification responses by asking the user,
        /// then re-sends with answers until status == "ok" or the user cancels.
        /// Returns null (and sets cancelled=false) on network/parse error.
        /// </summary>
        private static ActionResponse RunClarificationLoop(
            string instruction, RevitContext context, out bool cancelled)
        {
            cancelled = false;

            ActionResponse response;

            // First call
            try
            {
                response = BimApiClient.GenerateAction(instruction, context);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("BIM AI — Network Error", ex.Message);
                return null;
            }

            for (int round = 0; round < MaxClarificationRounds; round++)
            {
                // ── status == "ok" ────────────────────────────────────────────
                if (response.Status == "ok" || IsDirectResponse(response))
                {
                    if (response.Actions == null || response.Actions.Count == 0)
                    {
                        TaskDialog.Show("BIM AI — Error",
                            "Backend returned status 'ok' but no actions.\n\n" +
                            (response.RawLlmOutput ?? "(empty)"));
                        return null;
                    }
                    return response;
                }

                // ── status == "needs_clarification" ───────────────────────────
                if (response.Status == "needs_clarification")
                {
                    if (response.Questions == null || response.Questions.Count == 0)
                    {
                        TaskDialog.Show("BIM AI — Error",
                            "Backend requested clarification but sent no questions.");
                        return null;
                    }

                    Dictionary<string, object> answers = ClarificationDialog.Show(response.Questions);

                    if (answers == null)
                    {
                        cancelled = true;
                        return null;
                    }

                    // Re-send with answers
                    try
                    {
                        response = BimApiClient.SendAnswers(instruction, answers);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("BIM AI — Network Error", ex.Message);
                        return null;
                    }

                    continue;
                }

                // ── unknown status ────────────────────────────────────────────
                TaskDialog.Show("BIM AI — Error",
                    $"Unexpected backend status: \"{response.Status}\".\n\n" +
                    (response.RawLlmOutput ?? "(no raw output)"));
                return null;
            }

            TaskDialog.Show("BIM AI — Error",
                $"Clarification loop did not resolve after {MaxClarificationRounds} rounds.\n" +
                "Try rephrasing your instruction.");
            return null;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        // Backwards compatibility: if backend doesn't send "status" yet but has "actions"
        private static bool IsDirectResponse(ActionResponse r) =>
            string.IsNullOrEmpty(r.Status) && r.Actions != null && r.Actions.Count > 0;

        private static RevitContext BuildContext(UIDocument uiDoc, Document doc)
        {
            string levelName = null;
            try
            {
                var view = uiDoc.ActiveView;
                if (view?.GenLevel != null)
                    levelName = view.GenLevel.Name;

                if (levelName == null)
                    levelName = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .OrderBy(l => l.Elevation)
                        .FirstOrDefault()?.Name;
            }
            catch { }

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
            var sb     = new StringBuilder();
            var groups = response.Actions.GroupBy(a => a.ActionType).OrderBy(g => g.Key);
            foreach (var g in groups)
                sb.AppendLine($"• {ActionLabel(g.Key)} × {g.Count()}");
            return sb.ToString().TrimEnd();
        }

        private static string ActionLabel(string actionType)
        {
            switch (actionType)
            {
                case "create_wall":   return "Wall";
                case "create_column": return "Column";
                case "create_beam":   return "Beam";
                case "add_window":    return "Window";
                case "add_door":      return "Door";
                default:              return actionType;
            }
        }
    }
}
