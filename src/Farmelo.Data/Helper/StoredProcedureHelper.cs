using Microsoft.Data.SqlClient;
using System.Data;

namespace Farmelo.Data.Helper;

internal static class StoredProcedureHelper
{
    public static string BuildStoredProcedureCommand(
        string storedProcedureName,
        IEnumerable<SqlParameter>? parameters)
    {
        if (parameters == null || !parameters.Any())
        {
            return $"EXEC {storedProcedureName}";
        }

        var parameterNames = parameters.Select(p =>
            p.Direction == ParameterDirection.Output
                ? $"{p.ParameterName} OUTPUT"
                : p.ParameterName);

        return $"EXEC {storedProcedureName} {string.Join(", ", parameterNames)}";
    }
}
