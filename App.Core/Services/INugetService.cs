using System.Threading.Tasks;

namespace App.Core.Services
{
    public interface INugetService
    {
        Task InstallPackage(string id, string version);
    }
}