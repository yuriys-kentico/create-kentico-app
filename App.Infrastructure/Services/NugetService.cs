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
        private NuGetPackageManager? packageManager;
        private SourceRepositoryProvider? sourceRepositoryProvider;
        private Settings? nugetSettings;
        private EmptyNuGetProjectContext? projectContext;

        public NuGetPackageManager PackageManager
        {
            get => packageManager ??= new NuGetPackageManager(
                SourceRepositoryProvider,
                NugetSettings,
                PackagesFolderPath
                );
        }

        public SourceRepositoryProvider SourceRepositoryProvider
        {
            get => sourceRepositoryProvider ??= new SourceRepositoryProvider(
                new PackageSourceProvider(NugetSettings),
                FactoryExtensionsV3.GetCoreV3(Repository.Provider)
                );
        }

        public Settings NugetSettings
        {
            get => nugetSettings ??= new Settings(settings.Path ?? throw new ArgumentNullException(nameof(settings.Path)));
        }

        public string PackagesFolderPath => Path.Combine(
            settings.Path ?? throw new ArgumentNullException(nameof(settings.Path)),
            "packages"
            );

        public PackagesConfigNuGetProject Project
        {
            get => new PackagesConfigNuGetProject(
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

        public EmptyNuGetProjectContext ProjectContext
        {
            get => projectContext ??= new EmptyNuGetProjectContext
            {
                PackageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv2,
                    PackageExtractionBehavior.XmlDocFileSaveMode,
                    ClientPolicyContext.GetClientPolicy(NugetSettings, NullLogger.Instance),
                    NullLogger.Instance
                    )
            };
        }

        public NugetService(Core.Models.Settings settings)
        {
            this.settings = settings;
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
                ProjectContext,
                SourceRepositoryProvider.GetRepositories(),
                Array.Empty<SourceRepository>(),
                CancellationToken.None
                );
        }
    }
}