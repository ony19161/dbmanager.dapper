using Npgsql;

namespace DbManager.Dapper.Interfaces
{
    public interface IDbConnectionFactory
    {
        NpgsqlConnection CreateConnection(string schema);
    }
}
