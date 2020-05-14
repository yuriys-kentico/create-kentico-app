using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;
using App.Core.Tasks;
using App.Infrastructure.Models;

namespace App.Infrastructure.Tasks
{
    public class SetupTask : ISetupTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly IKenticoPathService kenticoPath;
        private readonly TaskResolver tasks;

        public SetupTask(
            Settings settings,
            Terms terms,
            ServiceResolver services,
            TaskResolver tasks
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
                    .Select(property =>
                    {
                        var typeRequiredDescription = (typeof(Terms).GetProperty($"Help{property.Name}")?.GetValue(terms) as string)?.Split('|');

                        var type = string.Empty;
                        var required = string.Empty;
                        var description = string.Empty;

                        switch (typeRequiredDescription?.Length)
                        {
                            case 1:
                                description = typeRequiredDescription[0];
                                break;

                            case 2:
                                type = typeRequiredDescription[0];
                                description = typeRequiredDescription[1];
                                break;

                            case 3:
                                type = typeRequiredDescription[0];
                                required = typeRequiredDescription[1];
                                description = typeRequiredDescription[2];
                                break;
                        }

                        return new HelpRow
                        {
                            Name = $"--{property.Name[0].ToString().ToLower()}{property.Name.Substring(1)}",
                            Aliases = property.GetCustomAttribute<AliasesAttribute>()?.Aliases.Aggregate((a, b) => $"{a}, {b}"),
                            Type = type,
                            Required = required,
                            Description = description
                        };
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