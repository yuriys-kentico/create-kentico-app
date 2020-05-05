using System.IO;

using App.Core;
using App.Core.Services;

using Microsoft.Extensions.DependencyInjection;

using NeoSmart.Caching.Sqlite;

namespace App.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastucture(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IOutputService, OutputService>()
                .AddSingleton<ICacheService, CacheService>()
                .AddTransient<IProcessService, ProcessService>()
                .AddTransient<IDatabaseService, DatabaseService>()
                .AddTransient<IKenticoPathService, KenticoPathService>()
                .AddSingleton<ISetupTask, SetupTask>()
                .AddSqliteCache(options =>
                {
                    options.CachePath = @$"{Path.GetTempPath()}\create-kentico-app.db";
                });

            return serviceCollection;
        }
    }
}