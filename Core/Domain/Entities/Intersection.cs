using ClashOpenings.Core.Domain.Entities.Elements;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities;

public class Intersection(BuildingComponent componentA, BuildingComponent componentB, Xyz intersectionPoint)
{
    public BuildingComponent ComponentA { get; private set; } = componentA;
    public BuildingComponent ComponentB { get; private set; } = componentB;
    public Xyz IntersectionPoint { get; private set; } = intersectionPoint;
}