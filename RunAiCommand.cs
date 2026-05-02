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
        private const int MaxClarificationRounds = 5;

        // Session history — stateless backend, plugin owns the conversation
        // Persists for the lifetime of the Revit session; cleared by "Clear" in the dialog
        private static readonly List<ConversationMessage> _sessionHistory =
            new List<ConversationMessage>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument    uiDoc = uiApp.ActiveUIDocument;
            Document      doc   = uiDoc.Document;

            string selectedLevel = ResolveActiveLevel(uiDoc, doc);

            string instruction = InputDialog.Show();
            if (instruction == null)
                return Result.Cancelled;

            ActionResponse response = RunClarificationLoop(
                instruction, selectedLevel, out bool cancelled);

            if (cancelled) return Result.Cancelled;
            if (response  == null) return Result.Failed;

            // Append to session history AFTER a successful round-trip
            _sessionHistory.Add(new ConversationMessage { Role = "user",      Content = instruction });
            _sessionHistory.Add(new ConversationMessage { Role = "assistant", Content = response.RawLlmOutput ?? "" });

            // Execute all actions in one atomic transaction
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

            string summary = BuildSummary(response);
            InputDialog.RecordAction(instruction, summary);

            TaskDialog.Show("BIM AI — Done",
                $"{executed} element(s) created.\n\n{summary}\n\nInstruction: {instruction}");

            return Result.Succeeded;
        }

        // ── Clarification loop ────────────────────────────────────────────────

        private ActionResponse RunClarificationLoop(
            string instruction, string selectedLevel, out bool cancelled)
        {
            cancelled = false;

            // First call — no answers yet
            var request = new BimRequest
            {
                Instruction   = instruction,
                SelectedLevel = selectedLevel,
                Answers       = null,
                History       = new List<ConversationMessage>(_sessionHistory)
            };

            ActionResponse response;
            try { response = BimApiClient.Post(request); }
            catch (Exception ex)
            {
                TaskDialog.Show("BIM AI — Network Error", ex.Message);
                return null;
            }

            for (int round = 0; round < MaxClarificationRounds; round++)
            {
                if (response.Status == "ok")
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

                if (response.Status == "needs_clarification")
                {
                    if (response.Questions == null || response.Questions.Count == 0)
                    {
                        TaskDialog.Show("BIM AI — Error",
                            "Backend requested clarification but provided no questions.");
                        return null;
                    }

                    Dictionary<string, object> answers = ClarificationDialog.Show(response.Questions);
                    if (answers == null) { cancelled = true; return null; }

                    request = new BimRequest
                    {
                        Instruction   = instruction,
                        SelectedLevel = selectedLevel,
                        Answers       = answers,
                        History       = new List<ConversationMessage>(_sessionHistory)
                    };

                    try { response = BimApiClient.Post(request); }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("BIM AI — Network Error", ex.Message);
                        return null;
                    }
                    continue;
                }

                // Unknown status
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

        /// <summary>Clears the in-memory conversation history.</summary>
        public static void ClearHistory() => _sessionHistory.Clear();

        private static string ResolveActiveLevel(UIDocument uiDoc, Document doc)
        {
            try
            {
                if (uiDoc.ActiveView?.GenLevel != null)
                    return uiDoc.ActiveView.GenLevel.Name;
            }
            catch { }

            try
            {
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .OrderBy(l => l.Elevation)
                    .FirstOrDefault()?.Name ?? "Level 1";
            }
            catch { return "Level 1"; }
        }

        private static string BuildSummary(ActionResponse response)
        {
            var sb = new StringBuilder();
            foreach (var g in response.Actions.GroupBy(a => a.ActionType).OrderBy(g => g.Key))
                sb.AppendLine($"• {ActionLabel(g.Key)} × {g.Count()}");
            return sb.ToString().TrimEnd();
        }

        private static string ActionLabel(string t)
        {
            switch (t)
            {
                case "create_wall":   return "Wall";
                case "create_column": return "Column";
                case "create_beam":   return "Beam";
                case "add_window":    return "Window";
                case "add_door":      return "Door";
                default:              return t;
            }
        }
    }
}
