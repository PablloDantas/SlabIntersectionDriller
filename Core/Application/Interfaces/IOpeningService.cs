namespace ClashOpenings.Core.Application.Interfaces;

public interface IOpeningService<out T>
{
    public T CreateOpening();
}