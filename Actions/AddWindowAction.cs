using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class AddWindowAction
    {
        private const double MtoFt = 3.28084;

        public static void Execute(Document doc, ActionPayload action)
        {
            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, familyName: null, typeName: null, BuiltInCategory.OST_Windows);

            if (!symbol.IsActive)
                symbol.Activate();

            int    count = action.Count   ?? 1;
            double x     = (action.Position?.X ?? 0)   * MtoFt;
            double y     = (action.Position?.Y ?? 0)   * MtoFt;
            double z     = (action.Position?.Z ?? 0.9) * MtoFt;   // 0.9 m sill height default
            double step  = (action.Spacing     ?? 1.5) * MtoFt;   // 1.5 m spacing default

            Wall hostWall = ResolveHostWall(doc, action.WallId, x, y);

            for (int i = 0; i < count; i++)
            {
                doc.Create.NewFamilyInstance(
                    new XYZ(x + i * step, y, z),
                    symbol,
                    hostWall,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                );
            }
        }

        private static Wall ResolveHostWall(Document doc, string wallId, double x, double y)
        {
            if (!string.IsNullOrWhiteSpace(wallId) &&
                int.TryParse(wallId, out int id))
            {
                var element = doc.GetElement(new ElementId((long)id));
                if (element is Wall wall) return wall;

                throw new System.Exception(
                    $"wall_id {wallId} not found or is not a Wall.");
            }

            // wall_id null → auto-select nearest wall, always notify user
            var pt = new XYZ(x, y, 0);
            var walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .Cast<Wall>()
                .OrderBy(w => w.Location is LocationCurve lc ? lc.Curve.Distance(pt) : double.MaxValue)
                .ToList();

            if (walls.Count == 0)
                throw new System.Exception("No walls in document. Create a wall first.");

            Wall nearest = walls[0];
            double dist  = nearest.Location is LocationCurve lc2 ? lc2.Curve.Distance(pt) : 0;

            bool ambiguous = walls.Count > 1 &&
                             walls[1].Location is LocationCurve lc3 &&
                             System.Math.Abs(lc3.Curve.Distance(pt) - dist) < 0.3 * MtoFt;

            TaskDialog.Show("BIM AI — Wall Selection",
                (ambiguous ? "Multiple walls are close — " : "") +
                $"wall_id was null. Auto-selected wall Id={nearest.Id.Value} " +
                $"(distance {dist / MtoFt:F2} m).");

            return nearest;
        }
    }
}
