using System;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using App.Boilerplate.Core;
using App.Boilerplate.Core.Context;
using App.Boilerplate.Core.Routing;
using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Infrastructure.Bundles;
using App.Boilerplate.Infrastructure.Routing;
using App.Boilerplate.Infrastructure.Widgets;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;

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

namespace App.Boilerplate.Infrastructure
{
    internal class Startup
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
            ConfigureServices();
            ConfigureBundles();
            ConfigureRoutes();
            ConfigureWidgets();

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

        private void ConfigureServices()
        {
            Services.Scan(source => source
                .FromCallingAssembly()
                    .AddClasses(classes => classes.AssignableTo<IController>())
                        .AsSelf()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<IPageTreeRoutesRepository>(), false)
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                    .AddClasses(classes => classes.AssignableTo<IPageTreeModelService>(), false)
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                    .AddClasses(classes => classes.AssignableTo<ISiteContext>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                );

            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(Services);
            containerBuilder.RegisterSource(new ServicesRegistrations());

            DependencyResolver.SetResolver(new AutofacDependencyResolver(containerBuilder.Build()));
        }

        private void ConfigureBundles()
        {
            var styles = new StyleBundle("~/css")
                .IncludeDirectory($"~/{ConfigureOptions.WwwRoot}/css", "*.css", true);

            ConfigureOptions.ConfigureStyles?.Invoke(styles);

            styles.Transforms.Add(new WwwRootTransform(ConfigureOptions.WwwRoot));

            BundleTable.Bundles.Add(styles);

            var scripts = new StyleBundle("~/js")
                .IncludeDirectory($"~/{ConfigureOptions.WwwRoot}/js", "*.js", true);

            ConfigureOptions.ConfigureScripts?.Invoke(scripts);

            scripts.Transforms.Add(new WwwRootTransform(ConfigureOptions.WwwRoot));

            BundleTable.Bundles.Add(scripts);
        }

        private void ConfigureRoutes()
        {
            var routes = RouteTable.Routes;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.Kentico().MapRoutes();

            routes.MapRoute(
                "PageTreeRoutes",
                "{*url}",
                new { controller = "Basic", action = "Index" },
                new
                {
                    pageTree = new PageTreeConstraint(
                        DependencyResolver.Current.GetService<IPageTreeRoutesRepository>(),
                        DependencyResolver.Current.GetService<ISiteContext>()
                    )
                });

            ConfigureOptions.ConfigureRoutes?.Invoke(routes);

            var (controller, action) = ConfigureOptions.Http404ControllerAction;

            routes.MapRoute(
                name: "404",
                url: "{*url}",
                defaults: new { controller, action }
            );
        }

        private void ConfigureWidgets()
        {
            HostingEnvironment.RegisterVirtualPathProvider(new UserWidgetsPathProvider(
                DependencyResolver.Current.GetService<IUserWidgetsRepository>()
            ));

            DependencyResolver.Current.GetService<IUserWidgetsService>().RegisterAll();
        }
    }
}