using ClashOpenings.Core.Domain.Entities.Identifiers;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities.Elements;

public class Opening(
    float width,
    float length,
    float height,
    Id id,
    Solid? solid = null)
    : BuildingComponent(id, solid)
{
    public float Width { get; private set; } = width;
    public float Length { get; private set; } = length;
    public float Height { get; private set; } = height;
}