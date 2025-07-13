namespace ClashOpenings.Core.Domain.ValueObjects.Geometries;

public class Line(Xyz startPoint, Xyz endPoint) : Geometry
{
    public Xyz StartPoint { get; private set; } = startPoint;
    public Xyz EndPoint { get; private set; } = endPoint;

    public static Line ByPoints(Xyz startPoint, Xyz endPoint)
    {
        return new Line(startPoint, endPoint);
    }
}