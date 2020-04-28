using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;

using System.Security.AccessControl;

using App.Core;
using App.Core.Models;
using App.Core.Services;

using static App.Install.Constants;

namespace App.Install
{
    public class InstallTask : IInstallTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly Func<IProcessService> process;
        private readonly IHotfixTask hotfixTask;
        private readonly HttpClient httpClient;

        private IDictionary<Version, string> VersionUris => new Dictionary<Version, string>
        {
            {new Version(12,0,0),  "https://download.kentico.com/Kentico_12_0.exe"}
        };

        public InstallTask(
            Settings settings,
            Terms terms,
            Services services,
            IHotfixTask hotfixTask,
            HttpClient httpClient
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            cache = services.CacheService();
            process = services.ProcessService;
            this.hotfixTask = hotfixTask;
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            output.Display(terms.InstallTaskStart);

            settings.AppName = settings.AppName ?? throw new ArgumentException($"'{nameof(settings.AppName)}' must be set!");

            settings.DbName ??= settings.AppName;

            if (settings.Version == null)
            {
                output.Display(terms.VersionNotSpecified);
                output.Display(terms.UsingLatestVersion);

                settings.Version = new Version(12, 0, UnspecifiedHotfix);
            }

            var versionPair = VersionUris.First(pair => pair.Key.Major == settings.Version.Major);
            var versionUri = versionPair.Value;

            var installDownloadPath = await cache.GetString(versionUri);

            if (installDownloadPath == null)
            {
                installDownloadPath = Path.GetTempFileName();

                await DownloadFile(versionUri, installDownloadPath);
                await cache.SetString(versionUri, installDownloadPath);

                output.Display(terms.DownloadComplete);
            }
            else
            {
                output.Display(terms.SkippingDownload);
            }

            var installXmlPath = Path.GetTempFileName();
            var installLogPath = Path.GetTempFileName();
            var setupPath = @$"{{%programfiles%}}\Kentico\{versionPair.Key.Major}.{versionPair.Key.Minor}";

            settings.AppPath ??= @$"C:\inetpub\wwwroot\{settings.AppName}";

            SetPermissions(settings.AppPath, WellKnownSidType.NetworkServiceSid, FileSystemRights.Modify);

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
            }
            else if (DateTime.Now - installProcess.StartTime > TimeSpan.FromSeconds(10))
            {
                throw new Exception("Install process failed!");
            }

            if (installProcess.ExitCode == 1) throw new Exception("Install after uninstall process failed!");

            await hotfixTask.Run();

            output.Display(terms.InstallComplete);
            output.Display(string.Format(terms.SolutionPath, settings.AppPath));
            output.Display(string.Format(terms.WebPath, settings.AppWebPath));
        }

        private async Task DownloadFile(string requestUri, string downloadPath)
        {
            using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var progressBar = output.ProgressBar((int?)response.Content.Headers.ContentLength ?? 0, terms.Downloading);

            var buffer = new byte[8192];
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true);

            while (true)
            {
                var readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length);

                if (readBytes > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, readBytes);

                    output.UpdateProgress(progressBar, readBytes);
                }
                else
                {
                    break;
                }
            }
        }

        private void SetPermissions(string path, WellKnownSidType sid, FileSystemRights fileSystemRights)
        {
            var directory = Directory.CreateDirectory(path);
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
                    new XElement("Setup",
                        new XAttribute("NET", "4.6.1"),
                        new XAttribute("SetupFolder", setupPath),
                        new XAttribute("DevelopmentModel", "MVC"),
                        new XAttribute("WebProject", "WebApplication"),
                        new XAttribute("OpenAfterInstall", false),
                        new XAttribute("RegisterToIIS", false)
                    ),
                    new XElement("IIS",
                        new XAttribute("Website", string.Empty),
                        new XAttribute("TargetFolder", settings.AppPath),
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
    }
}