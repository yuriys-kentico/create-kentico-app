using System;
using System.Collections.Generic;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Localization;

using Newtonsoft.Json;

namespace App.Boilerplate
{
    public partial class PropertiesEditor : FormEngineUserControl
    {
        public override object Value
        {
            get => propertiesValue.Value;
            set => propertiesValue.Value = ValidationHelper.GetString(value, null);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckFieldEmptiness = false;

            propertiesValue.ClientIDMode = ClientIDMode.Static;
            propertiesValue.Attributes["data-localization"] = GetLocalization();
            propertiesValue.Attributes["data-form-components"] = GetFormComponents();

            Page.Header.Controls.Add(
                new LiteralControl(
                    ScriptHelper.GetScriptTag(
                        HTMLHelper.EncodeForHtmlAttribute("~/CMSModules/App.Boilerplate/FormControls/propertiesEditor.js"),
                        false,
                        ScriptExecutionModeEnum.Deferred
                        )
                    )
                );
        }

        private string GetLocalization()
        {
            var cultureName = LocalizationContext.CurrentUICulture.CultureCode;

            var localizationNamespaces = new LocalizationNamespaces();

            var keys = new[]
            {
                "unigrid.actions",
                "general.name",
                "general.type",
                "app.boilerplate.ui.userwidget.properties.formcomponent",
                "app.boilerplate.ui.userwidget.properties.defaultvalue",
                "general.label",
                "app.boilerplate.ui.userwidget.properties.tooltip",
                "app.boilerplate.ui.userwidget.properties.explanation",
                "app.boilerplate.ui.userwidget.properties.addproperty",
                "general.apply",
                "general.cancel",
                "general.dragmove",
                "general.pleaseselect",
                "general.edit",
                "general.delete",
            };

            foreach (var key in keys)
            {
                localizationNamespaces.Translation.Add(key, ResHelper.GetString(key, cultureName));
            }

            var localization = new Dictionary<string, LocalizationNamespaces> {
                { cultureName, localizationNamespaces }
            };

            return JsonConvert.SerializeObject(localization);
        }

        private string GetFormComponents()
        {
            var formComponents = new Dictionary<string, string>
            {
                { "Kentico.TextInput", typeof(string).FullName },
                { "Kentico.TextArea", typeof(string).FullName },
                { "Kentico.IntInput", typeof(int).FullName },
                { "Kentico.CheckBox", typeof(bool).FullName },
                { "Kentico.EmailInput", typeof(string).FullName },
                { "Kentico.USPhone", typeof(string).FullName },
            };

            return JsonConvert.SerializeObject(formComponents);
        }

        private class LocalizationNamespaces
        {
            [JsonProperty("translation")]
            public IDictionary<string, string> Translation { get; set; } = new Dictionary<string, string>();
        }
    }
}