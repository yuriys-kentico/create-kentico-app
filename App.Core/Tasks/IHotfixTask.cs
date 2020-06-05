using App.Core.Tasks;

namespace App.Core
{
    public interface IHotfixTask : ITask
    {
        bool Mvc { get; set; }
    }
}