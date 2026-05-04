using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class CreateBeamAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action, List<string> warnings)
        {
            string familyHint = action.Section ?? action.FamilyName;
            string typeHint   = action.Section ?? action.TypeName;

            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, familyHint, typeHint, BuiltInCategory.OST_StructuralFraming, warnings);

            if (!symbol.IsActive) symbol.Activate();

            Level level = LevelHelper.Resolve(doc, action.Level, out bool levelFallback);
            if (levelFallback)
                RevitLogger.Warn(warnings,
                    $"Beam: level \"{action.Level}\" not found — using \"{level.Name}\".");

            // Backend sends absolute z in meters — convert directly
            double sx = (action.Start?.X ?? 0) * MtoFt;
            double sy = (action.Start?.Y ?? 0) * MtoFt;
            double sz = (action.Start?.Z ?? 0) * MtoFt;
            double ex = (action.End?.X   ?? 5) * MtoFt;
            double ey = (action.End?.Y   ?? 0) * MtoFt;
            double ez = (action.End?.Z   ?? 0) * MtoFt;

            var line = Line.CreateBound(new XYZ(sx, sy, sz), new XYZ(ex, ey, ez));
            doc.Create.NewFamilyInstance(line, symbol, level, StructuralType.Beam);
        }
    }
}
