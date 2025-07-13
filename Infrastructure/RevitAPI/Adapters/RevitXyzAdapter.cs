using RevitXyz = Autodesk.Revit.DB.XYZ;
using DomainXyz = ClashOpenings.Core.Domain.ValueObjects.Geometries.Xyz;

namespace ClashOpenings.Infrastructure.RevitAPI.Adapters;

public static class RevitXyzAdapter
{
    public static DomainXyz? ToDomain(this RevitXyz? revitXyz)
    {
        return revitXyz is null
            ? null
            : DomainXyz.ByCoordinates((float)revitXyz.X, (float)revitXyz.Y, (float)revitXyz.Z);
    }
}