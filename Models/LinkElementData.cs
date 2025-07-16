using Autodesk.Revit.DB;

namespace ClashOpenings.Models;

/// <summary>
///     Representa um conjunto de dados contendo uma instância de link do Revit e uma lista de elementos associados a ela.
///     Usado para agrupar dados de elementos de um link específico para processamento, como detecção de conflitos.
/// </summary>
/// <param name="linkInstance">A instância de link do Revit à qual os elementos pertencem.</param>
/// <param name="elements">A lista de elementos contidos na instância de link.</param>
public class LinkElementData(RevitLinkInstance linkInstance, List<Element> elements)
{
    /// <summary>
    ///     Obtém a instância de link do Revit associada a estes dados.
    /// </summary>
    public RevitLinkInstance LinkInstance { get; } = linkInstance;

    /// <summary>
    ///     Obtém a lista de elementos contidos nesta instância de link.
    /// </summary>
    public List<Element> Elements { get; } = elements;
}