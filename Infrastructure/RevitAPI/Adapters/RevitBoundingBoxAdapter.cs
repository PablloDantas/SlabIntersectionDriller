using RevitBoundingBox = Autodesk.Revit.DB.BoundingBoxXYZ;
using DomainBoundingBox = ClashOpenings.Core.Domain.ValueObjects.Geometries.BoundingBox;

namespace ClashOpenings.Infrastructure.RevitAPI.Adapters;

public static class RevitBoundingBoxAdapter
{
    public static DomainBoundingBox? ToDomain(this RevitBoundingBox? revitBoundingBox)
    {
        if (revitBoundingBox is null) return null;

        var min = revitBoundingBox.Min.ToDomain();
        var max = revitBoundingBox.Max.ToDomain();

        if (min is null || max is null) return null;

        return DomainBoundingBox.Create(min, max);
    }
}