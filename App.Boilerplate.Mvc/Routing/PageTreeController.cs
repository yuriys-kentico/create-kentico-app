using System.Web.Mvc;

using App.Boilerplate.Core.Routing;

using DotNet.Globbing;

namespace App.Boilerplate.Mvc.Routing
{
    public abstract class PageTreeController<T> : Controller, IPageTreeController
    {
        public string PageType { get; }

        public Glob PageTreePattern { get; }

        protected PageTreeController(string pageType = null, string pageTreePattern = null)
        {
            PageType = pageType;

            if (!string.IsNullOrWhiteSpace(pageTreePattern))
            {
                PageTreePattern = Glob.Parse(pageTreePattern);
            }
        }
    }
}