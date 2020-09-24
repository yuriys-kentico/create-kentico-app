using System;
using System.Threading.Tasks;

using App.Core;
using App.Core.Context;
using App.Core.Services;

namespace App.Install.Tasks
{
    public class DatabaseTask : IDatabaseTask
    {
        private readonly IAppContext appContext;
        private readonly IOutputService output;
        private readonly IDatabaseService database;

        public DatabaseTask(
            IAppContext appContext,
            ServiceResolver services
            )
        {
            this.appContext = appContext;
            output = services.OutputService();
            database = services.DatabaseService();
        }

        public async Task Run()
        {
            var settings = appContext.Settings;
            var terms = appContext.Terms;

            output.Display(terms.DatabaseTaskStart);

            settings.Name = settings.Name ?? throw new ArgumentNullException(nameof(settings.Name));
            settings.DatabaseName = settings.DatabaseName ?? throw new ArgumentNullException(nameof(settings.DatabaseName));

            if (appContext.Mvc)
            {
                await database
                    .From($"{settings.DatabaseName}.dbo.CMS_Site")
                    .Where("SiteName", "=", settings.Name)
                    .Update(new { SitePresentationURL = $"https://{settings.AppDomain}" });
            }
        }
    }
}