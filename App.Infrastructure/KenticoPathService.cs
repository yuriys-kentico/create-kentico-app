using System;

using App.Core.Models;
using App.Core.Services;

namespace App.Infrastructure
{
    public class KenticoPathService : IKenticoPathService
    {
        private readonly Settings settings;

        public KenticoPathService(Settings settings)
        {
            this.settings = settings;
        }

        public string GetInstallerUri(KenticoVersion? version = default)
        {
            version ??= settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

            var baseUri = $"https://download.kentico.com/Kentico_{version.Major}_{version.Minor}";

            if (settings.Source ?? false)
            {
                return version.Major switch
                {
                    12 => baseUri + "_SourceCode461.exe",
                    11 => baseUri + "_SourceCode46.exe",
                    10 => baseUri + "_SourceCode45.exe",
                    _ => throw new ArgumentOutOfRangeException($"Version '{version.Major}' has no supported source URI."),
                };
            }

            return baseUri + ".exe";
        }

        public string GetSetupPath(KenticoVersion? version = default)
        {
            version ??= settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

            return @$"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Kentico\{version.Major}.{version.Minor}";
        }

        public string GetSolutionPath(string? appName = default)
        {
            appName ??= settings.Name ?? throw new ArgumentNullException(nameof(settings.Name));

            return @$"C:\inetpub\wwwroot\{appName}";
        }

        public string GetHotfixesUri()
        {
            return "https://service.kentico.com/CMSUpgradeService.asmx";
        }

        public string GetHotfixUri(KenticoVersion? version = default)
        {
            version ??= settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

            var baseUri = $"https://www.kentico.com/Downloads/HotFix/{version.Major}_{version.Minor}/HotFix_{version.Major}_{version.Minor}_{version.Hotfix}";

            if (settings.Source ?? false)
            {
                return baseUri + "_src.exe";
            }

            return baseUri + ".exe";
        }
    }
}