using System.Web;
using System.Web.Routing;

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

            path = $"/{path.TrimStart('/')}".ToLower();

            var key = $"{siteContext.SiteId}|{path}";

            if (pageTreeRoutesRepository.RoutesDictionary.TryGetValue(key, out var setNodeRouteData))
            {
                values["path"] = path;

                setNodeRouteData(values);

                return true;
            }

            return false;
        }
    }
}