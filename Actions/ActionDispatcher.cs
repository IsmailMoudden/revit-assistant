using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class ActionDispatcher
    {
        /// <summary>
        /// Executes all actions inside an open Transaction.
        /// Returns count of executed actions and collects all fallback warnings
        /// into <paramref name="warnings"/> — no TaskDialogs are shown mid-execution.
        /// Throws on unrecoverable error; caller must roll back.
        /// </summary>
        public static int ExecuteAll(
            Document doc,
            UIApplication uiApp,
            List<ActionPayload> actions,
            List<string> warnings)
        {
            int executed = 0;
            foreach (ActionPayload action in actions)
            {
                ExecuteOne(doc, uiApp, action, warnings);
                executed++;
            }
            return executed;
        }

        private static void ExecuteOne(
            Document doc,
            UIApplication uiApp,
            ActionPayload action,
            List<string> warnings)
        {
            switch (action.ActionType)
            {
                case "create_wall":
                    CreateWallAction.Execute(doc, action, warnings);
                    break;

                case "create_column":
                    CreateColumnAction.Execute(doc, action, warnings);
                    break;

                case "create_beam":
                    CreateBeamAction.Execute(doc, action, warnings);
                    break;

                case "add_window":
                    AddWindowAction.Execute(doc, action, warnings);
                    break;

                case "add_door":
                    AddDoorAction.Execute(doc, action, warnings);
                    break;

                default:
                    warnings.Add($"Action type \"{action.ActionType}\" is not supported and was skipped.");
                    break;
            }
        }
    }
}
