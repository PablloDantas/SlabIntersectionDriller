using ClashOpenings.Core.Domain.Entities.Identifiers;
using ClashOpenings.Core.Domain.ValueObjects.Geometries;

namespace ClashOpenings.Core.Domain.Entities.Elements;

public class Pipe(Id id, Line location, BoundingBox boundingBox, Solid solid, float diameter)
    : BuildingComponent(id, boundingBox, solid)
{
    public float Diameter { get; private set; } = diameter;
    public Line Location { get; private set; } = location;

    public static Pipe Create(Id id, Line location, BoundingBox boundingBox, Solid solid, float diameter)
    {
        return new Pipe(id, location, boundingBox, solid, diameter);
    }

    public static Pipe Create(Id id, Xyz startPoint, Xyz endPoint, BoundingBox boundingBox, Solid solid, float diameter)
    {
        var location = Line.ByPoints(startPoint, endPoint);
        return new Pipe(id, location, boundingBox, solid, diameter);
    }
}