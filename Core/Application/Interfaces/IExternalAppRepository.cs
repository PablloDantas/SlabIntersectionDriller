namespace ClashOpenings.Core.Application.Interfaces;

public interface IExternalAppRepository<out T, in TU, in TV, out TX>
{
    public IEnumerable<T> GetByCategory(TU category);
    public IEnumerable<T> GetByCategoryAndView(TU category, TV view);
    public IEnumerable<T> GetAll();
    public IEnumerable<T> GetByIds(params long[]? ids);
    public IEnumerable<TX> GetLinkedModels();
}