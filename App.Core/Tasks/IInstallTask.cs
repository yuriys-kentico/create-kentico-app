using App.Core.Tasks;

namespace App.Core
{
    public interface IInstallTask : ITask
    {
        bool Source { get; set; }

        bool Template { get; set; }

        bool Mvc { get; set; }
    }
}