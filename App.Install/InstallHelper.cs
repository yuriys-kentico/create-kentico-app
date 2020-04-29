using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using App.Core.Services;

namespace App.Install
{
    internal static class InstallHelper
    {
        private static readonly Random random = new Random();
        private static readonly IDictionary<int, string> randomStringCache = new Dictionary<int, string>();

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

        internal static string GetRandomString(int length)
        {
            if (randomStringCache.TryGetValue(length, out var result))
            {
                return result;
            }

            const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            result = string.Empty;

            for (int i = 0; i < length; i++)
            {
                result += validCharacters[random.Next(0, validCharacters.Length - 1)];
            }

            randomStringCache.Add(length, result);

            return result;
        }
    }
}