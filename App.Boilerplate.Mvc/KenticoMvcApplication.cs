using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using App.Boilerplate.Core;
using App.Boilerplate.Infrastructure;

using CMS.AspNet.Platform;

using Owin;

namespace App.Boilerplate.Mvc
{
    public abstract class KenticoMvcApplication : HttpApplication
    {
        protected abstract (string, string) ErrorControllerAction { get; }

        protected void Application_Start()
        {
        }

        protected void Application_Error()
        {
            ApplicationErrorLogger.LogLastApplicationError();

            if (Context.IsCustomErrorEnabled)
            {
                var error = Server.GetLastError();
                Context.ClearError();

                var (controllerName, action) = ErrorControllerAction;
                var routeData = Context.Request.RequestContext.RouteData;

                routeData.Values["controller"] = controllerName;
                routeData.Values["action"] = action;
                routeData.Values["exception"] = error;

                var requestContext = new RequestContext(new HttpContextWrapper(Context), routeData);
                var controllerFactory = ControllerBuilder.Current.GetControllerFactory();

                var errorController = controllerFactory.CreateController(requestContext, controllerName);
                errorController.Execute(requestContext);
            }
        }

        protected void Configure(IAppBuilder app, ConfigureOptions configureOptions = null)
        {
            new Startup(configureOptions)
                .Configure(GetType(), app);
        }

        protected static string ControllerName<T>()
        {
            var name = typeof(T).Name;
            return name.Substring(0, name.LastIndexOf("Controller"));
        }
    }
}