using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using App.Boilerplate.Core;
using App.Boilerplate.Core.Context;
using App.Boilerplate.Core.Routing;
using App.Boilerplate.Infrastructure.Bundles;
using App.Boilerplate.Infrastructure.Routing;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;

using Kentico.Web.Mvc;

using Microsoft.Extensions.DependencyInjection;

namespace App.Boilerplate.Infrastructure
{
    internal static class StartupExtensions
    {
        internal static Startup AddInfrastructure(this Startup startup)
        {
            ConfigureServices(startup);
            ConfigureBundles(startup);
            ConfigureRoutes(startup);

            return startup;
        }

        private static void ConfigureServices(Startup startup)
        {
            startup.Services.Scan(source => source
                .FromCallingAssembly()
                    .AddClasses(classes => classes.AssignableTo<IController>())
                        .AsSelf()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<IPageTreeRoutesRepository>(), false)
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                    .AddClasses(classes => classes.AssignableTo<ISiteContext>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                );

            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(startup.Services);
            containerBuilder.RegisterSource(new ServicesRegistrations());

            DependencyResolver.SetResolver(new AutofacDependencyResolver(containerBuilder.Build()));
        }

        private static void ConfigureBundles(Startup startup)
        {
            var styles = new StyleBundle("~/css")
                .IncludeDirectory($"~/{startup.ConfigureOptions.WwwRoot}/css", "*.css", true);

            startup.ConfigureOptions.ConfigureStyles?.Invoke(styles);

            styles.Transforms.Add(new WwwRootTransform(startup.ConfigureOptions.WwwRoot));

            BundleTable.Bundles.Add(styles);

            var scripts = new StyleBundle("~/js")
                .IncludeDirectory($"~/{startup.ConfigureOptions.WwwRoot}/js", "*.js", true);

            startup.ConfigureOptions.ConfigureScripts?.Invoke(scripts);

            scripts.Transforms.Add(new WwwRootTransform(startup.ConfigureOptions.WwwRoot));

            BundleTable.Bundles.Add(scripts);
        }

        private static void ConfigureRoutes(Startup startup)
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

            startup.ConfigureOptions.ConfigureRoutes?.Invoke(routes);
        }
    }
}