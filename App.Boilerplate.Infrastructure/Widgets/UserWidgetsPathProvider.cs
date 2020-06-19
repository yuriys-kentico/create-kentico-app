using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;

using App.Boilerplate.Core.Widgets;

namespace App.Boilerplate.Infrastructure.Widgets
{
    internal class UserWidgetsPathProvider : VirtualPathProvider
    {
        private readonly IUserWidgetsRepository userWidgetRepository;

        private readonly Regex VirtualPathRegex = new Regex(@"\/Views\/Shared\/Widgets\/_(.*)\.cshtml");

        public UserWidgetsPathProvider(IUserWidgetsRepository userWidgetRepository)
        {
            this.userWidgetRepository = userWidgetRepository;
        }

        public override bool FileExists(string virtualPath)
        {
            return userWidgetRepository.UserWidgets.Keys.Select(key => new[] {
                    $"~/Views/Shared/Widgets/_{key}.cshtml",
                    $"/Views/Shared/Widgets/_{key}.cshtml"
                })
                .SelectMany(virtualPaths => virtualPaths)
                .Contains(virtualPath) || base.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var identifier = VirtualPathRegex.Match(virtualPath).Groups[1];

            if (userWidgetRepository.UserWidgets.TryGetValue(identifier.Value, out var widget))
            {
                return new UserWidgetView(virtualPath, widget);
            }

            return base.GetFile(virtualPath);
        }
    }
}