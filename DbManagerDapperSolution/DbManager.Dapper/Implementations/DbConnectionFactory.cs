using DbManager.Dapper.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Dapper.Implementations
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public NpgsqlConnection CreateConnection(string schema)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SET search_path TO \"{schema}\";";
            cmd.ExecuteNonQuery();

            return conn;
        }
    }
}
