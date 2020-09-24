using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using App.Core;
using App.Core.Context;
using App.Core.Models;
using App.Core.Services;

using Microsoft.Web.Administration;

using static App.Install.InstallHelper;
using static App.Core.CoreHelper;

namespace App.Install.Tasks
{
    public class InstallTask : IInstallTask
    {
        private readonly IAppContext appContext;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly IProcessService process;
        private readonly IDatabaseService database;
        private readonly HttpClient httpClient;

        public InstallTask(
            IAppContext appContext,
            ServiceResolver services,
            HttpClient httpClient
            )
        {
            this.appContext = appContext;
            output = services.OutputService();
            cache = services.CacheService();
            process = services.ProcessService();
            database = services.DatabaseService();
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            var settings = appContext.Settings;
            var terms = appContext.Terms;

            output.Display(terms.InstallTaskStart);

            settings.Name = settings.Name ?? throw new ArgumentNullException(nameof(settings.Name));
            settings.Path = settings.Path ?? throw new ArgumentNullException(nameof(settings.Path));

            settings.DatabaseName ??= settings.Name;
            settings.DatabaseName = await EnsureValidDatabaseName(settings.DatabaseName);

            var boilerplateTempPath = string.Empty;

            if (appContext.Boilerplate)
            {
                settings.BoilerplateUri = settings.BoilerplateUri.Replace("v{}", $"v{appContext.Version}");

                var boilerplateDownloadPath = await cache.GetString(settings.BoilerplateUri);

                if (boilerplateDownloadPath == null || !File.Exists(boilerplateDownloadPath))
                {
                    boilerplateDownloadPath = Path.GetTempFileName();

                    await DownloadFile(httpClient, settings.BoilerplateUri, boilerplateDownloadPath, output, terms.DownloadingBoilerplate);
                    await cache.SetString(settings.BoilerplateUri, boilerplateDownloadPath);

                    output.Display(terms.DownloadComplete);
                }
                else
                {
                    output.Display(terms.SkippingDownload);
                }

                boilerplateTempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                ZipFile.ExtractToDirectory(boilerplateDownloadPath, boilerplateTempPath, true);

                var packagesConfigContent = await File.ReadAllTextAsync(Path.Combine(boilerplateTempPath, "packages.config"));

                var kenticoLibrariesMatch = new Regex("(?<=Kentico\\.Libraries).*?version=\"(.*?)\"").Match(packagesConfigContent);

                if (!kenticoLibrariesMatch.Success)
                {
                    throw new Exception("Boilerplate does not reference Kentico.Libraries!");
                }

                settings.Version = new KenticoVersion(kenticoLibrariesMatch.Groups[1].Value);
            }

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

            if (settings.Version.Major == 12 && settings.Version.Hotfix < 29)
            {
                settings.Version.Hotfix = 29;
            }

            using var iisManager = new ServerManager();

            if (appContext.Mvc)
            {
                settings.AppDomain ??= GetNextUnboundIpAddress(iisManager, settings.Version);
            }

            settings.AdminDomain ??= GetNextUnboundIpAddress(iisManager, settings.Version, settings.AppDomain);

            var versionUri = settings.InstallerUri;

            var installDownloadPath = await cache.GetString(versionUri);

            if (installDownloadPath == null || !File.Exists(installDownloadPath))
            {
                installDownloadPath = Path.GetTempFileName();

                await DownloadFile(httpClient, versionUri, installDownloadPath, output, string.Format(terms.DownloadingInstaller, settings.Version.Major));
                await cache.SetString(versionUri, installDownloadPath);

                output.Display(terms.DownloadComplete);
            }
            else
            {
                output.Display(terms.SkippingDownload);
            }

            SetPermissions(settings.Path, WellKnownSidType.NetworkServiceSid, FileSystemRights.Modify);

            if (appContext.Source)
            {
                output.Display(terms.BeginSourceInstallOutput);

                var sourceInstallProcess = process
                    .FromPath(installDownloadPath)
                    .InDirectory(Path.GetDirectoryName(installDownloadPath))
                    .WithArguments($"-o\"{settings.AppPath}\" -p\"{settings.SourcePassword}\" -y")
                    .Run();

                if (sourceInstallProcess.ExitCode > 0) throw new Exception("Source install process failed!");

                return;
            }

            var installXmlPath = Path.GetTempFileName();
            var installLogPath = Path.GetTempFileName();
            var setupPath = settings.SetupPath;

            await WriteInstallXml(installXmlPath, installLogPath, setupPath);

            output.Display(terms.BeginInstallOutput);

            var installProcess = process
                .FromPath(installDownloadPath)
                .InDirectory(Path.GetDirectoryName(installDownloadPath))
                .WithArguments($"{Path.GetFileName(installXmlPath)}")
                .Run();

            if (installProcess.ExitCode == 1)
            {
                if (DateTime.Now - installProcess.StartTime < TimeSpan.FromSeconds(10))
                {
                    output.Display(terms.BeginUninstallOutput);

                    var uninstallProcess = process
                        .FromPath(installDownloadPath)
                        .WithArguments("-u")
                        .Run();

                    if (uninstallProcess.ExitCode == 1) throw new Exception("Uninstall process failed!");

                    Directory.Delete(setupPath, true);

                    installProcess = process
                        .FromPath(installDownloadPath)
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

            if (appContext.Boilerplate)
            {
                Directory.Move(boilerplateTempPath, settings.AppPath);

                File.Move(Path.Combine(settings.AppPath, settings.BoilerplateSln), settings.BoilerplateSlnPath);

                var boilerplateSlnContent = await File.ReadAllTextAsync(settings.BoilerplateSlnPath);

                if (string.IsNullOrWhiteSpace(boilerplateSlnContent))
                {
                    throw new Exception("Boilerplate SLN is empty!");
                }

                var newBoilerplateSlnContent = new StringBuilder(boilerplateSlnContent);

                newBoilerplateSlnContent.Replace("{{settings.Name}}", settings.Name);

                await File.WriteAllTextAsync(settings.BoilerplateSlnPath, newBoilerplateSlnContent.ToString());
            }
        }

        private async Task<string> EnsureValidDatabaseName(string dbName)
        {
            var databases = await database.Select("name").From("sys.databases").Query();

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
                RequestUri = new Uri(appContext.Settings.HotfixesUri),
                Method = HttpMethod.Post,
                Content = new StringContent(getHotfixesRequest.ToString(), Encoding.UTF8, "application/soap+xml")
            };

            var getHotfixesResponse = await httpClient.SendAsync(request);

            var getHotfixesXml = await XDocument.LoadAsync(
                await getHotfixesResponse.Content.ReadAsStreamAsync(),
                LoadOptions.None,
                default
                );

            var latestHotfixVersion = new KenticoVersion(getHotfixesXml
                .Descendants(service + "HotfixListItem")
                .First()
                .Element(service + "Version")
                .Value);

            return latestHotfixVersion.Hotfix;
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
                        new XAttribute("TargetFolder", appContext.Settings.Path),
                        new XAttribute("KillRunningProcesses", true)
                    ),
                    GetSqlElement(),
                    GetWebSitesElement(),
                    new XElement("Modules",
                        new XAttribute("type", "InstallAll")
                    ),
                    new XElement("UICultures",
                        new XAttribute("type", "InstallAll")
                    ),
                    new XElement("Dictionaries",
                        new XAttribute("type", "InstallAll")
                    ),
                    GetLicensesElement()
                )
            );

            await File.WriteAllTextAsync(installXmlPath, installXml.ToString());
        }

        private XElement GetSetupElement(string setupPath)
        {
            var settings = appContext.Settings;

            settings.Version = settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

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
                _ => throw new ArgumentOutOfRangeException($"Version '{settings.Version.Major}' has no supported XML configuration for the Setup element."),
            };
        }

        private XElement GetSqlElement()
        {
            var settings = appContext.Settings;

            if (!string.IsNullOrWhiteSpace(settings.DatabaseServerUser))
            {
                settings.DatabaseServerPassword = settings.DatabaseServerPassword
                    ?? throw new ArgumentNullException(
                            nameof(settings.DatabaseServerPassword),
                            $"Must be set if '{nameof(settings.DatabaseServerUser)}' is set."
                        );

                return new XElement("Sql",
                    new XAttribute("InstallDatabase", true),
                    new XAttribute("Server", settings.DatabaseServerName),
                    new XAttribute("Database", settings.DatabaseName),
                    new XAttribute("Authentication", "SQL"),
                    new XAttribute("SqlName", settings.DatabaseServerUser),
                    new XAttribute("SqlPswd", settings.DatabaseServerPassword)
                );
            }
            else
            {
                return new XElement("Sql",
                    new XAttribute("InstallDatabase", true),
                    new XAttribute("Server", settings.DatabaseServerName),
                    new XAttribute("Database", settings.DatabaseName),
                    new XAttribute("Authentication", "Windows")
                );
            }
        }

        private XElement? GetWebSitesElement()
        {
            var settings = appContext.Settings;

            if (!appContext.Template)
            {
                return null;
            }

            settings.Version = settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

            return settings.Version.Major switch
            {
                10 => new XElement("WebSites",
                    new XElement("WebSite",
                        new XAttribute("domain", settings.AdminDomain),
                        new XAttribute("displayname", settings.Name),
                        new XAttribute("webtemplatename", settings.AppTemplate)
                    )
                ),
                11 => new XElement("WebSites",
                    new XElement("WebSite",
                        new XAttribute("domain", settings.AdminDomain),
                        new XAttribute("displayname", settings.Name),
                        new XAttribute("webtemplatename", settings.AppTemplate)
                    )
                ),
                12 => new XElement("WebSites",
                    new XElement("WebSite",
                        new XAttribute("domain", settings.AdminDomain),
                        new XAttribute("displayname", settings.Name),
                        new XAttribute("webtemplatename", settings.AppTemplate),
                        new XAttribute("projectdirectoryname", settings.Name)
                    )
                ),
                _ => throw new ArgumentOutOfRangeException($"Version '{settings.Version.Major}' has no supported XML configuration for the WebSites element."),
            };
        }

        private XElement? GetLicensesElement()
        {
            var settings = appContext.Settings;

            if (string.IsNullOrWhiteSpace(settings.License))
            {
                return null;
            }

            return new XElement("Licenses",
                new XElement("License",
                    new XAttribute("domain", settings.AdminDomain),
                    settings.License
                )
            );
        }
    }
}