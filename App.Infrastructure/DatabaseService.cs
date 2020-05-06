using System.Collections.Generic;

using System.Data.SqlClient;
using System.Threading.Tasks;

using App.Core.Models;
using App.Core.Services;
using Dapper;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace App.Infrastructure
{
    public class DatabaseService : IDatabaseService
    {
        private readonly QueryFactory database;

        private Query QueryBuilder { get; set; } = new Query();

        public DatabaseService(Settings settings)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                UserID = settings.DatabaseServerUser,
                Password = settings.DatabaseServerPassword,
                DataSource = settings.DatabaseServerName
            };

            database = new QueryFactory(new SqlConnection(connectionStringBuilder.ConnectionString), new SqlServerCompiler());
        }

        public IDatabaseService Select(params string[] columns)
        {
            QueryBuilder.Select(columns);

            return this;
        }

        public IDatabaseService From(string tableName)
        {
            QueryBuilder.From(tableName);

            return this;
        }

        public IDatabaseService Where(string column, string op, object value)
        {
            QueryBuilder.Where(column, op, value);

            return this;
        }

        public async Task<IEnumerable<dynamic>> Query()
        {
            var result = await database.FromQuery(QueryBuilder).GetAsync();

            QueryBuilder = new Query();

            return result;
        }

        public async Task<int> Update(object data)
        {
            var result = await database.FromQuery(QueryBuilder).UpdateAsync(data);

            QueryBuilder = new Query();

            return result;
        }
    }
}