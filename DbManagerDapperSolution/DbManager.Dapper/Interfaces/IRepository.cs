namespace DbManager.Dapper.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>Returns non-deleted rows, paged. <paramref name="limit"/> is capped by the repository.</summary>
        Task<IEnumerable<TEntity>> GetAllAsync(int limit = 100, int offset = 0, CancellationToken ct = default);
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<TEntity>> FindByAsync(IReadOnlyDictionary<string, object> conditions, int limit = 100, int offset = 0, CancellationToken ct = default);
        Task InsertAsync(TEntity entity, CancellationToken ct = default);
        Task UpdateAsync(Guid id, object updatedFields, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}
