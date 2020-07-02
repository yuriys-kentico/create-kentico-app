using App.Boilerplate.Admin;

using CMS;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.UIControls;

[assembly: RegisterModule(typeof(AdminModule))]

namespace App.Boilerplate.Admin
{
    internal class AdminModule : Module
    {
        public AdminModule() : base(nameof(AdminModule))
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            UniGridTransformations.Global.RegisterTransformation(
                "#icon",
                (object parameter) => UIHelper.GetAccessibleImageMarkup(null, ValidationHelper.GetString(parameter, ""), null)
            );
        }
    }
}