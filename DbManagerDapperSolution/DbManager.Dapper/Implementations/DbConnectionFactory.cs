using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DbManager.Dapper.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DbManager.Dapper.Implementations
{
    /// <summary>
    /// Register as a singleton. Holds one <see cref="NpgsqlDataSource"/> (and therefore one
    /// connection pool) per tenant schema. The schema is applied through the connection's
    /// Search Path startup parameter, so no session state is mutated after checkout and
    /// nothing can leak between tenants through a pooled connection.
    /// </summary>
    public sealed partial class DbConnectionFactory : IDbConnectionFactory, IDisposable, IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly ConcurrentDictionary<string, NpgsqlDataSource> _dataSources = new(StringComparer.Ordinal);

        // Postgres identifiers: max 63 bytes. Restrict to a conservative safe subset.
        [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]{0,62}$")]
        private static partial Regex SchemaPattern();

        public DbConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public NpgsqlConnection CreateConnection(string schema)
        {
            if (string.IsNullOrEmpty(schema) || !SchemaPattern().IsMatch(schema))
                throw new ArgumentException("Invalid schema name.", nameof(schema));

            var dataSource = _dataSources.GetOrAdd(schema, static (s, cs) =>
            {
                var builder = new NpgsqlConnectionStringBuilder(cs) { SearchPath = s };
                return NpgsqlDataSource.Create(builder.ConnectionString);
            }, _connectionString);

            return dataSource.CreateConnection();
        }

        public void Dispose()
        {
            foreach (var ds in _dataSources.Values) ds.Dispose();
            _dataSources.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var ds in _dataSources.Values) await ds.DisposeAsync();
            _dataSources.Clear();
        }
    }
}
