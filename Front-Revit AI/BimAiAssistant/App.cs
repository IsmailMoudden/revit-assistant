using System.Reflection;
using Autodesk.Revit.UI;

namespace BimAiAssistant
{
    /// <summary>
    /// Entry point registered in the .addin manifest.
    /// Creates the "BIM AI" ribbon tab and "Run AI" button on Revit startup.
    /// </summary>
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            string tabName = "BIM AI";
            app.CreateRibbonTab(tabName);

            RibbonPanel panel = app.CreateRibbonPanel(tabName, "Assistant");

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                name:       "RunAI",
                text:       "Run AI",
                assemblyName: assemblyPath,
                className:  "BimAiAssistant.RunAiCommand"
            )
            {
                ToolTip = "Send an instruction to the BIM AI backend and execute the result in Revit."
            };

            panel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;
    }
}
