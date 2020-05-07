using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using App.Core.Services;

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace App.Infrastructure
{
    public class NugetService : INugetService
    {
        private readonly SourceRepositoryProvider sourceRepositoryProvider;
        private readonly NuGetPackageManager packageManager;
        private readonly PackagesConfigNuGetProject project;
        private readonly EmptyNuGetProjectContext projectContext;

        private readonly ResolutionContext resolutionContext = new ResolutionContext(
                DependencyBehavior.Lowest,
                includePrelease: true,
                includeUnlisted: false,
                VersionConstraints.None);

        public NugetService(Core.Models.Settings settings)
        {
            settings.Path = settings.Path ?? throw new ArgumentNullException(nameof(settings.Path));
            settings.Name = settings.Name ?? throw new ArgumentNullException(nameof(settings.Name));

            var nugetSettings = new Settings(settings.Path);
            var packagesFolderPath = Path.Combine(settings.Path, "packages");

            sourceRepositoryProvider = new SourceRepositoryProvider(
                    new PackageSourceProvider(nugetSettings),
                    FactoryExtensionsV3.GetCoreV3(Repository.Provider)
                    );

            packageManager = new NuGetPackageManager(
                sourceRepositoryProvider,
                nugetSettings,
                packagesFolderPath);

            project = new PackagesConfigNuGetProject(
                Path.Combine(settings.Path, settings.Name),
                new Dictionary<string, object> {
                    { "Name", packagesFolderPath },
                    { "TargetFramework", NuGetFramework.AnyFramework }
                }
                );

            projectContext = new EmptyNuGetProjectContext
            {
                PackageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv2,
                    PackageExtractionBehavior.XmlDocFileSaveMode,
                    ClientPolicyContext.GetClientPolicy(nugetSettings, NullLogger.Instance),
                    NullLogger.Instance
                    )
            };
        }

        public async Task InstallPackage(string id, string version)
        {
            await packageManager.InstallPackageAsync(
                project,
                new PackageIdentity(id, new NuGetVersion(version)),
                resolutionContext,
                projectContext,
                sourceRepositoryProvider.GetRepositories(),
                Array.Empty<SourceRepository>(),
                CancellationToken.None
                );
        }
    }
}