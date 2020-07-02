using System;
using System.Collections.Generic;

namespace App.Boilerplate.Core.Widgets
{
    public interface IUserWidgetsPropertiesGenerator
    {
        void ReloadDynamicAssembly();

        Type GetPropertiesType(string identifier, IList<UserWidgetProperty> properties);
    }
}