using System;
using System.Collections.Generic;

using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Shared.Widgets;

using CMS;
using CMS.Core;

[assembly: RegisterImplementation(typeof(IUserWidgetsPropertiesGenerator), typeof(DummyUserWidgetsPropertiesGenerator), Priority = RegistrationPriority.Fallback)]

namespace App.Boilerplate.Shared.Widgets
{
    internal class DummyUserWidgetsPropertiesGenerator : IUserWidgetsPropertiesGenerator
    {
        public void ReloadDynamicAssembly()
        {
            throw new NotImplementedException("This interface must be depended on in an MVC application.");
        }

        public Type GetPropertiesType(string identifier, IList<UserWidgetProperty> properties)
        {
            throw new NotImplementedException("This interface must be depended on in an MVC application.");
        }
    }
}