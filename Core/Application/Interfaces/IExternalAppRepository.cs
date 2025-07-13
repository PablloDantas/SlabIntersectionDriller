namespace ClashOpenings.Core.Application.Interfaces;

public interface IExternalAppRepository<T, U, V, X>
{
    public IEnumerable<T> GetByCategory(U category);
    public IEnumerable<T> GetByCategoryAndView(U category, V view);
    public IEnumerable<T> GetAll();
    public IEnumerable<T> GetByIds(params long[]? ids);
    public IEnumerable<X> GetLinkedModels();
}