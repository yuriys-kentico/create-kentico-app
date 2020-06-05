using System;
using System.Linq;

using App.Boilerplate.Admin.Routing;

using CMS;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

[assembly: RegisterExtension(typeof(RoutingMacroMethods), typeof(string))]

namespace App.Boilerplate.Admin.Routing
{
    public class RoutingMacroMethods : MacroMethodContainer
    {
        [MacroMethod(Type = typeof(string))]
        [MacroMethodParam(0, "source", typeof(string), "")]
        public static object Route(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    if (parameters[0] is string source)
                    {
                        return "/" + string.Join(
                            "/",
                            source
                                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(part => URLHelper.GetSafeUrlPart(part, SiteContext.CurrentSiteName))
                            );
                    }
                    break;
            }

            throw new NotSupportedException();
        }
    }
}