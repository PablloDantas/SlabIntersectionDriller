using Autodesk.Revit.DB;

namespace ClashOpenings.src.Services.ClashDetection;

public class ClashResult
{
    public ClashResult(
        XYZ centerPoint,
        Element element1,
        Element element2,
        double intersectionVolume,
        double thickness,
        double diameter)
    {
        CenterPoint = centerPoint;
        Element1 = element1;
        Element2 = element2;
        IntersectionVolume = intersectionVolume;
        Thickness = thickness;
        Diameter = diameter;
    }

    public XYZ CenterPoint { get; }
    public Element Element1 { get; }
    public Element Element2 { get; }
    public double IntersectionVolume { get; }
    public double Thickness { get; }
    public double Diameter { get; }

    public Floor? GetFloor()
    {
        return Element1 as Floor ?? Element2 as Floor;
    }

    public MEPCurve? GetMepCurve()
    {
        return Element1 as MEPCurve ?? Element2 as MEPCurve;
    }
}