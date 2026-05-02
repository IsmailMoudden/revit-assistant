using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class ActionDispatcher
    {
        public static void Execute(Document doc, UIApplication uiApp, ActionPayload action)
        {
            switch (action.ActionType)
            {
                case "create_wall":
                    CreateWallAction.Execute(doc, action);
                    break;

                case "add_window":
                    AddWindowAction.Execute(doc, action);
                    break;

                case "add_door":
                    AddDoorAction.Execute(doc, action);
                    break;

                default:
                    TaskDialog.Show(
                        "BIM AI — Unknown Action",
                        $"Action type \"{action.ActionType}\" is not supported in this version."
                    );
                    break;
            }
        }
    }
}
