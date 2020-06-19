using System.Collections.Concurrent;
using System.Collections.Generic;

using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Infrastructure.Widgets;

using CMS;
using CMS.Core;
using CMS.Helpers;

using Kentico.Forms.Web.Mvc;

using Newtonsoft.Json;

[assembly: RegisterImplementation(typeof(IUserWidgetsRepository), typeof(UserWidgetsRepository), Priority = RegistrationPriority.Default)]

namespace App.Boilerplate.Infrastructure.Widgets
{
    internal class UserWidgetsRepository : IUserWidgetsRepository
    {
        private readonly IUserWidgetsPropertiesGenerator userWidgetsPropertiesGenerator;

        public UserWidgetsRepository(IUserWidgetsPropertiesGenerator userWidgetsPropertiesGenerator)
        {
            this.userWidgetsPropertiesGenerator = userWidgetsPropertiesGenerator;
        }

        public IDictionary<string, UserWidget> UserWidgets => CacheHelper.Cache(cacheSettings =>
            {
                IDictionary<string, UserWidget> dictionary = new ConcurrentDictionary<string, UserWidget>();

                var userWidgetInfos = UserWidgetInfoProvider.GetUserWidgets();

                foreach (var userWidgetInfo in userWidgetInfos)
                {
                    dictionary.Add(
                        userWidgetInfo.UserWidgetCodeName,
                        new UserWidget
                        {
                            Name = userWidgetInfo.UserWidgetName,
                            Description = userWidgetInfo.UserWidgetDescription,
                            Icon = userWidgetInfo.UserWidgetIcon,
                            Type = userWidgetsPropertiesGenerator.GetPropertiesType(
                                userWidgetInfo.UserWidgetCodeName + "Properties",
                                JsonConvert.DeserializeObject<IList<UserWidgetProperty>>(userWidgetInfo.UserWidgetProperties)
                            ),
                            View = userWidgetInfo.UserWidgetView
                        }
                    );
                }

                if (cacheSettings.Cached)
                {
                    cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"App.Boilerplate.UserWidget|all");
                }

                return dictionary;
            }, new CacheSettings(60 * 24, $"{typeof(UserWidgetsRepository).FullName}|{nameof(UserWidgets)}"));
    }
}