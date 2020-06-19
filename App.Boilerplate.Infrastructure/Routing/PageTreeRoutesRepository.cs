using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

using App.Boilerplate.Core.Routing;

using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace App.Boilerplate.Infrastructure.Routing
{
    internal class PageTreeRoutesRepository : IPageTreeRoutesRepository
    {
        private readonly IEnumerable<IPageTreeController> pageTreeControllers;
        private readonly IPageTreeModelService pageTreeModelService;

        public IDictionary<string, Action<RouteValueDictionary>> RoutesDictionary =>
            CacheHelper.Cache(cacheSettings =>
                {
                    if (cacheSettings.Cached)
                    {
                        cacheSettings.CacheDependency = CacheHelper.GetCacheDependency("cms.node|all");
                    }

                    return DocumentHelper.GetDocuments()
                        .Columns(
                            nameof(TreeNode.NodeIsContentOnly),
                            nameof(DocumentNodeDataInfo.NodeID),
                            nameof(DocumentNodeDataInfo.NodeSiteID),
                            nameof(DocumentNodeDataInfo.NodeClassID),
                            nameof(DocumentNodeDataInfo.NodeAliasPath),
                            nameof(DocumentCultureDataInfo.DocumentID),
                            nameof(DocumentCultureDataInfo.DocumentCulture),
                            nameof(DocumentCultureDataInfo.DocumentForeignKeyValue),
                            nameof(DocumentCultureDataInfo.DocumentPageTemplateConfiguration),
                            nameof(DocumentCultureDataInfo.DocumentNamePath)
                            )
                        .LatestVersion()
                        .WhereTrue(nameof(TreeNode.NodeIsContentOnly))
                        .Select(node => Tuple.Create(GetNodeRoute(node), node))
                        .ToDictionary(
                            nodeData => $"{nodeData.Item2.NodeSiteID}|{nodeData.Item1}".ToLower(),
                            nodeData => GetSetNodeRouteData(nodeData.Item1, nodeData.Item2)
                            );
                },
                new CacheSettings(60 * 24, $"{typeof(PageTreeRoutesRepository).FullName}|{nameof(RoutesDictionary)}")
            );

        public PageTreeRoutesRepository(
            IEnumerable<IPageTreeController> pageTreeControllers,
            IPageTreeModelService pageTreeModelService
            )
        {
            this.pageTreeControllers = pageTreeControllers;
            this.pageTreeModelService = pageTreeModelService;
        }

        private static string GetNodeRoute(TreeNode node) => DocumentURLProvider.GetUrl(node).TrimStart('~');

        private Action<RouteValueDictionary> GetSetNodeRouteData(string nodeRoute, TreeNode node) =>
            CacheHelper.Cache<Action<RouteValueDictionary>>(cacheSettings =>
                {
                    node.MakeComplete(true);

                    var controllerType = pageTreeControllers
                        .FirstOrDefault(pageTreeController =>
                            pageTreeController.PageType == node.NodeClassName
                            || (pageTreeController.PageTreePattern?.IsMatch(nodeRoute) ?? false))?
                        .GetType();

                    var modelType = controllerType?.BaseType.GetGenericArguments()[0];

                    if (cacheSettings.Cached)
                    {
                        cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"cms.node|{node.NodeID}");
                    }

                    return (RouteValueDictionary values) =>
                    {
                        if (controllerType != null)
                        {
                            values["controller"] = controllerType.Name.Replace("Controller", string.Empty);
                        }

                        values["page"] = new Page(node);

                        if (modelType != null && modelType != typeof(Page))
                        {
                            values["page"] = pageTreeModelService.GetModel(modelType, values);
                        }
                    };
                },
                new CacheSettings(60 * 24, $"{typeof(PageTreeRoutesRepository).FullName}|{nameof(RoutesDictionary)}|{node.NodeSiteID}|{nodeRoute}")
            );
    }
}