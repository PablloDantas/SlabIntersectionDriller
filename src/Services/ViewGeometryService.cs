using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClashOpenings.src.Services;

public class ViewGeometryService
{
    private readonly View _activeView;
    private readonly double _basePointElevation;
    private readonly Document _doc;

    public ViewGeometryService(Document doc)
    {
        _doc = doc;
        _activeView = doc.ActiveView;
        _basePointElevation = GetBasePointElevation();
    }

    public Outline GetSearchVolume()
    {
        if (_activeView is ViewPlan viewPlan) return GetSearchVolumeForViewPlan(viewPlan);

        if (_activeView is View3D view3D && view3D.IsSectionBoxActive) return GetSearchVolumeForView3D(view3D);

        TaskDialog.Show("Error", "Unsupported view type. Use a plan view or a 3D view with an active section box.");
        return null;
    }

    private Outline GetSearchVolumeForViewPlan(ViewPlan viewPlan)
    {
        var cropBox = viewPlan.CropBox;
        var vr = viewPlan.GetViewRange();
        var topZ = GetPlaneZ(_doc, vr, PlanViewPlane.TopClipPlane, _basePointElevation);
        var bottomZ = GetPlaneZ(_doc, vr, PlanViewPlane.BottomClipPlane, _basePointElevation);
        var newMin = new XYZ(cropBox.Min.X, cropBox.Min.Y, bottomZ);
        var newMax = new XYZ(cropBox.Max.X, cropBox.Max.Y, topZ);
        return new Outline(newMin, newMax);
    }

    private Outline GetSearchVolumeForView3D(View3D view3D)
    {
        var sectBox = view3D.GetSectionBox();
        var tr = sectBox.Transform;
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
        double xMin = corners.Min(p => p.X), xMax = corners.Max(p => p.X);
        double yMin = corners.Min(p => p.Y), yMax = corners.Max(p => p.Y);
        double zMin = corners.Min(p => p.Z), zMax = corners.Max(p => p.Z);
        return new Outline(new XYZ(xMin, yMin, zMin), new XYZ(xMax, yMax, zMax));
    }

    private double GetBasePointElevation()
    {
        var basePoints = new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .WhereElementIsNotElementType()
            .ToElements();

        foreach (var bp in basePoints)
        {
            var param = bp.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
            if (param != null && param.HasValue) return param.AsDouble();
        }

        return 0;
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