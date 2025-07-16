using Autodesk.Revit.DB;

namespace ClashOpenings.src.Services.Collectors;

public static class ClassCollectors
{
    public static List<RevitLinkInstance> AllLinkInstances(Document document)
    {
        return new FilteredElementCollector(document)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .ToList();
    }
}