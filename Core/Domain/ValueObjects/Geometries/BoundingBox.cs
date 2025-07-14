namespace ClashOpenings.Core.Domain.ValueObjects.Geometries;

public class BoundingBox(Xyz min, Xyz max) : Geometry
{
    public Xyz Min { get; private set; } = min;
    public Xyz Max { get; private set; } = max;

    public static BoundingBox Create(Xyz min, Xyz max)
    {
        return new BoundingBox(min, max);
    }
}