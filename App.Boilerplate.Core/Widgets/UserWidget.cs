using System;
using System.Collections.Generic;

namespace App.Boilerplate.Core.Widgets
{
    public class UserWidget
    {
        public Type Type { get; set; }

        public string View { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public IDictionary<string, UserWidgetProperty> Properties { get; set; }
    }
}