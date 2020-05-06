using System;
using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;

namespace App.Install
{
    public class DatabaseTask : IDatabaseTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly IDatabaseService database;

        public DatabaseTask(
            Settings settings,
            Terms terms,
            Services services
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            database = services.DatabaseService();
        }

        public async Task Run()
        {
            output.Display(terms.DatabaseTaskStart);

            settings.Name = settings.Name ?? throw new ArgumentException($"'{nameof(settings.Name)}' must be set.");
            settings.DatabaseName = settings.DatabaseName ?? throw new ArgumentException($"'{nameof(settings.DatabaseName)}' must be set.");

            await database
                .From($"{settings.DatabaseName}.dbo.CMS_Site")
                .Where("SiteName", "=", settings.Name)
                .Update(new { SitePresentationURL = $"https://{settings.AppDomain}" });
        }
    }
}