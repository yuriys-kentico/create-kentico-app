using System;
using System.Web.UI;

using CMS.FormEngine.Web.UI;
using CMS.Helpers;

namespace App.Boilerplate
{
    public partial class IconSelector : FormEngineUserControl, IPostBackEventHandler
    {
        public override object Value
        {
            get => HTMLHelper.EncodeForHtmlAttribute(fontIconSelector.Value);
            set => fontIconSelector.Value = ValidationHelper.GetString(value, null);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (StopProcessing)
            {
                pnlIcons.Visible = false;
                return;
            }

            CheckFieldEmptiness = false;
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            if (eventArgument == "update")
                ShowChangesSaved();
        }
    }
}