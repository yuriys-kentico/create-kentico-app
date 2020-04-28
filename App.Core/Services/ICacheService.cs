using System.Threading;
using System.Threading.Tasks;

namespace App.Core.Services
{
    public interface ICacheService
    {
        public Task<string> GetString(string key, CancellationToken token = default);

        public Task SetString(string key, string value, CancellationToken token = default);
    }
}