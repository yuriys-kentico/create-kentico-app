using System;
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
        [HttpGet]
        public ActionResult Index(TreeNode node, string path)
        {
            if (node.GetValue(nameof(DocumentCultureDataInfo.DocumentPageTemplateConfiguration)) != null)
            {
                return new TemplateResult(node.DocumentID);
            }

            var searchLocations = new[]
            {
                $"~/Views{path}/Index.cshtml",
                $"~/Views/{node.NodeClassName}/Index.cshtml"
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
                throw new PageTreeRoutingException(
                    $"View for path '{path}' not found. Searched locations: {string.Join(", ", searchLocations.Select(searchLocation => $"'{searchLocation}'"))}."
                    );
            }

            if (pageTreeViewResult.View is RazorView view)
            {
                HttpContext.Kentico().PageBuilder().Initialize(node.DocumentID);

                var viewModelType = BuildManager.GetCompiledType(view.ViewPath).BaseType.GetGenericArguments()[0];

                return View(view, viewModelType != null && viewModelType != typeof(TreeNode)
                            ? Activator.CreateInstance(viewModelType, new[] { node })
                            : node);
            }

            throw new PageTreeRoutingException($"View at '{pageTreeViewResult.SearchedLocations.FirstOrDefault()}' is not a '{nameof(RazorView)}'.");
        }
    }
}