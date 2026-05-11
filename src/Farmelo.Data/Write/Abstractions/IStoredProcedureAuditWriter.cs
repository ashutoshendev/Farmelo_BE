#pragma warning disable S107
namespace Farmelo.Data.Write.Abstractions;

public interface IStoredProcedureAuditWriter
{
    Task WriteEntityAuditAsync<T>(
        string tableName,
        string companyCode,
        string recordKey,
        T? before,
        T? after,
        CancellationToken ct,
        string? remarks = null,
        IEnumerable<string>? excludedProperties = null,
        string? moduleCode = null,
        string? moduleName = null)
        where T : class;

    Task WriteCollectionAuditAsync<T>(
        string tableName,
        string companyCode,
        IReadOnlyCollection<T> beforeRows,
        IReadOnlyCollection<T> afterRows,
        Func<T, string> recordKeySelector,
        CancellationToken ct,
        string? remarks = null,
        IEnumerable<string>? excludedProperties = null,
        string? moduleCode = null,
        string? moduleName = null)
        where T : class;
}
#pragma warning restore S107
