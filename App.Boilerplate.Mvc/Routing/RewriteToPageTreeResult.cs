using System.Web.Mvc;
using System.Web.Routing;

using App.Boilerplate.Core.Context;
using App.Boilerplate.Core.Routing;

namespace App.Boilerplate.Mvc
{
    public class RewriteToPageTreeResult : ActionResult
    {
        private readonly string path;
        private readonly IPageTreeRoutesRepository pageTreeRoutesRepository;
        private readonly ISiteContext siteContext;

        public RewriteToPageTreeResult(string path)
        {
            this.path = $"/{path.TrimStart('/')}".ToLower();
            pageTreeRoutesRepository = DependencyResolver.Current.GetService<IPageTreeRoutesRepository>();
            siteContext = DependencyResolver.Current.GetService<ISiteContext>();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var routeData = context.RouteData;

            routeData.Values["controller"] = "Basic";
            routeData.Values["action"] = "Index";

            var key = $"{siteContext.SiteId}|{path}";

            if (pageTreeRoutesRepository.RoutesDictionary.TryGetValue(key, out var setNodeRouteData))
            {
                routeData.Values["path"] = path;

                setNodeRouteData(routeData.Values);
            }

            var requestContext = new RequestContext(context.HttpContext, routeData);
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

            var controller = controllerFactory.CreateController(requestContext, routeData.Values["controller"] as string);
            controller.Execute(requestContext);
        }
    }
}