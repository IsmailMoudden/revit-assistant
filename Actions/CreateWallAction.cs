using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class CreateWallAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action)
        {
            // Backend guarantees meters — convert to Revit internal feet
            double startX = (action.Start?.X ?? 0) * MtoFt;
            double startY = (action.Start?.Y ?? 0) * MtoFt;
            double endX   = (action.End?.X   ?? 3) * MtoFt;
            double endY   = (action.End?.Y   ?? 0) * MtoFt;
            double height = (action.Height   ?? 3) * MtoFt;

            var line = Line.CreateBound(new XYZ(startX, startY, 0), new XYZ(endX, endY, 0));

            Level level = ResolveLevel(doc, action.Level, out bool usedFallback);

            if (usedFallback)
                TaskDialog.Show("BIM AI — Level Fallback",
                    $"Level \"{action.Level}\" not found. Using \"{level.Name}\" instead.");

            Wall wall = Wall.Create(doc, line, level.Id, structural: false);

            wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM)?.Set(height);
        }

        private static Level ResolveLevel(Document doc, string levelName, out bool usedFallback)
        {
            usedFallback = false;

            if (!string.IsNullOrWhiteSpace(levelName))
            {
                var match = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault(l => l.Name == levelName);

                if (match != null) return match;
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
