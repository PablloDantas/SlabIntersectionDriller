using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClashOpenings.Presentation.Commands;

[Transaction(TransactionMode.ReadOnly)]
public class SlabsOpeningsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var activeView = doc.ActiveView;

        // 1. Recupera todas as instâncias de link
        var linkInstances = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .ToList();
        if (!linkInstances.Any())
        {
            TaskDialog.Show("Erro", "Nenhuma instância de link encontrada.");
            return Result.Failed;
        }

        // 2. Obter elevação do Project Base Point
        var basePoints = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .WhereElementIsNotElementType()
            .ToElements();
        double bpElevation = 0;
        foreach (var bp in basePoints)
        {
            var param = bp.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
            if (param != null && param.HasValue)
            {
                bpElevation = param.AsDouble();
                break;
            }
        }

        // 3. Cria filtro de categorias visíveis na vista ativa
        var categoryIds = doc.Settings.Categories.Cast<Category>().Select(c => c.Id);
        var visibleCatFilters = categoryIds
            .Where(id => !activeView.GetCategoryHidden(id))
            .Select(id => new ElementCategoryFilter(id))
            .Cast<ElementFilter>()
            .ToList();
        var catFilter = new LogicalOrFilter(visibleCatFilters);

        // 4. Define volume (Outline) de coleta conforme tipo de vista
        Outline worldOutline;

        if (activeView is ViewPlan viewPlan)
        {
            // Vista 2D: usa CropBox + ViewRange
            var cropBox = viewPlan.CropBox;
            var vr = viewPlan.GetViewRange();
            var topZ = GetPlaneZ(doc, vr, PlanViewPlane.TopClipPlane, bpElevation);
            var bottomZ = GetPlaneZ(doc, vr, PlanViewPlane.BottomClipPlane, bpElevation);

            var newMin = new XYZ(cropBox.Min.X, cropBox.Min.Y, bottomZ);
            var newMax = new XYZ(cropBox.Max.X, cropBox.Max.Y, topZ);
            worldOutline = new Outline(newMin, newMax);
        }
        else if (activeView is View3D view3D && view3D.IsSectionBoxActive)
        {
            var sectBox = view3D.GetSectionBox();
            var tr = sectBox.Transform;

            // Gera os 8 vértices da section box
            var corners = new List<XYZ>();
            var min = sectBox.Min;
            var max = sectBox.Max;
            var xs = new[] { min.X, max.X };
            var ys = new[] { min.Y, max.Y };
            var zs = new[] { min.Z, max.Z };

            foreach (var x in xs)
            foreach (var y in ys)
            foreach (var z in zs)
                corners.Add(tr.OfPoint(new XYZ(x, y, z)));

            // Recalcula os limites alinhados aos eixos do mundo
            double xMin = corners.Min(p => p.X), xMax = corners.Max(p => p.X);
            double yMin = corners.Min(p => p.Y), yMax = corners.Max(p => p.Y);
            double zMin = corners.Min(p => p.Z), zMax = corners.Max(p => p.Z);

            worldOutline = new Outline(new XYZ(xMin, yMin, zMin),
                new XYZ(xMax, yMax, zMax));
        }
        else
        {
            TaskDialog.Show("Erro", "Tipo de vista não suportado. Use vista plan ou 3D.");
            return Result.Failed;
        }

        // 5. Coleta elementos de todos os links, transformando volume para o espaço de cada link
        var allCollected = new List<Element>();
        foreach (var linkInst in linkInstances)
        {
            var linkDoc = linkInst.GetLinkDocument();
            if (linkDoc == null) continue;

            // Transform do espaço mundial para o espaço do link
            var transform = linkInst.GetTotalTransform();
            var inv = transform.Inverse;
            var linkMin = inv.OfPoint(worldOutline.MinimumPoint);
            var linkMax = inv.OfPoint(worldOutline.MaximumPoint);
            var linkOutline = new Outline(linkMin, linkMax);
            var bbFilter = new BoundingBoxIntersectsFilter(linkOutline);

            // Combina filtros de categoria e bounding box no link
            var filter = new LogicalAndFilter(catFilter, bbFilter);
            var collected = new FilteredElementCollector(linkDoc)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();

            allCollected.AddRange(collected);
        }

        // 6. Exibe total de elementos coletados
        TaskDialog.Show("Resultado", $"Total de elementos coletados: {allCollected.Count}");
        return Result.Succeeded;
    }

    private double GetPlaneZ(Document doc, PlanViewRange vr, PlanViewPlane plane, double bpElevation)
    {
        try
        {
            var level = doc.GetElement(vr.GetLevelId(plane)) as Level;
            return level.Elevation + vr.GetOffset(plane) - bpElevation;
        }
        catch
        {
            var cutLevel = doc.GetElement(vr.GetLevelId(PlanViewPlane.CutPlane)) as Level;
            return cutLevel.Elevation + vr.GetOffset(PlanViewPlane.CutPlane) - bpElevation;
        }
    }
}