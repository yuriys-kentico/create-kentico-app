using System.IO;
using System.Text;
using System.Web.Hosting;

using App.Boilerplate.Core.Widgets;

namespace App.Boilerplate.Infrastructure.Widgets
{
    internal class UserWidgetView : VirtualFile
    {
        private readonly string view;

        public UserWidgetView(string path, UserWidget userWidget) : base(path)
        {
            view = userWidget.View;
        }

        public override Stream Open() => new MemoryStream(Encoding.UTF8.GetBytes(view));
    }
}