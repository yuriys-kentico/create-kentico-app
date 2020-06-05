using System.Web.Optimization;

namespace App.Boilerplate.Infrastructure.Bundles
{
    internal class WwwRootTransform : IBundleTransform
    {
        private readonly string wwwRoot;

        public WwwRootTransform(string wwwRoot)
        {
            this.wwwRoot = wwwRoot;
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            foreach (var file in response.Files)
            {
                file.IncludedVirtualPath = file.IncludedVirtualPath.Replace(wwwRoot, "");
            }
        }
    }
}