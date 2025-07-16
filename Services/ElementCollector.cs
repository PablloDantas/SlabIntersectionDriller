using Autodesk.Revit.DB;

namespace ClashOpenings.Services;

/// <summary>
///     Fornece métodos utilitários para coletar elementos de documentos do Revit,
///     incluindo instâncias de links e elementos dentro de links.
/// </summary>
public static class ElementCollector
{
    /// <summary>
    ///     Obtém todas as instâncias de links do Revit presentes no documento fornecido.
    /// </summary>
    /// <param name="doc">O documento do Revit a ser inspecionado.</param>
    /// <returns>Uma lista de objetos <see cref="RevitLinkInstance" />.</returns>
    public static List<RevitLinkInstance> GetAllLinkInstances(Document doc)
    {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .ToList();
    }

    /// <summary>
    ///     Coleta elementos de um documento vinculado do Revit, filtrando por categoria visível
    ///     e pela interseção com um contorno de caixa delimitadora no espaço do modelo hospedeiro.
    /// </summary>
    /// <param name="doc">O documento do Revit hospedeiro (do qual a vista ativa e as categorias são consideradas).</param>
    /// <param name="linkInst">A instância do link do Revit de onde os elementos serão coletados.</param>
    /// <param name="worldOutline">
    ///     O contorno da caixa delimitadora no sistema de coordenadas do mundo do Revit
    ///     (documento hospedeiro) para filtrar elementos por sua localização.
    /// </param>
    /// <returns>Uma lista de objetos <see cref="Element" /> que atendem aos critérios de filtro.</returns>
    public static List<Element> GetElementsFromLink(Document doc, RevitLinkInstance linkInst, Outline worldOutline)
    {
        var linkDoc = linkInst.GetLinkDocument();
        if (linkDoc == null) return new List<Element>();

        var activeView = doc.ActiveView;
        var categoryIds = doc.Settings.Categories.Cast<Category>().Select(c => c.Id);
        var visibleCatFilters = categoryIds
            .Where(id => !activeView.GetCategoryHidden(id))
            .Select(id => new ElementCategoryFilter(id))
            .Cast<ElementFilter>()
            .ToList();
        var catFilter = new LogicalOrFilter(visibleCatFilters);

        var transform = linkInst.GetTotalTransform();
        var inv = transform.Inverse;
        var linkMin = inv.OfPoint(worldOutline.MinimumPoint);
        var linkMax = inv.OfPoint(worldOutline.MaximumPoint);
        var linkOutline = new Outline(linkMin, linkMax);
        var bbFilter = new BoundingBoxIntersectsFilter(linkOutline);
        var filter = new LogicalAndFilter(catFilter, bbFilter);

        return new FilteredElementCollector(linkDoc)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .ToList();
    }
}