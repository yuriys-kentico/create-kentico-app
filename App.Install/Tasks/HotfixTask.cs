using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using App.Core;
using App.Core.Context;
using App.Core.Services;

using static App.Install.InstallHelper;

namespace App.Install.Tasks
{
    public class HotfixTask : IHotfixTask
    {
        private readonly IAppContext appContext;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly IProcessService process;
        private readonly HttpClient httpClient;

        public HotfixTask(
            IAppContext appContext,
            ServiceResolver services,
            HttpClient httpClient
            )
        {
            this.appContext = appContext;
            output = services.OutputService();
            cache = services.CacheService();
            process = services.ProcessService();
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            var settings = appContext.Settings;
            var terms = appContext.Terms;

            output.Display(terms.HotfixTaskStart);

            settings.Version ??= settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

            var hotfixUri = settings.HotfixUri;

            var hotfixDownloadPath = await cache.GetString(hotfixUri + HotfixDownloadCacheKeySuffix);

            if (hotfixDownloadPath == null || !File.Exists(hotfixDownloadPath))
            {
                hotfixDownloadPath = Path.GetTempFileName();

                await DownloadFile(httpClient, hotfixUri, hotfixDownloadPath, output, string.Format(terms.DownloadingHotfix, settings.Version));
                await cache.SetString(hotfixUri + HotfixDownloadCacheKeySuffix, hotfixDownloadPath);

                output.Display(terms.DownloadComplete);
            }
            else
            {
                output.Display(terms.SkippingDownload);
            }

            var hotfixUnpackPath = await cache.GetString(hotfixUri + HotfixUnpackCacheKeySuffix);

            if (hotfixUnpackPath == null || !File.Exists(hotfixUnpackPath))
            {
                hotfixUnpackPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                Directory.CreateDirectory(hotfixUnpackPath);
                await cache.SetString(hotfixUri + HotfixUnpackCacheKeySuffix, hotfixUnpackPath);
            }

            var hotfixPath = Path.Combine(hotfixUnpackPath, "Hotfix.exe");

            if (!File.Exists(hotfixPath))
            {
                output.Display(terms.UnpackingHotfix);

                var hotfixUnpackProcess = process
                    .FromPath(hotfixDownloadPath)
                    .InDirectory(Path.GetDirectoryName(hotfixUnpackPath))
                    .WithArguments($"/verysilent /dir=\"{hotfixUnpackPath}\"")
                    .Run();

                if (hotfixUnpackProcess.ExitCode > 0) throw new Exception("Hotfix unpack process failed!");
            }
            else
            {
                output.Display(terms.SkippingUnpackingHotfix);
            }

            output.Display(terms.BeginHotfixOutput);

            var hotfixProcess = process
                .FromPath(hotfixPath)
                .InDirectory(Path.GetDirectoryName(hotfixPath))
                .WithArguments($"/silentpath=\"{settings.Path}\"")
                .Run();

            if (hotfixProcess.ExitCode > 0) throw new Exception("Hotfix unpack process failed!");

            output.Display(terms.RebuildingSolution);

            var nugetServicePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    "NuGetCLI",
                    "nuget.exe"
                    );

            process
                .FromPath(nugetServicePath)
                .WithArguments($"restore {settings.BoilerplateSlnPath}")
                .Run();

            var buildServicePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "App.Infrastructure.Services.BuildService",
                "BuildService.exe"
                );

            var buildService = process
                .FromPath(buildServicePath)
                .WithArguments(settings.BoilerplateSlnPath)
                .Run();

            if (buildService.ExitCode != 0) throw new Exception("Build process failed!");

            // Restore CI
            // Run CI
        }
    }
}