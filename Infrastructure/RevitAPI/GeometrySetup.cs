using Autodesk.Revit.DB;

namespace ClashOpenings.Infrastructure.RevitAPI;

public static class GeometrySetup
{
    public static Options High => new()
    {
        ComputeReferences = false,
        DetailLevel = ViewDetailLevel.Fine
    };

    public static Options Medium => new()
    {
        ComputeReferences = false,
        DetailLevel = ViewDetailLevel.Medium
    };

    public static Options Low => new()
    {
        ComputeReferences = false,
        DetailLevel = ViewDetailLevel.Coarse
    };
}