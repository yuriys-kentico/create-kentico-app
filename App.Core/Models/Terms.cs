namespace App.Core.Models
{
    public class Terms
    {
        public string CreateKenticoAppVersion => "create-kentico-app v";

        public string Help => "Welcome to create-kentico-app! A way to easily start a Kentico project. This is a CLI tool. The available parameters:";

        public string HelpHelp => "Display this help text and ignore any other parameters.";

        public string HelpName => "App name.";

        public string HelpPath => "Solution path. Must be non-existent or empty.";

        public string HelpAdminDomain => "Admin domain. Must be unique in local IIS and different from appDomain.";

        public string HelpAppDomain => "App domain. Must be unique in local IIS and different from adminDomain.";

        public string HelpAppTemplate => "App template. Must be a template that comes with the Kentico installer.";

        public string HelpVersion => "Version. Must exist as a version of Kentico. Can be a partial version like 12. If 12.0.X, must be greater than 12.0.29.";

        public string HelpSource => "Install source code. Must also set sourcePassword.";

        public string HelpSourcePassword => "Source code password.";

        public string HelpDatabaseName => "Database name. Must be unique in database server.";

        public string HelpDatabaseServerName => "Database server name.";

        public string HelpDatabaseServerUser => "Database server user. Must also set databaseServerPassword. If not set, Windows Authentication will be used.";

        public string HelpDatabaseServerPassword => "Database server password.";

        public string HelpLicense => "License key. Must be valid for the version if version is set. If version is not set, must be valid for the latest version.";

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