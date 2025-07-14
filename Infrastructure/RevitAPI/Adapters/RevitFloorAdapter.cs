using Autodesk.Revit.DB;
using ClashOpenings.Core.Application.Interfaces;
using ClashOpenings.Core.Domain.Entities.Elements;
using DomainFloor = ClashOpenings.Core.Domain.Entities.Elements.Floor;
using RevitFloor = Autodesk.Revit.DB.Floor;

namespace ClashOpenings.Infrastructure.RevitAPI.Adapters;

public class RevitFloorAdapter : IBuildingComponentAdapter<Element, View>
{
    public BuildingComponent? ToDomain(Element element, View view)
    {
        if (element is not RevitFloor floor) return null;

        var id = floor.Id.ToDomain();
        var solid = floor.get_Geometry(GeometrySetup.High).ToDomainSolid();
        var boundingBox = floor.get_BoundingBox(view).ToDomain();
        var thickness = (float)floor
            .FloorType
            .get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM)
            .AsDouble();

        return DomainFloor.Create(id, boundingBox, solid, thickness);
    }
}