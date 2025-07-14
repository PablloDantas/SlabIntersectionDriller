using ClashOpenings.Core.Domain.Entities.Elements;

namespace ClashOpenings.Core.Application.Interfaces;

public interface IBuildingComponentFactory<in T, in TU>
{
    public BuildingComponent? ToDomain(T component, TU view);
}