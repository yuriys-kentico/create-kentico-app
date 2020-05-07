using App.Core.Tasks;

namespace App.Core
{
    public interface IDatabaseTask : ITask
    {
        bool Mvc { get; set; }
    }
}