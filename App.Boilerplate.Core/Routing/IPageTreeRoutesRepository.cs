using System;
using System.Collections.Generic;

using App.Boilerplate.Core.Models;

namespace App.Boilerplate.Core.Routing
{
    public interface IPageTreeRoutesRepository
    {
        IDictionary<string, Func<NodeRouteData>> RoutesDictionary { get; }
    }
}