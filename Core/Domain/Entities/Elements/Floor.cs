using ClashOpenings.Core.Domain.Entities.Identifiers;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities.Elements;

public class Floor(Id id, Solid solid, float thickness)
    : BuildingComponent(id, solid)
{
    public float Thickness { get; private set; } = thickness;

    public static Floor Create(Id id, Solid solid, float thickness)
    {
        return new Floor(id, solid, thickness);
    }
}