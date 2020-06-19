using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace App.Boilerplate.Core.Routing
{
    public interface IPageTreeRoutesRepository
    {
        IDictionary<string, Action<RouteValueDictionary>> RoutesDictionary { get; }
    }
}