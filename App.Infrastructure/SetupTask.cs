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

            await installTask.Run();
            await hotfixTask.Run();
            await iisSiteTask.Run();

            output.Display(terms.InstallComplete);
            output.Display(string.Format(terms.SolutionPath, settings.AppPath));
            output.Display(string.Format(terms.WebPath, settings.AppWebPath));
        }
    }
}