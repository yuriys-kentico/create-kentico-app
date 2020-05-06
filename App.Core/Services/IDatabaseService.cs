using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Core.Services
{
    public interface IDatabaseService
    {
        IDatabaseService Select(params string[] columns);

        IDatabaseService From(string tableName);

        IDatabaseService Where(string column, string op, object value);

        Task<IEnumerable<dynamic>> Query();

        Task<int> Update(object data);
    }
}