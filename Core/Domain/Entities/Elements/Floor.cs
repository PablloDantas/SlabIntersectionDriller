using ClashOpenings.Core.Domain.Entities.Identifiers;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities.Elements;

public class Floor(Id id, BoundingBox boundingBox, Solid solid, float thickness)
    : BuildingComponent(id, boundingBox, solid)
{
    public float Thickness { get; private set; } = thickness;

    public static Floor Create(Id id, BoundingBox boundingBox, Solid solid, float thickness)
    {
        return new Floor(id, boundingBox, solid, thickness);
    }
}