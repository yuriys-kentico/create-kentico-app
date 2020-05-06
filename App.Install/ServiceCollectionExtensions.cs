using App.Core;

using Microsoft.Extensions.DependencyInjection;

namespace App.Install
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInstall(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IIisTask, IisTask>()
                .AddSingleton<IDatabaseTask, DatabaseTask>()
                .AddSingleton<IInstallTask, InstallTask>()
                .AddHttpClient<IInstallTask, InstallTask>();

            serviceCollection
                .AddSingleton<IHotfixTask, HotfixTask>()
                .AddHttpClient<IHotfixTask, HotfixTask>();

            return serviceCollection;
        }
    }
}