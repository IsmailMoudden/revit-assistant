using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimAiAssistant.Actions
{
    public static class FamilySymbolHelper
    {
        public static FamilySymbol Resolve(
            Document doc,
            string familyName,
            string typeName,
            BuiltInCategory category)
        {
            var symbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(category)
                .Cast<FamilySymbol>()
                .ToList();

            if (symbols.Count == 0)
            {
                // Hard stop — nothing to fall back to
                throw new System.Exception(
                    $"No families loaded for category {category}.\n" +
                    "Load at least one family into the project before running this action."
                );
            }

            // Exact match: family + type
            if (!string.IsNullOrWhiteSpace(familyName) && !string.IsNullOrWhiteSpace(typeName))
            {
                var exact = symbols.FirstOrDefault(s =>
                    s.FamilyName == familyName && s.Name == typeName);
                if (exact != null)
                    return exact;

                // Partial match: family only
                var byFamily = symbols.FirstOrDefault(s => s.FamilyName == familyName);
                if (byFamily != null)
                {
                    WarnFallback(
                        $"Type \"{typeName}\" not found in family \"{familyName}\".\n" +
                        $"Using type \"{byFamily.Name}\" instead."
                    );
                    return byFamily;
                }
            }
            else if (!string.IsNullOrWhiteSpace(familyName))
            {
                var byFamily = symbols.FirstOrDefault(s => s.FamilyName == familyName);
                if (byFamily != null)
                    return byFamily;
            }

            // Full fallback: first available symbol — always tell the user what was picked
            var fallback = symbols.First();
            WarnFallback(BuildFallbackMessage(familyName, typeName, fallback, symbols));
            return fallback;
        }

        private static string BuildFallbackMessage(
            string requestedFamily,
            string requestedType,
            FamilySymbol chosen,
            List<FamilySymbol> available)
        {
            string requested = string.IsNullOrWhiteSpace(requestedFamily)
                ? "(none specified)"
                : string.IsNullOrWhiteSpace(requestedType)
                    ? requestedFamily
                    : $"{requestedFamily} / {requestedType}";

            string availableNames = string.Join(", ",
                available.Take(5).Select(s => $"{s.FamilyName}/{s.Name}"));
            if (available.Count > 5)
                availableNames += $" … (+{available.Count - 5} more)";

            return
                $"Requested family/type: {requested}\n" +
                $"Not found in project. Using: {chosen.FamilyName} / {chosen.Name}\n\n" +
                $"Available: {availableNames}";
        }

        private static void WarnFallback(string message)
        {
            TaskDialog.Show("BIM AI — Family Fallback", message);
        }
    }
}
