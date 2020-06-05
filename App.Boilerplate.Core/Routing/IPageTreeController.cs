using System.Web.Mvc;

using DotNet.Globbing;

namespace App.Boilerplate.Core.Routing
{
    public interface IPageTreeController : IController
    {
        string PageType { get; }

        Glob PageTreePattern { get; }
    }
}