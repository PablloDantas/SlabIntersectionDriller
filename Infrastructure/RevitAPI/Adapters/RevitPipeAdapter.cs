using Autodesk.Revit.DB;
using ClashOpenings.Core.Application.Interfaces;
using ClashOpenings.Core.Domain.Entities.Elements;

namespace ClashOpenings.Infrastructure.RevitAPI.Adapters;

public class RevitPipeAdapter : IBuildingComponentAdapter<Element, View>
{
    public BuildingComponent? ToDomain(Element element, View view)
    {
        if (element is not MEPCurve pipe) return null;

        var pipeLine = pipe.Location as LocationCurve;
        var curve = pipeLine?.Curve;
        var id = pipe.Id.ToDomain();
        var startPoint = curve.GetEndPoint(0).ToDomain();
        var endPoint = curve.GetEndPoint(1).ToDomain();
        var boundingBox = pipe.get_BoundingBox(view).ToDomain();
        var solid = pipe?.get_Geometry(GeometrySetup.High).ToDomainSolid();
        var diameter = (float)pipe.Diameter;

        return Pipe.Create(id, startPoint, endPoint, boundingBox, solid, diameter);
    }
}