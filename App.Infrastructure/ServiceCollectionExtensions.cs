using System.IO;

using App.Core;
using App.Core.Context;
using App.Core.Services;
using App.Infrastructure.Context;
using App.Infrastructure.Services;
using App.Infrastructure.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NeoSmart.Caching.Sqlite;

namespace App.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastucture(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IAppContext, AppContext>()
                .AddSingleton<IOutputService, OutputService>()
                .AddSingleton<ICacheService, CacheService>()
                .AddTransient<IProcessService, ProcessService>()
                .AddTransient<IDatabaseService, DatabaseService>()
                .AddSingleton<ISetupTask, SetupTask>()
                .AddSqliteCache(options =>
                {
                    options.CachePath = @$"{Path.GetTempPath()}\create-kentico-app.db";
                });

            return serviceCollection;
        }
    }
}