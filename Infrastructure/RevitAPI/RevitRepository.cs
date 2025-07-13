using Autodesk.Revit.DB;
using ClashOpenings.Core.Application.Interfaces;

namespace ClashOpenings.Infrastructure.RevitAPI;

public class RevitRepository(Document? document)
    : IExternalAppRepository<Element, BuiltInCategory, View, RevitLinkInstance>
{
    private readonly Document _document = document ?? throw new ArgumentNullException(nameof(document));

    public IEnumerable<Element> GetByCategory(BuiltInCategory category)
    {
        return new FilteredElementCollector(_document)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .ToElements();
    }

    public IEnumerable<Element> GetByCategoryAndView(BuiltInCategory category, View view)
    {
        if (view == null) throw new ArgumentException($"View '{view?.Name}' not exists in model.", nameof(view));

        return new FilteredElementCollector(_document, view.Id)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .ToElements();
    }

    public IEnumerable<Element> GetAll()
    {
        return new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .ToElements();
    }

    public IEnumerable<Element> GetByIds(params long[]? ids)
    {
        if (ids == null || ids.Length == 0)
            return [];

        return ids
            .Select(id => _document.GetElement(new ElementId(id)))
            .Where(element => element != null);
    }

    public IEnumerable<RevitLinkInstance> GetLinkedModels()
    {
        return new FilteredElementCollector(_document)
            .OfClass(typeof(RevitLinkInstance))
            .OfType<RevitLinkInstance>();
    }
}