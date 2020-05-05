using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using App.Core;
using App.Core.Models;
using App.Core.Services;

using Microsoft.Web.Administration;

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
        private readonly IKenticoPathService kenticoPath;
        private readonly HttpClient httpClient;

        public bool Source { get; set; }

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
            kenticoPath = services.KenticoPathService();
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            output.Display(terms.InstallTaskStart);

            settings.Name = settings.Name ?? throw new ArgumentException($"'{nameof(settings.Name)}' must be set.");
            settings.Path ??= kenticoPath.GetSolutionPath();
            settings.Path = EnsureValidAppPath(settings.Path);

            settings.DbName ??= settings.Name;
            settings.DbName = await EnsureValidDatabaseName(settings.DbName);

            if (settings.Version == null)
            {
                output.Display(terms.VersionNotSpecified);
                output.Display(terms.UsingLatestVersion);

                settings.Version = new KenticoVersion(12);
            }

            if (settings.Version.Hotfix < 0)
            {
                output.Display(terms.HotfixNotSpecified);
                output.Display(terms.GettingLatestHotfix);

                settings.Version.Hotfix = await GetLatestHotfix(settings.Version.Major);
            }

            var versionUri = kenticoPath.GetInstallerUri();

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

            SetPermissions(settings.Path, WellKnownSidType.NetworkServiceSid, FileSystemRights.Modify);

            if (Source)
            {
                output.Display(terms.BeginSourceInstallOutput);

                var sourceInstallProcess = process()
                    .NewProcess(installDownloadPath)
                    .InDirectory(Path.GetDirectoryName(installDownloadPath))
                    .WithArguments($"-o\"{settings.Path}\" -p\"{settings.SourcePassword}\" -y")
                    .Run();

                if (sourceInstallProcess.ExitCode > 0) throw new Exception("Source install process failed!");

                return;
            }

            var installXmlPath = Path.GetTempFileName();
            var installLogPath = Path.GetTempFileName();
            var setupPath = kenticoPath.GetSetupPath();

            await WriteInstallXml(installXmlPath, installLogPath, setupPath);

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

        private string EnsureValidAppPath(string appPath)
        {
            if (Directory.Exists(appPath) && Directory.EnumerateFileSystemEntries(appPath).Any())
            {
                appPath += $"_{GetRandomString(10)}";
            }

            Directory.CreateDirectory(appPath);
            return appPath;
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

        private async Task<int> GetLatestHotfix(int version)
        {
            XNamespace soap12 = "http://www.w3.org/2003/05/soap-envelope";
            XNamespace service = "http://service.kentico.com/";

            var getHotfixesRequest = new XDocument(
                new XElement(soap12 + "Envelope",
                    new XAttribute(XNamespace.Xmlns + nameof(soap12), soap12),
                    new XElement(soap12 + "Body",
                        new XElement(service + "GetHotfixes",
                            new XElement(service + "fromVersion", version),
                            new XElement(service + "toVersion", version)
                        )
                    )
                )
            );

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(kenticoPath.GetHotfixesUri()),
                Method = HttpMethod.Post,
                Content = new StringContent(getHotfixesRequest.ToString(), Encoding.UTF8, "application/soap+xml")
            };

            var getHotfixesResponse = await httpClient.SendAsync(request);

            var getHotfixesXml = await XDocument.LoadAsync(
                await getHotfixesResponse.Content.ReadAsStreamAsync(),
                LoadOptions.None,
                default
                );

            var latestHotfixVersion = new Version(getHotfixesXml
                .Descendants(service + "HotfixListItem")
                .First()
                .Element(service + "Version")
                .Value);

            return latestHotfixVersion.Build;
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

        private async Task WriteInstallXml(string installXmlPath, string installLogPath, string setupPath)
        {
            var installXml = new XDocument(
                new XElement("SilentInstall",
                    new XAttribute("ShowProgress", "CommandPrompt"),
                    new XAttribute("Log", false),
                    new XAttribute("OnError", "Stop"),
                    new XAttribute("LogFile", installLogPath),
                    new XAttribute("CheckRequirements", true),
                    GetSetupElement(setupPath),
                    new XElement("IIS",
                        new XAttribute("Website", string.Empty),
                        new XAttribute("TargetFolder", settings.Path),
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
                    GetTemplatesElement(),
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

        private XElement GetSetupElement(string setupPath)
        {
            settings.Version = settings.Version ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set.");

            return settings.Version.Major switch
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
                _ => throw new ArgumentOutOfRangeException($"Version '{settings.Version.Major}' has no supported XML configuration."),
            };
        }

        private XElement? GetTemplatesElement()
        {
            settings.Version = settings.Version ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set.");

            if (string.IsNullOrWhiteSpace(settings.AppTemplate))
            {
                return null;
            }

            using var iisManager = new ServerManager();

            settings.AppDomain ??= GetNextUnboundIpAddress(iisManager, settings.Version);

            return new XElement("WebSites",
                new XElement("WebSite",
                    new XAttribute("domain", settings.AppDomain),
                    new XAttribute("displayname", settings.Name),
                    new XAttribute("webtemplatename", settings.AppTemplate),
                    new XAttribute("projectdirectoryname", settings.Name)
                )
            );
        }
    }
}