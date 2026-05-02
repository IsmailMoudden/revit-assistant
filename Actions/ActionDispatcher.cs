using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class ActionDispatcher
    {
        /// <summary>
        /// Executes a list of actions sequentially.
        /// Must be called inside an open Transaction.
        /// Throws on the first unrecoverable error — the caller must roll back.
        /// </summary>
        /// <returns>Number of actions successfully executed.</returns>
        public static int ExecuteAll(Document doc, UIApplication uiApp, List<ActionPayload> actions)
        {
            int executed = 0;

            foreach (ActionPayload action in actions)
            {
                ExecuteOne(doc, uiApp, action);
                executed++;
            }

            return executed;
        }

        private static void ExecuteOne(Document doc, UIApplication uiApp, ActionPayload action)
        {
            switch (action.ActionType)
            {
                case "create_wall":
                    CreateWallAction.Execute(doc, action);
                    break;

                case "create_column":
                    CreateColumnAction.Execute(doc, action);
                    break;

                case "create_beam":
                    CreateBeamAction.Execute(doc, action);
                    break;

                case "add_window":
                    AddWindowAction.Execute(doc, action);
                    break;

                case "add_door":
                    AddDoorAction.Execute(doc, action);
                    break;

                default:
                    // Unknown action type — do not crash; warn and skip
                    TaskDialog.Show(
                        "BIM AI — Unknown Action",
                        $"Action type \"{action.ActionType}\" is not supported.\n" +
                        "This action was skipped. All other actions in this batch were executed."
                    );
                    break;
            }
        }
    }
}
