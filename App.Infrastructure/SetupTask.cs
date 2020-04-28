using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;

namespace App.Infrastructure
{
    public class SetupTask : ISetupTask
    {
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly IInstallTask installTask;

        public SetupTask(
            Terms terms,
            Services services,
            IInstallTask installTask
            )
        {
            this.terms = terms;
            output = services.OutputService();
            this.installTask = installTask;
        }

        public async Task Run()
        {
            output.Display(terms.Setup);

            await installTask.Run();
        }
    }
}