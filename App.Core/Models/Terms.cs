namespace App.Core.Models
{
    public class Terms
    {
        public string Setup => "Setting up...";

        public string InstallTaskStart => "Starting installation...";

        public string VersionNotSpecified => "Version not specified.";

        public string UsingLatestVersion => "Using latest version.";

        public string Downloading => "Downloading...";

        public string DownloadComplete => "Download complete!";

        public string SkippingDownload => "Found existing file, skipping download.";

        public string BeginInstallOutput => "Showing output from installer...";

        public string BeginUninstallOutput => "Showing output from uninstaller...";

        public string IisTaskStart => "Starting IIS registration...";

        public string CreatedNewCertificate => "Created new certificate.";

        public string SkippingCreatingCertificate => "Found existing certificate, skipping creating a new one...";

        public string HotfixTaskStart => "Starting hotfixing...";

        public string HotfixNotSpecified => "Hotfix not specified.";

        public string GettingLatestHotfix => "Getting latest hotfix...";

        public string UnpackingHotfix => "Unpacking hotfix...";

        public string SkippingUnpackingHotfix => "Found existing files, skipping unpacking hotfix.";

        public string BeginHotfixOutput => "Showing output from hotfixer...";

        public string InstallComplete => "Installation completed.";

        public string SolutionPath => "Solution installed at: {0}";

        public string WebPath => "Admin available at: https://{0}";
    }
}