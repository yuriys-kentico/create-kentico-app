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

        public bool Mvc { get; set; }

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

            settings.Name = settings.Name ?? throw new ArgumentNullException(nameof(settings.Name));
            settings.DatabaseName = settings.DatabaseName ?? throw new ArgumentNullException(nameof(settings.DatabaseName));

            if (Mvc)
            {
                await database
                    .From($"{settings.DatabaseName}.dbo.CMS_Site")
                    .Where("SiteName", "=", settings.Name)
                    .Update(new { SitePresentationURL = $"https://{settings.AppDomain}" });
            }
        }
    }
}