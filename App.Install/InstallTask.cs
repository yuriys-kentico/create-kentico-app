using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;

using App.Core;
using App.Core.Models;
using App.Core.Services;

using static App.Install.Constants;
using static App.Install.InstallHelper;

namespace App.Install
{
    public class InstallTask : IInstallTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly Func<IProcessService> process;
        private readonly IDatabaseService database;
        private readonly HttpClient httpClient;

        private Func<Version, string> VersionUri => version => $"https://download.kentico.com/Kentico_{version.Major}_{version.Minor}.exe";

        private Func<Version, string> SetupPath => version => @$"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Kentico\{version.Major}.{version.Minor}";

        private Func<string, string> AppPath => appFolderName => @$"C:\inetpub\wwwroot\{appFolderName}";

        public InstallTask(
            Settings settings,
            Terms terms,
            Services services,
            HttpClient httpClient
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            cache = services.CacheService();
            process = services.ProcessService;
            database = services.DatabaseService();
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            output.Display(terms.InstallTaskStart);

            settings.AppName = settings.AppName ?? throw new ArgumentException($"'{nameof(settings.AppName)}' must be set!");
            settings.AppPath ??= AppPath(settings.AppName);
            settings.AppPath = EnsureValidAppPath(settings.AppPath);

            settings.DbName ??= settings.AppName;
            settings.DbName = await EnsureValidDatabaseName(settings.DbName);

            if (settings.ParsedVersion == null)
            {
                output.Display(terms.VersionNotSpecified);
                output.Display(terms.UsingLatestVersion);

                settings.ParsedVersion = new KenticoVersion(12);
            }

            var versionUri = VersionUri(settings.ParsedVersion);

            var installDownloadPath = await cache.GetString(versionUri);

            if (installDownloadPath == null || !File.Exists(installDownloadPath))
            {
                installDownloadPath = Path.GetTempFileName();

                await DownloadFile(httpClient, versionUri, installDownloadPath, output, terms.Downloading);
                await cache.SetString(versionUri, installDownloadPath);

                output.Display(terms.DownloadComplete);
            }
            else
            {
                output.Display(terms.SkippingDownload);
            }

            var installXmlPath = Path.GetTempFileName();
            var installLogPath = Path.GetTempFileName();
            var setupPath = SetupPath(settings.ParsedVersion);

            SetPermissions(settings.AppPath, WellKnownSidType.NetworkServiceSid, FileSystemRights.Modify);

            await WriteInstallXml(installXmlPath, installLogPath, setupPath, settings.AppPath, settings.ParsedVersion);

            output.Display(terms.BeginInstallOutput);

            var installProcess = process()
                .NewProcess(installDownloadPath)
                .InDirectory(Path.GetDirectoryName(installDownloadPath))
                .WithArguments($"{Path.GetFileName(installXmlPath)}")
                .Run();

            if (installProcess.ExitCode == 1)
            {
                if (DateTime.Now - installProcess.StartTime < TimeSpan.FromSeconds(10))
                {
                    output.Display(terms.BeginUninstallOutput);

                    var uninstallProcess = process()
                        .NewProcess(installDownloadPath)
                        .WithArguments("-u")
                        .Run();

                    if (uninstallProcess.ExitCode == 1) throw new Exception("Uninstall process failed!");

                    Directory.Delete(setupPath, true);

                    installProcess = process()
                        .NewProcess(installDownloadPath)
                        .InDirectory(Path.GetDirectoryName(installDownloadPath))
                        .WithArguments($"{Path.GetFileName(installXmlPath)}")
                        .Run();
                }
                else
                {
                    throw new Exception("Install process failed!");
                }
            }

            if (installProcess.ExitCode == 1) throw new Exception("Install after uninstall process failed!");
        }

        private async Task<string> EnsureValidDatabaseName(string dbName)
        {
            var databases = await database.Query("select name from sys.databases");

            if (databases.Any(database => dbName.Equals(database.name)))
            {
                dbName += $"_{GetRandomString(10)}";
            }

            return dbName;
        }

        private string EnsureValidAppPath(string appPath)
        {
            if (Directory.Exists(appPath) && Directory.EnumerateFileSystemEntries(appPath).Any())
            {
                appPath += $"_{GetRandomString(10)}";
            }

            Directory.CreateDirectory(appPath);
            return appPath;
        }

        private void SetPermissions(string path, WellKnownSidType sid, FileSystemRights fileSystemRights)
        {
            var directory = new DirectoryInfo(path);
            var security = directory.GetAccessControl(AccessControlSections.Access);

            security.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(sid, null),
                fileSystemRights,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

            directory.SetAccessControl(security);
        }

        private async Task WriteInstallXml(string installXmlPath, string installLogPath, string setupPath, string appPath, Version version)
        {
            var installXml = new XDocument(
                new XElement("SilentInstall",
                    new XAttribute("ShowProgress", "CommandPrompt"),
                    new XAttribute("Log", false),
                    new XAttribute("OnError", "Stop"),
                    new XAttribute("LogFile", installLogPath),
                    new XAttribute("CheckRequirements", true),
                    GetSetupElement(version, setupPath),
                    new XElement("IIS",
                        new XAttribute("Website", string.Empty),
                        new XAttribute("TargetFolder", appPath),
                        new XAttribute("KillRunningProcesses", true)
                    ),
                    new XElement("Sql",
                        new XAttribute("InstallDatabase", true),
                        new XAttribute("Server", settings.DbServerName),
                        new XAttribute("Database", settings.DbName),
                        new XAttribute("Authentication", "SQL"),
                        new XAttribute("SqlName", settings.DbServerUser),
                        new XAttribute("SqlPswd", settings.DbServerPassword)
                    ),
                    new XElement("Modules",
                        new XAttribute("type", "InstallAll")
                    ),
                    new XElement("UICultures",
                        new XAttribute("type", "InstallAll")
                    ),
                    new XElement("Dictionaries",
                        new XAttribute("type", "InstallAll")
                    )
                )
            );

            await File.WriteAllTextAsync(installXmlPath, installXml.ToString());
        }

        private XElement GetSetupElement(Version version, string setupPath)
        {
            return version.Major switch
            {
                10 => new XElement("Setup",
                        new XAttribute("NET", "4.6"),
                        new XAttribute("SetupFolder", setupPath),
                        new XAttribute("Location", "VisualStudio"),
                        new XAttribute("WebProject", "WebApplication"),
                        new XAttribute("OpenAfterInstall", false)
                    ),
                11 => new XElement("Setup",
                        new XAttribute("NET", "4.7"),
                        new XAttribute("SetupFolder", setupPath),
                        new XAttribute("Location", "VisualStudio"),
                        new XAttribute("WebProject", "WebApplication"),
                        new XAttribute("OpenAfterInstall", false)
                    ),
                12 => new XElement("Setup",
                        new XAttribute("NET", "4.6.1"),
                        new XAttribute("SetupFolder", setupPath),
                        new XAttribute("DevelopmentModel", "MVC"),
                        new XAttribute("WebProject", "WebApplication"),
                        new XAttribute("OpenAfterInstall", false),
                        new XAttribute("RegisterToIIS", false)
                    ),
                _ => throw new ArgumentOutOfRangeException($"Version '{version.Major}' has no supported XML configuration."),
            };
        }
    }
}