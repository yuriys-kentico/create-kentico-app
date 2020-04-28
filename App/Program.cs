using System;
using System.Threading.Tasks;

using App.Core;
using App.Core.Services;
using App.Infrastructure;
using App.Install;

using Microsoft.Extensions.DependencyInjection;

namespace App
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var serviceProvider = ConfigureServices(args);

            try
            {
                await serviceProvider.GetRequiredService<ISetupTask>().Run();
            }
            catch (Exception ex)
            {
                serviceProvider.GetRequiredService<IOutputService>().Display(ex.ToString());
            }

            Console.ReadKey();
        }

        private static ServiceProvider ConfigureServices(string[] args)
        {
            var services = new ServiceCollection()
                .AddCore(args)
                .AddInfrastucture()
                .AddInstall();

            return services.BuildServiceProvider();
        }
    }
}