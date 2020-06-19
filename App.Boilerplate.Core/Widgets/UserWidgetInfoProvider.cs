using CMS.DataEngine;

namespace App.Boilerplate.Core.Widgets
{
    public class UserWidgetInfoProvider : AbstractInfoProvider<UserWidgetInfo, UserWidgetInfoProvider>
    {
        public UserWidgetInfoProvider() : base(UserWidgetInfo.TYPEINFO)
        {
        }

        public static ObjectQuery<UserWidgetInfo> GetUserWidgets() => ProviderObject.GetObjectQuery();

        public static UserWidgetInfo GetUserWidgetInfo(int id) => ProviderObject.GetInfoById(id);

        public static UserWidgetInfo GetUserWidgetInfo(string name) => ProviderObject.GetInfoByCodeName(name);

        public static void SetUserWidgetInfo(UserWidgetInfo infoObj) => ProviderObject.SetInfo(infoObj);

        public static void DeleteUserWidgetInfo(UserWidgetInfo infoObj) => ProviderObject.DeleteInfo(infoObj);
    }
}