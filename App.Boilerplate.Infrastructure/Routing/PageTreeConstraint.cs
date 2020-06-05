using System.Web;
using System.Web.Routing;

using AngleSharp.Network.Default;

using App.Boilerplate.Core.Context;
using App.Boilerplate.Core.Routing;

namespace App.Boilerplate.Infrastructure.Routing
{
    internal class PageTreeConstraint : IRouteConstraint
    {
        private readonly IPageTreeRoutesRepository pageTreeRoutesRepository;
        private readonly ISiteContext siteContext;

        public PageTreeConstraint(
            IPageTreeRoutesRepository pageTreeRoutesRepository,
            ISiteContext siteContext
            )
        {
            this.pageTreeRoutesRepository = pageTreeRoutesRepository;
            this.siteContext = siteContext;
        }

        public bool Match(
            HttpContextBase httpContext,
            Route route,
            string parameterName,
            RouteValueDictionary values,
            RouteDirection routeDirection
            )
        {
            var path = values["url"] as string ?? httpContext.Request.Path;

            var key = $"{siteContext.SiteId}|/{path.TrimStart('/').ToLower()}";

            if (pageTreeRoutesRepository.RoutesDictionary.TryGetValue(key, out var getNodeRouteData))
            {
                var nodeRouteData = getNodeRouteData();

                values["path"] = path;
                values["node"] = nodeRouteData.Node;

                if (nodeRouteData.ControllerName != null)
                {
                    values["controller"] = nodeRouteData.ControllerName;
                }

                return true;
            }

            return false;
        }
    }
}