using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using App.Core.Models;
using App.Core.Services;

using Microsoft.Web.Administration;

namespace App.Install
{
    public static class InstallHelper
    {
        public static string HotfixDownloadCacheKeySuffix => "_download";

        public static string HotfixUnpackCacheKeySuffix => "_unpack";

        public static string CertificateCacheKey => "certificateThumbprint";

        internal static async Task DownloadFile(HttpClient httpClient, string requestUri, string downloadPath, IOutputService output, string message)
        {
            using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var progressBar = output.ProgressBar((int?)response.Content.Headers.ContentLength ?? 0, message);

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

        internal static string GetNextUnboundIpAddress(ServerManager iisManager, KenticoVersion version, params string?[] blacklist)
        {
            var ipAddressFragment = $"127.{version.Major}.{version.Hotfix}.";

            var allBoundIpAddresses = iisManager.Sites
                .SelectMany(site => site.Bindings)
                .Select(binding => binding.BindingInformation.Split(':').First())
                .Concat(blacklist);

            var index = 0;

            while (true)
            {
                if (allBoundIpAddresses.Any(ipAddress => ipAddress != null && ipAddress.Equals(ipAddressFragment + index)))
                {
                    index++;
                    continue;
                }

                return ipAddressFragment + index;
            }
        }
    }
}