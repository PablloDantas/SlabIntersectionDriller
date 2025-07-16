using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace ClashOpenings.src.Services.Revit
{
    public static class RevitElementCollector
    {
        public static List<RevitLinkInstance> GetAllLinkInstances(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();
        }

        public static List<Element> GetElementsFromLink(Document doc, RevitLinkInstance linkInst, Outline worldOutline)
        {
            var linkDoc = linkInst.GetLinkDocument();
            if (linkDoc == null) return new List<Element>();

            var activeView = doc.ActiveView;
            var categoryIds = doc.Settings.Categories.Cast<Category>().Select(c => c.Id);
            var visibleCatFilters = categoryIds
                .Where(id => !activeView.GetCategoryHidden(id))
                .Select(id => new ElementCategoryFilter(id))
                .Cast<ElementFilter>()
                .ToList();
            var catFilter = new LogicalOrFilter(visibleCatFilters);

            var transform = linkInst.GetTotalTransform();
            var inv = transform.Inverse;
            var linkMin = inv.OfPoint(worldOutline.MinimumPoint);
            var linkMax = inv.OfPoint(worldOutline.MaximumPoint);
            var linkOutline = new Outline(linkMin, linkMax);
            var bbFilter = new BoundingBoxIntersectsFilter(linkOutline);
            var filter = new LogicalAndFilter(catFilter, bbFilter);

            return new FilteredElementCollector(linkDoc)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToList();
        }
    }
}
