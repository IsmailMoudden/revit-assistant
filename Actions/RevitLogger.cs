using Autodesk.Revit.UI;

namespace BimAiAssistant.Actions
{
    /// <summary>
    /// Non-blocking warnings shown via TaskDialog.
    /// Use for fallback decisions the user should know about but that shouldn't stop execution.
    /// </summary>
    public static class RevitLogger
    {
        public static void Warn(string message)
        {
            TaskDialog.Show("BIM AI — Warning", message);
        }
    }
}
