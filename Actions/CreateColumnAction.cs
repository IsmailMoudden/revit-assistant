using System.Linq;
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
            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, action.FamilyName, action.TypeName,
                BuiltInCategory.OST_StructuralColumns);

            if (!symbol.IsActive)
                symbol.Activate();

            Level level = LevelHelper.Resolve(doc, action.Level, out bool levelFallback);
            if (levelFallback)
                RevitLogger.Warn($"Column: level \"{action.Level}\" not found — using \"{level.Name}\".");

            // Base point — position.z is ignored; columns are anchored to the level
            double x = (action.Position?.X ?? 0) * MtoFt;
            double y = (action.Position?.Y ?? 0) * MtoFt;
            var origin = new XYZ(x, y, level.Elevation);

            // Place column
            FamilyInstance column = doc.Create.NewFamilyInstance(
                origin, symbol, level, StructuralType.Column);

            // Set top offset to express height
            double height = (action.Height ?? 3.0) * MtoFt;
            column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)?.Set(height);
        }
    }
}
