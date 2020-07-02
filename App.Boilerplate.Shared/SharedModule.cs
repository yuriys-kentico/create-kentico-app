using App.Boilerplate.Core.Widgets;
using App.Boilerplate.Shared;
using App.Boilerplate.Shared.Widgets;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterModule(typeof(SharedModule))]

namespace App.Boilerplate.Shared
{
    internal class SharedModule : Module
    {
        public SharedModule() : base(nameof(SharedModule))
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            WebFarmHelper.RegisterTask<UserWidgetWebFarmTask>(true);

            UserWidgetInfo.TYPEINFO.Events.Insert.Before += (_, eventArgs) => UpdateUserWidget(nameof(UserWidgetInfo.TYPEINFO.Events.Insert), eventArgs);
            UserWidgetInfo.TYPEINFO.Events.Update.Before += (_, eventArgs) => UpdateUserWidget(nameof(UserWidgetInfo.TYPEINFO.Events.Update), eventArgs);
            UserWidgetInfo.TYPEINFO.Events.Delete.Before += (_, eventArgs) => UpdateUserWidget(nameof(UserWidgetInfo.TYPEINFO.Events.Delete), eventArgs);
        }

        private void UpdateUserWidget(string type, ObjectEventArgs eventArgs)
        {
            if (eventArgs.Object is UserWidgetInfo userWidgetInfo)
            {
                WebFarmHelper.CreateTask(new UserWidgetWebFarmTask
                {
                    Type = type,
                    CodeName = userWidgetInfo.UserWidgetCodeName
                });
            }
        }
    }
}