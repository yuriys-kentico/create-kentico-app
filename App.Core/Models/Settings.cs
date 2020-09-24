using System;

namespace App.Core.Models
{
    public class Settings : BaseSettings
    {
        public Settings(BaseSettings settings)
        {
            Help = settings.Help;
            Name = settings.Name;
            DatabaseServerName = settings.DatabaseServerName;
            DatabaseName = settings.DatabaseName;
            DatabaseServerUser = settings.DatabaseServerUser;
            DatabaseServerPassword = settings.DatabaseServerPassword;
            Version = settings.Version;
            License = settings.License;
            Path = settings.Path;
            AppTemplate = settings.AppTemplate;
            AppDomain = settings.AppDomain;
            AdminDomain = settings.AdminDomain;
            Source = settings.Source;
            SourcePassword = settings.SourcePassword;
        }

        public string InstallerUri
        {
            get
            {
                var version = Version ?? throw new ArgumentNullException(nameof(Version));

                var baseUri = $"https://download.kentico.com/Kentico_{version.Major}_{version.Minor}";

                if (Source ?? false)
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
        }

        public string HotfixesUri { get; set; } = "https://service.kentico.com/CMSUpgradeService.asmx";

        public string HotfixUri
        {
            get
            {
                var version = Version ?? throw new ArgumentNullException(nameof(Version));

                var baseUri = $"https://download.kentico.com/CMSUpgrades/Hotfix/{version.Major}_{version.Minor}/HotFix_{version.Major}_{version.Minor}_{version.Hotfix}";

                if (Source ?? false)
                {
                    return baseUri + "_src.exe";
                }

                return baseUri + ".exe";
            }
        }

        public string BoilerplateUri { get; set; } = "https://github.com/yuriys-kentico/create-kentico-app/releases/download/v{}/App.Boilerplate.zip";

        public string SetupPath
        {
            get
            {
                var version = Version ?? throw new ArgumentNullException(nameof(Version));

                return @$"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Kentico\{version.Major}.{version.Minor}";
            }
        }

        public string AdminPath => System.IO.Path.Combine(Path ?? throw new ArgumentNullException(nameof(Path)), "CMS");

        public string AppPath => System.IO.Path.Combine(Path ?? throw new ArgumentNullException(nameof(Path)), Name ?? throw new ArgumentNullException(nameof(Name)));

        public string BoilerplateSln => "App.Boilerplate.sln";

        public string BoilerplateSlnPath => System.IO.Path.Combine(Path ?? throw new ArgumentNullException(nameof(Path)), BoilerplateSln);
    }
}