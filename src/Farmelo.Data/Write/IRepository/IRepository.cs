using Microsoft.Data.SqlClient;

namespace Farmelo.Data.Write.IRepository;

public interface IRepository<T>
    where T : class
{
    Task AddAsync(T entity, CancellationToken ct);
    Task AddRangeAsync(List<T> entities, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task UpdateRangeAsync(List<T> entities, CancellationToken ct);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task RemoveAsync(T entity, CancellationToken ct);
    Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken ct);
    Task<int> ExecuteStoredProcedureAsync(string storedProcedureName, IEnumerable<SqlParameter>? parameters, CancellationToken ct);
}
