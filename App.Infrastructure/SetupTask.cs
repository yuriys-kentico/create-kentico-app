using System;
using System.Reflection;
using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;

namespace App.Infrastructure
{
    public class SetupTask : ISetupTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly IKenticoPathService kenticoPath;
        private readonly IInstallTask installTask;
        private readonly IHotfixTask hotfixTask;
        private readonly IIisTask iisSiteTask;
        private readonly IDatabaseTask databaseTask;

        public SetupTask(
            Settings settings,
            Terms terms,
            Services services,
            IInstallTask installTask,
            IHotfixTask hotfixTask,
            IIisTask iisSiteTask,
            IDatabaseTask databaseTask
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            kenticoPath = services.KenticoPathService();
            this.installTask = installTask;
            this.hotfixTask = hotfixTask;
            this.iisSiteTask = iisSiteTask;
            this.databaseTask = databaseTask;
        }

        public async Task Run()
        {
            output.Display(terms.CreateKenticoAppVersion + Assembly.GetEntryAssembly()?
                               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                               .InformationalVersion);

            output.Display(terms.Setup);

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
                await hotfixTask.Run();
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