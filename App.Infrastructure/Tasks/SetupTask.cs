using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using App.Core;
using App.Core.Context;
using App.Core.Models;
using App.Core.Services;
using App.Core.Tasks;
using App.Infrastructure.Models;

using static App.Core.CoreHelper;

namespace App.Infrastructure.Tasks
{
    public class SetupTask : ISetupTask
    {
        private readonly IAppContext appContext;
        private readonly IOutputService output;
        private readonly TaskResolver tasks;

        public SetupTask(
            IAppContext appContext,
            ServiceResolver services,
            TaskResolver tasks
            )
        {
            this.appContext = appContext;
            output = services.OutputService();
            this.tasks = tasks;
        }

        public async Task Run()
        {
            var terms = appContext.Terms;
            var settings = appContext.Settings;

            output.Display(terms.CreateKenticoAppVersion + appContext.Version);

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

            settings.Name = settings.Name ?? throw new ArgumentNullException(nameof(settings.Name));
            settings.Path ??= Path.Combine(@"C:\inetpub\wwwroot\", settings.Name ?? throw new ArgumentNullException(nameof(settings.Name)));
            settings.Path = EnsureValidAppPath(settings.Path);

            var installTask = tasks.InstallTask();
            var iisSiteTask = tasks.IisTask();
            var databaseTask = tasks.DatabaseTask();
            var hotfixTask = tasks.HotfixTask();

            if (settings.AppTemplate == "Boilerplate")
            {
                appContext.Boilerplate = true;
                appContext.Mvc = true;
            }
            else if (!string.IsNullOrWhiteSpace(settings.AppTemplate))
            {
                appContext.Template = true;

                if (settings.AppTemplate.EndsWith("Mvc"))
                {
                    appContext.Mvc = true;
                }
            }

            if (settings.Source ?? false)
            {
                settings.SourcePassword = settings.SourcePassword
                    ?? throw new ArgumentNullException(
                            nameof(settings.SourcePassword),
                            $"Must be set if '{nameof(settings.Source)}' is 'true'."
                        );

                appContext.Source = true;

                await installTask.Run();
                await iisSiteTask.Run();

                output.Display(terms.InstallComplete);
                output.Display(string.Format(terms.SolutionPath, settings.Path));
                output.Display(string.Format(terms.AdminPath, settings.AdminDomain));
                output.Display(string.Format(terms.SourceHotfixStep, settings.HotfixUri));
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

            if (appContext.Mvc)
            {
                output.Display(string.Format(terms.AppPath, settings.AppDomain));
            }
        }

        private string EnsureValidAppPath(string appPath)
        {
            if (Directory.Exists(appPath) && Directory.EnumerateFileSystemEntries(appPath).Any())
            {
                appPath += $"_{GetRandomString(10)}";
            }

            Directory.CreateDirectory(appPath);
            return appPath;
        }
    }
}