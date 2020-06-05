using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using App.Boilerplate.Core;
using App.Boilerplate.Core.Models;
using App.Boilerplate.Infrastructure;

using CMS.AspNet.Platform;

using Owin;

namespace App.Boilerplate.Mvc
{
    public class KenticoMvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
        }

        protected void Application_Error()
        {
            ApplicationErrorLogger.LogLastApplicationError();

            if (Context.IsCustomErrorEnabled)
            {
                var error = Server.GetLastError();
                var controllerName = "Error";

                Context.ClearError();

                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                routeData.Values.Add("action", "Index");
                routeData.Values.Add("exception", error);

                var requestContext = new RequestContext(new HttpContextWrapper(Context), routeData);
                var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

                var errorController = controllerFactory.CreateController(requestContext, controllerName);
                errorController.Execute(requestContext);
            }
        }

        protected void Configure(IAppBuilder app, ConfigureOptions configureOptions = null)
        {
            new Startup(configureOptions)
                .Configure(GetType(), app)
                .AddInfrastructure();
        }

        protected static string ControllerName<T>()
        {
            var name = typeof(T).Name;
            return name.Substring(0, name.LastIndexOf("Controller"));
        }
    }
}