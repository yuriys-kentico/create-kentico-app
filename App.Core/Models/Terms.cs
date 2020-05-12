namespace App.Core.Models
{
    public class Terms
    {
        public string CreateKenticoAppVersion => "create-kentico-app v";

        public string Setup => "Setting up...";

        public string InstallTaskStart => "Starting installation...";

        public string VersionNotSpecified => "Version not specified.";

        public string UsingLatestVersion => "Using latest version.";

        public string DownloadingInstaller => "Downloading installer for {0}...";

        public string DownloadComplete => "Download complete!";

        public string SkippingDownload => "Found existing file, skipping download.";

        public string BeginInstallOutput => "Showing output from installer...";

        public string BeginSourceInstallOutput => "Showing output from source code unpacker...";

        public string BeginUninstallOutput => "Showing output from uninstaller...";

        public string HotfixTaskStart => "Starting hotfixing...";

        public string HotfixNotSpecified => "Hotfix not specified.";

        public string GettingLatestHotfix => "Getting latest hotfix...";

        public string DownloadingHotfix => "Downloading hotfix for {0}...";

        public string UnpackingHotfix => "Unpacking hotfix...";

        public string SkippingUnpackingHotfix => "Found existing files, skipping unpacking hotfix.";

        public string BeginHotfixOutput => "Showing output from hotfixer...";

        public string UpdatingKenticoLibraries => "Updating Kentico.Libraries NuGet package...";

        public string RebuildingSolution => "Rebuilding solution...";

        public string IisTaskStart => "Starting IIS registration...";

        public string CreatedNewCertificate => "Created new certificate.";

        public string CertificateName => "create-kentico-app";

        public string SkippingCreatingCertificate => "Found existing certificate, skipping creating a new one...";

        public string DatabaseTaskStart => "Starting database changes...";

        public string InstallComplete => "Installation completed.";

        public string SolutionPath => "Solution installed at: {0}";

        public string AdminPath => "Admin available at: https://{0}:443";

        public string AppPath => "App available at: https://{0}:443";

        public string SourceHotfixStep => "Source hotfix available at: {0}";

        public string AdditionalSourceSteps => "The source code installation requires the following steps: a manual build, install of the database, and install of the hotfix.";
    }
}