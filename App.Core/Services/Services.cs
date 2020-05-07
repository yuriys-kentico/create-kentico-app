using System;

using Microsoft.Extensions.DependencyInjection;

namespace App.Core.Services
{
    public class Services
    {
        private readonly IServiceProvider serviceProvider;

        public Services(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        public IProcessService ProcessService() => serviceProvider.GetRequiredService<IProcessService>();

        public IOutputService OutputService() => serviceProvider.GetRequiredService<IOutputService>();

        public ICacheService CacheService() => serviceProvider.GetRequiredService<ICacheService>();

        public IDatabaseService DatabaseService() => serviceProvider.GetRequiredService<IDatabaseService>();

        public IKenticoPathService KenticoPathService() => serviceProvider.GetRequiredService<IKenticoPathService>();

        public INugetService NugetService() => serviceProvider.GetRequiredService<INugetService>();
    }
}