using ClashOpenings.Core.Domain.Entities.Elements;

namespace ClashOpenings.Core.Application.Interfaces;

public interface IBuildingComponentAdapter<in T, in TU>
{
    public BuildingComponent? ToDomain(T component, TU view);
}