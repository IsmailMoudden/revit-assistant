using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class ActionDispatcher
    {
        /// <summary>
        /// Executes each action in its own sub-transaction so a single failure
        /// doesn't roll back the whole batch. Must be called inside an open Transaction.
        /// Returns one ExecutionResult per action (status "success" or "error").
        /// Fallback warnings are appended to <paramref name="warnings"/>.
        /// </summary>
        public static List<ExecutionResult> ExecuteAll(
            Document doc,
            UIApplication uiApp,
            List<ActionPayload> actions,
            List<string> warnings)
        {
            var results = new List<ExecutionResult>();

            foreach (ActionPayload action in actions)
            {
                var actionWarnings = new List<string>();
                long? revitId = null;
                string errorReason = null;

                using (var sub = new SubTransaction(doc))
                {
                    sub.Start();
                    try
                    {
                        revitId = ExecuteOne(doc, uiApp, action, actionWarnings);
                        sub.Commit();
                        warnings.AddRange(actionWarnings);
                    }
                    catch (Exception ex)
                    {
                        sub.RollBack();
                        errorReason = ex.Message;
                    }
                }

                results.Add(new ExecutionResult
                {
                    Action         = action.ActionType,
                    Status         = errorReason == null ? "success" : "error",
                    RevitId        = revitId,
                    Reason         = errorReason,
                    OriginalParams = action
                });
            }

            return results;
        }

        // Returns the ElementId.Value of the created element (for the execution_results payload).
        private static long? ExecuteOne(
            Document doc,
            UIApplication uiApp,
            ActionPayload action,
            List<string> warnings)
        {
            switch (action.ActionType)
            {
                case "create_wall":
                    return CreateWallAction.Execute(doc, action, warnings);

                case "create_column":
                    return CreateColumnAction.Execute(doc, action, warnings);

                case "create_beam":
                    return CreateBeamAction.Execute(doc, action, warnings);

                case "add_window":
                    return AddWindowAction.Execute(doc, action, warnings);

                case "add_door":
                    return AddDoorAction.Execute(doc, action, warnings);

                default:
                    throw new Exception(
                        $"Action type \"{action.ActionType}\" is not supported.");
            }
        }
    }
}
