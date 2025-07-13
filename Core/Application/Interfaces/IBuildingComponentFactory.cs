namespace ClashOpenings.Core.Application.Interfaces;

public interface IBuildingComponentFactory<T>
{
    public IBuildingComponentAdapter<T> CreateAdapter(T component);
}