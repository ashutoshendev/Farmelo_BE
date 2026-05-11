using Farmelo.Data.Helper;
using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.IRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.Data.Write.Repository;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly FarmeloDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(FarmeloDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken ct)
    {
        await DbSet.AddAsync(entity, ct);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(List<T> entities, CancellationToken ct)
    {
        await DbSet.AddRangeAsync(entities, ct);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(T entity, CancellationToken ct)
    {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(List<T> entities, CancellationToken ct)
    {
        DbSet.UpdateRange(entities);
        await DbContext.SaveChangesAsync(ct);
    }

    public void Remove(T entity)
    {
        DbSet.Remove(entity);
        DbContext.SaveChanges();
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
        DbContext.SaveChanges();
    }

    public async Task RemoveAsync(T entity, CancellationToken ct)
    {
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken ct)
    {
        DbSet.RemoveRange(entities);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task<int> ExecuteStoredProcedureAsync(
        string storedProcedureName,
        IEnumerable<SqlParameter>? parameters,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storedProcedureName))
        {
            throw new ArgumentException("Stored procedure name cannot be null", nameof(storedProcedureName));
        }

        var sql = StoredProcedureHelper.BuildStoredProcedureCommand(storedProcedureName, parameters);

        return await DbContext.Database.ExecuteSqlRawAsync(
            sql,
            parameters?.ToArray() ?? Array.Empty<object>(),
            ct);
    }
}
