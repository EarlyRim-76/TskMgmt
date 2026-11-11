namespace TskMgmt.Api.Interface
{
    public interface IGenericService<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(Guid id);

        Task<T> CreateAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task<T> PatchAsync(T entity);

        Task<bool> DeleteAsync(Guid id);
    }

}
