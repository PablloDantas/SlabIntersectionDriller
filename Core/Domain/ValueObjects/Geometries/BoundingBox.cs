namespace ClashOpenings.Core.Domain.ValueObjects.Geometries;

public class BoundingBox(Xyz min, Xyz max) : Geometry
{
    private Xyz Min { get; set; } = min;
    private Xyz Max { get; set; } = max;
}