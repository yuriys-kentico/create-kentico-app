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
            version ??= settings.Version ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set.");

            var baseUri = $"https://download.kentico.com/Kentico_{version.Major}_{version.Minor}";

            if (settings.Source.HasValue && settings.Source.Value)
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
            version ??= settings.Version ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set.");

            return @$"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Kentico\{version.Major}.{version.Minor}";
        }

        public string GetSolutionPath(string? appName = default)
        {
            appName ??= settings.Name ?? throw new ArgumentException($"'{nameof(settings.Name)}' must be set.");

            return @$"C:\inetpub\wwwroot\{appName}";
        }

        public string GetHotfixesUri()
        {
            return "https://service.kentico.com/CMSUpgradeService.asmx";
        }

        public string GetHotfixUri(KenticoVersion? version = default)
        {
            version ??= settings.Version ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set.");

            var baseUri = $"https://www.kentico.com/Downloads/HotFix/{version.Major}_{version.Minor}/HotFix_{version.Major}_{version.Minor}_{version.Hotfix}";

            if (settings.Source.HasValue && settings.Source.Value)
            {
                return baseUri + "_src.exe";
            }

            return baseUri + ".exe";
        }
    }
}