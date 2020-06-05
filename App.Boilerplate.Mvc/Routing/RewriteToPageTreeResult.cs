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
            this.path = path;
            pageTreeRoutesRepository = DependencyResolver.Current.GetService<IPageTreeRoutesRepository>();
            siteContext = DependencyResolver.Current.GetService<ISiteContext>();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Basic");
            routeData.Values.Add("action", "Index");

            var key = $"{siteContext.SiteId}|/{path.TrimStart('/').ToLower()}";

            if (pageTreeRoutesRepository.RoutesDictionary.TryGetValue(key, out var getNodeRouteData))
            {
                var nodeRouteData = getNodeRouteData();

                routeData.Values["path"] = path;
                routeData.Values["node"] = nodeRouteData.Node;

                if (nodeRouteData.ControllerName != null)
                {
                    routeData.Values["controller"] = nodeRouteData.ControllerName;
                }
            }

            var requestContext = new RequestContext(context.HttpContext, routeData);
            var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

            var errorController = controllerFactory.CreateController(requestContext, routeData.Values["controller"] as string);
            errorController.Execute(requestContext);
        }
    }
}