using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class CreateBeamAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action)
        {
            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, action.FamilyName, action.TypeName,
                BuiltInCategory.OST_StructuralFraming);

            if (!symbol.IsActive)
                symbol.Activate();

            Level level = LevelHelper.Resolve(doc, action.Level, out bool levelFallback);
            if (levelFallback)
                RevitLogger.Warn($"Beam: level \"{action.Level}\" not found — using \"{level.Name}\".");

            double sx = (action.Start?.X ?? 0) * MtoFt;
            double sy = (action.Start?.Y ?? 0) * MtoFt;
            double sz = (action.Start?.Z ?? 0) * MtoFt;
            double ex = (action.End?.X   ?? 5) * MtoFt;
            double ey = (action.End?.Y   ?? 0) * MtoFt;
            double ez = (action.End?.Z   ?? 0) * MtoFt;

            // Beams live at the level elevation; z offset from the payload lifts them above it
            double elevation = level.Elevation;
            var startPt = new XYZ(sx, sy, elevation + sz);
            var endPt   = new XYZ(ex, ey, elevation + ez);

            var line = Line.CreateBound(startPt, endPt);

            doc.Create.NewFamilyInstance(line, symbol, level, StructuralType.Beam);
        }
    }
}
