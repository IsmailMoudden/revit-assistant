using Autodesk.Revit.DB;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class CreateWallAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action)
        {
            double startX = (action.Start?.X ?? 0) * MtoFt;
            double startY = (action.Start?.Y ?? 0) * MtoFt;
            double endX   = (action.End?.X   ?? 3) * MtoFt;
            double endY   = (action.End?.Y   ?? 0) * MtoFt;
            double height = (action.Height   ?? 3) * MtoFt;

            var line = Line.CreateBound(new XYZ(startX, startY, 0), new XYZ(endX, endY, 0));

            Level level = LevelHelper.Resolve(doc, action.Level, out bool usedFallback);
            if (usedFallback)
                RevitLogger.Warn($"Wall: level \"{action.Level}\" not found — using \"{level.Name}\".");

            Wall wall = Wall.Create(doc, line, level.Id, structural: false);
            wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM)?.Set(height);
        }
    }
}
