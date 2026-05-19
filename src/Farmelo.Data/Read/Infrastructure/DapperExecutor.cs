using Dapper;
using Farmelo.Data.Connections;
using System.Data;

namespace Farmelo.Data.Read.Infrastructure;

public sealed class DapperExecutor : IDapperExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperExecutor(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<T?> QuerySingleAsync<T>(
        string sqlOrSp,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(
                sqlOrSp,
                parameters,
                commandType: commandType,
                cancellationToken: ct));
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sqlOrSp,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken ct = default,
        int? commandTimeoutSeconds = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryAsync<T>(
            new CommandDefinition(
                sqlOrSp,
                parameters,
                commandType: commandType,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: ct));

        return result.AsList();
    }

    public async Task<(IReadOnlyList<T> Items, int TotalCount)> QueryPagedAsync<T>(
        string sql,
        object parameters,
        CancellationToken ct = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                parameters,
                commandType: CommandType.Text,
                cancellationToken: ct));

        var items = (await multi.ReadAsync<T>()).AsList();
        var totalCount = await multi.ReadFirstAsync<int>();

        return (items, totalCount);
    }

    public async Task<TResult> QueryMultipleAsync<TResult>(
        string sql,
        object? parameters,
        Func<SqlMapper.GridReader, Task<TResult>> map,
        CancellationToken ct = default,
        int? commandTimeoutSeconds = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                parameters,
                commandType: CommandType.Text,
                commandTimeout: commandTimeoutSeconds,
                cancellationToken: ct));

        return await map(multi);
    }
}
