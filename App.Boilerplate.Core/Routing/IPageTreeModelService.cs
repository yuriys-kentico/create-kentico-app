using System;
using System.Collections.Generic;

namespace App.Boilerplate.Core.Routing
{
    public interface IPageTreeModelService
    {
        object GetModel(Type modelType, IDictionary<string, object> parameters);
    }
}