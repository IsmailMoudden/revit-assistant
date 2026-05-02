using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class CreateColumnAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action)
        {
            // "section" (e.g. "HEA200") is the canonical field from the backend.
            // Fall back to legacy family_name / type_name if section is absent.
            string familyHint = action.Section ?? action.FamilyName;
            string typeHint   = action.Section ?? action.TypeName;

            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, familyHint, typeHint, BuiltInCategory.OST_StructuralColumns);

            if (!symbol.IsActive)
                symbol.Activate();

            Level level = LevelHelper.Resolve(doc, action.Level, out bool levelFallback);
            if (levelFallback)
                RevitLogger.Warn($"Column: level \"{action.Level}\" not found — using \"{level.Name}\".");

            double x = (action.Position?.X ?? 0) * MtoFt;
            double y = (action.Position?.Y ?? 0) * MtoFt;

            // Columns are anchored to the level — z from payload is ignored
            var origin = new XYZ(x, y, level.Elevation);

            FamilyInstance column = doc.Create.NewFamilyInstance(
                origin, symbol, level, StructuralType.Column);

            // Express height as top offset above the base level
            double height = (action.Height ?? 3.0) * MtoFt;
            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)?.Set(height);
        }
    }
}
