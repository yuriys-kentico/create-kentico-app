using System;
using System.IO;
using System.Web.Mvc;

using App.Boilerplate.Core.Models;
using App.Boilerplate.Core.Routing;

using Kentico.Activities.Web.Mvc;
using Kentico.CampaignLogging.Web.Mvc;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Newsletters.Web.Mvc;
using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

using Owin;

namespace App.Boilerplate.Core
{
    public class Startup
    {
        public ConfigureOptions ConfigureOptions { get; private set; }

        public ServiceCollection Services { get; private set; }

        public Startup(ConfigureOptions configureOptions)
        {
            ConfigureOptions = configureOptions ?? new ConfigureOptions();
            Services = new ServiceCollection();
        }

        public Startup Configure(Type appType, IAppBuilder app)
        {
            ConfigureOptions.WwwRoot = ConfigureOptions.WwwRoot ?? "wwwroot";

            ConfigureStaticFiles(app);
            ConfigureServices(appType);
            ConfigureFeatures();

            return this;
        }

        private void ConfigureStaticFiles(IAppBuilder app)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string wwwRoot = Path.Combine(root, ConfigureOptions.WwwRoot);

            var fileServerOptions = new FileServerOptions
            {
                EnableDirectoryBrowsing = false,
                RequestPath = new PathString(string.Empty),
                FileSystem = new PhysicalFileSystem(wwwRoot)
            };

            fileServerOptions.StaticFileOptions.ServeUnknownFileTypes = true;

            app.UseFileServer(fileServerOptions).UseStageMarker(PipelineStage.MapHandler);
        }

        private void ConfigureServices(Type appType)
        {
            Services.Scan(source => source
                .FromAssembliesOf(appType)
                    .AddClasses(classes => classes.AssignableTo<IController>())
                        .AsSelf()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<IPageTreeController>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                );

            if (ConfigureOptions?.ConfigureServices != null)
            {
                Services.Scan(ConfigureOptions.ConfigureServices);
            }
        }

        private static void ConfigureFeatures()
        {
            var builder = ApplicationBuilder.Current;

            builder.UsePreview();
            builder.UsePageBuilder();
            builder.UseDataAnnotationsLocalization();
            builder.UsePageRouting(new PageRoutingOptions
            {
                EnableAlternativeUrls = true
            });
            builder.UseABTesting();
            builder.UseActivityTracking();
            builder.UseCampaignLogger();
            builder.UseEmailTracking(new EmailTrackingOptions());
        }
    }
}