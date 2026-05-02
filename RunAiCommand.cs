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
            Document doc = uiApp.ActiveUIDocument.Document;

            // 1. Show dialog — returns null if user cancels
            string instruction = InputDialog.Show();
            if (instruction == null)
                return Result.Cancelled;

            // 2. Call backend
            ActionResponse response;
            try
            {
                response = BimApiClient.GenerateAction(instruction);
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("BIM AI — Network Error", ex.Message);
                return Result.Failed;
            }

            if (response?.Action == null)
            {
                TaskDialog.Show("BIM AI — Error", "Backend returned an empty or unrecognised response.");
                return Result.Failed;
            }

            // 3. Execute inside a Revit transaction
            using (var tx = new Transaction(doc, $"BIM AI — {response.Action.ActionType}"))
            {
                tx.Start();
                try
                {
                    ActionDispatcher.Execute(doc, uiApp, response.Action);
                    tx.Commit();
                }
                catch (System.Exception ex)
                {
                    tx.RollBack();
                    TaskDialog.Show("BIM AI — Execution Error", ex.Message);
                    return Result.Failed;
                }
            }

            // 4. Record in session history and confirm
            InputDialog.RecordAction(instruction, response.Action.ActionType);

            TaskDialog.Show(
                "BIM AI — Done",
                $"{ActionLabel(response.Action.ActionType)} executed successfully.\n\nInstruction: {instruction}"
            );

            return Result.Succeeded;
        }

        private static string ActionLabel(string actionType)
        {
            switch (actionType)
            {
                case "create_wall":  return "Wall created";
                case "add_window":   return "Window(s) added";
                case "add_door":     return "Door(s) added";
                default:             return $"Action \"{actionType}\" executed";
            }
        }
    }
}
