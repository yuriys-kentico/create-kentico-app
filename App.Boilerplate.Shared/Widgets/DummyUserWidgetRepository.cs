using System;
using System.Collections.Generic;

using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Shared.Widgets;

using CMS;
using CMS.Core;

[assembly: RegisterImplementation(typeof(IUserWidgetsRepository), typeof(DummyUserWidgetRepository), Priority = RegistrationPriority.Fallback)]

namespace App.Boilerplate.Shared.Widgets
{
    internal class DummyUserWidgetRepository : IUserWidgetsRepository
    {
        public IDictionary<string, UserWidget> UserWidgets => throw new NotImplementedException("This interface must be depended on in an MVC application.");
    }
}