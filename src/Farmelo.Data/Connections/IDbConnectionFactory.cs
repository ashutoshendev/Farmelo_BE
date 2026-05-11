using System.Data;

namespace Farmelo.Data.Connections;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
