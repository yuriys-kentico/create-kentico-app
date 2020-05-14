using System.Linq;
using System.Reflection;

using App.Core.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection serviceCollection, string[] args)
        {
            var aliasMappings = typeof(Settings)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(property => property.GetCustomAttribute<AliasesAttribute>()?
                    .Aliases
                    .Select(alias => (alias, property.Name)))
                .ToDictionary(pair => pair.alias, pair => pair.Name);

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args, aliasMappings)
                .Build();

            var settings = new Settings();

            ConfigurationBinder.Bind(configuration, settings);

            serviceCollection
                .AddSingleton(settings)
                .AddSingleton(new Terms())
                .AddSingleton(serviceProvider => new Services.ServiceResolver(serviceProvider))
                .AddSingleton(serviceProvider => new Tasks.TaskResolver(serviceProvider));

            return serviceCollection;
        }
    }
}