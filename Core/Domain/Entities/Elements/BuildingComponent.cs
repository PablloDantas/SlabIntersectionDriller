using ClashOpenings.Core.Domain.Entities.Identifiers;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities.Elements;

public abstract class BuildingComponent(Id id, Solid? solid = null)
{
    public Id Id { get; private set; } = id;
    public Solid? Solid { get; private set; } = solid;
}