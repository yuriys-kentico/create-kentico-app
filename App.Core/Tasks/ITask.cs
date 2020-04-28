using System.Threading.Tasks;

namespace App.Core.Tasks
{
    public interface ITask
    {
        public Task Run();
    }
}