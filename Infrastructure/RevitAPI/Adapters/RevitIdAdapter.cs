using RevitId = Autodesk.Revit.DB.ElementId;
using DomainId = ClashOpenings.Core.Domain.Entities.Identifiers.Id;

namespace ClashOpenings.Infrastructure.RevitAPI.Adapters;

public static class RevitIdAdapter
{
    public static DomainId? ToDomain(this RevitId? revitId)
    {
        return revitId is null ? null : DomainId.Create(revitId.Value);
    }
}