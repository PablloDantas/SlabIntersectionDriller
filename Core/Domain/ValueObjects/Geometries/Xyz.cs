namespace ClashOpenings.Core.Domain.ValueObjects.Geometries;

public class Xyz(float x, float y, float z) : Geometry
{
    public float X { get; private set; } = x;
    public float Y { get; private set; } = y;
    public float Z { get; private set; } = z;

    public static Xyz ByCoordinates(float x, float y, float z)
    {
        return new Xyz(x, y, z);
    }

    public static Xyz ByCoordinates(float x, float y)
    {
        return new Xyz(x, y, 0);
    }
}