using System;
using System.Collections.Generic;
using System.Linq;

using App.Boilerplate.Core.Models;
using App.Boilerplate.Core.Routing;

using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

namespace App.Boilerplate.Infrastructure.Routing
{
    internal class PageTreeRoutesRepository : IPageTreeRoutesRepository
    {
        private readonly IEnumerable<IPageTreeController> pageTreeControllers;

        public IDictionary<string, Func<NodeRouteData>> RoutesDictionary =>
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
                        nameof(DocumentCultureDataInfo.DocumentID),
                        nameof(DocumentCultureDataInfo.DocumentForeignKeyValue),
                        nameof(DocumentCultureDataInfo.DocumentPageTemplateConfiguration),
                        nameof(DocumentCultureDataInfo.DocumentNamePath)
                        )
                    .LatestVersion()
                    .Select(node => Tuple.Create(GetNodeRoute(node), node))
                    .ToDictionary(
                        nodeData => $"{nodeData.Item2.NodeSiteID}|{nodeData.Item1}".ToLower(),
                        nodeData => GetGetNodeRouteData(nodeData.Item1, nodeData.Item2)
                        );
                },
                new CacheSettings(60 * 24, $"{typeof(PageTreeRoutesRepository).FullName}|{nameof(RoutesDictionary)}")
            );

        public PageTreeRoutesRepository(IEnumerable<IPageTreeController> pageTreeControllers)
        {
            this.pageTreeControllers = pageTreeControllers;
        }

        private static string GetNodeRoute(TreeNode node) => DocumentURLProvider.GetUrl(node).TrimStart('~');

        private Func<NodeRouteData> GetGetNodeRouteData(string nodeRoute, TreeNode node) =>
            () => CacheHelper.Cache(cacheSettings =>
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

                    return new NodeRouteData
                    {
                        Id = node.DocumentID,
                        Type = node.NodeClassName,
                        ControllerName = controllerType?.Name.Replace("Controller", string.Empty),
                        Node = modelType != null && modelType != typeof(TreeNode)
                            ? Activator.CreateInstance(modelType, new[] { node })
                            : node
                    };
                },
                new CacheSettings(60 * 24, $"{typeof(PageTreeRoutesRepository).FullName}|{nameof(RoutesDictionary)}|{node.NodeSiteID}|{nodeRoute}")
            );
    }
}