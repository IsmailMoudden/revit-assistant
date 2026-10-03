using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using BimAiAssistant.Models;

namespace BimAiAssistant.Actions
{
    public static class AddWindowAction
    {
        private const double MtoFt = 3.28084;

        public static long? Execute(Document doc, ActionPayload action, List<string> warnings)
        {
            FamilySymbol symbol = FamilySymbolHelper.Resolve(
                doc, familyName: null, typeName: null, BuiltInCategory.OST_Windows, warnings);

            if (!symbol.IsActive) symbol.Activate();

            int    count = action.Count   ?? 1;
            double x     = (action.Position?.X ?? 0)   * MtoFt;
            double y     = (action.Position?.Y ?? 0)   * MtoFt;
            double z     = (action.Position?.Z ?? 0.9) * MtoFt;
            double step  = (action.Spacing     ?? 1.5) * MtoFt;

            Wall hostWall = ResolveHostWall(doc, action.WallId, x, y, warnings);

            long? lastId = null;
            for (int i = 0; i < count; i++)
            {
                var inst = doc.Create.NewFamilyInstance(
                    new XYZ(x + i * step, y, z),
                    symbol,
                    hostWall,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                lastId = inst.Id.Value;
            }
            return lastId;
        }

        private static Wall ResolveHostWall(
            Document doc, string wallId, double x, double y, List<string> warnings)
        {
            if (!string.IsNullOrWhiteSpace(wallId) && int.TryParse(wallId, out int id))
            {
                var element = doc.GetElement(new ElementId((long)id));
                if (element is Wall wall) return wall;
                throw new System.Exception($"wall_id {wallId} not found or is not a Wall.");
            }

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

            warnings.Add(
                (ambiguous ? "Multiple walls are close — " : "") +
                $"No wall_id specified. Auto-selected wall Id={nearest.Id.Value} " +
                $"(distance {dist / MtoFt:F2} m).");

            return nearest;
        }
    }
}
