using System;
using System.Linq;

using App.Boilerplate.Shared.Routing;

using CMS;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(RoutingMacroMethods), typeof(string))]

namespace App.Boilerplate.Shared.Routing
{
    public class RoutingMacroMethods : MacroMethodContainer
    {
        [MacroMethod]
        public static object Route(EvaluationContext context, params object[] parameters)
        {
            var source = parameters
                .Select(parameter => parameter.ToString().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                .SelectMany(parameter => parameter)
                .Select(part => URLHelper.GetSafeUrlPart(part, SiteContext.CurrentSiteName));

            return "/" + string.Join("/", source);
        }
    }
}