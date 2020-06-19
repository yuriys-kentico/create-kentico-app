using System.Linq;
using System.Web.Compilation;
using System.Web.Mvc;

using App.Boilerplate.Core.Routing;

using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;

using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.Web.Mvc;

namespace App.Boilerplate.Infrastructure.Routing
{
    public sealed class BasicController : Controller
    {
        private readonly IPageTreeModelService pageTreeModelService;

        public BasicController(IPageTreeModelService pageTreeModelService)
        {
            this.pageTreeModelService = pageTreeModelService;
        }

        [HttpGet]
        public ActionResult Index(Page page, string path)
        {
            var searchLocations = new[]
            {
                $"~/Views{page.NodeAliasPath}/Index.cshtml",
                $"~/Views/{page.NodeClassName}/Index.cshtml"
            };

            ViewEngineResult pageTreeViewResult = null;

            foreach (var searchLocation in searchLocations)
            {
                if (pageTreeViewResult?.View == null)
                {
                    pageTreeViewResult = ViewEngines.Engines.FindView(ControllerContext, searchLocation, null);
                }
            }

            if (pageTreeViewResult.View == null)
            {
                if (page.GetValue(nameof(DocumentCultureDataInfo.DocumentPageTemplateConfiguration)) != null)
                {
                    return new TemplateResult(page.DocumentID);
                }

                throw new PageTreeRoutingException(
                    $"View for path '{path}' not found. Searched locations: {string.Join(", ", searchLocations.Select(searchLocation => $"'{searchLocation}'"))}."
                    );
            }

            var view = pageTreeViewResult.View as RazorView ?? TryGetFromGlimpseWrapper(pageTreeViewResult);

            if (view == null)
            {
                throw new PageTreeRoutingException($"View at '{pageTreeViewResult.SearchedLocations.FirstOrDefault()}' is not a '{nameof(RazorView)}'.");
            }

            HttpContext.Kentico().PageBuilder().Initialize(page.DocumentID);

            var viewModelType = BuildManager.GetCompiledType(view.ViewPath).BaseType.GetGenericArguments()[0];

            if (viewModelType == null)
            {
                return View(view);
            }

            return View(view, viewModelType == typeof(TreeNode)
                        ? page
                        : pageTreeModelService.GetModel(viewModelType, RouteData.Values));
        }

        private static RazorView TryGetFromGlimpseWrapper(ViewEngineResult pageTreeViewResult)
        {
            try
            {
                return ((dynamic)pageTreeViewResult.View).GetWrappedObject();
            }
            catch
            {
            }

            return null;
        }
    }
}