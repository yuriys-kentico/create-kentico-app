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

namespace App.Infrastructure.Services
{
    public class NugetService : INugetService
    {
        private readonly Core.Models.Settings settings;
        private Settings? nugetSettings;
        private SourceRepositoryProvider? sourceRepositoryProvider;
        private NuGetPackageManager? packageManager;
        private PackagesConfigNuGetProject? project;
        private readonly EmptyNuGetProjectContext projectContext;

        public string PackagesFolderPath => Path.Combine(
            settings.Path ?? throw new ArgumentNullException(nameof(settings.Path))
            , "packages"
            );

        public Settings NugetSettings
        {
            get => nugetSettings ??= new Settings(settings.Path);
        }

        public SourceRepositoryProvider SourceRepositoryProvider
        {
            get => sourceRepositoryProvider ??= new SourceRepositoryProvider(
                        new PackageSourceProvider(nugetSettings),
                        FactoryExtensionsV3.GetCoreV3(Repository.Provider)
                        );
        }

        public NuGetPackageManager PackageManager
        {
            get => packageManager ??= new NuGetPackageManager(
                SourceRepositoryProvider,
                NugetSettings,
                PackagesFolderPath);
        }

        public PackagesConfigNuGetProject Project
        {
            get => project ??= new PackagesConfigNuGetProject(
                Path.Combine(
                    settings.Path ?? throw new ArgumentNullException(nameof(settings.Path)),
                    settings.Name ?? throw new ArgumentNullException(nameof(settings.Name))
                    ),
                new Dictionary<string, object> {
                    { "Name", PackagesFolderPath },
                    { "TargetFramework", NuGetFramework.AnyFramework }
                }
                );
        }

        public NugetService(Core.Models.Settings settings)
        {
            this.settings = settings;

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
            await PackageManager.InstallPackageAsync(
                Project,
                new PackageIdentity(id, new NuGetVersion(version)),
                new ResolutionContext(
                    DependencyBehavior.Lowest,
                    includePrelease: true,
                    includeUnlisted: false,
                    VersionConstraints.None
                ),
                projectContext,
                SourceRepositoryProvider.GetRepositories(),
                Array.Empty<SourceRepository>(),
                CancellationToken.None
                );
        }
    }
}