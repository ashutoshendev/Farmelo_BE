#pragma warning disable CA1068
using Dapper;
using System.Data;

namespace Farmelo.Data.Read.Infrastructure;

public interface IDapperExecutor
{
    Task<T?> QuerySingleAsync<T>(
        string sqlOrSp,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken ct = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sqlOrSp,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken ct = default,
        int? commandTimeoutSeconds = null);

    Task<(IReadOnlyList<T> Items, int TotalCount)> QueryPagedAsync<T>(
        string sql,
        object parameters,
        CancellationToken ct = default);

    Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        object? parameters,
        Func<SqlMapper.GridReader, Task<TResult>> map,
        CancellationToken ct = default,
        int? commandTimeoutSeconds = null);
}
#pragma warning restore CA1068
