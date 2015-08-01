using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Owin.HealthCheck.Sql
{
    public class SqlHealthCheck : BaseHealthCheck
    {
        private readonly string _connectionString;

        public SqlHealthCheck(string name, string connectionString)
            : base(name)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("invalid connection string", nameof(connectionString));
            _connectionString = connectionString;
        }

        public override async Task<HealthCheckStatus> Check()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var cmd = new SqlCommand("select 1", connection);
                await cmd.ExecuteScalarAsync();
                return HealthCheckStatus.Passed();
            }
        }
    }
}
