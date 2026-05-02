using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Actions;
using BimAiAssistant.Api;
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

            // 1. Prompt user for instruction
            string instruction = InputDialog.Show("BIM AI Assistant", "Enter your instruction:");
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

            // 3. Dispatch action into a Revit transaction
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

            TaskDialog.Show("BIM AI", $"Action \"{response.Action.ActionType}\" executed successfully.");
            return Result.Succeeded;
        }
    }
}
