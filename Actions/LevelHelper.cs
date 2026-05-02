using System.Linq;
using Autodesk.Revit.DB;

namespace BimAiAssistant.Actions
{
    public static class LevelHelper
    {
        public static Level Resolve(Document doc, string levelName, out bool usedFallback)
        {
            usedFallback = false;

            if (!string.IsNullOrWhiteSpace(levelName))
            {
                var match = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault(l => l.Name == levelName);

                if (match != null)
                    return match;
            }

            usedFallback = true;
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .First();
        }
    }
}
