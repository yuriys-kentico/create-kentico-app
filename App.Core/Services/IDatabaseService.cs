using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Core.Services
{
    public interface IDatabaseService
    {
        public Task<IEnumerable<dynamic>> Query(string query);
    }
}