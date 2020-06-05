using App.Boilerplate.Core.Context;

namespace App.Boilerplate.Infrastructure.Context
{
    internal class SiteContext : ISiteContext
    {
        public int SiteId
        {
            get
            {
                return CMS.SiteProvider.SiteContext.CurrentSiteID;
            }
        }
    }
}