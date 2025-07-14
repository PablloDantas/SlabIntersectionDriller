using ClashOpenings.Core.Domain.Entities.Identifiers;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities.Elements;

public abstract class BuildingComponent(Id id, BoundingBox boundingBox, Solid? solid = null)
{
    public Id Id { get; private set; } = id;
    public BoundingBox BoundingBox { get; private set; } = boundingBox;
    public Solid? Solid { get; private set; } = solid;
}