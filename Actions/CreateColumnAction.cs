using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class CreateColumnAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action, List<string> warnings)
        {
            string familyHint = action.Section ?? action.FamilyName;
            string typeHint   = action.Section ?? action.TypeName;

            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, familyHint, typeHint, BuiltInCategory.OST_StructuralColumns, warnings);

            if (!symbol.IsActive) symbol.Activate();

            Level level = LevelHelper.Resolve(doc, action.Level, out bool levelFallback);
            if (levelFallback)
                RevitLogger.Warn(warnings,
                    $"Column: level \"{action.Level}\" not found — using \"{level.Name}\".");

            double x = (action.Position?.X ?? 0) * MtoFt;
            double y = (action.Position?.Y ?? 0) * MtoFt;

            FamilyInstance column = doc.Create.NewFamilyInstance(
                new XYZ(x, y, level.Elevation), symbol, level, StructuralType.Column);

            double height = (action.Height ?? 3.0) * MtoFt;
            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)?.Set(height);
        }
    }
}
