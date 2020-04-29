using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using App.Core;
using App.Core.Models;
using App.Core.Services;

using Microsoft.Web.Administration;

using static App.Install.Constants;
using static App.Install.InstallHelper;

namespace App.Install
{
    public class IisSiteTask : IIisSiteTask
    {
        private readonly Settings settings;
        private readonly Terms terms;
        private readonly IOutputService output;
        private readonly ICacheService cache;

        private Func<string, string> AppPath => appFolderName => @$"C:\inetpub\wwwroot\{appFolderName}";

        public IisSiteTask(
            Settings settings,
            Terms terms,
            Services services
            )
        {
            this.settings = settings;
            this.terms = terms;
            output = services.OutputService();
            cache = services.CacheService();
        }

        public async Task Run()
        {
            output.Display(terms.IisTaskStart);

            using var iisManager = new ServerManager();

            settings.AppName = settings.AppName ?? throw new ArgumentException($"'{nameof(settings.AppName)}' must be set!");
            settings.AppPath ??= AppPath(settings.AppName);
            settings.AppWebPath ??= $"{GetNextUnboundIpAddress(iisManager)}:443";

            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);

            X509Certificate2 certificate;

            var certificateThumbprint = await cache.GetString(CertificateCacheKey);
            var filteredCertificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);

            if (certificateThumbprint != null && filteredCertificates.Count > 0)
            {
                output.Display(terms.SkippingCreatingCertificate);

                certificate = filteredCertificates[0];
            }
            else
            {
                certificate = GetSelfSignedCertificate("create-kentico-app");

                store.Add(certificate);

                await cache.SetString(CertificateCacheKey, certificate.Thumbprint);

                output.Display(terms.CreatedNewCertificate);
            }

            store.Close();

            var siteName = EnsureValidSiteName(iisManager.Sites, settings.AppName);

            var appSite = iisManager.Sites.Add(
                siteName,
                settings.AppWebPath + ":",
                Path.Combine(settings.AppPath, "CMS"),
                certificate.GetCertHash(),
                store.Name,
                SslFlags.None
                );

            var appPool = iisManager.ApplicationPools.Add(EnsureValidAppPoolName(iisManager.ApplicationPools, siteName));
            appPool.Cpu.Action = ProcessorAction.KillW3wp;
            appPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;

            appSite.ApplicationDefaults.ApplicationPoolName = appPool.Name;

            iisManager.CommitChanges();
        }

        private string GetNextUnboundIpAddress(ServerManager iisManager)
        {
            settings.Version = settings.Version ?? throw new ArgumentException($"'{nameof(settings.Version)}' must be set!");

            var ipAddressFragment = $"127.{settings.Version.Major}.{settings.Version.Build}.";

            var allBoundIpAddresses = iisManager.Sites
                .SelectMany(site => site.Bindings)
                .Select(binding => binding.BindingInformation.Split(':').First());

            var index = 0;

            while (true)
            {
                index++;

                if (allBoundIpAddresses.Any(ipAddress => ipAddress.Equals(ipAddressFragment + index)))
                {
                    continue;
                }

                return ipAddressFragment + index;
            }
        }

        private string EnsureValidSiteName(SiteCollection sites, string siteName)
        {
            if (sites.Any(site => site.Name.Equals(siteName)))
            {
                siteName += $"_{GetRandomString(10)}";
            }

            return siteName;
        }

        private string EnsureValidAppPoolName(ApplicationPoolCollection appPools, string appPoolName)
        {
            if (appPools.Any(appPool => appPool.Name.Equals(appPoolName)))
            {
                appPoolName += $"_{GetRandomString(10)}";
            }

            return appPoolName;
        }

        private X509Certificate2 GetSelfSignedCertificate(string name)
        {
            var distinguishedName = new X500DistinguishedName($"CN={name}");

            using var rsa = RSA.Create(2048);

            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false)
                );

            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)
                );

            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName(Environment.MachineName);

            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

            certificate.FriendlyName = name;

            var password = $"{Environment.MachineName}_{settings.AppName}";

            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags.MachineKeySet);
        }
    }
}