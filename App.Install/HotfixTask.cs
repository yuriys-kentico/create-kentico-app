using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;

using static App.Install.InstallHelper;

namespace App.Install
{
    public class HotfixTask : IHotfixTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly IProcessService process;
        private readonly IKenticoPathService kenticoPath;
        private readonly Func<INugetService> nuget;
        private readonly HttpClient httpClient;

        public HotfixTask(
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
            process = services.ProcessService();
            kenticoPath = services.KenticoPathService();
            nuget = services.NugetService;
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            output.Display(terms.HotfixTaskStart);

            var hotfixUri = kenticoPath.GetHotfixUri();

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

            settings.Version ??= settings.Version ?? throw new ArgumentNullException(nameof(settings.Version));

            if (settings.Version.Major == 12 && settings.Version.Hotfix > 29)
            {
                await nuget().InstallPackage("Kentico.Libraries", settings.Version.ToString());
            }
        }
    }
}