using Autodesk.Revit.DB;

namespace ClashOpenings.src.Models;

public class LinkElementData(RevitLinkInstance linkInstance, List<Element> elements)
{
    public RevitLinkInstance LinkInstance { get; } = linkInstance;
    public List<Element> Elements { get; } = elements;
}