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
        private readonly IIisSiteTask iisSiteTask;

        public SetupTask(
            Settings settings,
            Terms terms,
            Services services,
            IInstallTask installTask,
            IHotfixTask hotfixTask,
            IIisSiteTask iisSiteTask
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            kenticoPath = services.KenticoPathService();
            this.installTask = installTask;
            this.hotfixTask = hotfixTask;
            this.iisSiteTask = iisSiteTask;
        }

        public async Task Run()
        {
            output.Display(terms.CreateKenticoAppVersion + Assembly.GetEntryAssembly()?
                               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                               .InformationalVersion);

            output.Display(terms.Setup);

            if (settings.Source.HasValue && settings.Source.Value)
            {
                settings.SourcePassword = settings.SourcePassword ?? throw new InvalidOperationException($"'{nameof(settings.SourcePassword)}' must be set if '{nameof(settings.Source)}' is 'true'.");

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

            output.Display(terms.InstallComplete);
            output.Display(string.Format(terms.SolutionPath, settings.Path));
            output.Display(string.Format(terms.AdminPath, settings.AdminDomain));
            output.Display(string.Format(terms.AppPath, settings.AppDomain));
        }
    }
}