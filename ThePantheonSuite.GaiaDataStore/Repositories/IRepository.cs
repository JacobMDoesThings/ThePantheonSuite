using ThePantheonSuite.GaiaDataStore.Entities;

namespace ThePantheonSuite.GaiaDataStore.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetAsync(string id);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
}
