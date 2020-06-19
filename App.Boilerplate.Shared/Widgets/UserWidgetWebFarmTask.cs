using App.Boilerplate.Core.Widgets;

using CMS.Core;

namespace App.Boilerplate.Shared.Widgets
{
    public class UserWidgetWebFarmTask : WebFarmTaskBase
    {
        private readonly IUserWidgetsService userWidgetsService;

        public string Type { get; set; }

        public string CodeName { get; set; }

        public UserWidgetWebFarmTask()
        {
            userWidgetsService = Service.Resolve<IUserWidgetsService>();
        }

        public override void ExecuteTask()
        {
            switch (Type)
            {
                case nameof(UserWidgetInfo.TYPEINFO.Events.Insert):
                    userWidgetsService.Add(CodeName);
                    break;
                case nameof(UserWidgetInfo.TYPEINFO.Events.Delete):
                    userWidgetsService.Remove(CodeName);
                    break;
            }
        }
    }
}