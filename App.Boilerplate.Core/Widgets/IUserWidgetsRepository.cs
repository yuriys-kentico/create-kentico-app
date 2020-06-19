using System.Collections.Generic;

namespace App.Boilerplate.Core.Widgets
{
    public interface IUserWidgetsRepository
    {
        IDictionary<string, UserWidget> UserWidgets { get; }
    }
}