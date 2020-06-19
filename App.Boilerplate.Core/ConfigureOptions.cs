using System;
using System.Web.Optimization;
using System.Web.Routing;

using Scrutor;

namespace App.Boilerplate.Core
{
    public class ConfigureOptions
    {
        public Action<ITypeSourceSelector> ConfigureServices { get; set; }

        public string WwwRoot { get; set; }

        public Action<Bundle> ConfigureStyles { get; set; }

        public Action<Bundle> ConfigureScripts { get; set; }

        public Action<RouteCollection> ConfigureRoutes { get; set; }

        public (string, string) Http404ControllerAction { get; set; }
    }
}