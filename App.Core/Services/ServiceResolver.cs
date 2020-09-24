using System;

using Microsoft.Extensions.DependencyInjection;

namespace App.Core.Services
{
    public class ServiceResolver
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceResolver(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        public IProcessService ProcessService() => serviceProvider.GetRequiredService<IProcessService>();

        public IOutputService OutputService() => serviceProvider.GetRequiredService<IOutputService>();

        public ICacheService CacheService() => serviceProvider.GetRequiredService<ICacheService>();

        public IDatabaseService DatabaseService() => serviceProvider.GetRequiredService<IDatabaseService>();
    }
}