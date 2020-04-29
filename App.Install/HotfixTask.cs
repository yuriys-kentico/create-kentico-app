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
using static App.Install.InstallHelper;

namespace App.Install
{
    public class HotfixTask : IHotfixTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly ICacheService cache;
        private readonly Func<IProcessService> process;
        private readonly HttpClient httpClient;

        private string GetHotfixesUri => "https://service.kentico.com/CMSUpgradeService.asmx";

        private Func<Version, string> HotfixUri => version => $"https://www.kentico.com/Downloads/HotFix/{version.Major}_{version.Minor}/HotFix_{version.Major}_{version.Minor}_{version.Build}.exe";

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

            var hotfixUri = HotfixUri(settings.Version);

            var hotfixDownloadPath = await cache.GetString(hotfixUri + HotfixDownloadCacheKeySuffix);

            if (hotfixDownloadPath == null)
            {
                hotfixDownloadPath = Path.GetTempFileName();

                await DownloadFile(httpClient, hotfixUri, hotfixDownloadPath, output, terms.Downloading);
                await cache.SetString(hotfixUri + HotfixDownloadCacheKeySuffix, hotfixDownloadPath);

                output.Display(terms.DownloadComplete);
            }
            else
            {
                output.Display(terms.SkippingDownload);
            }

            var hotfixUnpackPath = await cache.GetString(hotfixUri + HotfixUnpackCacheKeySuffix);

            if (hotfixUnpackPath == null)
            {
                hotfixUnpackPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                Directory.CreateDirectory(hotfixUnpackPath);
                await cache.SetString(hotfixUri + HotfixUnpackCacheKeySuffix, hotfixUnpackPath);
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