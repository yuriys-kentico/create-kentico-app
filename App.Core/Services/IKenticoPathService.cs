using App.Core.Models;

namespace App.Core.Services
{
    public interface IKenticoPathService
    {
        string GetInstallerUri(KenticoVersion? version = default);

        string GetSetupPath(KenticoVersion? version = default);

        string GetSolutionPath(string? appName = default);

        string GetHotfixesUri();

        string GetHotfixUri(KenticoVersion? version = default);
    }
}