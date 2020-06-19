using System;
using System.Collections.Generic;

namespace App.Boilerplate.Core.Widgets
{
    public interface IUserWidgetsPropertiesGenerator
    {
        Type GetPropertiesType(string identifier, IList<UserWidgetProperty> properties);
    }
}