using System;

using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Shared.Widgets;

using CMS;
using CMS.Core;

[assembly: RegisterImplementation(typeof(IUserWidgetsService), typeof(DummyUserWidgetsService), Priority = RegistrationPriority.Fallback)]

namespace App.Boilerplate.Shared.Widgets
{
    internal class DummyUserWidgetsService : IUserWidgetsService
    {
        public void RegisterAll()
        {
            throw new NotImplementedException("This interface must be depended on in an MVC application.");
        }

        public void Add(string identifier)
        {
            throw new NotImplementedException("This interface must be depended on in an MVC application.");
        }

        public void Remove(string identifier)
        {
            throw new NotImplementedException("This interface must be depended on in an MVC application.");
        }
    }
}