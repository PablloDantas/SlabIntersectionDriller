namespace ClashOpenings.Core.Domain.ValueObjects.Geometries;

public class Solid(float[] vertices, int[] faces) : Geometry
{
    public float[] Vertices { get; private set; } = vertices;
    public int[] Faces { get; private set; } = faces;

    public static Solid Create(float[] vertices, int[] face)
    {
        return new Solid(vertices, face);
    }
}