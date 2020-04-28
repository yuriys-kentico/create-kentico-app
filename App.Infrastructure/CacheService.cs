using System.Threading;
using System.Threading.Tasks;

using App.Core.Services;

using Microsoft.Extensions.Caching.Distributed;

namespace App.Infrastructure
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache cache;

        public CacheService(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public Task<string> GetString(string key, CancellationToken token = default)
        {
            return cache.GetStringAsync(key, token);
        }

        public Task SetString(string key, string value, CancellationToken token = default)
        {
            return cache.SetStringAsync(key, value, token);
        }
    }
}