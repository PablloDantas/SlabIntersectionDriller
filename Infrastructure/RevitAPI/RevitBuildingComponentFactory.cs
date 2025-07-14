using Autodesk.Revit.DB;
using ClashOpenings.Core.Application.Interfaces;
using ClashOpenings.Core.Domain.Entities.Elements;
using ClashOpenings.Infrastructure.RevitAPI.Adapters;

namespace ClashOpenings.Infrastructure.RevitAPI;

public class RevitBuildingComponentFactory : IBuildingComponentFactory<Element, View>
{
    private readonly Dictionary<BuiltInCategory, Func<IBuildingComponentAdapter<Element, View>>> _adapterRegistry =
        new()
        {
            { BuiltInCategory.OST_PipeCurves, () => new RevitPipeAdapter() },
            { BuiltInCategory.OST_Floors, () => new RevitFloorAdapter() }
        };

    public BuildingComponent? ToDomain(Element? element, View view)
    {
        if (element?.Category == null)
            throw new ArgumentNullException(nameof(element), "Element or its category cannot be null.");

        var category = element.Category.BuiltInCategory;
        if (!_adapterRegistry.TryGetValue(category, out var createAdapter))
            throw new NotSupportedException($"Element category '{element.Category.Name}' is not supported.");

        var adapter = createAdapter();
        return adapter.ToDomain(element, view);
    }
}