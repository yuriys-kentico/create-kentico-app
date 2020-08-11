using App.Boilerplate;
using App.Boilerplate.Controllers;
using App.Boilerplate.Core;
using App.Boilerplate.Mvc;

using CMS;

using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

using Microsoft.Owin;

using Owin;

[assembly: AssemblyDiscoverable]
[assembly: OwinStartup(typeof(MvcApplication))]

namespace App.Boilerplate
{
    public class MvcApplication : KenticoMvcApplication
    {
        protected override (string, string) ErrorControllerAction => (ControllerName<ErrorController>(), nameof(ErrorController.Index));

        public void Configuration(IAppBuilder app)
        {
            Configure(app, new ConfigureOptions
            {
                Http404ControllerAction = (ControllerName<ErrorController>(), nameof(ErrorController.Http404))
            });
        }
    }
}