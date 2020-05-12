using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;
using App.Core.Tasks;
using App.Infrastructure.Models;

namespace App.Infrastructure
{
    public class SetupTask : ISetupTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly IKenticoPathService kenticoPath;
        private readonly Tasks tasks;

        public SetupTask(
            Settings settings,
            Terms terms,
            Services services,
            Tasks tasks
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            kenticoPath = services.KenticoPathService();
            this.tasks = tasks;
        }

        public async Task Run()
        {
            output.Display(terms.CreateKenticoAppVersion + Assembly.GetEntryAssembly()?
                               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                               .InformationalVersion);

            if (settings.Help ?? false)
            {
                output.Display(terms.Help);

                var rows = typeof(Settings)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => new HelpRow
                    {
                        Name = $"--{property.Name}",
                        Aliases = string.Join(',', property.GetCustomAttribute<AliasesAttribute>()?.Aliases.Select(alias => $"-{alias}") ?? new[] { "" }),
                        Type = property.PropertyType.UnderlyingSystemType.Name,
                        Description = typeof(Terms).GetProperty($"Help{property.Name}")?.GetValue(terms) as string
                    });

                output.DisplayTable(rows);

                return;
            }

            output.Display(terms.Setup);

            var installTask = tasks.InstallTask();
            var iisSiteTask = tasks.IisTask();
            var databaseTask = tasks.DatabaseTask();

            if (!string.IsNullOrWhiteSpace(settings.AppTemplate))
            {
                installTask.Template = true;

                if (settings.AppTemplate.EndsWith("Mvc"))
                {
                    installTask.Mvc = true;
                    iisSiteTask.Mvc = true;
                    databaseTask.Mvc = true;
                }
            }

            if (settings.Source ?? false)
            {
                settings.SourcePassword = settings.SourcePassword ?? throw new ArgumentNullException(nameof(settings.SourcePassword), $"Must be set if '{nameof(settings.Source)}' is 'true'.");

                installTask.Source = true;

                await installTask.Run();
                await iisSiteTask.Run();

                output.Display(terms.InstallComplete);
                output.Display(string.Format(terms.SolutionPath, settings.Path));
                output.Display(string.Format(terms.AdminPath, settings.AdminDomain));
                output.Display(string.Format(terms.SourceHotfixStep, kenticoPath.GetHotfixUri()));
                output.Display(terms.AdditionalSourceSteps);

                return;
            }

            await installTask.Run();

            if (settings.Version?.Hotfix > 0)
            {
                await tasks.HotfixTask().Run();
            }

            await iisSiteTask.Run();
            await databaseTask.Run();

            output.Display(terms.InstallComplete);
            output.Display(string.Format(terms.SolutionPath, settings.Path));
            output.Display(string.Format(terms.AdminPath, settings.AdminDomain));

            if (installTask.Mvc)
            {
                output.Display(string.Format(terms.AppPath, settings.AppDomain));
            }
        }
    }
}