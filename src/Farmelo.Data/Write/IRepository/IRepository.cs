using Microsoft.Data.SqlClient;

namespace Farmelo.Data.Write.IRepository;

public interface IRepository<T>
    where T : class
{
    Task<T?> FindAsync(object keyValue, CancellationToken ct);
    Task<T?> FindAsync(object[] keyValues, CancellationToken ct);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    Task AddAsync(T entity, CancellationToken ct);
    Task AddRangeAsync(List<T> entities, CancellationToken ct);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task UpdateRangeAsync(List<T> entities, CancellationToken ct);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task RemoveAsync(T entity, CancellationToken ct);
    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, IEnumerable<SqlParameter>? parameters, CancellationToken ct);
}
