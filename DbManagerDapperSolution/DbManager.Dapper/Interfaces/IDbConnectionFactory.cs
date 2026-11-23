using Npgsql;

namespace DbManager.Dapper.Interfaces
{
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Returns an UNOPENED connection bound to the given tenant schema.
        /// The connection is checked out of the pool only while a command runs
        /// (Dapper/SqlKata open and close it automatically). The caller owns it
        /// and must dispose it (register it as scoped, or use a <c>using</c>).
        /// </summary>
        /// <exception cref="ArgumentException">The schema name is not a valid identifier.</exception>
        NpgsqlConnection CreateConnection(string schema);
    }
}
