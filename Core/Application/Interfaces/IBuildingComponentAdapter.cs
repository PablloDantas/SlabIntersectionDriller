using ClashOpenings.Core.Domain.Entities.Elements;

namespace ClashOpenings.Core.Application.Interfaces;

public interface IBuildingComponentAdapter<T>
{
    public BuildingComponent? ToDomain(T component);
}