using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Threading.Tasks;

using App.Core.Models;
using App.Core.Services;

using Dapper;

namespace App.Infrastructure
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IDbConnection connection;

        public DatabaseService(Settings settings)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                UserID = settings.DbServerUser,
                Password = settings.DbServerPassword,
                DataSource = settings.DbServerName
            };

            connection = new SqlConnection(connectionStringBuilder.ConnectionString);
        }

        public async Task<IEnumerable<dynamic>> Query(string query)
        {
            return await connection.QueryAsync(query);
        }
    }
}