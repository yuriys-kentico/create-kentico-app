using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using App.Core;
using App.Core.Models;
using App.Core.Services;

using static App.Install.Constants;

namespace App.Install
{
    public class HotfixTask : IHotfixTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly IIisSiteTask iisSiteTask;
        private readonly Func<IProcessService> process;
        private readonly HttpClient httpClient;

        private string GetHotfixesUri => "https://service.kentico.com/CMSUpgradeService.asmx";

        public HotfixTask(
            Settings settings,
            Terms terms,
            Services services,
            IIisSiteTask iisSiteTask,
            HttpClient httpClient
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            cache = services.CacheService();
            this.iisSiteTask = iisSiteTask;
            process = services.ProcessService;
            this.httpClient = httpClient;
        }

        public async Task Run()
        {
            output.Display(terms.HotfixTaskStart);

            var hotfix = settings.Version?.Build ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set!");

            if (hotfix == UnspecifiedHotfix)
            {
                output.Display(terms.HotfixNotSpecified);
                output.Display(terms.GettingLatestHotfix);

                settings.Version = new Version(settings.Version.Major, settings.Version.Minor, await GetLatestHotfix(settings.Version.Major));
            }

            var hotfixUriFragment = $"{settings.Version.Major}_0/HotFix_{settings.Version.Major}_0_{settings.Version.Build}.exe";
            var hotfixUri = "https://www.kentico.com/Downloads/HotFix/" + hotfixUriFragment;

            var hotfixDownloadPath = await cache.GetString(hotfixUri);

            if (hotfixDownloadPath == null)
            {
                hotfixDownloadPath = Path.GetTempFileName();

                await DownloadFile(hotfixUri, hotfixDownloadPath);
                await cache.SetString(hotfixUri, hotfixDownloadPath);

                output.Display(terms.DownloadComplete);
            }
            else
            {
                output.Display(terms.SkippingDownload);
            }

            var hotfixUnpackPath = await cache.GetString(hotfixUriFragment);

            if (hotfixUnpackPath == null)
            {
                hotfixUnpackPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                Directory.CreateDirectory(hotfixUnpackPath);
                await cache.SetString(hotfixUriFragment, hotfixUnpackPath);
            }

            var hotfixPath = Path.Combine(hotfixUnpackPath, "Hotfix.exe");

            if (!File.Exists(hotfixPath))
            {
                output.Display(terms.UnpackingHotfix);

                var hotfixUnpackProcess = process()
                    .NewProcess(hotfixDownloadPath)
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

            var hotfixProcess = process()
                .NewProcess(hotfixPath)
                .InDirectory(Path.GetDirectoryName(hotfixPath))
                .WithArguments($"/silentpath=\"{settings.AppPath}\"")
                .Run();

            if (hotfixProcess.ExitCode > 0) throw new Exception("Hotfix unpack process failed!");

            await iisSiteTask.Run();
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
                RequestUri = new Uri(GetHotfixesUri),
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
    }
}